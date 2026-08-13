using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        private readonly string[] _pagEffString =
        {
            "PopupBigWin/bigWin.pag", "PopupBigWin/superWin.pag", "PopupBigWin/megaWin.pag"
        };
        
        private readonly string[] _winTypeString = { "BIG", "HUGE", "MASSIVE" };

        private long _score;
        private string _winType;
        private int _playCount;
        private int _winIndex;
        private bool _isOk;
        
        private GTextField _bigWinText;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            
            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        // SpinDown();
                    }
                },
            };
        }
        
        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        private void InitParam(EventData eventData = null)
        {
            // if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _bigWinCom = contentPane.GetChild("anchorBigWin").asCom;
            _bigWinPag = new PagSlotBinding("bigWin", PagPath);
            _bigWinPag.EnsureSlot(_bigWinCom);

            // _bigWinPag.Play(new PagSequencePlay(
            //     new[] { new PagSegment(_bigWin1080, 1) },
            //     PagPlayLayout.Center,
            //     PagPresentationDefaults.DisplayScale,
            //     useGpuSyncGroup: false));
            Timers.inst.Add(12, 1, (gameObj) =>
            {
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
            ClearPag();
        }

        private void ClearPag()
        {
            _bigWinPag.Dispose();
            _bigWinPag = null;
        }
    }
}