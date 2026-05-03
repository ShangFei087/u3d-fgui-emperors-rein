using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
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
        private const float Duration = 12f;
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
            // if (_monoHelper == null)
            //     _monoHelper = new GameObject("MonoHelper").AddComponent<MonoHelper>();
            StartLoading();
            // _monoHelper.StartCoroutine(StartGameLoading(8));
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
        
        
        // private IEnumerator StartGameLoading(float duration)
        // {
        //     _loadingBar.value = 0;
        //     _loadingBar.max = 100;
        //
        //     if (_isFirstOpen)
        //     {
        //         // ========== 串行预加载，但每完成一个更新进度 ==========
        //         var pagesToPreload = new List<PageName>
        //         {
        //             PageName.CaiFuZhiJiaPageGameMain,
        //             PageName.CaiFuZhiJiaPopupFreeSpinTrigger,
        //             PageName.CaiFuZhiJiaPopupFreeSpinResult,
        //             PageName.CaiFuZhiJiaPopupJackpotTrigger,
        //             PageName.CaiFuZhiJiaPopupJackpotGame,
        //             PageName.CaiFuZhiJiaPopupJackpotResult,
        //             PageName.CaiFuZhiJiaPopupJackpotWin,
        //             PageName.CaiFuZhiJiaPopupOverWin,
        //         };
        //
        //         int totalCount = pagesToPreload.Count;
        //
        //         for (int i = 0; i < totalCount; i++)
        //         {
        //             bool isLoaded = false;
        //             PageManager.Instance.PreloadPage(pagesToPreload[i], () => isLoaded = true);
        //             yield return new WaitUntil(() => isLoaded);
        //
        //             // 每加载完一个页面，更新进度
        //             _loadingBar.value = (float)(i + 1) / totalCount * 90f;
        //         }
        //
        //         _isFirstOpen = false;
        //         Debug.Log("CaiFuZhiJia is Preloaded!");
        //     }
        //
        //     // ========== 最后 10% 快速走完 ==========
        //     float elapsed = 0f;
        //     double startValue = _loadingBar.value;
        //
        //     while (elapsed < 0.2f)
        //     {
        //         elapsed += Time.deltaTime;
        //         float t = Mathf.Clamp01(elapsed / 0.2f);
        //         _loadingBar.value = Mathf.Lerp((float)startValue, 100, t);
        //         yield return null;
        //     }
        //
        //     _loadingBar.value = 100;
        // }


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
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupOverWin, null);
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