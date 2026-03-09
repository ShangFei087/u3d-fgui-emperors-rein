using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TabHardwareButtonTest 
{
    GComponent goOwnerTab;

    GButton TicketOutBtn, SpinBtn, ConsoleBtn, UpBtn, DownBtn, DoorBtn;
    MachineCustomButton curBtnInfo;
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
    Dictionary<MachineButtonKey, float> longClickTime = new Dictionary<MachineButtonKey, float>();
    public void InitParam(GComponent go, string tabName)
    {
        goOwnerTab = go;

        TicketOutBtn = go.GetChild("TicketOutBtn").asButton;
        SpinBtn = go.GetChild("SpinBtn").asButton;
        ConsoleBtn = go.GetChild("ConsoleBtn").asButton;
        UpBtn = go.GetChild("UpBtn").asButton;
        DownBtn = go.GetChild("DownBtn").asButton;
        DoorBtn = go.GetChild("DoorBtn").asButton;
    }
    /// <summary>
    /// 检查硬件按钮状态
    /// </summary>
    public void CheckButtons()
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

    public void OnKeyDown(MachineButtonKey value)
    {
        string keyName = Enum.GetName(typeof(MachineButtonKey), value);
        DebugUtils.LogWarning($"【machine】KeyDown;  Key Name = {keyName};");

        if (!longClickTime.ContainsKey(value))
            longClickTime.Add(value, Time.unscaledTime);
        else
            longClickTime[value] = Time.unscaledTime;

        if (curBtnInfo == null || !curBtnInfo.isPriority)
            switch (value)
            {
                case MachineButtonKey.BtnDoor:
                    {
                        DebugUtils.Log("点击了BtnDoor");
                        //模拟点击
                        DoorBtn.FireClick(true);
                    }
                    return;
                case MachineButtonKey.BtnConsole:
                    {
                        DebugUtils.Log("点击了BtnConsole");
                        //模拟点击
                        ConsoleBtn.FireClick(true);
                    }
                    return;
                case MachineButtonKey.BtnCreditUp:
                    {
                        DebugUtils.Log("点击了BtnCreditUp");
                        //模拟点击
                        UpBtn.FireClick(true);
                    }
                    return;
                case MachineButtonKey.BtnCreditDown:
                    {
                        DebugUtils.Log("点击了BtnCreditDown");
                        //模拟点击
                        DownBtn.FireClick(true);
                    }
                    return;
                case MachineButtonKey.BtnTicketOut:
                    {
                        DebugUtils.Log("点击了BtnTicketOut");
                        //模拟点击
                        TicketOutBtn.FireClick(true);
                    }
                    return;
                case MachineButtonKey.BtnSpin:
                    {
                        DebugUtils.Log("点击了BtnSpin");
                        //模拟点击
                        SpinBtn.FireClick(true);
                    }
                    return;
            }
    }

    public void OnKeyUp(MachineButtonKey value)
    {
        string keyName = Enum.GetName(typeof(MachineButtonKey), value);
        DebugUtils.LogWarning($"【machine】KeyUp;  Key Name = {keyName};");

        if (curBtnInfo == null || !curBtnInfo.isPriority)
            switch (value)
            {
                case MachineButtonKey.BtnCreditUp:
                    {
                       
                    }
                    return;
                case MachineButtonKey.BtnCreditDown:
                    {
                        
                    }
                    return;
                case MachineButtonKey.BtnTicketOut:
                    {

                    }
                    return;

                case MachineButtonKey.BtnSpin:
                    {

                    }
                    return;
            }
    }

   
}
