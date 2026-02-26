using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PageDemo : PageBase
{
    public const string pkgName = "Console";
    public const string resName = "PageDemo";

    protected override void OnInit()
    {
        
        base.OnInit();

        int count = 1;

        Action callback = () =>
        {
            if (--count == 0)
            {
                isInit = true;
                InitParam();
            }
        };

        // 异步加载资源

        ResourceManager02.Instance.LoadAsset<GameObject>(
        "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
        (GameObject clone) =>
        {
            callback();
        });

    }

    public override void OnOpen(PageName name, EventData data)
    {
        base.OnOpen(name, data);

        // 添加事件监听

        InitParam();
    }


    public override void OnClose(EventData data = null)
    {

        // 删除事件监听

        base.OnClose(data);
    }


    // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

    GButton btnClose;

    

    public override void InitParam()
    {

        if (!isInit) return;

        if (!isOpen) return;

        // btnClose =  this.contentPane.GetChild("btnExit").asButton;
        btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
        btnClose.onClick.Clear();
        btnClose.onClick.Add(() => {
            //DebugUtils.Log("i am here 123");
            CloseSelf(null);
            //  CloseSelf(new EventData("Exit"));
        });


        /* 
        if (inParams != null)
        {   
            Dictionary<string, object> argDic = null;
            argDic = (Dictionary<string, object>)inParams.value;
            title = (string)argDic["title"];
            isPlaintext = (bool)argDic["isPlaintext"];
            if (argDic.ContainsKey("content"))
            {
                input = (string)argDic["content"];
            }
        }
       */
    }
}
