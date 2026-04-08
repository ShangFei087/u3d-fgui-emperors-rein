using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupJackpotTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupJackpotTrigger";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupJackpotTrigger/SpinePrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;

        private GButton _jackpotStartBtn;
        private GTextField _jackpotRoundText;
        private GComponent _compareJpPupCollectFrame, _compareTransitionFgToJp;

        private GameObject _jpPupCollectFrameObj,
            _cloneJpPupCollectFrameObj,
            _transitionFgToJpObj,
            _cloneTransitionFgToJpObj;

        private Animator _animator;
        private Transform _effectTransform;

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
            _jackpotStartBtn = contentPane.GetChild("jackpotStartBtn").asButton;
            _jackpotRoundText = contentPane.GetChild("jackpotRoundText").asTextField;
            _jackpotRoundText.visible = false;
            Timers.inst.Add(3.3f, 1, (obj) =>
            {
                _jackpotRoundText.visible = true;
                _cloneJpPupCollectFrameObj.SetActive(true);
            });
            _jackpotStartBtn.onClick.Add((() =>
            {
                _jackpotRoundText.visible = false;
                _cloneJpPupCollectFrameObj.SetActive(false);
                PlayAnimationByName(_animator, "transition");
                Timers.inst.Add(2.5f,1, (obj) =>
                {
                    _effectTransform.gameObject.SetActive(true);
                });
                Timers.inst.Add(3.8f, 1, (obj) =>
                {
                    CloseSelf(null);
                });
            }));
        }

        private void LoadResAsync()
        {
            _resCount = 2;
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "jp_pup_CollectFrame.prefab",
                (cloneObj) =>
                {
                    _jpPupCollectFrameObj = cloneObj;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Transition_FgToJp.prefab",
                (cloneObj) =>
                {
                    _transitionFgToJpObj = cloneObj;
                    ResLoadedCallback();
                });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentCom = contentPane.GetChild("anchor_JpPupCollectFrame").asCom;
            if (_compareJpPupCollectFrame != currentCom)
            {
                _cloneJpPupCollectFrameObj = Object.Instantiate(_jpPupCollectFrameObj);
                _cloneJpPupCollectFrameObj.SetActive(false);
                GameCommon.FguiUtils.DeleteWrapper(_compareJpPupCollectFrame);
                _compareJpPupCollectFrame = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareJpPupCollectFrame, _cloneJpPupCollectFrameObj);
            }

            currentCom = contentPane.GetChild("anchor_Transition_FgToJp").asCom;
            if (_compareTransitionFgToJp != currentCom)
            {
                _cloneTransitionFgToJpObj = Object.Instantiate(_transitionFgToJpObj);
                _animator = _cloneTransitionFgToJpObj.GetComponentInChildren<Animator>();
                _effectTransform = _cloneTransitionFgToJpObj.transform.Find("Effect").transform.Find("fg_eff_pup_TipFrame");
                _effectTransform.gameObject.SetActive(false);
                GameCommon.FguiUtils.DeleteWrapper(_compareTransitionFgToJp);
                _compareTransitionFgToJp = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareTransitionFgToJp, _cloneTransitionFgToJpObj);
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
            Object.Destroy(_cloneJpPupCollectFrameObj);
            Object.Destroy(_cloneTransitionFgToJpObj);
            GameCommon.FguiUtils.DeleteWrapper(_compareJpPupCollectFrame);
            GameCommon.FguiUtils.DeleteWrapper(_compareTransitionFgToJp);

            _compareTransitionFgToJp = null;
            _compareJpPupCollectFrame = null;
            _cloneJpPupCollectFrameObj = null;
            _cloneTransitionFgToJpObj = null;
        }
    }
}