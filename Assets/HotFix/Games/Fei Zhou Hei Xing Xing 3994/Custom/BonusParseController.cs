using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom
{
    /// <summary>
    /// Jackpot/Bonus 游戏单圈解析结果
    /// </summary>
    public class BonusSpin
    {
        /// <summary>第几圈（1-based）</summary>
        public int SpinIndex { get; set; }

        /// <summary>原始掩码</summary>
        private int Mask { get; set; }

        /// <summary>香蕉位置掩码（低16位）</summary>
        private int BananaMask => Mask & 0xFFFF;

        /// <summary>猩猩位置掩码（高16位）</summary>
        private int GorillaMask => Mask >> 16;

        /// <summary>香蕉位置列表（0~14）</summary>
        private List<int> BananaPositions { get; set; } = new List<int>();

        /// <summary>猩猩位置列表（0~14）</summary>
        private List<int> GorillaPositions { get; set; } = new List<int>();

        /// <summary>香蕉金额列表（按位置从小到大排序）</summary>
        private List<int> BananaValues { get; set; }

        /// <summary>该圈总得分</summary>
        public int TotalScore => BananaValues.Sum();

        /// <summary>15格盘面最终显示值（0=空, 20001=纯猩猩, 30000+香蕉=猩猩香蕉重叠）</summary>
        public List<int> Grid { get; private set; } = new List<int>(15);

        public BonusSpin(int spinIndex, int mask, List<int> bananaValues)
        {
            SpinIndex = spinIndex;
            Mask = mask;
            BananaValues = new List<int>(bananaValues);

            // 解析香蕉位置（低16位）
            for (int i = 0; i < 15; i++)
            {
                if ((BananaMask & (1 << i)) != 0)
                    BananaPositions.Add(i);
            }

            // 解析猩猩位置（高16位）
            for (int i = 0; i < 15; i++)
            {
                if ((GorillaMask & (1 << i)) != 0)
                    GorillaPositions.Add(i);
            }

            // 构建15格盘面
            BuildGrid();
        }

        private void BuildGrid()
        {
            Grid = Enumerable.Repeat(0, 15).ToList();

            // 先放置香蕉金额
            for (int i = 0; i < BananaPositions.Count; i++)
            {
                int pos = BananaPositions[i];
                int val = BananaValues[i];

                // 如果该位置同时有猩猩，显示值为 30000 + 香蕉金额
                if (GorillaPositions.Contains(pos))
                    Grid[pos] = 30000 + val;
                else
                    Grid[pos] = val;
            }

            // 再放置纯猩猩（没有香蕉的猩猩格）
            foreach (int pos in GorillaPositions)
            {
                if (!BananaPositions.Contains(pos))
                    Grid[pos] = 20001;
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"第{SpinIndex}圈 — 掩码 {Mask}");
            sb.AppendLine($"  香蕉位置: [{string.Join(", ", BananaPositions)}] → 金额: [{string.Join(", ", BananaValues)}]");
            sb.AppendLine($"  猩猩位置: [{string.Join(", ", GorillaPositions)}]");
            sb.AppendLine($"  15格盘面: [{string.Join(", ", Grid)}]");
            sb.AppendLine($"  合计得分: {TotalScore}");
            return sb.ToString();
        }
    }

    public static class BonusParseController
    {
        /// <summary>
        /// 解析所有圈数，每圈用 List 存储15格盘面，全部放入 Queue
        /// </summary>
        /// <param name="bonusPos">原始 BonusPos 数组（每圈一个 int[]）</param>
        /// <param name="bonusData">原始 BonusData 数组（每圈得分）</param>
        public static Queue<List<int>> ParseAllSpins(List<int[]> bonusPos, List<int> bonusData)
        {
            Queue<List<int>> queue = new Queue<List<int>>();

            for (int i = 0; i < bonusPos.Count; i++)
            {
                int mask = bonusPos[i][0];
                List<int> bananaValues = bonusPos[i].Skip(1).ToList();

                BonusSpin spin = new BonusSpin(i + 1, mask, bananaValues);

                // 验证得分是否匹配
                if (i < bonusData.Count && spin.TotalScore != bonusData[i])
                {
                    Debug.LogError($"⚠ 警告: 第{i + 1}圈得分不匹配! 解析={spin.TotalScore}, 期望={bonusData[i]}");
                }

                // 将15格盘面 List<int> 入队
                queue.Enqueue(spin.Grid);
            }

            return queue;
        }

        /// <summary>
        /// 传入 BonusData JSON 字符串，返回 List&lt;int&gt;
        /// </summary>
        /// <param name="bonusDataJson">例如: "[750,825,0,425,1500,1000,1750,475,0,0,0,0,0,0,0]"</param>
        /// <returns>List&lt;int&gt; — 每圈得分</returns>
        public static List<int> ParseBonusData(string bonusDataJson)
        {
            if (string.IsNullOrWhiteSpace(bonusDataJson))
                return new List<int>();

            // 去掉首尾空白和方括号
            string trimmed = bonusDataJson.Trim().TrimStart('[').TrimEnd(']');
            if (string.IsNullOrWhiteSpace(trimmed))
                return new List<int>();

            var result = new List<int>();
            string[] parts = trimmed.Split(',');

            foreach (string part in parts)
            {
                if (int.TryParse(part.Trim(), out int value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

        /// <summary>
        /// 传入 BonusPos JSON 字符串，返回 List&lt;int[]&gt;
        /// </summary>
        /// <param name="bonusPosJson">例如: "[[16392,375,375],[258,400,425],...]"</param>
        /// <returns>List&lt;int[]&gt; — 每圈一个 int[]</returns>
        public static List<int[]> ParseBonusPos(string bonusPosJson)
        {
            if (string.IsNullOrWhiteSpace(bonusPosJson))
                return new List<int[]>();
        
            var result = new List<int[]>();
            string trimmed = bonusPosJson.Trim();
        
            // 去掉最外层方括号
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }
        
            // 按顶层数组拆分（简单实现：按 ],[ 分割）
            // 更健壮的做法：逐字符解析
            var arrays = SplitTopLevelArrays(trimmed);
        
            foreach (string arr in arrays)
            {
                string inner = arr.Trim().TrimStart('[').TrimEnd(']');
                if (string.IsNullOrWhiteSpace(inner))
                    continue;
        
                string[] parts = inner.Split(',');
                var values = new List<int>();
        
                foreach (string part in parts)
                {
                    if (int.TryParse(part.Trim(), out int value))
                    {
                        values.Add(value);
                    }
                }
        
                if (values.Count > 0)
                    result.Add(values.ToArray());
            }
        
            return result;
        }

        /// <summary>
        /// 将 "[a,b],[c,d],[e]" 拆分为 ["[a,b]", "[c,d]", "[e]"]
        /// </summary>
        private static List<string> SplitTopLevelArrays(string input)
        {
            var result = new List<string>();
            int depth = 0;
            int start = 0;
        
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
        
                if (c == '[') depth++;
                else if (c == ']') depth--;
        
                if (depth == 0 && c == ']')
                {
                    result.Add(input.Substring(start, i - start + 1));
                    start = i + 1;
                    // 跳过逗号和空格
                    while (start < input.Length && (input[start] == ',' || input[start] == ' '))
                        start++;
                }
            }
        
            return result;
        }

        /// <summary> 通过索引获取其在 3 行 5 列格子中所在的行和列；索引 0-4 为第 0 行，索引 5-9 为第 1 行，索引 10-14 为第 2 行。 </summary>
        public static (int Row, int Col) GetRowColByIndex(int index)
        {
            if (index < 0 || index >= 15)
            {
                Debug.LogError($"GetRowColByIndex: index {index} out of range [0, 14]");
                return (-1, -1);
            }

            const int colCount = 5;
            int row = index / colCount;
            int col = index % colCount;
            return (row, col);
        }
    }
}