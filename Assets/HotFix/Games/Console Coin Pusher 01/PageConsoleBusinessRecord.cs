using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleCoinPusher01
{
    public class PageConsoleBusinessRecord : MachinePageBase
    {
        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleBusinessRecord";
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
            PageTitleManager.Instance.AddPageNode("总营收记录");
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

        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());
            this.contentPane.GetChild("contents1").asCom.GetChild("totalBet").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalBet.ToString();
            this.contentPane.GetChild("contents1").asCom.GetChild("totalWin").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalWin.ToString();
            this.contentPane.GetChild("contents1").asCom.GetChild("toralProfit").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalProfitBet.ToString();

            this.contentPane.GetChild("contents2").asCom.GetChild("totalCoinIn").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalCoinInCredit.ToString();
            this.contentPane.GetChild("contents2").asCom.GetChild("totalCoinOut").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalCoinOutCredit.ToString();
            this.contentPane.GetChild("contents2").asCom.GetChild("toralCoinProfit").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalProfitCoinIn.ToString();

            this.contentPane.GetChild("contents3").asCom.GetChild("totalScoreUp").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalScoreUpCredit.ToString();
            this.contentPane.GetChild("contents3").asCom.GetChild("totalScoreDown").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalScoreDownCredit.ToString();
            this.contentPane.GetChild("contents3").asCom.GetChild("toralScoreProfit").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.HistoryTotalProfitScoreUp.ToString();


            string str = SBoxModel.Instance.BillInfoTime.ToString() + "<br/>" + SBoxModel.Instance.BillInfoLineMachineNumber.ToString() + "<br/>" + SBoxModel.Instance.BillInfoHardwareAlgorithmVer.ToString();
            this.contentPane.GetChild("contents4").asCom.GetChild("value").asRichTextField.text = str;

            AddClickEvent();

            Clear(false);
        }


        void Clear(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            curIndexMenuItem = 0;
            goMenu = this.contentPane.GetChild("items").asCom;
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

