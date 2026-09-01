using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupSmallGameResult : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupSmallGameResult";

        private const string PrefabPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupSmallGameResult/";

        private int _totalCount = -1;
        private bool _isInitialized;
        private GButton _collectBtn;
        private GTextField _numText;
        private GComponent _jackpotResultTipWindow;

        // Spine动画
        private GComponent _smallGameFadeCom, _smallGameResultCom;

        private Animator _tipAni;
        private GameObject _smallGameFadeObj, _cloneSmallGameFadeObj, _smallGameResultObj, _cloneSmallGameResultObj;

        // 记录UI初始位置
        private Vector3 _collectBtnLocalScale, _numTextLocalScale, _collectBtnLocalPos, _numTextLocalPos;

        private bool _isClicked;
        private GameSoundController3997 _gameSoundController;
        private TimerCallback _delayCloseCallback;
        private TimerCallback _delayPlayEndCallback;
        private TimerCallback _autoClickCallback;
        private Action _changeNormalPage, _changeNpcAnimationClip;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 2;
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "SmallGameFade.prefab",
                (clone) =>
                {
                    _smallGameFadeObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "SmallGameResult.prefab",
                (clone) =>
                {
                    _smallGameResultObj = clone;
                    ResLoadedCallback();
                });

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

            // --------------------- 获取UI组件 ------------------------
            _jackpotResultTipWindow = contentPane.GetChild("jackpotResultTipWindow").asCom;
            _collectBtn = _jackpotResultTipWindow.GetChild("jackpotResultButton").asButton;
            _numText = _jackpotResultTipWindow.GetChild("jackpotResultScore").asCom.GetChild("number").asTextField;
            _numText.text = ContentModel.Instance.totalBonusReward.ToString();

            // -------------------------- 记录UI初始位置 -----------------------
            Transform collectBtnTran = _collectBtn.displayObject.gameObject.transform;
            Transform numTxt = _numText.displayObject.gameObject.transform;
            _collectBtnLocalPos = collectBtnTran.localPosition;
            _collectBtnLocalScale = collectBtnTran.localScale;
            _numTextLocalPos = numTxt.localPosition;
            _numTextLocalScale = numTxt.localScale;

            // ----------------------- 绑定Prefab到UI -------------------------
            GComponent currentGCom = _jackpotResultTipWindow.GetChild("smallResult").asCom;
            if (currentGCom != _smallGameResultCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_smallGameResultCom);
                _smallGameResultCom = currentGCom;
                _cloneSmallGameResultObj = Object.Instantiate(_smallGameResultObj);
                GameCommon.FguiUtils.AddWrapper(_smallGameResultCom, _cloneSmallGameResultObj);
            } // 结算弹窗
            _tipAni = _cloneSmallGameResultObj.GetComponentInChildren<Animator>();


            currentGCom = _jackpotResultTipWindow.GetChild("smallFade").asCom;
            if (currentGCom != _smallGameFadeCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_smallGameFadeCom);
                _smallGameFadeCom = currentGCom;
                _cloneSmallGameFadeObj = Object.Instantiate(_smallGameFadeObj);
                _cloneSmallGameFadeObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_smallGameFadeCom, _cloneSmallGameFadeObj);
            } // 过场动画

            // ------------------ 将UI组件挂点到对应的Spine节点上 -----------------------
            string rootPath =
                "Anchor/Spine Mecanim GameObject (sg_pop_frame)/SkeletonUtility-SkeletonRoot/root/zong/zi/";
            Transform btnTran = _cloneSmallGameResultObj.transform.Find(rootPath + "btn01");
            collectBtnTran.SetParent(btnTran, false);
            collectBtnTran.localPosition = new Vector3(-1.87f, 2.69f, 0);
            collectBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Transform numTran = _cloneSmallGameResultObj.transform.Find(rootPath + "num01");
            numTxt.SetParent(numTran, false);
            numTxt.localPosition = new Vector3(-5.46f, 1.42f, 0);
            numTxt.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // ----------------------- 按钮点击事件 -------------------------
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changeNormalPage = args["changeNormalPage"] as Action;
                _changeNpcAnimationClip = args["changeNpcAnimationClip"] as Action;
            }

            _collectBtn.onClick.Clear();
            _collectBtn.onClick.Add(() => OnClickSpinButton(null));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_collectBtn != null && _jackpotResultTipWindow != null &&
                        _jackpotResultTipWindow.visible && isOpen)
                    {
                        _collectBtn.onClick.Call();
                    }

                    _autoClickCallback = null;
                };
                Timers.inst.Add(3.0f, 1, _autoClickCallback);
            }
        }

        private void OnClickSpinButton(EventData res)
        {
            if (_isClicked) return;
            _isClicked = true;
            RemoveTimer(ref _delayPlayEndCallback);
            RemoveTimer(ref _delayCloseCallback);
            _collectBtn.visible = false;

            PlayAnimationByName(_tipAni, "end");
            _delayPlayEndCallback = (obj) =>
            {
                _cloneSmallGameFadeObj.SetActive(true);
                _cloneSmallGameResultObj.SetActive(false);
                _delayPlayEndCallback = null;
            };
            Timers.inst.Add(1.667f, 1, _delayPlayEndCallback);


            _delayCloseCallback = (obj) =>
            {
                _changeNormalPage?.Invoke();
                _changeNpcAnimationClip?.Invoke();
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
                _collectBtn.visible = true;
            };
            Timers.inst.Add(4.667f, 1, _delayCloseCallback);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmBonusTrigger));
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);

            // 清除音效
            _gameSoundController?.Dispose();
            _gameSoundController = null;

            // 清除Main界面委托
            _changeNormalPage = null;
            _changeNpcAnimationClip = null;

            _tipAni = null;

            // 清除回调
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            // 还原UI位置
            Transform startBtnTran = _collectBtn.displayObject.gameObject.transform;
            Transform numTxt = _numText.displayObject.gameObject.transform;
            Transform parentTran = _jackpotResultTipWindow.displayObject.gameObject.transform;
            startBtnTran.SetParent(parentTran, false);
            startBtnTran.localPosition = _collectBtnLocalPos;
            startBtnTran.localScale = _collectBtnLocalScale;
            numTxt.SetParent(parentTran, false);
            numTxt.localPosition = _numTextLocalPos;
            numTxt.localScale = _numTextLocalScale;

            // 还原预制体初始显隐状态
            _cloneSmallGameFadeObj.SetActive(false);
            _cloneSmallGameResultObj.SetActive(true);
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount != 0) return;
            _isInitialized = true;
            InitParam(null);
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