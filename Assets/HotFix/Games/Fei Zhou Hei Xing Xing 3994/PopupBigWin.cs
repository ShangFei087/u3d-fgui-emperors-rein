using FairyGUI;
using GameMaker;

namespace FeiZhouHeiXingXing_3994
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupBigWin";

        private const string PagPath = "Games/Fei Zhou Hei Xing Xing 3994/Pag/";

        // Pag
        private GComponent _bigWinCom;

        private PagSlotBinding _bigWinPag;

        private readonly string _bigWin1080 = "PopupBigWin/bigwin_1080.pag";  // 备用选项
        private readonly string _bigWin720 = "PopupBigWin/bigwin_720.pag";

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
        }

        private void InitParam(EventData eventData = null)
        {
            // if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _bigWinCom = contentPane.GetChild("anchorBigWin").asCom;
            _bigWinPag = new PagSlotBinding("bigWin", PagPath);
            _bigWinPag.EnsureSlot(_bigWinCom);

            _bigWinPag.Play(new PagSequencePlay(
                new[] { new PagSegment(_bigWin1080, 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false));
            Timers.inst.Add(12, 1, (gameObj) =>
            {
                DebugUtils.LogError("11111");
                CloseSelf(null);
            });
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
        }
    }
}