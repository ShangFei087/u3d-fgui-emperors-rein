using GameMaker;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;
using System.Linq.Expressions;



public class NetCmdHandle
{

    public string cmdName;

    public Action<NetCmdInfo> onInvoke;

    public string mark;

}

public class NetCmdInfo
{
    public string dataType = "";
    public object data;
    public Action<object> onCallback;  // object可以是任何类型  object = object[] 、 Dictionary 、List
}



public partial class NetCmdManager : MonoSingleton<NetCmdManager>
{
    /*
    private CorController _corCtrl;
    private CorController corCtrl
    {
        get
        {
            if (_corCtrl == null)
            {
                _corCtrl = new CorController(this);
            }
            return _corCtrl;
        }
    }

    public void ClearCorStartsWith(string prefix) => corCtrl.ClearCorStartsWith(prefix);
    public void ClearCo(string name) => corCtrl.ClearCo(name);

    public void ClearAllCor() => corCtrl.ClearAllCor();

    public void DoCo(string name, IEnumerator routine) => corCtrl.DoCo(name, routine);

    public bool IsCo(string name) => corCtrl.IsCo(name);

    public IEnumerator DoTaskRepeat(Action cb, int ms) => corCtrl.DoTaskRepeat(cb, ms);

    public IEnumerator DoTask(Action cb, int ms) => corCtrl.DoTask(cb, ms);

    */

    List<NetCmdHandle> handles = new List<NetCmdHandle>();
    public void AddHandles(NetCmdHandle info)
    {
        handles.Add(info);
    }
    public void ReomveHandles(NetCmdHandle info)
    {
        //ClearCorStartsWith($"{info.mark}__{info.cmdName}");

        handles.Remove(info);
    }
    public void ReomveHandles(string mark)
    {
        //ClearCorStartsWith(mark);

        int idx = handles.Count;
        while (--idx >= 0)
        {
            if (handles[idx].mark == mark)
                handles.RemoveAt(idx);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cmdName">那个按钮</param>
    /// <param name="info">NetCmdInfo数据</param>
    private void OnInvoke(string cmdName, NetCmdInfo info)
    {
        for (int i = 0; i < handles.Count; i++)
        {
            if (handles[i].cmdName == cmdName)
                handles[i].onInvoke.Invoke(info);
        }
    }
}


public partial class NetCmdManager : MonoSingleton<NetCmdManager>
{
    /// <summary> 指令：投币</summary>
    public const string CMD_COIN_IN = "CMD_COIN_IN";
    /// <summary> 指令：退票</summary>
    public const string CMD_COIN_OUT = "CMD_COIN_OUT";
    /// <summary> 指令：游戏彩金当前值</summary>
    public const string CMD_JACKPOT_CUR_DATA = "CMD_JACKPOT_CUR_DATA";

    /// <summary> 指令：上分指令</summary>
    public const string CMD_SCORE_UP = "CMD_SCORE_UP";

    /// <summary> 指令：下分指令</summary>
    public const string CMD_SCORE_DOWN = "CMD_SCORE_DOWN";


    /// <summary> 指令：连续玩指令</summary>
    public const string CMD_TOTAL_SPINS = "CMD_TOTAL_SPINS";



    /// <summary> 数据上报：赢分 </summary>
    public const string CMD_REPORT_WIN = "CMD_REPORT_WIN";



    void Start()
    {
        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_MQTT_REMOTE_CONTROL_EVENT, OnMqttRemoteControlCmd);
    }

    protected override void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_MQTT_REMOTE_CONTROL_EVENT, OnMqttRemoteControlCmd);

        base.OnDestroy();
    }


    [Button]
    public void TestCmdCoinIn()
    {
        OnInvoke(CMD_COIN_IN, new NetCmdInfo()
        {
            dataType = "",
            data = 8,
            onCallback = (isOk) =>
            {
                DebugUtils.Log($"TestCmdCoinIn: {isOk}");
            }
        });
    }

    [Button]
    public void TestCmdCoinOut()
    {
        OnInvoke(CMD_COIN_OUT, new NetCmdInfo()
        {
            dataType = "",
            data = null,
            onCallback = (coinOutCount) =>
            {
                DebugUtils.Log($"退票个数： {coinOutCount}");
            }
        });
    }
}




public partial class NetCmdManager : MonoSingleton<NetCmdManager>
{

    /// <summary> mqtt 网页按钮数据 </summary>
    public const string DATA_MQTT_REMOTE_CONTROL = "DATA_MQTT_REMOTE_CONTROL";




    // 监听mqtt的cmd指令
    public void OnMqttRemoteControlCmd(EventData req)
    {

        Debug.Log("监听mqtt的cmd指令");
        Debug.Log(req.name);
        if (req.name.StartsWith("Btn") || req.name == MqttRemoteCtrlMethod.On || req.name == MqttRemoteCtrlMethod.Off)
        {
            return;
        }


        //JObject data = JObject.Parse((string)req.value);
        //int seqId = data["seq_id"].ToObject<int>();


        JObject data = null;
        int seqId;


        try
        {
            data = JObject.Parse((string)req.value);
        }
        catch (Exception e)
        {
            Dictionary<string, object> resErr = new Dictionary<string, object>()
            {
                ["code"] = 1,
                ["msg"] = "数据结构错误",
                ["data"] = req.value
            };
            MqttRemoteController.Instance.RespondCommand(req.name, resErr, -1);
            return;
        }



        try
        {
            seqId = data["seq_id"].ToObject<int>();
        }
        catch (Exception e)
        {
            Dictionary<string, object> resErr = new Dictionary<string, object>()
            {
                ["code"] = 1,
                ["msg"] = "seq_id格式错误",
                ["data"] = req.value
            };
            MqttRemoteController.Instance.RespondCommand(req.name, resErr, -1);
            return;
        }


        Action onDataError = () =>
        {
            Dictionary<string, object> resErr = new Dictionary<string, object>()
            {
                ["code"] = 1,
                ["msg"] = "数据结构错误",
                ["data"] = req.value
            };
            MqttRemoteController.Instance.RespondCommand(req.name, resErr, seqId);
        };


        // 通用逻辑在这里处理，其他页面专属逻辑使用OnInvoke进行触发
        switch (req.name)
        {
            //投币命令
            case MqttRemoteCtrlMethod.AddCoins:
                {

                    JObject node = null;
                    int coinInCount = 0;

                    try
                    {
                        node = JObject.Parse((string)req.value);
                        coinInCount = (int)node["body"][0]["num"];
                    }
                    catch (Exception e)
                    {
                        onDataError();
                        return;
                    }



                    OnInvoke(CMD_COIN_IN, new NetCmdInfo()
                    {
                        dataType = DATA_MQTT_REMOTE_CONTROL,
                        data = coinInCount,
                        onCallback = (isOk) =>
                        {
                            /*
                            int code = isOk ? 1 : 0;
                            string resStr = $@"{{
                                ""code"": {code}
                            }}";
                            JObject res = JObject.Parse(resStr);*/

                            Dictionary<string, object> res = new Dictionary<string, object>()
                            {
                                ["code"] = (bool)isOk ? 0 : 1,
                            };
                            MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.AddCoins, res, seqId);
                        }
                    });
                }
                break;
            case MqttRemoteCtrlMethod.ScoreUp:
                {
                    string tp, orderId;
                    int credit;

                    try
                    {
                        JObject node = data["body"][0] as JObject;
                        tp = node["type"].ToObject<string>();
                        orderId = node["order_id"].ToObject<string>();
                        credit = node["credit"].ToObject<int>();
                    }
                    catch (Exception e)
                    {
                        onDataError();
                        return;
                    }

                    DeviceRemoteInOut.Instance.CreditUp(
                    tp, orderId, credit,
                    (result) =>
                    {

                        Dictionary<string, object> data = result[2] as Dictionary<string, object>;

                        Dictionary<string, object> res = new Dictionary<string, object>()
                        {
                            ["code"] = (int)result[0],
                            ["msg"] = (string)result[1],
                            ["type"] = data["type"],
                            ["order_id"] = data["order_id"],
                            ["credit"] = data["credit"],
                            ["credit_after"] = data["credit_after"],
                        };
                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.ScoreUp, res, seqId);

                    });

                }
                break;
            case MqttRemoteCtrlMethod.ScoreDown:
                {

                    //JObject node = JObject.Parse((string)req.value);
                    //int coinInCount = (int)node["body"][0]["num"];

                    string tp, orderId;
                    int credit;

                    try
                    {

                        JObject node = data["body"][0] as JObject;
                        credit = (int)node["credit"];
                        tp = node["type"].ToObject<string>();
                        orderId = node["order_id"].ToObject<string>();
                    }
                    catch (Exception e)
                    {
                        onDataError();
                        return;
                    }


                    int targetCredit = 0;

                    if (credit == -1)
                    {
                        targetCredit = (int)SBoxModel.Instance.myCredit;
                    }
                    else if(credit > 0)
                    {
                        targetCredit = credit;
                    }


                   // DebugUtils.LogError($"下分的分数： {targetCredit}");

                    DeviceRemoteInOut.Instance.CreditDown(
                    tp, orderId, targetCredit,
                    (result) =>
                    {

                        Dictionary<string, object> data = result[2] as Dictionary<string, object>;

                        Dictionary<string, object> res = new Dictionary<string, object>()
                        {
                            ["code"] = (int)result[0],
                            ["msg"] = (string)result[1],
                            ["type"] = data["type"],
                            ["order_id"] = data["order_id"],
                            ["credit"] = data["credit"],
                            ["credit_after"] = data["credit_after"],
                        };
                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.ScoreDown, res, seqId);

                    });
                    
                }
                break;
            case MqttRemoteCtrlMethod.TotalSpins:
                {


                    JObject node = null;
                    int totalSpinsCount;

                    try
                    {
                        node = data["body"][0] as JObject;
                        totalSpinsCount = (int)node["num"];
                    }
                    catch (Exception e)
                    {
                        onDataError();
                        return;
                    }

                    OnInvoke(CMD_TOTAL_SPINS, new NetCmdInfo()
                    {
                        dataType = DATA_MQTT_REMOTE_CONTROL,
                        data = totalSpinsCount,
                        onCallback = (obj) =>
                        {
                            object[] objs = obj as object[];    
                            int code = (int)objs[0];
                            string msg = (string)objs[1];
                            int num = (int)objs[2];

                            Dictionary<string, object> res = new Dictionary<string, object>()
                            {
                                ["code"] = code,
                                ["msg"] = msg,
                                ["num"] = num,
                            };
                            MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.TotalSpins, res, seqId);
                        }
                    });
                }
                break;
            /*case MqttRemoteCtrlMethod.GetCoinCount:
                {

                    OnInvoke(CMD_COIN_OUT, new NetCmdInfo()
                    {
                        dataType = DATA_MQTT_REMOTE_CONTROL,
                        data = null,
                        onCallback = (coinOutCount) =>
                        {
                            Dictionary<string, object> res = new Dictionary<string, object>()
                            {
                                ["code"] = 0,
                                ["CoinCount"] = coinOutCount,
                            };
                            MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.AddCoins, res, seqId);
                        }
                    });
                }
                break;
            case MqttRemoteCtrlMethod.GetBonus:
                {
                    OnInvoke(CMD_JACKPOT_CUR_DATA, new NetCmdInfo()
                    {
                        dataType = DATA_MQTT_REMOTE_CONTROL,
                        data = null,
                        onCallback = (data) =>
                        {

                            Dictionary<string, object> res = new Dictionary<string, object>()
                            {
                                ["code"] = 0,
                                ["jp1"] = 10000,
                                ["jp2"] = 1000,
                                ["jp3"] = 100,
                                ["jp4"] = 10,
                            };
                            MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.AddCoins, res, seqId);
                        }
                    });
                }
                break;*/
            case MqttRemoteCtrlMethod.GetErrorCode:
                {

                    TextAsset jsn = ResourceManager.Instance.LoadAssetAtPathOnce<TextAsset>(ConfigUtils.GetErrorCode());

                    JSONNode node = JSONNode.Parse("{}");
                    node.Add("code",0);
                    node.Add("msg", 0);
                    node.Add("error_code", JSONNode.Parse(jsn.text));

                    MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.GetErrorCode, node.ToString(), seqId);
                }
                break;
            case MqttRemoteCtrlMethod.GetMachineInfo:
                {

                    ResourceManager02.Instance.LoadAsset<TextAsset>(ConfigUtils.curGameInfoURL, 
                    (TextAsset jsn) =>
                    {
                        JSONNode gameInfoNode = JSONNode.Parse(jsn.text);
                        Dictionary<string, object> res = new Dictionary<string, object>()
                        {
                            ["code"] = 0,
                            ["msg"] = "",
                            ["machine_name"] = (string)gameInfoNode["game_name"],
                            ["active"] = SBoxModel.Instance.isMachineActive,
                            ["avatar_url"] = ConfigUtils.curGameAvararWebUrl
                        };
                        // Assets/GameBackup/_Common/Game Maker/G200/Game Avatar 200.png

                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.GetMachineInfo, res, seqId);

                    });

                }
                break;
            case MqttRemoteCtrlMethod.GetGameState:
                {
                    Dictionary<string, object> res = new Dictionary<string, object>()
                    {
                        ["code"] = 0,
                        ["msg"] = "",
                        ["game_state"] = MainModel.Instance.contentMD.gameState,
                        ["credit"] = MainModel.Instance.myCredit,
                        ["bet"] = MainModel.Instance.contentMD.totalBet,
                        ["is_spin"] = MainModel.Instance.contentMD.isSpin,
                        ["is_auto"] = MainModel.Instance.contentMD.isAuto,
                        ["jp1"] = MainModel.Instance.contentMD.uiGrandJP.curCredit,
                        ["jp2"] = MainModel.Instance.contentMD.uiMajorJP.curCredit,
                        ["jp3"] = MainModel.Instance.contentMD.uiMinorJP.curCredit,
                        ["jp4"] = MainModel.Instance.contentMD.uiMiniJP.curCredit,
                    };


                    MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.GetGameState, res, seqId);
                }
                break;
            /* 暂时不用
             case MqttRemoteCtrlMethod.Report:
                {
                    JObject node = JObject.Parse((string)req.value);
                    string tp = (string)node["body"][0]["type"];

                    if (tp == "SqlSelect")
                    {
                        string sqlSelect = (string)node["body"][0]["sql"];
                        string result = ReportDB.Instance.DoSQLSelect(sqlSelect);

                        Dictionary<string, object> res = new Dictionary<string, object>()
                        {
                            ["code"] = 0,
                            ["msg"] = "",
                            ["game_state"] = MainModel.Instance.contentMD.gameState,
                            ["type"] = tp,
                            ["data"] = result,
                        };
                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.Report, res, seqId);
                    }
                    else
                    {
                        Dictionary<string, object> res = new Dictionary<string, object>()
                        {
                            ["code"] = 1,
                            ["msg"] = "无法识别指令",
                            ["data"] = node.ToString(),
                        };
                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.Report, res, seqId);
                    }

                }
                break;*/
        }
    }

    public void RpcUpReportWin(Dictionary<string, object> req)
    {

        if (SBoxModel.Instance.isConnectRemoteControl)
        {
            /*
            Dictionary<string, object> res = new Dictionary<string, object>()
            {
                ["type"] = "FreeSpinResult",
                ["game_number"] = "112-333-444-555",
                ["total_times"] = 7,
                ["total_coins"] = 1220
            };
            */

            MqttRemoteController.Instance.RequestCommand(MqttRemoteCtrlMethod.Win, req);
        }
    }
}
