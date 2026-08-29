using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupBigWin";

        private const string GamePagFolder = "Games/Cai Fu Zhi Jia 3997/Pag";
        private const string PrefabPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupBigWin/";

        private int _totalCount = -1;
        private bool _isInitialized = false;

        private Animator _bigWinAnimator;
        private GComponent _compareBigWin;
        private GameObject _bigWinObj, _cloneBigWinObj;

        private readonly string[] winString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] winOpenString = { "bigwin_start", "bigwin_superwin", "superwin_megawin" };
        private readonly string[] winCloseString = { "bigwin_end", "superwin_end", "megawin_end" };

        private readonly string[] WinOpenEffString =
        {
            "bigwin/bigwin_start.pag", "bigwin/supwin_start.pag", "bigwin/megawin_start.pag"
        };

        private readonly string[] WinIdleEffString =
        {
            "bigwin/bigwin_idle.pag", "bigwin/superwin_idle.pag", "bigwin/megewin_idle.pag"
        };

        private long score;
        private string winType;
        private int playCount;
        private int winIndex;
        private bool isOk = false;

        private GTextField _bigWinText;

        // 定时器回调委托
        private TimerCallback _sequenceCallback;
        private TimerCallback _exitCallback;

        // 每级动画持续时间
        private const float LevelDuration = 3.0f;

        // 结束动画等待时间
        private const float ExitDelay = 1.0f;

        // 结束动画跳转时间点
        private const float CloseTime = 14.5f;

        // 状态标记
        private bool _isExiting = false;

        // Pag
        private GComponent _bigWinCom;
        private PagSlotBinding _bigWinPag;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "BigWin.prefab",
                (clone) =>
                {
                    _bigWinObj = clone;
                    ResLoadedCallback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        SpinDown();
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
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            // 获取UI组件
            _bigWinCom = contentPane.GetChild("pag_BigWin").asCom;
            _bigWinText = contentPane.GetChild("bigWinText").asTextField;
            // 绑定Spine
            GComponent currentGCom = contentPane.GetChild("anchorBigWin").asCom;
            if (currentGCom != _compareBigWin)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBigWin);
                _compareBigWin = currentGCom;
                _cloneBigWinObj = Object.Instantiate(_bigWinObj);
                _bigWinAnimator = _cloneBigWinObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(_compareBigWin, _cloneBigWinObj);
            }

            // 绑定Pag
            if (_bigWinCom == null) return;
            _bigWinPag = new PagSlotBinding("bigWin", GamePagFolder);
            _bigWinPag.EnsureSlot(_bigWinCom);

            ShowAnimAndEffect();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            if (eventData?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    score = longScore;

                winType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }

            winIndex = Array.IndexOf(winString, winType);
            if (winIndex < 0) winIndex = 0;
            if (winIndex > 2) winIndex = 2;

            isOk = false;
            _isExiting = false;
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            Reset();
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void SpinDown()
        {
            if (_isExiting) return;

            if (!isOk)
            {
                // 强制完成数字滚动
                NumberAnimation.Instance.StopAllAnimations();
                _bigWinText.text = score.ToString();

                // 直接跳到结束
                PlayEndAnimation();
            }
            else
            {
                Exit();
            }
        }

        private void ShowAnimAndEffect()
        {
            try
            {
                if (winString.Length < 3)
                {
                    Debug.LogError("WinImageString must have at least 3 elements");
                    return;
                }

                _bigWinText.visible = true;
                playCount = 0;

                // 启动数字滚动
                int showtime = 4 * (winIndex + 1);
                NumberAnimation.Instance.AnimateNumber(_bigWinText, 0, score, showtime, EaseType.Linear, null);

                // 初始化委托
                _sequenceCallback = OnSequenceStep;
                _exitCallback = OnExitTimer;

                // 立即播放第1级动画
                PlayCurrentLevel();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 播放当前层级的动画
        /// </summary>
        private void PlayCurrentLevel()
        {
            if (playCount < 0 || playCount >= winOpenString.Length)
            {
                Debug.LogError($"playCount 越界: {playCount}");
                return;
            }

            _bigWinAnimator.Play(winOpenString[playCount]);
            if (_bigWinPag == null) return;
            _bigWinPag.StopWithDefaults();
            _bigWinPag.Play(new PagSequencePlay(PagPlaySpecs.IntroLoop(WinOpenEffString[playCount], WinIdleEffString[playCount]), PagPlayLayout.Center));

            // 3秒后进入下一步
            Timers.inst.Add(LevelDuration, 1, _sequenceCallback);
        }

        /// <summary>
        /// 每级动画结束后的回调
        /// </summary>
        private void OnSequenceStep(object obj)
        {
            playCount++;

            // 如果还有更多层级，继续播放
            if (playCount <= winIndex)
            {
                PlayCurrentLevel();
            }
            else
            {
                // 所有层级播放完毕，进入结束流程
                // 如果数字还在滚动，直接停止并赋最终值
                NumberAnimation.Instance.StopAllAnimations();
                _bigWinText.text = score.ToString();

                PlayEndAnimation();
            }
        }

        /// <summary>
        /// 播放结束动画并等待关闭
        /// </summary>
        private void PlayEndAnimation()
        {
            if (_isExiting) return;
            _isExiting = true;
            isOk = true;

            // 播放结束动画
            int closeIndex = Mathf.Clamp(winIndex, 0, winCloseString.Length - 1);
            _bigWinAnimator.Play(winCloseString[closeIndex]);

            float closetime = CloseTime;
            AnimatorStateInfo stateInfo = _bigWinAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = closetime / stateInfo.length;
            _bigWinAnimator.Play(stateInfo.fullPathHash, 0, normalizedTime);

            // 清理动画序列定时器
            if (_sequenceCallback != null && Timers.inst.Exists(_sequenceCallback))
                Timers.inst.Remove(_sequenceCallback);

            // 等待1秒后退出
            if (!Timers.inst.Exists(_exitCallback))
            {
                Timers.inst.Add(ExitDelay, 1, _exitCallback);
            }
        }

        private void OnExitTimer(object obj)
        {
            Exit();
        }

        private void Exit()
        {
            ClearAllTimers();
            CloseSelf(null);
        }

        private void ClearAllTimers()
        {
            if (_sequenceCallback != null && Timers.inst.Exists(_sequenceCallback))
                Timers.inst.Remove(_sequenceCallback);

            if (_exitCallback != null && Timers.inst.Exists(_exitCallback))
                Timers.inst.Remove(_exitCallback);

            Debug.Log("所有定时器已清理");
        }

        private void ClearPag()
        {
            _bigWinPag?.Dispose();
            _bigWinPag = null;
            _bigWinCom = null;
        }

        private void Reset()
        {
            ClearPag();
            ClearAllTimers();
        }
    }
}