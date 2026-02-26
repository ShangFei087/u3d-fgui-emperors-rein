using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace ConsoleCoinPusher01
{
    public class PageConsoleSettings : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleSettings";
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
            /*
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
            (GameObject clone) =>
            {
                callback();
            });
            */
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

        
  
        InfoBaseController baseCtrl = new InfoBaseController();

        TabSettings01 tabSettings01 =new TabSettings01();
        TabSettings02 tabSettings02=new TabSettings02();
        TabSettings03 tabSettings03=new TabSettings03();


        GList glstFooter;

        int tabIndex;
        List<ConsoleMenuBase> tabs = new List<ConsoleMenuBase>();

        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("设置");
            base.OnOpen(name, data);
            tabIndex = 0;
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }

        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());


            glstFooter = this.contentPane.GetChild("footer").asList;

            tabSettings01.InitParam(this.contentPane.GetChild("tab1").asCom, OnClickPrev, OnClickNext, OnClickExitCallback);
            tabSettings02.InitParam(this.contentPane.GetChild("tab2").asCom, OnClickPrev, OnClickNext, OnClickExitCallback);
            tabSettings03.InitParam(this.contentPane.GetChild("tab3").asCom, OnClickPrev, OnClickNext, OnClickExitCallback);

            tabs.Clear();
            tabs.Add(tabSettings01);
            tabs.Add(tabSettings02);
            //tabs.Add(tabSettings03);
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

        void OnClickPrev()
        {
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
