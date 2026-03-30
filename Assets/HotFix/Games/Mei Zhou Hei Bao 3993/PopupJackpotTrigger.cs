using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupJackpotTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupJackpotTrigger";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupJackpotTrigger/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _jackpotStartBtn;
        private GTextField _jackpotRoundText;
        private GComponent _compareJpPupCollectFrame;
        private GameObject _jpPupCollectFrameObj, _cloneJpPupCollectFrameObj;

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
            _jackpotStartBtn = contentPane.GetChild("jackpotStartBtn").asButton;
            _jackpotRoundText = contentPane.GetChild("jackpotRoundText").asTextField;
           
            _jackpotStartBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "jp_pup_CollectFrame.prefab",
                (cloneObj) =>
                {
                    _jpPupCollectFrameObj = cloneObj;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_JpPupCollectFrame").asCom;
            if (_compareJpPupCollectFrame != currentCom)
            {
                _cloneJpPupCollectFrameObj = Object.Instantiate(_jpPupCollectFrameObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareJpPupCollectFrame);
                _compareJpPupCollectFrame = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareJpPupCollectFrame, _cloneJpPupCollectFrameObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneJpPupCollectFrameObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareJpPupCollectFrame);

            _compareJpPupCollectFrame = null;
            _cloneJpPupCollectFrameObj = null;
        }
    }
}