using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
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
        private GButton _freeStartBtn = null;
        private GComponent _freeResultTipWindow = null;
        private GComponent _freeGameResultScore = null;

        // Spine
        private GameObject _dollarSpineObj = null;
        private GameObject _cloneDollarSpineObj = null;
        private GComponent _compareDollarSpineGCom = null;

        // Effect
        private GameObject _goldPurpleEffectObj = null, _lightEffectObj = null;
        private GameObject _cloneGoldPurpleEffectObj = null, _cloneLightEffectObj = null;
        private GComponent _compareGoldPurpleEffectGCom = null, _compareLightEffectGCom = null;

        // Animation
        private GameObject _freeGetAnimationObj = null;
        private GameObject _cloneFreeGetAnimationObj = null;
        private GComponent _compareFreeGetAnimationGCom = null;

        // ========== 新增：记录原始父节点，用于还原 ==========
        private Transform _freeGameResultScoreOriginalParent = null;
        private Vector3 _freeGameResultScoreOriginalPos;
        private Vector3 _freeGameResultScoreOriginalScale;
        private Transform _freeStartBtnOriginalParent = null;
        private Vector3 _freeStartBtnOriginalPos;
        private Vector3 _freeStartBtnOriginalScale;
        // ========== 新增结束 ==========

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 4;
            LoadAsyncRes();
            
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
        
        private void OnClickSpinButton(EventData res)
        {
            _freeStartBtn.onClick.Add((() =>
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData(SlotMachineEvent.FreeGameFadeTransition));
                _freeResultTipWindow.visible = false;
                _freeGameResultScore.visible = false;
                _cloneFreeGetAnimationObj.SetActive(false);
                _cloneDollarSpineObj.SetActive(true);
                _cloneGoldPurpleEffectObj.SetActive(true);

                Timers.inst.Add(3.03f, 1, (obj) =>
                {
                    _cloneDollarSpineObj.SetActive(false);
                    CloseSelf(null);

                    // 新增测试
                    // PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupFreeSpinTrigger);
                });
            }));
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            BindPrefabsToUI();
            BindUIToAnimator();
            ShowEffectAndSpine();
        }

        private GameSoundController3997 _gameSoundController;
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            _freeResultTipWindow = contentPane.GetChild("freeResultTipWindow").asCom;
            _freeStartBtn = _freeResultTipWindow.GetChild("freeStartBtn").asButton;
            _freeGameResultScore = _freeResultTipWindow.GetChild("freeGameResultScore").asCom;
            _freeGameResultScore.GetChild("number").asTextField.text =
                ContentModel.Instance.freeSpinTotalWinCoins.ToString();
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmFreeSpinResult));
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmRegularGame));
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
        }

        private void BindPrefabsToUI()
        {
            // Spine
            GComponent currentGCom = contentPane.GetChild("dollarSpine").asCom;
            if (currentGCom != _compareDollarSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDollarSpineGCom);
                _compareDollarSpineGCom = currentGCom;
                _cloneDollarSpineObj = Object.Instantiate(_dollarSpineObj);
                _cloneDollarSpineObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDollarSpineGCom, _cloneDollarSpineObj);
            }

            // Effect
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
            }

            // Animation
            currentGCom = _freeResultTipWindow.GetChild("freeGetAnimation").asCom;
            if (currentGCom != _compareFreeGetAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeGetAnimationGCom);
                _compareFreeGetAnimationGCom = currentGCom;
                _cloneFreeGetAnimationObj = Object.Instantiate(_freeGetAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareFreeGetAnimationGCom, _cloneFreeGetAnimationObj);
            }
        }

        private void ShowEffectAndSpine()
        {
            _freeStartBtn.onClick.Add((() =>
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData(SlotMachineEvent.FreeGameFadeTransition));
                _freeResultTipWindow.visible = false;
                _freeGameResultScore.visible = false;
                _cloneFreeGetAnimationObj.SetActive(false);
                _cloneDollarSpineObj.SetActive(true);
                _cloneGoldPurpleEffectObj.SetActive(true);

                Timers.inst.Add(3.03f, 1, (obj) =>
                {
                    _cloneDollarSpineObj.SetActive(false);
                    CloseSelf(null);

                    // 新增测试
                    // PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupFreeSpinTrigger);
                });
            }));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                Timers.inst.Add(0.3f, 1, (obj) =>
                {
                    if (_freeStartBtn != null && _freeResultTipWindow != null && _freeResultTipWindow.visible)
                    {
                        _freeStartBtn.onClick.Call();
                    }
                });
            }
        }

        // private void BindUIToAnimator()
        // {
        //     //fgui放入ugui
        //     string candidatePaths = $"Anchor/sg_pop_settlement/Animation/numdi";
        //     Transform num01 = _cloneFreeGetAnimationObj.transform.Find(candidatePaths);
        //     GObject _gfreetxt = this.contentPane.GetChild("freeResultTipWindow").asCom.GetChild("freeGameResultScore");
        //     if (_gfreetxt?.displayObject?.gameObject != null)
        //     {
        //         Transform t = _gfreetxt.displayObject.gameObject.transform;
        //         t.SetParent(num01, false);
        //         t.localPosition = new Vector3(-2.79f, 0.02f, 0);
        //         //t.localRotation = Quaternion.identity;
        //         t.localScale = new Vector3(0.005f, 0.005f, 0.01f);
        //     }
        //
        //     string startButtonPath = $"Anchor/sg_pop_settlement/Animation/btn";
        //     num01 = _cloneFreeGetAnimationObj.transform.Find(startButtonPath);
        //     GObject gStartBtn = this.contentPane.GetChild("freeResultTipWindow").asCom.GetChild("freeStartBtn");
        //     if (gStartBtn?.displayObject?.gameObject != null)
        //     {
        //         Transform t = gStartBtn.displayObject.gameObject.transform;
        //         t.SetParent(num01, false);
        //         t.localPosition = new Vector3(-1.34f, -0.33f, 0);
        //         //t.localRotation = Quaternion.identity;
        //         t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
        //     }
        // }

        private void BindUIToAnimator()
        {
            // ========== 修改：绑定前先记录原始状态，方便后续还原 ==========

            // freeGameResultScore 文本
            string candidatePaths = $"Anchor/sg_pop_settlement/Animation/numdi";
            Transform num01 = _cloneFreeGetAnimationObj.transform.Find(candidatePaths);
            GObject _gfreetxt = this.contentPane.GetChild("freeResultTipWindow").asCom.GetChild("freeGameResultScore");
            if (_gfreetxt?.displayObject?.gameObject != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;

                // 记录原始父节点和变换（只在第一次绑定时记录）
                if (_freeGameResultScoreOriginalParent == null)
                {
                    _freeGameResultScoreOriginalParent = t.parent;
                    _freeGameResultScoreOriginalPos = t.localPosition;
                    _freeGameResultScoreOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-2.79f, 0.02f, 0);
                t.localScale = new Vector3(0.005f, 0.005f, 0.01f);
            }

            // freeStartBtn 按钮
            string startButtonPath = $"Anchor/sg_pop_settlement/Animation/btn";
            num01 = _cloneFreeGetAnimationObj.transform.Find(startButtonPath);
            GObject gStartBtn = this.contentPane.GetChild("freeResultTipWindow").asCom.GetChild("freeStartBtn");
            if (gStartBtn?.displayObject?.gameObject != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;

                // 记录原始父节点和变换
                if (_freeStartBtnOriginalParent == null)
                {
                    _freeStartBtnOriginalParent = t.parent;
                    _freeStartBtnOriginalPos = t.localPosition;
                    _freeStartBtnOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-1.34f, -0.33f, 0);
                t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
            }
        }

        // ========== 新增：还原 FGUI UI 元素到原始父节点 ==========
        private void RestoreUIElements()
        {
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

            GObject gStartBtn = freeResultTipWindow.GetChild("freeStartBtn");
            if (gStartBtn?.displayObject?.gameObject != null && _freeStartBtnOriginalParent != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;
                t.SetParent(_freeStartBtnOriginalParent, false);
                t.localPosition = _freeStartBtnOriginalPos;
                t.localScale = _freeStartBtnOriginalScale;
            }
        }
        // ========== 新增结束 ==========

        private void ResetView()
        {
            RestoreUIElements();

            _freeResultTipWindow.visible = true;
            _freeGameResultScore.visible = true;

            GameCommon.FguiUtils.DeleteWrapper(_compareDollarSpineGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareGoldPurpleEffectGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareFreeGetAnimationGCom);

            _compareDollarSpineGCom = null;
            _compareGoldPurpleEffectGCom = null;
            _compareLightEffectGCom = null;
            _compareFreeGetAnimationGCom = null;


            Object.Destroy(_cloneDollarSpineObj);
            Object.Destroy(_cloneGoldPurpleEffectObj);
            Object.Destroy(_cloneLightEffectObj);
            Object.Destroy(_cloneFreeGetAnimationObj);

            _cloneDollarSpineObj = null;
            _cloneGoldPurpleEffectObj = null;
            _cloneLightEffectObj = null;
            _cloneFreeGetAnimationObj = null;
        }
    }
}