using FairyGUI;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameMaker;
using SBoxApi;
using Newtonsoft.Json;

namespace ConsoleCoinPusher01
{
    public class PopupConsoleRecord : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PopupConsoleRecord";
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

                        OnClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        OnClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                        OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        OnClickNext();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        OnClickPrev();
                    },
                    [MachineButtonKey.BtnConsole] = (info) =>
                    {
                        CloseSelf(null);
                    }
                }
            };

        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            // 释放重复创建的UI
            InitParam();
        }
        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("详细");
            base.OnOpen(name, data);
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }

        


        GList glst;

        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu;

        GRichTextField info;


        Dictionary<int, string> menuMap;



        int curIndexMenuItem = 0;

        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());




            goMenu = this.contentPane.GetChild("items").asCom;

            info = this.contentPane.GetChild("info").asRichTextField;

            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                if (argDic.ContainsKey("value"))
                {
                    info.text = (string)argDic["value"];
                }
            }


            AddClickEvent();

            Clear(false);
        }


        void Clear(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            curIndexMenuItem = 0;
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                GComponent goItem = goMenu.GetChildAt(i).asCom;
                if (isClearAllArrow)
                    goItem.GetChild("icon").visible = false;
                else
                    goItem.GetChild("icon").visible = i == curIndexMenuItem;

                menuMap.Add(i, goItem.name);
            }

        }


        void SetAllow()
        {
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                goMenu.GetChildAt(i).asCom.GetChild("icon").visible = i == curIndexMenuItem;
            }
        }
        void OnClickNext()
        {
            if (++curIndexMenuItem >= goMenu.numChildren)
                curIndexMenuItem = 0;
            SetAllow();
        }

        void OnClickPrev()
        {
            if (--curIndexMenuItem < 0)
                curIndexMenuItem = goMenu.numChildren - 1;
            SetAllow();
        }

        void OnClickConfirm()
        {
            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem])
                {
                    case "exit":
                        {
                            CloseSelf(null);
                        }
                        return;
                }
            }
        }


        void AddClickEvent()
        {
            for (int i = 0; i < contentPane.GetChild("items").asCom.numChildren; i++)
            {
                int index = i;
                contentPane.GetChild("items").asCom.GetChildAt(index).onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }




    }



}