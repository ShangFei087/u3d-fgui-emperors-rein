using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>
    /// 大奖小游戏中奖弹窗。按 major / minor / mini 换 Spine 和 PAG。
    /// FGUI：anchorPopupJackPotPag、anchorPopupJackPot、btnCollect、txtWin。
    /// Spine 状态为 In / idle / Out（大小写敏感，不是小写 in/out）。
    /// </summary>
    public class PopupSmallGameJackpotWin : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupSmallGameJackpotWin";

        private const string PrefabDir = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupSmallGameJackpotWin/";
        private const string PrefabMajor = PrefabDir + "Pup_MAJOR.prefab";
        private const string PrefabMinor = PrefabDir + "Pop_Minor.prefab";
        private const string PrefabMini = PrefabDir + "Pop_Mini.prefab";
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";

        //弹窗 Spine
        private GameObject _prefabMajor;
        private GameObject _prefabMinor;
        private GameObject _prefabMini;
        private GameObject _cloneJackpot;
        private GComponent _anchorJackpot;
        private AnimPlayer _animJackpot;
        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;

        //pag
        private GComponent _anchorPagJackpot;
        private PagSlotBinding _pagJackpot;

        private GButton _btnCollect;
        private GTextField _txtWin;

        private string _boundType;          // 当前挂上的 JP 类型，避免同类型重复实例化
        private EventData _openEventData;   // OpenPage 传入，优先于 jpGameRes
        private bool _isClicked;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 3; // MAJOR / MINOR / MINI 三个预制体都到齐再 InitParam
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabMajor, clone =>
            {
                _prefabMajor = clone;
                callback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabMinor, clone =>
            {
                _prefabMinor = clone;
                callback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabMini, clone =>
            {
                _prefabMini = clone;
                callback();
            });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;

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
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);

            string jpType = ResolveJackpotType();
            float winCredit = ResolveWinCredit();

            _anchorPagJackpot = contentPane.GetChild("anchorPopupJackPotPag")?.asCom;
            if (_pagJackpot == null) _pagJackpot = new PagSlotBinding("3993pagJackpotWin", PagPath);
            if (_anchorPagJackpot != null)
                _pagJackpot.EnsureSlot(_anchorPagJackpot);
            PlayPagInIdle(jpType);

            BindSpine(jpType);
            _animJackpot?.PlayThen("In", "idle", true); // Controller 状态名是 In，不是 in

            _btnCollect = contentPane.GetChild("btnCollect")?.asButton;
            _txtWin = contentPane.GetChild("txtWin")?.asTextField;
            if (_txtWin != null)
                _txtWin.text = "0";

            if (_btnCollect != null)
            {
                _btnCollect.touchable = false;
                _btnCollect.onClick.Clear();
                _btnCollect.onClick.Add(() => OnCloseBtn());
            }

            // 进场后再滚分，滚完才允许点 Collect
            Timers.inst.Add(0.5f, 1, obj =>
            {
                if (_txtWin != null)
                    NumberAnimation.Instance.AnimateNumber(_txtWin, 0, winCredit, 3.0f, EaseType.Linear, () => { });
            });
            Timers.inst.Add(3.5f, 1, obj =>
            {
                if (_btnCollect != null) _btnCollect.touchable = true;
            });

            AttachUi(jpType);
            ScheduleAutoModeClick(4.0f);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _openEventData = eventData;
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            _animJackpot?.DetachAll();
            _pagJackpot?.StopWithDefaults();
            _isClicked = false;
            base.OnClose(eventData);
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            if (_btnCollect != null)
                _btnCollect.touchable = false;
            _animJackpot?.Play("Out"); // Controller 状态名是 Out，不是 out
            PlayPagOut(ResolveJackpotType());

            RemoveTimer(ref _delayCloseCallback);
            _delayCloseCallback = obj =>
            {
                if (isOpen) CloseSelf(eventData);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(1.0f, 1, _delayCloseCallback);
        }

        /// <summary>按类型把对应 Spine 预制体挂到 anchorPopupJackPot。</summary>
        private void BindSpine(string jpType)
        {
            GComponent local = contentPane.GetChild("anchorPopupJackPot")?.asCom;
            if (local == null)
                return;

            GameObject prefab = GetPrefab(jpType);
            if (prefab == null)
                return;

            if (_anchorJackpot == local && _boundType == jpType && _animJackpot != null)
                return;

            _animJackpot?.DetachAll();
            GameCommon.FguiUtils.DeleteWrapper(_anchorJackpot);
            _cloneJackpot = UnityEngine.Object.Instantiate(prefab);
            _anchorJackpot = local;
            _boundType = jpType;
            GameCommon.FguiUtils.AddWrapper(_anchorJackpot, _cloneJackpot);
            _animJackpot = new AnimPlayer(_cloneJackpot);
        }

        /// <summary>
        /// 按钮挂 button 骨，分数挂 frame 骨。
        /// MAJOR: Panther/button、Panther2/frame；MINOR: crocodile；MINI: snake。
        /// </summary>
        private void AttachUi(string jpType)
        {
            if (_animJackpot == null)
                return;

            string spine = GetSpineName(jpType);
            string body = GetBodyBone(jpType);
            string root = $"Anchor/Spine Mecanim GameObject ({spine})/SkeletonUtility-SkeletonRoot/root/All/{body}";

            if (_btnCollect != null)
            {
                _animJackpot.Attach(
                    _btnCollect,
                    root + "/button",
                    localPos: new Vector3(-2.3f, 0.75f, 0.0f),
                    localScale: new Vector3(0.01f, 0.01f, 0.01f),
                    localRot: Quaternion.identity);
            }

            if (_txtWin != null)
            {
                _animJackpot.Attach(
                    _txtWin,
                    root + $"/{GetFrameParent(jpType)}/frame",
                    localPos: new Vector3(-5.35f, 1.1f, 0.0f),
                    localScale: new Vector3(0.01f, 0.01f, 0.01f),
                    localRot: Quaternion.identity);
            }
        }

        /// <summary>PAG 路径不要带 .pag，例如 jp_pup/jp_pup_MAJOR_pag/jp_pup_MAJOR_in。</summary>
        private void PlayPagInIdle(string jpType)
        {
            if (_pagJackpot == null) return;
            string prefix = GetPagPrefix(jpType);
            _pagJackpot.StopWithDefaults();
            _pagJackpot.Play(new PagSequencePlay(
                PagPlaySpecs.IntroLoop(prefix + "_in", prefix + "_idle"),
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false));
        }

        private void PlayPagOut(string jpType)
        {
            if (_pagJackpot == null) return;
            string prefix = GetPagPrefix(jpType);
            _pagJackpot.StopWithDefaults();
            _pagJackpot.Play(new PagSequencePlay(
                new[] { new PagSegment(prefix + "_out", 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false,
                callbacks: new PagPlayCallbacks(
                    onFinished: () => _pagJackpot?.StopWithDefaults(),
                    stopAfterFinished: true)));
        }

        /// <summary>优先 OpenPage 的 Dictionary / EventData&lt;string&gt;，否则读 jpGameRes.jpWinLst[0].name，默认 major。</summary>
        private string ResolveJackpotType()
        {
            EventData<Dictionary<string, object>> dictData = _openEventData as EventData<Dictionary<string, object>>;
            if (dictData?.value != null && dictData.value.TryGetValue("jpType", out object typeObj) && typeObj != null)
                return NormalizeType(typeObj.ToString());

            EventData<string> typeData = _openEventData as EventData<string>;
            if (typeData != null && !string.IsNullOrEmpty(typeData.value))
                return NormalizeType(typeData.value);

            JackpotRes res = ContentModel.Instance.jpGameRes;
            if (res?.jpWinLst != null && res.jpWinLst.Count > 0 && !string.IsNullOrEmpty(res.jpWinLst[0].name))
                return NormalizeType(res.jpWinLst[0].name);

            return "major";
        }

        /// <summary>优先 Dictionary.winCredit / EventData&lt;float&gt;，否则读 jpWinLst[0].winCredit。</summary>
        private float ResolveWinCredit()
        {
            EventData<Dictionary<string, object>> dictData = _openEventData as EventData<Dictionary<string, object>>;
            if (dictData?.value != null && dictData.value.TryGetValue("winCredit", out object creditObj) && creditObj != null)
                return Convert.ToSingle(creditObj);

            EventData<float> floatData = _openEventData as EventData<float>;
            if (floatData != null)
                return floatData.value;

            JackpotRes res = ContentModel.Instance.jpGameRes;
            if (res?.jpWinLst != null && res.jpWinLst.Count > 0)
                return res.jpWinLst[0].winCredit;

            return 0f;
        }

        private GameObject GetPrefab(string jpType)
        {
            if (jpType == "minor") return _prefabMinor;
            if (jpType == "mini") return _prefabMini;
            return _prefabMajor;
        }

        private static string GetSpineName(string jpType)
        {
            if (jpType == "minor") return "jp_pup_MINOR";
            if (jpType == "mini") return "jp_pup_MINI";
            return "jp_pup_MAJOR";
        }

        private static string GetBodyBone(string jpType)
        {
            if (jpType == "minor") return "crocodile";
            if (jpType == "mini") return "snake";
            return "Panther";
        }

        private static string GetFrameParent(string jpType)
        {
            if (jpType == "minor") return "crocodile2";
            if (jpType == "mini") return "snake2";
            return "Panther2";
        }

        private static string GetPagPrefix(string jpType)
        {
            if (jpType == "minor") return "jp_pup/jp_pup_MINOR_pag/jp_pup_MINOR";
            if (jpType == "mini") return "jp_pup/jp_pup_MINI_pag/jp_pup_MINI";
            return "jp_pup/jp_pup_MAJOR_pag/jp_pup_MAJOR";
        }

        /// <summary>minor 必须先于 mini 判断，否则 "minor" 会被 Contains("mini") 误判。</summary>
        private static string NormalizeType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "major";
            string lower = name.ToLowerInvariant();
            if (lower.Contains("minor")) return "minor";
            if (lower.Contains("mini")) return "mini";
            return "major";
        }

        private void ScheduleAutoModeClick(float delaySeconds)
        {
            RemoveTimer(ref _autoClickCallback);
            if (!TestManager.Instance.IsAutoModeRunning) return;
            _autoClickCallback = obj =>
            {
                if (isOpen && !_isClicked)
                    OnCloseBtn();
                _autoClickCallback = null;
            };
            Timers.inst.Add(delaySeconds, 1, _autoClickCallback);
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}
