using FairyGUI;
using System.Collections.Generic;
using System;
using GameMaker;
using SBoxApi;


namespace ConsoleCoinPusher01
{
    public class PageConsoleJpRecord : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleJpRecord";
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
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                            OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {

                            OnClickNext();
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
            base.OnOpen(name, data);
            InitParam();
            PageTitleManager.Instance.AddPageNode("彩金记录");
            SBoxIdea.IntoConsolePage(1);
        }


        public override void OnClose(EventData data = null)
        {
            SBoxIdea.IntoConsolePage(0);
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }



        

        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu, goRecord;
        GLabel labTip;




        Dictionary<int, string> menuMap;



        int curIndexMenuItem = 0;




        public override void InitParam()
        {

            //DebugUtils.LogError("i am here PageConsoleMain !!!!! ");


            if (!isInit) return;


            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());

            goRecord = this.contentPane.GetChild("record").asCom;
            goMenu = this.contentPane.GetChild("items").asCom;


            Clear(false);

            AddClickEvent();
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
                    case "data":
                        {
                            ChangeDate();
                        }
                        return;
                    case "next":
                        {
                            NextPage();
                        }
                        return;
                    case "prev":
                        {
                            PervPage();
                        }
                        return;
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
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                int index = i;
                goMenu.GetChildAt(index).onClick.Clear();
                goMenu.GetChildAt(index).onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }


        int curPageIndex;
        void ChangeDate()
        {

        }

        void SetDate(int index)
        {

        }

        void NextPage()
        {

        }

        void PervPage()
        {

        }

    }



}