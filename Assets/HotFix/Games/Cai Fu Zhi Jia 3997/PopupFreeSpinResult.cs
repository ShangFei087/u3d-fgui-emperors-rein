using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupFreeSpinResult : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupFreeSpinResult";

        // 用的资源和免费触发的资源是一样的，所以路径不需要修改
        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinTrigger/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinTrigger/EffectPrefabs/";

        private const string AnimationPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinResult/AnimationPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private GButton _freeStartBtn;
        private GComponent _freeResultTipWindow;
        private GComponent _freeGameResultScore;

        // Spine
        private GameObject _dollarSpineObj;
        private GameObject _cloneDollarSpineObj;
        private GComponent _compareDollarSpineGCom;

        // Effect
        private GameObject _goldPurpleEffectObj, _lightEffectObj;
        private GameObject _cloneGoldPurpleEffectObj, _cloneLightEffectObj;
        private GComponent _compareGoldPurpleEffectGCom, _compareLightEffectGCom;

        // Animation
        private GameObject _freeGetAnimationObj;
        private GameObject _cloneFreeGetAnimationObj;
        private GComponent _compareFreeGetAnimationGCom;

        // 记录原始父节点，用于还原
        private Transform _freeGameResultScoreOriginalParent;
        private Vector3 _freeGameResultScoreOriginalPos;
        private Vector3 _freeGameResultScoreOriginalScale;
        private Transform _freeStartBtnOriginalParent;
        private Vector3 _freeStartBtnOriginalPos;
        private Vector3 _freeStartBtnOriginalScale;

        // 计时器回调
        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;

        private bool _isClicked = false;
        private EventData _openData;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 4;
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "dollarSpine.prefab",
                (clone) =>
                {
                    _dollarSpineObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "goldPurpleEffect.prefab",
                (clone) =>
                {
                    _goldPurpleEffectObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "lightEffect.prefab",
                (clone) =>
                {
                    _lightEffectObj = clone;
                    ResLoadedCallback();
                });

            // 加载Animation
            ResourceManager02.Instance.LoadAsset<GameObject>(
                AnimationPrefabPath + "freeGetAnimation.prefab",
                (clone) =>
                {
                    _freeGetAnimationObj = clone;
                    ResLoadedCallback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnClickSpinButton(res);
                    },
                }
            };
        }

        private void InitParam(EventData eventData)
        {
            if (eventData != null) _openData = eventData;
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;
            RemoveTimer(ref _autoClickCallback);

            // -------------------------- 获取UI组件 -----------------------
            _freeResultTipWindow = contentPane.GetChild("freeResultTipWindow").asCom;
            _freeStartBtn = _freeResultTipWindow.GetChild("collectBtn").asButton;
            _freeGameResultScore = _freeResultTipWindow.GetChild("freeGameResultScore").asCom;
            _freeGameResultScore.GetChild("number").asTextField.text =
                ContentModel.Instance.freeSpinTotalWinCoins.ToString();

            // -------------------------- 绑定Prefab到UI上 -----------------------
            GComponent currentGCom = contentPane.GetChild("dollarSpine").asCom;
            if (currentGCom != _compareDollarSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDollarSpineGCom);
                _compareDollarSpineGCom = currentGCom;
                _cloneDollarSpineObj = Object.Instantiate(_dollarSpineObj);
                _cloneDollarSpineObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDollarSpineGCom, _cloneDollarSpineObj);
            } // Spine

            currentGCom = contentPane.GetChild("lightEffect").asCom;
            if (currentGCom != _compareLightEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
                _compareLightEffectGCom = currentGCom;
                _cloneLightEffectObj = Object.Instantiate(_lightEffectObj);
                GameCommon.FguiUtils.AddWrapper(_compareLightEffectGCom, _cloneLightEffectObj);
            }

            currentGCom = contentPane.GetChild("goldPurpleEffect").asCom;
            if (currentGCom != _compareGoldPurpleEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareGoldPurpleEffectGCom);
                _compareGoldPurpleEffectGCom = currentGCom;
                _cloneGoldPurpleEffectObj = Object.Instantiate(_goldPurpleEffectObj);
                _cloneGoldPurpleEffectObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareGoldPurpleEffectGCom, _cloneGoldPurpleEffectObj);
            } // Effect

            currentGCom = _freeResultTipWindow.GetChild("freeGetAnimation").asCom;
            if (currentGCom != _compareFreeGetAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeGetAnimationGCom);
                _compareFreeGetAnimationGCom = currentGCom;
                _cloneFreeGetAnimationObj = Object.Instantiate(_freeGetAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareFreeGetAnimationGCom, _cloneFreeGetAnimationObj);
            } // Animation

            // ---------------------------------- 绑定UI到Animator上 ---------------------
            string candidatePaths = $"Anchor/sg_pop_settlement/Animation/numdi";
            Transform num01 = _cloneFreeGetAnimationObj.transform.Find(candidatePaths);
            GObject _gfreetxt = this.contentPane.GetChild("freeResultTipWindow").asCom.GetChild("freeGameResultScore");
            if (_gfreetxt?.displayObject?.gameObject != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;

                if (_freeGameResultScoreOriginalParent == null)
                {
                    _freeGameResultScoreOriginalParent = t.parent;
                    _freeGameResultScoreOriginalPos = t.localPosition;
                    _freeGameResultScoreOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-2.79f, 0.02f, 0);
                t.localScale = new Vector3(0.005f, 0.005f, 0.01f);
            } // freeGameResultScore 文本

            string startButtonPath = $"Anchor/sg_pop_settlement/Animation/btn";
            num01 = _cloneFreeGetAnimationObj.transform.Find(startButtonPath);
            GObject gStartBtn = this.contentPane.GetChild("freeResultTipWindow").asCom.GetChild("collectBtn");
            if (gStartBtn?.displayObject?.gameObject != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;

                if (_freeStartBtnOriginalParent == null)
                {
                    _freeStartBtnOriginalParent = t.parent;
                    _freeStartBtnOriginalPos = t.localPosition;
                    _freeStartBtnOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-1.34f, -0.33f, 0);
                t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
            } // freeStartBtn 按钮

            _freeStartBtn.onClick.Clear();
            _freeStartBtn.onClick.Add(() => OnClickSpinButton(_openData));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_freeStartBtn != null && _freeResultTipWindow != null &&
                        _freeResultTipWindow.visible && isOpen)
                    {
                        _freeStartBtn.onClick.Call();
                    }
                    _autoClickCallback = null;
                };
                Timers.inst.Add(3.0f, 1, _autoClickCallback);
            }
        }

        private void OnClickSpinButton(EventData eventData)
        {
            if (_isClicked) return;
            _isClicked = true;
            RemoveTimer(ref _delayCloseCallback);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeGameFadeTransition));
            _freeResultTipWindow.visible = false;
            _freeGameResultScore.visible = false;
            _cloneFreeGetAnimationObj.SetActive(false);
            _cloneDollarSpineObj.SetActive(true);
            _cloneGoldPurpleEffectObj.SetActive(true);

            _delayCloseCallback = (obj) =>
            {
                if (eventData is { value: Dictionary<string, object> args })
                {
                    Action changePage = args["changeNormalPage"] as Action;
                    changePage?.Invoke();
                }
                if (_cloneDollarSpineObj != null) _cloneDollarSpineObj.SetActive(false);
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(3.03f, 1, _delayCloseCallback);
        }

        private GameSoundController3997 _gameSoundController;

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmFreeSpinResult));
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmRegularGame));
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            // ------------------------ 复原UI显隐状态 ----------------------
            if (_freeResultTipWindow != null) _freeResultTipWindow.visible = true;
            if (_freeGameResultScore != null) _freeGameResultScore.visible = true;
            if (_cloneDollarSpineObj != null) _cloneDollarSpineObj.SetActive(false);
            if (_cloneGoldPurpleEffectObj != null) _cloneGoldPurpleEffectObj.SetActive(false);
            if (_cloneFreeGetAnimationObj != null) _cloneFreeGetAnimationObj.SetActive(true);

            // ------------------------ 复原UI父物体 ----------------------
            if (contentPane == null) return;
            var freeResultTipWindow = contentPane.GetChild("freeResultTipWindow")?.asCom;
            if (freeResultTipWindow == null) return;
            GObject _gfreetxt = freeResultTipWindow.GetChild("freeGameResultScore");
            if (_gfreetxt?.displayObject?.gameObject != null && _freeGameResultScoreOriginalParent != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;
                t.SetParent(_freeGameResultScoreOriginalParent, false);
                t.localPosition = _freeGameResultScoreOriginalPos;
                t.localScale = _freeGameResultScoreOriginalScale;
            }

            GObject gStartBtn = freeResultTipWindow.GetChild("collectBtn");
            if (gStartBtn?.displayObject?.gameObject != null && _freeStartBtnOriginalParent != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;
                t.SetParent(_freeStartBtnOriginalParent, false);
                t.localPosition = _freeStartBtnOriginalPos;
                t.localScale = _freeStartBtnOriginalScale;
            }
            // 防御性编程，避免操作已销毁或被回收的transform
            _freeGameResultScoreOriginalParent = null;
            _freeStartBtnOriginalParent = null;
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam(null);
            }
        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        /// <remarks>使用ref关键字按引用传递，使得方法内部对参数的置空操作能够直接反映到外部的成员变量上，避免悬挂引用</remarks>>
        /// <param name="timerCallback"></param>
        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;

            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}