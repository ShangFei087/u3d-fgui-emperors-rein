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
    public class PopupSmallGameResult : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupSmallGameResult";

        private const string PrefabPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupSmallGameResult/";

        private int _totalCount;
        private GButton _collectBtn;
        private bool _isClicked;
        private GTextField _scoreText;
        private GameSoundController3994 _gameSoundController;

        // Spine
        private Animator _triggerAnimator;
        private GComponent _compareTrigger, _compareFade;

        private GameObject _triggerObj, _cloneTriggerObj, _fadeObj, _cloneFadeObj;

        // 打开界面传入的数据
        private int _gameScore;
        private Action _changePage;

        // 挂点记录初始数据，方便后续还原
        private Quaternion _collectBtnQuaternion, _scoreQuaternion;
        private Vector3 _btnScale, _btnPos, _scoreScale, _scorePos;
        private Transform _collectBtnTran, _scoreTran, _parentTran;

        private TimerCallback
            _delayCloseCallback, _delayChangePageCallback, _delayPlayFadeCallback; // 延时关闭回调 延时切换界面 延时播放过渡动画回调

        private TimerCallback _autoClickCallback, _btnDelayCallback;

        /// <summary>当前实例绑定的语言，切语言时强制重绑。</summary>
        private I18nLang _boundLang;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 2;
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_Result.prefab",
                (clone) =>
                {
                    _triggerObj = clone;
                    ResLoadCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BackMain.prefab",
                (clone) =>
                {
                    _fadeObj = clone;
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

        private void InitParam(EventData eventData)
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = true;

            // 获取UI组件
            _collectBtn = contentPane.GetChild("collectBtn").asButton;
            _scoreText = contentPane.GetChild("scoreText").asTextField;
            _scoreText.text = _gameScore.ToString();

            // 保存初始信息
            _parentTran = contentPane.displayObject.gameObject.transform;
            _collectBtnTran = _collectBtn.displayObject.gameObject.transform;
            _scoreTran = _scoreText.displayObject.gameObject.transform;
            _btnScale = _collectBtnTran.localScale;
            _btnPos = _collectBtnTran.localPosition;
            _collectBtnQuaternion = _collectBtnTran.localRotation;
            _scoreScale = _scoreTran.localScale;
            _scorePos = _scoreTran.localPosition;
            _scoreQuaternion = _scoreTran.localRotation;

            // 绑定Spine
            GComponent currentCom = contentPane.GetChild("anchorResult").asCom;
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

            currentCom = contentPane.GetChild("anchorFade").asCom;
            if (currentCom != _compareFade)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFade);
                _compareFade = currentCom;
                _cloneFadeObj = Object.Instantiate(_fadeObj);
                _cloneFadeObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneFadeObj);
            }

            // 将UI挂载在Spine动画上
            GameObject fatherObj = _cloneTriggerObj.transform.GetChild(0).gameObject;
            string path = "Spine Mecanim GameObject (sg_bor_congrats2)/SkeletonUtility-SkeletonRoot/root/a/";
            Transform father = fatherObj.transform.Find(path + "collect");
            _collectBtnTran.SetParent(father, false);
            _collectBtnTran.localPosition = new Vector3(-2.12f, 0.9f, 0.01f);
            _collectBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _collectBtnTran.localRotation = Quaternion.Euler(0, 0, 0);

            father = fatherObj.transform.Find(path + "k/sz");
            _scoreTran.SetParent(father, false);
            _scoreTran.localPosition = new Vector3(-4.23f, 0.86f, 0f);
            _scoreTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _scoreTran.localRotation = Quaternion.Euler(0, 0, 0);
            
            // 按钮延时点击
            _collectBtn.touchable = false;
            _btnDelayCallback = (obj) =>
            {
                _collectBtn.touchable = true;
                _isClicked = false;
                _btnDelayCallback = null;
            };
            Timers.inst.Add(2f, 1, _btnDelayCallback);

            // 按钮点击事件
            _collectBtn.onClick.Clear();
            _collectBtn.onClick.Add(() => OnCloseBtn());

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_collectBtn != null && isOpen)
                    {
                        _collectBtn.onClick.Call();
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
                new EventData(Game3994AudioEvent.BgmBonusResult));

            // 获取事件信息
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changePage = args["changeNormalPage"] as Action;
                _gameScore = (int)args["smallTotalScore"];
            }

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);

            _gameSoundController?.Dispose();
            _gameSoundController = null;

            // 解除UI绑定
            _collectBtnTran.SetParent(_parentTran);
            _collectBtnTran.localPosition = _btnPos;
            _collectBtnTran.localScale = _btnScale;
            _collectBtnTran.localRotation = _collectBtnQuaternion;
            _scoreTran.SetParent(_parentTran);
            _scoreTran.localPosition = _scorePos;
            _scoreTran.localScale = _scoreScale;
            _scoreTran.localRotation = _scoreQuaternion;

            _cloneFadeObj.SetActive(false);
        }

        private void ResLoadCallback()
        {
            if (--_totalCount != 0) return;

            isInit = true;
            InitParam(null);
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            RemoveDesignCallBack(_delayCloseCallback);
            RemoveDesignCallBack(_delayChangePageCallback);
            RemoveDesignCallBack(_delayPlayFadeCallback);
            _delayCloseCallback = null;
            _delayChangePageCallback = null;
            _delayPlayFadeCallback = null;

            // 播放关闭动画并显示切换pag
            _scoreText.text = string.Empty;
            _collectBtn.visible = false;
            PlayAnimationByName(_triggerAnimator, "end");

            // 延时播放
            _delayPlayFadeCallback = CreateDelayCallback(() => _cloneFadeObj.SetActive(true), 0.2f);
            _delayChangePageCallback = CreateDelayCallback(() => _changePage?.Invoke(), 0.3f);
            _delayCloseCallback = CreateDelayCallback(() =>
            {
                if (isOpen) CloseSelf(null);
                _collectBtn.visible = true;
                _changePage = null;
            }, 1.7f);
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