using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupJackpotTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupJackpotTrigger";

        // 用的资源和免费触发的资源是一样的，所以路径不需要修改
        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/EffectPrefabs/";

        private const string AnimationPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/AnimationPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private GButton _jackpotTriggerButton = null;
        private GComponent _jackpotTriggerTipWindow = null;

        // Spine
        private GameObject _diamondSpineObj = null;
        private GameObject _cloneDiamondSpineObj = null;
        private GComponent _compareDiamondSpineGCom = null;

        // Effect
        private GameObject _diamondBgEffectObj = null /*, _lightEffectObj = null*/;
        private GameObject _cloneDiamondBgEffectObj = null /*, _cloneLightEffectObj = null*/;
        private GComponent _compareDiamondBgEffectGCom = null /*, _compareLightEffectGCom = null*/;

        // Todo：等Animation做出来之后，直接取消注释即可
        // Animation
        private GameObject _diamondAnimationObj = null;
        private GameObject _cloneDiamondAnimationObj = null;
        private GComponent _compareDiamondAnimationGCom = null;

        // ========== 新增：记录原始父节点，用于还原 ==========
        private Transform _jackpotTriggerButtonOriginalParent = null;
        private Vector3 _jackpotTriggerButtonOriginalPos;
        private Vector3 _jackpotTriggerButtonOriginalScale;
        // ========== 新增结束 ==========

        private bool _isClicked = false;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 3; //4
            LoadAsyncRes();

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;
                        if (!isReady) return;
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnClickSpinButton(res);
                    },
                }
            };
        }

        private void OnClickSpinButton(EventData res)
        {
            if(_isClicked)return;
            _isClicked = true;
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));
            _jackpotTriggerTipWindow.visible = false;
            _cloneDiamondAnimationObj.SetActive(false);

            _cloneDiamondSpineObj.SetActive(true);
            _cloneDiamondBgEffectObj.SetActive(true);
            Timers.inst.Add(3, 1, (obj) => _cloneDiamondSpineObj.SetActive(false));
            Timers.inst.Add(5, 1, (obj) =>
            {
                PageManager.Instance.ClosePage(PageName.CaiFuZhiJiaPageGameMain);
                PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupJackpotGame, creditEventData);
            });
            Timers.inst.Add(7, 1, (obj) =>
            {
                CloseSelf(null);
            });
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;
            _jackpotTriggerTipWindow = contentPane.GetChild("jackpotTriggerTipWindow").asCom;
            _jackpotTriggerButton = _jackpotTriggerTipWindow.GetChild("jackpotTriggerButton").asButton;
            BindPrefabsToUI();
            BindUIToAnimator();
            ShowEffectAndSpine();
            isReady = true;
        }

        private GameSoundController3997 _gameSoundController;
        private EventData creditEventData;

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmBonusTrigger));

            // PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotGame, null);
            creditEventData = eventData;
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _gameSoundController?.Dispose();
            _gameSoundController = null;
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
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "diamondSpine.prefab",
                (clone) =>
                {
                    _diamondSpineObj = clone;
                    ResLoadedCallback();
                });

            // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "diamondBgEffect.prefab",
                (clone) =>
                {
                    _diamondBgEffectObj = clone;
                    ResLoadedCallback();
                });

            // ResourceManager02.Instance.LoadAsset<GameObject>(
            //     EffectPrefabPath + "lightEffect.prefab",
            //     (clone) =>
            //     {
            //         _lightEffectObj = clone;
            //         ResLoadedCallback();
            //     });

            // 加载Animation
            ResourceManager02.Instance.LoadAsset<GameObject>(
                AnimationPrefabPath + "diamondAnimation.prefab",
                (clone) =>
                {
                    _diamondAnimationObj = clone;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            // Spine
            GComponent currentGCom = contentPane.GetChild("diamondSpine").asCom;
            if (currentGCom != _compareDiamondSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondSpineGCom);
                _compareDiamondSpineGCom = currentGCom;
                _cloneDiamondSpineObj = Object.Instantiate(_diamondSpineObj);
                _cloneDiamondSpineObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondSpineGCom, _cloneDiamondSpineObj);
            }

            // Effect
            currentGCom = contentPane.GetChild("diamondBgEffect").asCom;
            if (currentGCom != _compareDiamondBgEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondBgEffectGCom);
                _compareDiamondBgEffectGCom = currentGCom;
                _cloneDiamondBgEffectObj = Object.Instantiate(_diamondBgEffectObj);
                _cloneDiamondBgEffectObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondBgEffectGCom, _cloneDiamondBgEffectObj);
            }

            // currentGCom = contentPane.GetChild("lightEffect").asCom;
            // if (currentGCom != _compareLightEffectGCom)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
            //     _compareLightEffectGCom = currentGCom;
            //     _cloneLightEffectObj = Object.Instantiate(_lightEffectObj);
            //     GameCommon.FguiUtils.AddWrapper(_compareLightEffectGCom, _cloneLightEffectObj);
            // }

            // Animation
            currentGCom = _jackpotTriggerTipWindow.GetChild("diamondAnimation").asCom;
            if (currentGCom != _compareDiamondAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondAnimationGCom);
                _compareDiamondAnimationGCom = currentGCom;
                _cloneDiamondAnimationObj = Object.Instantiate(_diamondAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondAnimationGCom, _cloneDiamondAnimationObj);
            }
        }

        private void ShowEffectAndSpine()
        {
            
            _jackpotTriggerButton.onClick.Add((() =>
            {
                OnClickSpinButton(null);
            }));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                Timers.inst.Add(0.3f, 1, (obj) =>
                {
                    if (_jackpotTriggerButton != null && _jackpotTriggerTipWindow != null &&
                        _jackpotTriggerTipWindow.visible)
                    {
                        _jackpotTriggerButton.onClick.Call();
                    }
                });
            }
        }
        
        private void BindUIToAnimator()
        {
            // ========== 修改：绑定前先记录原始状态，方便后续还原 ==========

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
        }

        // ========== 新增：还原 FGUI UI 元素到原始父节点 ==========
        private void RestoreUIElements()
        {
            if (contentPane == null) return;
            var jackpotTriggerTipWindow = contentPane.GetChild("jackpotTriggerTipWindow")?.asCom;
            if (jackpotTriggerTipWindow == null) return;

            GObject gStartBtn = jackpotTriggerTipWindow.GetChild("jackpotTriggerButton");
            if (gStartBtn?.displayObject?.gameObject != null && _jackpotTriggerButtonOriginalParent != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;
                t.SetParent(_jackpotTriggerButtonOriginalParent, false);
                t.localPosition = _jackpotTriggerButtonOriginalPos;
                t.localScale = _jackpotTriggerButtonOriginalScale;
            }
        }
        // ========== 新增结束 ==========

        private void ResetView()
        {
            RestoreUIElements();
            _jackpotTriggerTipWindow.visible = true;

            GameCommon.FguiUtils.DeleteWrapper(_compareDiamondSpineGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareDiamondBgEffectGCom);
            // GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareDiamondAnimationGCom);

            _compareDiamondSpineGCom = null;
            _compareDiamondBgEffectGCom = null;
            // _compareLightEffectGCom = null;
            _compareDiamondAnimationGCom = null;

            Object.Destroy(_cloneDiamondSpineObj);
            Object.Destroy(_cloneDiamondBgEffectObj);
            // Object.Destroy(_cloneLightEffectObj);
            Object.Destroy(_cloneDiamondAnimationObj);

            _cloneDiamondSpineObj = null;
            _cloneDiamondBgEffectObj = null;
            // _cloneLightEffectObj = null;
            _cloneDiamondAnimationObj = null;
        }
    }
}