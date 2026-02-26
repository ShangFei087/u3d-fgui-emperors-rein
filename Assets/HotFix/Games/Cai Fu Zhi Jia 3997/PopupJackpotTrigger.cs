using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private GameObject _diamondBgEffectObj = null, _lightEffectObj = null;
        private GameObject _cloneDiamondBgEffectObj = null, _cloneLightEffectObj = null;
        private GComponent _compareDiamondBgEffectGCom = null, _compareLightEffectGCom = null;

        // Todo：等Animation做出来之后，直接取消注释即可
        // Animation
        private GameObject _diamondAnimationObj = null;
        private GameObject _cloneDiamondAnimationObj = null;
        private GComponent _compareDiamondAnimationGCom = null;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 4;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            _jackpotTriggerTipWindow = contentPane.GetChild("jackpotTriggerTipWindow").asCom;
            _jackpotTriggerButton = _jackpotTriggerTipWindow.GetChild("jackpotTriggerButton").asButton;
            if (!_isInitialized) return;
            ResetView();

            BindPrefabsToUI();
            ShowEffectAndSpine();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

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

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "lightEffect.prefab",
                (clone) =>
                {
                    _lightEffectObj = clone;
                    ResLoadedCallback();
                });

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

            currentGCom = contentPane.GetChild("lightEffect").asCom;
            if (currentGCom != _compareLightEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
                _compareLightEffectGCom = currentGCom;
                _cloneLightEffectObj = Object.Instantiate(_lightEffectObj);
                GameCommon.FguiUtils.AddWrapper(_compareLightEffectGCom, _cloneLightEffectObj);
            }

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
                _jackpotTriggerTipWindow.visible = false;
                _cloneLightEffectObj.SetActive(false);
                _cloneDiamondAnimationObj.SetActive(false);

                _cloneDiamondSpineObj.SetActive(true);
                _cloneDiamondBgEffectObj.SetActive(true);
                Timers.inst.Add(3, 1, (obj) => _cloneDiamondSpineObj.SetActive(false));
                Timers.inst.Add(7, 1, (obj) =>
                {
                    CloseSelf(null);
                });
            }));
        }

        private void ResetView()
        {
            _jackpotTriggerTipWindow.visible = true;

            _compareDiamondSpineGCom = null;
            _compareDiamondBgEffectGCom = null;
            _compareLightEffectGCom = null;
            _compareDiamondAnimationGCom = null;

            Object.Destroy(_cloneDiamondSpineObj);
            Object.Destroy(_cloneDiamondBgEffectObj);
            Object.Destroy(_cloneLightEffectObj);
            Object.Destroy(_cloneDiamondAnimationObj);

            _cloneDiamondSpineObj = null;
            _cloneDiamondBgEffectObj = null;
            _cloneLightEffectObj = null;
            _cloneDiamondAnimationObj = null;
        }
    }
}