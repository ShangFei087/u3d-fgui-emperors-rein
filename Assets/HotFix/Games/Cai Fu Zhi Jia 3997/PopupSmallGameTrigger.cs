using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupSmallGameTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupSmallGameTrigger";

        private const string PrefabPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupSmallGameTrigger/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private GButton _startBtn;
        private GComponent _jackpotTriggerTipWindow;

        // Spine动画
        private Animator _smallGameTriggerAnimator;
        private GComponent _smallGameFadeCom, _smallGameTriggerCom;
        private GameObject _smallGameFadeObj, _cloneSmallGameFadeObj, _smallGameTriggerObj, _cloneSmallGameTriggerObj;

        // 记录挂点前数据
        private Vector3 _startBtnLocalScale, _startBtnLocalPos;
        private bool _isClicked = false;
        private GameSoundController3997 _gameSoundController;

        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;

        private Action _changeSmallGamePage;

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
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "SmallGameTrigger.prefab",
                (clone) =>
                {
                    _smallGameTriggerObj = clone;
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

            // ------------------ 获取UI组件 -----------------------
            _jackpotTriggerTipWindow = contentPane.GetChild("jackpotTriggerTipWindow").asCom;
            _startBtn = _jackpotTriggerTipWindow.GetChild("jackpotTriggerButton").asButton;

            // --------------------- 记录UI组件初始信息 ---------------------
            Transform startBtnTran = _startBtn.displayObject.gameObject.transform;
            _startBtnLocalPos = startBtnTran.localPosition;
            _startBtnLocalScale = startBtnTran.localScale;
            
            // ------------------ 绑定prefab到UI上 -----------------------
            GComponent currentGCom = _jackpotTriggerTipWindow.GetChild("smallFade").asCom;
            if (currentGCom != _smallGameFadeCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_smallGameFadeCom);
                _smallGameFadeCom = currentGCom;
                _cloneSmallGameFadeObj = Object.Instantiate(_smallGameFadeObj);
                _cloneSmallGameFadeObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_smallGameFadeCom, _cloneSmallGameFadeObj);
            }// 过场动画

            currentGCom = _jackpotTriggerTipWindow.GetChild("smallTrigger").asCom;
            if (currentGCom != _smallGameTriggerCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_smallGameTriggerCom);
                _smallGameTriggerCom = currentGCom;
                _cloneSmallGameTriggerObj = Object.Instantiate(_smallGameTriggerObj);
                _smallGameTriggerAnimator = _cloneSmallGameTriggerObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(_smallGameTriggerCom, _cloneSmallGameTriggerObj);
            }// 触发弹窗
            
            // ------------------ 将UI组件挂点到对应的Spine节点上 -----------------------
            string rootPath = "Anchor/Spine Mecanim GameObject (sg_pop_frame)/SkeletonUtility-SkeletonRoot/root/zong/zi/";
            Transform btnTran = _cloneSmallGameTriggerObj.transform.Find(rootPath + "btn01");
            startBtnTran.SetParent(btnTran, false);
            startBtnTran.localPosition = new Vector3(-1.62f, 2.13f, 0);
            startBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;
            RemoveTimer(ref _autoClickCallback);

            // -------------------------- 添加UI点击事件 --------------------------
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changeSmallGamePage = args["changeSmallGamePage"] as Action;
            }

            _startBtn.onClick.Clear();
            _startBtn.onClick.Add(() => OnClickSpinButton(null));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_startBtn != null && _jackpotTriggerTipWindow != null &&
                        _jackpotTriggerTipWindow.visible && isOpen)
                    {
                        _startBtn.onClick.Call();
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

            _cloneSmallGameFadeObj.SetActive(true);
            _cloneSmallGameTriggerObj.SetActive(false);
            RemoveTimer(ref _delayCloseCallback);

            _delayCloseCallback = (obj) =>
            {
                _changeSmallGamePage?.Invoke();
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(3f, 1, _delayCloseCallback);

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
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
            _changeSmallGamePage = null;

            // 清除回调
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            // 还原UI位置
            Transform startBtnTran = _startBtn.displayObject.gameObject.transform;
            Transform parentTran = _jackpotTriggerTipWindow.displayObject.gameObject.transform;
            startBtnTran.SetParent(parentTran, false);
            startBtnTran.localPosition = _startBtnLocalPos;
            startBtnTran.localScale = _startBtnLocalScale;
            
            // 还原预制体初始显隐状态
            _cloneSmallGameFadeObj.SetActive(false);
            _cloneSmallGameTriggerObj.SetActive(true);
        }

        private void ResLoadedCallback(EventData eventData = null)
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam(eventData);
            }
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;

            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}