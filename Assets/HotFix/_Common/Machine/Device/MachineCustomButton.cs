using System.Collections.Generic;
using System;
using UnityEngine;
public enum MachineButtonType
{
    /// <summary> 亮灯按钮 </summary>
    Regular,
    /// <summary> 闪灯按钮 </summary>
    Light,
}

[System.Serializable]
public class MachineCustomButton
{
    
    public MachineCustomButton()
    {
    }

    /// <summary> 获得机台按钮聚焦 </summary>
    public const string MACHINE_CUSTOM_BUTTON_FOCUS_EVENT = "MACHINE_CUSTOM_BUTTON_FOCUS_EVENT";

    /// <summary> 按钮类型 </summary>
    public MachineButtonType btnType = MachineButtonType.Regular;

    /// <summary> 亮灯按钮列表</summary>
    public List<MachineButtonKey> btnShowLst = new List<MachineButtonKey>()
    {
        MachineButtonKey.BtnPrev,
        MachineButtonKey.BtnNext,
        MachineButtonKey.BtnSpin,
    };

    /// <summary> 是否显示按钮灯 </summary>
    // public bool isShowBtn = true;

    /// <summary> 灯的个数 </summary>
    public int numlightBtn = 0;

    /// <summary> 标记 </summary>
    string _mark = null;
    public string mark
    {
        get{
            if (string.IsNullOrEmpty(_mark))
                _mark = Guid.NewGuid().ToString();
            return _mark;
        }
        set => _mark = value;
    }

    /// <summary> 是否优先处理 </summary>
    public bool isPriority = false;
}