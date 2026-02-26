using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CommonPopupType
{
    /// <summary>
    /// 无显示
    /// </summary>
    None = 0,


    /// <summary>
    /// 只显示text文本
    /// </summary>
    TextOnly,

    /// <summary>
    /// 显示text文本和btn1按钮
    /// </summary>
    OK,


    /// <summary>
    /// 显示title标题，text文本和btn1按钮
    /// </summary>
    OkWithTitle,

    /// <summary>
    /// 显示text文本和btn1按钮、btn2按钮
    /// </summary>
    YesNo,


    /// <summary>
    /// 显示text文本和btn1按钮
    /// </summary>
    /// <remarks>
    /// 点击按钮会回到登录界面
    /// </remarks>
    SystemReset,


    SystemTextOnly,
}

public class CommonPopupInfo
{
    public CommonPopupType type;

    /// <summary> popup title </summary>
    public string title;

    /// <summary> popup content </summary>
    public string text;

    /// <summary> button1 show text </summary>
    public string buttonText1;

    /// <summary> button2 show text </summary>
    public string buttonText2;

    /// <summary> is show button_close </summary>
    public bool isUseXButton = false;

    /// <summary> is close popup when click button1 </summary>
    public bool buttonAutoClose1 = true;

    /// <summary> is close popup when click button2 </summary>
    public bool buttonAutoClose2 = true;

    /// <summary> click button1 callback </summary>
    public UnityAction callback1;

    /// <summary> click button2 callback </summary>
    public UnityAction callback2;

    /// <summary> click button_close callback </summary>
    public UnityAction callbackX;

    public string mark = null;

    // public int autoCloseTimesS = -1;
}
