using FairyGUI;
using GameMaker;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class PopupJackpotResult : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupJackpotResult";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupJackpotResult/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GComponent _totalGetWindow, _jackpotGetWindow;
        private GButton _totalCollectBtn, _jackpotCollectBtn;
        private GComponent _compareJackpotGetWindow, _compareRealGetWindow;

        private GameObject _jackpotGetWindowObj, _realGetWindowObj;
        private GameObject _cloneJackpotGetWindowObj, _cloneRealGetWindowObj;

        private GTextField _totalGetText, _jackpotGetText;

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

        private void LoadResAsync()
        {
            _resCount = 2;

            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "JackpotGetWindow.prefab", (cloneObj) =>
            {
                _jackpotGetWindowObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "RealGetWindow.prefab", (cloneObj) =>
            {
                _realGetWindowObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = _totalGetWindow.GetChild("anchor_RealGetWindow").asCom;
            if (currentCom != _compareRealGetWindow)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareRealGetWindow);
                _compareRealGetWindow = currentCom;
                _cloneRealGetWindowObj = Object.Instantiate(_realGetWindowObj);
                GameCommon.FguiUtils.AddWrapper(_compareRealGetWindow, _cloneRealGetWindowObj);
            }

            currentCom = _jackpotGetWindow.GetChild("anchor_JackpotGetWindow").asCom;
            if (currentCom != _compareJackpotGetWindow)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotGetWindow);
                _compareJackpotGetWindow = currentCom;
                _cloneJackpotGetWindowObj = Object.Instantiate(_jackpotGetWindowObj);
                _cloneJackpotGetWindowObj.transform.Find("Anchor").GetChild(ContentModel.Instance.bonusIndex).gameObject
                    .SetActive(true);
                GameCommon.FguiUtils.AddWrapper(_compareJackpotGetWindow, _cloneJackpotGetWindowObj);
            }
        }

        private void InitUICom()
        {
            _totalGetWindow = contentPane.GetChild("totalGetWindow").asCom;
            _jackpotGetWindow = contentPane.GetChild("jackpotGetWindow").asCom;

            _totalCollectBtn = _totalGetWindow.GetChild("totalCollectBtn").asButton;
            _jackpotCollectBtn = _jackpotGetWindow.GetChild("jackpotCollectBtn").asButton;

            _totalGetText = _totalGetWindow.GetChild("totalGetText").asTextField;
            _jackpotGetText = _jackpotGetWindow.GetChild("jackpotGetText").asTextField;

            _totalCollectBtn.visible = false;
            _totalGetText.visible = false;
            _jackpotCollectBtn.visible = false;
            _jackpotGetText.visible = false;

            _totalGetText.text = ContentModel.Instance.bonusTotalBet.ToString();
            _jackpotGetText.text = ContentModel.Instance.bonusTotalBet.ToString();

            Timers.inst.Add(1.5f, 1, (obj) =>
            {
                _jackpotCollectBtn.visible = true;
                _jackpotGetText.visible = true;
            });

            _jackpotCollectBtn.onClick.Add(() =>
            {
                _jackpotGetWindow.visible = false;
                _totalGetWindow.visible = true;

                Timers.inst.Add(0.5f, 1, (obj) =>
                {
                    _totalCollectBtn.visible = true;
                    _totalGetText.visible = true;
                });
            });

            _totalCollectBtn.onClick.Add(() =>
            {
                CloseSelf(null);
                PageManager.Instance.ClosePage(PageName.CaiFuZhiMenPopupJackpotGame);
                PageManager.Instance.OpenPage(PageName.CaiFuZhiMenPopupJackpotLoad);
            });
        }

        private void ResetPage()
        {
            _jackpotGetWindow.visible = true;
            _totalGetWindow.visible = false;

            Object.Destroy(_cloneJackpotGetWindowObj);
            Object.Destroy(_cloneRealGetWindowObj);

            _cloneJackpotGetWindowObj = null;
            _cloneRealGetWindowObj = null;

            GameCommon.FguiUtils.DeleteWrapper(_compareJackpotGetWindow);
            GameCommon.FguiUtils.DeleteWrapper(_compareRealGetWindow);

            _compareJackpotGetWindow = null;
            _compareRealGetWindow = null;
            ContentModel.Instance.bonusIndex = -1;
        }
    }
}