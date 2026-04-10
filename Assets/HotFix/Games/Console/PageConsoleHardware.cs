using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ConsoleSlot01
{
    public class PageConsoleHardware : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleHardware";
        public override PageType pageType => PageType.Overlay;
        private TimerCallback _updateCallback;
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


            callback();
            _updateCallback = OnUpdate;
            Timers.inst.AddUpdate(_updateCallback);
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            // 添加事件监听
            InitParam();
        }

        // 每帧调用的更新方法
        private void OnUpdate(object param)
        {
            tabBtnTestCtrl.CheckButtons();
        }


        public override void OnClose(EventData data = null)
        {

            // 删除事件监听
            // 移除 Update 回调
            if (_updateCallback != null)
            {
                Timers.inst.Remove(_updateCallback);
                _updateCallback = null;
            }
            base.OnClose(data);
        }


        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose;
        TabHardwareButtonTest tabBtnTestCtrl = new TabHardwareButtonTest();
        TabHardwareScreenTest tabScreenTestCtrl = new TabHardwareScreenTest();
        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            // btnClose =  this.contentPane.GetChild("btnExit").asButton;
            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(null);
            });

            tabBtnTestCtrl.InitParam(this.contentPane.GetChild("pages").asCom.GetChildAt(0).asCom);
            tabScreenTestCtrl.InitParam(this.contentPane.GetChild("pages").asCom.GetChildAt(1).asCom);
        }
    }
}
