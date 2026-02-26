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
        private GameObject _blueBoomEffectObj = null, _goldPurpleEffectObj = null, _lightEffectObj = null;

        private GameObject _cloneBlueBoomEffectObj = null,
            _cloneGoldPurpleEffectObj = null,
            _cloneLightEffectObj = null;

        private GComponent _compareBlueBoomEffectGCom = null,
            _compareGoldPurpleEffectGCom = null,
            _compareLightEffectGCom = null;

        // Animation
        private GameObject _freeGetAnimationObj = null;
        private GameObject _cloneFreeGetAnimationObj = null;
        private GComponent _compareFreeGetAnimationGCom = null;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 5;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            
            ResetView();
            BindPrefabsToUI();
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
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "blueBoomEffect.prefab",
                (clone) =>
                {
                    _blueBoomEffectObj = clone;
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
            currentGCom = contentPane.GetChild("blueBoomEffect").asCom;
            if (currentGCom != _compareBlueBoomEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBlueBoomEffectGCom);
                _compareBlueBoomEffectGCom = currentGCom;
                _cloneBlueBoomEffectObj = Object.Instantiate(_blueBoomEffectObj);
                GameCommon.FguiUtils.AddWrapper(_compareBlueBoomEffectGCom, _cloneBlueBoomEffectObj);
            }

            currentGCom = contentPane.GetChild("lightEffect").asCom;
            if (currentGCom != _compareLightEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
                _compareLightEffectGCom = currentGCom;
                _cloneLightEffectObj = Object.Instantiate(_lightEffectObj);
                _cloneLightEffectObj.SetActive(false);
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
            currentGCom = _freeTipWindow.GetChild("freeGetAnimation").asCom;
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
            Timers.inst.Add(1f, 1, (obj) =>
            {
                _cloneBlueBoomEffectObj.SetActive(false);
                _freeTipWindow.visible = true;
                _cloneLightEffectObj.SetActive(true);
            });

            _freeStartBtn.onClick.Add((() =>
            {
                _freeTipWindow.visible = false;
                _cloneDollarSpineObj.SetActive(true);
                _cloneGoldPurpleEffectObj.SetActive(true);

                Timers.inst.Add(3, 1, (obj) => _cloneDollarSpineObj.SetActive(false));
                Timers.inst.Add(7, 1, (obj) => CloseSelf(null));
            }));
        }

        private void ResetView()
        {
            _compareDollarSpineGCom = null;
            _compareBlueBoomEffectGCom = null;
            _compareGoldPurpleEffectGCom = null;
            _compareLightEffectGCom = null;

            Object.Destroy(_cloneDollarSpineObj);
            Object.Destroy(_cloneBlueBoomEffectObj);
            Object.Destroy(_cloneGoldPurpleEffectObj);
            Object.Destroy(_cloneLightEffectObj);

            _cloneDollarSpineObj = null;
            _cloneBlueBoomEffectObj = null;
            _cloneGoldPurpleEffectObj = null;
            _cloneLightEffectObj = null;
        }
    }
}