using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string PrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinTrigger/";

        private int _totalCount = -1;
        private bool _isInitialized;
        private GTextField _freeRoundTxt;
        private GButton _freeStartBtn;

        private GComponent _freeTipWindow;

        // 记录挂点前数据
        private Vector3 _startBtnLocalScale, _numTextLocalScale, _startBtnLocalPos, _numTextLocalPos;

        // Spine
        private Animator _freeTriggerAni;
        private GComponent _freeTriggerCom;
        private GameObject _freeTriggerObj, _cloneFreeTriggerObj;

        // 定时器记录，方便使用后清除，避免内存泄漏
        private TimerCallback _autoClickCallback, _delayCloseCallback;

        private GameSoundController3997 _gameSoundController;
        private bool _isClicked = false;
        private Action _changeFreePage, _changeNpcAnimationClip;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(
                PrefabPath + "FreeTrigger.prefab",
                (clone) =>
                {
                    _freeTriggerObj = clone;
                    ResLoadedCallback();
                }); // 加载Spine

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnClickSpinButton(res);
                    },
                }
            };
        }

        private void InitParam(EventData eventData)
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;
            RemoveTimer(ref _autoClickCallback);

            // --------------------- 获取UI组件 ---------------------
            _freeTipWindow = contentPane.GetChild("freeTipWindow").asCom;
            _freeStartBtn = _freeTipWindow.GetChild("freeStartBtn").asButton;
            _freeRoundTxt = _freeTipWindow.GetChild("freeRound").asTextField;

            // --------------------- 记录UI组件初始信息 ---------------------
            Transform startBtnTran = _freeStartBtn.displayObject.gameObject.transform;
            Transform roundTran = _freeRoundTxt.displayObject.gameObject.transform;
            _startBtnLocalPos = startBtnTran.localPosition;
            _startBtnLocalScale = startBtnTran.localScale;
            _numTextLocalPos = roundTran.localPosition;
            _numTextLocalScale = roundTran.localScale;

            // --------------------- 绑定预制体到UI ---------------------
            GComponent currentGCom = _freeTipWindow.GetChild("freeTrigger").asCom;
            if (currentGCom != _freeTriggerCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_freeTriggerCom);
                _freeTriggerCom = currentGCom;
                _cloneFreeTriggerObj = Object.Instantiate(_freeTriggerObj);
                GameCommon.FguiUtils.AddWrapper(_freeTriggerCom, _cloneFreeTriggerObj);
            } // Spine
            _freeTriggerAni = _cloneFreeTriggerObj.GetComponentInChildren<Animator>();


            // --------------------- 将UI组件挂点到对应的Spine节点上 ---------------------
            string rootPath = "Anchor/Spine Mecanim GameObject (fg_pop_prompt)/SkeletonUtility-SkeletonRoot/root/All/";
            Transform btnTran = _cloneFreeTriggerObj.transform.Find(rootPath + "pop_all/btn");
            startBtnTran.SetParent(btnTran, false);
            startBtnTran.localPosition = new Vector3(-1.97f, 0.88f, 0);
            startBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Transform numTran = _cloneFreeTriggerObj.transform.Find(rootPath + "pop_all/pop/number");
            roundTran.SetParent(numTran, false);
            roundTran.localPosition = new Vector3(-2.9f, 2.3f, 0);
            roundTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // --------------------- 按钮点击事件 ---------------------


            _freeStartBtn.onClick.Clear();
            _freeStartBtn.onClick.Add(() => { OnClickSpinButton(null); });
            // 自动模式定时器
            if (!TestManager.Instance.IsAutoModeRunning) return;
            _autoClickCallback = (obj) =>
            {
                if (_freeStartBtn != null && _freeTipWindow is { visible: true } && isOpen)
                {
                    _freeStartBtn.onClick.Call();
                }

                _autoClickCallback = null;
            };
            Timers.inst.Add(3.0f, 1, _autoClickCallback);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmFreeSpinTrigger));

            if (eventData is { value: Dictionary<string, object> args })
            {
                _changeFreePage = args["changeFreePage"] as Action;
                _changeNpcAnimationClip = args["changeNpcAnimationClip"] as Action;
            }

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _changeFreePage = null;
            _changeNpcAnimationClip = null;

            _freeTriggerAni = null;

            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            // --------------------- 还原UI组件 ---------------------
            Transform startBtnTran = _freeStartBtn.displayObject.gameObject.transform;
            Transform roundTran = _freeRoundTxt.displayObject.gameObject.transform;
            Transform parentTran = _freeTipWindow.displayObject.gameObject.transform;
            startBtnTran.SetParent(parentTran, false);
            startBtnTran.localPosition = _startBtnLocalPos;
            startBtnTran.localScale = _startBtnLocalScale;
            roundTran.SetParent(parentTran, false);
            roundTran.localPosition = _numTextLocalPos;
            roundTran.localScale = _numTextLocalScale;

            _freeRoundTxt.visible = true;
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount != 0) return;
            _isInitialized = true;
            InitParam(null);
        }

        private void OnClickSpinButton(EventData res)
        {
            if (_isClicked) return;
            _isClicked = true;
            _freeStartBtn.visible = false;
            _freeRoundTxt.visible = false;
            PlayAnimationByName(_freeTriggerAni, "ng_fade_fg");
            RemoveTimer(ref _delayCloseCallback);
            _delayCloseCallback = (obj) =>
            {
                _changeFreePage?.Invoke();
                _changeNpcAnimationClip?.Invoke();
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
                _freeStartBtn.visible = true;
            };
            Timers.inst.Add(2.7f, 1, _delayCloseCallback);
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeGameFadeTransition));
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }

        private void PlayAnimationByName(Animator animator, string aniName, Action callback = null)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
            callback?.Invoke();
        }
    }
}