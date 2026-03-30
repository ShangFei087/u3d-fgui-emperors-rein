using FairyGUI;
using System;
using GameMaker;


namespace ConsoleSlot01
{
    public class PageConsoleLogRecord : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleLogRecord";
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

        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose;
        GButton btnPrev;
        GButton btnNext;
        GRichTextField rtxtBottomTitle;
        Controller tabController;

        TabLogRecordController tabEventRecordCtrl = new TabLogRecordController();
        TabLogRecordController tabErrorRecordCtrl = new TabLogRecordController();
        LogPageInfo _eventPageInfo = new LogPageInfo() { curPageNumber = 0, totalPageCount = 0, logRecords = null };
        LogPageInfo _errorPageInfo = new LogPageInfo() { curPageNumber = 0, totalPageCount = 0, logRecords = null };
        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            tabController = this.contentPane.GetController("tab");
            tabController.onChanged.Clear();
            tabController.onChanged.Add(OnTabChanged);

            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() => {
                CloseSelf(null);
            });

            btnPrev = this.contentPane.GetChild("navBottom").asCom.GetChild("btnPrev").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickPrev);

            btnNext = this.contentPane.GetChild("navBottom").asCom.GetChild("btnNext").asButton;
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickNext);

            rtxtBottomTitle = this.contentPane.GetChild("navBottom").asCom.GetChild("title").asRichTextField;

            tabEventRecordCtrl.InitParam(this.contentPane.GetChild("pages").asCom.GetChildAt(0).asCom, ConsoleTableName.TABLE_LOG_EVENT_RECORD, OnEventPageChange);
            tabErrorRecordCtrl.InitParam(this.contentPane.GetChild("pages").asCom.GetChildAt(1).asCom, ConsoleTableName.TABLE_LOG_ERROR_RECORD, OnErrorPageChange);

            UpdateBottomTitle();
        }

        void OnTabChanged(EventContext context)
        {
            UpdateBottomTitle();
        }

        void OnEventPageChange(LogPageInfo pageInfo)
        {
            _eventPageInfo = pageInfo;
            if (tabController != null && tabController.selectedIndex == 0)
            {
                UpdateBottomTitle();
            }
        }

        void OnErrorPageChange(LogPageInfo pageInfo)
        {
            _errorPageInfo = pageInfo;
            if (tabController != null && tabController.selectedIndex == 1)
            {
                UpdateBottomTitle();
            }
        }

        void OnClickPrev()
        {
            if (tabController == null) return;

            if (tabController.selectedIndex == 0)
            {
                tabEventRecordCtrl.PrevPage();
            }
            else
            {
                tabErrorRecordCtrl.PrevPage();
            }
        }

        void OnClickNext()
        {
            if (tabController == null) return;

            if (tabController.selectedIndex == 0)
            {
                tabEventRecordCtrl.NextPage();
            }
            else
            {
                tabErrorRecordCtrl.NextPage();
            }
        }

        void UpdateBottomTitle()
        {
            if (rtxtBottomTitle == null) return;

            bool isEventTab = tabController == null || tabController.selectedIndex == 0;
            LogPageInfo pageInfo = isEventTab ? _eventPageInfo : _errorPageInfo;
            int curPage = pageInfo != null ? pageInfo.curPageNumber : 0;
            int totalPage = pageInfo != null ? pageInfo.totalPageCount : 0;

            if (SBoxModel.Instance.language == "cn")
            {
                string tabText = isEventTab ? "事件记录" : "报警记录";
                rtxtBottomTitle.text = $"{tabText}，第{curPage}/{totalPage}页";
            }
            else
            {
                string tabText = isEventTab ? "Event Record" : "Warning Record";
                rtxtBottomTitle.text = $"{tabText}, Page {curPage} of {totalPage}";
            }
        }
    }
}
