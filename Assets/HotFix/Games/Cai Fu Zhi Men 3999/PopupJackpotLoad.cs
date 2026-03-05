using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class PopupJackpotLoad : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupJackpotLoad";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupJackpotLoad/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GameObject _jackpotBgObj, _catGirlGlideObj;
        private GameObject _cloneJackpotBgObj, _cloneCatGirlGlideObj;
        private GComponent _compareJackpotBg, _compareCatGirlGlide;

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
            ClosePage();
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
            _resCount = 2;
            
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "JackpotBg.prefab", (cloneObj) =>
            {
                _jackpotBgObj = cloneObj;
                ResLoadedCallback();
            });
            
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "CatGirlGlide.prefab", (cloneObj) =>
            {
                _catGirlGlideObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_JackpotBg").asCom;
            if (currentCom != _compareJackpotBg)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotBg);
                _compareJackpotBg = currentCom;
                _cloneJackpotBgObj = Object.Instantiate(_jackpotBgObj);
                GameCommon.FguiUtils.AddWrapper(_compareJackpotBg, _cloneJackpotBgObj);
            }
            
            currentCom = contentPane.GetChild("anchor_CatGirlGlide").asCom;
            if (currentCom != _compareCatGirlGlide)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareCatGirlGlide);
                _compareCatGirlGlide = currentCom;
                _cloneCatGirlGlideObj = Object.Instantiate(_catGirlGlideObj);
                GameCommon.FguiUtils.AddWrapper(_compareCatGirlGlide, _cloneCatGirlGlideObj);
            }
        }

        private void ClosePage()
        {
            Timers.inst.Add(2.5f,1, (obj) =>
            {
                CloseSelf(null);
                GameSoundHelper3999.Instance.PlayMusicSingle(SoundKey.RegularBG);
            });
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneJackpotBgObj);
            Object.Destroy(_cloneCatGirlGlideObj);

            _cloneJackpotBgObj = null;
            _cloneCatGirlGlideObj = null;

            _compareJackpotBg = null;
            _compareCatGirlGlide = null;
        }
    }
}