using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class PopupJackpotWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupJackpotWin";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotWin/SpinePrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;

        private GComponent _compareJackpotWinGCom;
        private GameObject _jackpotWinObj, _cloneJackpotWinObj;

        private GButton _collectBtn;
        private GTextField _winBetText;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _collectBtn = contentPane.GetChild("winCollectBtn").asButton;
            _winBetText = contentPane.GetChild("jackpotWinBet").asTextField;

            

            _totalCount = 1;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            BindPrefabsToUI();

            ShowWinBet();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam();
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
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "JackpotWin.prefab",
                (clone) =>
                {
                    _jackpotWinObj = clone;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            // Spine
            GComponent currentGCom = contentPane.GetChild("anchor_JackpotWin").asCom;
            if (currentGCom != _compareJackpotWinGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotWinGCom);
                _compareJackpotWinGCom = currentGCom;
                _cloneJackpotWinObj = Object.Instantiate(_jackpotWinObj);
                _cloneJackpotWinObj.transform.GetChild(0).GetChild(ContentModel.Instance.currentJpSpineIndex).gameObject
                    .SetActive(true);
                GameCommon.FguiUtils.AddWrapper(_compareJackpotWinGCom, _cloneJackpotWinObj);
            }
        }

        private void ShowWinBet()
        {
            _collectBtn.visible = false;
            _winBetText.visible = false;
            Timers.inst.Add(2f, 1, (obj) =>
            {
                _winBetText.visible = true;
                _collectBtn.visible = true;
                _winBetText.text = ContentModel.Instance.currentShowJpBet.ToString();

                if (TestManager.Instance.IsAutoModeRunning && _collectBtn != null && _collectBtn.visible)
                {
                    _collectBtn.onClick.Call();
                }
            });

            _collectBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void ResetView()
        {
            GameCommon.FguiUtils.DeleteWrapper(_compareJackpotWinGCom);
            _compareJackpotWinGCom = null;
            Object.Destroy(_cloneJackpotWinObj);
            _cloneJackpotWinObj = null;
        }
    }
}