using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeGameLoading : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeGameLoading/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;
        private GComponent _compareJpTransitionCom;
        private GameObject _jpTransitionObj, _cloneJpTransitionObj;

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
            
            Timers.inst.Add(2.66f, 1, (obj) =>
            {
                CloseSelf(null);
            });
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            InitParam();
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
            _resCount = 1;
            // 加载动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "jp_Transition.prefab", (cloneObj) =>
            {
                _jpTransitionObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_JpTransition").asCom;
            if (_compareJpTransitionCom != currentCom)
            {
                _cloneJpTransitionObj = Object.Instantiate(_jpTransitionObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareJpTransitionCom);
                _compareJpTransitionCom = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareJpTransitionCom, _cloneJpTransitionObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneJpTransitionObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareJpTransitionCom);

            _cloneJpTransitionObj = null;
            _compareJpTransitionCom = null;
        }
    }
}