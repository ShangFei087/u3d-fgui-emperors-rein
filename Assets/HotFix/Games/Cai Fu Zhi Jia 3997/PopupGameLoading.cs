using FairyGUI;
using GameMaker;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupGameLoading/SpinePrefabs/";

        // 初始化
        private int _totalResCount = -1;
        private bool _isInitialized = false;
        private bool _isFirstOpen = true;
        private const float Duration = 8f;
        private GTweener _loadingGTween;

        private GSlider _loadingBar;
        private GameObject _bgSpineObj, _centerSpineObj;
        private GameObject _cloneBgSpineObj, _cloneCenterSpineObj;
        private GComponent _compareBgSpine, _compareCenterSpine;

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
            if (--_totalResCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchorBG").asCom;
            if (_compareBgSpine != currentCom)
            {
                _cloneBgSpineObj = Object.Instantiate(_bgSpineObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareBgSpine);
                _compareBgSpine = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareBgSpine, _cloneBgSpineObj);
            }

            currentCom = contentPane.GetChild("anchorCenter").asCom;
            if (currentCom != _compareCenterSpine)
            {
                _cloneCenterSpineObj = Object.Instantiate(_centerSpineObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareCenterSpine);
                _compareCenterSpine = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareCenterSpine, _cloneCenterSpineObj);
            }
        }

        private void InitUICom()
        {
            _loadingBar = contentPane.GetChild("sliderLoading").asSlider;
        }

        private void StartLoading()
        {
            if (_isFirstOpen)
            {
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPageGameMain, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinTrigger, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinResult, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotTrigger, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotGame, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotResult, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotWin, null);
                _isFirstOpen = false;

                Debug.LogError("CaiFuZhiJia is Preloaded!");
            }

            if (_loadingGTween != null) _loadingGTween.Kill();
            _loadingGTween = GTween.To(0, 100, Duration).SetEase(EaseType.Linear).OnUpdate((tween) =>
            {
                _loadingBar.value = tween.value.x;
            }).OnComplete(() =>
            {
                CloseSelf(null);
                PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPageGameMain);
            });
        }

        private void LoadResAsync()
        {
            _totalResCount = 2;

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "BGSpine.prefab", (cloneObj) =>
            {
                _bgSpineObj = cloneObj;
                ResLoadedCallback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "CenterSpine.prefab", (cloneObj) =>
            {
                _centerSpineObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneBgSpineObj);
            Object.Destroy(_cloneCenterSpineObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareBgSpine);
            GameCommon.FguiUtils.DeleteWrapper(_compareCenterSpine);

            _cloneBgSpineObj = null;
            _cloneCenterSpineObj = null;
            _compareBgSpine = null;
            _compareCenterSpine = null;
        }
    }
}