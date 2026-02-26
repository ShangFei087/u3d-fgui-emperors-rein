using FairyGUI;
using System.Collections.Generic;
using System;
using GameMaker;


namespace ConsoleCoinPusher01
{
    public class PageConsoleRecordChoose : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleRecordChoose";
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


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("记录");
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




        Dictionary<int, string> menuMap;



        int curIndexMenuItem = 0;



        GComponent goItemCoinRecord, goItemGameRecord,goItemJpRecord,goItemEventRecord,goItemErroRecord,goBusinessRecord;
        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());


        

            goMenu = this.contentPane.GetChild("items").asCom;

            GComponent BusinessRecord = goMenu.GetChild("businessRecord")?.asCom ?? null;
            if (BusinessRecord != null)
            {
                goMenu.RemoveChild(BusinessRecord);
                BusinessRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleRecordChoose-menu-businessRecord";
                BusinessRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(BusinessRecord);
                if (goBusinessRecord != null&& goBusinessRecord.displayObject.gameObject!=null)
                    goBusinessRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goBusinessRecord = BusinessRecord;
                goBusinessRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            GComponent coinRecord = goMenu.GetChild("coinRecord")?.asCom ?? null;
            if (coinRecord != null)
            {
                goMenu.RemoveChild(coinRecord);
                coinRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleRecordChoose-menu-coinRecord";
                coinRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(coinRecord);
                if (goItemCoinRecord != null && goItemCoinRecord.displayObject.gameObject != null)
                    goItemCoinRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemCoinRecord = coinRecord;
                goItemCoinRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            GComponent gameRecord = goMenu.GetChild("gameRecord")?.asCom ?? null;
            if (gameRecord != null)
            {
                goMenu.RemoveChild(gameRecord);
                gameRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleRecordChoose-menu-gameRecord";
                gameRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(gameRecord);
                if (goItemGameRecord != null && goItemGameRecord.displayObject.gameObject != null)
                    goItemGameRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemGameRecord = gameRecord;
                goItemGameRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            GComponent jpRecord = goMenu.GetChild("jpRecord")?.asCom ?? null;
            if (jpRecord != null)
            {
                goMenu.RemoveChild(jpRecord);
                jpRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleRecordChoose-menu-jpRecord";
                jpRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(jpRecord);
                if (goItemJpRecord != null && goItemJpRecord.displayObject.gameObject != null)
                    goItemJpRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemJpRecord = jpRecord;
                goItemJpRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            GComponent goItem = goMenu.GetChild("eventRecord")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleRecordChoose-menu-eventRecord";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goItemEventRecord != null && goItemEventRecord.displayObject.gameObject != null)
                    goItemEventRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemEventRecord = goItem;
                goItemEventRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            GComponent erroRecord = goMenu.GetChild("erroRecord")?.asCom ?? null;
            if (erroRecord != null)
            {
                goMenu.RemoveChild(erroRecord);
                erroRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleRecordChoose-menu-erroRecord";
                erroRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(erroRecord);
                if (goItemErroRecord != null && goItemErroRecord.displayObject.gameObject != null)
                    goItemErroRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemErroRecord = erroRecord;
                goItemErroRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }



            if (PlayerPrefsUtils.isUseAllConsolePage)
            {
                goMenu.AddChildAt(goBusinessRecord, goMenu.numChildren - 1);
                goMenu.AddChildAt(goItemCoinRecord, goMenu.numChildren - 1);
                goMenu.AddChildAt(goItemGameRecord, goMenu.numChildren - 1);
                goMenu.AddChildAt(goItemJpRecord, goMenu.numChildren - 1);

            }

            goMenu.AddChildAt(goItemEventRecord, goMenu.numChildren - 1);
            goMenu.AddChildAt(goItemErroRecord, goMenu.numChildren - 1);



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
                    case "businessRecord":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleBusinessRecord);
                        }
                        return;
                    case "coinRecord":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleCheckHardware);
                        }
                        return;
                    case "gameRecord":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleTestCoinPush);
                        }
                        return;
                    case "jpRecord":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleSettings);
                        }
                        return;
                    case "eventRecord":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleEventRecord);
                        }
                        return;
                    case "erroRecord":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleErrorRecord);
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