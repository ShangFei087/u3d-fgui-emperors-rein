using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private bool _isInitialized = false;
        private GButton _freeStartBtn = null;
        private GComponent _freeTipWindow = null;

        // Spine
        private GameObject _dollarSpineObj = null;
        private GameObject _cloneDollarSpineObj = null;
        private GComponent _compareDollarSpineGCom = null;

        // Effect
        private GameObject /*_blueBoomEffectObj = null, */_goldPurpleEffectObj = null/*, _lightEffectObj = null*/;

        private GameObject /*_cloneBlueBoomEffectObj = null,*/
            _cloneGoldPurpleEffectObj = null
            /*_cloneLightEffectObj = null*/;

        private GComponent /*_compareBlueBoomEffectGCom = null,*/
            _compareGoldPurpleEffectGCom = null
            /*_compareLightEffectGCom = null*/;

        // Animation
        private GameObject _freeGetAnimationObj = null;
        private GameObject _cloneFreeGetAnimationObj = null;
        private GComponent _compareFreeGetAnimationGCom = null;
        
        // ========== 新增：记录原始父节点，用于还原 ==========
        private Transform _freeRoundOriginalParent = null;
        private Vector3 _freeRoundOriginalPos;
        private Vector3 _freeRoundOriginalScale;
        private Transform _freeStartBtnOriginalParent = null;
        private Vector3 _freeStartBtnOriginalPos;
        private Vector3 _freeStartBtnOriginalScale;
        // ========== 新增结束 ==========

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 3;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            ResetView();
            BindPrefabsToUI();
            BindUIToAnimator();
            ShowEffectAndSpine();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            _freeTipWindow = contentPane.GetChild("freeTipWindow").asCom;
            _freeTipWindow.visible = false;
            _freeStartBtn = _freeTipWindow.GetChild("freeStartBtn").asButton;

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
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "dollarSpine.prefab",
                (clone) =>
                {
                    _dollarSpineObj = clone;
                    ResLoadedCallback();
                });

            // 加载Effect
            // ResourceManager02.Instance.LoadAsset<GameObject>(
            //     EffectPrefabPath + "blueBoomEffect.prefab",
            //     (clone) =>
            //     {
            //         _blueBoomEffectObj = clone;
            //         ResLoadedCallback();
            //     });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "goldPurpleEffect.prefab",
                (clone) =>
                {
                    _goldPurpleEffectObj = clone;
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
            // currentGCom = contentPane.GetChild("blueBoomEffect").asCom;
            // if (currentGCom != _compareBlueBoomEffectGCom)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareBlueBoomEffectGCom);
            //     _compareBlueBoomEffectGCom = currentGCom;
            //     _cloneBlueBoomEffectObj = Object.Instantiate(_blueBoomEffectObj);
            //     GameCommon.FguiUtils.AddWrapper(_compareBlueBoomEffectGCom, _cloneBlueBoomEffectObj);
            // }

            // currentGCom = contentPane.GetChild("lightEffect").asCom;
            // if (currentGCom != _compareLightEffectGCom)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
            //     _compareLightEffectGCom = currentGCom;
            //     _cloneLightEffectObj = Object.Instantiate(_lightEffectObj);
            //     _cloneLightEffectObj.SetActive(false);
            //     GameCommon.FguiUtils.AddWrapper(_compareLightEffectGCom, _cloneLightEffectObj);
            // }

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
            currentGCom = _freeTipWindow.GetChild("freeGetAnimation").asCom;
            if (currentGCom != _compareFreeGetAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeGetAnimationGCom);
                _compareFreeGetAnimationGCom = currentGCom;
                _cloneFreeGetAnimationObj = Object.Instantiate(_freeGetAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareFreeGetAnimationGCom, _cloneFreeGetAnimationObj);
            }
        }

            private void BindUIToAnimator()
        {
            // ========== 修改：绑定前先记录原始状态，方便后续还原 ==========
            
            // freeRound 文本
            string candidatePaths = $"Anchor/fg_pop_prompt/Animation/all1/all3/num02";
            Transform num01 = _cloneFreeGetAnimationObj.transform.Find(candidatePaths);
            GObject _gfreetxt = this.contentPane.GetChild("freeTipWindow").asCom.GetChild("freeRound");
            if (_gfreetxt?.displayObject?.gameObject != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;
                
                // 记录原始父节点和变换（只在第一次绑定时记录）
                if (_freeRoundOriginalParent == null)
                {
                    _freeRoundOriginalParent = t.parent;
                    _freeRoundOriginalPos = t.localPosition;
                    _freeRoundOriginalScale = t.localScale;
                }
                
                t.SetParent(num01, false);
                t.localPosition = new Vector3(-2.6f, 2.33f, 0);
                t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
            }

            // freeStartBtn 按钮
            string startButtonPath = $"Anchor/fg_pop_prompt/Animation/all1/all/btn01";
            num01 = _cloneFreeGetAnimationObj.transform.Find(startButtonPath);
            GObject gStartBtn = this.contentPane.GetChild("freeTipWindow").asCom.GetChild("freeStartBtn");
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

        // private void BindUIToAnimator()
        // {
        //     //fgui放入ugui
        //     string candidatePaths = $"Anchor/fg_pop_prompt/Animation/all1/all3/num02";
        //     Transform num01 = _cloneFreeGetAnimationObj.transform.Find(candidatePaths);
        //     GObject _gfreetxt = this.contentPane.GetChild("freeTipWindow").asCom.GetChild("freeRound");
        //     if (_gfreetxt?.displayObject?.gameObject != null)
        //     {
        //         Transform t = _gfreetxt.displayObject.gameObject.transform;
        //         t.SetParent(num01, false);
        //         t.localPosition = new Vector3(-2.6f, 2.33f, 0);
        //         //t.localRotation = Quaternion.identity;
        //         t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
        //     }
        //
        //     string startButtonPath = $"Anchor/fg_pop_prompt/Animation/all1/all/btn01";
        //     num01 = _cloneFreeGetAnimationObj.transform.Find(startButtonPath);
        //     GObject gStartBtn = this.contentPane.GetChild("freeTipWindow").asCom.GetChild("freeStartBtn");
        //     if (gStartBtn?.displayObject?.gameObject != null)
        //     {
        //         Transform t = gStartBtn.displayObject.gameObject.transform;
        //         t.SetParent(num01, false);
        //         t.localPosition = new Vector3(-1.34f, -0.33f, 0);
        //         //t.localRotation = Quaternion.identity;
        //         t.localScale = new Vector3(0.008f, 0.008f, 0.01f);
        //     }
        // }
        
        // ========== 新增：还原 FGUI UI 元素到原始父节点 ==========
        private void RestoreUIElements()
        {
            GObject _gfreetxt = this.contentPane?.GetChild("freeTipWindow")?.asCom?.GetChild("freeRound");
            if (_gfreetxt?.displayObject?.gameObject != null && _freeRoundOriginalParent != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;
                t.SetParent(_freeRoundOriginalParent, false);
                t.localPosition = _freeRoundOriginalPos;
                t.localScale = _freeRoundOriginalScale;
            }

            GObject gStartBtn = this.contentPane?.GetChild("freeTipWindow")?.asCom?.GetChild("freeStartBtn");
            if (gStartBtn?.displayObject?.gameObject != null && _freeStartBtnOriginalParent != null)
            {
                Transform t = gStartBtn.displayObject.gameObject.transform;
                t.SetParent(_freeStartBtnOriginalParent, false);
                t.localPosition = _freeStartBtnOriginalPos;
                t.localScale = _freeStartBtnOriginalScale;
            }
        }
        // ========== 新增结束 ==========

        private void ShowEffectAndSpine()
        {
            // Timers.inst.Add(1f, 1, (obj) =>
            // {
            //     // _cloneBlueBoomEffectObj.SetActive(false);
            //     // _cloneLightEffectObj.SetActive(true);
            // });
            // _freeTipWindow.visible = true;
            // _cloneLightEffectObj.SetActive(true);
            _freeTipWindow.visible = true;

            _freeStartBtn.onClick.Add((() =>
            {
                _freeTipWindow.visible = false;
                _cloneDollarSpineObj.SetActive(true);
                _cloneGoldPurpleEffectObj.SetActive(true);

                // Timers.inst.Add(3, 1, (obj) => _cloneDollarSpineObj.SetActive(false));
                Timers.inst.Add(3.033f, 1, (obj) =>
                {
                    _cloneDollarSpineObj.SetActive(false);
                    CloseSelf(null);
                });
            }));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                Timers.inst.Add(3.0f, 1, (obj) =>
                {
                    if (_freeStartBtn != null && _freeTipWindow != null && _freeTipWindow.visible)
                    {
                        _freeStartBtn.onClick.Call();
                    }
                });
            }
        }

        private void ResetView()
        {
            RestoreUIElements();
            GameCommon.FguiUtils.DeleteWrapper(_compareDollarSpineGCom);
            // GameCommon.FguiUtils.DeleteWrapper(_compareBlueBoomEffectGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareGoldPurpleEffectGCom);
            // GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareFreeGetAnimationGCom);

            _compareDollarSpineGCom = null;
            // _compareBlueBoomEffectGCom = null;
            _compareGoldPurpleEffectGCom = null;
            // _compareLightEffectGCom = null;
            _compareFreeGetAnimationGCom = null;

            Object.Destroy(_cloneDollarSpineObj);
            // Object.Destroy(_cloneBlueBoomEffectObj);
            Object.Destroy(_cloneGoldPurpleEffectObj);
            // Object.Destroy(_cloneLightEffectObj);
            Object.Destroy(_cloneFreeGetAnimationObj);

            _cloneDollarSpineObj = null;
            // _cloneBlueBoomEffectObj = null;
            _cloneGoldPurpleEffectObj = null;
            _cloneFreeGetAnimationObj = null;
            // _cloneLightEffectObj = null;
        }
    }
}