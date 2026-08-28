using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using SlotZhuZaiJinBi1700;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XingYunZhiLun_3998
{
    public enum SBoxGameState
    {
        GSNormal = 0,

        GSStart = 1,
        /// <summary> 普通局且不中线 </summary>
        GSEnd = 2,
        /// <summary> 赢线 </summary>
        GSWinline = 3,
        /// <summary> 免费游戏 </summary>
        GSFreeGame = 4,
        /// <summary> 送球 </summary>
        GSBonus = 5,
        /// <summary> 中了中小彩金 </summary>
        GSJpSmalm = 6,
        /// <summary> 中了大彩金 (弃用)</summary>
        GSJpMajor = 7,
        /// <summary> 中了巨大彩金 (弃用)</summary>
        GSJpGrand = 8,
        /// <summary> 礼盒游戏 </summary>
        GSLihe = 10,
        /// <summary> Wild游戏 </summary>
        GSWild = 11,
        /// <summary> 中奖倍率 </summary>
        GSMult = 12,

        GSOperater = 9
    }

    enum ResultType
    {
        RT_Lose,
        RT_Win,
        RT_FreeWin,
        RT_BonusWin,
        RT_Jackpot,
        RT_JackpotOnline,
    }

    enum OpenType
    {
        OT_Normal,
        OT_Give,
    }

    public class MachineDataG3998Controller : MonoSingleton<MachineDataG3998Controller>
    {

        private List<SymbolInclude> freeGameInclude = new List<SymbolInclude>();


        /// <summary>
        ///解析为本游戏 JSON与 <"ParseSlotSpin"/> 使用的字段一致。
        /// </summary>
        public static JSONNode ParseCoinPushSpinPayload(int[] data, int startPos)
        {
            JSONNode result = JSONNode.Parse("{}");
            if (data == null || startPos >= data.Length)
                return result;

            int pos = startPos;
            int openType = data[pos++];
            int resultType = data[pos++];
            int winlineNum = data[pos++];
            int totalBet = data[pos++];
            int matrixLength = data[pos++];
            result["OpenType"] = openType;
            result["ResultType"] = resultType;
            result["lineNum"] = winlineNum;
            result["TotalBet"] = totalBet;
            result["IDVec"] = new JSONArray();
            for (int i = 0; i < winlineNum; i++)
            {
                int id = data[pos++];
                result["IDVec"].Add(id);
            }

            result["Matrix"] = new JSONArray();
            for (int i = 0; i < matrixLength; i++)
            {
                int id = data[pos++];
                result["Matrix"].Add(id);
            }

            if(openType == (int)OpenType.OT_Give)
            {
                result["WildPosArrray"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["WildPosArrray"].Add(id);
                }
            }

            if (resultType == (int)ResultType.RT_FreeWin)
            {
                int totalFreeTime = data[pos++];
                int totalFreeBet = data[pos++];
                result["FreeBetArray"] = new JSONArray();
                for (int i = 0; i < totalFreeTime; i++)
                {
                    int id = data[pos++];
                    result["FreeBetArray"].Add(id);
                }
                result["TotalFreeTime"] = totalFreeTime;
                result["TotalFreeBet"] = totalFreeBet;
            }

            if (resultType == (int)ResultType.RT_BonusWin)
            {
                int bonusBet = data[pos++];
                int bonusType = data[pos++];
                if(bonusType == 0)
                {
                    int blindSymbol = data[pos++];
                }
                else if (bonusType == 1)
                {
                    int blindSymbol = data[pos++];
                    result["BlindSymbol"] = blindSymbol;
                }
                else 
                {
                    int bonusMultiply = data[pos++];
                    result["BonusMultiply"] = bonusMultiply;
                }

                result["BonusData"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
                }
                result["BonusBet"] = bonusBet;
                result["BonusType"] = bonusType;

                if (bonusType == 0 || bonusType == 1)
                {
                    result["BonusIDVec"] = new JSONArray();
                    int bonusIDVecSize= data[pos++];
                    for (int i = 0; i < bonusIDVecSize; i++)
                    {
                        int id = data[pos++];
                        result["BonusIDVec"].Add(id);
                    }

                    result["BonusIDVecSize"] = bonusIDVecSize;
                }
            }

            if(resultType == (int)ResultType.RT_Jackpot)
            {
                int jpCount = data[pos++];
                result["JPCount"] = jpCount;
                result["JPTypeArray"] = new JSONArray();
                for(int i = 0; i < 3; i++)
                {
                    if (data[pos] == 0)
                    {
                        pos++;
                        continue;
                    }
                    result["JPTypeArray"].Add(data[pos++]);
                }

                result["JPBetArray"] = new JSONArray();
                for(int i = 0; i < 3; i++)
                {
                    if (data[pos] == 0)
                    {
                        pos++;
                        continue;
                    }
                    result["JPBetArray"].Add(data[pos++]);
                }
                result["TotalJackpotBet"] = data[pos++];
            }

            return result;
        }

        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (++MainModel.Instance.gameNumber < 0) MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();
            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";
            ContentModel.Instance.isFreeSpinTrigger = false;
            ContentModel.Instance.isWild = false;
            ContentModel.Instance.isMult = false;
            ContentModel.Instance.isLihe = false;
            ContentModel.Instance.isDrawWins = false;
            ContentModel.Instance.isJackpotWin = false;
            ContentModel.Instance.bonusWinCredit = 0;
            ContentModel.Instance.drawWinsCredits = 0;
            ContentModel.Instance.jackpotWinCredit = 0;
            ContentModel.Instance.jackpotType = 0;
            ContentModel.Instance.bonusWinList = new List<SymbolWin>();

            int openType = (int)res["OpenType"];
            int resultType = (int)res["ResultType"];
            int lineNum = (int)res["lineNum"];
            int totalwin = (int)res["TotalBet"];
            int matrixLength = (int)res["MatrixLength"];
            int rows = CustomModel.Instance.row; // 3行
            int cols = CustomModel.Instance.column; // 5列
            int wheelChessNum = rows * cols;
            string strDeckRowCol = "";
            int totalLineWin = 0;
            int bonusWin = 0;
            int jackpotWin = 0;
            int maxLink = 0;
            int betMul = MainModel.Instance.contentMD.betmultiple;
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            List<SymbolWin> winList = new List<SymbolWin>();
            JackpotRes jpGameRes = new JackpotRes();

            if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            {
                ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                bool expectGiveSpin = ContentModel.Instance.freeSpinTotalTimes > 0 && ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes;
                if (expectGiveSpin && openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError(
                        $"[G3998] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
                    FreeSpinSessionStoreG3998.Clear(SBoxModel.Instance.pid);
                    FreeSpinSessionStoreG3998.ResetContentModelFreeStateToBaseGame();
                }
            }
            //免费游戏记录新出现的wild
            if (openType == 1)
            {
                foreach (int key in ContentModel.Instance.wildPos.Keys)
                {
                    ContentModel.Instance.wildPos[key].Clear();
                }
                ContentModel.Instance.tempRows.Clear();
            }

            //记录当前免费游戏之前的wild数据
            if (resultType == 2)
            {
                foreach (List<int> value in ContentModel.Instance.freeWildRecord.Values)
                {
                    value.Clear();
                }
            }

            //判断普通奖
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;

                    if (resultType == 2)
                    {
                        if (int.Parse(res["Matrix"][index].Value) == 9 || int.Parse(res["Matrix"][index].Value) == 8)
                        {
                            if (!ContentModel.Instance.freeWildRecord.ContainsKey(col))
                            {
                                ContentModel.Instance.freeWildRecord[col] = new List<int>();
                            }
                            ContentModel.Instance.freeWildRecord[col].Add(row);
                        }
                    }
                    else if (openType == 1)
                    {
                        if (int.Parse(res["Matrix"][index].Value) == 8)
                        {
                            bool haveNewWild = false;
                            if (!ContentModel.Instance.freeWildRecord.ContainsKey(col))
                            {
                                ContentModel.Instance.freeWildRecord[col] = new List<int>();
                                haveNewWild = true;
                            }
                            else if (!ContentModel.Instance.freeWildRecord[col].Contains(row))
                            {
                                haveNewWild = true;
                            }
                            ContentModel.Instance.freeWildRecord[col].Add(row);

                            if (haveNewWild)
                            {
                                if (!ContentModel.Instance.wildPos.ContainsKey(col))
                                {
                                    ContentModel.Instance.wildPos[col] = new List<int>();
                                }
                                ContentModel.Instance.wildPos[col].Add(row);
                                ContentModel.Instance.tempRows.Add(row);
                            }
                        }

                        if (ContentModel.Instance.freeWildRecord.ContainsKey(col) && ContentModel.Instance.freeWildRecord[col].Contains(row))
                        {
                            res["Matrix"][index].Value = 8.ToString();
                        }
                    }

                    strDeckRowCol += res["Matrix"][index].Value;

                    if (col < cols - 1)
                    {
                        strDeckRowCol += ","; // 列之间用逗号分隔
                    }
                }

                if (row < rows - 1)
                {
                    strDeckRowCol += "#"; // 行之间用#号分隔
                }
            }

            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            // 普通奖线（IDVec）；入账以算法 TotalBet 为准
            int normalMaxLink;
            winList = ParseIdVecToWinList(res["IDVec"], lineNum, betMul, out normalMaxLink);
            ContentModel.Instance.winList = winList;
            maxLink = normalMaxLink;
            totalLineWin = totalwin * betMul;
            int localLineSum = 0;
            foreach (var w in winList)
                localLineSum += (int)w.earnCredit;
            if (lineNum > 0 && localLineSum != totalLineWin)
            {
                DebugUtils.LogError(
                    $"[G3998] 普通线分不一致 TotalBet*mul={totalLineWin} local={localLineSum}");
            }
            ContentModel.Instance.baseGameWinCredit = totalLineWin;
            //检查算法结果
            CheckGameResult(strDeckRowCol, totalwin);

            List<int> deckRowCol = SlotTool.GetDeckRowCol(strDeckRowCol);
            int wild = CustomModel.Instance.symbolNumber[8];
            int scatter = CustomModel.Instance.symbolNumber[9];
            const int bonus = 11;

            //判断免费奖
            if (CustomModel.Instance.freeGameConfig.IsHasFreeGame && !CustomModel.Instance.freeGameConfig.IsScatterInLine)
            {
                int scatterCount = 0;
                bool isFree = false;
                int freeTime = 0;
                for (int i = 0; i < wheelChessNum; ++i)
                {
                    if (deckRowCol[i] == scatter)
                    {
                        scatterCount += 1;
                    }
                }

                ContentModel.Instance.scatterCount = scatterCount;

                for (int i = 0; i < CustomModel.Instance.freeGameConfig.Make2FreeGameCount.Length; ++i)
                {
                    if (scatterCount == CustomModel.Instance.freeGameConfig.Make2FreeGameCount[i])
                    {
                        isFree = true;
                        freeTime = CustomModel.Instance.freeGameConfig.FreeGameTime[i];

                    }
                }


                if (resultType == (int)ResultType.RT_FreeWin && isFree && (freeTime == (int)res["TotalFreeTime"]))
                {
                    int totalFreeTime = (int)res["TotalFreeTime"];
                    int totalFreeBet = (int)res["TotalFreeBet"];
                    ContentModel.Instance.curReelStripsIndex = "BS";
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                    ContentModel.Instance.isFreeSpinTrigger = true;
                    ContentModel.Instance.freeSpinTotalTimes = freeTime;
                    ContentModel.Instance.freeSpinPlayTimes = 0;
                    ContentModel.Instance.freeSpinTotalWinCredit = totalLineWin + totalFreeBet * betMul;
                    ContentModel.Instance.curFreeCredit = totalLineWin;

                    ContentModel.Instance.newFreeOnceCredit.Clear();
                    for (int i = 0; i < totalFreeTime; i++)
                    {
                        ContentModel.Instance.newFreeOnceCredit.Add((int)res["FreeBetArray"][i]);
                    }
                }
            }
            //判断赠送局
            if (openType == 1 && ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes)
            {
                if (openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError($"[G3998][CheckOpenType] 校验不一致，OpenType={(int)OpenType.OT_Give}");
                }

                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.freeSpinPlayTimes += 1;
                ContentModel.Instance.curFreeCredit += totalLineWin;
                ContentModel.Instance.baseGameWinCredit = totalLineWin;

                if (ContentModel.Instance.freeSpinTotalTimes == ContentModel.Instance.freeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
                ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" && ContentModel.Instance.nextReelStripsIndex == "BS";
            }
            //判断大奖（仅 ResultType=大奖）
            if (CustomModel.Instance.bonusGameconfig.IsHasBonusGame &&
                !CustomModel.Instance.bonusGameconfig.IsBonusInLine &&
                resultType == (int)ResultType.RT_BonusWin)
            {
                int scatterBonusCount = 0;
                for (int i = 0; i < wheelChessNum; ++i)
                {
                    if (deckRowCol[i] == scatter)
                        scatterBonusCount += 1;
                }
                ContentModel.Instance.scatterCount = scatterBonusCount;

                if (res.HasKey("BonusBet"))
                {
                    bonusWin = (int)res["BonusBet"] * betMul;
                    ContentModel.Instance.bonusWinCredit = bonusWin;
                }

                int bonusType = (int)res["BonusType"];
                switch (bonusType)
                {
                    case 0:
                    {
                        ContentModel.Instance.isWild = true;
                        ContentModel.Instance.cols.Clear();
                        for (int i = 0; i < scatterBonusCount - 2; i++)
                        {
                            if (ContentModel.Instance.cols.Contains(res["BonusData"][i])) continue;
                            ContentModel.Instance.cols.Add(res["BonusData"][i]);
                        }

                        int bonusIdCount = res.HasKey("BonusIDVecSize")
                            ? (int)res["BonusIDVecSize"]
                            : (res.HasKey("BonusIDVec") ? res["BonusIDVec"].Count : 0);
                        int bonusMaxLink;
                        ContentModel.Instance.bonusWinList =
                            ParseIdVecToWinList(res["BonusIDVec"], bonusIdCount, betMul, out bonusMaxLink);
                        maxLink = bonusMaxLink > 0 ? bonusMaxLink : maxLink;
                        ContentModel.Instance.maxLink = maxLink;
                        break;
                    }
                    case 1:
                    {
                        ContentModel.Instance.isLihe = true;
                        ContentModel.Instance.rewardIndex = (int)res["BlindSymbol"];
                        ContentModel.Instance.changeLiheIcon.Clear();
                        for (int row = 0; row < rows; row++)
                        {
                            for (int col = 0; col < cols; col++)
                            {
                                int index = row * cols + col;
                                ContentModel.Instance.changeLiheIcon.Add(int.Parse(res["BonusData"][index].Value));
                            }
                        }

                        int bonusIdCount = res.HasKey("BonusIDVecSize")
                            ? (int)res["BonusIDVecSize"]
                            : (res.HasKey("BonusIDVec") ? res["BonusIDVec"].Count : 0);
                        int bonusMaxLink;
                        ContentModel.Instance.bonusWinList =
                            ParseIdVecToWinList(res["BonusIDVec"], bonusIdCount, betMul, out bonusMaxLink);
                        break;
                    }
                    case 2:
                        ContentModel.Instance.isMult = true;
                        ContentModel.Instance.multiple = (int)res["BonusMultiply"];
                        break;
                    case 3:
                        ContentModel.Instance.drawWinsCredits = bonusWin;
                        ContentModel.Instance.isDrawWins = true;
                        break;
                }
            }
            //判断彩金
            if(resultType == (int)ResultType.RT_Jackpot)
            {
                ContentModel.Instance.isJackpotWin = true;
                ContentModel.Instance.jackpotWinCredit = res["TotalJackpotBet"];
                ContentModel.Instance.jackpotType = res["JPTypeArray"][0];

                jackpotWin = ContentModel.Instance.jackpotWinCredit;
            }

            ContentModel.Instance.baseGameWinCredit = totalLineWin + bonusWin + jackpotWin;

            bool isJackpotMajor = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 ? sboxJackpotData.Lottery[0] == 1 : false);
            bool isJackpotMinor = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 ? sboxJackpotData.Lottery[1] == 1 : false);
            bool isJackpotMini = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 2 ? sboxJackpotData.Lottery[2] == 1 : false);

            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 0 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMinior = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMini = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[2] : 0;
            ContentModel.Instance.jpGameRes = jpGameRes;

            if (isJackpotMajor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "major",
                    id = 1,
                    winCredit = sboxJackpotData.Jackpotlottery[0],
                    whenCredit = sboxJackpotData.JackpotOld[0],
                    curCredit = sboxJackpotData.JackpotOut[0],
                });
            }

            if (isJackpotMinor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "minor",
                    id = 1,
                    winCredit = sboxJackpotData.Jackpotlottery[1],
                    whenCredit = sboxJackpotData.JackpotOld[1],
                    curCredit = sboxJackpotData.JackpotOut[1],
                });
            }

            if (isJackpotMini)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "mini",
                    id = 1,
                    winCredit = sboxJackpotData.Jackpotlottery[2],
                    whenCredit = sboxJackpotData.JackpotOld[2],
                    curCredit = sboxJackpotData.JackpotOut[2],
                });
            }
            //计算赢分
            long creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (ContentModel.Instance.isSysCredit)
            {
                ContentModel.Instance.isSysCredit = false;
                creditBefore = ContentModel.Instance.realCredit;
            }
            long creditAfter = creditBefore - totalBet + totalLineWin + bonusWin + jackpotWin;
            if (ContentModel.Instance.gameState == GameState.FreeSpin) creditAfter += totalBet;

            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            ContentModel.Instance.realCredit = creditAfter;
            DebugUtils.Log($"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 当前押注倍率：{betMul} 押注后分数:  afterBetCredit = {creditAfter}  totalWin={totalLineWin} bonusWin={bonusWin} jackpotWin={jackpotWin}");

            // 记录游戏数据到数据库
            Record(totalBet, res);
            FreeSpinSessionStoreG3998.TryPersistOrClearSession();
        }

        /// <summary>
        /// 解析 IDVec / BonusIDVec：万千位线号，百位命中数，个十位图标。
        /// </summary>
        private List<SymbolWin> ParseIdVecToWinList(JSONNode idVec, int count, int betMul, out int maxHit)
        {
            maxHit = 0;
            var list = new List<SymbolWin>();
            if (idVec == null || count <= 0)
                return list;

            for (int i = 0; i < count; i++)
            {
                int ID = (int)idVec[i];
                int symbolNumber = ID % 100;
                int hitCount = (ID / 100) % 10;
                int lineNumber = ID / 1000;
                maxHit = maxHit >= hitCount ? maxHit : hitCount;

                int[] lineInfo = CustomModel.Instance.payLines[lineNumber].ToArray();
                var cells = new List<Cell>();
                for (int c = 0; c < hitCount; c++)
                    cells.Add(new Cell(c, lineInfo[c]));

                int lineWinCredit = GetLineOdds(symbolNumber, hitCount) * betMul;
                list.Add(new SymbolWin
                {
                    earnCredit = lineWinCredit,
                    multiplier = betMul,
                    lineNumber = lineNumber,
                    symbolNumber = symbolNumber,
                    cells = cells,
                });
            }
            return list;
        }

        private int GetLineOdds(int symbolType, int hitCount)
        {
            List<PayTableSymbolInfo> payTable = CustomModel.Instance.payTableSymbolWin;

            PayTableSymbolInfo info = null;
            if (symbolType >= 0 && symbolType < payTable.Count && payTable[symbolType].symbol == symbolType)
            {
                info = payTable[symbolType];
            }
            else
            {
                info = payTable.Find(x => x.symbol == symbolType);
            }

            if (info == null)
                return 0;

            switch (hitCount)
            {
                case 3: return info.x3;
                case 4: return info.x4;
                case 5: return info.x5;
                default: return 0;
            }
        }

        //检查算法结果
        private void CheckGameResult(string strDeckRowCol, int TotalWin)
        {
            List<List<int>> deckColRow = SlotTool.GetDeckColRow03(strDeckRowCol);
            int wild = CustomModel.Instance.symbolNumber[8];
            int scatter = CustomModel.Instance.symbolNumber[9];
            const int bonus = 11;
            int colCount = CustomModel.Instance.column;
            int calcTotalWin = 0; // 本地累计的总赢分（用于和服务器回包对比）
            List<List<int>> winLinesRule = CustomModel.Instance.payLines; // 中奖线
            List<PayTableSymbolInfo> payTable = CustomModel.Instance.payTableSymbolWin; // 赔率表

            if (deckColRow == null || deckColRow.Count == 0 || winLinesRule == null || payTable == null)
            {
                DebugUtils.LogError("[G3998][CheckGameResult] 数据为空，无法校验中奖结果。");
                return;
            }

            //判断中奖线,遍历每一条支付线
            for (int i = 0; i < MainModel.Instance.lineNum; ++i)
            {
                // 取当前线的行索引规则
                List<int> currentLineRule = winLinesRule[i];

                // 第 1 列在线上的行索引
                int firstRow = currentLineRule[0];
                // 线首个图标类型（作为中奖类型）
                int firstSymbolType = deckColRow[0][firstRow];
                // 从第 2 列开始累计“连续命中数量”（不含第 1 列）
                int sameTypeCount = 0;
                // 从第 2 列开始向右连续判断
                for (int n = 1; n < colCount; ++n)
                {
                    // 当前列在线上的行索引
                    int curRow = currentLineRule[n];
                    // 当前列该线位置的图标类型
                    int currentSymbolType = deckColRow[n][curRow];

                    // Wild 无法替代 Scatter 或 Bonus
                    if ((firstSymbolType == scatter || firstSymbolType == bonus) && currentSymbolType == firstSymbolType)
                    {
                        sameTypeCount += 1;
                    }
                    else if ((firstSymbolType != scatter && firstSymbolType != bonus) &&
                             (currentSymbolType == firstSymbolType || currentSymbolType == wild))
                    {
                        sameTypeCount += 1;
                    }
                    // 第一个图标是 Wild，遇到可替代图标后以该图标作为基准
                    else if ((currentSymbolType != scatter && currentSymbolType != bonus) && firstSymbolType == wild)
                    {
                        firstSymbolType = currentSymbolType; // 把当前普通图标设为新的基准图标
                        sameTypeCount += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                // 命中个数 = 连续计数 + 第 1 列自身
                int hitCount = sameTypeCount + 1;
                // 普通奖不统计 Scatter/Bonus
                if (firstSymbolType != scatter && firstSymbolType != bonus && hitCount >= 3)
                {
                    int lineOdds = GetLineOdds(firstSymbolType, hitCount);
                    if (lineOdds > 0)
                    {
                        calcTotalWin += lineOdds; // 累加本地计算总赢分
                    }
                }
            }

            int diff = Math.Abs(calcTotalWin - TotalWin); // 计算本地校验值与算法差值
            if (diff != 0)
            {
                DebugUtils.LogError($"[G3998][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}");
            }
            else
            {
                DebugUtils.Log($"[G3998][CheckGameResult] 校验通过，TotalWin={TotalWin}");
            }
        }

        /// <summary>
        /// 记录游戏数据到数据库
        /// </summary>
        private void Record(long totalBet, JSONNode res)
        {
            // 游戏场景记录
            GameSenceData gameSenceData = new GameSenceData();

            if (++MainModel.Instance.reportId < 0)
                MainModel.Instance.reportId = 1;

            gameSenceData.respone = ContentModel.Instance.response;
            gameSenceData.reportId = MainModel.Instance.reportId;
            gameSenceData.timeS = ContentModel.Instance.curGameCreatTimeMS / 1000;
            gameSenceData.gameNumber = MainModel.Instance.gameNumber;
            gameSenceData.gameNumberFreeSpinTrigger = ContentModel.Instance.isFreeSpin ? ContentModel.Instance.gameNumberFreeSpinTrigger : 0;
            gameSenceData.isFreeSpin = ContentModel.Instance.isFreeSpin;
            gameSenceData.freeSpinAddNum = ContentModel.Instance.freeSpinAddNum;

            gameSenceData.curStripsIndex = ContentModel.Instance.curReelStripsIndex;
            gameSenceData.nextStripsIndex = ContentModel.Instance.nextReelStripsIndex;
            gameSenceData.strDeckRowCol = ContentModel.Instance.strDeckRowCol;
            gameSenceData.deckRowCol = SlotTool.GetDeckRowCol(ContentModel.Instance.strDeckRowCol);

            gameSenceData.winFreeSpinTrigger = null;
            gameSenceData.winList = ContentModel.Instance.winList;
            //gameSenceData.freeSpinPlayTimes = ContentModel.Instance.freeSpinPlayTimes;
            //gameSenceData.freeSpinTotalTimes = ContentModel.Instance.freeSpinTotalTimes;
            //gameSenceData.freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            gameSenceData.totalBet = totalBet;

            // 获取游戏前后的分数
            long creditBefore = MainBlackboardController.Instance.myTempCredit + totalBet;
            long creditAfter = MainBlackboardController.Instance.myRealCredit;

            gameSenceData.creditBefore = creditBefore;
            gameSenceData.creditAfter = creditAfter;

            // 计算赢分
            long totalEarnCredit = 0;
            if (ContentModel.Instance.winList != null)
            {
                foreach (var win in ContentModel.Instance.winList)
                {
                    totalEarnCredit += win.earnCredit;
                }
            }
            gameSenceData.baseGameWinCredit = totalEarnCredit;

            // 彩金数据
            JackpotRes info = ContentModel.Instance.jpGameRes;

            gameSenceData.jpGrand = info.curJackpotGrand;
            gameSenceData.jpMajor = info.curJackpotMajor;
            gameSenceData.jpMinor = info.curJackpotMinior;
            gameSenceData.jpMini = info.curJackpotMini;

            // 确定游戏类型（先取 resultType，再按本局类型取分，禁止用上局 ContentModel 残留）
            int resultType = res != null ? (int)res["ResultType"] : 0;
            int openType = res != null ? (int)res["OpenType"] : 0;
            long betMul = Math.Max(1, ContentModel.Instance.betmultiple);

            long jackpotWinCredit = 0;
            string jackpotType = "";
            bool isJackpotResult = resultType == (int)ResultType.RT_Jackpot ||
                                  resultType == (int)ResultType.RT_JackpotOnline;
            // 本局机台 Lottery 实中：jpWinLst 有条目即可记彩金（不一定 ResultType=Jackpot）
            if (info.jpWinLst != null && info.jpWinLst.Count > 0)
            {
                JackpotWinInfo item = info.jpWinLst[0];
                gameSenceData.jpWinInfo = item;
                jackpotWinCredit = (long)item.winCredit;
                if (string.IsNullOrEmpty(jackpotType))
                {
                    if (item.name == "major") jackpotType = "0";
                    else if (item.name == "minor") jackpotType = "1";
                    else if (item.name == "mini") jackpotType = "2";
                }
            }
            else if (isJackpotResult)
            {
                if (res != null && res.HasKey("TotalJackpotBet"))
                    jackpotWinCredit = (long)(int)res["TotalJackpotBet"];
                else if (ContentModel.Instance.jackpotWinCredit > 0)
                    jackpotWinCredit = ContentModel.Instance.jackpotWinCredit;

                if (ContentModel.Instance.isJackpotWin)
                    jackpotType = ContentModel.Instance.jackpotType.ToString();
                if (string.IsNullOrEmpty(jackpotType) && res != null && res.HasKey("JPTypeArray") &&
                    res["JPTypeArray"].Count > 0)
                    jackpotType = res["JPTypeArray"][0].Value;
            }
            gameSenceData.jackpotWinCredit = jackpotWinCredit;

            // 大奖 / 免费：只认本局 ResultType + 回包字段
            long bonusGameWinCredit = 0;
            if (resultType == (int)ResultType.RT_BonusWin)
            {
                if (ContentModel.Instance.bonusWinCredit > 0)
                    bonusGameWinCredit = ContentModel.Instance.bonusWinCredit;
                else if (res != null && res.HasKey("BonusBet"))
                    bonusGameWinCredit = (long)(int)res["BonusBet"] * betMul;
                else if (ContentModel.Instance.drawWinsCredits > 0)
                    bonusGameWinCredit = ContentModel.Instance.drawWinsCredits;
            }

            long freeGameWinCredit = 0;
            if (resultType == (int)ResultType.RT_FreeWin && res != null && res.HasKey("TotalFreeBet"))
                freeGameWinCredit = (long)(int)res["TotalFreeBet"] * betMul;

            // ParseSlotSpin02 线分未乘 betmultiple：若与算法 TotalBet 相等则补乘，与钱包/大奖分对齐
            long baseGameWinCredit = totalEarnCredit;
            if (res != null && res.HasKey("TotalBet"))
            {
                int serverLineWin = (int)res["TotalBet"];
                if (serverLineWin > 0 && totalEarnCredit == serverLineWin)
                    baseGameWinCredit = totalEarnCredit * betMul;
            }

            long totalWinCredit = baseGameWinCredit + freeGameWinCredit + bonusGameWinCredit + jackpotWinCredit;

            // 构建记录对象
            TableSlotGameRecordItem slotGameRecordItem = new TableSlotGameRecordItem()
            {
                open_type = openType,
                result_type = resultType,
                jackpot_type = jackpotType,
                free_curtime = ContentModel.Instance.freeSpinPlayTimes,
                free_totaltime = ContentModel.Instance.freeSpinTotalTimes,
                game_id = 3998,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet = totalBet,
                credit_before = creditBefore,
                credit_after = creditAfter,
                base_game_win_credit = baseGameWinCredit,
                free_spin_win_credit = freeGameWinCredit,
                bonus_game_win_credit = bonusGameWinCredit,
                jackpot_win_credit = jackpotWinCredit,
                total_win_credit = totalWinCredit,
                strDeckRowCol = ContentModel.Instance.strDeckRowCol,
                symbol_icon_mapping = JsonConvert.SerializeObject(CustomModel.Instance.symbolIcon) // 
            };

            // 场景数据存入数据库
            slotGameRecordItem.scene = JsonConvert.SerializeObject(gameSenceData);


            // 插入数据
            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableSlotGameRecordItem>(
                ConsoleTableName.TABLE_SLOT_GAME_RECORD,
                slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //DebugUtils.Log($"[G1700] 游戏记录已写入数据库: gameType={gameType}, game_uid={ContentModel.Instance.curGameGuid}");
        }


        void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        void OnGMEvent(EventData res)
        {
            if (ApplicationSettings.Instance.isMock == false) return;

            if (res.id != 3998) return;

            switch (res.name)
            {
                case GlobalEvent.GMFreeSpin:
                    nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMBigWin:
                    nextSpin = SpinDataType.lihe;
                    break;
                case GlobalEvent.GMJp1:
                    nextSpin = SpinDataType.Jp1;
                    //GlobalJackpotConsole.NetClientManager.Instance.testIsHitJpGrandNext = true;
                    break;
                case GlobalEvent.GMJp2:
                    nextSpin = SpinDataType.Jp2;
                    //GlobalJackpotConsole.NetClientManager.Instance.testIsHitJpMajorNext = true;
                    break;
                case GlobalEvent.GMJp3:
                    nextSpin = SpinDataType.Jp3;
                    break;
                case GlobalEvent.GMJp4:
                    nextSpin = SpinDataType.Jp4;
                    break;
                case GlobalEvent.GMJpOnline:
                    //nextSpin = SpinDataType.JpOnline;

                    break;
                case GlobalEvent.GMBonus1:
                    nextSpin = SpinDataType.Wild;
                    break;
                case GlobalEvent.GMBonus2:
                    nextSpin = SpinDataType.Bonus;
                break;
                case GlobalEvent.GMMultipleWinLine:
                    nextSpin = SpinDataType.Multiple;
                    break;
            }

        }


        SpinDataType nextSpin = SpinDataType.Normal;


        enum SpinDataType
        {
            None,
            Normal,
            FreeSpin,
            BigWin,
            Jp1,
            Jp2,
            Jp3,
            Jp4,
            JpOnline,
            Bonus,
            lihe,
            Wild,
            Multiple
        };

        private Dictionary<SpinDataType, List<string[]>> spinDatas = new Dictionary<SpinDataType, List<string[]>>()
        {
            [SpinDataType.FreeSpin] = new List<string[]>()
            {
               new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_6.json",
                    "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_7.json",
                },
            },
            [SpinDataType.Normal] = new List<string[]>()
            {
                //new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__null_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__win_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__win_1.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__win_2.json" },
            },
            [SpinDataType.Jp1] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__jackpot_grand.json" },
            },
            [SpinDataType.Jp2] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__jackpot_major.json" },
            },
            [SpinDataType.Jp3] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__jackpot_minor.json" },
            },
            [SpinDataType.Jp4] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__jackpot_mini.json" },
            },
            [SpinDataType.Bonus] = new List<string[]>()
            {
                //new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_0.json" },
                //new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_1.json" },
                //new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_2.json" },
                //new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_3.json" }
                
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__jackpot.json" },
            },
            [SpinDataType.BigWin] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__Bigwin_0.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__Bigwin_1.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__Bigwin_2.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__Bigwin_3.json" },
            },
            [SpinDataType.lihe] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__lihe.json"},
            },
            [SpinDataType.Wild] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__Wild.json" }
            },
            [SpinDataType.Multiple] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3998_real/g200__slot_spin__multWin_11.json" }
            }
        };

        Queue<string> curDatas = new Queue<string>();


        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback, Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (curDatas.Count == 0)
                {
                    /*  随机数据
                    int dataIndex = UnityEngine.Random.Range(0, spinDatas.Count);
                    List<string[]> target = nextSpin != SpinDataType.None?
                        spinDatas[nextSpin] : spinDatas.ElementAt(dataIndex).Value;
                    nextSpin = SpinDataType.None;
                    */
                    List<string[]> target = null;
                    target = nextSpin != SpinDataType.None ? spinDatas[nextSpin] : spinDatas[SpinDataType.Normal];
                    nextSpin = SpinDataType.None;

                    string[] strs = target[UnityEngine.Random.Range(0, target.Count)];
                    curDatas = new Queue<string>(strs);  // 会改变引用数据  
                }

                string path = curDatas.Dequeue();
                int resourcesIndex = path.IndexOf("Resources/");
                string remainingPath = path.Substring(resourcesIndex + "Resources/".Length);
                remainingPath = remainingPath.Split('.')[0];

                try
                {
                    Debug.LogWarning($"<color=yellow>mock down</color>: 使用数据: {remainingPath}");
                    TextAsset jsn = Resources.Load<TextAsset>(remainingPath);
                    if (jsn != null && jsn.text != null)
                    {
                        JSONNode res = JSON.Parse(jsn.text);
                        successCallback?.Invoke(res);
                    }
                    else
                    {
                        BagelCodeError err = new BagelCodeError() { code = 404, msg = $"找不到数据: {path}" };
                        errorCallback?.Invoke(err);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"数据报错： {remainingPath}");
                }
            });
        }



        #region 可以对之前的转盘图标进行存储和其他操作，对 GenerateGameArray 进行了重载操作

        public List<List<int>> gameResultList = new List<List<int>>
        {
            new List<int>(new int[5]), // 第一行
            new List<int>(new int[5]), // 第二行
            new List<int>(new int[5]) // 第三行
        };

        public string strDeckRowCol;
        public string GenerateGameArray(List<List<int>> allLines, List<int> symbolNumber, List<WinningLineInfo> winningLines, int[] exclude, List<SymbolInclude> include, List<SymbolInclude> freeInclude, bool isFreeTrigger, int[] rows, int[] cols)
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

            foreach(SymbolInclude freeSymbolInclude in freeInclude)
            {
                gameResultList[freeSymbolInclude.rowIdx][freeSymbolInclude.colIdx] = freeSymbolInclude.symbolNumber;
            }

            int index = 0;
            foreach (SymbolInclude symbolInclude in include)
            {
                if (index < rows.Length)
                {
                    symbolInclude.colIdx = cols[index];
                    symbolInclude.rowIdx = rows[index];
                    index++;
                }

                int colIdx = symbolInclude.colIdx;
                int rowIdx = symbolInclude.rowIdx;

                if (gameResultList[rowIdx][colIdx] != -1) continue;

                if (colIdx == -1 && rowIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1);
                }
                else if (colIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                    } while (gameResultList[rowIdx][colIdx] != -1);
                }
                else if (rowIdx == -1)
                {
                    do
                    {
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1);
                }

                symbolInclude.colIdx = colIdx;
                symbolInclude.rowIdx = rowIdx;

                gameResultList[rowIdx][colIdx] = 20;

                if (!ContentModel.Instance.wildPos.ContainsKey(colIdx))
                {
                    ContentModel.Instance.wildPos[colIdx] = new List<int>();
                }
                ContentModel.Instance.wildPos[colIdx].Add(rowIdx);

                if (freeInclude.Count > 0)
                {
                    List<SymbolInclude> temp = new List<SymbolInclude>();
                    foreach (SymbolInclude freeSymbolInclude in freeInclude)
                    {
                        if (freeSymbolInclude.colIdx != symbolInclude.colIdx || freeSymbolInclude.rowIdx != rowIdx)
                        {
                            temp.Add(symbolInclude);
                        }
                    }
                    freeInclude.AddRange(temp);
                }
                else
                {
                    freeInclude.Add(symbolInclude);
                }
            }

            foreach (SymbolInclude freeSymbolInclude in freeInclude)
            {
                if (isFreeTrigger)
                {
                    gameResultList[freeSymbolInclude.rowIdx][freeSymbolInclude.colIdx] = 8;
                }
                else
                {
                    gameResultList[freeSymbolInclude.rowIdx][freeSymbolInclude.colIdx] = 9;
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

        public string WildGenerateGameArray(List<List<int>> allLines, List<int> symbolNumber, List<WinningLineInfo> winningLines, int[] exclude, List<SymbolInclude> include, int[] rows, int[] cols)
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
                    if (cols.Contains(cIndex))
                    {
                        continue;
                    }

                    int rIndex = line[cIndex];
                    gameResultList[rIndex][cIndex] = item.SymbolNumber;
                }
            }

            int index = 0;
            foreach (SymbolInclude symbolInclude in include)
            {
                if (index < rows.Length)
                {
                    symbolInclude.colIdx = cols[index];
                    symbolInclude.rowIdx = rows[index];
                    index++;
                }

                int colIdx = symbolInclude.colIdx;
                int rowIdx = symbolInclude.rowIdx;

                if (colIdx == -1 && rowIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1);
                }
                else if (colIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                    } while (gameResultList[rowIdx][colIdx] != -1);
                }
                else if (rowIdx == -1)
                {
                    do
                    {
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1);
                }

                symbolInclude.colIdx = colIdx;
                symbolInclude.rowIdx = rowIdx;

                gameResultList[rowIdx][colIdx] = 8;
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

        #endregion
    }

}