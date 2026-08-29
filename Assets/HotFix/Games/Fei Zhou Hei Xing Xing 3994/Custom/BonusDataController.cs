// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// namespace HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom
// {
//     public class BonusOnceData
//     {
//         /// <summary>
//         /// 构造单局数据
//         /// </summary>
//         /// <param name="totalScore">本局总得分</param>
//         /// <param name="specialIconCount">本局特殊图标（-1）的数量，由外部构造算法决定</param>
//         public BonusOnceData(int totalScore, int specialIconCount = 0)
//         {
//         }
//
//         /// <summary> 本局最终的各个格子的节点信息（长度固定15） </summary>
//         public readonly List<int> CurrentBonusData = new List<int>();
//
//         /// <summary> 彩金格子总数 </summary>
//         private const int BonusCellCount = 15;
//
//      
//     }
//
//     public class BonusDataController : BaseManager<BonusDataController>
//     {
//         /// <summary> 默认总局数（初始值） </summary>
//         private const int DefaultBonusRoundCount = 3;
//
//         /// <summary> 当前总局数，由外部传入 totalRoundCount 时更新，默认 3 </summary>
//         public int TotalBonusRoundCount { get; private set; } = DefaultBonusRoundCount;
//
//         /// <summary> 根据算法给出的 bonusData 解析出每局的总得分并存入 List </summary>
//         public List<int> GetEachRoundScore(string bonusData)
//         {
//             string trimmed = bonusData.TrimStart('[').TrimEnd(']');
//             return string.IsNullOrWhiteSpace(trimmed)
//                 ? new List<int>()
//                 : trimmed.Split(',').Select(s => int.Parse(s.Trim())).ToList();
//         }
//
//         /// <summary>
//         /// 计算特殊图标总数的目标范围，根据 jpCount 决定：
//         /// - jpCount = 1：5 ~ 9 个
//         /// - jpCount = 2：10 ~ 14 个
//         /// - jpCount = 3：15 个
//         /// </summary>
//         private int GetTargetSpecialIconCount(int jpCount)
//         {
//             switch (jpCount)
//             {
//                 case 1:
//                     return Random.Range(5, 10); // 5,6,7,8,9
//                 case 2:
//                     return Random.Range(10, 15); // 10,11,12,13,14
//                 case 3:
//                     return 15;
//                 default:
//                     return Random.Range(0, 5);
//             }
//         }
//
//         /// <summary>
//         /// 根据总局数 totalRoundCount、jpCount，通过回溯算法构造特殊图标（-1）的分布。
//         /// 
//         /// 核心规则：
//         /// - 起始剩余 3 次机会；
//         /// - 每玩一局，剩余次数 -1；
//         /// - 若该局有特殊图标（≥1 个），剩余次数重置为 3；
//         /// - 最终恰好进行 totalRoundCount 局后，剩余次数 <= 0（游戏结束）。
//         /// - 特殊图标总数由 jpCount 决定，每局最多 5 个。
//         /// 
//         /// 约束：
//         /// 1. 最后 3 局不允许出现特殊图标（保证最后阶段自然耗尽，不再刷新）；
//         /// 2. 仅在 validRounds 中指定的局才允许出现特殊图标（默认排除 0 分局）；
//         /// 3. 若严格构造失败，会尝试放宽 validRounds 限制（允许所有非最后 3 局出现特殊）。
//         /// 
//         /// 返回：有特殊图标的局的索引 → 该局特殊图标数量的映射。
//         /// </summary>
//         private Dictionary<int, int> CalculateSpecialRounds(int totalRoundCount, HashSet<int> validRounds, int jpCount)
//         {
//             int totalSpecialCount = GetTargetSpecialIconCount(jpCount);
//             int minRoundsNeeded = (totalSpecialCount + 4) / 5; // ceil(totalSpecialCount / 5)，每局最多 5 个
//             int maxRoundsAllowed = totalSpecialCount; // 每局至少 1 个
//
//             // 边界检查：最后 3 局无特殊，所以最多只有 totalRoundCount - 3 局可以放特殊
//             int maxAvailableRounds = totalRoundCount - 3;
//             if (maxAvailableRounds < minRoundsNeeded)
//             {
//                 Debug.LogError(
//                     $"[BonusDataController] 总局数 {totalRoundCount} 不足，最多只有 {maxAvailableRounds} 局可放特殊图标，" +
//                     $"但 jpCount={jpCount} 需要至少 {minRoundsNeeded} 局来容纳 {totalSpecialCount} 个特殊图标。");
//                 return new Dictionary<int, int>();
//             }
//
//             var specialCounts = new Dictionary<int, int>();
//             var specials = new HashSet<int>();
//
//             // 回溯搜索：round 为当前局索引（0-based），remaining 为当前局开始前的剩余次数
//             bool Dfs(int round, int remaining, int specialRoundCountSoFar)
//             {
//                 // 已处理完所有局，检查是否恰好结束
//                 if (round == totalRoundCount)
//                     return remaining <= 0;
//
//                 // 无法继续，剪枝
//                 if (remaining <= 0)
//                     return false;
//
//                 // 剪枝：即使后面每局都 -1，remaining 也减不到 0，不可能自然结束
//                 if (remaining > totalRoundCount - round)
//                     return false;
//
//                 // 剪枝：即使后面所有局都放特殊，specialRoundCount 也不够容纳 totalSpecialCount
//                 int maxPossibleSpecialRounds = specialRoundCountSoFar + (totalRoundCount - round);
//                 if (maxPossibleSpecialRounds < minRoundsNeeded)
//                     return false;
//
//                 // 剪枝：已经确定的特殊局数太多，超过了上限（每局至少 1 个）
//                 if (specialRoundCountSoFar > maxRoundsAllowed)
//                     return false;
//
//                 bool canHaveSpecial = validRounds.Contains(round);
//
//                 // 随机打乱尝试顺序，使每次生成的特殊图标分布更自然、不固定
//                 bool trySpecialFirst = canHaveSpecial && Random.value < 0.5f;
//
//                 if (trySpecialFirst)
//                 {
//                     // 先尝试：本局有特殊图标
//                     if (canHaveSpecial)
//                     {
//                         specials.Add(round);
//                         if (Dfs(round + 1, 3, specialRoundCountSoFar + 1)) return true;
//                         specials.Remove(round);
//                     }
//
//                     // 再尝试：本局无特殊图标
//                     if (Dfs(round + 1, remaining - 1, specialRoundCountSoFar)) return true;
//                 }
//                 else
//                 {
//                     // 先尝试：本局无特殊图标
//                     if (Dfs(round + 1, remaining - 1, specialRoundCountSoFar)) return true;
//                     // 再尝试：本局有特殊图标
//                     if (canHaveSpecial)
//                     {
//                         specials.Add(round);
//                         if (Dfs(round + 1, 3, specialRoundCountSoFar + 1)) return true;
//                         specials.Remove(round);
//                     }
//                 }
//
//                 return false;
//             }
//
//             bool success = Dfs(0, 3, 0);
//
//             // 若严格模式失败，尝试放宽：允许所有非最后 3 局出现特殊图标（包括 0 分局）
//             if (!success)
//             {
//                 Debug.LogWarning(
//                     $"[BonusDataController] 严格模式构造失败（0 分局不允许特殊），尝试放宽限制。总局数: {totalRoundCount}, jpCount: {jpCount}");
//
//                 var relaxedValidRounds = new HashSet<int>();
//                 for (int i = 0; i < totalRoundCount - 3; i++)
//                     relaxedValidRounds.Add(i);
//
//                 validRounds = relaxedValidRounds;
//                 specials.Clear();
//                 success = Dfs(0, 3, 0);
//             }
//
//             if (!success)
//             {
//                 Debug.LogError(
//                     $"[BonusDataController] 无法为总局数 {totalRoundCount}、jpCount {jpCount} 构造合法的特殊图标分布。");
//                 return new Dictionary<int, int>();
//             }
//
//             // 分配特殊图标数量到各个有特殊图标的局中
//             // 每局至少 1 个，最多 5 个，总和 = totalSpecialCount
//             int specialRoundCount = specials.Count;
//
//             // 先每局分配 1 个
//             foreach (var r in specials)
//                 specialCounts[r] = 1;
//
//             int remaining = totalSpecialCount - specialRoundCount;
//
//             // 随机分配剩余数量，每局不超过 5 个
//             var roundList = specials.ToList();
//             int safetyCounter = 0;
//             while (remaining > 0 && safetyCounter < 1000)
//             {
//                 safetyCounter++;
//                 int idx = Random.Range(0, roundList.Count);
//                 int r = roundList[idx];
//                 if (specialCounts[r] < 5)
//                 {
//                     specialCounts[r]++;
//                     remaining--;
//                 }
//             }
//
//             if (remaining > 0)
//             {
//                 Debug.LogError(
//                     $"[BonusDataController] 特殊图标数量分配失败，剩余 {remaining} 个无法分配。totalSpecialCount={totalSpecialCount}, specialRoundCount={specialRoundCount}");
//             }
//
//             return specialCounts;
//         }
//
//         /// <summary>
//         /// 根据后端返回的每局得分、jpCount 与总局数，生成与总局数对应的所有局数据并放入队列。
//         ///
//         /// 说明：
//         /// 1. 外部传入的总局数 totalRoundCount（BonusTime）与队列 Count 完全一致；
//         ///    - 若 roundScores 分数多于总局数，只取前 totalRoundCount 个；
//         ///    - 若 roundScores 分数不足 totalRoundCount 个，补充 0 分局保证队列长度等于总局数；
//         /// 2. 最后三轮（队列末尾 3 局）不允许出现特殊图标（-1），保证最后阶段不会再刷新局数；
//         /// 3. 0 分局（补充局）默认不允许出现特殊图标，若因此导致构造失败会自动放宽该限制；
//         /// 4. jpCount 决定特殊图标总数：
//         ///    - 1 个彩金：5~9 个特殊图标
//         ///    - 2 个彩金：10~14 个特殊图标
//         ///    - 3 个彩金：15 个特殊图标
//         ///    每局最多 5 个特殊图标；
//         /// 5. 小游戏中的局数流转（初始 3 局、出现特殊图标时重置为 3 局、否则每局减少 1 局）
//         ///    由 PageGameMain 的 smallGameSpinCount 控制；本方法只负责将完整的局数据准备好放入队列，
//         ///    后续小游戏直接按顺序 Dequeue 消费队列中的数据进行逻辑处理即可。
//         /// </summary>
//         public Queue<BonusOnceData> GetEachRoundData(List<int> roundScores, int jpCount,
//             int totalRoundCount = DefaultBonusRoundCount)
//         {
//             TotalBonusRoundCount = totalRoundCount;
//             Queue<BonusOnceData> result = new Queue<BonusOnceData>();
//             if (totalRoundCount <= 0)
//                 return result;
//
//             // 确定严格模式下哪些局可以有特殊图标（有真实分数且不是最后 3 局）
//             var validSpecialRounds = new HashSet<int>();
//             for (int i = 0; i < totalRoundCount; i++)
//             {
//                 bool hasRealScore = roundScores != null && i < roundScores.Count && roundScores[i] > 0;
//                 bool isLastThree = i >= totalRoundCount - 3;
//                 if (hasRealScore && !isLastThree)
//                     validSpecialRounds.Add(i);
//             }
//
//             // 通过回溯算法计算特殊图标分布，保证总局数严格等于 totalRoundCount
//             var specialCounts = CalculateSpecialRounds(totalRoundCount, validSpecialRounds, jpCount);
//
//             // 按顺序生成每一局的完整格子数据并入队
//             for (int i = 0; i < totalRoundCount; i++)
//             {
//                 bool hasRealScore = roundScores != null && i < roundScores.Count;
//                 int score = hasRealScore ? roundScores[i] : 0;
//                 int specialCount = specialCounts.ContainsKey(i) ? specialCounts[i] : 0;
//
//                 result.Enqueue(new BonusOnceData(score, specialCount));
//             }
//
//             return result;
//         }
//     }
// }