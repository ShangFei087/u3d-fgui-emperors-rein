using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using System.Text;
using System.Collections.Generic;
using GameUtil;
using SBoxApi;
using Newtonsoft.Json;

// 事件类型定义
public class MqttEventHandle
{
    public static string MQTTConnectFailed = nameof(MQTTConnectFailed);
    public static string SendBoardID = nameof(SendBoardID); // 发送控制板ID)

    // 设备主动上报事件
    public const string BonusAutoReport = "BonusAutoReport";                // 自动中奖上报 (BonusReport)
    public const string ErrorAutoReport = "ErrorAutoReport";                // 自动错误上报 (int)

    // 系统事件
    public const string MQTTConnected = "MQTTConnected";                    // 连接成功
    public const string MQTTDisconnected = "MQTTDisconnected";
    public static string DevicedOff = nameof(DevicedOff);
}



public class MqttEventCmd
{
    public const string AddCoins = "AddCoins";
    public const string AddCoinsAck = "AddCoinsAck";
    public const string PlayGame = "PlayGame";
    public const string PlayGameAck = "PlayGameAck";
    public const string GetBonus = "GetBonusBtn";
    public const string GetBonusAck = "GetBonusAck";


    public const string Request = "sub";
    public const string Respond = "pub";

    public const string DeviceOn = "mqtt/state/on";
    public const string DeviceOff = "mqtt/state/off";

}


[System.Serializable]
public class BonusReport
{
    public int type;
    public int state;
    public int score;
}


/*
public enum MqttAppType
{
    /// <summary> App 控制端 </summary>
    IsCtrlApp,
    /// <summary> 机台端 </summary>
    IsGameApp,
}*/

/// <summary>
/// 这个脚本已经弃用
/// </summary>
public class MQTTControllerOld : MonoSingleton<MQTTControllerOld>
{
    [Header("MQTT Settings")]
    [SerializeField] public string brokerAddress = "192.168.3.174";
    [SerializeField] public int brokerPort = 1883;
    [SerializeField] public string selfClientId = "tomappBB";
    [SerializeField] private float reconnectInterval = 5f;
    public string targetClientId = "";

    public MqttAppType CurMqttAppType;

    private MqttClient _client;
    public bool _isConnected;

    [JsonObject]
    public class JackpotData
    {
        [JsonProperty("jp1")]
        public int JP1 { get; set; }

        [JsonProperty("jp2")]
        public int JP2 { get; set; }

        [JsonProperty("jp3")]
        public int JP3 { get; set; }

        [JsonProperty("all")]
        public int JPAll { get; set; }
    }

    [JsonObject]
    public class BonusData
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }

    [JsonObject]
    public class BonusReport
    {
        [JsonProperty("bonus_type")]
        public int BonusType { get; set; }

        [JsonProperty("state")]
        public int State { get; set; }

        [JsonProperty("award")]
        public int Award { get; set; }
    }


    //======================== 连接管理 ========================//
    public void Connect()
    {
        if (_isConnected) return;

        if (CurMqttAppType == MqttAppType.IsGameApp)  // 机台游戏
        {
            selfClientId = SBoxSandbox.DeviceId().ToString();
            if (string.IsNullOrEmpty(selfClientId) || selfClientId == "0")
            {
                selfClientId = "default_device_001";
                targetClientId = selfClientId;
            }
        }
        try
        {
            _client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
            _client.MqttMsgPublishReceived += OnMessageReceived;
            _client.ConnectionClosed += OnDisconnected;

            _client.Connect(selfClientId);
            if (_client.IsConnected)
            {
                DebugUtils.Log("MQTT连接成功");
                _isConnected = true;
                HandleConnectionSuccess();
            }
        }
        catch (Exception e)
        {
            DebugUtils.LogError($"Connection failed: {e.Message}");
            ScheduleReconnect();
        }
    }

    private void HandleConnectionSuccess()
    {
        Loom.QueueOnMainThread((res) => EventCenter.Instance.EventTrigger(MqttEventHandle.MQTTConnected), null);
        //UnityMainThreadDispatcher.Instance.Enqueue(() => EventCenter.Instance.EventTrigger(MqttEventHandle.MQTTConnected));

        if (CurMqttAppType == MqttAppType.IsCtrlApp)
        {
            SubscribeTopics(new[] {
                MqttEventCmd.DeviceOn,

            });
        }
        else
        {
            StartHeartbeat();
            SubscribeTopics(new[] {
                $"mqtt/{selfClientId}/{MqttEventCmd.AddCoins}",
                $"mqtt/{selfClientId}/{MqttEventCmd.PlayGame}",
                $"mqtt/{selfClientId}/{MqttEventCmd.GetBonus}",
            });
        }
    }

    private void SubscribeTopics(string[] topics)
    {
        byte[] qosLevels = new byte[topics.Length];
        for (int i = 0; i < qosLevels.Length; i++)
        {
            qosLevels[i] = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
        }
        _client.Subscribe(topics, qosLevels);
    }


    private void SubscribeOneTopic(string additionalTopic = null)
    {
        List<string> topics = new List<string>();
        if (!string.IsNullOrEmpty(additionalTopic))
        {
            topics.Add(additionalTopic);
        }
        byte[] qosLevels = new byte[topics.Count];
        for (int i = 0; i < qosLevels.Length; i++)
        {
            qosLevels[i] = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
        }
        _client.Subscribe(topics.ToArray(), qosLevels);
    }

    private void StartHeartbeat()
    {
        SendHeartbeat();
        Timer.LoopAction(5, (sec) => SendHeartbeat());
    }

    private void SendHeartbeat()
    {
        if (!_isConnected) return;

        _client.Publish(
            MqttEventCmd.DeviceOn,
            Encoding.UTF8.GetBytes(selfClientId),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
            false
        );
    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
       // DebugUtils.Log("topic:"+e.Topic+": " +e.Message);
        string topic = e.Topic;
        string payload = Encoding.UTF8.GetString(e.Message);


        Loom.QueueOnMainThread((res) =>
        {
            try
            {
                HandleCommandResponse(topic, payload);
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"消息处理错误: {ex.Message}");
            }
        }, null);

        /*
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            try
            {
                HandleCommandResponse(topic, payload);
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"消息处理错误: {ex.Message}");
            }
        });*/
    }


    //======================== 消息处理核心 ========================//
    private void HandleCommandResponse(string topic, string payload)
    {
        if (topic == MqttEventCmd.DeviceOn)
        {
            targetClientId = payload;
            if (CurMqttAppType == MqttAppType.IsCtrlApp)
            {
                EventCenter.Instance.EventTrigger(MqttEventHandle.SendBoardID, targetClientId);
                SubscribeOneTopic($"mqtt/{targetClientId}/{MqttEventCmd.AddCoinsAck}");
                SubscribeOneTopic($"mqtt/{targetClientId}/{MqttEventCmd.GetBonusAck}");
                SubscribeOneTopic($"mqtt/{targetClientId}/{MqttEventCmd.PlayGameAck}");
            }
            return;
        }
       

        string[] segments = topic.Split('/');
        string commandType = segments[2]; // 解析命令类型：AddCoins、PlayGame等

        //-- 根据协议规范选择解析方式 --//
        switch (commandType)
        {
            //=== 简单参数命令 ===//
            case MqttEventCmd.AddCoins:
                EventCenter.Instance.EventTrigger<string>(MqttEventCmd.AddCoins, payload);
                break;
            case MqttEventCmd.AddCoinsAck:
                EventCenter.Instance.EventTrigger<int>(MqttEventCmd.AddCoinsAck, Int32.Parse(payload));
                break;
            case MqttEventCmd.PlayGame:
                EventCenter.Instance.EventTrigger<int>(MqttEventCmd.PlayGame, Int32.Parse(payload));
                break;
            case MqttEventCmd.PlayGameAck:
                EventCenter.Instance.EventTrigger(MqttEventCmd.PlayGameAck);
                break;
            case MqttEventCmd.GetBonus:
                EventCenter.Instance.EventTrigger(MqttEventCmd.GetBonus);
                break;
            case MqttEventCmd.GetBonusAck:
                var data = JsonConvert.DeserializeObject<JackpotData>(payload);
                EventCenter.Instance.EventTrigger<JackpotData>(MqttEventCmd.GetBonusAck, data);
                break;
            default:
                DebugUtils.LogWarning($"未知命令类型: {commandType}");
                break;
        }
    }


    // 给目标发送接口
    public void SendCommand(string cmdId, object param = null)
    {
        if (!_isConnected)
        {
            DebugUtils.LogWarning("MQTT未连接");
            return;
        }

        string topic = $"mqtt/{targetClientId}/{cmdId}";
        _client.Publish(
            topic,
            Encoding.UTF8.GetBytes(param.ToString()),
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            false
        );
    }

    private void OnDisconnected(object sender, EventArgs e)
    {
        DebugUtils.LogError("重连中..");
        _isConnected = false;

        Loom.QueueOnMainThread((res) =>
        {
            EventCenter.Instance.EventTrigger(MqttEventHandle.MQTTDisconnected);
            ScheduleReconnect();
        }, null);

        /*
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            EventCenter.Instance.EventTrigger(MqttEventHandle.MQTTDisconnected);
            ScheduleReconnect();
        });*/

    }

    private void ScheduleReconnect() =>
        Invoke(nameof(Connect), reconnectInterval);

    protected override void OnDestroy()
    {
        if (_isConnected)
            _client.Disconnect();

        CancelInvoke();

        base.OnDestroy();
    }
}

