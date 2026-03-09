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

public class MachineDeviceController : MonoSingleton<MachineDeviceController>
{
    // 添加后台模式标志
    private bool isInConsoleMode = false;

    // 添加后台模式标志
    private bool isInTicketOut = false;

    public readonly Dictionary<ulong, MachineButtonKey> keyMap = new Dictionary<ulong, MachineButtonKey>()
    {
        //游戏按钮：
        { (ulong)SBOX_SWITCH.SWITCH_ENTER ,MachineButtonKey.BtnSpin},  // 开始玩 或 确认
        { (ulong)SBOX_SWITCH.SWITCH_RULE,MachineButtonKey.BtnPayTable},

        //管理按钮：
        { (ulong)SBOX_SWITCH.SWITCH_PAYOUT ,MachineButtonKey.BtnTicketOut},  // 下一页
        { (ulong)SBOX_SWITCH.SWITCH_SCORE_UP ,MachineButtonKey.BtnCreditUp},
        { (ulong)SBOX_SWITCH.SWITCH_SCORE_DOWN ,MachineButtonKey.BtnCreditDown},
        { (ulong)SBOX_SWITCH.SWITCH_SET ,MachineButtonKey.BtnConsole},  // 进入 或 退出 后台
        { (ulong)SBOX_SWITCH.SWITCH_UP ,MachineButtonKey.BtnUp},
        { (ulong)SBOX_SWITCH.SWITCH_DOWN ,MachineButtonKey.BtnDown},
        { (ulong)SBOX_SWITCH.SWITCH_SWITCH ,MachineButtonKey.BtnSwitch},  // 雨刷 
    };

    private void OnEnable()
    {
        AddNetButtonHandle();

        EventCenter.Instance.AddEventListener<EventData>(MachineCustomButton.MACHINE_CUSTOM_BUTTON_FOCUS_EVENT, OnEventMachineCustomButton);
    }

    private void OnDisable()
    {
        RemoveNetButtonHandle();

        EventCenter.Instance.RemoveEventListener<EventData>(MachineCustomButton.MACHINE_CUSTOM_BUTTON_FOCUS_EVENT, OnEventMachineCustomButton);
    }

    private void OnEventMachineCustomButton(EventData evt)
    {
        curBtnInfo = (MachineCustomButton)evt.value;
    }

    void Start()
    {

    }

    #region 按钮检查
    void Update()
    {
        if (!ApplicationSettings.Instance.isMachine) return;

        if (SBoxSandbox.SwitchInState() != 0)
            GetPressedButtons(SBoxSandbox.SwitchInState());

        if (btnStartTimeInfos.Count > 0)
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

    private Dictionary<ulong, float> btnStartTimeInfos = new Dictionary<ulong, float>();

    public void GetPressedButtons(ulong buttonValue)
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
    }

    private void OnKeyUp(ulong value)
    {
        if (keyMap.ContainsKey(value))
            OnKeyUp(keyMap[value]);
    }

    private void OnKeyDown(ulong value)
    {
        if (keyMap.ContainsKey(value))
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
                        // 如果已经在后台模式，按下后台按钮准备退出
                        if (isInConsoleMode)
                        {
                            DebugUtils.Log("【machine】准备退出后台模式");
                        }
                        else
                        {
                            // 进入后台时，设置标志位
                            isInConsoleMode = true;
                            //MachineDeviceCommonBiz.Instance.OpenConsole();
                        }
                    }
                    break;
                case MachineButtonKey.BtnTicketOut:
                    {
                        // 如果已经在退票
                        if (isInTicketOut)
                        {
                            DebugUtils.Log("【machine】退票中");
                        }
                        else
                        {
                            if (!isInConsoleMode)
                            {
                                isInTicketOut = true;
                            }
                        }

                        EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT, new EventData<MachineButtonInfo>
                      (
                      curBtnInfo.mark,
                      new MachineButtonInfo()
                      {
                          isUp = false,
                          btnKey = value,
                      }
                  ));
                    }
                    break;
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
                        if (coCreditUpLongClick != null) StopCoroutine(coCreditUpLongClick);
                        coCreditUpLongClick = StartCoroutine(DoCreditUpLongClick());
                    }
                    return;
                case MachineButtonKey.BtnCreditDown:
                    {
                        if (coCreditDownLongClick != null) StopCoroutine(coCreditDownLongClick);
                        coCreditDownLongClick = StartCoroutine(DoCreditDownLongClick());
                    }
                    return;
                case MachineButtonKey.BtnTicketOut:
                   
                    return;
            }

        if (curBtnInfo != null)
        {
            EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                new EventData<MachineButtonInfo>(
                    curBtnInfo.mark,
                    new MachineButtonInfo()
                    {
                        isUp = false,
                        btnKey = value,
                    }
                ));
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
                        // 如果已经在后台模式，且按下的是后台按钮，则退出后台
                        if (isInConsoleMode)
                        {
                            //isInConsoleMode = false;
                            MachineDeviceCommonBiz.Instance.OpenConsole();
                        }
                    }
                    break;
                case MachineButtonKey.BtnTicketOut:
                    {
                        if(isInTicketOut&&!isInConsoleMode)
                        {
                            DeviceCoinOut.Instance.DoCoinOut();
                        }
                    }
                    break;
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
                       
                    }
                    return;

                case MachineButtonKey.BtnSpin:
                    {
                        //关闭弹窗
                        if (CommonPopupHandler.Instance.iPopup.IsOpen())
                        {
                            CommonPopupHandler.Instance.ClosePopup();
                            return;
                        }
                      
                    }
                    break;
            }

        if (curBtnInfo != null)
        {
            EventCenter.Instance.EventTrigger<EventData>(MACHINE_BUTTON_EVENT,
                new EventData<MachineButtonInfo>(
                    curBtnInfo.mark,
                    new MachineButtonInfo()
                    {
                        isUp = true,
                        btnKey = value,
                    }
            ));
        }
    }

    Dictionary<MachineButtonKey, float> longClickTime = new Dictionary<MachineButtonKey, float>();
    MachineCustomButton curBtnInfo;

    public const string MACHINE_BUTTON_EVENT = "MACHINE_BUTTON_EVENT";

    bool IsSysPriority(MachineButtonKey value)
    {
        // 如果已经在后台模式，禁止所有按钮（除了后台按钮本身）
        if (isInConsoleMode)
        {
            // 在后台模式下，只允许后台按钮操作（用于退出）
            // 其他按钮都不响应
            if (value == MachineButtonKey.BtnConsole)
            {
                DebugUtils.Log($"【machine】在后台模式下，只允许后台按钮操作");
                return true; // 允许后台按钮处理退出
            }
            else
            {
                // 其他按钮都不响应
                DebugUtils.Log($"【machine】在后台模式，按钮 {value} 被禁止");
                return true; // 返回true表示由系统处理（实际不做任何操作）
            }
        }

        if (isInTicketOut)
        {
           
            // 其他按钮都不响应
            if (value == MachineButtonKey.BtnTicketOut)
            {
                DebugUtils.Log($"【machine】在退票模式下，只允许退票按钮操作");
                return true; 
            }
            else
            {
                // 其他按钮都不响应
                DebugUtils.Log($"【machine】在退票模式，按钮 {value} 被禁止");
                return true; // 返回true表示由系统处理（实际不做任何操作）
            }
        }

        // 正常模式下的系统优先级判断
        if (value == MachineButtonKey.BtnConsole &&
            PageManager.Instance.IndexOf(PageName.ConsolePageConsoleMain) == -1)
        {
            DebugUtils.Log($"【machine】允许进入后台");
            return true; // 允许进入后台
        }

        // 正常模式下的系统优先级判断
        if (value == MachineButtonKey.BtnTicketOut &&
            PageManager.Instance.IndexOf(PageName.ConsolePopupConsoleMask) == -1)
        {
            DebugUtils.Log($"【machine】允许退票");
            return true; // 允许退票
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
    }

    void RemoveNetButtonHandle() => NetButtonManager.Instance.ReomveHandles(MARK_NET_BTN_MACHINE_DEVICE);

    void _OnNetBtnClick(NetButtonInfo info, MachineButtonKey mBtn, BtnName nBtn)
    {
        if (info.dataType != NetButtonManager.DATA_MACHINE_BUTTON_CONTROL) return;

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

        NetButtonManager.Instance.ShowUIAminButtonLongClick(() =>
        {
            OnKeyDown(MachineButtonKey.BtnSpin);
        }, () => {
            OnKeyUp(MachineButtonKey.BtnSpin);
        }, MARK_NET_BTN_MACHINE_DEVICE, BtnName.BtnAuto);

        info.onCallback?.Invoke(true);
    }

    #endregion

    /// <summary>
    /// 退出后台模式
    /// </summary>
    public void ExitConsoleMode()
    {
        isInConsoleMode = false;
        DebugUtils.Log("【machine】退出后台模式");
    }



    /// <summary>
    /// 获取当前是否在后台模式
    /// </summary>
    public bool IsInConsoleMode()
    {
        return isInConsoleMode;
    }

    /// <summary>
    /// 退出退票
    /// </summary>
    public void ExitTicketOut()
    {
        isInTicketOut = false;
        DebugUtils.Log("退票完成");
    }
}