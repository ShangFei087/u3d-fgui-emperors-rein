using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiMen_3999
{
    public class PopupOnlineJackpot : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupOnlineJackpot";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupJackpotResult/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _jackpotCollectBtn;
        private GTextField _jackpotGetText;
        private GComponent _compareJackpotGetWindow;

        private GameObject _jackpotGetWindowObj;
        private GameObject _cloneJackpotGetWindowObj;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            LoadResAsync();
            AddMachineBtnClickListener();
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
            _resCount = 1;

            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "JackpotGetWindow.prefab", (cloneObj) =>
            {
                _jackpotGetWindowObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_Jackpot").asCom;
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
            _jackpotCollectBtn = contentPane.GetChild("collectBtn").asButton;
            _jackpotGetText = contentPane.GetChild("jackpotText").asTextField;
            
            // 设置获奖信息
            WinJackpotInfo winJackpotInfo = ContentModel.Instance.jpOnlineWin[0];
            _jackpotGetText.text = winJackpotInfo.win.ToString();
            ContentModel.Instance.bonusIndex = winJackpotInfo.jackpotId;

            _jackpotCollectBtn.visible = false;
            _jackpotGetText.visible = false;
            _jackpotGetText.text = ContentModel.Instance.bonusTotalBet.ToString();

            Timers.inst.Add(1.5f, 1, (obj) =>
            {
                _jackpotCollectBtn.visible = true;
                _jackpotGetText.visible = true;
            });

            _jackpotCollectBtn.onClick.Add(() =>
            {
                CloseSelf(null);
            });
        }

        private void JackpotBtnEvent()
        {
            CloseSelf(null);
        }

        private void AddMachineBtnClickListener()
        {
            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        JackpotBtnEvent();
                    }
                },
            };
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneJackpotGetWindowObj);
            _cloneJackpotGetWindowObj = null;
            GameCommon.FguiUtils.DeleteWrapper(_compareJackpotGetWindow);
            _compareJackpotGetWindow = null;
            ContentModel.Instance.bonusIndex = -1;
        }
    }
}