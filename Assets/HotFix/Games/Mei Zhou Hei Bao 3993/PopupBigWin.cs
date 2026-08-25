using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupBigWin";

        //private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupBigWin/";
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        private string[] PagPathBigWin = {
            "ng_pop_bigwin/BigWin_bmp",
            "ng_pop_bigwin/SuperWin_bmp",
            "ng_pop_bigwin/MegaWin_bmp" };
        private float [] CloseBigWinTime = { 5.0f, 6.0f, 9.0f, }; //关闭bigwin页面时间


        private readonly string[] winString = { "BIG", "HUGE", "MASSIVE" };

        private long _score;//分数
        private string _winType;
        private int _winIndex;

        private GTextField textBigWin;
        private GComponent comBigWin;
        private PagSlotBinding pagBigWin;
        private TimerCallback _rollCallback;
        private TimerCallback _exitCallback;
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            isInit = true;
            InitParam();
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            pagBigWin?.StopWithDefaults();
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        public override void InitParam()
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();

            textBigWin = contentPane.GetChild("txtWin").asTextField;
            textBigWin.text = string.Empty;
            comBigWin = contentPane.GetChild("anchorPagBigWin").asCom;
            if (pagBigWin == null)
                pagBigWin = new PagSlotBinding("bigWin", PagPath);
            pagBigWin.EnsureSlot(comBigWin);

            if (!isOpen) return;

            ClearAllTimers();
            pagBigWin.Play(new PagSequencePlay(
                new[] { new PagSegment(PagPathBigWin[_winIndex], 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false));

            _rollCallback = obj =>
            {
                NumberAnimation.Instance.AnimateNumber(textBigWin, 0, _score, CloseBigWinTime[_winIndex] - 2.0f, EaseType.Linear, () => { });
            };
            Timers.inst.Add(1, 1, _rollCallback);
            _exitCallback = exit;
            Timers.inst.Add(CloseBigWinTime[_winIndex], 1, _exitCallback);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            // 解析数据
            if (eventData?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    _score = longScore;

                _winType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }
            _winIndex = Array.IndexOf(winString, _winType);
            if (_winIndex < 0) _winIndex = 0;
            if (_winIndex > 2) _winIndex = 2;
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            ClearAllTimers();
            ClearPag();
            base.OnClose(eventData);
        }
        public void exit(object obj = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            textBigWin.text = string.Empty;
            ClearPag();
            ClearAllTimers();
            CloseSelf(null);
        }

        private void ClearPag()
        {
            pagBigWin?.StopWithDefaults();
        }

        private void ClearAllTimers()
        {
            RemoveTimer(ref _rollCallback);
            RemoveTimer(ref _exitCallback);
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            if (Timers.inst.Exists(timerCallback))
                Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}
