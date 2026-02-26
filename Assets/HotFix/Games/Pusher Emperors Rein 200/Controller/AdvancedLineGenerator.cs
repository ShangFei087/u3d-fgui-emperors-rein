using SlotMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;



    public class SymbolInclude
    {
        public int colIdx = -1;
        public int rowIdx = -1;
        public int symbolNumber = -1;
    }

    public class AdvancedLineGenerator : MonoSingleton<AdvancedLineGenerator>
    {
        // 三行五列的游戏结果矩阵（0=第一行，1=第二行，2=第三行）
        public List<List<int>> gameResultList = new List<List<int>>
        {
            new List<int>(new int[5]), // 第一行
            new List<int>(new int[5]), // 第二行
            new List<int>(new int[5]) // 第三行
        };

        public string strDeckRowCol;

        /// <summary>
        /// 生成3行5列游戏矩阵，核心规则：
        /// 1. 有效连线必须从第一列（索引0）开始，包含3个及以上连续相同符号
        /// 2. 鬼牌（10）若存在形成有效连线的风险，直接替换鬼牌
        /// </summary>




        public string GenerateGameArray(List<List<int>> allLines, List<int> symbolNumber,
            List<WinningLineInfo> winningLines, int[] exclude, List<SymbolInclude> include)
        {
            if (winningLines == null)
                winningLines = new List<WinningLineInfo>();
            // 初始化游戏结果矩阵
            gameResultList = new List<List<int>>();
            for (int raw = 0; raw < 3; raw++)
            {
                // 为每行创建一个包含5个0的 List<int>，避免空引用
                List<int> row = new List<int>();
                for (int col = 0; col < 5; col++)
                {
                    row.Add(-1);
                }

                gameResultList.Add(row); // 将行添加到矩阵中
            }

            List<int> excludeLst = new List<int>();
            excludeLst.AddRange(exclude);

            foreach (WinningLineInfo item in winningLines)
            {
                excludeLst.Add(item.SymbolNumber);

                int lineIndex = item.LineNumber - 1;

                List<int> line = allLines[lineIndex];

                for (int cIndex = 0; cIndex < item.WinCount; cIndex++)
                {
                    int rIndex = line[cIndex];
                    gameResultList[rIndex][cIndex] = item.SymbolNumber;
                }
            }

            foreach (SymbolInclude symbolInclude in include)
            {
                int colIdx = symbolInclude.colIdx;
                int rowIdx = symbolInclude.colIdx;
                int endlessLoop = 1000;
                if (colIdx == -1 && rowIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }
                else if (colIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }
                else if (rowIdx == -1)
                {
                    do
                    {
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }

                if (endlessLoop < 0)
                    DebugUtils.LogError($"【endless loop】: when add include symbol");

                gameResultList[rowIdx][colIdx] = symbolInclude.symbolNumber;
            }

            for (int i = 0; i < 3; i++)
            {
                if (gameResultList[i][2] == -1)
                {
                    int middleSymbolNumber = -1;
                    int endlessLoop = 1000;

                    do
                    {
                        int symbolIdx = UnityEngine.Random.Range(0, symbolNumber.Count);
                        middleSymbolNumber = symbolNumber[symbolIdx];
                    } while (excludeLst.Contains(middleSymbolNumber) && --endlessLoop >= 0);

                    if (endlessLoop < 0)
                    {
                       DebugUtils.LogError($"【endless loop】: when add middle col symbol");
                    }

                    excludeLst.Add(middleSymbolNumber);

                    gameResultList[i][2] = middleSymbolNumber;
                }
            }


            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (gameResultList[i][j] == -1)
                    {
                        int tempSymbolNumber = -1;
                        int endlessLoop = 1000;
                        do
                        {
                            int symbolIdx = UnityEngine.Random.Range(0, symbolNumber.Count);
                            tempSymbolNumber = symbolNumber[symbolIdx];
                        } while (excludeLst.Contains(tempSymbolNumber) && --endlessLoop >= 0);

                        if (endlessLoop < 0)
                        {
                            DebugUtils.LogError($"【endless loop】: when add remain symbol");
                        }
                        gameResultList[i][j] = tempSymbolNumber;
                    }
                }
            }

            string strDeckRowCol = SlotTool.GetDeckColRow(gameResultList);
            return strDeckRowCol;

        }

#if false
    public string GenerateGameArray(List<int[]> allLines, List<int> symbolNumber,
            List<WinningLineInfo> winningLines, int[] exclude)
        {

            if (winningLines == null)
                winningLines = new List<WinningLineInfo>();

            // 初始化游戏结果矩阵
            gameResultList = new List<List<int>>();
            for (int raw = 0; raw < 3; raw++)
            {
                // 为每行创建一个包含5个0的 List<int>，避免空引用
                List<int> row = new List<int>();
                for (int col = 0; col < 5; col++)
                {
                    row.Add(-1);
                }

                gameResultList.Add(row); // 将行添加到矩阵中
            }


            List<int> excludeLst = new List<int>();
            excludeLst.AddRange(exclude);

            foreach (WinningLineInfo item in winningLines)
            {
                // public int LineNumber;   // 线路号（1到线路总数）
                //    public int SymbolNumber; // 中奖符号（符号池内）
                //    public int WinCount;     // 中奖数量（3-5，特殊符号只能为1）
                excludeLst.Add(item.SymbolNumber);

                int lineIndex = item.LineNumber - 1;

                int[] line = allLines[lineIndex];

                for (int cIndex = 0; cIndex < item.WinCount; cIndex++)
                {
                    int rIndex = line[cIndex];
                    gameResultList[rIndex][cIndex] = item.SymbolNumber;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (gameResultList[i][2] == -1)
                {
                    int middleSymbolNumber = -1;
                    do
                    {
                        int symbolIdx = UnityEngine.Random.Range(0, symbolNumber.Count);
                        middleSymbolNumber = symbolNumber[symbolIdx];
                    } while (excludeLst.Contains(middleSymbolNumber));

                    excludeLst.Add(middleSymbolNumber);

                    gameResultList[i][2] = middleSymbolNumber;
                }
            }


            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (gameResultList[i][j] == -1)
                    {
                        int tempSymbolNumber = -1;
                        do
                        {
                            int symbolIdx = UnityEngine.Random.Range(0, symbolNumber.Count);
                            tempSymbolNumber = symbolNumber[symbolIdx];
                        } while (excludeLst.Contains(tempSymbolNumber));

                        gameResultList[i][j] = tempSymbolNumber;
                    }
                }
            }

            string strDeckRowCol = SlotTool.GetDeckColRow(gameResultList);
            return strDeckRowCol;
        }
#endif














#if false

    public string GenerateGameArray(List<int[]> allLines, List<int> symbolNumber, int gui,
            List<WinningLineInfo> winningLines = null)
        {
            // 最多重试次数，防止无限循环
            int maxRetries = 10;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    // 初始化游戏结果矩阵
                    gameResultList = new List<List<int>>();
                    for (int i = 0; i < 3; i++)
                    {
                        // 为每行创建一个包含5个0的 List<int>，避免空引用
                        List<int> row = new List<int>();
                        for (int j = 0; j < 5; j++)
                        {
                            row.Add(0);
                        }

                        gameResultList.Add(row); // 将行添加到矩阵中
                    }

                    // 重置矩阵为0
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            gameResultList[i][j] = 0;
                        }
                    }

                    // 处理空中奖线路（转为空列表）
                    winningLines ??= new List<WinningLineInfo>();

                    // 检查中奖线路中特殊符号合法性
                    ValidateSpecialSymbolsInWinningLines(winningLines, gui);

                    // 标记中奖位置及禁止列范围
                    bool[,] isWinningPosition = new bool[3, 5];
                    Dictionary<int, HashSet<int>> forbiddenColumnsBySymbol = new Dictionary<int, HashSet<int>>();

                    // 1. 处理中奖线路并计算禁止列（中奖线路必须从第一列开始）
                    if (winningLines.Count > 0)
                    {
                        if (winningLines.Count > 3)
                            throw new ArgumentException("中奖线路数量不能超过3条");

                        foreach (var lineInfo in winningLines)
                        {
                            // 基础参数验证
                            if (lineInfo.LineNumber < 1 || lineInfo.LineNumber > allLines.Count)
                                throw new ArgumentOutOfRangeException(
                                    $"线路号{lineInfo.LineNumber}必须在1-{allLines.Count}之间");
                            if (lineInfo.WinCount < 3 || lineInfo.WinCount > 5)
                                throw new ArgumentOutOfRangeException($"线路{lineInfo.LineNumber}的中奖数量必须在3-5之间");
                            if (!symbolNumber.Contains(lineInfo.SymbolNumber))
                                throw new ArgumentException($"线路{lineInfo.LineNumber}的符号{lineInfo.SymbolNumber}不在符号池内");

                            int targetSymbol = lineInfo.SymbolNumber;
                            int winCount = lineInfo.WinCount;
                            int lineIndex = lineInfo.LineNumber - 1;
                            int[] linePattern = allLines[lineIndex];

                            // 验证中奖线路是否从第一列开始（必须包含第一列）
                            if (linePattern[0] < 0 || linePattern[0] >= 3)
                                throw new InvalidOperationException($"线路{lineInfo.LineNumber}未包含第一列，不符合有效连线规则");

                            // 收集中奖列（必须从0开始）
                            List<int> winningCols = new List<int>();
                            for (int col = 0; col < winCount; col++) winningCols.Add(col);

                            // 计算禁止列（中奖列 + 下一列，最后中奖列的下下一列允许）
                            HashSet<int> forbiddenCols = new HashSet<int>();
                            foreach (int col in winningCols)
                            {
                                forbiddenCols.Add(col);
                                if (col + 1 < 5) forbiddenCols.Add(col + 1);
                            }

                            int lastWinningCol = winningCols[winningCols.Count - 1];
                            int allowedCol = lastWinningCol + 2;
                            if (allowedCol < 5) forbiddenCols.Remove(allowedCol);

                            if (forbiddenColumnsBySymbol.ContainsKey(targetSymbol))
                            {
                                foreach (var col in forbiddenCols)
                                {
                                    if (!forbiddenColumnsBySymbol[targetSymbol].Contains(col))
                                        forbiddenColumnsBySymbol[targetSymbol].Add(col);
                                }
                            }
                            else
                            {
                                forbiddenColumnsBySymbol[targetSymbol] = new HashSet<int>(forbiddenCols);
                            }

                            // 填充中奖位置（从第一列开始）
                            for (int col = 0; col < winCount; col++)
                            {
                                int row = linePattern[col];
                                if (gameResultList[row][col] != 0 && gameResultList[row][col] != targetSymbol)
                                    throw new InvalidOperationException($"线路冲突：({row},{col})");
                                gameResultList[row][col] = targetSymbol;
                                isWinningPosition[row, col] = true;
                            }
                        }
                    }

                    // 2. 填充非中奖位置（遵循规则）
                    System.Random random = new System.Random();
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 5; col++)
                        {
                            if (isWinningPosition[row, col]) continue;

                            int newSymbol;
                            bool foundValidSymbol = false;
                            int maxAttempts = 100; // 防止单个位置无限循环
                            int attempt = 0;

                            do
                            {
                                newSymbol = symbolNumber[random.Next(symbolNumber.Count)];
                                attempt++;
                                if (IsSymbolValidForNonWinningPosition(
                                        row, col, newSymbol, isWinningPosition, forbiddenColumnsBySymbol, gui))
                                {
                                    foundValidSymbol = true;
                                    break;
                                }
                            } while (attempt < maxAttempts);

                            if (!foundValidSymbol)
                            {
                                throw new InvalidOperationException($"无法为位置({row},{col})找到有效的符号，需要重新生成");
                            }

                            gameResultList[row][col] = newSymbol;
                        }
                    }

                    // 3. 确保特殊符号（10、11、12）最多各存在一个
                    EnsureSpecialSymbolLimit(random, symbolNumber, forbiddenColumnsBySymbol, gui);

                    // 4. 鬼牌（10）风险检查：若可能形成从第一列开始的有效连线，则替换鬼牌
                    ReplaceWildcardIfRisky(random, symbolNumber, allLines, gui);

                    // 5. 验证所有位置填充完成
                    bool isAllFilled = true;
                    foreach (var row in gameResultList)
                    {
                        if (row.Contains(0))
                        {
                            isAllFilled = false;
                            break;
                        }
                    }

                    if (isAllFilled)
                    {
                        string strDeckRowCol = SlotTool.GetDeckColRow(gameResultList);
                        return strDeckRowCol;
                    }
                    else
                    {
                        throw new InvalidOperationException("游戏数组未完全填充，生成失败");
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    // 达到最大重试次数仍失败，抛出异常
                    if (retryCount >= maxRetries)
                    {
                        throw new InvalidOperationException($"经过{maxRetries}次重试后仍无法生成有效的游戏数组", ex);
                    }
                    // 可以在这里添加日志记录，记录重试原因
                    // Console.WriteLine($"生成失败，正在进行第{retryCount}次重试: {ex.Message}");
                }
            }

            throw new InvalidOperationException("超出最大重试次数，无法生成游戏数组");
        }


#endif

    /// <summary>
    /// 检查鬼牌（gui）是否存在形成"从第一列开始的有效连线"的风险，若有则替换鬼牌
    /// </summary>
    private void ReplaceWildcardIfRisky(Random random, List<int> symbolNumber, List<int[]> allLines, int gui)
        {
            // 查找鬼牌位置
            (int row, int col) wildcardPos = (-1, -1);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    if (gameResultList[r][c] == gui)
                    {
                        wildcardPos = (r, c);
                        break;
                    }
                }

                if (wildcardPos.row != -1) break;
            }

            if (wildcardPos.row == -1) return; // 无鬼牌

            // 检查所有包含鬼牌位置的线路是否有风险（仅关注从第一列开始的连线）
            bool hasRisk = false;
            List<int[]> linesWithWildcard = new List<int[]>();
            foreach (var line in allLines)
            {
                // 检查线路是否包含鬼牌位置
                for (int c = 0; c < 5; c++)
                {
                    if (line[c] == wildcardPos.row && c == wildcardPos.col)
                    {
                        linesWithWildcard.Add(line);
                        // 检查是否存在"从第一列开始的潜在连线"风险
                        if (HasLineRiskWithWildcard(line, wildcardPos, gui))
                        {
                            hasRisk = true;
                        }

                        break;
                    }
                }
            }

            if (!hasRisk) return; // 无风险

            // 替换鬼牌（最多尝试10次）
            int maxAttempts = 10;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                attempts++;
                // 生成新符号（非10，符合特殊符号限制）
                int newSymbol;
                do
                {
                    newSymbol = symbolNumber[random.Next(symbolNumber.Count)];
                } while (newSymbol == gui || !IsValidReplacementSymbol(newSymbol, wildcardPos, gui)); // 传入 gui

                // 临时替换并检查风险
                int originalSymbol = gameResultList[wildcardPos.row][wildcardPos.col];
                gameResultList[wildcardPos.row][wildcardPos.col] = newSymbol;

                // 验证所有线路是否无"从第一列开始的有效连线"
                bool riskEliminated = true;
                foreach (var line in linesWithWildcard)
                {
                    if (HasLineRiskAfterReplacement(line))
                    {
                        riskEliminated = false;
                        break;
                    }
                }

                if (riskEliminated)
                {
                    return; // 替换成功
                }
                else
                {
                    gameResultList[wildcardPos.row][wildcardPos.col] = originalSymbol; // 恢复
                }
            }

            throw new InvalidOperationException($"无法替换鬼牌位置（{wildcardPos.row},{wildcardPos.col}）以消除从第一列开始的连线风险");
        }

        /// <summary>
        /// 检查包含鬼牌的线路是否存在风险：可能形成"从第一列开始的3连及以上连线"
        /// </summary>
        private bool HasLineRiskWithWildcard(int[] line, (int row, int col) wildcardPos, int gui)
        {
            // 线路符号序列（鬼牌为10）
            List<int> lineSymbols = new List<int>();
            for (int c = 0; c < 5; c++)
            {
                int r = line[c];
                lineSymbols.Add(gameResultList[r][c]);
            }

            // 有效连线必须从第一列（索引0）开始，因此第一列符号是基础
            int firstColSymbol = lineSymbols[0];
            if (firstColSymbol == gui)
            {
                // 第一列是鬼牌：可能变换为任意符号，需检查是否可能与后续符号形成连续
                return CheckFirstColIsWildcardRisk(lineSymbols, gui);
            }
            else
            {
                // 第一列是普通符号：检查鬼牌是否可能参与形成从第一列开始的连续序列
                return CheckFirstColIsNormalSymbolRisk(lineSymbols, firstColSymbol, wildcardPos.col);
            }
        }

        /// <summary>
        /// 第一列是鬼牌时的风险检查：鬼牌变换后是否可能与后续符号形成从第一列开始的连续序列
        /// </summary>
        private bool CheckFirstColIsWildcardRisk(List<int> lineSymbols, int gui)
        {
            // 第一列是鬼牌（10），假设变换为s，检查是否能与列1、列2形成3连
            if (lineSymbols.Count < 3) return false;

            // 列1和列2若为相同符号，鬼牌变换后可形成3连（0-1-2）
            if (lineSymbols[1] != gui && lineSymbols[2] != gui && lineSymbols[1] == lineSymbols[2])
            {
                return true;
            }

            // 列1是鬼牌，列2和列3相同：鬼牌变换后可形成0-1-2（变换为列2符号）
            if (lineSymbols[1] == gui && lineSymbols.Count >= 4 &&
                lineSymbols[2] != gui && lineSymbols[3] != gui && lineSymbols[2] == lineSymbols[3])
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 第一列是普通符号时的风险检查：鬼牌是否可能参与形成从第一列开始的连续序列
        /// </summary>
        private bool CheckFirstColIsNormalSymbolRisk(List<int> lineSymbols, int firstSymbol, int wildcardCol)
        {
            // 从第一列开始，连续相同符号的数量（包含鬼牌可能的变换）
            int consecutiveCount = 1; // 第一列已算1个

            for (int c = 1; c < lineSymbols.Count; c++)
            {
                if (c == wildcardCol)
                {
                    // 鬼牌位置：可变换为firstSymbol，因此计数+1
                    consecutiveCount++;
                }
                else if (lineSymbols[c] == firstSymbol)
                {
                    // 普通符号与第一列相同，计数+1
                    consecutiveCount++;
                }
                else
                {
                    // 符号不同，中断连续
                    break;
                }

                // 若连续计数达到3，存在风险
                if (consecutiveCount >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查替换鬼牌后，线路是否存在"从第一列开始的有效连线"（3连及以上）
        /// </summary>
        private bool HasLineRiskAfterReplacement(int[] line)
        {
            // 线路符号序列
            List<int> lineSymbols = new List<int>();
            for (int c = 0; c < 5; c++)
            {
                int r = line[c];
                lineSymbols.Add(gameResultList[r][c]);
            }

            // 有效连线必须从第一列开始，且连续3个及以上相同符号
            if (lineSymbols.Count < 3) return false;

            int firstSymbol = lineSymbols[0];
            int consecutiveCount = 1;

            for (int c = 1; c < lineSymbols.Count; c++)
            {
                if (lineSymbols[c] == firstSymbol)
                {
                    consecutiveCount++;
                    if (consecutiveCount >= 3)
                    {
                        return true; // 存在从第一列开始的有效连线
                    }
                }
                else
                {
                    break; // 中断连续，无需继续检查
                }
            }

            return false;
        }

        /// <summary>
        /// 验证替换鬼牌的新符号是否符合规则
        /// </summary>
        private bool IsValidReplacementSymbol(int newSymbol, (int row, int col) pos, int gui)
        {
            // 1. 特殊符号限制：自定义鬼牌（gui）、11、12 最多各1个
            List<int> specialSymbols = new List<int> { gui, 11, 12 };
            if (specialSymbols.Contains(newSymbol) && CountSpecialSymbolOccurrences(newSymbol, gui) >= 1)
            {
                return false;
            }

            // 2. 原有连续符号规则（不变）
            int row = pos.row;
            int col = pos.col;
            if (col >= 2)
            {
                int prevCol1 = col - 2;
                int prevCol2 = col - 1;
                int s1 = gameResultList[row][prevCol1];
                int s2 = gameResultList[row][prevCol2];
                if (s1 == s2 && newSymbol == s1)
                {
                    return false;
                }
            }

            return true;
        }

        // 以下方法与之前逻辑一致，适配新规则
        private void ValidateSpecialSymbolsInWinningLines(List<WinningLineInfo> winningLines, int gui)
        {
            // 特殊符号集合：自定义鬼牌（gui）+ 固定特殊符号11、12
            Dictionary<int, int> specialSymbolCounts = new Dictionary<int, int>
            {
                { gui, 0 }, // 自定义鬼牌
                { 11, 0 },
                { 12, 0 }
            };

            foreach (var lineInfo in winningLines)
            {
                int symbol = lineInfo.SymbolNumber;
                if (specialSymbolCounts.ContainsKey(symbol))
                {
                    specialSymbolCounts[symbol]++;
                    // 特殊符号不能在多个中奖线路中使用
                    if (specialSymbolCounts[symbol] > 1)
                        throw new InvalidOperationException($"特殊符号 {symbol}（自定义鬼牌）不能在多个中奖线路中使用");
                    // 特殊符号中奖数量只能为1
                    if (lineInfo.WinCount > 1)
                        throw new InvalidOperationException($"特殊符号 {symbol}（自定义鬼牌）的中奖数量不能超过1");
                }
            }
        }

        // 修复后（接入 gui 参数）
        private bool IsSymbolValidForNonWinningPosition(
            int row, int col, int symbol, bool[,] isWinningPosition,
            Dictionary<int, HashSet<int>> forbiddenColumnsBySymbol, int gui)
        {
            // 1. 特殊符号限制：自定义鬼牌（gui）、11、12 最多各1个
            if (IsSpecialSymbol(symbol, gui) && CountSpecialSymbolOccurrences(symbol, gui) >= 1)
                return false;

            // 2. 原有禁止列规则（不变）
            if (forbiddenColumnsBySymbol.ContainsKey(symbol) && forbiddenColumnsBySymbol[symbol].Contains(col))
                return false;

            // 3. 原有连续符号规则（不变）
            if (col >= 2)
            {
                int prevCol1 = col - 2;
                int prevCol2 = col - 1;
                if (!isWinningPosition[row, prevCol1] && !isWinningPosition[row, prevCol2])
                {
                    int s1 = gameResultList[row][prevCol1];
                    int s2 = gameResultList[row][prevCol2];
                    if (s1 == s2 && symbol == s1)
                        return false;
                }
            }

            return true;
        }

        private void EnsureSpecialSymbolLimit(
            Random random, List<int> symbolNumber,
            Dictionary<int, HashSet<int>> forbiddenColumnsBySymbol, int gui)
        {
            // 统计自定义鬼牌（gui）、11、12 的数量
            Dictionary<int, int> counts = new Dictionary<int, int> { { gui, 0 }, { 11, 0 }, { 12, 0 } };

            foreach (var row in gameResultList)
            foreach (var s in row)
                if (counts.ContainsKey(s))
                    counts[s]++;

            // 遍历特殊符号时，用 gui 替换 10
            foreach (var s in new List<int> { gui, 11, 12 })
            {
                while (counts[s] > 1)
                {
                    bool replaced = false;
                    for (int r = 0; r < 3 && !replaced; r++)
                    {
                        for (int c = 0; c < 5 && !replaced; c++)
                        {
                            if (gameResultList[r][c] == s)
                            {
                                int newSymbol;
                                do
                                {
                                    newSymbol = symbolNumber[random.Next(symbolNumber.Count)];
                                }
                                // 校验时传入 gui 参数
                                while (newSymbol == s || !IsSymbolValidForNonWinningPosition(r, c, newSymbol,
                                           new bool[3, 5], forbiddenColumnsBySymbol, gui));

                                gameResultList[r][c] = newSymbol;
                                counts[s]--;
                                replaced = true;
                            }
                        }
                    }

                    if (!replaced)
                        throw new InvalidOperationException($"无法调整特殊符号 {s}（自定义鬼牌）的数量");
                }
            }
        }

        private bool IsSpecialSymbol(int symbol, int gui) => symbol == gui || symbol == 11 || symbol == 12;

        private int CountSpecialSymbolOccurrences(int symbol, int gui)
        {
            int count = 0;
            foreach (var row in gameResultList)
            {
                // 若统计的是特殊符号（自定义鬼牌、11、12），直接计数；否则按原逻辑
                if (IsSpecialSymbol(symbol, gui))
                    count += row.FindAll(s => s == symbol).Count;
                else
                    count += row.FindAll(s => s == symbol).Count;
            }

            return count;
        }
    }

    [Serializable]
    public class WinningLineInfo
    {
        public int LineNumber; // 线路号（1到线路总数）
        public int SymbolNumber; // 中奖符号（符号池内）
        public int WinCount; // 中奖数量（3-5，特殊符号只能为1）
    }
