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
        /// <summary>15 轴数量。</summary>
        public const int ReelCount = 15;
        /// <summary>每轴图标节点数（可见 1 + 循环缓冲）。</summary>
        public const int ElementPerReel = 4;

        /// <summary>大奖盘根节点。</summary>
        private readonly GComponent _root;
        /// <summary>所属流程管理器。</summary>
        private readonly RewardMgr3993 _rewardMgr;
        /// <summary>Bonus/彩金 Spine 对象池。</summary>
        private readonly FguiPoolHelper _fguiPoolHelper;
        /// <summary>层级参照节点，锁定格抬高 sortingOrder。</summary>
        private readonly GComponent _goExpectation;

        /// <summary>符号对象池，供 Element 取 Spine。</summary>
        internal FguiPoolHelper FguiPoolHelper => _fguiPoolHelper;
        /// <summary>层级参照。</summary>
        internal GComponent GoExpectation => _goExpectation;
        /// <summary>收集光效预制体。</summary>
        internal GameObject GlowPrefab { get; private set; }
        /// <summary>每轴上的图标列表。</summary>
        private readonly List<RewardElement3993>[] _elements = new List<RewardElement3993>[ReelCount];
        /// <summary>每轴外框节点。</summary>
        private readonly GComponent[] _reelBoxes = new GComponent[ReelCount];

        /// <summary>该轴本把是否在滚。</summary>
        private readonly bool[] _elementBoxRoll = new bool[ReelCount];
        /// <summary>该轴是否已锁定 bonus/彩金。</summary>
        private readonly bool[] _elementBoxBonus = new bool[ReelCount];
        /// <summary>锁定格已写入的分值，0 表示刚停出尚未刷 Spine。</summary>
        private readonly int[] _elementBoxBonusData = new int[ReelCount];
        /// <summary>BonusData 对应的最终分值（含彩金编码）。</summary>
        private readonly int[] _bonusScores = new int[ReelCount];
        /// <summary>该轴已进入回滚的图标数。</summary>
        private readonly int[] _startBackRollCount = new int[ReelCount];
        /// <summary>该轴回滚完成的图标数。</summary>
        private readonly int[] _finishBackRollCount = new int[ReelCount];

        /// <summary>本把计划停出的轴下标。</summary>
        private readonly List<int> _singleBonus = new List<int>();
        /// <summary>已停稳轴数（含预先锁定轴）。</summary>
        private int _rollEndCount;
        /// <summary>true 为自动逐轴停，false 为手动全停。</summary>
        private bool _isAuto = true;
        /// <summary>BonusRound 计划当前把下标。</summary>
        private int _bonusRoundSpinIndex;
        /// <summary>本把新停出的 bonus/彩金数量。</summary>
        private int _landedThisSpin;
        /// <summary>本把是否有轴真正滚动（全锁则 false）。</summary>
        private bool _didRollThisSpin;

        /// <summary>绑定 15 轴 reel/symbols/rollElement 节点。</summary>
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

        /// <summary>初始化各轴图标槽位。</summary>
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

        /// <summary>清空锁定与分值，全部图标复位。</summary>
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

        /// <summary>设置收集光效预制体，供锁定格使用。</summary>
        public void SetGlowPrefab(GameObject prefab)
        {
            GlowPrefab = prefab;
        }

        /// <summary>收集已锁定且有分的可见格，供结算飞分。</summary>
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

        /// <summary>按触发盘面与 BonusData 铺初始锁定格，并生成第一把停出计划。</summary>
        public void InitRoll(IList<int> matrix, IList<int> bonusData)
        {
            int bonusId = CustomModel.Instance.symbolNumber[12];
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

            int sum = ContentModel.SumBonusAmounts(_bonusScores);
            if (ContentModel.Instance.BonusBet > 0 && sum != ContentModel.Instance.BonusBet)
                DebugUtils.LogWarning($"[3993][RewardRoll] BonusBet 校验不一致，sum={sum} BonusBet={ContentModel.Instance.BonusBet}");
            else
                DebugUtils.Log($"[3993][RewardRoll] InitRoll sum={sum} BonusBet={ContentModel.Instance.BonusBet}");

            _bonusRoundSpinIndex = 0;
            CalculationTimesAndCount();
            SyncLockedIdles();
        }

        /// <summary>驱动未锁定轴循环滚动。</summary>
        public void Update(float dt)
        {
            for (int i = 0; i < ReelCount; i++)
            {
                for (int j = 0; j < _elements[i].Count; j++)
                    _elements[i][j].Update(dt);
            }
        }

        /// <summary>未锁定轴开转；若已全锁则直接 RollEnd。</summary>
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

            _didRollThisSpin = anyRolling;
            if (!anyRolling)
                RollEnd();
        }

        /// <summary>自动模式：延迟停下一根仍在滚的轴。</summary>
        public void StopRoll()
        {
            if (!_isAuto)
                return;

            int index = GetCanStopIndex();
            if (index >= ReelCount)
                return;

            _rewardMgr.Delay(0.2f, () => Stop(index));
        }

        /// <summary>手动急停：所有滚动轴立即停。</summary>
        public void ManualStop()
        {
            _isAuto = false;
            for (int i = 0; i < ReelCount; i++)
                Stop(i);
        }

        /// <summary>轴上一个图标进入回弹；四个都进入后触发停下一轴。</summary>
        public void StartBackRoll(int wheelIndex)
        {
            if (wheelIndex < 0 || wheelIndex >= ReelCount)
                return;

            _startBackRollCount[wheelIndex] += 1;
            if (_startBackRollCount[wheelIndex] == ElementPerReel)
                StopRoll();
        }

        /// <summary>轴上回弹完成：新锁定则刷 Spine 并重置次数；全轴停稳则 RollEnd。</summary>
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
                SyncLockedIdles();
                //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusSymbolAppear);
                //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusDown1);
            }

            if (_rollEndCount >= ReelCount)
                RollEnd();
        }

        /// <summary>释放全部图标资源。</summary>
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

        /// <summary>一把开转前清计数；已锁定轴计入 rollEndCount。</summary>
        private void InitData()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                _elementBoxRoll[i] = false;
                _startBackRollCount[i] = 0;
                _finishBackRollCount[i] = 0;
            }

            _rollEndCount = 0;
            _landedThisSpin = 0;
            _didRollThisSpin = false;
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

        /// <summary>已锁定格从头播 idle。</summary>
        private void SyncLockedIdles()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                if (!_elementBoxBonus[i] || _bonusScores[i] <= 0)
                    continue;
                if (_elements[i] == null || _elements[i].Count == 0)
                    continue;
                _elements[i][0].PlayIdleFromStart();
            }
        }

        /// <summary>从左到右找第一根仍在滚的轴。</summary>
        private int GetCanStopIndex()
        {
            for (int i = 0; i < ReelCount; i++)
            {
                if (_elementBoxRoll[i])
                    return i;
            }

            return ReelCount;
        }

        /// <summary>停指定轴：在计划内则锁定 bonus，否则空白，然后回弹。</summary>
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
                _landedThisSpin++;
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

        /// <summary>全轴停稳：驱动 NPC 反应，再判断结束或开下一把。</summary>
        private void RollEnd()
        {
            //if (GameSoundHelper3993.Instance.IsPlaySound(SoundKey.BonusRolling))GameSoundHelper3993.Instance.StopSound(SoundKey.BonusRolling);

            if (_didRollThisSpin)
                _rewardMgr.PlayNpcAfterRoll(_landedThisSpin > 0);

            if (CalculationTimesAndCount(checkGameEnd: true))
                return;

            if (!_rewardMgr.IsGameOver)
                _rewardMgr.Ready2Start();
        }

        /// <summary>统计未出图标数；可结束则 GameEnd，否则套用下一把 BonusRound。</summary>
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

        /// <summary>从 BonusRound 取出本把应停出的轴下标写入 _singleBonus。</summary>
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

        /// <summary>线性查找列表是否含指定值。</summary>
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
