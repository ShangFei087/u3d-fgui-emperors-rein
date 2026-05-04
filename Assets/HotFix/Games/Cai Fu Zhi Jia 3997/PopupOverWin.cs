using FairyGUI;
using GameMaker;
using Spine.Unity;
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

        private Transform _bgEffectParent /*, _boomEffectParent*/; // 特效父物体

        private readonly string[] WinString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] WinOpenString = { "bigwin_start", "bigwin_superwin", "superwin_megawin" };
        private readonly string[] WinCloseString = { "bigwin_end", "superwin_end", "megawin_end" };

        private readonly string[] NpcStartString =
        {
            "ng_pop_border_bigwin", "ng_pop_border_supwin", "ng_pop_border_megawin"
        };

        private long score; //分数
        private string WinType;
        private int playCount;
        private int WinIndex; //bigwin等级下标
        private bool isok = false;

        private GTextField _overWinText;

        //定时器
        private List<TimerCallback> _timerCallbacks = new List<TimerCallback>();

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
            InitParam();

            if (eventData?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    score = longScore;

                WinType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }

            WinIndex = Array.IndexOf(WinString, WinType); // 获取当前中奖索引
            isok = false;
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

            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "overWin.prefab",
                (clone) =>
                {
                    _overWinObj = clone;
                    ResLoadedCallback();
                });

            // 加载3D Obj
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
            // Spine
            GComponent currentGCom = contentPane.GetChild("anchorOverWin").asCom;
            if (currentGCom != _compareOverWin)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);
                _compareOverWin = currentGCom;
                _cloneOverWinObj = Object.Instantiate(_overWinObj);
                // _cloneOverWinObj.SetActive(false);
                _overWinAnimator = _cloneOverWinObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(_compareOverWin, _cloneOverWinObj);
            }

            // 3D Obj
            currentGCom = contentPane.GetChild("anchorPlayer").asCom;
            if (currentGCom != _compareTrader)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
                _compareTrader = currentGCom;
                _cloneTraderObj = Object.Instantiate(_traderObj);
                _traderAnimator = _cloneTraderObj.GetComponentInChildren<Animator>();
                _bgEffectParent = _cloneTraderObj.transform.Find("BgEffect");
                // _boomEffectParent = _cloneTraderObj.transform.Find("BoomEffect");
                GameCommon.FguiUtils.AddWrapper(_compareTrader, _cloneTraderObj);
            }
        }
        
        public void SpinDown()
        {
            if (!isok)
            {
                AniEnd();
            }
            else
            {
                ClearAllTimers();
                exit();
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
                int showtime = 4 * (WinIndex + 1);
                NumberAnimation.Instance.AnimateNumber(_overWinText, 0, score, showtime, EaseType.Linear, () => { });

                playCount = 0;
                TimerCallback sequenceCallback = obj =>
                {
                    int currentPlayCount = playCount;
                    _traderAnimator.Play(NpcStartString[playCount]);
                    _overWinAnimator.Play(WinOpenString[playCount]);

                    TimerCallback innerCallback = innerObj =>
                    {
                        if (currentPlayCount == WinIndex)
                        {
                            NumberAnimation.Instance.StopAllAnimations();
                            _overWinText.text = score.ToString();
                            AniEnd();
                        }
                    };
                    playCount++;
                    Timers.inst.Add(3.0f, 1, innerCallback);
                    _timerCallbacks.Add(innerCallback);
                };
                Timers.inst.Add(3.0f, WinIndex, sequenceCallback);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void AniEnd()
        {
            _overWinText.visible = false;
            // _traderAnimator.Play(WinCloseString[playCount]);
            _overWinAnimator.Play(WinCloseString[playCount]);
            //bigwinPig动画播放到指定时间.
            float closetime = 14.5f;
            AnimatorStateInfo stateInfo = _overWinAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = closetime / stateInfo.length;

            _overWinAnimator.Play(stateInfo.fullPathHash, 0, normalizedTime);
            ClearAllTimers();
            isok = true;
            Timers.inst.Add(1f, 1, exit);
            _timerCallbacks.Add(exit);
        }
        
        public void exit(object obj = null)
        {
            ClearAllTimers();
            CloseSelf(null);
        }
        
        private void ClearAllTimers()
        {
            // 遍历列表移除所有定时器
            foreach (var callback in _timerCallbacks)
            {
                if (Timers.inst.Exists(callback)) // 检查定时器是否存在
                    Timers.inst.Remove(callback);
            }

            _timerCallbacks.Clear(); // 清空列表
            Debug.Log("所有定时器已清理");
        }

        private void ResetView()
        {
            GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
            GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);

            Object.Destroy(_cloneTraderObj);
            Object.Destroy(_cloneOverWinObj);

            _cloneTraderObj = null;
            _cloneOverWinObj = null;

            _traderAnimator = null;
            _bgEffectParent = null;
            // _boomEffectParent = null;
        }
    }
}