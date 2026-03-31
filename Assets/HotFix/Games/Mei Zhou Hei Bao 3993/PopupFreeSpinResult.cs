using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeSpinResult : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeSpinResult";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinResult/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _freeResultBtn;
        private GTextField _freeGetText, _freeTotalCountText;
        private GComponent _compareFreeResultFrame;
        private GameObject _freeResultFrameObj, _cloneFreeResultFrameObj;

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
            _freeResultBtn = contentPane.GetChild("freeResultBtn").asButton;
            _freeGetText = contentPane.GetChild("freeGetText").asTextField;
            _freeTotalCountText = contentPane.GetChild("freeTotalCountText").asTextField;
            _freeTotalCountText.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            _freeGetText.text = ContentModel.Instance.freeTotalBet.ToString();
            _freeResultBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "fg_pup_TipFrame.prefab",
                (cloneObj) =>
                {
                    _freeResultFrameObj = cloneObj;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_FreeResultFrame").asCom;
            if (_compareFreeResultFrame != currentCom)
            {
                _cloneFreeResultFrameObj = Object.Instantiate(_freeResultFrameObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeResultFrame);
                _compareFreeResultFrame = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareFreeResultFrame, _cloneFreeResultFrameObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneFreeResultFrameObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareFreeResultFrame);

            _compareFreeResultFrame = null;
            _cloneFreeResultFrameObj = null;
        }
    }
}