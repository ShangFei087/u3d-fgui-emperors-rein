using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupOverWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupOverWin";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupOverWin/SpinePrefabs/";

        private const string ModelPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Npc/";

        private int _totalCount = -1;
        private bool _isInitialized = false;

        private Animator _traderAnimator, _overWinAnimator;
        private GComponent _compareOverWin, _compareTrader;
        private GameObject _overWinObj, _cloneOverWinObj, _traderObj, _cloneTraderObj;

        private Transform _bgEffectParent;

        private readonly string[] WinString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] WinOpenString = { "bigwin_start", "bigwin_superwin", "superwin_megawin" };
        private readonly string[] WinCloseString = { "bigwin_end", "superwin_end", "megawin_end" };

        private readonly string[] NpcStartString =
        {
            "ng_pop_border_bigwin", "ng_pop_border_supwin", "ng_pop_border_megawin"
        };

        private long score;
        private string WinType;
        private int playCount;
        private int WinIndex;
        private bool isok = false;

        private GTextField _overWinText;

        // 定时器回调委托
        private TimerCallback _sequenceCallback;
        private TimerCallback _exitCallback;

        // 每级动画持续时间
        private const float LEVEL_DURATION = 3.0f;

        // 结束动画等待时间
        private const float EXIT_DELAY = 1.0f;

        // 结束动画跳转时间点
        private const float CLOSE_TIME = 14.5f;

        // 状态标记
        private bool _isExiting = false;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            _overWinText = contentPane.GetChild("overWinText").asTextField;
            LoadAsyncRes();
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

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            BindPrefabToUI();
            ShowAnimAndEffect();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            if (eventData?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    score = longScore;

                WinType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }

            WinIndex = Array.IndexOf(WinString, WinType);
            Debug.LogError("WinIndex:" + WinIndex);
            if (WinIndex < 0) WinIndex = 0;
            if (WinIndex > 2) WinIndex = 2;

            isok = false;
            _isExiting = false;
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ResetView();
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void LoadAsyncRes()
        {
            _totalCount = 2;

            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "overWin.prefab",
                (clone) =>
                {
                    _overWinObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                ModelPrefabPath + "ng_pop_border.prefab",
                (clone) =>
                {
                    _traderObj = clone;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabToUI()
        {
            GComponent currentGCom = contentPane.GetChild("anchorOverWin").asCom;
            if (currentGCom != _compareOverWin)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);
                _compareOverWin = currentGCom;
                _cloneOverWinObj = Object.Instantiate(_overWinObj);
                _overWinAnimator = _cloneOverWinObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(_compareOverWin, _cloneOverWinObj);
            }

            currentGCom = contentPane.GetChild("anchorPlayer").asCom;
            if (currentGCom != _compareTrader)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
                _compareTrader = currentGCom;
                _cloneTraderObj = Object.Instantiate(_traderObj);
                _traderAnimator = _cloneTraderObj.GetComponentInChildren<Animator>();
                _bgEffectParent = _cloneTraderObj.transform.Find("BgEffect");
                GameCommon.FguiUtils.AddWrapper(_compareTrader, _cloneTraderObj);
            }
        }

        public void SpinDown()
        {
            if (_isExiting) return;

            if (!isok)
            {
                // 强制完成数字滚动
                NumberAnimation.Instance.StopAllAnimations();
                _overWinText.text = score.ToString();

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
                if (WinString.Length < 3)
                {
                    Debug.LogError("WinImageString must have at least 3 elements");
                    return;
                }

                _overWinText.visible = true;
                playCount = 0;

                // 启动数字滚动
                int showtime = 4 * (WinIndex + 1);
                NumberAnimation.Instance.AnimateNumber(_overWinText, 0, score, showtime, EaseType.Linear, null);

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
            if (playCount < 0 || playCount >= NpcStartString.Length)
            {
                Debug.LogError($"playCount 越界: {playCount}");
                return;
            }

            _traderAnimator.Play(NpcStartString[playCount]);
            _overWinAnimator.Play(WinOpenString[playCount]);
            ShowEffect(playCount);

            // 3秒后进入下一步
            Timers.inst.Add(LEVEL_DURATION, 1, _sequenceCallback);
        }

        /// <summary>
        /// 每级动画结束后的回调
        /// </summary>
        private void OnSequenceStep(object obj)
        {
            playCount++;

            // 如果还有更多层级，继续播放
            if (playCount <= WinIndex)
            {
                PlayCurrentLevel();
            }
            else
            {
                // 所有层级播放完毕，进入结束流程
                // 如果数字还在滚动，直接停止并赋最终值
                NumberAnimation.Instance.StopAllAnimations();
                _overWinText.text = score.ToString();

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
            isok = true;

            // 播放结束动画
            int closeIndex = Mathf.Clamp(WinIndex, 0, WinCloseString.Length - 1);
            _overWinAnimator.Play(WinCloseString[closeIndex]);

            float closetime = CLOSE_TIME;
            AnimatorStateInfo stateInfo = _overWinAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = closetime / stateInfo.length;
            _overWinAnimator.Play(stateInfo.fullPathHash, 0, normalizedTime);

            // 清理动画序列定时器
            if (_sequenceCallback != null && Timers.inst.Exists(_sequenceCallback))
                Timers.inst.Remove(_sequenceCallback);

            // 等待1秒后退出
            if (!Timers.inst.Exists(_exitCallback))
            {
                Timers.inst.Add(EXIT_DELAY, 1, _exitCallback);
            }
        }

        private void OnExitTimer(object obj)
        {
            Exit();
        }

        public void Exit()
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

        private void ShowEffect(int index)
        {
            for (int i = 0; i < _bgEffectParent.transform.childCount; i++)
            {
                _bgEffectParent.transform.GetChild(i).gameObject.SetActive(i == index);
            }
        }

        private void ResetView()
        {
            ClearAllTimers();

            GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
            GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);

            Object.Destroy(_cloneTraderObj);
            Object.Destroy(_cloneOverWinObj);

            _compareTrader = null;
            _compareOverWin = null;
            _cloneTraderObj = null;
            _cloneOverWinObj = null;

            _traderAnimator = null;
            _bgEffectParent = null;
        }
    }
}