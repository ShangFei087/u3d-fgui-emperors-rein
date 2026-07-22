using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupSmallGameTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupSmallGameTrigger";

        // 用的资源和免费触发的资源是一样的，所以路径不需要修改
        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/EffectPrefabs/";

        private const string AnimationPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/AnimationPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private GButton _jackpotTriggerButton;
        private GComponent _jackpotTriggerTipWindow;

        // Spine
        private GameObject _diamondSpineObj;
        private GameObject _cloneDiamondSpineObj;
        private GComponent _compareDiamondSpineGCom;

        // Effect
        private GameObject _diamondBgEffectObj;
        private GameObject _cloneDiamondBgEffectObj;
        private GComponent _compareDiamondBgEffectGCom;

        // Animation
        private GameObject _diamondAnimationObj;
        private GameObject _cloneDiamondAnimationObj;
        private GComponent _compareDiamondAnimationGCom;

        private Transform _jackpotTriggerButtonOriginalParent;
        private Vector3 _jackpotTriggerButtonOriginalPos;
        private Vector3 _jackpotTriggerButtonOriginalScale;

        private bool _isClicked = false;
        private EventData _openData;
        private GameSoundController3997 _gameSoundController;

        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;

        private Action _changeSmallGamePage;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 3;
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "diamondSpine.prefab",
                (clone) =>
                {
                    _diamondSpineObj = clone;
                    ResLoadedCallback();
                }); // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "diamondBgEffect.prefab",
                (clone) =>
                {
                    _diamondBgEffectObj = clone;
                    ResLoadedCallback();
                }); // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(
                AnimationPrefabPath + "diamondAnimation.prefab",
                (clone) =>
                {
                    _diamondAnimationObj = clone;
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

            // ------------------ 获取UI组件 -----------------------
            _jackpotTriggerTipWindow = contentPane.GetChild("jackpotTriggerTipWindow").asCom;
            _jackpotTriggerButton = _jackpotTriggerTipWindow.GetChild("jackpotTriggerButton").asButton;

            // ------------------ 绑定prefab到UI上 -----------------------
            GComponent currentGCom = contentPane.GetChild("diamondSpine").asCom;
            if (currentGCom != _compareDiamondSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondSpineGCom);
                _compareDiamondSpineGCom = currentGCom;
                _cloneDiamondSpineObj = Object.Instantiate(_diamondSpineObj);
                _cloneDiamondSpineObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondSpineGCom, _cloneDiamondSpineObj);
            } // Spine

            currentGCom = contentPane.GetChild("diamondBgEffect").asCom;
            if (currentGCom != _compareDiamondBgEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondBgEffectGCom);
                _compareDiamondBgEffectGCom = currentGCom;
                _cloneDiamondBgEffectObj = Object.Instantiate(_diamondBgEffectObj);
                _cloneDiamondBgEffectObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondBgEffectGCom, _cloneDiamondBgEffectObj);
            } // Effect

            currentGCom = _jackpotTriggerTipWindow.GetChild("diamondAnimation").asCom;
            if (currentGCom != _compareDiamondAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondAnimationGCom);
                _compareDiamondAnimationGCom = currentGCom;
                _cloneDiamondAnimationObj = Object.Instantiate(_diamondAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondAnimationGCom, _cloneDiamondAnimationObj);
            } // Animation

            // -------------------------- 绑定UI到Animator --------------------------
            string parentPath = $"Anchor/sg_pop_prompt/Animation/btn";
            Transform num01 = _cloneDiamondAnimationObj.transform.Find(parentPath);
            GObject gStartBtn = this.contentPane.GetChild("jackpotTriggerTipWindow").asCom
                .GetChild("jackpotTriggerButton");
            if (gStartBtn?.displayObject?.gameObject != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;

                if (_jackpotTriggerButtonOriginalParent == null)
                {
                    _jackpotTriggerButtonOriginalParent = t.parent;
                    _jackpotTriggerButtonOriginalPos = t.localPosition;
                    _jackpotTriggerButtonOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-1.76f, 0.34f, 0);
                t.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }

            // -------------------------- 添加UI点击事件 --------------------------
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changeSmallGamePage = args["changeSmallGamePage"] as Action;
            }
            _jackpotTriggerButton.onClick.Clear();
            _jackpotTriggerButton.onClick.Add(() => OnClickSpinButton(null));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_jackpotTriggerButton != null && _jackpotTriggerTipWindow != null &&
                        _jackpotTriggerTipWindow.visible && isOpen)
                    {
                        _jackpotTriggerButton.onClick.Call();
                    }

                    _autoClickCallback = null;
                };
                Timers.inst.Add(3.0f, 1, _autoClickCallback);
            }
        }

        private void OnClickSpinButton(EventData res)
        {
            if (_isClicked) return;
            _isClicked = true;

            RemoveTimer(ref _delayCloseCallback);

            _jackpotTriggerTipWindow.visible = false;
            _cloneDiamondAnimationObj.SetActive(false);

            _cloneDiamondSpineObj.SetActive(true);
            _cloneDiamondBgEffectObj.SetActive(true);

            _delayCloseCallback = (obj) =>
            {
                _changeSmallGamePage?.Invoke();
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(5, 1, _delayCloseCallback);

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmBonusTrigger));

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _changeSmallGamePage = null;

            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            _jackpotTriggerTipWindow.visible = true;
            _cloneDiamondAnimationObj.SetActive(true);
            _cloneDiamondSpineObj.SetActive(false);
            _cloneDiamondBgEffectObj.SetActive(false);

            GObject gStartBtn = _jackpotTriggerTipWindow.GetChild("jackpotTriggerButton");
            if (gStartBtn?.displayObject?.gameObject != null && _jackpotTriggerButtonOriginalParent != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;
                t.SetParent(_jackpotTriggerButtonOriginalParent, false);
                t.localPosition = _jackpotTriggerButtonOriginalPos;
                t.localScale = _jackpotTriggerButtonOriginalScale;
            }

            _jackpotTriggerButtonOriginalParent = null;
        }

        private void ResLoadedCallback(EventData eventData = null)
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam(eventData);
            }
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;

            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}