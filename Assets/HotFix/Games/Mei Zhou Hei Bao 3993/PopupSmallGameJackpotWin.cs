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
        /// <summary>FairyGUI 包名。</summary>
        public new const string pkgName = "MeiZhouHeiBao";
        /// <summary>弹窗组件名。</summary>
        public new const string resName = "PopupSmallGameJackpotWin";

        /// <summary>三种 JP Spine 预制体所在目录。</summary>
        private const string PrefabDir = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupSmallGameJackpotWin/";
        /// <summary>Major 弹窗预制体。</summary>
        private const string PrefabMajor = PrefabDir + "Pup_MAJOR.prefab";
        /// <summary>Minor 弹窗预制体。</summary>
        private const string PrefabMinor = PrefabDir + "Pop_Minor.prefab";
        /// <summary>Mini 弹窗预制体。</summary>
        private const string PrefabMini = PrefabDir + "Pop_Mini.prefab";
        /// <summary>PAG 资源目录。</summary>
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";

        /// <summary>Major Spine 预制体。</summary>
        private GameObject _prefabMajor;
        /// <summary>Minor Spine 预制体。</summary>
        private GameObject _prefabMinor;
        /// <summary>Mini Spine 预制体。</summary>
        private GameObject _prefabMini;
        /// <summary>当前挂上的 Spine 实例。</summary>
        private GameObject _cloneJackpot;
        /// <summary>Spine 挂点。</summary>
        private GComponent _anchorJackpot;
        /// <summary>弹窗 Spine 播放器。</summary>
        private AnimPlayer _animJackpot;
        /// <summary>播完 Out 后延迟关页。</summary>
        private TimerCallback _delayCloseCallback;
        /// <summary>自动化测试自动点收集。</summary>
        private TimerCallback _autoClickCallback;
        /// <summary>延迟开始滚分。</summary>
        private TimerCallback _rollCallback;
        /// <summary>入场后延迟点亮收集按钮。</summary>
        private TimerCallback _enableBtnCallback;

        /// <summary>PAG 挂点。</summary>
        private GComponent _anchorPagJackpot;
        /// <summary>JP 弹窗 PAG 槽。</summary>
        private PagSlotBinding _pagJackpot;

        /// <summary>收集按钮。</summary>
        private GButton _btnCollect;
        /// <summary>赢分文本。</summary>
        private GTextField _txtWin;

        /// <summary>当前挂上的 JP 类型，避免同类型重复实例化。</summary>
        private string _boundType;
        /// <summary>当前实例绑定的语言，切语言时强制重绑。</summary>
        private I18nLang _boundLang;
        /// <summary>OpenPage 传入数据，优先于 jpGameRes。</summary>
        private EventData _openEventData;
        /// <summary>是否已点过关闭，防连点。</summary>
        private bool _isClicked;

        /// <summary>加载 MAJOR/MINOR/MINI 三个预制体，注册机台短按 Spin 关页。</summary>
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 3;
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

        /// <summary>按 JP 类型挂 Spine、滚分、延迟可点，自动化则定时点击。</summary>
        public override void InitParam()
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _rollCallback);
            RemoveTimer(ref _enableBtnCallback);

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

            _rollCallback = obj =>
            {
                if (_txtWin != null)
                    NumberAnimation.Instance.AnimateNumber(_txtWin, 0, winCredit, 3.0f, EaseType.Linear, () => { });
            };
            Timers.inst.Add(0.5f, 1, _rollCallback);
            _enableBtnCallback = obj =>
            {
                if (_btnCollect != null) _btnCollect.touchable = true;
            };
            Timers.inst.Add(3.5f, 1, _enableBtnCallback);

            AttachUi(jpType);
            ScheduleAutoModeClick(4.0f);
        }

        /// <summary>记录开页数据并刷新界面。</summary>
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _openEventData = eventData;
            InitParam();
        }

        /// <summary>关页：停滚分、清定时器、卸骨骼挂点、停 PAG。</summary>
        public override void OnClose(EventData eventData = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _rollCallback);
            RemoveTimer(ref _enableBtnCallback);
            _animJackpot?.DetachAll();
            _pagJackpot?.StopWithDefaults();
            _isClicked = false;
            base.OnClose(eventData);
        }

        /// <summary>点击收集：播 Out，约 1 秒后关页。</summary>
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

            if (_anchorJackpot == local && _boundType == jpType
                && _boundLang == PopupSpineLang3993.CurrentLang && _animJackpot != null)
                return;

            _animJackpot?.DetachAll();
            GameCommon.FguiUtils.DeleteWrapper(_anchorJackpot);
            _cloneJackpot = UnityEngine.Object.Instantiate(prefab);
            PopupSpineLang3993.Apply(_cloneJackpot);
            _anchorJackpot = local;
            _boundType = jpType;
            _boundLang = PopupSpineLang3993.CurrentLang;
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

            string spineGoName = _cloneJackpot.transform.GetChild(0).GetChild(0).name;
            string body = GetBodyBone(jpType);
            string root = $"Anchor/{spineGoName}/SkeletonUtility-SkeletonRoot/root/All/{body}";

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
                    localPos: new Vector3(-5.36f, 1.75f, 0.0f),
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

        /// <summary>PAG 离场：播一遍 out 后停止。</summary>
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

        /// <summary>按类型取对应 Spine 预制体，默认 Major。</summary>
        private GameObject GetPrefab(string jpType)
        {
            if (jpType == "minor") return _prefabMinor;
            if (jpType == "mini") return _prefabMini;
            return _prefabMajor;
        }

        /// <summary>身体骨骼名：Panther / crocodile / snake。</summary>
        private static string GetBodyBone(string jpType)
        {
            if (jpType == "minor") return "crocodile";
            if (jpType == "mini") return "snake";
            return "Panther";
        }

        /// <summary>分数框父骨骼：Panther2 / crocodile2 / snake2。</summary>
        private static string GetFrameParent(string jpType)
        {
            if (jpType == "minor") return "crocodile2";
            if (jpType == "mini") return "snake2";
            return "Panther2";
        }

        /// <summary>PAG 路径前缀（不含 _in/_idle/_out）。</summary>
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

        /// <summary>自动化测试开启时，延迟后自动点收集。</summary>
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

        /// <summary>移除 FairyGUI 定时器并置空引用。</summary>
        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}
