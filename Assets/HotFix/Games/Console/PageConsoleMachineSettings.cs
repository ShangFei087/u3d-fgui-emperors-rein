using FairyGUI;
using GameMaker;


namespace ConsoleSlot01
{

    public class PageConsoleMachineSettings : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleMachineSettings";

        Controller myController;
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();
        }



        public void TestGetPage()
        {
            GList glst = this.contentPane.GetChild("tabs").asList;
            GComponent gcom = glst.GetChildAt(0).asCom;

            GComponent gcomActive = gcom.GetChild("active").asCom;
            if (gcomActive is null)
            {
                DebugUtils.LogError("i am null");
            }
            else
            {

                GObject chd = gcomActive.GetChild("value");
                if (chd is GButton gbtn)
                {
                    (gbtn.GetChild("title").asTextField).text = "666";
                }
                else
                {
                    (chd.asTextField).text = "666";
                }

            }
        }


        public override void OnTop()
        {
            DebugUtils.Log($"i am top ConsoleMainPage {this.name}");
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
           
            InitParam();
        }



        GComponent cmpTabMachine, cmpTabInOut, cmpTabProgressive;

        GButton btnClose;

        /*
        GButton btnClose, btnCoinInScale, btnCoinOutScale, btnScoreScale, btnPrintOut, btnBillIn, btnScoreUpLongClickScale,
                btnChangePwdShift, btnChangePwdManager, btnChangePwdAdmin,
                btnMaxCoinInOutRecord, btnMaxGameRecord, btnMaxEventRecord, btnMaxErrorRecord, btnMaxBusinessDayRecord,
                btnFlipScreen,
                btnBillValidatorModel, btnPrinterModel,
                btnJpPercent,
                btnIOTAccessMethods,
                btnAgentID, btnMachineID,
                btnRemoteControlSetting, btnRemoteControlAccount,
                btnBonusReportSetting,
                btnJackpotGamePercent;
        */

        TabSettingsMachineController tabMachineCtrl = new TabSettingsMachineController();


        TabSettingsInOutController tabInOuCtrl = new TabSettingsInOutController();

        TabSettingsProgressive tabProgressive = new TabSettingsProgressive();
        public override void InitParam()
        {
            //btnClose = this.contentPane.GetChild("navBottom/btnExit").asButton;
            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(null);
            });
            myController = this.contentPane.GetController("tab");
            myController.onChanged.Clear();
            myController.onChanged.Add(OnControllerChanged);
            myController.selectedIndex = 0;

            cmpTabMachine = this.contentPane.GetChild("tabs").asList.GetChildAt(0).asCom;
            cmpTabInOut = this.contentPane.GetChild("tabs").asList.GetChildAt(1).asCom;
            cmpTabProgressive = this.contentPane.GetChild("tabs").asList.GetChildAt(2).asCom;

            tabMachineCtrl.InitParam(cmpTabMachine);

            tabInOuCtrl.InitParam(cmpTabInOut);

            tabProgressive.InitParam(cmpTabProgressive);
        }

        private void OnControllerChanged(EventContext context)
        {
            Controller controller = (Controller)context.sender;
            DebugUtils.Log($"控制器已切换，当前页签索引: {controller.selectedIndex}, 名称: {controller.selectedPage}");
            int pageIndex = controller.selectedIndex;
            //OnControllerChanged(pageIndex);
        }

        public override void OnClose(EventData data = null)
        {
            tabMachineCtrl.OnClose();
            //tabProgressive.Disable();
            base.OnClose(data);
        }

        //private void OnControllerChanged(int pageIndex)
        //{
        //    if (pageIndex == 0)
        //    {
        //        rtxtPageButtom1.text = I18nMgr.T(pageButtomInfo[pageIndex].title);
        //    }
        //    else if (pageIndex == 1)
        //    {
        //        rtxtPageButtom2.text = string.Format(I18nMgr.T(pageButtomInfo[pageIndex].title),
        //            pageButtomInfo[pageIndex].curPageIndex + 1, pageButtomInfo[pageIndex].totalPageCount);
        //    }
        //}
    }
}