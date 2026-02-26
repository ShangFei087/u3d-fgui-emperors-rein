using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameMaker;


namespace ConsoleCoinPusher01
{
    public class PageConsoleAdmin : MachinePageBase
    {
        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleAdmin";
        public override PageType pageType => PageType.Overlay;


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


            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        tabs[tabIndex].OnClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        tabs[tabIndex].OnClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                        tabs[tabIndex].OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        tabs[tabIndex].OnClickNext();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        tabs[tabIndex].OnClickPrev();
                    },
                    [MachineButtonKey.BtnConsole] = (info) =>
                    {
                        CloseSelf(null);
                    }
                }
            };
        }


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("超级管理员");
            base.OnOpen(name, data);
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }
        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }


        InfoBaseController baseCtrl = new InfoBaseController();

        

        TabAdmin01 tab01 = new TabAdmin01();
        TabAdmin0002 tab02 = new TabAdmin0002();

        int tabIndex = 0;
        List<ConsoleMenuBase> tabs = new List<ConsoleMenuBase>();

        GList glstFooter;
        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            glstFooter = this.contentPane.GetChild("footer").asList;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());


            tab01.InitParam(this.contentPane.GetChild("tab1").asCom, OnClickPrev, OnClickNext, OnClickExitCallback);
            tab02.InitParam(this.contentPane.GetChild("tab2").asCom, OnClickPrev, OnClickNext, OnClickExitCallback);


            tabs.Clear();
            tabs.Add(tab01);
            tabs.Add(tab02);

            ChangeTab();

            glstFooter.numItems = tabs.Count;
            glstFooter.visible = tabs.Count > 1;

        }



        void ChangeTab()
        {
            foreach (ConsoleMenuBase tab in tabs)
            {
                tab.goOwnerMenu.visible = false;
            }
            tabs[tabIndex].goOwnerMenu.visible = true;


            GObject[] gChds = glstFooter.GetChildren();

            foreach (GObject chd in gChds)
            {
                chd.asImage.color = Color.gray;
            }
            gChds[tabIndex].asImage.color = Color.white;
        }

        void OnClickPrev() {
            if (--tabIndex < 0)
                tabIndex = 0;
            ChangeTab();
        } 

        void OnClickNext()
        {
            if (++tabIndex >= tabs.Count)
                tabIndex = tabs.Count - 1;
            ChangeTab();
        }
        void OnClickExitCallback() => CloseSelf(null);
        
    }

}