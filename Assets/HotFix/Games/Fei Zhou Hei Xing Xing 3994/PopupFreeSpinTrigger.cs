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
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupFreeSpinTrigger";
        private const string PagPath = "Games/Fei Zhou Hei Xing Xing 3994/Pag/";

        private const string PrefabPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupFreeSpinTrigger/";

        private int _totalCount = -1;
        private GButton _startBtn;
        private GTextField _spinCountText;
        private bool _isClicked;
        private GameSoundController3994 _gameSoundController;

        // Spine
        private GComponent _compareTrigger;
        private Animator _triggerAnimator;
        private GameObject _triggerObj, _cloneTriggerObj;

        // 打开界面传入的数据
        private int _gameCount;
        private Action _changePage;

        // 挂点记录初始数据，方便后续还原
        private Quaternion _startBtnQuaternion, _numQuaternion;
        private Vector3 _btnScale, _btnPos, _numScale, _numPos;
        private Transform _startBtnTran, _spinTextTran, _parentTran;

        // Pag
        private GComponent _fadeCom;
        private PagSlotBinding _fadePag;
        private readonly string _fade1920 = "PopupFreeSpinTrigger/fade.pag";

        private TimerCallback
            _autoClickCallback,
            _delayCloseCallback,
            _delayPlayPagCallback,
            _changePageCallback,
            _btnDelayCallback; // 延时关闭回调   延时播放Pag回调   切换界面回调

        /// <summary>当前实例绑定的语言，切语言时强制重绑。</summary>
        private I18nLang _boundLang;

        /// <summary>注册并 Prepare PAG 槽位（InitParam 时调用一次即可）。</summary>
        private void BindPagSlot()
        {
            // 绑定npc pag并默认播放idle动画
            _fadeCom = contentPane.GetChild("anchorFadePag").asCom;
            if (_fadeCom == null) return;
            _fadePag = new PagSlotBinding("fade", PagPath);
            _fadePag.EnsureSlot(_fadeCom);
        }

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

        private void InitParam(EventData eventData = null)
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = true;

            // 获取UI组件
            _startBtn = contentPane.GetChild("startBtn").asButton;
            _spinCountText = contentPane.GetChild("spinCountText").asTextField;
            _spinCountText.text = _gameCount.ToString();

            // 保存初始信息
            _parentTran = contentPane.displayObject.gameObject.transform;
            _startBtnTran = _startBtn.displayObject.gameObject.transform;
            _spinTextTran = _spinCountText.displayObject.gameObject.transform;
            _btnScale = _startBtnTran.localScale;
            _btnPos = _startBtnTran.localPosition;
            _startBtnQuaternion = _startBtnTran.localRotation;
            _numScale = _spinTextTran.localScale;
            _numPos = _spinTextTran.localPosition;
            _numQuaternion = _spinTextTran.localRotation;

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

            BindPagSlot();

            GameObject fatherObj = _cloneTriggerObj.transform.GetChild(0).gameObject;
            // 将UI挂载在Spine动画上
            string path = "Spine Mecanim GameObject (fg_bor_congrats2)/SkeletonUtility-SkeletonRoot/root/sx/";
            Transform father = fatherObj.transform.Find(path + "start");
            _startBtnTran.SetParent(father, false);
            _startBtnTran.localPosition = new Vector3(0.98f, 2.03f, 0.01f);
            _startBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _startBtnTran.localRotation = Quaternion.Euler(0, 0, -90);

            father = fatherObj.transform.Find(path + "number");
            _spinTextTran.SetParent(father, false);
            _spinTextTran.localPosition = new Vector3(2.05f, 1.93f, 0f);
            _spinTextTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _spinTextTran.localRotation = Quaternion.Euler(0, 0, 270);
            
            // 按钮延时点击
            _startBtn.touchable = false;
            _btnDelayCallback = (obj) =>
            {
                _startBtn.touchable = true;
                _isClicked = false;
                _btnDelayCallback = null;
            };
            Timers.inst.Add(1.333f, 1, _btnDelayCallback);

            // 按钮点击事件
            _startBtn.onClick.Clear();
            _startBtn.onClick.Add(() => OnCloseBtn(null));

            // 自动模式定时器
            if (!TestManager.Instance.IsAutoModeRunning) return;
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

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3994();
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3994AudioEvent.BgmFreeSpinTrigger));

            // 获取事件信息
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changePage = args["changeFreePage"] as Action;
                _gameCount = (int)args["freeSpinCount"];
            }

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _gameSoundController?.Dispose();
            _gameSoundController = null;

            // _fadePag?.Dispose();
            _fadeCom = null;
            if (_fadePag != null) _fadePag.StopWithDefaults();

            // 解除UI绑定
            _startBtnTran.SetParent(_parentTran);
            _startBtnTran.localPosition = _btnPos;
            _startBtnTran.localScale = _btnScale;
            _startBtnTran.localRotation = _startBtnQuaternion;
            _spinTextTran.SetParent(_parentTran);
            _spinTextTran.localPosition = _numPos;
            _spinTextTran.localScale = _numScale;
            _spinTextTran.localRotation = _numQuaternion;
        }

        private void ResLoadCallback(EventData eventData = null)
        {
            if (--_totalCount != 0) return;
            isInit = true;
            InitParam(eventData);
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

            // 播放关闭动画并显示切换pag
            _spinCountText.text = string.Empty;
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
            }, 3.5f);
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