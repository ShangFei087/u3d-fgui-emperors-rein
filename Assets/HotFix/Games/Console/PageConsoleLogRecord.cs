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

        


        TabLogRecordController tabEventRecordCtrl = new TabLogRecordController();
        TabLogRecordController tabErrorRecordCtrl = new TabLogRecordController();
        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() => {
                CloseSelf(null);
            });


            tabEventRecordCtrl.InitParam(this.contentPane.GetChild("pages").asCom.GetChildAt(0).asCom, ConsoleTableName.TABLE_LOG_EVENT_RECORD);
            tabErrorRecordCtrl.InitParam(this.contentPane.GetChild("pages").asCom.GetChildAt(1).asCom, ConsoleTableName.TABLE_LOG_ERROR_RECORD);
        }
    }
}
