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

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupBigWin/SpinePrefabs/";

        private const string ModelPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Npc/";

        private int _totalCount = -1;
        private bool _isInitialized = false;

        private Animator _npcAnimator, _overWinAnimator;
        private GComponent _compareOverWin, _compareNpc;
        private GameObject _overWinObj, _cloneOverWinObj, _npcObj, _cloneNpcObj;

        private Transform _bgEffectParent;

        private readonly string[] winString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] winOpenString = { "bigwin_start", "bigwin_superwin", "superwin_megawin" };
        private readonly string[] winCloseString = { "bigwin_end", "superwin_end", "megawin_end" };

        private readonly string[] npcStartString =
        {
            "ng_pop_border_bigwin", "ng_pop_border_supwin", "ng_pop_border_megawin"
        };

        private long score;
        private string winType;
        private int playCount;
        private int winIndex;
        private bool isOk = false;

        private GTextField _overWinText;

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

        private void InitParam(EventData eventData = null)
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

                winType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }

            winIndex = Array.IndexOf(winString, winType);
            // Debug.LogError("WinIndex:" + WinIndex);
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
                    _npcObj = clone;
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

            currentGCom = contentPane.GetChild("anchorTipNpc").asCom;
            if (currentGCom != _compareNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareNpc);
                _compareNpc = currentGCom;
                _cloneNpcObj = Object.Instantiate(_npcObj);
                _npcAnimator = _cloneNpcObj.GetComponentInChildren<Animator>();
                _bgEffectParent = _cloneNpcObj.transform.Find("BgEffect");
                GameCommon.FguiUtils.AddWrapper(_compareNpc, _cloneNpcObj);
            }
        }

        private void SpinDown()
        {
            if (_isExiting) return;

            if (!isOk)
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
                if (winString.Length < 3)
                {
                    Debug.LogError("WinImageString must have at least 3 elements");
                    return;
                }

                _overWinText.visible = true;
                playCount = 0;

                // 启动数字滚动
                int showtime = 4 * (winIndex + 1);
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
            if (playCount < 0 || playCount >= npcStartString.Length)
            {
                Debug.LogError($"playCount 越界: {playCount}");
                return;
            }

            _npcAnimator.Play(npcStartString[playCount]);
            _overWinAnimator.Play(winOpenString[playCount]);
            ShowEffect(playCount);

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
            isOk = true;

            // 播放结束动画
            int closeIndex = Mathf.Clamp(winIndex, 0, winCloseString.Length - 1);
            _overWinAnimator.Play(winCloseString[closeIndex]);

            float closetime = CloseTime;
            AnimatorStateInfo stateInfo = _overWinAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = closetime / stateInfo.length;
            _overWinAnimator.Play(stateInfo.fullPathHash, 0, normalizedTime);

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

        private void ShowEffect(int index)
        {
            for (int i = 0; i < _bgEffectParent.transform.childCount; i++)
            {
                _bgEffectParent.transform.GetChild(i).gameObject.SetActive(i == index);
            }
        }

        private void Reset()
        {
            ClearAllTimers();

            // GameCommon.FguiUtils.DeleteWrapper(_compareNpc);
            // GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);
            //
            // Object.Destroy(_cloneNpcObj);
            // Object.Destroy(_cloneOverWinObj);
            //
            // _compareNpc = null;
            // _compareOverWin = null;
            // _cloneNpcObj = null;
            // _cloneOverWinObj = null;
            //
            // _npcAnimator = null;
            // _bgEffectParent = null;
        }
    }
}