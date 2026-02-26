using FairyGUI;
using GameMaker;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiMen_3999
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupGameLoading/SpinePrefabs/";

        private bool _isInitialized = false;
        private bool _isFirstOpen = true;
        private int _resCount = -1;
        private const float Duration = 8f;
        private GTweener _loadingGTween;

        private GSlider _loadingBar;
        private GComponent _compareCatGirl, _compareGameName;
        private GameObject _catGirlObj, _gameNameObj, _cloneCatGirlObj, _cloneGameNameObj;

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

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "CatGirl.prefab", (cloneObj) =>
            {
                _catGirlObj = cloneObj;
                ResLoadedCallback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "GameName.prefab", (cloneObj) =>
            {
                _gameNameObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_CatGirl").asCom;
            if (_compareCatGirl != currentCom)
            {
                _cloneCatGirlObj = Object.Instantiate(_catGirlObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareCatGirl);
                _compareCatGirl = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareCatGirl, _cloneCatGirlObj);
            }

            currentCom = contentPane.GetChild("anchor_GameName").asCom;
            if (currentCom != _compareGameName)
            {
                _cloneGameNameObj = Object.Instantiate(_gameNameObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareGameName);
                _compareGameName = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareGameName, _cloneGameNameObj);
            }
        }

        private void InitUICom()
        {
            _loadingBar = contentPane.GetChild("loadingBar").asSlider;
        }

        private void StartLoading()
        {
            if (_isFirstOpen)
            {
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiMenPageGameMain, null);
                // PageManager.Instance.PreloadPage(PageName.CaiFuZhiMenPopupFreeSpinTrigger, null);
                // PageManager.Instance.PreloadPage(PageName.CaiFuZhiMenPopupFreeSpinResult, null);
                // PageManager.Instance.PreloadPage(PageName.CaiFuZhiMenPopupJackpotGameTrigger, null);
                // PageManager.Instance.PreloadPage(PageName.CaiFuZhiMenPopupJackpotGame, null);
                // PageManager.Instance.PreloadPage(PageName.CaiFuZhiMenPopupJackpotGameResult, null);
                _isFirstOpen = false;
                
                Debug.LogError("CaiFuZhiMen is Preloaded!");
            }

            if (_loadingGTween != null) _loadingGTween.Kill();
            _loadingGTween = GTween.To(0, 100, Duration).SetEase(EaseType.Linear).OnUpdate((tween) =>
            {
                _loadingBar.value = tween.value.x;
            }).OnComplete(() =>
            {
                CloseSelf(null);
                PageManager.Instance.OpenPage(PageName.CaiFuZhiMenPageGameMain);
            });
        }
    }
}