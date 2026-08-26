using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>普通局 Big / Huge / Massive 赢分弹窗，播 PAG 并滚动数字后自动关闭。</summary>
    public class PopupBigWin : MachinePageBase
    {
        /// <summary>FairyGUI 包名。</summary>
        public new const string pkgName = "MeiZhouHeiBao";
        /// <summary>弹窗组件名。</summary>
        public new const string resName = "PopupBigWin";

        /// <summary>PAG 资源目录。</summary>
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        /// <summary>Big / Super / Mega 三档 PAG 路径。</summary>
        private string[] PagPathBigWin = {
            "ng_pop_bigwin/BigWin_bmp",
            "ng_pop_bigwin/SuperWin_bmp",
            "ng_pop_bigwin/MegaWin_bmp" };
        /// <summary>对应三档弹窗自动关闭时长（秒）。</summary>
        private float [] CloseBigWinTime = { 5.0f, 6.0f, 9.0f, };
        /// <summary>对应三档数字滚动时长（秒）。</summary>
        private float[] CloseScoreNumTime = { 4.8f, 5.8f, 8.8f, };
        /// <summary>与 WinLevelType 对应的档位名。</summary>
        private readonly string[] winString = { "BIG", "HUGE", "MASSIVE" };

        /// <summary>本局展示赢分。</summary>
        private long _score;
        /// <summary>开页传入的赢分档位字符串。</summary>
        private string _winType;
        /// <summary>0=Big，1=Huge，2=Massive。</summary>
        private int _winIndex;

        /// <summary>赢分文本。</summary>
        private GTextField textBigWin;
        /// <summary>PAG 挂点。</summary>
        private GComponent comBigWin;
        /// <summary>大奖 PAG 播放槽。</summary>
        private PagSlotBinding pagBigWin;
        /// <summary>延迟启动数字滚动的定时器。</summary>
        private TimerCallback _rollCallback;
        /// <summary>到时关页的定时器。</summary>
        private TimerCallback _exitCallback;
        private TimerCallback _closeNumCallback;
        /// <summary>创建弹窗根节点并 InitParam。</summary>
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            isInit = true;
            InitParam();
        }

        /// <summary>切语言时重建 UI 并重新绑定 PAG。</summary>
        protected override void OnLanguageChange(I18nLang lang)
        {
            pagBigWin?.StopWithDefaults();
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        /// <summary>绑定赢分文本与 PAG；打开时按档位播放并定时滚分、关页。</summary>
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
                NumberAnimation.Instance.AnimateNumber(textBigWin, 0, _score, CloseBigWinTime[_winIndex]-2.0f, EaseType.Linear, () => { });
            };
            Timers.inst.Add(1, 1, _rollCallback);

            _closeNumCallback = obj =>
            {
                textBigWin.text=string.Empty;
            };
            Timers.inst.Add(CloseScoreNumTime[_winIndex], 1, _closeNumCallback);

            _exitCallback = exit;
            Timers.inst.Add(CloseBigWinTime[_winIndex], 1, _exitCallback);
        }

        /// <summary>解析赢分与档位后刷新界面。</summary>
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

        /// <summary>关页：停滚分、清定时器与 PAG。</summary>
        public override void OnClose(EventData eventData = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            ClearAllTimers();
            ClearPag();
            base.OnClose(eventData);
        }
        /// <summary>定时器到期：停动画并关闭自身。</summary>
        public void exit(object obj = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            textBigWin.text = string.Empty;
            ClearPag();
            ClearAllTimers();
            CloseSelf(null);
        }

        /// <summary>停止大奖 PAG。</summary>
        private void ClearPag()
        {
            pagBigWin?.StopWithDefaults();
        }

        /// <summary>移除滚分与关页定时器。</summary>
        private void ClearAllTimers()
        {
            RemoveTimer(ref _rollCallback);
            RemoveTimer(ref _exitCallback);
            RemoveTimer(ref _closeNumCallback);
        }

        /// <summary>若定时器仍在队列中则移除并置空。</summary>
        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            if (Timers.inst.Exists(timerCallback))
                Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}
