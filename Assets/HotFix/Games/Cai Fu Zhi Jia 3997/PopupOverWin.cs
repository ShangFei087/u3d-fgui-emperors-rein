using FairyGUI;
using GameMaker;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class PopupOverWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupOverWin";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupOverWin/SpinePrefabs/";

        private const string ModelPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PageGameMain/ModelPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;

        private Animator _traderAnimator;
        private GComponent _compareOverWin, _compareTrader;
        private GameObject _overWinObj, _cloneOverWinObj, _traderObj, _cloneTraderObj;

        private GTextField _overWinText;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            _overWinText = contentPane.GetChild("overWinText").asTextField;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            BindPrefabToUI();
            _overWinText.text = ContentModel.Instance.normalWinBet.ToString();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            InitParam();

            Timers.inst.Add(3, 1, (obj) => CloseSelf(null));
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
            // _totalCount = 1; //2

            // // 加载Spine
            // ResourceManager02.Instance.LoadAsset<GameObject>(
            //     SpinePrefabPath + "overWin.prefab",
            //     (clone) =>
            //     {
            //         _overWinObj = clone;
            //         ResLoadedCallback();
            //     });

            // // 加载3D Obj
            // ResourceManager02.Instance.LoadAsset<GameObject>(
            //     ModelPrefabPath + "trader.prefab",
            //     (clone) =>
            //     {
            //         _traderObj = clone;
            //         ResLoadedCallback();
            //     });
        }

        private void BindPrefabToUI()
        {
            // // Spine
            // GComponent currentGCom = contentPane.GetChild("anchorOverWin").asCom;
            // if (currentGCom != _compareOverWin)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);
            //     _compareOverWin = currentGCom;
            //     _cloneOverWinObj = Object.Instantiate(_overWinObj);
            //     _cloneOverWinObj.SetActive(false);
            //     GameCommon.FguiUtils.AddWrapper(_compareOverWin, _cloneOverWinObj);
            // }

            // // 3D Obj
            // currentGCom = contentPane.GetChild("anchorPlayer").asCom;
            // if (currentGCom != _compareTrader)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
            //     _compareTrader = currentGCom;
            //     _cloneTraderObj = Object.Instantiate(_traderObj);
            //     _traderAnimator= _cloneTraderObj.GetComponentInChildren<Animator>();
            //     GameCommon.FguiUtils.AddWrapper(_compareTrader, _cloneTraderObj);
            // }
        }

        private void ResetView()
        {
            GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
            GameCommon.FguiUtils.DeleteWrapper(_compareOverWin);

            // Object.Destroy(_cloneTraderObj);
            // Object.Destroy(_cloneOverWinObj);

            _cloneTraderObj = null;
            _cloneOverWinObj = null;
        }
    }
}