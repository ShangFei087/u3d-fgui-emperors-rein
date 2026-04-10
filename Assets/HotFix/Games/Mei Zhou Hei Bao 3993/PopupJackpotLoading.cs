using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupJackpotLoading : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupJackpotLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupJackpotLoading/SpinePrefabs/";
        
        private int _resCount = -1;
        private bool _isInitialized = false;

        private GComponent _compareTransitionFgToJp;
        private GameObject _transitionFgToJpObj, _cloneTransitionFgToJpObj;

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
            
            Timers.inst.Add(6f, 1, (obj) =>
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
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Transition_FgToJp.prefab", (cloneObj) =>
            {
                _transitionFgToJpObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_TransitionFgToJp").asCom;
            if (_compareTransitionFgToJp != currentCom)
            {
                _cloneTransitionFgToJpObj = Object.Instantiate(_transitionFgToJpObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareTransitionFgToJp);
                _compareTransitionFgToJp = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareTransitionFgToJp, _cloneTransitionFgToJpObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneTransitionFgToJpObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareTransitionFgToJp);

            _cloneTransitionFgToJpObj = null;
            _compareTransitionFgToJp = null;
        }
        
    }
}