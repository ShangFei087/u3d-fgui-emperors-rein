using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupGameLoading/SpinePrefabs/";

        private const string EffectPrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupGameLoading/EffectPrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;
        private bool _isFirstOpened = true;
        private const float Duration = 8f;
        private GTweener _loadingGTween;
        private GComponent _compareBootCom, _compareBarEffectCom;
        private GProgressBar _loadingBar;
        private GameObject _bootObj, _cloneBootObj, _barEffectObj, _cloneBarEffectObj;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            LoadResAsync();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            BindPrefabsToUI();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            InitUICom();
            InitParam();
            StartLoading();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ResetPage();
        }

        private void ResLoadedCallback()
        {
            if (--_resCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void LoadResAsync()
        {
            _resCount = 2;
            // 加载动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Boot.prefab", (cloneObj) =>
            {
                _bootObj = cloneObj;
                ResLoadedCallback();
            });
            // 加载特效
            ResourceManager02.Instance.LoadAsset<GameObject>(EffectPrefabsPath + "BarEffect.prefab", (cloneObj) =>
            {
                _barEffectObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_Boot").asCom;
            if (_compareBootCom != currentCom)
            {
                _cloneBootObj = Object.Instantiate(_bootObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareBootCom);
                _compareBootCom = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareBootCom, _cloneBootObj);
            }

            currentCom = _loadingBar.GetChild("anchor_BarEffect").asCom;
            if (_compareBarEffectCom != currentCom)
            {
                _cloneBarEffectObj = Object.Instantiate(_barEffectObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareBarEffectCom);
                _compareBarEffectCom = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareBarEffectCom, _cloneBarEffectObj);
            }
        }

        private void InitUICom()
        {
            _loadingBar = contentPane.GetChild("loadingBar").asProgress;
        }

        private void StartLoading()
        {
            if (_isFirstOpened)
            {
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPageGameMain, null);
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupFreeGameLoading, null);
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupFreeSpinTrigger, null);
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupJackpotGame, null);
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupJackpotLoading, null);
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupJackpotTrigger, null);
                PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupJackpotResult, null);
                _isFirstOpened = false;
            }

            if (_loadingGTween != null) _loadingGTween.Kill();
            _loadingGTween = GTween.To(0, 100, Duration).SetEase(EaseType.Linear).OnUpdate((tween) =>
            {
                _loadingBar.value = tween.value.x;
            }).OnComplete(() =>
            {
                CloseSelf(null);
                PageManager.Instance.OpenPage(PageName.MeiZhouHeiBaoPageGameMain);
            });
        }


        private void ResetPage()
        {
            Object.Destroy(_cloneBootObj);
            Object.Destroy(_cloneBarEffectObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareBootCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareBarEffectCom);

            _compareBootCom = null;
            _compareBarEffectCom = null;
            _cloneBootObj = null;
            _cloneBarEffectObj = null;
        }
    }
}