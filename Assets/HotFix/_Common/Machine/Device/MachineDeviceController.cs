using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBoxApi;
using System;
using System.Linq;
using GameMaker;
using static NetButtonManager;
using Debug = UnityEngine.Debug;
using System.Reflection;

public class MachineButtonInfo
{
    public MachineButtonKey btnKey;
    public bool isUp;
    public int value;
}
public enum MachineButtonKey
{

    BtnLight,

    //////游戏按钮//////
    BtnSpin,
    BtnPrev,
    BtnNext,
    BtnUp,
    BtnDown,
    BtnExit,
    BtnSwitch,
    BtnBetUp,
    BtnBetDown,
    BtnBetMax,
    BtnPayTable,
    BtnPlayTime,
    //////功能按钮//////
    BtnTicketOut,
    /// <summary> 上分 </summary>
    BtnCreditUp,
    /// <summary> 下分 </summary>
    BtnCreditDown,

    /// <summary> 管理后台 </summary>
    BtnConsole,

    /// <summary> 门开关 </summary>
    BtnDoor,
    
}

public class MachineDeviceController  : MonoSingleton<MachineDeviceController>  // 
{

    public readonly Dictionary<ulong, MachineButtonKey> keyMap = new Dictionary<ulong, MachineButtonKey>()
    {
        
        //游戏按钮：
        { (ulong)SBOX_SWITCH.SWITCH_ENTER ,MachineButtonKey.BtnSpin},  // 开始玩 或 确认
        //##?? { { (ulong)SBOX_IDEA_COIN_PUSH_KEYVALUE.SWITCH_SWITCH, MachineButtonKey.BtnExit},
        { (ulong)SBOX_SWITCH.SWITCH_RULE,MachineButtonKey.BtnPayTable},

        //管理按钮：
        { (ulong)SBOX_SWITCH.SWITCH_PAYOUT ,MachineButtonKey.BtnTicketOut},  // 下一页
        { (ulong)SBOX_SWITCH.SWITCH_SCORE_UP ,MachineButtonKey.BtnCreditUp},
        { (ulong)SBOX_SWITCH.SWITCH_SCORE_DOWN ,MachineButtonKey.BtnCreditDown},
        { (ulong)SBOX_SWITCH.SWITCH_SET ,MachineButtonKey.BtnConsole},  // 进入 或 退出 后台
       //##?? { (ulong)SBOX_IDEA_COIN_PUSH_KEYVALUE.SWITCH_DOOR_SWITCH ,MachineButtonKey.BtnDoor}
        { (ulong)SBOX_SWITCH.SWITCH_UP ,MachineButtonKey.BtnUp},
        { (ulong)SBOX_SWITCH.SWITCH_DOWN ,MachineButtonKey.BtnDown},
        { (ulong)SBOX_SWITCH.SWITCH_SWITCH ,MachineButtonKey.BtnSwitch},  // 雨刷 
    };

    private void OnEnable()
    {
        AddNetButtonHandle();
        //AddNetCmdHandle();

        EventCenter.Instance.AddEventListener<EventData>(MachineCustomButton.MACHINE_CUSTOM_BUTTON_FOCUS_EVENT, OnEventMachineCustomButton);

    }



    private void OnDisable()
    {
        RemoveNetButtonHandle();
        //RemoveNetCmdHandle();

        EventCenter.Instance.RemoveEventListener<EventData>(MachineCustomButton.MACHINE_CUSTOM_BUTTON_FOCUS_EVENT, OnEventMachineCustomButton);

    }
    

    private void OnEventMachineCustomButton(EventData evt)
    {
        curBtnInfo = (MachineCustomButton)evt.value;

        /*
        if (isInit == false)
        {
            LightAllOn();
            yield return new WaitForSecondsRealtime(2f);
            LightAllOff();
            isInit = true;
            yield return new WaitForSecondsRealtime(2f);
        }*/
    }

    
    
    void Start()
    {
        
    }


   
    
#region 按钮检查
    void Update()
    {
        if (!ApplicationSettings.Instance.isMachine) return;

        if (  SBoxSandbox.SwitchInState() !=0)
            GetPressedButtons(SBoxSandbox.SwitchInState());

        if (btnStartTimeInfos.Count>0)
        {
            int i = btnStartTimeInfos.Count;
            while (--i >= 0)
            {
                var kv = btnStartTimeInfos.ElementAt(i);
                if (Time.unscaledTime - kv.Value > 0.06f)
                {
                    btnStartTimeInfos.Remove(kv.Key);
                    OnKeyUp((ulong)kv.Key);
                }
            }
        }
    }

    private Dictionary<ulong, float> btnStartTimeInfos 
        = new Dictionary<ulong, float>();
    
    public  void GetPressedButtons(ulong buttonValue)
    {            
        DebugUtils.Log($" IO值: {buttonValue}");

        Type t = typeof(SBOX_SWITCH);
        var fields = t.GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            // 获取字段的值
            ulong button = (ulong)fieldInfo.GetValue(null);
            // 检查该位是否被置位（按位与运算结果非0表示置位）
            if ((buttonValue & button) != 0)
            {
                DebugUtils.Log($" 按键按下 {fieldInfo.Name} (值: 0x{button:X}) 被置位");
                if (!btnStartTimeInfos.ContainsKey(button))
                {
                    btnStartTimeInfos.Add(button, Time.unscaledTime);
                    // 【btn  down】
                    OnKeyDown((ulong)button);
                }
                else
                {
                    btnStartTimeInfos[button] = Time.unscaledTime;
                }
            }
        }


        /*
        // 遍历枚举的所有值
        foreach (SBOX_IDEA_COIN_PUSH_KEYVALUE button in Enum.GetValues(typeof(SBOX_IDEA_COIN_PUSH_KEYVALUE)))
        {
            // 检查当前按钮是否被按下
            if ((buttonValue & (int)button) != 0)
            {
                DebugUtils.Log($"按下：{Enum.GetName(typeof(SBOX_IDEA_COIN_PUSH_KEYVALUE),button)} ：{button}");
                if (!btnStartTimeInfos.ContainsKey(button))
                {
                    btnStartTimeInfos.Add(button, Time.unscaledTime);
                    // 【btn  down】
                    OnKeyDown((ulong)button);
                }else
                {
                    btnStartTimeInfos[button] = Time.unscaledTime;
                }
            }
        }*/
    }

    private void OnKeyUp(ulong value)
    {
        if(keyMap.ContainsKey(value))
            OnKeyUp(keyMap[value]);
    } 
    private void OnKeyDown(ulong value)
    {
        if(keyMap.ContainsKey(value))
            OnKeyDown(keyMap[value]);
    }     
    
#endregion




#region  按钮长按逻辑

#endregion

    public void OnKeyDown(MachineButtonKey value)
    {
        
        string keyName = Enum.GetName(typeof(MachineButtonKey), value);
        //DebugUtils.LogWarning($"【machine】KeyDown;  Key Name = {keyName};");
        
        
        if (!longClickTime.ContainsKey(value))
            longClickTime.Add(value, Time.unscaledTime);
        else
            longClickTime[value] = Time.unscaledTime;

        if (IsSysPriority(value))
        {
            switch (value)
            {
                case MachineButtonKey.BtnConsole:
                    {
                      //  MachineDeviceCommonBiz.Instance.OpenConsole();
                    }
                    return;
            }
            return;
        }

        if (curBtnInfo == null || !curBtnInfo.isPriority)
            switch (value)
            {
                case MachineButtonKey.BtnDoor:
                    return;
                case MachineButtonKey.BtnConsole:
                    return;
                case MachineButtonKey.BtnCreditUp:
                    {
                        if(coCreditUpLongClick != null) StopCoroutine(coCreditUpLongClick);
                        coCreditUpLongClick = StartCoroutine(DoCreditUpLongClick());

                        //DoCo(COR_CREDIT_UP_LONG_CLICK, DoCreditUpLongClick());
                    }
                    return;
                case MachineButtonKey.BtnCreditDown:
                    {
                        if (coCreditDownLongClick != null) StopCoroutine(coCreditDownLongClick);
                        coCreditDownLongClick = StartCoroutine(DoCreditDownLongClick());

                     //DoCo(COR_CREDIT_DOWN_LONG_CLICK, DoCreditDownLongClick());
                     }
                    return;
                case MachineButtonKey.BtnTicketOut:
                    EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                   new EventData<MachineButtonInfo>(
                       curBtnInfo.mark,
                       new MachineButtonInfo()
                       {
                           isUp = false,
                           btnKey = value, //$"{keyName}_Down",
                       }
                   ));
                    return;

            }

        if (curBtnInfo != null)// && curBtnInfo.isShowBtn)
        {
            /*
            if (curBtnInfo.btnType == MachineButtonType.Light)
            {
                List<MachineButtonKey> lst = GetLightBtnLst();
                if (lst.Contains(value))
                {
                    EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                        new EventData<MachineButtonInfo>(
                            curBtnInfo.mark,
                            new MachineButtonInfo()
                            {
                                isUp = false,
                                btnKey = MachineButtonKey.BtnLight,
                                value = lst.IndexOf(value),
                            }
                        ));
                }
            }
            else*/
            {
                EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                    new EventData<MachineButtonInfo>(
                        curBtnInfo.mark,
                        new MachineButtonInfo()
                        {
                            isUp = false,
                            btnKey = value, //$"{keyName}_Down",
                        }
                    ));
            }
            
        }
    }


    public void OnKeyUp(MachineButtonKey value)
    {
        string keyName = Enum.GetName(typeof(MachineButtonKey), value);
        DebugUtils.LogWarning($"【machine】KeyUp;  Key Name = {keyName};");
        
        
        
        if (IsSysPriority(value))
        {
            switch (value)
            {
                case MachineButtonKey.BtnConsole:
                    {
                        MachineDeviceCommonBiz.Instance.OpenConsole();
                    }
                    return;
            }
            return;
        }
        
        if (curBtnInfo == null || !curBtnInfo.isPriority)
            switch (value)
            {

                case MachineButtonKey.BtnCreditUp:
                    {

                        if (coCreditUpLongClick != null) 
                            StopCoroutine(coCreditUpLongClick);
                        coCreditUpLongClick = null;

                        bool isLongClick = Time.unscaledTime - longClickTime[MachineButtonKey.BtnCreditUp] > 5;
                        if (!isLongClick)
                        {
                            DeviceCreditUpDown.Instance.CreditUp();
                        }

                    }                        
                    return;
                case MachineButtonKey.BtnCreditDown:
                    {

                        if (coCreditDownLongClick != null) 
                            StopCoroutine(coCreditDownLongClick);
                        coCreditDownLongClick = null;

                        bool isLongClick = Time.unscaledTime - longClickTime[MachineButtonKey.BtnCreditDown] > 5;
                        if (!isLongClick)
                        {
                            DeviceCreditUpDown.Instance.CreditDown();
                        }
                        
                    }
                    return;
              case MachineButtonKey.BtnTicketOut:
                    {
                        DeviceCoinOut.Instance.DoCoinOut();  
                    }
                    return;
            }
        
        if (curBtnInfo != null)/// && curBtnInfo.isShowBtn)
        {
            /*
            if (curBtnInfo.btnType == MachineButtonType.Light)
            {
                List<MachineButtonKey> lst = GetLightBtnLst();
                if (lst.Contains(value))
                {
                    EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                        new EventData<MachineButtonInfo>(
                            curBtnInfo.mark,
                            new MachineButtonInfo()
                            {
                                isUp = true,
                                btnKey = MachineButtonKey.BtnLight,
                                value = lst.IndexOf(value),
                            }
                        ));

                }
            }
            else*/
            {
                EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                    new EventData<MachineButtonInfo>(
                        curBtnInfo.mark,
                        new MachineButtonInfo()
                        {
                            isUp = true,
                            btnKey = value, // $"{keyName}_Up",
                        }
                ));
            }
        }
    }


   
    
    Dictionary<MachineButtonKey, float> longClickTime = new Dictionary<MachineButtonKey, float>();
    MachineCustomButton curBtnInfo;

    public const string MACHINE_BUTTON_EVENT = "MACHINE_BUTTON_EVENT";
    
    bool IsSysPriority(MachineButtonKey value)
    {
        if (value == MachineButtonKey.BtnConsole && PageManager.Instance.IndexOf(PageName.ConsolePusher01PageConsoleMain) == -1)
        {
            return true;
        }

        return false;


    }




    #region 长按上下分


    Coroutine coCreditUpLongClick, coCreditDownLongClick;

    /// <summary>
    /// 长按上分
    /// </summary>
    /// <returns></returns>
    IEnumerator DoCreditUpLongClick()
    {

        yield return new WaitForSecondsRealtime(3f);

        while (true)
        {
            DeviceCreditUpDown.Instance.CreditUp(true);
            yield return new WaitForSecondsRealtime(0.7f);
        }
    }

    /// <summary>
    /// 长按下分清零
    /// </summary>
    /// <returns></returns>
    IEnumerator DoCreditDownLongClick()
    {

        yield return new WaitForSecondsRealtime(3f);

        DeviceCreditUpDown.Instance.CreditAllDown();
    }




    #endregion


















#if false //网络命令

    const string MARK_NET_CMD_MACHINE_DEVICE = "MARK_NET_CMD_MACHINE_DEVICE";

    void AddNetCmdHandle()
    {
        NetCmdManager.Instance.AddHandles(new NetCmdHandle()
        {
            cmdName = NetCmdManager.CMD_COIN_IN,
            mark = MARK_NET_CMD_MACHINE_DEVICE,
            onInvoke = OnNetCmdCoinIn,
        });

        NetCmdManager.Instance.AddHandles(new NetCmdHandle()
        {
            cmdName = NetCmdManager.CMD_SCORE_DOWN,
            mark = MARK_NET_CMD_MACHINE_DEVICE,
            onInvoke = OnNetCmdScoreDown,
        });

        NetCmdManager.Instance.AddHandles(new NetCmdHandle()
        {
            cmdName = NetCmdManager.CMD_SCORE_UP,
            mark = MARK_NET_CMD_MACHINE_DEVICE,
            onInvoke = OnNetCmdScoreUp,
        });

    }

    void RemoveNetCmdHandle() => NetCmdManager.Instance.ReomveHandles(MARK_NET_CMD_MACHINE_DEVICE);

    void OnNetCmdScoreUp(NetCmdInfo info)
    {
    }


    void OnNetCmdScoreDown(NetCmdInfo info)
    {
    }

    void OnNetCmdCoinIn(NetCmdInfo info)
    {
        //## MachineDeviceCommonBiz.Instance.deviceCoinIn.DoCmdCoinIn((int)info.data, info.onCallback);
    }
#endif






    #region 网络远程按钮 - 直接转机台按钮
    const string MARK_NET_BTN_MACHINE_DEVICE = "MARK_NET_BTN_MACHINE_DEVICE";
    void AddNetButtonHandle()
    {
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnPayTable,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnPayTable,
        });
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnPrev,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnPrev,
        });
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnNext,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnNext,
        });
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnExit,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnExit,
        });
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnSpin,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnSpin,
        });

        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnBetUp,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnBetUp,
        });
     
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnBetDown,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnBetDown,
        });
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnBetMax,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnBetMax,
        });

        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnSwitch,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnSwitch,
        });

        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnTicketOut,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnTicketOut,
        });
        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnAuto,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnAuto,
        });


        NetButtonManager.Instance.AddHandles(new NetButtonHandle()
        {
            buttonName = NetButtonManager.BtnName.BtnWiper,
            mark = MARK_NET_BTN_MACHINE_DEVICE,
            onClick = OnNetBtnWiper,
        });
        //BtnWiper
    }

    void RemoveNetButtonHandle() => NetButtonManager.Instance.ReomveHandles(MARK_NET_BTN_MACHINE_DEVICE);




    void _OnNetBtnClick(NetButtonInfo info , MachineButtonKey mBtn, BtnName nBtn )
    {
        if (info.dataType != NetButtonManager.DATA_MACHINE_BUTTON_CONTROL) return;
        //if (PageManager.Instance.IndexOf(MainModel.Instance.contentMD.pageName) != 0) return;

        NetButtonManager.Instance.ShowUIAminButtonClick(() =>
        {
            OnKeyDown(mBtn);
        }, () => {
            OnKeyUp(mBtn);
        }, MARK_NET_BTN_MACHINE_DEVICE, nBtn);

        info.onCallback?.Invoke(true);
    }


    void OnNetBtnSpin(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnSpin, BtnName.BtnSpin);


    void OnNetBtnPayTable(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnPayTable, BtnName.BtnPayTable);

    void OnNetBtnNext(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnNext, BtnName.BtnNext);

    void OnNetBtnPrev(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnPrev, BtnName.BtnPrev);

    void OnNetBtnExit(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnExit, BtnName.BtnExit);

    void OnNetBtnBetUp(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnBetUp, BtnName.BtnBetUp);

    void OnNetBtnBetDown(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnBetDown, BtnName.BtnBetDown);

    void OnNetBtnBetMax(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnBetMax, BtnName.BtnBetMax);


    void OnNetBtnSwitch(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnSwitch, BtnName.BtnSwitch);


    void OnNetBtnTicketOut(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnTicketOut, BtnName.BtnTicketOut);

    
    void OnNetBtnWiper(NetButtonInfo info) => _OnNetBtnClick(info, MachineButtonKey.BtnSwitch, BtnName.BtnWiper);


    void OnNetBtnAuto(NetButtonInfo info)
    {
        if (info.dataType != NetButtonManager.DATA_MACHINE_BUTTON_CONTROL) return;
        //if (PageManager.Instance.IndexOf(MainModel.Instance.contentMD.pageName) != 0) return;

        NetButtonManager.Instance.ShowUIAminButtonLongClick(() =>
        {
            OnKeyDown(MachineButtonKey.BtnSpin);
        }, () => {
            OnKeyUp(MachineButtonKey.BtnSpin);
        }, MARK_NET_BTN_MACHINE_DEVICE, BtnName.BtnAuto);

        info.onCallback?.Invoke(true);
    }


    #endregion

}
