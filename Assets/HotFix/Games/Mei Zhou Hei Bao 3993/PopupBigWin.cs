using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>普通局 Big / Huge / Massive 赢分弹窗，播 Spine 并滚动数字后自动关闭。</summary>
    public class PopupBigWin : MachinePageBase
    {
        /// <summary>FairyGUI 包名。</summary>
        public new const string pkgName = "MeiZhouHeiBao";
        /// <summary>弹窗组件名。</summary>
        public new const string resName = "PopupBigWin";

        /// <summary>弹窗 Spine 预制体路径。</summary>
        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupBigWin/PopupBigWin.prefab";
        /// <summary>弹窗特效 Spine 预制体路径。</summary>
        private const string EffPrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Eff_pop.prefab";
        /// <summary>控制器三档入口：BIG / HUGE / MASSIVE，后续 in→idle→out 由 Animator 跳转。</summary>
        private static readonly string[] EntryAnims = { "big_in", "big_in 0", "big_in 1" };
        /// <summary>特效三档：随主 Spine 升档切换。</summary>
        private static readonly string[] EffAnims = { "big_idle", "super_idle", "mega_idle" };

        // /// <summary>PAG 资源目录。</summary>
        // private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        // /// <summary>Big / Super / Mega 三档 PAG 路径。</summary>
        // private string[] PagPathBigWin = {
        //     "ng_pop_bigwin/BigWin_bmp",
        //     "ng_pop_bigwin/SuperWin_bmp",
        //     "ng_pop_bigwin/MegaWin_bmp" };
        /// <summary>对应三档弹窗自动关闭时长（秒）。</summary>
        private float [] CloseBigWinTime = { 2.5f, 4.8f, 7.2f, };
        /// <summary>对应三档数字滚动时长（秒）。</summary>
        private float[] CloseScoreNumTime = { 2.0f, 2.5f, 7.0f, };
        /// <summary>与 WinLevelType 对应的档位名。</summary>
        private readonly string[] winString = { "BIG", "HUGE", "MASSIVE" };

        /// <summary>本局展示赢分。</summary>
        private long _score;
        /// <summary>开页传入的赢分档位字符串。</summary>
        private string _winType;
        /// <summary>0=Big，1=Huge，2=Massive。</summary>
        private int _winIndex;

        /// <summary>赢分文本。</summary>
        private GTextField textBigWin;
        /// <summary>加载后的 Spine 预制体。</summary>
        private GameObject goBigWin;
        /// <summary>Spine 挂点。</summary>
        private GComponent anchorBigWin;
        /// <summary>场景中的 Spine 实例。</summary>
        private GameObject clonegoBigWin;
        /// <summary>弹窗 Spine 播放器。</summary>
        private AnimPlayer _animBigWin;
        /// <summary>加载后的特效 Spine 预制体。</summary>
        private GameObject goBigWinEff;
        /// <summary>特效 Spine 挂点。</summary>
        private GComponent anchorBigWinEff;
        /// <summary>场景中的特效 Spine 实例。</summary>
        private GameObject clonegoBigWinEff;
        /// <summary>弹窗特效 Spine 播放器。</summary>
        private AnimPlayer _animBigWinEff;
        /// <summary>当前正在播的特效动画，避免升档检测重复 Play。</summary>
        private string _playingEffAnim;
        // /// <summary>PAG 挂点。</summary>
        // private GComponent comBigWin;
        // /// <summary>大奖 PAG 播放槽。</summary>
        // private PagSlotBinding pagBigWin;
        /// <summary>延迟启动数字滚动的定时器。</summary>
        private TimerCallback _rollCallback;
        /// <summary>到时关页的定时器。</summary>
        private TimerCallback _exitCallback;
        private TimerCallback _closeNumCallback;
        /// <summary>跟随主 Spine 升档切换特效。</summary>
        private TimerCallback _syncEffCallback;
        /// <summary>创建弹窗根节点并加载 Spine 预制体。</summary>
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 2;
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                PrefabPath,
                (GameObject clone) =>
                {
                    goBigWin = clone;
                    callback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffPrefabPath,
                (GameObject clone) =>
                {
                    goBigWinEff = clone;
                    callback();
                });
        }

        /// <summary>Dispose contentPane 前先摘 wrapTarget，避免 GoWrapper 把 Spine 实例一起毁掉。</summary>
        protected override void OnBeforetLanguageChange(I18nLang lang)
        {
            _animBigWin?.DetachAll();
            _animBigWinEff?.DetachAll();
            DetachSpineFromAnchor(anchorBigWin);
            DetachSpineFromAnchor(anchorBigWinEff);
            HideUnwrappedClone(clonegoBigWin);
            HideUnwrappedClone(clonegoBigWinEff);
            anchorBigWin = null;
            anchorBigWinEff = null;
        }

        /// <summary>绑定赢分文本与 Spine；打开时按档位播入口动画并定时滚分、关页。</summary>
        public override void InitParam()
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();

            textBigWin = contentPane.GetChild("txtWin").asTextField;
            textBigWin.text = string.Empty;
            // comBigWin = contentPane.GetChild("anchorPagBigWin").asCom;
            // if (pagBigWin == null)
            //     pagBigWin = new PagSlotBinding("bigWin", PagPath);
            // pagBigWin.EnsureSlot(comBigWin);

            GComponent localBigWin = contentPane.GetChild("anchorBigWin") as GComponent;
            if (localBigWin == null)
                localBigWin = contentPane.GetChild("anchorPagBigWin") as GComponent;
            EnsureSpine(ref anchorBigWin, localBigWin, goBigWin, ref clonegoBigWin, ref _animBigWin);

            GComponent localEff = contentPane.GetChild("anchorBigWinEff") as GComponent;
            EnsureSpine(ref anchorBigWinEff, localEff, goBigWinEff, ref clonegoBigWinEff, ref _animBigWinEff);

            SetAnchorSpineVisible(anchorBigWin, false);
            SetAnchorSpineVisible(anchorBigWinEff, false);
            if (!isOpen) return;

            ClearAllTimers();
            // pagBigWin.Play(new PagSequencePlay(
            //     new[] { new PagSegment(PagPathBigWin[_winIndex], 1) },
            //     PagPlayLayout.Center,
            //     PagPresentationDefaults.DisplayScale,
            //     useGpuSyncGroup: false));

            SetAnchorSpineVisible(anchorBigWin, true);
            SetAnchorSpineVisible(anchorBigWinEff, true);
            _animBigWin?.Play(EntryAnims[_winIndex]);
            PlayEffOnOpen();

            _rollCallback = obj =>
            {
                NumberAnimation.Instance.AnimateNumber(textBigWin, 0, _score, CloseBigWinTime[_winIndex]-1.0f, EaseType.Linear, () => { });
            };
            Timers.inst.Add(0, 1, _rollCallback);

            _closeNumCallback = obj =>
            {
                textBigWin.text=string.Empty;
            };
            Timers.inst.Add(CloseScoreNumTime[_winIndex], 1, _closeNumCallback);

            _exitCallback = exit;
            Timers.inst.Add(CloseBigWinTime[_winIndex], 1, _exitCallback);
        }

        /// <summary>解析赢分与档位后刷新界面。</summary>
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            // 解析数据
            if (eventData?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    _score = longScore;

                _winType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }
            _winIndex = Array.IndexOf(winString, _winType);
            if (_winIndex < 0) _winIndex = 0;
            if (_winIndex > 2) _winIndex = 2;
            InitParam();
        }

        /// <summary>关页：停滚分、清定时器，只隐藏 Spine，不销毁实例。</summary>
        public override void OnClose(EventData eventData = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            ClearAllTimers();
            // ClearPag();
            SetAnchorSpineVisible(anchorBigWin, false);
            SetAnchorSpineVisible(anchorBigWinEff, false);
            base.OnClose(eventData);
        }
        /// <summary>定时器到期：停动画并关闭自身。</summary>
        public void exit(object obj = null)
        {
            NumberAnimation.Instance.StopAllAnimations();
            textBigWin.text = string.Empty;
            // ClearPag();
            ClearAllTimers();
            CloseSelf(null);
        }

        /// <summary>只创建一次 Spine；挂点变了只换 wrapper，不 Instantiate。</summary>
        private static void EnsureSpine(ref GComponent bound, GComponent local, GameObject prefab,
            ref GameObject clone, ref AnimPlayer anim)
        {
            if (local == null || prefab == null) return;
            if (clone == null)
                clone = GameObject.Instantiate(prefab);
            if (bound != local)
            {
                GameCommon.FguiUtils.AddWrapper(local, clone);
                bound = local;
            }
            if (anim == null)
                anim = new AnimPlayer(clone);
        }

        /// <summary>开场播特效，HUGE/MASSIVE 再跟主 Spine 升档。</summary>
        private void PlayEffOnOpen()
        {
            _playingEffAnim = null;
            PlayEffForLevel(0);
            if (_winIndex <= 0) return;

            _syncEffCallback = obj => PlayEffForLevel(GetEffLevelFromMainSpine());
            Timers.inst.Add(0.05f, 0, _syncEffCallback);
        }

        /// <summary>按主 Spine 当前状态得到特效档位：0=Big，1=Super，2=Mega。</summary>
        private int GetEffLevelFromMainSpine()
        {
            Animator animator = _animBigWin?.Animator;
            if (animator == null) return 0;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("super to mega") || info.IsName("mega_idle") || info.IsName("mega_out"))
                return 2;
            if (info.IsName("big to super") || info.IsName("big to super 0")
                || info.IsName("super_idle") || info.IsName("super_idle 0") || info.IsName("super_out"))
                return 1;
            return 0;
        }

        /// <summary>特效切到指定档位；同档不重播。</summary>
        private void PlayEffForLevel(int level)
        {
            if (level < 0) level = 0;
            if (level > 2) level = 2;
            string anim = EffAnims[level];
            if (_animBigWinEff == null || _playingEffAnim == anim) return;
            _playingEffAnim = anim;
            _animBigWinEff.Play(anim);
        }

        /// <summary>关 GoWrapper 根节点并关 MeshRenderer；只 SetActive clone 关不掉缓存网格。</summary>
        private static void SetAnchorSpineVisible(GComponent anchor, bool visible)
        {
            if (anchor == null) return;
            anchor.visible = visible;
            GGraph holder = anchor.GetChild("holder") as GGraph;
            if (holder != null)
                holder.visible = visible;

            GameObject target = GameCommon.FguiUtils.GetWrapperTarget(anchor);
            if (target == null) return;
            target.SetActive(visible);
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }
        }

        /// <summary>从 GoWrapper 摘下实例但不销毁，供 contentPane.Dispose 前调用。</summary>
        private static void DetachSpineFromAnchor(GComponent anchor)
        {
            GoWrapper wrapper = GameCommon.FguiUtils.GetWrapper(anchor);
            wrapper?.SetWrapTarget(null, false);
        }

        /// <summary>摘下后的 clone 会回到场景根，先关掉网格避免闪一帧。</summary>
        private static void HideUnwrappedClone(GameObject clone)
        {
            if (clone == null) return;
            clone.SetActive(false);
            Renderer[] renderers = clone.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = false;
            }
        }

        // /// <summary>停止大奖 PAG。</summary>
        // private void ClearPag()
        // {
        //     pagBigWin?.StopWithDefaults();
        // }

        /// <summary>移除滚分与关页定时器。</summary>
        private void ClearAllTimers()
        {
            RemoveTimer(ref _rollCallback);
            RemoveTimer(ref _exitCallback);
            RemoveTimer(ref _closeNumCallback);
            RemoveTimer(ref _syncEffCallback);
        }

        /// <summary>若定时器仍在队列中则移除并置空。</summary>
        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            if (Timers.inst.Exists(timerCallback))
                Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}
