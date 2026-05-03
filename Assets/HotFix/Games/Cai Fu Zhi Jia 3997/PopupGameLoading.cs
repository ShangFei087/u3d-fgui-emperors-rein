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
        private GameObject _traderObj, _gameTitleObj;
        private GameObject _cloneTraderObj, _cloneGameTitleObj;
        private GComponent _compareTrader, _compareGameTitle;

        // private MonoHelper _monoHelper;
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
            GComponent currentCom = contentPane.GetChild("anchorTrader").asCom;
            if (_compareTrader != currentCom)
            {
                _cloneTraderObj = Object.Instantiate(_traderObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
                _compareTrader = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareTrader, _cloneTraderObj);
            }

            currentCom = contentPane.GetChild("anchorGameTitle").asCom;
            if (currentCom != _compareGameTitle)
            {
                _cloneGameTitleObj = Object.Instantiate(_gameTitleObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareGameTitle);
                _compareGameTitle = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareGameTitle, _cloneGameTitleObj);
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
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotGame, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupOverWin, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotWin, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinTrigger, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinResult, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotTrigger, null);
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotResult, null);
                
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
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Trader.prefab", (cloneObj) =>
            {
                _traderObj = cloneObj;
                ResLoadedCallback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "GameTitle.prefab", (cloneObj) =>
            {
                _gameTitleObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneTraderObj);
            Object.Destroy(_cloneGameTitleObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
            GameCommon.FguiUtils.DeleteWrapper(_compareGameTitle);

            _cloneTraderObj = null;
            _cloneGameTitleObj = null;
            _compareTrader = null;
            _compareGameTitle = null;
        }
    }
}