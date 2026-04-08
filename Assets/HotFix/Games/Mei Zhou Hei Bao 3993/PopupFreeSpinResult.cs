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
        
        private const string EffectPrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinTrigger/EffectPrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _freeResultBtn;
        private GTextField _freeGetText, _freeTotalCountText;
        private GComponent _compareFreeResultFrame, _compareEffSlash;
        private GameObject _freeResultFrameObj, _cloneFreeResultFrameObj, _effSlashObj, _cloneEffSlashObj;

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
            _freeGetText.visible = false;
            _freeTotalCountText = contentPane.GetChild("freeTotalCountText").asTextField;
            _freeTotalCountText.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            _freeTotalCountText.visible = false;
            _freeGetText.text = ContentModel.Instance.freeTotalBet.ToString();
            Timers.inst.Add(2f,1, (obj) =>
            {
                // _freeGetText.visible = true;
                // _freeTotalCountText.visible = true;
                _cloneFreeResultFrameObj.SetActive(true);
                _cloneEffSlashObj.SetActive(false);
            });
            Timers.inst.Add(2.2f,1, (obj) =>
            {
                _freeGetText.visible = true;
                _freeTotalCountText.visible = true;
            });
            _freeResultBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 2;
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "fg_pup_TipFrame.prefab",
                (cloneObj) =>
                {
                    _freeResultFrameObj = cloneObj;
                    ResLoadedCallback();
                });
            
            // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(EffectPrefabsPath + "eff_Slash 1.prefab",
                (cloneObj) =>
                {
                    _effSlashObj = cloneObj;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_FreeResultFrame").asCom;
            if (_compareFreeResultFrame != currentCom)
            {
                _cloneFreeResultFrameObj = Object.Instantiate(_freeResultFrameObj);
                _cloneFreeResultFrameObj.SetActive(false);
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeResultFrame);
                _compareFreeResultFrame = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareFreeResultFrame, _cloneFreeResultFrameObj);
            }
            
            // 绑定Effect
            currentCom = contentPane.GetChild("anchor_eff_Slash 1").asCom;
            if (_compareEffSlash != currentCom)
            {
                _cloneEffSlashObj = Object.Instantiate(_effSlashObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareEffSlash);
                _compareEffSlash = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareEffSlash, _cloneEffSlashObj);
            }
        }

        private void ResetPage()
        {
            Object.Destroy(_cloneEffSlashObj);
            Object.Destroy(_cloneFreeResultFrameObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareEffSlash);
            GameCommon.FguiUtils.DeleteWrapper(_compareFreeResultFrame);

            _compareEffSlash = null;
            _compareFreeResultFrame = null;
            _cloneEffSlashObj = null;
            _cloneFreeResultFrameObj = null;
        }
    }
}