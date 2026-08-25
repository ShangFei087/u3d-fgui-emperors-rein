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
        private const float AutoStopDelay = 1.2f;
        private const float NextRollDelay = 1.5f;
        private const float CollectStartDelay = 0.4f;
        private const float CollectTrailDuration = 0.4f;
        private const float CollectAnimDuration = 1.0f;
        private const float CollectBetweenDelay = 0.15f;
        private const string PkgName = "MeiZhouHeiBao";
        private const string NgRoarPag = "ng_Roar/ng_Roar";

        private GComponent _root;
        private GComponent _pageRoot;
        private MonoHelper _monoHelper;
        private RewardRoll3993 _rewardRoll;
        private GTextField _txtRemainTime;
        private readonly UnityAction _onUpdate;
        private readonly List<Coroutine> _delayCos = new List<Coroutine>();
        private readonly List<RewardElement3993> _collectElements = new List<RewardElement3993>();
        private readonly List<int> _collectScores = new List<int>();

        private GComponent _effectFrame;
        private GComponent _templateTrails;
        private GameObject _glowPrefab;
        private GameObject _trailsPrefab;
        private PanelController3993 _panelController;
        private PagSlotBinding _pagRoar;

        private bool _isStartRoll;
        private bool _waitingAutoStop;
        private float _timer;
        private int _bonusTime = 3;
        private bool _updateRegistered;

        public bool IsGameOver { get; private set; }
        public bool IsRolling => _isStartRoll && !IsGameOver;
        public int BonusTime => _bonusTime;

        public RewardMgr3993()
        {
            _onUpdate = OnUpdate;
        }

        public void Init(GComponent rewardSlotMachine, GComponent pageRoot, MonoHelper monoHelper,
            FguiPoolHelper fguiPoolHelper, GComponent goExpectation)
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

        public void SetRoarPag(PagSlotBinding pagRoar)
        {
            _pagRoar = pagRoar;
        }

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
            RegisterUpdate(true);
        }

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

        public void StartStop(bool isAuto = true)
        {
            _waitingAutoStop = false;
            if (isAuto)
                _rewardRoll.StopRoll();
            else
                _rewardRoll.ManualStop();
        }

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

        public void UpdateBonusTime(int time)
        {
            _bonusTime = Mathf.Max(0, time);
            ContentModel.Instance.bonusSpinTime = _bonusTime;
            if (_txtRemainTime != null) _txtRemainTime.text = _bonusTime.ToString();
        }

        public void Delay(float seconds, Action action)
        {
            if (_monoHelper == null || action == null)
                return;
            Coroutine co = _monoHelper.StartCoroutine(DelayCo(seconds, action));
            _delayCos.Add(co);
        }

        public void Dispose()
        {
            StopAllDelays();
            RegisterUpdate(false);
            ClearBonusTrails();
            _rewardRoll?.Dispose();
            _rewardRoll = null;
            _pagRoar = null;
            _isStartRoll = false;
            _waitingAutoStop = false;
        }

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

        private void BindRemainTime()
        {
            _txtRemainTime = null;
            if (_pageRoot == null) return;

            GObject outFrameObj = _pageRoot.GetChild("OutFrame");
            GLoader loader = outFrameObj as GLoader;
            GComponent frame = loader != null ? loader.component : outFrameObj as GComponent;
            if (frame != null) _txtRemainTime = frame.GetChild("txtRemainTime") as GTextField;

        }

        private IEnumerator CollectBonusSymbols()
        {
            yield return PlayNgRoar();

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
                element?.PlayCollect();

                if (ContentModel.IsJackpotScore(score))
                {
                    int jpType = ContentModel.GetJackpotType(score);
                    int jpBet = ContentModel.Instance.GetJackpotBet(jpType);
                    yield return new WaitForSeconds(CollectAnimDuration);
                    yield return OpenJackpotWinPopup(jpType, jpBet);
                    OnTrailArrived(jpBet);
                    element?.SetCollected(score);
                    yield return new WaitForSeconds(CollectBetweenDelay);
                    continue;
                }

                GComponent trail = CreateTrail();
                if (trail == null)
                {
                    OnTrailArrived(score);
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
                int capturedScore = score;
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

            yield return new WaitForSeconds(CollectTrailDuration);
            FinishGame();
        }

        private IEnumerator PlayNgRoar()
        {
            if (_pagRoar == null)
                yield break;

            bool finished = false;
            _pagRoar.StopWithDefaults();
            bool started = _pagRoar.Play(new PagSequencePlay(
                new[] { new PagSegment(NgRoarPag, 1) },
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

        private static WaitForSeconds WaitCollectRemain(float elapsed)
        {
            float remain = CollectAnimDuration - elapsed;
            return remain > 0f ? new WaitForSeconds(remain) : null;
        }

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

        private GComponent CreateTrail()
        {
            GComponent trail = UIPackage.CreateObject(PkgName, "anchorTrails")?.asCom;
            if (trail == null && _templateTrails != null && !string.IsNullOrEmpty(_templateTrails.resourceURL))
                trail = UIPackage.CreateObjectFromURL(_templateTrails.resourceURL)?.asCom;
            return trail;
        }

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

        private void SetSpinButtonStop()
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
            _panelController?.ChangButtonNo(true);
            _panelController?.SetSpinButtonLocked(true);
        }

        private void FinishGame()
        {
            ClearBonusTrails();
            ContentModel.Instance.isSmallGameFinish = true;
            ContentModel.Instance.isSmallGameSpin = false;
            SetSpinButtonStop();
            DebugUtils.Log("[3993][RewardMgr] GameEnd");
        }

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

        private IEnumerator DelayCo(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

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
