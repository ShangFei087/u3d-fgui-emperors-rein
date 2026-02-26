using Newtonsoft.Json;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using Sirenix.OdinInspector;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;



/*
 1. App 告诉 机台要要订阅某个消息（ mqtt/clientid/sub  -- 消息类型A ）
 2. 机台发布消息给App （ mqtt/clientid/pub -- 消息类型A ） 
 * 
 * 
 */



/*
 * 
 * 
指令主题：mqtt/clientid/sub
响应主题：mqtt/clientid/pub
设备主动通知主题：mqtt/clientid/state

*/


public enum MqttAppType
{
    /// <summary> App 控制端 </summary>
    IsCtrlApp,
    /// <summary> 机台端 </summary>
    IsGameApp,
}

// 事件类型定义
public class MqttEvent
{
    // 系统事件
    public const string MQTTConnected = "MQTTConnected";     // 连接成功
    public static string MQTTDisconnected = nameof(MQTTDisconnected);   //public const string MQTTDisconnected = "MQTTDisconnected";
}
public class MqttRemoteCtrlMethod
{

    public const string Off = "Off";
    public const string On = "On";

    //==== App端主动发起


    /// <summary> 退币数量 </summary>
    public const string GetCoinCount = "GetCoinCount";
    /// <summary> 彩金数量 </summary>
    public const string GetBonus = "GetBonus";



    /// <summary> 雨刮控制 </summary>
    public const string Wiper = "Wiper";
    /// <summary> 游戏难度 </summary>
    public const string Setlevel = "Setlevel";



    public const string PlayGame = "PlayGame";
    



    // ==== 机台主动上报
    /// <summary> 彩金主动上报 </summary>
    public const string Bonus = "Bonus";





    // ==== 新版本 ====：

    // ==== 按钮指令
    public const string BtnTicketOut = "BtnTicketOut";
    public const string BtnSpin = "BtnSpin";
    public const string BtnAuto = "BtnAuto";
    public const string BtnBetUp = "BtnBetUp";
    public const string BtnBetDown = "BtnBetDown";
    public const string BtnBetMax = "BtnBetMax";
    public const string BtnTable = "BtnTable";
    public const string BtnPrevious = "BtnPrevious";
    public const string BtnNext = "BtnNext";
    public const string BtnExit = "BtnExit";
    public const string BtnSwitch = "BtnSwitch";

    public const string BtnWiper = "BtnWiper";

    // ==== 查询指令
    public const string GetMachineInfo = "GetMachineInfo";
    public const string GetErrorCode = "GetErrorCode";
    public const string GetGameState = "GetGameState";
    //public const string GetGameJackpot = "GetGameJackpot";


    // ==== 游玩指令
    /// <summary> 连线玩多局指令 </summary>
    public const string TotalSpins = "TotalSpins";


    // ==== 投退币指令
    /// <summary> 投币数量 </summary>
    public const string AddCoins = "AddCoins";
    /// <summary> 上分 </summary>
    public const string ScoreUp = "ScoreUp";
    /// <summary> 下分 </summary>
    public const string ScoreDown = "ScoreDown";

    // ==== 设备上报
    public const string Error = "Error";
    public const string Report = "Report";
    /// <summary> 中奖结果上报 </summary>
    public const string Win = "Win";

}


public partial class MqttRemoteController : MonoSingleton<MqttRemoteController>
{
    /// <summary> 【主题】： app发给指定机台 </summary>
    public string TOPIC_A2M(string id) => $"mqtt/{id}/sub";
    /// <summary> 【主题】： 指定机台发给app </summary>
    public string TOPIC_M2A(string id) => $"mqtt/{id}/pub";

    /// <summary> 【主题】： 任意机台发给app - 心跳：上线 </summary>
    public const string TOPIC_M2A_DEVICE_ON = "mqtt/state/on";

    /// <summary> 【主题】： 任意机台发给app - 心跳：下线 </summary>
    public const string TOPIC_M2A_DEVICE_OFF = "mqtt/state/off";



    [Header("MQTT Settings")]
    [SerializeField] public string brokerAddress = "192.168.3.174";
    [SerializeField] public int brokerPort = 1883;
    [SerializeField] public string selfClientId = "tomappBB";
    [SerializeField] private float reconnectInterval = 5f;
    public string targetClientId = "";

    public string username = "";
    public string password = "";

    public MqttAppType appType;

    private MqttClient _client;
    bool _isConnected;
    public bool isConnected => _isConnected;

    /// <summary> 允许重连 </summary>
    bool _isUseReconnect = true;


    // -1 : 没有， 0 : 系统
    private int _seqID = 0;
    private int CreatSeqID()
    {
        if (++this._seqID > 10000)
            this._seqID = 1;
        return _seqID;
    }
    /// <summary> 请求的seqId </summary>
    /// <remarks>
    /// * RESP 和 NOTIFY，不需要响应
    /// * REQ 需要向响应
    /// </remarks>
    Dictionary<int, float> requestSeqIds = new Dictionary<int, float>();


    public void Init(string brokerAddress, int brokerPort, MqttAppType type, string selfClientId , string username, string password)
    {
        string tp = type == MqttAppType.IsGameApp ? "machine" : "app";
        DebugUtils.LogWarning($"【Mqtt Remote Button】Init {brokerAddress} {brokerPort} {tp} {selfClientId} {username} {password}");

        this.brokerAddress = brokerAddress;
        this.brokerPort = brokerPort;
        this.selfClientId = selfClientId;
        this.appType = type;
        this.username = username;
        this.password = password;
        _isUseReconnect = true;

        Connect();
    }


    public void Close()
    {
        if (_corConnect != null)
            StopCoroutine(_corConnect);
        _corConnect = null;

        _isUseReconnect = false;

        ClearConnect();
    }


    void ClearConnect()
    {
        try
        {
            if (_client != null)
            {

                if(_isConnected && appType == MqttAppType.IsGameApp)
                {
                    _client.Publish(
                        TOPIC_M2A_DEVICE_OFF,
                        Encoding.UTF8.GetBytes(selfClientId),
                        MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                        false
                    );
                    DebugUtils.LogWarning($"【Mqtt Remote Button】<color=green>rpc up</color>  topic: {TOPIC_M2A_DEVICE_OFF}  payload: {selfClientId}");
                }

                _client.Disconnect();
                _client.MqttMsgPublishReceived -= OnMessageReceived;
                _client.ConnectionClosed -= OnDisconnected;
            }
        }
        catch (Exception ex){ }
        _client = null;

        _isConnected = false;
    }


    float CONNECTION_TIMEOUT = 10f; // 10秒超时


    int _connectNumber = 0;
    public void Connect()
    {
        if (_corConnect != null)
            StopCoroutine(_corConnect);
        _corConnect = StartCoroutine(DoConnect());
    }


    Coroutine _corConnect = null;

    IEnumerator DoConnect()
    {

        if (++_connectNumber > 1000)
            _connectNumber = 0;

        bool shouldReconnect = true;

        StopHeartbeat();
        ClearConnect();

        DebugUtils.LogWarning($"【Mqtt Remote Button】连接  ip:{brokerAddress}  port:{brokerPort}  number:{_connectNumber}");

        _client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        _client.MqttMsgPublishReceived += OnMessageReceived;
        _client.ConnectionClosed += OnDisconnected;
        int connectNumber = _connectNumber;

        // 检查连接超时
        bool isNext = false;
        bool isConnected = false;

        Task workerTask = Task.Run(() =>
        {
            try
            {
                _client.Connect(selfClientId, username, password); //这里会造成堵塞
                isConnected = _client.IsConnected;
                isNext = true; // 完成继续
            }
            catch (Exception ex)
            {
                isConnected = false;
                isNext = true;  // 报错继续
            }
        });

        float lastRunTimeS = Time.unscaledTime;
        yield return new WaitUntil(()=> isNext == true || (Time.unscaledTime - lastRunTimeS > CONNECTION_TIMEOUT) );

        if (connectNumber != _connectNumber)  //用户断开本次连接
        {
            shouldReconnect = false;
            yield break;
        }

        if (isNext == false && Time.unscaledTime - lastRunTimeS > CONNECTION_TIMEOUT)
        {
            DebugUtils.LogWarning($"【Mqtt Remote Button】MQTT连接超时（{CONNECTION_TIMEOUT}秒） number:{_connectNumber}");
            shouldReconnect = true;
        }
        else
        {

            if (isConnected)
            {
                DebugUtils.LogWarning($"【Mqtt Remote Button】MQTT连接成功  number:{_connectNumber}");
                shouldReconnect = false;
                HandleConnectionSuccess();
            }
            else
            {
                DebugUtils.LogWarning($"【Mqtt Remote Button】MQTT连接失败  number:{_connectNumber}");
                shouldReconnect = true;
            }
        }

        if (shouldReconnect)
        {
            ClearConnect();
            // 超时处理：断开连接尝试
            ScheduleReconnect();
        }
    }






    /// <summary>
    /// 连接上
    /// </summary>
    private void HandleConnectionSuccess()
    {
        DebugUtils.LogWarning($"【Mqtt Remote Button】MQTT连接成功 ip: {brokerAddress}  port: {brokerPort}  username: {username}  password: {password}");

        _isConnected = true;

        //Loom.QueueOnMainThread((res) => EventCenter.Instance.EventTrigger(MqttEventHandle.MQTTConnected), null);

        // 订阅消息
        if (appType == MqttAppType.IsCtrlApp)
        {
            SubscribeTopics(new[] {
                TOPIC_M2A_DEVICE_ON,
                TOPIC_M2A_DEVICE_OFF,
            });
        }
        else
        {
            StartHeartbeat();
            SubscribeTopics(new[] {
                TOPIC_M2A_DEVICE_ON,
                TOPIC_A2M(selfClientId),
                //MQTT_M2A(selfClientId),
                //$"mqtt/{selfClientId}/{MqttEventCmd.Request}",
                //$"mqtt/{selfClientId}/{MqttEventCmd.Respond}",
            });
        }
    }

    /// <summary>
    /// 订阅多个消息
    /// </summary>
    /// <param name="topics"></param>
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



    private void StopHeartbeat()
    {
        if (_corHeartbeat != null)
            StopCoroutine(_corHeartbeat);
        _corHeartbeat = null;
    }

    Coroutine _corHeartbeat = null;
    private void StartHeartbeat()
    {
        StopHeartbeat();
        _corHeartbeat = StartCoroutine(OnSendHeartbeat());
    }

    IEnumerator OnSendHeartbeat()
    {
        while (true)
        {
            SendHeartbeat();
            yield return new WaitForSecondsRealtime(5f);
        }
    }

    private void SendHeartbeat()
    {
        if (!_isConnected) return;

        string topic = TOPIC_M2A_DEVICE_ON;

        _client.Publish(
            //MqttEventCmd.DeviceOn,
            topic,
            Encoding.UTF8.GetBytes(selfClientId),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
            false
        );

        DebugUtils.LogWarning($"【Mqtt Remote Button】<color=green>rpc up</color>  topic: {topic}  payload: {selfClientId}");
    }



    /// <summary>
    /// 接受到数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
                DebugUtils.LogWarning($"【Mqtt Remote Button】消息处理错误: {ex.Message}");
            }
        }, null);

    }

    /// <summary> App端发起 </summary>
    const string REQ = "req";
    /// <summary> 设备端响应 </summary>
    const string RESP = "resp";
    /// <summary> 设备端通知 </summary>
    const string NOTIFY = "notify";

    //======================== 消息处理核心 ========================//

    /// <summary>
    /// 下行消息处理
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payload"></param>
    private void HandleCommandResponse(string topic, string payload)
    {
        DebugUtils.LogWarning($"【Mqtt Remote Button】<color=yellow>rpc down</color>  topic: {topic}  payload: {payload}");


        if (appType == MqttAppType.IsCtrlApp)  // app
        {
            /*if (msgtype != NOTIFY 
                && msgtype != RESP 
                && topic != MQTT_M2A_DEVICE_OFF 
                && topic != MQTT_M2A_DEVICE_ON) return;*/


            if (topic == TOPIC_M2A_DEVICE_OFF || topic == TOPIC_M2A_DEVICE_ON)
            {
               EventCenter.Instance.EventTrigger<EventData>("ON_MQTT_REMOTE_CONTROL_EVENT",
                   new EventData<string>(  topic == TOPIC_M2A_DEVICE_OFF ? MqttRemoteCtrlMethod.Off : MqttRemoteCtrlMethod.On, payload));
            }
            else
            {

                JObject res = JObject.Parse(payload);
                string msgtype = res["msgtype"].ToObject<string>();
                string method = res["method"].ToObject<string>();
                int seqid = res["seq_id"].ToObject<int>();
                Debug.Log("msgtype==" + msgtype);
                Debug.Log("method==" + method);
                Debug.Log("seqid==" + seqid);
                /*bool isOk = true;
                switch (method)
                {
                    case MqttRemoteCtrlMethod.Bonus:
                        break;
                    default:
                        isOk = false;
                        break;
                }

                if (isOk)*/
                EventCenter.Instance.EventTrigger<EventData>("ON_MQTT_REMOTE_CONTROL_EVENT",
                        new EventData<string>(method, payload));
            }

        }
        else  // 机器
        {
            /* if (msgtype != REQ  
                && topic != MQTT_M2A_DEVICE_OFF
                && topic != MQTT_M2A_DEVICE_ON) return;*/

            if (topic == TOPIC_M2A_DEVICE_OFF || topic == TOPIC_M2A_DEVICE_ON)
            {
                //EventCenter.Instance.EventTrigger<EventData>("ON_MQTT_REMOTE_CONTROL_EVENT", new EventData<string>( topic == MQTT_M2A_DEVICE_OFF ? MqttRemoteCtrlMethod.Off : MqttRemoteCtrlMethod.On, payload));
            }
            else
            {


                JObject res = JObject.Parse(payload);
                string msgtype = res["msgtype"].ToObject<string>();
                string method = res["method"].ToObject<string>();
                int seqid = res["seq_id"].ToObject<int>();

               /* bool isOk = true;
                switch (method)
                {
                    // Mqtt
                    case MqttRemoteCtrlMethod.AddCoins:
                        break;
                    case MqttRemoteCtrlMethod.GetCoinCount:
                        break;
                    case MqttRemoteCtrlMethod.GetBonus:
                        break;
                    default:
                        isOk = false;
                        break;
                }
                if (isOk)*/
                    EventCenter.Instance.EventTrigger<EventData>("ON_MQTT_REMOTE_CONTROL_EVENT",
                        new EventData<string>(method, payload));
            }

        }
    }


    public void SetTargetClientId(string clientId)
    {
        if (appType != MqttAppType.IsCtrlApp)
            return;

        if (!string.IsNullOrEmpty(targetClientId))  //取消旧的订阅
        {
            // _client.Unsubscribe(new string[] { $"mqtt/{targetClientId}/pub" });
            _client.Unsubscribe(new string[] { TOPIC_M2A(targetClientId) });  
        }

        targetClientId = clientId;
        //SubscribeOneTopic($"mqtt/{targetClientId}/pub");
        SubscribeOneTopic(TOPIC_M2A(targetClientId));
    }

    public void ClearTargetClientId()
    {
        if (appType != MqttAppType.IsCtrlApp)
            return;
        if (!string.IsNullOrEmpty(targetClientId))  //取消旧的订阅
        {
            //_client.Unsubscribe(new string[] { $"mqtt/{targetClientId}/pub" });
            _client.Unsubscribe(new string[] { TOPIC_M2A(targetClientId) });
        }
        targetClientId = null;
    }


    /// <summary>
    /// 请求数据
    /// </summary>
    /// <param name="method"></param>
    /// <param name="data"></param>
    public int RequestCommand(string method, object data)
    {
        if (!_isConnected)
        {
            DebugUtils.LogWarning("【Mqtt Remote Button】MQTT未连接");
            return -1;
        }
        if (appType == MqttAppType.IsCtrlApp && string.IsNullOrEmpty(targetClientId))
        {
            DebugUtils.LogWarning("【Mqtt Remote Button】没有设置 targetClientId");
            return -1;
        }

        JArray body = new JArray();
        if (data != null)
        {
            //string dataStr = JsonConvert.SerializeObject(data);
            string dataStr = data is string ? (string)data : JsonConvert.SerializeObject(data);
            body = JArray.Parse($"[{dataStr}]");
        }



        int seqId = CreatSeqID();
        JObject req = JObject.Parse("{}");
        req["msgtype"] = appType == MqttAppType.IsCtrlApp ? REQ : NOTIFY;
        req["method"] = method;
        req["body"] = body;
        req["seq_id"] = seqId;
        //string topic = $"mqtt/{targetClientId}/{MqttEventCmd.Request}";

        string topic = appType == MqttAppType.IsCtrlApp ?
            TOPIC_A2M(targetClientId) : TOPIC_M2A(selfClientId);


        //string payload = req.ToString(); //存在格式
        string payload = JsonConvert.SerializeObject(req, Formatting.None); //去掉格式

        _client.Publish(
            topic,
            Encoding.UTF8.GetBytes(payload),
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            false
        );
        DebugUtils.LogWarning($"【Mqtt Remote Button】<color=green>rpc up</color>  topic: {topic}  payload: {payload}");

        return seqId;
    }

    /// <summary>
    /// 响应请求
    /// </summary>
    /// <param name="method"></param>
    /// <param name="data"></param>
    /// <param name="seqId"></param>

    public void RespondCommand(string method, object data, int seqId)
    {
        if (!_isConnected)
        {
            DebugUtils.LogWarning("【Mqtt Remote Button】MQTT未连接");
            return;
        }
        if (appType == MqttAppType.IsCtrlApp && string.IsNullOrEmpty(targetClientId))
        {
            DebugUtils.LogWarning("【Mqtt Remote Button】没有设置 targetClientId");
            return;
        }


        //JArray body = JArray.Parse("[]");
        //body.Add(data);

        JArray body = new JArray();
        if (data != null)
        {
            string dataStr = data is string? (string)data: JsonConvert.SerializeObject(data);
            body = JArray.Parse($"[{dataStr}]");
        }

        JObject res = JObject.Parse("{}");
        res["msgtype"] = appType == MqttAppType.IsCtrlApp ? REQ : RESP;
        res["method"] = method;
        res["seq_id"] = seqId;
        res["body"] = body;


        string topic = appType == MqttAppType.IsCtrlApp ?
            TOPIC_A2M(targetClientId) : TOPIC_M2A(selfClientId);


        //string payload = res.ToString(); //存在格式
        string payload = JsonConvert.SerializeObject(res, Formatting.None); //去掉格式

        _client.Publish(
            topic,
            Encoding.UTF8.GetBytes(payload),
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            false
        );
        DebugUtils.LogWarning($"【Mqtt Remote Button】<color=green>rpc up</color>  topic: {topic}  payload: {payload}");
    }

    private void OnDisconnected(object sender, EventArgs e)
    {
        _isConnected = false;

        Loom.QueueOnMainThread((res) =>
        {
            EventCenter.Instance.EventTrigger(MqttEvent.MQTTDisconnected); //"MQTTDisconnected"
            ScheduleReconnect();
        }, null);
    }

    private void ScheduleReconnect()
    {
        if (_isUseReconnect)
        {
            DebugUtils.LogWarning("【Mqtt Remote Button】重连中..");
            Invoke(nameof(Connect), reconnectInterval);
        }
    }


    protected override void OnDestroy()
    {
        ClearConnect();

        CancelInvoke();

        base.OnDestroy();
    }


}


public partial class MqttRemoteController : MonoSingleton<MqttRemoteController>
{

    public void TestA2MRequestCommand(string method, object data)
    {
        if (!_isConnected)
        {
            DebugUtils.LogWarning("【Mqtt Remote Button】MQTT未连接");
            return;
        }

        JArray body01 = new JArray();
        body01.Add(new JObject
        {
            ["code"] = 0
        });

        //JArray body = new JArray.Parse("[]");
        /* 下面代码报错
        JArray body = new JArray();
        if (data != null) {
            string dataStr = JsonConvert.SerializeObject(data);
            JObject dat = JObject.Parse(dataStr);
            body.Add(dat);
        }*/
        JArray body = new JArray();
        if (data != null)
        {
            //string dataStr = JsonConvert.SerializeObject(data);
            string dataStr = data is string ? (string)data : JsonConvert.SerializeObject(data);
            body = JArray.Parse($"[{dataStr}]");
        }


        JObject req = JObject.Parse("{}");
        req["msgtype"] = REQ;
        req["method"] = method;
        req["body"] = body;
        req["seq_id"] = CreatSeqID();

        targetClientId = selfClientId;
        string topic = TOPIC_A2M(targetClientId);

        //string payload = req.ToString(); //存在格式
        string payload = JsonConvert.SerializeObject(req, Formatting.None); //去掉格式

        _client.Publish(
            topic,
            Encoding.UTF8.GetBytes(payload),
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            false
        );
        DebugUtils.LogWarning($"【Mqtt Remote Button Test A2M】rpc up  topic: {topic}  payload: {payload}");
    }



    [Button]
    void TestA2MSendBtnTicketOut()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnTicketOut, null);
    }


    [Button]
    void TestA2MSendBtnSpin()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnSpin, null);
    }

    [Button]
    void TestA2MSendBtnAuto()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnAuto, null);
    }

    [Button]
    void TestA2MSendBtnExit()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnExit, null);
    }



    [Button]
    void TestA2MSendBtnBetUp()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnBetUp, null);
    }

    [Button]
    void TestA2MSendBtnBetDown()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnBetDown, null);
    }
    [Button]
    void TestA2MSendBtnBetMax()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnBetMax, null);
    }

    [Button]
    void TestA2MSendBtnTable()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnTable, null);
    }

    [Button]
    void TestA2MSendBtnPrevious()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnPrevious, null);
    }

    [Button]
    void TestA2MSendBtnNext()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnNext, null);
    }


    [Button]
    void TestA2MSendBtnSwitch()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.BtnSwitch, null);
    }



    [Button]
    void TestA2MSendCmdGetGameState()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.GetGameState, null);
    }

    [Button]
    void TestA2MSendCmdGetMachineInfo()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.GetMachineInfo, null);
    }


    [Button]
    void TestA2MSendCmdGetErrorCode()
    {
        targetClientId = selfClientId;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.GetErrorCode, null);
    }


    /*
    [Button]
    void TestA2MReqGetCoinCount()
    {
        targetClientId = selfClientId;
        Dictionary<string, object> data = null;
        TestA2MRequestCommand(MqttRemoteCtrlMethod.GetCoinCount, data);
    }*/

    [Button]
    void TestA2MReqAddCoins()
    {
        targetClientId = selfClientId;
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            ["num"] = 3,
        };
        TestA2MRequestCommand(MqttRemoteCtrlMethod.AddCoins, data);
    }

}

