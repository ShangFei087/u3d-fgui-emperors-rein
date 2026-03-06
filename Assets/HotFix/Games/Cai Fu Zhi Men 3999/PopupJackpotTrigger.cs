using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class PopupJackpotTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupJackpotTrigger";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupJackpotTrigger/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GGraph _doorMask;
        private GButton _jackpotStartBtn;
        private GComponent _winBoxWindow;

        private GComponent _compareGirlOpenDoor, _compareWinJackpot;

        private GameObject _girlOpenDoorObj, _winJackpotObj;
        private GameObject _cloneGirlOpenDoorObj, _cloneWinJackpotObj;


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

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "GirlOpenDoor.prefab", (cloneObj) =>
            {
                _girlOpenDoorObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "WinJackpot.prefab", (cloneObj) =>
            {
                _winJackpotObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void InitUICom()
        {
            _doorMask = contentPane.GetChild("doorMask").asGraph;
            _winBoxWindow = contentPane.GetChild("winBoxWindow").asCom;
            _jackpotStartBtn = _winBoxWindow.GetChild("jackpotStartBtn").asButton;
            _jackpotStartBtn.visible = false;

            Timers.inst.Add(0.5f, 1, (obj) => _jackpotStartBtn.visible = true);

            _jackpotStartBtn.onClick.Add(() =>
            {
                _winBoxWindow.visible = false;

                _doorMask.visible = true;
                Animator girlAni = _cloneGirlOpenDoorObj.GetComponentInChildren<Animator>();
                _cloneGirlOpenDoorObj.SetActive(true);
                PlayAnimationByName(girlAni, "ng_sg");
                Timers.inst.Add(4.5f, 1, (obj) =>
                {
                    CloseSelf(null);
                    PageManager.Instance.OpenPage(PageName.CaiFuZhiMenPopupJackpotGame);
                });
            });
        }

        private void BindPrefabsToUI()
        {
            // 绑定Spine
            GComponent currentCom = contentPane.GetChild("anchor_GirlOpenDoor").asCom;
            if (currentCom != _compareGirlOpenDoor)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareGirlOpenDoor);
                _compareGirlOpenDoor = currentCom;
                _cloneGirlOpenDoorObj = Object.Instantiate(_girlOpenDoorObj);
                _cloneGirlOpenDoorObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareGirlOpenDoor, _cloneGirlOpenDoorObj);
            }

            currentCom = _winBoxWindow.GetChild("anchor_WinJackpot").asCom;
            if (currentCom != _compareWinJackpot)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareWinJackpot);
                _compareWinJackpot = currentCom;
                _cloneWinJackpotObj = Object.Instantiate(_winJackpotObj);
                GameCommon.FguiUtils.AddWrapper(_compareWinJackpot, _cloneWinJackpotObj);
            }
        }

        private void ResetPage()
        {
            _doorMask.visible = false;
            _winBoxWindow.visible = true;

            GameCommon.FguiUtils.DeleteWrapper(_compareGirlOpenDoor);
            GameCommon.FguiUtils.DeleteWrapper(_compareWinJackpot);

            _compareGirlOpenDoor = null;
            _compareWinJackpot = null;

            Object.Destroy(_cloneGirlOpenDoorObj);
            Object.Destroy(_cloneWinJackpotObj);

            _cloneGirlOpenDoorObj = null;
            _cloneWinJackpotObj = null;
        }

        private void PlayAnimationByName(Animator animator, string aniName)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
        }
    }
}