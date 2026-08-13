using FairyGUI;
using GameMaker;

namespace FeiZhouHeiXingXing_3994
{
    public class PopupSmallGameJackpotWin : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupSmallGameJackpotWin";

        private const string PrefabPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupSmallGameJackpotWin/";

        private int _totalCount;
        private bool _isClicked;
        private GTextField _scoreText;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
        }

        private void InitParam(EventData eventData)
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
        }

        public override void OnClose(EventData eventData = null)
        {
        }
        
        private void ResLoadCallback()
        {
            if (--_totalCount != 0) return;

            isInit = true;
            InitParam(null);
        }
    }
}