using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinTrigger/SpinePrefabs/";

        private const string EffectPrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinTrigger/EffectPrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _freeStartBtn;
        private GTextField _freeRoundText;
        private GComponent _compareCollectFrame, _compareEffSlash;
        private GameObject _collectFrameObj, _cloneCollectFrameObj, _effSlashObj, _cloneEffSlashObj;

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
            _freeStartBtn = contentPane.GetChild("freeStartBtn").asButton;
            _freeRoundText = contentPane.GetChild("freeRoundText").asTextField;

            _freeStartBtn.visible = false;
            _freeRoundText.visible = false;
            _freeRoundText.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            Timers.inst.Add(2, 1, (obj) =>
            {
                _freeStartBtn.visible = true;
                _freeRoundText.visible = true;
                _cloneCollectFrameObj.SetActive(true);
                _cloneEffSlashObj.SetActive(false);
            });
            _freeStartBtn.onClick.Add((() =>
            {
                CloseSelf(null);
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 2;
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "fg_pup_CollectFrame.prefab",
                (cloneObj) =>
                {
                    _collectFrameObj = cloneObj;
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
            // 绑定Spine
            GComponent currentCom = contentPane.GetChild("anchor_FreeStartFrame").asCom;
            if (_compareCollectFrame != currentCom)
            {
                _cloneCollectFrameObj = Object.Instantiate(_collectFrameObj);
                _cloneCollectFrameObj.SetActive(false);
                GameCommon.FguiUtils.DeleteWrapper(_compareCollectFrame);
                _compareCollectFrame = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareCollectFrame, _cloneCollectFrameObj);
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
            Object.Destroy(_cloneCollectFrameObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareEffSlash);
            GameCommon.FguiUtils.DeleteWrapper(_compareCollectFrame);

            _compareEffSlash = null;
            _compareCollectFrame = null;
            _cloneEffSlashObj = null;
            _cloneCollectFrameObj = null;
        }
    }
}