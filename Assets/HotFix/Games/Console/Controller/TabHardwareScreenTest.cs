using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class TabHardwareScreenTest 
{
    GComponent owner;
    GButton btnScreeColor;

    protected  void OnInit()
    {
        
    }
    public void InitParam(GComponent go)
    {
        owner = go;
        btnScreeColor = owner.GetChild("ScreenTest").asCom.GetChild("value").asButton;
        btnScreeColor.onClick.Clear();
        btnScreeColor.onClick.Add(OnClickScreeColor);
    }

    public void OnClickScreeColor()
    {
        PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleScreenColor);
    }
}
