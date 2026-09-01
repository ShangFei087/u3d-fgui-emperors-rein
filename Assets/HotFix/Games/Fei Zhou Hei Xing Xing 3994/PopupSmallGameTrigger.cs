using FairyGUI;
using GameMaker;
using HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FeiZhouHeiXingXing_3994
{
    public class PopupSmallGameTrigger : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupSmallGameTrigger";
        private const string PagPath = "Games/Fei Zhou Hei Xing Xing 3994/Pag/";

        private const string PrefabPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupSmallGameTrigger/";

        private int _totalCount;
        private GButton _startBtn;
        private bool _isClicked;
        private GameSoundController3994 _gameSoundController;

        // Spine动画
        private Animator _triggerAnimator;
        private GComponent _compareTrigger;
        private GameObject _triggerObj, _cloneTriggerObj;

        // 挂点记录初始数据，方便后续还原
        private Vector3 _btnScale, _btnPos;
        private Quaternion _startBtnQuaternion;
        private Transform _startBtnTran, _parentTran;

        // Pag视频
        private GComponent _fadeCom;
        private PagSlotBinding _fadePag;
        private readonly string _fade1920 = "PopupSmallGameTrigger/fade.pag";

        private TimerCallback _delayCloseCallback, _delayPlayPagCallback, _changePageCallback; // 延时关闭回调   延时播放Pag回调

        private Action _changePage; // 切换游戏界面的回调

        private TimerCallback _autoClickCallback;

        /// <summary>当前实例绑定的语言，切语言时强制重绑。</summary>
        private I18nLang _boundLang;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_Trigger.prefab",
                (clone) =>
                {
                    _triggerObj = clone;
                    ResLoadCallback();
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
                        OnCloseBtn(res);
                    },
                }
            };
        }

        public override void InitParam()
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            _isClicked = false;

            // 获取UI组件
            _startBtn = contentPane.GetChild("startBtn").asButton;
            _fadeCom = contentPane.GetChild("anchorFadePag").asCom;

            // 保存初始信息
            _parentTran = contentPane.displayObject.gameObject.transform;
            _startBtnTran = _startBtn.displayObject.gameObject.transform;
            _btnScale = _startBtnTran.localScale;
            _btnPos = _startBtnTran.localPosition;
            _startBtnQuaternion = _startBtnTran.localRotation;

            // 绑定Spine
            GComponent currentCom = contentPane.GetChild("anchorTrigger").asCom;
            if (currentCom != _compareTrigger || _boundLang != PopupLang3994.CurrentLang)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareTrigger);
                _compareTrigger = currentCom;
                _cloneTriggerObj = Object.Instantiate(_triggerObj);
                PopupLang3994.Apply(_cloneTriggerObj);
                _boundLang = PopupLang3994.CurrentLang;
                _triggerAnimator = _cloneTriggerObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneTriggerObj);
            }

            // 绑定Pag
            if (_fadeCom == null) return;
            _fadePag = new PagSlotBinding("fade", PagPath);
            _fadePag.EnsureSlot(_fadeCom);

            // 将UI挂载在Spine动画上
            GameObject fatherObj = _cloneTriggerObj.transform.GetChild(0).gameObject;
            string path = "Spine Mecanim GameObject (sg_bor_congrats)/SkeletonUtility-SkeletonRoot/root/st1/";
            Transform father = fatherObj.transform.Find(path + "start");
            _startBtnTran.SetParent(father, false);
            _startBtnTran.localPosition = new Vector3(0.98f, 2.03f, 0.01f);
            _startBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _startBtnTran.localRotation = Quaternion.Euler(0, 0, -92);

            // 按钮点击事件
            _startBtn.onClick.Clear();
            _startBtn.onClick.Add(() => OnCloseBtn());

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_startBtn != null && isOpen)
                    {
                        _startBtn.onClick.Call();
                    }

                    _autoClickCallback = null;
                };
                Timers.inst.Add(3.0f, 1, _autoClickCallback);
            }
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            _gameSoundController = new GameSoundController3994();
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3994AudioEvent.BgmBonusTrigger));

            // 获取事件信息
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changePage = args["changeSmallGamePage"] as Action;
            }

            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);

            // 清除音频残留
            _gameSoundController?.Dispose();
            _gameSoundController = null;

            // 清除Pag残留
            // _fadePag?.Dispose();
            _fadeCom = null;
            if (_fadePag != null) _fadePag.StopWithDefaults();

            // 解除UI绑定
            _startBtnTran.SetParent(_parentTran);
            _startBtnTran.localPosition = _btnPos;
            _startBtnTran.localScale = _btnScale;
            _startBtnTran.localRotation = _startBtnQuaternion;
        }

        private void ResLoadCallback()
        {
            if (--_totalCount != 0) return;

            isInit = true;
            InitParam();
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;

            // 只能点击一次
            _isClicked = true;

            // 清除回调
            RemoveDesignCallBack(_delayCloseCallback);
            RemoveDesignCallBack(_delayPlayPagCallback);
            RemoveDesignCallBack(_changePageCallback);
            _delayCloseCallback = null;
            _delayPlayPagCallback = null;
            _changePageCallback = null;

            // 播放动画
            _startBtn.visible = false;
            PlayAnimationByName(_triggerAnimator, "end");

            // end动画播放结束之后接着播放Pag视频
            _delayPlayPagCallback = CreateDelayCallback(() =>
            {
                PlayDesignPag(_fadePag, _fade1920, 1);
            }, 0.5f);

            _changePageCallback = CreateDelayCallback(() =>
            {
                _changePage?.Invoke();
            }, 2f);

            // 播放Pag视频之后关闭界面
            _delayCloseCallback = CreateDelayCallback(() =>
            {
                if (isOpen) CloseSelf(null);
                _startBtn.visible = true;
                _changePage = null;
            }, 4.58f);
        }

        /// <summary>播放指定Pag文件</summary>
        private void PlayDesignPag(PagSlotBinding pagSlot, string pagName, int loopCount = -1)
        {
            if (pagSlot == null) return;
            pagSlot.StopWithDefaults();
            pagSlot.Play(pagName, loopCount);
        }

        private void PlayAnimationByName(Animator animator, string aniName, Action callback = null)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
            callback?.Invoke();
        }

        /// <summary> 移除指定的回调 </summary>
        private void RemoveDesignCallBack(TimerCallback designCallback)
        {
            if (designCallback == null) return;
            Timers.inst.Remove(designCallback);
        }

        /// <summary> 创建延迟回调并注册到 Timers，返回 TimerCallback 供调用方存储以便后续 Remove </summary>
        private TimerCallback CreateDelayCallback(Action callBack, float delayTime)
        {
            TimerCallback callback = null;
            callback = obj =>
            {
                callBack?.Invoke();
                callback = null;
            };
            Timers.inst.Add(delayTime, 1, callback);
            return callback;
        }
    }
}