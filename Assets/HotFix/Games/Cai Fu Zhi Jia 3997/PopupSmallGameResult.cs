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

        // 用的资源和免费触发的资源是一样的，所以路径不需要修改
        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotTrigger/EffectPrefabs/";

        private const string AnimationPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotResult/AnimationPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private GButton _jackpotResultButton;
        private GComponent _jackpotResultTipWindow;

        // Spine
        private GameObject _diamondSpineObj;
        private GameObject _cloneDiamondSpineObj;
        private GComponent _compareDiamondSpineGCom;

        // Effect
        private GameObject _diamondBgEffectObj, _lightEffectObj;
        private GameObject _cloneDiamondBgEffectObj, _cloneLightEffectObj;
        private GComponent _compareDiamondBgEffectGCom, _compareLightEffectGCom;

        // Todo：等Animation做出来之后，直接取消注释即可
        // Animation
        private GameObject _diamondAnimationObj;
        private GameObject _cloneDiamondAnimationObj;
        private GComponent _compareDiamondAnimationGCom;

        private Transform _jackpotResultButtonOriginalParent;
        private Vector3 _jackpotResultButtonOriginalPos;
        private Vector3 _jackpotResultButtonOriginalScale;
        private Transform _jackpotResultScoreOriginalParent;
        private Vector3 _jackpotResultScoreOriginalPos;
        private Vector3 _jackpotResultScoreOriginalScale;

        private bool _isClicked = false;
        private GameSoundController3997 _gameSoundController;
        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;
        private Action _changeNormalPage;


        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 4;
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "diamondSpine.prefab",
                (clone) =>
                {
                    _diamondSpineObj = clone;
                    ResLoadedCallback();
                }); // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "diamondBgEffect.prefab",
                (clone) =>
                {
                    _diamondBgEffectObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "lightEffect.prefab",
                (clone) =>
                {
                    _lightEffectObj = clone;
                    ResLoadedCallback();
                }); // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(
                AnimationPrefabPath + "diamondAnimation.prefab",
                (clone) =>
                {
                    _diamondAnimationObj = clone;
                    ResLoadedCallback();
                }); // 加载Animation

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

        private EventData _openData;

        private void InitParam(EventData eventData)
        {
            if (eventData != null) _openData = eventData;
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;
            RemoveTimer(ref _autoClickCallback);

            // --------------------- 获取UI组件 ------------------------
            _jackpotResultTipWindow = contentPane.GetChild("jackpotResultTipWindow").asCom;
            _jackpotResultButton = _jackpotResultTipWindow.GetChild("jackpotResultButton").asButton;
            _jackpotResultTipWindow.GetChild("jackpotResultScore").asCom.GetChild("number").asTextField.text =
                ContentModel.Instance.totalBonusReward.ToString();

            // ----------------------- 绑定Prefab到UI -------------------------
            GComponent currentGCom = contentPane.GetChild("diamondSpine").asCom;
            if (currentGCom != _compareDiamondSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondSpineGCom);
                _compareDiamondSpineGCom = currentGCom;
                _cloneDiamondSpineObj = Object.Instantiate(_diamondSpineObj);
                _cloneDiamondSpineObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondSpineGCom, _cloneDiamondSpineObj);
            } // Spine

            currentGCom = contentPane.GetChild("diamondBgEffect").asCom;
            if (currentGCom != _compareDiamondBgEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondBgEffectGCom);
                _compareDiamondBgEffectGCom = currentGCom;
                _cloneDiamondBgEffectObj = Object.Instantiate(_diamondBgEffectObj);
                _cloneDiamondBgEffectObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondBgEffectGCom, _cloneDiamondBgEffectObj);
            } // Effect

            currentGCom = contentPane.GetChild("lightEffect").asCom;
            if (currentGCom != _compareLightEffectGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareLightEffectGCom);
                _compareLightEffectGCom = currentGCom;
                _cloneLightEffectObj = Object.Instantiate(_lightEffectObj);
                GameCommon.FguiUtils.AddWrapper(_compareLightEffectGCom, _cloneLightEffectObj);
            }

            currentGCom = _jackpotResultTipWindow.GetChild("diamondAnimation").asCom;
            if (currentGCom != _compareDiamondAnimationGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareDiamondAnimationGCom);
                _compareDiamondAnimationGCom = currentGCom;
                _cloneDiamondAnimationObj = Object.Instantiate(_diamondAnimationObj);
                GameCommon.FguiUtils.AddWrapper(_compareDiamondAnimationGCom, _cloneDiamondAnimationObj);
            } // Animation

            // ------------------------- 绑定UI到Animator -------------------------
            string parentPath = $"Anchor/sg_pop_settlement/Animation/btn";
            Transform num01 = _cloneDiamondAnimationObj.transform.Find(parentPath);
            GObject gObject = contentPane.GetChild("jackpotResultTipWindow").asCom.GetChild("jackpotResultButton");
            if (gObject?.displayObject?.gameObject != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;

                if (_jackpotResultButtonOriginalParent == null)
                {
                    _jackpotResultButtonOriginalParent = t.parent;
                    _jackpotResultButtonOriginalPos = t.localPosition;
                    _jackpotResultButtonOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-1.76f, 0.34f, 0);
                t.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            } // jackpotResultButton 按钮

            parentPath = $"Anchor/sg_pop_settlement/Animation/numdi";
            num01 = _cloneDiamondAnimationObj.transform.Find(parentPath);
            gObject = contentPane.GetChild("jackpotResultTipWindow").asCom.GetChild("jackpotResultScore").asCom;
            if (gObject?.displayObject?.gameObject != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;

                if (_jackpotResultScoreOriginalParent == null)
                {
                    _jackpotResultScoreOriginalParent = t.parent;
                    _jackpotResultScoreOriginalPos = t.localPosition;
                    _jackpotResultScoreOriginalScale = t.localScale;
                }

                t.SetParent(num01, false);
                t.localPosition = new Vector3(-2.83f, 0.01f, 0);
                t.localScale = new Vector3(0.005f, 0.005f, 0.01f);
            } // jackpotResultScore 分数

            // ----------------------- 按钮点击事件 -------------------------
            if (eventData is { value: Dictionary<string, object> args })
            {
                _changeNormalPage = args["changeNormalPage"] as Action;
            }

            _jackpotResultButton.onClick.Clear();
            _jackpotResultButton.onClick.Add(() => OnClickSpinButton(null));

            if (TestManager.Instance.IsAutoModeRunning)
            {
                _autoClickCallback = (obj) =>
                {
                    if (_jackpotResultButton != null && _jackpotResultTipWindow != null &&
                        _jackpotResultTipWindow.visible && isOpen)
                    {
                        _jackpotResultButton.onClick.Call();
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
            RemoveTimer(ref _delayCloseCallback);

            _jackpotResultTipWindow.visible = false;
            _cloneLightEffectObj.SetActive(false);
            _cloneDiamondAnimationObj.SetActive(false);
            _cloneDiamondSpineObj.SetActive(true);
            _cloneDiamondBgEffectObj.SetActive(true);

            _delayCloseCallback = (obj) =>
            {
                _changeNormalPage?.Invoke();
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(5, 1, _delayCloseCallback);
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
            Debug.Log("PopupJackpotResult OnClose");

            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _changeNormalPage = null;


            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _delayCloseCallback);

            _jackpotResultTipWindow.visible = true;
            _cloneLightEffectObj.SetActive(true);
            _cloneDiamondAnimationObj.SetActive(true);
            _cloneDiamondSpineObj.SetActive(false);
            _cloneDiamondBgEffectObj.SetActive(false);

            // ------------- 恢复UI到原始位置 ---------------------  
            GObject gObject = _jackpotResultTipWindow.GetChild("jackpotResultButton");
            if (gObject?.displayObject?.gameObject != null && _jackpotResultButtonOriginalParent != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;
                t.SetParent(_jackpotResultButtonOriginalParent, false);
                t.localPosition = _jackpotResultButtonOriginalPos;
                t.localScale = _jackpotResultButtonOriginalScale;
            }

            gObject = _jackpotResultTipWindow.GetChild("jackpotResultScore")?.asCom;
            if (gObject?.displayObject?.gameObject != null && _jackpotResultScoreOriginalParent != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;
                t.SetParent(_jackpotResultScoreOriginalParent, false);
                t.localPosition = _jackpotResultScoreOriginalPos;
                t.localScale = _jackpotResultScoreOriginalScale;
            }

            _jackpotResultScoreOriginalParent = null;
            _jackpotResultButtonOriginalParent = null;
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam(null);
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