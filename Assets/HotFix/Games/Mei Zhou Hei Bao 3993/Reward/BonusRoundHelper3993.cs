using GameMaker;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>
    /// 大奖每把 Spin 停轴计划：二维列表，内层 0~3 个轴 index。
    /// </summary>
    public static class BonusRoundHelper3993
    {
        public const int MaxBonusPerSpin = 3;
        public const int InitBonusTime = 3;

        public static List<List<int>> ParseFromJson(JSONNode node)
        {
            var result = new List<List<int>>();
            if (node == null || !node.IsArray)
                return result;

            for (int r = 0; r < node.Count; r++)
            {
                var spin = new List<int>();
                JSONNode rowNode = node[r];
                if (rowNode != null && rowNode.IsArray)
                {
                    for (int c = 0; c < rowNode.Count; c++)
                        spin.Add(rowNode[c].AsInt);
                }

                result.Add(spin);
            }

            return result;
        }

        /// <summary>
        /// 按 BonusData + 触发 Matrix 模拟每把 0~3 个 Bonus 停出顺序（Mock / 无协议字段时用）。
        /// </summary>
        public static List<List<int>> Build(IList<int> matrix, IList<int> bonusData, int bonusId = 12)
        {
            var result = new List<List<int>>();
            var remaining = new List<int>();

            for (int i = 0; i < ReelCount; i++)
            {
                int score = bonusData != null && i < bonusData.Count ? bonusData[i] : 0;
                bool preLocked = matrix != null && i < matrix.Count && matrix[i] == bonusId && score > 0;
                if (score > 0 && !preLocked)
                    remaining.Add(i);
            }

            int bonusTime = InitBonusTime;
            int guard = 0;
            while ((remaining.Count > 0 || bonusTime > 0) && guard++ < 500)
            {
                var thisSpin = new List<int>();
                int noShow = remaining.Count;

                if (noShow > 0)
                {
                    int maxCount = Mathf.Min(MaxBonusPerSpin, noShow);
                    int count = bonusTime == 1 ? noShow : Random.Range(0, maxCount + 1);
                    for (int c = 0; c < count; c++)
                    {
                        int pick = Random.Range(0, remaining.Count);
                        thisSpin.Add(remaining[pick]);
                        remaining.RemoveAt(pick);
                    }
                }

                result.Add(thisSpin);
                bonusTime--;

                if (thisSpin.Count > 0)
                    bonusTime = InitBonusTime;

                if (remaining.Count == 0 && bonusTime <= 0)
                    break;
            }

            return result;
        }

        public static bool Validate(IList<int> matrix, IList<int> bonusData, IList<List<int>> bonusRound, int bonusId = 12)
        {
            if (bonusRound == null || bonusData == null)
                return false;

            var locked = new bool[ReelCount];
            var expected = new List<int>();
            for (int i = 0; i < ReelCount; i++)
            {
                int score = i < bonusData.Count ? bonusData[i] : 0;
                bool preLocked = matrix != null && i < matrix.Count && matrix[i] == bonusId && score > 0;
                locked[i] = preLocked;
                if (score > 0 && !preLocked)
                    expected.Add(i);
            }

            var seen = new List<int>();
            for (int r = 0; r < bonusRound.Count; r++)
            {
                List<int> spin = bonusRound[r];
                if (spin == null || spin.Count > MaxBonusPerSpin)
                {
                    DebugUtils.LogWarning($"[3993][BonusRound] 第{r}把 Bonus 数量非法: {spin?.Count ?? -1}");
                    return false;
                }

                for (int j = 0; j < spin.Count; j++)
                {
                    int index = spin[j];
                    if (index < 0 || index >= ReelCount)
                    {
                        DebugUtils.LogWarning($"[3993][BonusRound] 第{r}把 index 越界: {index}");
                        return false;
                    }

                    if (bonusData[index] <= 0 || locked[index] || Contains(seen, index))
                    {
                        DebugUtils.LogWarning($"[3993][BonusRound] 第{r}把 index={index} 与 BonusData/锁定状态冲突");
                        return false;
                    }

                    seen.Add(index);
                }
            }

            if (seen.Count != expected.Count)
            {
                DebugUtils.LogWarning($"[3993][BonusRound] 计划轴数 {seen.Count} 与待出轴数 {expected.Count} 不一致");
                return false;
            }

            for (int i = 0; i < expected.Count; i++)
            {
                if (!Contains(seen, expected[i]))
                    return false;
            }

            return true;
        }

        private const int ReelCount = RewardRoll3993.ReelCount;

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
