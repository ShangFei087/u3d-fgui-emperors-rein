using FairyGUI;
using GameMaker;
using SlotMaker;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>
    /// 大奖小游戏 15 轴。按 BonusData[i] 定点落分，不随机重铺。
    /// </summary>
    public class RewardRoll3993
    {
        public const int ReelCount = 15;
        public const int ElementPerReel = 4;

        private readonly GComponent _root;
        private readonly RewardMgr3993 _rewardMgr;
        private readonly FguiPoolHelper _fguiPoolHelper;
        private readonly GComponent _goExpectation;

        internal FguiPoolHelper FguiPoolHelper => _fguiPoolHelper;
        internal GComponent GoExpectation => _goExpectation;
        internal GameObject GlowPrefab { get; private set; }
        private readonly List<RewardElement3993>[] _elements = new List<RewardElement3993>[ReelCount];
        private readonly GComponent[] _reelBoxes = new GComponent[ReelCount];

        private readonly bool[] _elementBoxRoll = new bool[ReelCount];
        private readonly bool[] _elementBoxBonus = new bool[ReelCount];
        private readonly int[] _elementBoxBonusData = new int[ReelCount];
        private readonly int[] _bonusScores = new int[ReelCount];
        private readonly int[] _startBackRollCount = new int[ReelCount];
        private readonly int[] _finishBackRollCount = new int[ReelCount];

        private readonly List<int> _singleBonus = new List<int>();
        private int _rollEndCount;
        private bool _isAuto = true;
        private int _bonusRoundSpinIndex;

        public RewardRoll3993(GComponent root, RewardMgr3993 rewardMgr, FguiPoolHelper fguiPoolHelper, GComponent goExpectation)
        {
            _root = root;
            _rewardMgr = rewardMgr;
            _fguiPoolHelper = fguiPoolHelper;
            _goExpectation = goExpectation;
            for (int n = 0; n < ReelCount; n++)
                _elements[n] = new List<RewardElement3993>(ElementPerReel);

            GComponent reels = root.GetChild("reels")?.asCom;
            if (reels == null)
            {
                DebugUtils.LogError("[3993][RewardRoll] 找不到 reels 节点");
                return;
            }

            for (int i = 0; i < ReelCount; i++)
            {
                GComponent reel = reels.GetChild("reel" + (i + 1))?.asCom;
                if (reel == null)
                {
                    DebugUtils.LogError($"[3993][RewardRoll] 找不到 reel{i + 1}");
                    continue;
                }

                GComponent symbols = reel.GetChild("symbols")?.asCom;
                if (symbols == null)
                {
                    DebugUtils.LogError($"[3993][RewardRoll] reel{i + 1} 找不到 symbols");
                    continue;
                }

                _reelBoxes[i] = reel;
                _elements[i].Clear();
                for (int k = 0; k < ElementPerReel; k++)
                {
                    GComponent el = symbols.GetChild("rollElement" + (k + 1))?.asCom;
                    if (el == null)
                    {
                        DebugUtils.LogError($"[3993][RewardRoll] reel{i + 1} 找不到 rollElement{k + 1}");
                        continue;
                    }

                    _elements[i].Add(new RewardElement3993(el, this));
                }
            }
        }

        public void Init()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                if (_elements[i] == null)
                    continue;
                for (int j = 0; j < _elements[i].Count; j++)
                    _elements[i][j].Init(i, j);
            }
        }

        public void ResetBoard()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                _elementBoxBonus[i] = false;
                _elementBoxBonusData[i] = 0;
                _bonusScores[i] = 0;
                _elementBoxRoll[i] = false;
                _startBackRollCount[i] = 0;
                _finishBackRollCount[i] = 0;
                if (_elements[i] == null)
                    continue;
                for (int j = 0; j < _elements[i].Count; j++)
                    _elements[i][j].Init(i, j);
            }

            _singleBonus.Clear();
            _bonusRoundSpinIndex = 0;
            _rollEndCount = 0;
            _isAuto = true;
        }

        public void SetGlowPrefab(GameObject prefab)
        {
            GlowPrefab = prefab;
        }

        public void CollectLockedBonuses(List<RewardElement3993> elements, List<int> scores)
        {
            elements?.Clear();
            scores?.Clear();
            if (elements == null || scores == null)
                return;

            for (int i = 0; i < ReelCount; i++)
            {
                if (!_elementBoxBonus[i] || _bonusScores[i] <= 0)
                    continue;
                if (_elements[i] == null || _elements[i].Count == 0)
                    continue;

                elements.Add(_elements[i][0]);
                scores.Add(_bonusScores[i]);
            }
        }

        public void InitRoll(IList<int> matrix, IList<int> bonusData)
        {
            int bonusId = CustomModel.Instance.symbolNumber[12];
            int sum = 0;
            for (int i = 0; i < ReelCount; i++)
            {
                int score = 0;
                if (bonusData != null && i < bonusData.Count)
                    score = bonusData[i];

                _bonusScores[i] = score;
                _elementBoxBonusData[i] = 0;
                _elementBoxBonus[i] = false;
                _elementBoxRoll[i] = false;
                if (_reelBoxes[i] != null)
                    _reelBoxes[i].visible = true;
                if (!ContentModel.IsJackpotScore(score))
                    sum += score;

                bool isTriggerBonus = matrix != null && i < matrix.Count && matrix[i] == bonusId && score > 0;
                for (int j = 0; j < _elements[i].Count; j++)
                {
                    _elements[i][j].Init(i, j);
                    if (isTriggerBonus && j == 0)
                        _elements[i][j].SetBonus(score);
                    else
                        _elements[i][j].SetBlank();
                }

                if (isTriggerBonus)
                {
                    _elementBoxBonus[i] = true;
                    _elementBoxBonusData[i] = score;
                }
            }

            if (ContentModel.Instance.BonusBet > 0 && sum != ContentModel.Instance.BonusBet)
                DebugUtils.LogWarning($"[3993][RewardRoll] BonusBet 校验不一致，sum={sum} BonusBet={ContentModel.Instance.BonusBet}");
            else
                DebugUtils.Log($"[3993][RewardRoll] InitRoll sum={sum} BonusBet={ContentModel.Instance.BonusBet}");

            _bonusRoundSpinIndex = 0;
            CalculationTimesAndCount();
        }

        public void Update(float dt)
        {
            for (int i = 0; i < ReelCount; i++)
            {
                for (int j = 0; j < _elements[i].Count; j++)
                    _elements[i][j].Update(dt);
            }
        }

        public void StartRoll()
        {
            InitData();
            //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusRolling);

            bool anyRolling = false;
            for (int i = 0; i < ReelCount; i++)
            {
                if (_elementBoxBonus[i])
                    continue;

                anyRolling = true;
                _elementBoxRoll[i] = true;
                for (int j = 0; j < _elements[i].Count; j++) _elements[i][j].StartRoll();
            }

            if (!anyRolling)
                RollEnd();
        }

        public void StopRoll()
        {
            if (!_isAuto)
                return;

            int index = GetCanStopIndex();
            if (index >= ReelCount)
                return;

            _rewardMgr.Delay(0.2f, () => Stop(index));
        }

        public void ManualStop()
        {
            _isAuto = false;
            for (int i = 0; i < ReelCount; i++)
                Stop(i);
        }

        public void StartBackRoll(int wheelIndex)
        {
            if (wheelIndex < 0 || wheelIndex >= ReelCount)
                return;

            _startBackRollCount[wheelIndex] += 1;
            if (_startBackRollCount[wheelIndex] == ElementPerReel)
                StopRoll();
        }

        public void BackRollEnd(int wheelIndex)
        {
            if (wheelIndex < 0 || wheelIndex >= ReelCount)
                return;

            _finishBackRollCount[wheelIndex] += 1;
            if (_finishBackRollCount[wheelIndex] < ElementPerReel)
                return;

            _rollEndCount += 1;
            if (_elementBoxBonus[wheelIndex] && _elementBoxBonusData[wheelIndex] == 0)
            {
                int data = _bonusScores[wheelIndex];
                _elements[wheelIndex][0].SetBonus(data);
                _elementBoxBonusData[wheelIndex] = data;
                _rewardMgr.UpdateBonusTime(3);
                //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusSymbolAppear);
                //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusDown1);
            }

            if (_rollEndCount >= ReelCount)
                RollEnd();
        }

        public void Dispose()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                if (_elements[i] == null)
                    continue;
                for (int j = 0; j < _elements[i].Count; j++)
                    _elements[i][j].Dispose();
            }
        }

        private void InitData()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                _elementBoxRoll[i] = false;
                _startBackRollCount[i] = 0;
                _finishBackRollCount[i] = 0;
            }

            _rollEndCount = 0;
            for (int i = 0; i < ReelCount; i++)
            {
                if (_elementBoxBonus[i])
                    _rollEndCount += 1;
            }

            for (int i = 0; i < ReelCount; i++)
            {
                if (_elementBoxBonus[i])
                    continue;
                for (int j = 0; j < _elements[i].Count; j++)
                    _elements[i][j].InitData();
            }

            _isAuto = true;
        }

        private int GetCanStopIndex()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                if (_elementBoxRoll[i])
                    return i;
            }

            return ReelCount;
        }

        private void Stop(int wheelIndex)
        {
            if (wheelIndex < 0 || wheelIndex >= ReelCount)
                return;
            if (!_elementBoxRoll[wheelIndex])
                return;

            _elementBoxRoll[wheelIndex] = false;
            bool landBonus = Contains(_singleBonus, wheelIndex);
            if (landBonus)
            {
                _elementBoxBonus[wheelIndex] = true;
                _singleBonus.Remove(wheelIndex);
            }

            for (int j = 0; j < _elements[wheelIndex].Count; j++)
            {
                RewardElement3993 element = _elements[wheelIndex][j];
                if (j == 0)
                {
                    if (landBonus)
                        element.SetBonus(_bonusScores[wheelIndex]);
                    else
                        element.SetBlank();
                }

                element.StartStopRoll();
            }
        }

        private void RollEnd()
        {
            //if (GameSoundHelper3993.Instance.IsPlaySound(SoundKey.BonusRolling))GameSoundHelper3993.Instance.StopSound(SoundKey.BonusRolling);

            if (CalculationTimesAndCount(checkGameEnd: true))
                return;

            if (!_rewardMgr.IsGameOver)
                _rewardMgr.Ready2Start();
        }

        private bool CalculationTimesAndCount(bool checkGameEnd = false)
        {
            _singleBonus.Clear();
            int showCount = 0;
            int totalShowCount = 0;
            for (int i = 0; i < ReelCount; i++)
            {
                if (_bonusScores[i] > 0)
                    totalShowCount += 1;
                if (_elementBoxBonus[i])
                    showCount += 1;
            }

            int noShowCount = totalShowCount - showCount;
            DebugUtils.Log($"[3993][RewardRoll] show={showCount} total={totalShowCount} noShow={noShowCount} bonusTime={_rewardMgr.BonusTime}");

            if (checkGameEnd)
            {
                if (noShowCount <= 0 && _rewardMgr.BonusTime <= 0)
                {
                    DebugUtils.Log("[3993][RewardRoll] ，结束大奖");
                    _rewardMgr.GameEnd();
                    return true;
                }
            }

            if (TryApplyBonusRoundPlan())
            {
                DebugUtils.Log($"[3993][RewardRoll] BonusRound spin={_bonusRoundSpinIndex} land=[{string.Join(",", _singleBonus)}]");
            }

            return false;
        }

        private bool TryApplyBonusRoundPlan()
        {
            List<List<int>> plan = ContentModel.Instance.BonusRound;
            if (plan == null || plan.Count == 0 || _bonusRoundSpinIndex >= plan.Count)
                return false;

            List<int> thisSpin = plan[_bonusRoundSpinIndex++];
            if (thisSpin == null)
                return true;

            for (int i = 0; i < thisSpin.Count; i++)
            {
                int index = thisSpin[i];
                if (index < 0 || index >= ReelCount)
                {
                    DebugUtils.LogWarning($"[3993][RewardRoll] BonusRound index 越界: {index}");
                    continue;
                }

                if (_bonusScores[index] <= 0 || _elementBoxBonus[index] || Contains(_singleBonus, index))
                {
                    DebugUtils.LogWarning($"[3993][RewardRoll] BonusRound index={index} 无法停出，已跳过");
                    continue;
                }

                _singleBonus.Add(index);
            }

            return true;
        }

        private static bool Contains(List<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                    return true;
            }

            return false;
        }
    }
}
