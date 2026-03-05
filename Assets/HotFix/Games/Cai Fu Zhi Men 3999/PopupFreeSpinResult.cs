using FairyGUI;
using GameMaker;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class PopupFreeSpinResult : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupFreeSpinResult";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupFreeSpinResult/SpinePrefabs/";

        private bool _isInitialized;
        private int _resCount = -1;

        private GComponent _freeResultWindow;
        private GTextField _resultScore;
        private GButton _collectBtn;

        private GameObject _freeResultTip, _catGirlRun, _redRay;
        private GameObject _cloneFreeResultTip, _cloneCatGirlRun, _cloneRedRay;
        private GComponent _compareFreeResultTip, _compareCatGirlRun, _compareRedRay;

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
            GameSoundHelper3999.Instance.StopSound(SoundKey.FreeSpinBG);
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
            _resCount = 3;

            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "FreeResultTip.prefab", (cloneObj) =>
            {
                _freeResultTip = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "CatGirlRun.prefab", (cloneObj) =>
            {
                _catGirlRun = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "RedRay.prefab", (cloneObj) =>
            {
                _redRay = cloneObj;
                ResLoadedCallback();
            });
        }

        private void InitUICom()
        {
            _freeResultWindow = contentPane.GetChild("freeResultWindow").asCom;
            _resultScore = _freeResultWindow.GetChild("resultScore").asTextField;
            _collectBtn = _freeResultWindow.GetChild("collectBtn").asButton;

            _resultScore.visible = false;
            _collectBtn.visible = false;

            _resultScore.text = ContentModel.Instance.freeTotalBet.ToString();

            Timers.inst.Add(0.5f, 1, (obj) =>
            {
                _resultScore.visible = true;
                _collectBtn.visible = true;
            });


            _collectBtn.onClick.Add(() =>
            {
                _freeResultWindow.visible = false;

                _cloneRedRay.SetActive(true);
                Animator ani = _cloneCatGirlRun.GetComponentInChildren<Animator>();
                _cloneCatGirlRun.SetActive(true);
                PlayAnimationByName(ani, "fg_ng");

                Timers.inst.Add(3.5f, 1, (obj) =>
                {
                    CloseSelf(null);
                    GameSoundHelper3999.Instance.PlayMusicSingle(SoundKey.RegularBG);
                });
            });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = _freeResultWindow.GetChild("anchor_FreeResultTip").asCom;
            if (currentCom != _compareFreeResultTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeResultTip);
                _compareFreeResultTip = currentCom;
                _cloneFreeResultTip = Object.Instantiate(_freeResultTip);
                GameCommon.FguiUtils.AddWrapper(_compareFreeResultTip, _cloneFreeResultTip);
            }

            currentCom = contentPane.GetChild("anchor_CatGirlRun").asCom;
            if (currentCom != _compareCatGirlRun)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareCatGirlRun);
                _compareCatGirlRun = currentCom;
                _cloneCatGirlRun = Object.Instantiate(_catGirlRun);
                _cloneCatGirlRun.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareCatGirlRun, _cloneCatGirlRun);
            }

            currentCom = contentPane.GetChild("anchor_RedRay").asCom;
            if (currentCom != _compareRedRay)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareRedRay);
                _compareRedRay = currentCom;
                _cloneRedRay = Object.Instantiate(_redRay);
                _cloneRedRay.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareRedRay, _cloneRedRay);
            }
        }
        
        private void PlayAnimationByName(Animator animator, string aniName)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
        }

        private void ResetPage()
        {
            _freeResultWindow.visible = true;
            _collectBtn.onClick.Clear();
            
            GameCommon.FguiUtils.DeleteWrapper(_compareFreeResultTip);
            GameCommon.FguiUtils.DeleteWrapper(_compareCatGirlRun);
            GameCommon.FguiUtils.DeleteWrapper(_compareRedRay);

            _compareFreeResultTip = null;
            _compareCatGirlRun = null;
            _compareRedRay = null;

            Object.Destroy(_cloneFreeResultTip);
            Object.Destroy(_cloneCatGirlRun);
            Object.Destroy(_cloneRedRay);

            _cloneFreeResultTip = null;
            _cloneCatGirlRun = null;
            _cloneRedRay = null;
        }
    }
}