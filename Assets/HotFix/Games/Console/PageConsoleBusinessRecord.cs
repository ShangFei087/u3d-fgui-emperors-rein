using FairyGUI;
using GameMaker;
using SBoxApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleSlot01
{
    public class PageButtomInfo
    {
        public string title;
        public long curPageIndex;
        public long totalPageCount;

        public PageButtomInfo(string title)
        {
            this.title = title;
            this.curPageIndex = 0;
            this.totalPageCount = 1;
        }
    }
    public class PageConsoleBusinessRecord : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleBusinessRecord";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();
        }


        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);

            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);


            InitParam();
        }

        public override void OnClose(EventData data = null)
        {

            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);



            base.OnClose(data);
        }

        public override void OnTop()
        {
            DebugUtils.Log($"i am top  {this.name}");
        }

        GButton btnClose, btnClose2, btnPrev, btnNext;

        GList lstPages;

        GComponent cmpTabBusiness, cmpTabCoin;

        Controller myController;





        GRichTextField rtxtTotalBet, rtxtTotalWin, rtxtTotalProfitBet, rtxtRemainPoints,

            rtxtTotalCoinIn, rtxtTotalCoinOut, rtxtTotalProfitCoinInOut,

            rtxtTotalScoreUp, rtxtTotalScoreDown, rtxtTotalProfitlScoreUpDown,

            rtxtbillingInformation,


            rtxtDayInOutTotalCoinIn, rtxtDayInOutTotalCoinOut, rtxtDayInOutTotalProfitlCoinInOut,
            rtxtDayInOutTotalScoreUp, rtxtDayInOutTotalScoreDown, rtxtDayInOutTotalProfitlScoreUpDown,

            rtxtPageButtom1, rtxtPageButtom2,


            rtxtTipDayInOut, rtxtTipDayBusniess;


        DayBusinessRecordController001 dayBusinessCtrl = new DayBusinessRecordController001();

        InOutRecordController inOutRecordCtrl = new InOutRecordController();

        List<PageButtomInfo> pageButtomInfo = new List<PageButtomInfo>()
        {
            new PageButtomInfo("Business Record") ,
            new PageButtomInfo("Coin In-Out History, Page {0} of {1}"),
        };


        public override void InitParam()
        {
            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(null);
            });

            btnClose2 = this.contentPane.GetChild("navBottom2").asCom.GetChild("btnExit").asButton;
            btnClose2.onClick.Clear();
            btnClose2.onClick.Add(() =>
            {
                CloseSelf(null);
            });


            lstPages = this.contentPane.GetChild("pages").asList;

            cmpTabBusiness = lstPages.GetChildAt(0).asCom;
            cmpTabCoin = lstPages.GetChildAt(1).asCom;


            myController = this.contentPane.GetController("tab");
            myController.onChanged.Clear();
            myController.onChanged.Add(OnControllerChanged);
            myController.selectedIndex = 0;




            rtxtPageButtom1 = this.contentPane.GetChild("navBottom").asCom.GetChild("title").asRichTextField;
            rtxtPageButtom2 = this.contentPane.GetChild("navBottom2").asCom.GetChild("title").asRichTextField;


            rtxtTipDayBusniess = cmpTabBusiness.GetChild("tipDayBusniess").asCom.GetChild("title").asRichTextField;
            rtxtTipDayInOut = cmpTabCoin.GetChild("tipDayInOut").asCom.GetChild("title").asRichTextField;
            rtxtTipDayBusniess.text = string.Format(I18nMgr.T("[Note]: Only retain the most recent {0} business day data."), SBoxModel.Instance.businiessDayRecordMax);
            rtxtTipDayInOut.text = string.Format(I18nMgr.T("[Note]: Only retain the most recent {0} In-Out data."), SBoxModel.Instance.coinInOutRecordMax);



            InitTotalBusiness();

            InitDayBusiness();

            InitInOutRecord();


            btnPrev = this.contentPane.GetChild("navBottom2").asCom.GetChild("btnPrev").asButton;
            btnNext = this.contentPane.GetChild("navBottom2").asCom.GetChild("btnNext").asButton;
            btnPrev.onChanged.Clear();
            btnNext.onChanged.Clear();
            btnPrev.onClick.Add(() =>
            {
                inOutRecordCtrl.OnPrevCoinInOutRecord();
            });
            btnNext.onClick.Add(() =>
            {
                inOutRecordCtrl.OnNextCoinInOutRecord();
            });



            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {

                SBoxAccount data = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {
                        SBoxModel.Instance.SboxPlayerAccount = playerAccountList[i];
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {

                DebugUtils.Log(err.msg);
            });

        }


        void InitTotalBusiness()
        {
            rtxtTotalBet = cmpTabBusiness.GetChild("totalBet").asCom.GetChild("value").asRichTextField;
            rtxtTotalWin = cmpTabBusiness.GetChild("totalWin").asCom.GetChild("value").asRichTextField;
            rtxtTotalProfitBet = cmpTabBusiness.GetChild("totalProfitBet").asCom.GetChild("value").asRichTextField;
            rtxtRemainPoints = cmpTabBusiness.GetChild("remainingPoints").asCom.GetChild("value").asRichTextField;

            rtxtTotalCoinIn = cmpTabBusiness.GetChild("totalCoinIn").asCom.GetChild("value").asRichTextField;
            rtxtTotalCoinOut = cmpTabBusiness.GetChild("totalCoinOut").asCom.GetChild("value").asRichTextField;
            rtxtTotalProfitCoinInOut = cmpTabBusiness.GetChild("totalProfitCoinInOut").asCom.GetChild("value").asRichTextField;

            rtxtTotalScoreUp = cmpTabBusiness.GetChild("totalScoreUp").asCom.GetChild("value").asRichTextField;
            rtxtTotalScoreDown = cmpTabBusiness.GetChild("totalScoreDown").asCom.GetChild("value").asRichTextField;
            rtxtTotalProfitlScoreUpDown = cmpTabBusiness.GetChild("totalProfitScoreUpDown").asCom.GetChild("value").asRichTextField;

            rtxtbillingInformation = cmpTabBusiness.GetChild("billingInformation").asCom.GetChild("value").asRichTextField;

            OnPropertyChangeSBoxPlayerAccount();
        }
        void InitDayBusiness()
        {
            GRichTextField rtxtTotalBetDB = cmpTabBusiness.GetChild("totalBetDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalWinDB = cmpTabBusiness.GetChild("totalWinDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalProfitBetDB = cmpTabBusiness.GetChild("totalProfitBetDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalCoinInDB = cmpTabBusiness.GetChild("totalCoinInDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalCoinOutDB = cmpTabBusiness.GetChild("totalCoinOutDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalProfitCoinInOutDB = cmpTabBusiness.GetChild("totalProfitCoinInOutDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalScoreUpDB = cmpTabBusiness.GetChild("totalScoreUpDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalScoreDownDB = cmpTabBusiness.GetChild("totalScoreDownDaly").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtTotalProfitScoreUpDownDB = cmpTabBusiness.GetChild("totalProfitScoreUpDownDaly").asCom.GetChild("value").asRichTextField;
            GComboBox comboDateBusinessDayRecord = cmpTabBusiness.GetChild("dateDalyBusiness").asCom.GetChild("value").asComboBox;

            // 日营收记录
            dayBusinessCtrl.InitParam(
                rtxtTotalBetDB,
                rtxtTotalWinDB,
                rtxtTotalProfitBetDB,
                rtxtTotalCoinInDB,
                rtxtTotalCoinOutDB,
                rtxtTotalProfitCoinInOutDB,
                rtxtTotalScoreUpDB,
                rtxtTotalScoreDownDB,
                rtxtTotalProfitScoreUpDownDB,
                comboDateBusinessDayRecord
            );
        }


        void InitInOutRecord()
        {
            GRichTextField rtxtDayInOutTotalCoinIn = cmpTabCoin.GetChild("totalCoinInSummary").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtDayInOutTotalCoinOut = cmpTabCoin.GetChild("totalCoinOutSummary").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtDayInOutTotalProfitlCoinInOut = cmpTabCoin.GetChild("totalProfitCoinSummary").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtDayInOutTotalScoreUp = cmpTabCoin.GetChild("totalScoreUpSummary").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtDayInOutTotalScoreDown = cmpTabCoin.GetChild("totalScoreDownSummary").asCom.GetChild("value").asRichTextField;
            GRichTextField rtxtDayInOutTotalProfitlScoreUpDown = cmpTabCoin.GetChild("totalProfitScoreSummary").asCom.GetChild("value").asRichTextField;
            GComboBox comboDateInOut = cmpTabCoin.GetChild("dateInOut").asCom.GetChild("value").asComboBox;

            List<GComponent> rows = new List<GComponent>();
            for (int i = 0; i <= 9; i++)
            {
                rows.Add(cmpTabCoin.GetChild($"row{i}").asCom);
            }


            inOutRecordCtrl.InitParam(
                rtxtDayInOutTotalCoinIn,
                rtxtDayInOutTotalCoinOut,
                rtxtDayInOutTotalProfitlCoinInOut,
                rtxtDayInOutTotalScoreUp,
                rtxtDayInOutTotalScoreDown,
                rtxtDayInOutTotalProfitlScoreUpDown,
                rows,
                comboDateInOut,
                OnInOutPageIndexChange
            );
        }


        void OnInOutPageIndexChange(int curPageIndex, int totalPageCount)
        {
            pageButtomInfo[1].curPageIndex = curPageIndex;
            pageButtomInfo[1].totalPageCount = totalPageCount;
            OnControllerChanged(1);
        }


        void OnPropertyChange(EventData res = null)
        {
            string name = res.name;
            switch (name)
            {
                case "SBoxModel/SboxPlayerAccount":
                    OnPropertyChangeSBoxPlayerAccount(res);
                    break;
            }
        }

        void OnPropertyChangeSBoxPlayerAccount(EventData res = null)
        {
            SetDataTotalWinInfo();
            SetDataTotalCoinInOutScoreUpDown();
            SetDataBillInfo();
        }

        void SetDataTotalWinInfo()
        {
            rtxtTotalBet.text = SBoxModel.Instance.HistoryTotalBet.ToString();
            rtxtTotalWin.text = SBoxModel.Instance.HistoryTotalWin.ToString();
            rtxtTotalProfitBet.text = SBoxModel.Instance.HistoryTotalProfitBet.ToString();
            rtxtRemainPoints.text = $"{SBoxModel.Instance.myCredit}";
        }

        /// <summary>
        /// 设置账单信息
        /// </summary>
        void SetDataBillInfo()
        {
            string res = $"{SBoxModel.Instance.BillInfoTime}\n{SBoxModel.Instance.BillInfoLineMachineNumber}\n{SBoxModel.Instance.BillInfoHardwareAlgorithmVer}";
            rtxtbillingInformation.text = res;
        }


        /// <summary>
        /// 历史总上下分、总投退币
        /// </summary>
        void SetDataTotalCoinInOutScoreUpDown()
        {
            rtxtTotalCoinIn.text = $"{SBoxModel.Instance.HistoryTotalCoinInCredit}";
            rtxtTotalCoinOut.text = $"{SBoxModel.Instance.HistoryTotalCoinOutCredit}";
            rtxtTotalProfitCoinInOut.text = $"{SBoxModel.Instance.HistoryTotalProfitCoinIn}";

            rtxtTotalScoreUp.text = $"{SBoxModel.Instance.HistoryTotalScoreUpCredit}";
            rtxtTotalScoreDown.text = $"{SBoxModel.Instance.HistoryTotalScoreDownCredit}";
            rtxtTotalProfitlScoreUpDown.text = $"{SBoxModel.Instance.HistoryTotalProfitScoreUp}";
        }

        // 控制器变化回调
        private void OnControllerChanged(EventContext context)
        {
            Controller controller = (Controller)context.sender;
            DebugUtils.Log($"控制器已切换，当前页签索引: {controller.selectedIndex}, 名称: {controller.selectedPage}");
            int pageIndex = controller.selectedIndex;
            OnControllerChanged(pageIndex);
        }

        private void OnControllerChanged(int pageIndex)
        {
            if (pageIndex == 0)
            {
                rtxtPageButtom1.text = I18nMgr.T(pageButtomInfo[pageIndex].title);
            }
            else if (pageIndex == 1)
            {
                rtxtPageButtom2.text = string.Format(I18nMgr.T(pageButtomInfo[pageIndex].title),
                    pageButtomInfo[pageIndex].curPageIndex + 1, pageButtomInfo[pageIndex].totalPageCount);
            }
        }
    }

}