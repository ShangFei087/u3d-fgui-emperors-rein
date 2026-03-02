using FairyGUI;
using GameMaker;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupFreeSpinTrigger/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _startBtn;
        private GTextField _freeRoundText;

        private GComponent _freeTriggerWindow, _freeBg;
        private GComponent _compareBlueGem, _compareFreeTriggerTip, _compareCatGirlClimb, _compareRedRay;

        private GameObject _blueGemObj, _catGirlClimb, _freeTriggerTipObj, _redRayObj;
        private GameObject _cloneBlueGemObj, _cloneCatGirlClimb, _cloneFreeTriggerTipObj, _cloneRedRayObj;

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
            GameSoundHelper3999.Instance.StopSound(SoundKey.RegularBG);
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
            _resCount = 4;

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "BlueGem.prefab", (cloneObj) =>
            {
                _blueGemObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "FreeTriggerTip.prefab", (cloneObj) =>
            {
                _freeTriggerTipObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "CatGirlClimb.prefab", (cloneObj) =>
            {
                _catGirlClimb = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "RedRay.prefab", (cloneObj) =>
            {
                _redRayObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void InitUICom()
        {
            _freeBg = contentPane.GetChild("freeBg").asCom;
            _freeTriggerWindow = contentPane.GetChild("freeTriggerWindow").asCom;

            _startBtn = _freeTriggerWindow.GetChild("startBtn").asButton;
            _freeRoundText = _freeTriggerWindow.GetChild("freeRoundText").asTextField;

            _startBtn.visible = false;
            _freeRoundText.visible = false;
            Timers.inst.Add(0.5f, 1, (obj) =>
            {
                _startBtn.visible = true;
                _freeRoundText.visible = true;
            });

            _startBtn.onClick.Add((() =>
            {
                _freeTriggerWindow.visible = false;
                _cloneFreeTriggerTipObj.SetActive(false);
                _cloneCatGirlClimb.SetActive(true);
                _cloneRedRayObj.SetActive(true);

                Timers.inst.Add(0.2f, 1, (obj) =>
                {
                    _freeBg.visible = true;
                    _cloneBlueGemObj.SetActive(false);
                });

                Timers.inst.Add(3.5f, 1, (obj) =>
                {
                    CloseSelf(null);
                    GameSoundHelper3999.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
                });
            }));
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_BlueGem").asCom;
            if (currentCom != _compareBlueGem)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBlueGem);
                _compareBlueGem = currentCom;
                _cloneBlueGemObj = Object.Instantiate(_blueGemObj);
                GameCommon.FguiUtils.AddWrapper(_compareBlueGem, _cloneBlueGemObj);
            }

            currentCom = _freeTriggerWindow.GetChild("freeTriggerTip").asCom;
            if (currentCom != _compareFreeTriggerTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeTriggerTip);
                _compareFreeTriggerTip = currentCom;
                _cloneFreeTriggerTipObj = Object.Instantiate(_freeTriggerTipObj);
                GameCommon.FguiUtils.AddWrapper(_compareFreeTriggerTip, _cloneFreeTriggerTipObj);
            }

            currentCom = contentPane.GetChild("anchor_CatGirlClimb").asCom;
            if (currentCom != _compareCatGirlClimb)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareCatGirlClimb);
                _compareCatGirlClimb = currentCom;
                _cloneCatGirlClimb = Object.Instantiate(_catGirlClimb);
                _cloneCatGirlClimb.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareCatGirlClimb, _cloneCatGirlClimb);
            }

            currentCom = contentPane.GetChild("anchor_RedRay").asCom;
            if (currentCom != _compareRedRay)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareRedRay);
                _compareRedRay = currentCom;
                _cloneRedRayObj = Object.Instantiate(_redRayObj);
                _cloneRedRayObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareRedRay, _cloneRedRayObj);
            }
        }

        private void ResetPage()
        {
            _freeBg.visible = false;
            _freeTriggerWindow.visible = true;

            Object.Destroy(_cloneBlueGemObj);
            Object.Destroy(_cloneFreeTriggerTipObj);
            Object.Destroy(_cloneCatGirlClimb);
            Object.Destroy(_cloneRedRayObj);

            _cloneBlueGemObj = null;
            _cloneFreeTriggerTipObj = null;
            _cloneCatGirlClimb = null;
            _cloneRedRayObj = null;

            _compareBlueGem = null;
            _compareFreeTriggerTip = null;
            _compareCatGirlClimb = null;
            _compareRedRay = null;
            
            // _cloneBlueGemObj.SetActive(true);
            // _cloneFreeTriggerTipObj.SetActive(true);
            // _cloneCatGirlClimb.SetActive(false);
            // _cloneRedRayObj.SetActive(false);
        }
    }
}