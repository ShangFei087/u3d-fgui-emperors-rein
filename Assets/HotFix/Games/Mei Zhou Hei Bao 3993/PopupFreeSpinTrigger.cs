using FairyGUI;
using GameMaker;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinTrigger/";

        private int _totalCount;
        private GButton _closeBtn;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
        }

        private void InitParam(EventData eventData)
        {
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
        }

        private void ResLoadCallback()
        {
            if (--_totalCount != 0) return;

            isInit = true;
            InitParam(null);
        }
    }
}