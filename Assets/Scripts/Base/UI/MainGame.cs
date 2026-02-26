using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class MainGame
{
    public void Start()
    {
        // 初始化UI
        UIPackage.AddPackage("UI/Basics");

        // 初始化窗口管理器
        //WindowManager.Instance.Initialize(GRoot.inst);

        // 打开主窗口
        var mainWindow = new MainWindow();
        //WindowManager.Instance.Push(mainWindow);
    }
}

/// <summary>
/// 主窗口示例
/// </summary>
public class MainWindow : PageBase
{
    protected override void OnInit()
    {
        base.OnInit();
        this.contentPane = UIPackage.CreateObject("Basics", "Window").asCom;

        // 绑定按钮事件
        //var btnOpenSub = this.contentPane.GetChild("btnOpenSub").asButton;
        //btnOpenSub.onClick.Add(() => NavigateTo(new SubWindow()));
    }
}

/// <summary>
/// 子窗口示例
/// </summary>
public class SubWindow : PageBase
{
    protected override void OnInit()
    {
        base.OnInit();
        this.contentPane = UIPackage.CreateObject("Basics", "SubWindow").asCom;

        // 绑定关闭按钮
        //var btnClose = this.contentPane.GetChild("btnClose").asButton;
        //btnClose.onClick.Add(CloseSelf);
    }
}