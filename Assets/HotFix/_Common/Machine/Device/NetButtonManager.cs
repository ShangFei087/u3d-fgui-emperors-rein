using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using GameMaker;
using Newtonsoft.Json.Linq;
using static NetButtonManager;


public class NetButtonHandle
{

    public BtnName buttonName;

    public Action<NetButtonInfo> onClick;

    public string mark;

}

public class NetButtonInfo
{
    public string dataType = "";
    public string toDo;
    public object data;
    public Action<bool> onCallback;
}


public partial class NetButtonManager : MonoSingleton<NetButtonManager>
{
    private CoController _corCtrl;
    private CoController corCtrl
    {
        get
        {
            if (_corCtrl == null)
            {
                _corCtrl = new CoController(this);
            }
            return _corCtrl;
        }
    }

    public void ClearCoStartsWith(string prefix) => corCtrl.ClearCoStartsWith(prefix);
    public void ClearCo(string name) => corCtrl.ClearCo(name);

    public void ClearAllCor() => corCtrl.ClearAllCo();

    public void DoCo(string name, IEnumerator routine) => corCtrl.DoCo(name, routine);

    public bool IsCo(string name) => corCtrl.IsCo(name);

    public IEnumerator DoTaskRepeat(Action cb, int ms) => corCtrl.DoTaskRepeat(cb, ms);

    public IEnumerator DoTask(Action cb, int ms) => corCtrl.DoTask(cb, ms);
    // Start is called before the first frame update
    void Start()
    {
        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_MQTT_REMOTE_CONTROL_EVENT, OnMqttRemoteControlButton);
    }

    protected override void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_MQTT_REMOTE_CONTROL_EVENT, OnMqttRemoteControlButton);

        base.OnDestroy();
    }


    List<NetButtonHandle> handles = new List<NetButtonHandle>();
    public void AddHandles(NetButtonHandle info)
    {
        handles.Add(info);
    }
    public void ReomveHandles(NetButtonHandle info)
    {
        ClearCoStartsWith($"{info.mark}__{info.buttonName}");

        handles.Remove(info);
    }
    public void ReomveHandles(string mark)
    {
        ClearCoStartsWith(mark);

        int idx = handles.Count;
        while (--idx >= 0)
        {
            if (handles[idx].mark == mark)
                handles.RemoveAt(idx);
        }
    }



    public enum BtnName
    {
        /// <summary> 加注 </summary>
        BtnBetUp,
        /// <summary> 减注 </summary>
        BtnBetDown,
        /// <summary> 最大注 </summary>
        BtnBetMax,
        /// <summary> 说明页 </summary>
        BtnPayTable,
        /// <summary> 开玩 </summary>
        BtnSpin,
        /// <summary> 停止 </summary>
        BtnStop,
        /// <summary> 自动 </summary>
        BtnAuto,
        /// <summary> 上一页 </summary>
        BtnPrev,
        /// <summary> 下一页 </summary>
        BtnNext,
        /// <summary> 退出 </summary>
        BtnExit,
        /// <summary> 切换游戏配置 </summary>
        BtnSwitch,
        /// <summary> 退票 </summary>
        BtnTicketOut,
        /// <summary> 雨刷 </summary>
        BtnWiper
    }

    /*
    /// <summary> 加注 </summary>
    public const string BTN_BET_UP = "BTN_BET_UP";
    /// <summary> 减注 </summary>
    public const string BTN_BET_DOWN = "BTN_BET_DOWN";
    /// <summary> 最大注 </summary>
    public const string BTN_BET_MAX = "BTN_BET_MAX";
    /// <summary> 说明页 </summary>
    public const string BTN_TABLE = "BTN_TABLE";
    /// <summary> 开玩 </summary>
    public const string BTN_SPIN = "BTN_SPIN";
    /// <summary> 停止 </summary>
    public const string BTN_STOP = "BTN_STOP";
    /// <summary> 自动 </summary>
    public const string BTN_AUTO = "BTN_AUTO";
    /// <summary> 上一页 </summary>
    public const string BTN_PREV = "BTN_PREV";
    /// <summary> 下一页 </summary>
    public const string BTN_NEXT = "BTN_NEXT";
    /// <summary> 退出 </summary>
    public const string BTN_EXIT = "BTN_EXIT";
    /// <summary> 切换游戏配置 </summary>
    public const string BTN_SWITCH = "BTN_SWITCH";
    /// <summary> 退票 </summary>
    public const string BTN_TICKET = "BTN_TICKET";

    /// <summary> 雨刷 </summary>
    public const string BTN_WIPER = "BTN_WIPER";
    */

    [Button]
    public void TestBtn(BtnName type) => DoButton(type);


    [Button]
    public void TestCommonPopOne()
    {
        CommonPopupHandler.Instance.OpenPopupSingle(
           new CommonPopupInfo()
           {
               isUseXButton = false,
               buttonAutoClose1 = true,
               buttonAutoClose2 = true,
               type = CommonPopupType.YesNo,
               text = I18nMgr.T("Error Password"),
               buttonText1 = I18nMgr.T("Cancel"),
               buttonText2 = I18nMgr.T("Confirm"),
           });
    }

    /// <summary> mqtt 网页按钮数据 </summary>
    public const string DATA_MACHINE_BUTTON_CONTROL = "DATA_MACHINE_BUTTON_CONTROL";

    public void DoButton(BtnName btnName)
    {
        switch (btnName)
        {
            case BtnName.BtnBetUp: 
                break;
        }

        NetButtonInfo info = new NetButtonInfo()
        {
            dataType = DATA_MACHINE_BUTTON_CONTROL,
        };

        OnInvoke(btnName, info);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="btnName">那个按钮</param>
    /// <param name="dataType">数据类型：普通、mqtt数据、视讯</param>
    /// <param name="data"> 数据</param>
    /// <param name="onCallback">回调函数</param>
    private void OnInvoke(BtnName btnName, NetButtonInfo info)
    {
        for (int i=0; i< handles.Count; i++ )
        {
            if(handles[i].buttonName == btnName)
                handles[i].onClick.Invoke(info);
        }
    }


    #region 播放UI按钮按下的动画
    public void ShowUIAminButtonClick(Button btn , string mark, BtnName btnName) // mark__btnName;
    {
        string btnNameStr = Enum.GetName(typeof(BtnName), btnName);
        if (btn.interactable)
        {
            DoCo($"{mark}__{btnNameStr}", DoBtnClick(btn));
        }
    }
    IEnumerator DoBtnClick(Button btn)
    {
        //只有按下动画，不触发事件
        btn.OnPointerDown(new PointerEventData(null)
        {
            button = PointerEventData.InputButton.Left,
        });

        yield return new WaitForSecondsRealtime(0.15f);

        //只有弹起动画，不触发事件
        btn.OnPointerUp(new PointerEventData(null)
        {
            button = PointerEventData.InputButton.Left,
        });

        btn.onClick.Invoke();//或 btn.OnSubmit(null); 
    }


    //IPointerDownHandler, IPointerUpHandler
    public void ShowUIAminButtonClick(Action onPointerDown, Action onPointerUp, string mark, BtnName btnName) // mark__btnName;
    {
        string btnNameStr = Enum.GetName(typeof(BtnName), btnName);
        DoCo($"{mark}__{btnNameStr}", DoBtnClick(onPointerDown, onPointerUp, 0.15f));
    }
    IEnumerator DoBtnClick(Action onPointerDown, Action onPointerUp, float timeS)
    {
        //只有按下动画，不触发事件
        onPointerDown.Invoke();

        yield return new WaitForSecondsRealtime(timeS);//0.15f
        //只有弹起动画，不触发事件
        onPointerUp.Invoke();
    }

    public void ShowUIAminButtonLongClick(Action onPointerDown, Action onPointerUp, string mark,BtnName btnName ) // mark__btnName;
    {
        string btnNameStr = Enum.GetName(typeof(BtnName), btnName);
        DoCo($"{mark}__{btnNameStr}", DoBtnClick(onPointerDown, onPointerUp, 1.5f));
    }


    #endregion


}





/// <summary>
///  针对Mqtt的按钮控制
/// </summary>
public partial class NetButtonManager : MonoSingleton<NetButtonManager>
{

    /// <summary> mqtt 网页按钮数据 </summary>
    public const string DATA_MQTT_REMOTE_CONTROL = "DATA_MQTT_REMOTE_CONTROL";



    Dictionary<string, BtnName> btnMap = new Dictionary<string, BtnName>()
    {
        [MqttRemoteCtrlMethod.BtnTicketOut] = BtnName.BtnTicketOut,
        [MqttRemoteCtrlMethod.BtnSpin] = BtnName.BtnSpin,  
        [MqttRemoteCtrlMethod.BtnAuto] = BtnName.BtnAuto,// BTN_AUTO,
        [MqttRemoteCtrlMethod.BtnBetUp] = BtnName.BtnBetUp,// BTN_BET_UP,
        [MqttRemoteCtrlMethod.BtnBetDown] = BtnName.BtnBetDown, // BTN_BET_DOWN,
        [MqttRemoteCtrlMethod.BtnBetMax] = BtnName.BtnBetMax,// BTN_BET_MAX,
        [MqttRemoteCtrlMethod.BtnTable] = BtnName.BtnPayTable, //BTN_TABLE,
        [MqttRemoteCtrlMethod.BtnPrevious] = BtnName.BtnPrev,// BTN_PREV,
        [MqttRemoteCtrlMethod.BtnNext] = BtnName.BtnNext, // BTN_NEXT,
        [MqttRemoteCtrlMethod.BtnExit] = BtnName.BtnExit ,//BTN_EXIT,
        [MqttRemoteCtrlMethod.BtnSwitch] = BtnName.BtnSwitch, // BTN_SWITCH,
        [MqttRemoteCtrlMethod.BtnWiper] = BtnName.BtnWiper, // BTN_SWITCH,
    };



    public void OnMqttRemoteControlButton(EventData req)
    {
        if (req.name.StartsWith("Btn"))
        {
            JObject data = null;
            int seqId;
            BtnName btnTarget;

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


            try
            {
                btnTarget = btnMap[req.name];
            }
            catch (Exception e)
            {
                Dictionary<string, object> resErr = new Dictionary<string, object>()
                {
                    ["code"] = 1,
                    ["msg"] = "协议名错误",
                    ["data"] = req.value
                };
                MqttRemoteController.Instance.RespondCommand(req.name, resErr, -1);
                return;
            }


            OnInvoke(btnTarget,
                new NetButtonInfo()
                {
                    dataType = DATA_MACHINE_BUTTON_CONTROL,
                    //toDo = res.name,
                    //data = res.value,
                    onCallback = (isOk) =>
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>()
                        {
                            ["code"] = (bool)isOk ? 0 : 1
                        };
                        MqttRemoteController.Instance.RespondCommand(req.name, data, seqId);
                    }
                });
        }

        switch (req.name)
        {
            /*   

            case MqttRemoteCtrlMethod.PlayGame:
                {
                    int data = int.Parse((string)req.value);
                    switch (data)
                    {
                        case 0:  //Spin
                            {
                                OnInvoke(BTN_SPIN, 
                                    new NetButtonInfo()
                                    {
                                       dataType = DATA_MQTT_REMOTE_CONTROL,
                                       toDo = "Spin",
                                       data = req.value,
                                       onClick = (isOk) =>
                                       {
                                           MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.PlayGame, (bool)isOk ? 1 : 2);
                                       }
                                    });

                            }
                            break;
                        case 1: //Auto
                            {
                                OnInvoke(BTN_AUTO, new NetButtonInfo()
                                {
                                    dataType = DATA_MQTT_REMOTE_CONTROL,
                                    toDo = "Auto",
                                    data = req.value,
                                    onClick = (isOk) =>
                                    {
                                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.PlayGame, (bool)isOk ? 1 : 2);
                                    }
                                });

                            }
                            break;
                        case 2: //Stop Auto
                            {
                                OnInvoke(BTN_STOP, new NetButtonInfo()
                                {
                                    dataType = DATA_MQTT_REMOTE_CONTROL,
                                    toDo = "StopAuto",
                                    data = req.value,
                                    onClick = (isOk) =>
                                    {
                                        MqttRemoteController.Instance.RespondCommand(MqttRemoteCtrlMethod.PlayGame, (bool)isOk ? 1 : 2);
                                    }
                                });
                            }
                            break;
                    }
                }
                break;
         */

            default:

                break;
        }
    }

}