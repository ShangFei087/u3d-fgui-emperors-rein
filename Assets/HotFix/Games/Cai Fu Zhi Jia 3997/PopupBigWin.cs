using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupBigWin";

        private const string PagPath = "Games/Cai Fu Zhi Jia 3997/Pag/bigwin/";

        // Pag
        private GComponent _bigWinCom;
        private PagSlotBinding _bigWinPag;

        /// <summary> BigWin中奖类型数组 </summary>
        private readonly string[] _winTypeString = { "BIG", "HUGE", "MASSIVE" };
        
        /// <summary> 对应不同级别BigWin的Pag视频数组 </summary>
        private readonly string[] _pagEffString = { "ng_pop_border-bigwin.pag", "ng_pop_border-supwin.pag", "ng_pop_border-megawin.pag" };

        /// <summary> 每个级别Pag视频的时长 </summary>
        private readonly float[] _pagTimes = { 5.23f, 10.03f, 14.63f};

        private long _score; // BigWin中奖得分
        private int _winIndex; // 当前中大奖索引
        private bool _isExiting; // 当前动画是否已经播放完成
        private GTextField _bigWinText; // 显示BigWin得分的组件
        private const float ExitDelay = 1.0f; // 每一级Pag的结束等待时间
        private TimerCallback _aniEndCallback, _exitCallback; // pag和数字滚动播放结束之后的回调函数 

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            InitParam(null); // 因为BigWin不需要加载预制体，所以需要将InitParam在OnInit里直接调用，否则无法触发Loading中的回调，导致无法正常进入游戏
            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        OnAniEnd(null);
                    }
                },
            };
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
        }

        private void InitParam(EventData eventData = null)
        {
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            // 重置状态
            _isExiting = false;
            // 获取UI组件
            _bigWinText = contentPane.GetChild("bigWinText").asTextField;
            // 绑定Pag视频
            _bigWinCom = contentPane.GetChild("pag_BigWin").asCom;
            _bigWinPag = new PagSlotBinding("bigWin", PagPath);
            _bigWinPag.EnsureSlot(_bigWinCom);

            PlayNumAniAndPag();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            if (eventData?.value is Dictionary<string, object> dic)
            {
                // 获取BigWin得分
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    _score = longScore;

                // 获取BigWin中奖类型索引
                string winType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
                if (_winTypeString.Contains(winType))
                    _winIndex = Array.IndexOf(_winTypeString, winType);
                if (_winIndex < 0) _winIndex = 0;
                if (_winIndex > _winTypeString.Length) _winIndex = _winTypeString.Length - 1;
            }

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ClearPag();
            ClearTimers();
        }

        /// <summary> 播放数字滚动动画以及对应的Pag视频 </summary>
        private void PlayNumAniAndPag()
        {
            try
            {
                if (_winTypeString.Length < 3)
                {
                    Debug.LogError("最少有三种中将类型");
                    return;
                }

                // 播放数字滚动动画
                _bigWinText.visible = true;
                float showtime = _pagTimes[_winIndex];
                NumberAnimation.Instance.AnimateNumber(_bigWinText, 0, _score, showtime);

                // 初始化动画结束之后的回调
                _exitCallback = OnExit;
                _aniEndCallback = OnAniEnd;

                // 播放对应中奖类型的Pag视频
                if (_bigWinPag == null) return;
                _bigWinPag.StopWithDefaults();
                _bigWinPag.Play(new PagSequencePlay(
                    new[] { new PagSegment(_pagEffString[_winIndex], 1) }, PagPlayLayout.Center,
                    useGpuSyncGroup: false));
                Timers.inst.Add(showtime, 1, _aniEndCallback);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnAniEnd(object obj)
        {
            // 停止数字动画，直接显示最终中奖结果
            NumberAnimation.Instance.StopAllAnimations();
            _bigWinText.text = _score.ToString();

            // 延时播放退出动画
            if (_isExiting) return;
            _isExiting = true;
            if (!Timers.inst.Exists(_exitCallback))
            {
                Timers.inst.Add(ExitDelay, 1, _exitCallback);
            }
        }

        private void OnExit(object obj)
        {
            CloseSelf(null);
        }

        /// <summary> 清除Pag对象，避免造成多余的内存占用</summary>
        private void ClearPag()
        {
            // _bigWinPag?.Dispose();
            _bigWinPag = null;
            if (_bigWinPag != null) _bigWinPag.StopWithDefaults();
        }

        /// <summary> 清除对Timers的事件监听，避免造成多余的内存占用</summary>
        private void ClearTimers()
        {
            if (_aniEndCallback != null && Timers.inst.Exists(_aniEndCallback))
                Timers.inst.Remove(_aniEndCallback);
            if (_exitCallback != null && Timers.inst.Exists(_exitCallback))
                Timers.inst.Remove(_exitCallback);
        }
    }
}