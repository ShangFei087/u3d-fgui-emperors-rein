using GameMaker;
using GameUtil;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
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

    public class MachineDataG3998Controller : MonoSingleton<MachineDataG3998Controller>
    {

        private List<SymbolInclude> freeGameInclude = new List<SymbolInclude>();

        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            SBoxGameState gameState = (SBoxGameState)((int)res["gameState"]);

            List<int> LineNumbers = new List<int>();
            int lineMark = (int)res["lineMark"];

            // lineMark /10000 / 100  // 3个线的数据

            int B = lineMark / 10000;
            int remainingAfterB = lineMark - B * 10000;
            int C = remainingAfterB / 100;
            int D = remainingAfterB - C * 100;

            if (B != 0)
                LineNumbers.Add(B);
            if (C != 0)
                LineNumbers.Add(C);
            if (D != 0)
                LineNumbers.Add(D);

            List<WinningLineInfo> winningLines = new List<WinningLineInfo>();

            for (int i = 0; i < res["lineData"].Count; i++)
            {
                JSONNode node = res["lineData"][i];

                int symbolNumber = node["icon"];
                int hitCount = node["link"];
                //int credit = node["reward"];
                int lineNumber = LineNumbers[i];

                winningLines.Add(new WinningLineInfo()
                {
                    LineNumber = lineNumber,
                    SymbolNumber = symbolNumber,
                    WinCount = hitCount,
                });
            }

            //bool isJackpotGrand = sboxJackpotData == null ? false : sboxJackpotData.Lottery[0] == 1;

            //bool isJackpotMajor = sboxJackpotData == null ? false : sboxJackpotData.Lottery[1] == 1;

            bool isJackpotGrand = gameState == SBoxGameState.GSJpGrand;

            bool isJackpotMajor = gameState == SBoxGameState.GSJpMajor;

            bool isJackpotMinMinor = gameState == SBoxGameState.GSJpSmalm;

            bool isBonusBall = gameState == SBoxGameState.GSBonus;

            bool isLihe = gameState == SBoxGameState.GSLihe;

            bool isWild = gameState == SBoxGameState.GSWild;

            bool isMult = gameState == SBoxGameState.GSMult;

            int freeSpinTotalTimes = (int)res["maxRound"];
            int freeSpinPlayTimes = (int)res["curRound"];



            bool isFreeSpinTrigger = freeSpinPlayTimes == 0 && freeSpinTotalTimes > 0;

            bool isFreeSpinResult = freeSpinTotalTimes > 0 && freeSpinPlayTimes == freeSpinTotalTimes;

            bool isFreeSpin = freeSpinPlayTimes > 0 && freeSpinTotalTimes > 0;

            string strDeckRowCol = "";



            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            
            JackpotRes jpGameRes = new JackpotRes();
            ContentModel.Instance.jpGameRes = jpGameRes;

            //jpGameRes.curJackpotGrand = sboxJackpotData.JackpotOut[0];
            //jpGameRes.curJackpotMajor = sboxJackpotData.JackpotOut[1];

            jpGameRes.curJackpotGrand = 3000;
            jpGameRes.curJackpotMajor = 2000;
            jpGameRes.curJackpotMinior = 1000;
            jpGameRes.curJackpotMini = 500;


            if (isJackpotGrand)
            {

                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    //name = "grand",
                    //id = 0,
                    //winCredit = sboxJackpotData.Jackpotlottery[0],
                    //whenCredit = sboxJackpotData.JackpotOld[0],
                    //curCredit = sboxJackpotData.JackpotOut[0],

                    name = "grand",
                    id = 0,
                    winCredit = 3000,
                    whenCredit = 3000,
                    curCredit = 3000,
                });

                symbolInclude = new List<SymbolInclude>();
                for (int i = 0; i < 5; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                   ContentModel.Instance.payLines,
                   CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 , 10 }, symbolInclude);
            }

            else if (isJackpotMajor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    //name = "major",
                    //id = 1,
                    //winCredit = sboxJackpotData.Jackpotlottery[1],
                    //whenCredit = sboxJackpotData.JackpotOld[1],
                    //curCredit = sboxJackpotData.JackpotOut[1],

                    name = "major",
                    id = 1,
                    winCredit = 2000,
                    whenCredit = 2000,
                    curCredit = 2000,
                });

                if (!isJackpotGrand)
                {
                    symbolInclude = new List<SymbolInclude>();
                    for (int i = 0; i < 4; i++)
                    {
                        symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                    }
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                   ContentModel.Instance.payLines,
                   CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 ,10}, symbolInclude);
            }
            else if (isJackpotMinMinor)
            {
                int winCredit = (int)res["num"];

                if (winCredit == 1000)
                {
                    jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                    {
                        name = "minor",
                        id = 3,
                        winCredit = 1000,
                        whenCredit = 1000,
                        curCredit = 1000,
                    });
                }
                else if (winCredit == 500)
                {
                    jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                    {
                        name = "mini",
                        id = 4,
                        winCredit = 500,
                        whenCredit = 500,
                        curCredit = 500,
                    });
                }


                if (!isJackpotGrand && !isJackpotMajor)
                {
                    symbolInclude = new List<SymbolInclude>();
                    for (int i = 0; i < 3; i++)
                    {
                        symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                    }
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] {8, 9, 10}, symbolInclude);

            }
            else if (isLihe)
            {
                for(int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                }

                int[] numRows = res["numRows"]?.ToString().Select(c => int.Parse(c.ToString())).ToArray();
                int[] numCols = res["numCols"]?.ToString().Select(c => int.Parse(c.ToString())).ToArray();

                ContentModel.Instance.winningLines = winningLines;
                ContentModel.Instance.rewardIndex = (int)res["rewardIcon"];

                strDeckRowCol = GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, null, new int[] { 8, 9, 10, (int)res["rewardIcon"] }, symbolInclude, freeGameInclude, true, numRows, numCols);
            }
            else if (isWild)
            {
                ContentModel.Instance.cols.Clear();
                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                }

                int[] numRows = res["numRows"]?.ToString().Select(c => int.Parse(c.ToString())).ToArray();
                int[] numCols = res["numCols"]?.ToString().Select(c => int.Parse(c.ToString())).ToArray();

                ContentModel.Instance.cols.AddRange(numCols);
                ContentModel.Instance.maxLink = (int)res["maxLink"];

                strDeckRowCol = WildGenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 ,10}, symbolInclude, numRows, numCols);
            }
            else if (isMult)
            {
                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                }
                //记录当前中奖的倍率
                ContentModel.Instance.multiple = (int)res["multiple"];

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 , 10 }, symbolInclude);
            }
            else if (isFreeSpinTrigger)
            {
                freeGameInclude.Clear();
                int[] numRows = res["numRows"]?.ToString().Select(c => int.Parse(c.ToString())).ToArray();
                int[] numCols = res["numCols"]?.ToString().Select(c => int.Parse(c.ToString())).ToArray();

                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                }

                strDeckRowCol = GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 , 10 }, symbolInclude, freeGameInclude, isFreeSpinTrigger, numRows, numCols);
            }
            else if (isFreeSpin)
            {
                int[] numRows = null;
                int[] numCols = null;
                foreach(int key in ContentModel.Instance.wildPos.Keys)
                {
                    ContentModel.Instance.wildPos[key].Clear();
                }

                if (res["numRows"] != null && res["numCols"] != null)
                {
                    numRows = res["numRows"].ToString().Select(c => int.Parse(c.ToString())).ToArray();
                    numCols = res["numCols"].ToString().Select(c => int.Parse(c.ToString())).ToArray();

                    ContentModel.Instance.tempRows.AddRange(numRows);

                }

                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 8 });
                }

                strDeckRowCol = GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 , 10 }, symbolInclude, freeGameInclude, isFreeSpinTrigger, numRows, numCols);
            }
            else
            {
                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 8, 9 , 10 }, symbolInclude);
            }

            long creditBefore = MainBlackboardController.Instance.myRealCredit;

            long afterBetCredit = 0;
            long totalEarnCredit = 0;

            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;

            ContentModel.Instance.response = res.ToString();

            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            List<SymbolWin> winList = new List<SymbolWin>();
            for (int i = 0; i < LineNumbers.Count; i++)
            {
                int lineNumber = LineNumbers[i];
                int lineIndex = lineNumber - 1;

                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();

                JSONNode lineNode = res["lineData"][i];
                int credit = lineNode["reward"];
                int hitCount = lineNode["link"];
                int symbolNumber = lineNode["icon"];

                List<Cell> _cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                SymbolWin sw = new SymbolWin()
                {
                    earnCredit = credit,
                    multiplier = 1,
                    lineNumber = lineNumber,
                    symbolNumber = symbolNumber,
                    cells = _cells,
                };
                winList.Add(sw);

                totalEarnCredit += credit;
            }

            ContentModel.Instance.winList = winList;



            if (isFreeSpin && freeSpinTotalTimes != 0)
            {
                ContentModel.Instance.freeSpinAddNum =
                    freeSpinTotalTimes - ContentModel.Instance.freeSpinTotalTimes;
            }
            else
            ContentModel.Instance.freeSpinAddNum = 0;


            if (isFreeSpin)
            {
                ContentModel.Instance.showFreeSpinRemainTime = ContentModel.Instance.freeSpinTotalTimes -
                                                               ContentModel.Instance.freeSpinPlayTimes - 1;
            }
            else
            {
                ContentModel.Instance.showFreeSpinRemainTime = 0;
            }

            ContentModel.Instance.isLihe = isLihe;
            ContentModel.Instance.isWild = isWild;
            ContentModel.Instance.isMult = isMult;


            ContentModel.Instance.freeSpinTotalTimes = freeSpinTotalTimes;
            ContentModel.Instance.freeSpinPlayTimes = freeSpinPlayTimes;


            ContentModel.Instance.isFreeSpinTrigger = isFreeSpinTrigger;
            ContentModel.Instance.isFreeSpinResult = isFreeSpinResult;

            if (isFreeSpinTrigger)
            {
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";
            }
            else if (isFreeSpinResult)
            {
                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.nextReelStripsIndex = "BS";
                freeGameInclude = new List<SymbolInclude>();
                ContentModel.Instance.tempRows.Clear();
            }
            else if (isFreeSpin)
            {
                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.nextReelStripsIndex = "FS";
            }
            else
            {
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "BS";
            }

            ContentModel.Instance.totalEarnCredit = totalEarnCredit;
            ContentModel.Instance.baseGameWinCredit = totalEarnCredit;


            //ContentModel.Instance.winFreeSpinTriggerOrAddCopy = null;


            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            ContentModel.Instance.curGameGuid = isFreeSpin
                ? $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}-{ContentModel.Instance.gameNumberFreeSpinTrigger}"
                : $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}";


            if (!isFreeSpin)
            {
                afterBetCredit = creditBefore - totalBet;
            }
            else
            {
                afterBetCredit = creditBefore;
            }
            // ## MainBlackboardController.Instance.SetMyTempCredit(afterBetCredit, false); // 不同步给ui



            long creditAfter = afterBetCredit + totalEarnCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }
            // ## MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            Debug.LogWarning(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            Debug.LogWarning($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            if (!isFreeSpin)
            {
                ContentModel.Instance.freeSpinTotalWinCredit = 0;
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCredit += totalEarnCredit;
                freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            }


            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);
            bool isReelsSlowMotion = ((int)res["num"] > 2) ? true : false;
            //bool isReelsSlowMotion = false;
            ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;


            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();
            /*
            if (res["contents"]["bonus_result"] != null && res["contents"]["bonus_result"].Count > 0)
            {
               foreach (JSONNode item in res["contents"]["bonus_result"])
               {
                   bonusResult.Add((int)item["bonus_id"],item);
               }
            }*/
            ContentModel.Instance.bonusResult = bonusResult;


            /*
            if (ContentModel.Instance.bonusResult.Count >0 )
            {
               ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Expectation02;
            }
            else
            {
               ContentModel.Instance.targetSlotGameEffect = isReelsSlowMotion ? SlotGameEffect.Expectation01 :
                   isFreeSpin ? SlotGameEffect.FreeSpin : SlotGameEffect.Default;
            }
            */
            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;
            SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);


        }

        private Dictionary<int, List<int>> freeWildRecord = new Dictionary<int, List<int>>();

        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            //IDVec
            int lineNum = (int)res["lineNum"];
            int totalEarnCredit = 0;
            int credit = (int)res["TotalBet"];
            List<SymbolWin> winList = new List<SymbolWin>();

            int maxLink = 0;

            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100;          // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10;       // 百位：消除数量（WinCount）
                int lineNumber = ID / 1000;           // 万千位：线编号
                

                // 输出调试信息（可选）
                Debug.Log($"ID: {ID}, Line: {lineNumber}, HitCount: {hitCount}, Symbol: {symbolNumber}");

                int lineIndex = lineNumber;
                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> _cells = new List<Cell>();

                maxLink = maxLink >= hitCount ? maxLink : hitCount;

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }


                SymbolWin sw = new SymbolWin()
                {
                    earnCredit = credit / lineNum,
                    multiplier = 1,
                    lineNumber = lineNumber,
                    symbolNumber = symbolNumber,
                    cells = _cells,
                };
                winList.Add(sw);

                totalEarnCredit += credit;
            }
            ContentModel.Instance.winList = winList;

            //Matrix
            int rows = 3;    // 3行
            int cols = 5;    // 5列
            string strDeckRowCol = "";
            int MatrixLength = (int)res["MatrixLength"];
            int wheelNum = 0;

            //判断免费奖或大奖
            int ResultType = (int)res["ResultType"];
            int OpenType = (int)res["OpenType"];
            int TotalFreeTime = (int)res["TotalFreeTime"];

            if(OpenType == 1)
            {
                //免费游戏记录新出现的wild
                foreach (int key in ContentModel.Instance.wildPos.Keys)
                {
                    ContentModel.Instance.wildPos[key].Clear();
                }
                ContentModel.Instance.tempRows.Clear();
            }

            if(ResultType == 2)
            {
                //记录当前免费游戏之前的wild数据
                foreach (List<int> value in freeWildRecord.Values)
                {
                    value.Clear();
                }
            }
            

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;
                    
                    if(int.Parse(res["Matrix"][index].Value) == 9)
                    {
                        wheelNum ++;
                    }
                    
                    if(ResultType == 2)
                    {
                        if(int.Parse(res["Matrix"][index].Value) == 9 || int.Parse(res["Matrix"][index].Value) == 8)
                        {
                            if (!freeWildRecord.ContainsKey(col))
                            {
                                freeWildRecord[col] = new List<int>();
                            }
                            freeWildRecord[col].Add(row);
                        }
                    }
                    else if ((int)res["OpenType"] == 1)
                    {
                        if (int.Parse(res["Matrix"][index].Value) == 8)
                        {
                            bool haveNewWild = false;
                            if (!freeWildRecord.ContainsKey(col))
                            {
                                freeWildRecord[col] = new List<int>();
                                haveNewWild = true;
                            }
                            else if (!freeWildRecord[col].Contains(row))
                            {
                                haveNewWild = true;
                            }
                            freeWildRecord[col].Add(row);

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

                        if(freeWildRecord.ContainsKey(col) && freeWildRecord[col].Contains(row))
                        {
                            res["Matrix"][index].Value = 8.ToString();
                        }
                    }

                    strDeckRowCol += res["Matrix"][index].Value;
                    if (col < cols - 1)
                    {
                        strDeckRowCol += ",";  // 列之间用逗号分隔
                    }
                }

                if (row < rows - 1)
                {
                    strDeckRowCol += "#";  // 行之间用#号分隔
                }
            }
            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            ContentModel.Instance.jackpotWinCredit = 0;

            //判断彩金
            JackpotRes jpGameRes = new JackpotRes();
            bool isJackpotMajor = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 ? sboxJackpotData.Lottery[0] == 1 : false);
            bool isJackpotMinor = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 ? sboxJackpotData.Lottery[1] == 1 : false);
            bool isJackpotMini = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 2 ? sboxJackpotData.Lottery[2] == 1 : false);

            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 0 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMinior = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMini = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[2] : 0;
            //Debug.Log("curJackpotMajor:" + jpGameRes.curJackpotMajor);
            //Debug.Log("curJackpotMinior:" + jpGameRes.curJackpotMinior);
            //Debug.Log("curJackpotMini:" + jpGameRes.curJackpotMini);
            ContentModel.Instance.jpGameRes = jpGameRes;

            if (isJackpotMajor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "major",
                    id = 1,
                    winCredit = sboxJackpotData.Jackpotlottery[1],
                    whenCredit = sboxJackpotData.JackpotOld[1],
                    curCredit = sboxJackpotData.JackpotOut[1],
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


            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();

            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";

            
            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            ContentModel.Instance.isWild = false;
            ContentModel.Instance.isMult = false;
            ContentModel.Instance.isLihe = false;

            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.freeSpinTotalTimes = wheelNum + 1;
                ContentModel.Instance.freeSpinPlayTimes = 0;

                ContentModel.Instance.newFreeOnceCredit.Clear();
                for (int i = 0; i < TotalFreeTime; i++)
                {
                    ContentModel.Instance.newFreeOnceCredit.Add((int)res["FreeBetArray"][i]);
                }
            }
            else if(ResultType == 3 && (int)res["BonusType"] == 0)
            {
                ContentModel.Instance.isWild = true;
                ContentModel.Instance.maxLink = maxLink;
                ContentModel.Instance.cols.Clear();

                for(int i = 0; i <= wheelNum - 3; i++)
                {
                    ContentModel.Instance.cols.Add(res["BonusData"][i]);
                }
            }
            else if(ResultType == 3 && (int)res["BonusType"] == 1)
            {
                ContentModel.Instance.isLihe = true;
                ContentModel.Instance.rewardIndex = (int)res["BlindSymbol"];

                //将BonusData转化到列表中确认转化的图标位置
                ContentModel.Instance.changeLiheIcon.Clear();
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int index = row * cols + col;
                        ContentModel.Instance.changeLiheIcon.Add(int.Parse(res["BonusData"][index].Value));
                    }
                }
                ContentModel.Instance.rewardIndex = (int)res["BlindSymbol"];
            }
            else if(ResultType == 3 && (int)res["BonusType"] == 2)
            {
                ContentModel.Instance.isMult = true;
                ContentModel.Instance.multiple = (int)res["BlindSymbol"];
            }
            else if(ResultType == 3 && (int)res["BonusType"] == 3)
            {
                ContentModel.Instance.jackpotWinCredit = (int)res["BonusBet"];

                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "mini",
                    id = 4,
                    winCredit = 500,
                    whenCredit = 500,
                    curCredit = 500,
                });


            }

            //赠送局
            if (OpenType == 1)
            {
                Debug.Log("-------赠送局--------");
                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.freeSpinPlayTimes += 1;
                if (ContentModel.Instance.freeSpinTotalTimes == ContentModel.Instance.freeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
            }




            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long creditBefore = MainBlackboardController.Instance.myTempCredit;
            //赢分
            long TotalBet = (int)res["TotalBet"];
            if (ResultType == 3) TotalBet += (int)res["BonusBet"];
            DebugUtils.Log("本局赢分TotalBet==" + TotalBet);
            long afterBetCredit = 0;
            if (OpenType == 1)
            {
                afterBetCredit = creditBefore + TotalBet;
            }
            else
            {
                afterBetCredit = creditBefore + TotalBet;

            }

            long creditAfter = afterBetCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log($"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            //if (OpenType == 1)
            //{
            //    ContentModel.Instance.freeSpinTotalWinCredit = 0;
            //}
            //else
            //{
            //    ContentModel.Instance.freeSpinTotalWinCredit += totalEarnCredit;
            //    freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            //}

            if (ResultType == 2)
            {
                ContentModel.Instance.freeSpinTotalWinCredit = (int)res["TotalFreeBet"];
            }


            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);
            //bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;
            bool isReelsSlowMotion = true;
            ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;


            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();
            /*
            if (res["contents"]["bonus_result"] != null && res["contents"]["bonus_result"].Count > 0)
            {
               foreach (JSONNode item in res["contents"]["bonus_result"])
               {
                   bonusResult.Add((int)item["bonus_id"],item);
               }
            }*/
            ContentModel.Instance.bonusResult = bonusResult;


            /*
            if (ContentModel.Instance.bonusResult.Count >0 )
            {
               ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Expectation02;
            }
            else
            {
               ContentModel.Instance.targetSlotGameEffect = isReelsSlowMotion ? SlotGameEffect.Expectation01 :
                   isFreeSpin ? SlotGameEffect.FreeSpin : SlotGameEffect.Default;
            }
            */
            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;
            SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);
        }



        /*
        public void Record()
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

            gameSenceData.winFreeSpinTrigger = ContentModel.Instance.winFreeSpinTriggerOrAddCopy;
            gameSenceData.winList = ContentModel.Instance.winList;
            gameSenceData.freeSpinPlayTimes = ContentModel.Instance.freeSpinPlayTimes;
            gameSenceData.freeSpinTotalTimes = ContentModel.Instance.freeSpinTotalTimes;
            gameSenceData.freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            gameSenceData.totalBet = ContentModel.Instance.totalBet;
            gameSenceData.creditBefore = ContentModel.Instance.creditBefore;
            gameSenceData.creditAfter = ContentModel.Instance.creditAfter; // 这是基础游戏+彩金【外设彩金-需要修改】
            gameSenceData.jackpotWinCredit = 0;  //【外设彩金-需要修改】
            gameSenceData.baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;


            TableSlotGameRecordItem slotGameRecordItem = new TableSlotGameRecordItem()
            {
                game_type = ContentModel.Instance.isFreeSpin ? "free_spin" : ContentModel.Instance.isFreeSpinTrigger ? "free_spin_trigger" : "spin",
                game_id = ConfigUtils.curGameId,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet = ContentModel.Instance.totalBet,
                credit_before = gameSenceData.creditBefore,
            };

            // 本剧数据存入数据库
            slotGameRecordItem.credit_after = gameSenceData.creditAfter;  //【外设彩金-需要修改】
            slotGameRecordItem.base_game_win_credit = gameSenceData.baseGameWinCredit;



            // 彩金数据
            JackpotRes info = ContentModel.Instance.jackpotRes;


            // 数据修改：
            gameSenceData.jpGrand = info.curJackpotGrand;
            gameSenceData.jpMajor = info.curJackpotMajor;
            gameSenceData.jpMinor = info.curJackpotMinior;
            gameSenceData.jpMini = info.curJackpotMini;

            if (info.jpWinLst != null && info.jpWinLst.Count > 0)
            {
                JackpotWinInfo item = info.jpWinLst[0];

                gameSenceData.jpWinInfo = item;


                int winJPCredit = (int)item.winCredit;

                slotGameRecordItem.jackpot_win_credit = winJPCredit;
                gameSenceData.jackpotWinCredit = winJPCredit;


                long creditBefore = ContentModel.Instance.creditAfter;
                long creditAfter = ContentModel.Instance.creditAfter += winJPCredit;

                ContentModel.Instance.creditAfter = creditAfter;
                gameSenceData.creditAfter = creditAfter;
                slotGameRecordItem.credit_after = creditAfter;


                // 通知算法卡
                MachineDataManager.Instance.NotifyGameJackpot(winJPCredit);
                SBoxModel.Instance.myCredit += winJPCredit;


                // 游戏彩金记录
                TableJackpotRecordAsyncManager.Instance.AddJackpotRecord(item.id, item.name, winJPCredit,
                creditBefore, creditAfter,
                ContentModel.Instance.curGameGuid, ContentModel.Instance.curGameCreatTimeMS);


                // 额外奖上报
                DeviceBonusReport.Instance.ReportBonus(item.name, item.name, winJPCredit, -1, (msg) => { }, (err) => { });

            }

            // 每日营收统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalBetWin(
                ContentModel.Instance.curReelStripsIndex == "FS" ? 0 : ContentModel.Instance.totalBet,
             ContentModel.Instance.baseGameWinCredit + gameSenceData.jackpotWinCredit, SBoxModel.Instance.myCredit);


            ContentModel.Instance.totalEarnCredit = ContentModel.Instance.baseGameWinCredit + gameSenceData.jackpotWinCredit;


            slotGameRecordItem.scene = JsonConvert.SerializeObject(gameSenceData);
            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD, slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);



            //try
            //{
            //    // 数据数据上报
            //    string str = ReportDataUtils.CreatReportData(gameSenceData, SBoxModel.Instance.sboxPlayerInfo);
            //    DebugUtils.Log($"数据上报成功 {str}");
            //    ReportManager.Instance.SendData(str, null, null);
            //}
            //catch (Exception ex) { }


        }
        */

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
                    //"Assets/HotFix/Games/Mock/Resources/g3998_real/g200__slot_spin__free_6.json",
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