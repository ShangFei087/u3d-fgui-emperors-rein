using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinTrigger/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinTrigger/EffectPrefabs/";

        private const string AnimationPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinTrigger/AnimationPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized;
        private GButton _freeStartBtn;
        private GComponent _freeTipWindow;

        // Spine
        private GameObject _dollarSpineObj;
        private GameObject _cloneDollarSpineObj;
        private GComponent _compareDollarSpineGCom;

        // Effect
        private GameObject _goldPurpleEffectObj;
        private GameObject _cloneGoldPurpleEffectObj;
        private GComponent _compareGoldPurpleEffectGCom;

        // Animation
        private GameObject _freeGetAnimationObj;
        private GameObject _cloneFreeGetAnimationObj;
        private GComponent _compareFreeGetAnimationGCom;

        // 记录UI父物体，方便界面关闭的时候还原UI位置
        private Transform _freeRoundOriginalParent = null;
        private Vector3 _freeRoundOriginalPos;
        private Vector3 _freeRoundOriginalScale;
        private Transform _freeStartBtnOriginalParent = null;
        private Vector3 _freeStartBtnOriginalPos;
        private Vector3 _freeStartBtnOriginalScale;

        // 定时器记录，方便使用后清除，避免内存泄漏
        private TimerCallback _autoClickCallback;
        private TimerCallback _delayCloseCallback;

        private GameSoundController3997 _gameSoundController;
        private bool _isClicked = false;
        private EventData _openData;
        private Action _changeFreePage;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 3;
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "dollarSpine.prefab",
                (clone) =>
                {
                    _dollarSpineObj = clone;
                    ResLoadedCallback();
                }); // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "goldPurpleEffect.prefab",
                (clone) =>
                {
                    _goldPurpleEffectObj = clone;
                    ResLoadedCallback();
                }); // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(
                AnimationPrefabPath + "freeGetAnimation.prefab",
                (clone) =>
                {
                    _freeGetAnimationObj = clone;
                    ResLoadedCallback();
                }); // 加载Animation

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

            // --------------------- 获取UI组件 ------------------------
            _freeTipWindow = contentPane.GetChild("freeTipWindow").asCom;
            _freeStartBtn = _freeTipWindow.GetChild("freeStartBtn").asButton;

            // --------------------- 绑定预制体到UI ---------------------
            GComponent currentGCom = contentPane.GetChild("dollarSpine").asCom;
            if (currentGCom != _compareDollarSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDollarSpineGCom);
                _compareDollarSpineGCom = currentGCom;
                _cloneDollarSpineObj = Object.Instantiate(_dollarSpineObj);
                _cloneDollarSpineObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDollarSpineGCom, _cloneDollarSpineObj);
            } // Spine

            currentGCom = contentPane.GetChild("goldPurpleEffect").asCom;
            if (currentGCom != _compareGoldPurpleEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareGoldPurpleEffectGCom);
                _compareGoldPurpleEffectGCom = currentGCom;
                _cloneGoldPurpleEffectObj = Object.Instantiate(_goldPurpleEffectObj);
                _cloneGoldPurpleEffectObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareGoldPurpleEffectGCom, _cloneGoldPurpleEffectObj);
            } // Effect

            currentGCom = _freeTipWindow.GetChild("freeGetAnimation").asCom;
            if (currentGCom != _compareFreeGetAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeGetAnimationGCom);
                _compareFreeGetAnimationGCom = currentGCom;
                _cloneFreeGetAnimationObj = Object.Instantiate(_freeGetAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareFreeGetAnimationGCom, _cloneFreeGetAnimationObj);
            } // Animation

            // --------------------- 绑定UI到动画 ---------------------
            string candidatePaths = $"Anchor/fg_pop_prompt/Animation/all1/all3/num02";
            Transform num01 = _cloneFreeGetAnimationObj.transform.Find(candidatePaths);
            GObject gFreeText = contentPane.GetChild("freeTipWindow").asCom.GetChild("freeRound");
            if (gFreeText?.displayObject?.gameObject != null)
            {
                Transform t = gFreeText.displayObject.gameObject.transform;
                if (_freeRoundOriginalParent == null)
                {
                    _freeRoundOriginalParent = t.parent;
                    _freeRoundOriginalPos = t.localPosition;
                    _freeRoundOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-2.6f, 2.33f, 0);
                t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
            } // freeRound 文本

            string startButtonPath = $"Anchor/fg_pop_prompt/Animation/all1/all/btn01";
            num01 = _cloneFreeGetAnimationObj.transform.Find(startButtonPath);
            GObject gStartBtn = contentPane.GetChild("freeTipWindow").asCom.GetChild("freeStartBtn");
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

            // --------------------- 按钮点击事件 ---------------------
            if (_openData is { value: Dictionary<string, object> args })
            {
                _changeFreePage = args["changeFreePage"] as Action;
            }
            _freeStartBtn.onClick.Clear();
            _freeStartBtn.onClick.Add(() => { OnClickSpinButton(null); });
            // 自动模式定时器
            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_freeStartBtn != null && _freeTipWindow != null && _freeTipWindow.visible && isOpen)
                    {
                        _freeStartBtn.onClick.Call();
                    }

                    _autoClickCallback = null;
                };
                Timers.inst.Add(3.0f, 1, _autoClickCallback);
            }
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmFreeSpinTrigger));
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _changeFreePage = null;

            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            // ----------------------- 重置UI和clone物体显隐 ---------------------
            if (_freeTipWindow != null) _freeTipWindow.visible = true;
            if (_cloneDollarSpineObj != null) _cloneDollarSpineObj.SetActive(false);
            if (_cloneGoldPurpleEffectObj != null) _cloneGoldPurpleEffectObj.SetActive(false);

            // ----------------------- 重置UI父物体 -----------------------
            GObject gFreeText = contentPane?.GetChild("freeTipWindow")?.asCom?.GetChild("freeRound");
            if (gFreeText?.displayObject?.gameObject != null && _freeRoundOriginalParent != null)
            {
                Transform t = gFreeText.displayObject.gameObject.transform;
                t.SetParent(_freeRoundOriginalParent, false);
                t.localPosition = _freeRoundOriginalPos;
                t.localScale = _freeRoundOriginalScale;
            }

            GObject gStartBtn = contentPane?.GetChild("freeTipWindow")?.asCom?.GetChild("freeStartBtn");
            if (gStartBtn?.displayObject?.gameObject != null && _freeStartBtnOriginalParent != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;
                t.SetParent(_freeStartBtnOriginalParent, false);
                t.localPosition = _freeStartBtnOriginalPos;
                t.localScale = _freeStartBtnOriginalScale;
            }

            _freeRoundOriginalParent = null;
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

        private void OnClickSpinButton(EventData res)
        {
            if (_isClicked) return;
            _isClicked = true;

            RemoveTimer(ref _delayCloseCallback);

            _freeTipWindow.visible = false;
            if (_cloneDollarSpineObj != null)
                _cloneDollarSpineObj.SetActive(true);
            if (_cloneGoldPurpleEffectObj != null)
                _cloneGoldPurpleEffectObj.SetActive(true);

            _delayCloseCallback = (obj) =>
            {
                _changeFreePage?.Invoke();
                if (_cloneDollarSpineObj != null) _cloneDollarSpineObj.SetActive(false);
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(3.033f, 1, _delayCloseCallback);

            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeGameFadeTransition));
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;

            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}