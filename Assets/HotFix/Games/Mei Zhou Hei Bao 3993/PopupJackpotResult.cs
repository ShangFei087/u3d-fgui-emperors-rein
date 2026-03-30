using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupJackpotResult : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupJackpotResult";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupJackpotResult/SpinePrefabs/";
        
        private int _resCount = -1;
        private bool _isInitialized = false;

        private GComponent _compareJpPupTipFrameCom;
        private GameObject _jpPupTipFrameObj, _cloneJpPupTipFrameObj;
        private GTextField _jackpotResultText;
        private GButton _jackpotResultBtn;
        
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
            _jackpotResultBtn = contentPane.GetChild("jackpotResultBtn").asButton;
            _jackpotResultText = contentPane.GetChild("jackpotResultText").asTextField;
            
            _jackpotResultBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "jp_pup_TipFrame.prefab", (cloneObj) =>
            {
                _jpPupTipFrameObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_JpPupTipFrame").asCom;
            if (_compareJpPupTipFrameCom != currentCom)
            {
                _cloneJpPupTipFrameObj = Object.Instantiate(_jpPupTipFrameObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareJpPupTipFrameCom);
                _compareJpPupTipFrameCom = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareJpPupTipFrameCom, _cloneJpPupTipFrameObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneJpPupTipFrameObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareJpPupTipFrameCom);

            _compareJpPupTipFrameCom = null;
            _cloneJpPupTipFrameObj = null;
        }
    }
}