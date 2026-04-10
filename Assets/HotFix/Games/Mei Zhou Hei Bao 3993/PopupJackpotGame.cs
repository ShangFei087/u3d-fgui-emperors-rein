using FairyGUI;
using GameMaker;
using SlotMaker;
using System.Collections.Generic;
using System;

namespace MeiZhouHeiBao_3993
{
    public class PopupJackpotGame : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupJackpotGame";

        private GComponent _gOwnerPanel;
        private GComponent _jackpotContent;
        private List<JackpotReel> _slotMachineContent = new List<JackpotReel>();

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
        }

        public override void InitParam()
        {
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            LoadPanel();
            ResetPanel();
            InitUICom();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ResetPage();
        }

        private void ResetPanel()
        {
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
        }

        private void LoadPanel()
        {
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            MainModel.Instance.contentMD = ContentModel.Instance;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
        }

        private void InitUICom()
        {
            _jackpotContent = contentPane.GetChild("jackpotContent").asCom;

            _slotMachineContent.Clear();
            for (int i = 1; i < _jackpotContent.numChildren; i++)
            {
                JackpotReel jackpotReel = new JackpotReel();
                GComponent jackpotReelCom = _jackpotContent.GetChildAt(i).asCom;
                for (int j = 0; j < jackpotReelCom.numChildren; j++)
                {
                    GComponent jackpotSymbol = jackpotReelCom.GetChild("jackpotSymbol_" + j).asCom;
                    jackpotReel.JackpotSymbols[j].IconLoader = jackpotSymbol.GetChild("icon").asLoader;
                    jackpotReel.JackpotSymbols[j].AnchorJackpotSpine =
                        jackpotSymbol.GetChild("anchor_JackpotSpine").asCom;
                }

                _slotMachineContent.Add(jackpotReel);
            }
        }

        private void ResetPage()
        {
        }
    }

    internal class JackpotSymbol
    {
        public GLoader IconLoader;
        public GComponent AnchorJackpotSpine;
    }

    internal class JackpotReel
    {
        public JackpotSymbol[] JackpotSymbols =
        {
            new JackpotSymbol(), new JackpotSymbol(), new JackpotSymbol(), new JackpotSymbol()
        };
    }
}