using FairyGUI;
using GameCommon;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MeiZhouHeiBao_3993
{
    /// <summary>
    /// 大奖小游戏流程：进场、自动停、次数、下一把或结束。
    /// </summary>
    public class RewardMgr3993
    {
        /// <summary>自动停轴前等待秒数。</summary>
        private const float AutoStopDelay = 1.2f;
        /// <summary>一把结束后到下一把开转的间隔。</summary>
        private const float NextRollDelay = 1.5f;
        /// <summary>GameEnd 后开始收集前的短延迟。</summary>
        private const float CollectStartDelay = 0.4f;
        /// <summary>收集拖尾飞行时长。</summary>
        private const float CollectTrailDuration = 0.4f;
        /// <summary>单格收集动画时长，用于拖尾后补齐等待。</summary>
        private const float CollectAnimDuration = 1.0f;
        /// <summary>相邻两格收集之间的间隔。</summary>
        private const float CollectBetweenDelay = 0.15f;
        /// <summary>FairyGUI 包名，用于创建拖尾节点。</summary>
        private const string PkgName = "MeiZhouHeiBao";
        /// <summary>收集开头咆哮 PAG。</summary>
        private const string NgRoarPag = "ng_Roar/ng_Roar";
        /// <summary>收集 bonus 时的右爪 PAG。</summary>
        private const string PagZhuaziYou = "eff_zhuazi_bmp/eff_zhuazi_you";
        /// <summary>收集彩金时的左爪 PAG。</summary>
        private const string PagZhuaziZuo = "eff_zhuazi_bmp/eff_zhuazi_zuo";
        /// <summary>大奖 NPC 循环待机。</summary>
        private const string NpcIdle1 = "Idle1";
        /// <summary>本把未出图标时的 NPC 反应。</summary>
        private const string NpcIdle2 = "Idle2";
        /// <summary>本把转出 bonus/彩金时的 NPC 反应。</summary>
        private const string NpcCol1 = "col1";
        /// <summary>收集普通 bonus 时的 NPC 抓取。</summary>
        private const string NpcCol2 = "col2";
        /// <summary>收集彩金时的 NPC 抓取。</summary>
        private const string NpcCol3 = "col3";
        /// <summary>全部收完后的结算动作。</summary>
        private const string NpcCol4 = "col4";

        /// <summary>大奖盘根节点 rewardSlotMachine。</summary>
        private GComponent _root;
        /// <summary>主页面根，用于取 OutFrame 剩余次数文本。</summary>
        private GComponent _pageRoot;
        /// <summary>协程与 Update 托管。</summary>
        private MonoHelper _monoHelper;
        /// <summary>15 轴滚动逻辑。</summary>
        private RewardRoll3993 _rewardRoll;
        /// <summary>剩余次数文本。</summary>
        private GTextField _txtRemainTime;
        /// <summary>挂到 MonoHelper.updateHandle 的滚动更新。</summary>
        private readonly UnityAction _onUpdate;
        /// <summary>Delay / 收集协程列表，Dispose 时统一停。</summary>
        private readonly List<Coroutine> _delayCos = new List<Coroutine>();
        /// <summary>本局待收集的锁定格。</summary>
        private readonly List<RewardElement3993> _collectElements = new List<RewardElement3993>();
        /// <summary>与 _collectElements 对应的分值（含彩金编码）。</summary>
        private readonly List<int> _collectScores = new List<int>();

        /// <summary>拖尾父节点。</summary>
        private GComponent _effectFrame;
        /// <summary>拖尾模板，清理时跳过。</summary>
        private GComponent _templateTrails;
        /// <summary>收集光效预制体。</summary>
        private GameObject _glowPrefab;
        /// <summary>收集拖尾预制体。</summary>
        private GameObject _trailsPrefab;
        /// <summary>底部 Panel，用于赢分框与按钮锁定。</summary>
        private PanelController3993 _panelController;
        /// <summary>全屏 PAG 槽：咆哮与爪子共用。</summary>
        private PagSlotBinding _pagRoar;
        /// <summary>大奖 NPC 播放器。</summary>
        private AnimPlayer _animNpc;

        /// <summary>当前是否正在滚动。</summary>
        private bool _isStartRoll;
        /// <summary>是否等待自动停轴计时。</summary>
        private bool _waitingAutoStop;
        /// <summary>开转后累计时间，超过 AutoStopDelay 自动停。</summary>
        private float _timer;
        /// <summary>剩余滚动次数，转出图标会重置为 3。</summary>
        private int _bonusTime = 3;
        /// <summary>Update 监听是否已注册。</summary>
        private bool _updateRegistered;

        /// <summary>大奖是否已进入结束收集。</summary>
        public bool IsGameOver { get; private set; }
        /// <summary>正在滚动且未结束。</summary>
        public bool IsRolling => _isStartRoll && !IsGameOver;
        /// <summary>当前剩余次数。</summary>
        public int BonusTime => _bonusTime;

        /// <summary>缓存 Update 委托。</summary>
        public RewardMgr3993()
        {
            _onUpdate = OnUpdate;
        }

        /// <summary>绑定大奖盘节点并创建/复用 RewardRoll。</summary>
        public void Init(GComponent rewardSlotMachine, GComponent pageRoot, MonoHelper monoHelper,FguiPoolHelper fguiPoolHelper, GComponent goExpectation)
        {
            _pageRoot = pageRoot;
            _monoHelper = monoHelper;
            if (_root != rewardSlotMachine || _rewardRoll == null)
            {
                _rewardRoll?.Dispose();
                _root = rewardSlotMachine;
                _rewardRoll = new RewardRoll3993(_root, this, fguiPoolHelper, goExpectation);
                _rewardRoll.Init();
            }

            _rewardRoll?.SetGlowPrefab(_glowPrefab);
        }

        /// <summary>注入收集拖尾、光效与 Panel。</summary>
        public void SetCollectContext(GComponent effectFrame, GComponent templateTrails, GameObject glowPrefab,
            GameObject trailsPrefab, PanelController3993 panelController)
        {
            _effectFrame = effectFrame;
            _templateTrails = templateTrails;
            _glowPrefab = glowPrefab;
            _trailsPrefab = trailsPrefab;
            _panelController = panelController;
            _rewardRoll?.SetGlowPrefab(_glowPrefab);
        }

        /// <summary>注入全屏 PAG（咆哮/爪子）。</summary>
        public void SetRoarPag(PagSlotBinding pagRoar)
        {
            _pagRoar = pagRoar;
        }

        /// <summary>注入大奖 NPC；切回普通局时传 null。</summary>
        public void SetSmallGameNpc(AnimPlayer animNpc)
        {
            _animNpc = animNpc;
        }

        /// <summary>滚动一轮：有新图标播 col1，否则播 Idle2。Controller 会回到 Idle1。</summary>
        public void PlayNpcAfterRoll(bool landedAny)
        {
            PlayNpc(landedAny ? NpcCol1 : NpcIdle2);
        }

        /// <summary>进大奖：铺盘、重置次数、NPC Idle1、开始 Update。</summary>
        public void Enter(IList<int> matrix, IList<int> bonusData)
        {
            if (_rewardRoll == null)
            {
                DebugUtils.LogError("[3993][RewardMgr] RewardRoll 未初始化");
                ContentModel.Instance.isSmallGameFinish = true;
                return;
            }

            IsGameOver = false;
            _isStartRoll = false;
            _waitingAutoStop = false;
            _timer = 0f;
            BindRemainTime();
            UpdateBonusTime(3);
            _rewardRoll.InitRoll(matrix, bonusData);
            PlayNpc(NpcIdle1, true);
            RegisterUpdate(true);
        }

        /// <summary>开转：扣一次剩余次数并滚动未锁定轴。</summary>
        public void StartRoll()
        {
            if (IsGameOver) return;
            if (_isStartRoll) return;

            StopAllDelays();
            _isStartRoll = true;
            _timer = 0f;
            _waitingAutoStop = true;
            UpdateBonusTime(_bonusTime - 1);
            _rewardRoll.StartRoll();

            ContentModel.Instance.isSpin = true;
            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
            _panelController?.ChangButtonNo(true);
            _panelController?.SetSpinButtonLocked(false);
        }

        /// <summary>本把结束后延迟自动开下一把。</summary>
        public void Ready2Start()
        {
            if (IsGameOver)
                return;

            _isStartRoll = false;
            _waitingAutoStop = false;
            SetSpinButtonStop();
            Delay(NextRollDelay, () =>
            {
                if (!IsGameOver && !_isStartRoll)
                    StartRoll();
            });
        }

        /// <summary>停轴：自动逐轴或手动全停。</summary>
        public void StartStop(bool isAuto = true)
        {
            _waitingAutoStop = false;
            if (isAuto)
                _rewardRoll.StopRoll();
            else
                _rewardRoll.ManualStop();
        }

        /// <summary>次数用尽且图标出齐：进入收集流程。</summary>
        public void GameEnd()
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            _isStartRoll = false;
            _waitingAutoStop = false;
            StopAllDelays();
            RegisterUpdate(false);
            SetSpinButtonStop();

            if (_monoHelper == null)
            {
                FinishGame();
                return;
            }

            Delay(CollectStartDelay, () =>
            {
                Coroutine co = _monoHelper.StartCoroutine(CollectBonusSymbols());
                if (co != null)
                    _delayCos.Add(co);
            });
        }

        /// <summary>刷新剩余次数文本。</summary>
        public void UpdateBonusTime(int time)
        {
            _bonusTime = Mathf.Max(0, time);
            ContentModel.Instance.bonusSpinTime = _bonusTime;
            if (_txtRemainTime != null) _txtRemainTime.text = _bonusTime.ToString();
        }

        /// <summary>延迟执行并记入协程列表。</summary>
        public void Delay(float seconds, Action action)
        {
            if (_monoHelper == null || action == null)
                return;
            Coroutine co = _monoHelper.StartCoroutine(DelayCo(seconds, action));
            _delayCos.Add(co);
        }

        /// <summary>停协程、卸 Update、清拖尾与 NPC/PAG 引用。</summary>
        public void Dispose()
        {
            StopAllDelays();
            RegisterUpdate(false);
            ClearBonusTrails();
            _rewardRoll?.Dispose();
            _rewardRoll = null;
            _pagRoar = null;
            _animNpc = null;
            _isStartRoll = false;
            _waitingAutoStop = false;
        }

        /// <summary>滚动中驱动各轴，到时自动停。</summary>
        private void OnUpdate()
        {
            if (!_isStartRoll || IsGameOver)
                return;

            float dt = Time.deltaTime;
            _rewardRoll.Update(dt);
            if (!_waitingAutoStop)
                return;

            _timer += dt;
            if (_timer > AutoStopDelay)
                StartStop();
        }

        /// <summary>从 OutFrame 绑定剩余次数文本。</summary>
        private void BindRemainTime()
        {
            _txtRemainTime = null;
            if (_pageRoot == null) return;

            GObject outFrameObj = _pageRoot.GetChild("OutFrame");
            GLoader loader = outFrameObj as GLoader;
            GComponent frame = loader != null ? loader.component : outFrameObj as GComponent;
            if (frame != null) _txtRemainTime = frame.GetChild("txtRemainTime") as GTextField;

        }

        /// <summary>收集：等 NPC 反应 → 咆哮 → 逐格爪子+拖尾/弹窗 → col4 → 结束。</summary>
        private IEnumerator CollectBonusSymbols()
        {
            yield return WaitCurrentNpcAnim();
            yield return PlayPag(NgRoarPag);

            _collectElements.Clear();
            _collectScores.Clear();
            _rewardRoll?.CollectLockedBonuses(_collectElements, _collectScores);

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                new EventData<long>(SlotMachineEvent.TotalWinCredit, 0L));
            _panelController?.HideWinBorders();

            if (_collectElements.Count == 0 || _effectFrame == null || _trailsPrefab == null)
            {
                FinishGame();
                yield break;
            }

            Vector2 to = GetWinBorderLocalPos();
            for (int i = 0; i < _collectElements.Count; i++)
            {
                RewardElement3993 element = _collectElements[i];
                int score = i < _collectScores.Count ? _collectScores[i] : 0;
                bool isJackpot = ContentModel.IsJackpotScore(score);
                element?.PlayCollect();
                yield return PlayCollectNpcAndZhuazi(isJackpot);

                if (isJackpot)
                {
                    int jpType = ContentModel.GetJackpotType(score);
                    int jpBet = ContentModel.Instance.GetJackpotBet(jpType);
                    yield return OpenJackpotWinPopup(jpType, jpBet);
                    OnTrailArrived(jpBet);
                    element?.SetCollected(score);
                    yield return new WaitForSeconds(CollectBetweenDelay);
                    continue;
                }

                int displayScore = ContentModel.GetDisplayScore(score);
                GComponent trail = CreateTrail();
                if (trail == null)
                {
                    OnTrailArrived(displayScore);
                    yield return WaitCollectRemain(0f);
                    element?.SetCollected(score);
                    yield return new WaitForSeconds(CollectBetweenDelay);
                    continue;
                }

                _effectFrame.AddChild(trail);
                trail.SetPivot(0.5f, 0.5f, true);
                Vector2 from = _effectFrame.GlobalToLocal(element != null ? element.GetCenterGlobal() : Vector2.zero);
                trail.xy = from;
                GameCommon.FguiUtils.AddWrapper(trail, Object.Instantiate(_trailsPrefab));

                bool arrived = false;
                GComponent captured = trail;
                int capturedScore = displayScore;
                trail.TweenMove(to, CollectTrailDuration).OnComplete(() =>
                {
                    GameCommon.FguiUtils.DeleteWrapper(captured);
                    captured.Dispose();
                    OnTrailArrived(capturedScore);
                    arrived = true;
                });

                yield return new WaitUntil(() => arrived);
                yield return WaitCollectRemain(CollectTrailDuration);
                element?.SetCollected(score);
                yield return new WaitForSeconds(CollectBetweenDelay);
            }

            PlayNpc(NpcCol4);
            yield return WaitCurrentNpcAnim();
            yield return new WaitForSeconds(CollectTrailDuration);
            FinishGame();
        }

        /// <summary>收集一格：bonus 播 col2，彩金播 col3，再播对应爪子 PAG。</summary>
        private IEnumerator PlayCollectNpcAndZhuazi(bool isJackpot)
        {
            PlayNpc(isJackpot ? NpcCol3 : NpcCol2);
            yield return new WaitForSeconds(0.5f);
            yield return PlayPag(isJackpot ? PagZhuaziYou : PagZhuaziZuo);
        }

        /// <summary>按状态名播大奖 NPC；循环由 Controller 决定。</summary>
        private void PlayNpc(string animName, bool loop = false)
        {
            if (_animNpc == null || string.IsNullOrEmpty(animName))
                return;
            _animNpc.Play(animName, loop);
        }

        /// <summary>等到当前非 Idle1 动画播完；Idle1 循环则立即返回。</summary>
        private IEnumerator WaitCurrentNpcAnim()
        {
            Animator animator = _animNpc?.Animator;
            if (animator == null)
                yield break;

            animator.Update(0f);
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(NpcIdle1))
                yield break;

            float remain = info.length * (1f - Mathf.Clamp01(info.normalizedTime));
            if (animator.speed > 0.0001f)
                remain /= animator.speed;
            if (remain > 0f)
                yield return new WaitForSeconds(remain);
        }

        /// <summary>在全屏 PAG 槽播一段并等待结束。</summary>
        private IEnumerator PlayPag(string pagName)
        {
            if (_pagRoar == null || string.IsNullOrEmpty(pagName))
                yield break;

            bool finished = false;
            _pagRoar.StopWithDefaults();
            bool started = _pagRoar.Play(new PagSequencePlay(
                new[] { new PagSegment(pagName, 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false,
                callbacks: new PagPlayCallbacks(
                    onFinished: () =>
                    {
                        _pagRoar?.StopWithDefaults();
                        finished = true;
                    },
                    onFailed: () => finished = true,
                    stopAfterFinished: true)));

            if (!started)
                yield break;

            yield return new WaitUntil(() => finished);
        }

        /// <summary>打开 Major/Minor/Mini 中奖弹窗，关闭后继续收集。</summary>
        private IEnumerator OpenJackpotWinPopup(int jpType, int jpBet)
        {
            bool closed = false;
            string typeName = ContentModel.GetJackpotTypeName(jpType);
            PageManager.Instance.OpenPageAsync(
                PageName.MeiZhouHeiBaoPopupSmallGameJackpotWin,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                {
                    ["jpType"] = typeName,
                    ["winCredit"] = (float)jpBet,
                }),
                ed => closed = true);
            yield return new WaitUntil(() => closed);
        }

        /// <summary>补齐收集动画剩余等待；已超时长则不等。</summary>
        private static WaitForSeconds WaitCollectRemain(float elapsed)
        {
            float remain = CollectAnimDuration - elapsed;
            return remain > 0f ? new WaitForSeconds(remain) : null;
        }

        /// <summary>拖尾到达赢分框：亮大奖框并加分。</summary>
        private void OnTrailArrived(int score)
        {
            _panelController?.ShowBigWinBorder();
            if (score > 0)
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                    new EventData<long>(SlotMachineEvent.SingleWinBonus, score));
            }

            //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusSymbolCollect);
        }

        /// <summary>Panel 赢分框中心在拖尾父节点下的本地坐标。</summary>
        private Vector2 GetWinBorderLocalPos()
        {
            if (_effectFrame == null)
                return Vector2.zero;

            GComponent target = _panelController?.AnchorWinBorder;
            if (target == null)
                return Vector2.zero;

            Vector2 global = target.LocalToGlobal(new Vector2(target.width * 0.5f, target.height * 0.5f));
            return _effectFrame.GlobalToLocal(global);
        }

        /// <summary>从包内或模板复制一条拖尾节点。</summary>
        private GComponent CreateTrail()
        {
            GComponent trail = UIPackage.CreateObject(PkgName, "anchorTrails")?.asCom;
            if (trail == null && _templateTrails != null && !string.IsNullOrEmpty(_templateTrails.resourceURL))
                trail = UIPackage.CreateObjectFromURL(_templateTrails.resourceURL)?.asCom;
            return trail;
        }

        /// <summary>清除拖尾父节点下除模板外的子节点。</summary>
        private void ClearBonusTrails()
        {
            if (_effectFrame == null)
                return;

            for (int i = _effectFrame.numChildren - 1; i >= 0; i--)
            {
                GObject child = _effectFrame.GetChildAt(i);
                if (_templateTrails != null && child == _templateTrails)
                    continue;

                GComponent com = child as GComponent;
                if (com != null)
                    GameCommon.FguiUtils.DeleteWrapper(com);
                child.Dispose();
            }
        }

        /// <summary>收集/结束时锁定 Spin 按钮。</summary>
        private void SetSpinButtonStop()
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
            _panelController?.ChangButtonNo(true);
            _panelController?.SetSpinButtonLocked(true);
        }

        /// <summary>清拖尾并置 isSmallGameFinish，主流程可切结算弹窗。</summary>
        private void FinishGame()
        {
            ClearBonusTrails();
            ContentModel.Instance.isSmallGameFinish = true;
            ContentModel.Instance.isSmallGameSpin = false;
            SetSpinButtonStop();
            DebugUtils.Log("[3993][RewardMgr] GameEnd");
        }

        /// <summary>注册或注销滚动 Update。</summary>
        private void RegisterUpdate(bool on)
        {
            if (_monoHelper == null)
                return;

            if (on && !_updateRegistered)
            {
                _monoHelper.updateHandle.AddListener(_onUpdate);
                _updateRegistered = true;
            }
            else if (!on && _updateRegistered)
            {
                _monoHelper.updateHandle.RemoveListener(_onUpdate);
                _updateRegistered = false;
            }
        }

        /// <summary>等待指定秒后执行。</summary>
        private IEnumerator DelayCo(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

        /// <summary>停止所有 Delay/收集协程。</summary>
        private void StopAllDelays()
        {
            if (_monoHelper == null)
                return;
            for (int i = 0; i < _delayCos.Count; i++)
            {
                if (_delayCos[i] != null)
                    _monoHelper.StopCoroutine(_delayCos[i]);
            }

            _delayCos.Clear();
        }
    }
}
