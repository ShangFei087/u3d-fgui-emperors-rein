using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupFreeSpinResult : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupFreeSpinResult";

        private const string PrefabPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupFreeSpinResult/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private GButton _freeCollectBtn;
        private GComponent _freeResultTipWindow;
        private GComponent _freeGameResultScore;
        private GTextField _freeResultText;

        // Spine
        private Animator _freeResultAni;
        private GComponent _freeResultCom;
        private GameObject _freeResultObj, _cloneFreeResultObj;

        // 记录UI初始位置
        private Vector3 _collectBtnLocalScale, _numTextLocalScale, _collectBtnLocalPos, _numTextLocalPos;

        // 计时器回调
        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;

        private bool _isClicked = false;
        private Action _changeNormalPage, _changeNpcAnimationClip;

        /// <summary>当前实例绑定的语言，切语言时强制重绑。</summary>
        private I18nLang _boundLang;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 1;
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "FreeResult.prefab",
                (clone) =>
                {
                    _freeResultObj = clone;
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

            // -------------------------- 获取UI组件 -----------------------
            _freeResultTipWindow = contentPane.GetChild("freeResultTipWindow").asCom;
            _freeCollectBtn = _freeResultTipWindow.GetChild("collectBtn").asButton;
            _freeGameResultScore = _freeResultTipWindow.GetChild("freeGameResultScore").asCom;
            _freeResultText = _freeGameResultScore.GetChild("number").asTextField;
            _freeResultText.text = ContentModel.Instance.freeSpinTotalWinCoins.ToString();

            // -------------------------- 记录UI初始位置 -----------------------
            Transform collectBtnTran = _freeCollectBtn.displayObject.gameObject.transform;
            Transform roundTran = _freeResultText.displayObject.gameObject.transform;
            _collectBtnLocalPos = collectBtnTran.localPosition;
            _collectBtnLocalScale = collectBtnTran.localScale;
            _numTextLocalPos = roundTran.localPosition;
            _numTextLocalScale = roundTran.localScale;


            // -------------------------- 绑定Prefab到UI上 -----------------------
            GComponent currentGCom = _freeResultTipWindow.GetChild("freeResult").asCom;
            if (currentGCom != _freeResultCom || _boundLang != PopupSpineLang3997.CurrentLang)
            {
                GameCommon.FguiUtils.DeleteWrapper(_freeResultCom);
                _freeResultCom = currentGCom;
                _cloneFreeResultObj = Object.Instantiate(_freeResultObj);
                PopupSpineLang3997.Apply(_cloneFreeResultObj);
                _boundLang = PopupSpineLang3997.CurrentLang;
                GameCommon.FguiUtils.AddWrapper(_freeResultCom, _cloneFreeResultObj);
            } // Spine

            _freeResultAni = _cloneFreeResultObj.GetComponentInChildren<Animator>();


            // --------------------- 将UI组件挂点到对应的Spine节点上 ---------------------
            GameObject fatherObj = _cloneFreeResultObj.transform.GetChild(0).GetChild(0).gameObject;
            string rootPath = "Spine Mecanim GameObject (fg_pop_settlement)/SkeletonUtility-SkeletonRoot/root/all/";
            Transform btnTran = fatherObj.transform.Find(rootPath + "btn");
            collectBtnTran.SetParent(btnTran, false);
            collectBtnTran.localPosition = new Vector3(-1.92f, 0.84f, 0);
            collectBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Transform numTran = fatherObj.transform.Find(rootPath + "sx_1/num");
            roundTran.SetParent(numTran, false);
            roundTran.localPosition = new Vector3(-5.31f, 1.53f, 0);
            roundTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);


            _freeCollectBtn.onClick.Clear();
            _freeCollectBtn.onClick.Add(() => OnClickSpinButton(null));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_freeCollectBtn != null && _freeResultTipWindow is { visible: true } && isOpen)
                    {
                        _freeCollectBtn.onClick.Call();
                    }

                    _autoClickCallback = null;
                };
                Timers.inst.Add(3.0f, 1, _autoClickCallback);
            }
        }

        private void OnClickSpinButton(EventData eventData)
        {
            if (_isClicked) return;
            _isClicked = true;
            _freeCollectBtn.visible = false;
            RemoveTimer(ref _delayCloseCallback);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeGameFadeTransition));

            PlayAnimationByName(_freeResultAni, "fg_fade_ng");
            _freeResultText.visible = false;

            _delayCloseCallback = (obj) =>
            {
                _changeNormalPage?.Invoke();
                _changeNpcAnimationClip?.Invoke();
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
                _freeCollectBtn.visible = true;
            };
            Timers.inst.Add(1.8f, 1, _delayCloseCallback);
        }

        private GameSoundController3997 _gameSoundController;

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3997();
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmFreeSpinResult));

            if (eventData is { value: Dictionary<string, object> args })
            {
                _changeNormalPage = args["changeNormalPage"] as Action;
                _changeNpcAnimationClip = args["changeNpcAnimationClip"] as Action;
            }

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmRegularGame));
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _changeNormalPage = null;
            _changeNpcAnimationClip = null;
            _freeResultAni = null;
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            // ------------------------ 复原UI父物体 ----------------------
            Transform startBtnTran = _freeCollectBtn.displayObject.gameObject.transform;
            Transform roundTran = _freeResultText.displayObject.gameObject.transform;
            Transform parentTran = _freeResultTipWindow.displayObject.gameObject.transform;
            startBtnTran.SetParent(parentTran, false);
            startBtnTran.localPosition = _collectBtnLocalPos;
            startBtnTran.localScale = _collectBtnLocalScale;
            roundTran.SetParent(parentTran, false);
            roundTran.localPosition = _numTextLocalPos;
            roundTran.localScale = _numTextLocalScale;

            _freeResultText.visible = true;
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam(null);
            }
        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        /// <remarks>使用ref关键字按引用传递，使得方法内部对参数的置空操作能够直接反映到外部的成员变量上，避免悬挂引用</remarks>>
        /// <param name="timerCallback"></param>
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