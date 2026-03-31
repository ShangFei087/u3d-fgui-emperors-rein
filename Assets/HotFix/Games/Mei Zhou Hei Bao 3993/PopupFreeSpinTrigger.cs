using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinTrigger/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _freeStartBtn;
        private GTextField _freeRoundText;
        private GComponent _compareCollectFrame;
        private GameObject _collectFrameObj, _cloneCollectFrameObj;

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

        private void InitUICom()
        {
            _freeStartBtn = contentPane.GetChild("freeStartBtn").asButton;
            _freeRoundText = contentPane.GetChild("freeRoundText").asTextField;
            _freeRoundText.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            _freeStartBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "fg_pup_CollectFrame.prefab",
                (cloneObj) =>
                {
                    _collectFrameObj = cloneObj;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_FreeStartFrame").asCom;
            if (_compareCollectFrame != currentCom)
            {
                _cloneCollectFrameObj = Object.Instantiate(_collectFrameObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareCollectFrame);
                _compareCollectFrame = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareCollectFrame, _cloneCollectFrameObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneCollectFrameObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareCollectFrame);

            _compareCollectFrame = null;
            _cloneCollectFrameObj = null;
        }
    }
}