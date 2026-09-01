using CaiFuHuoChe_3996;
using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XingYunZhiLun_3998;

namespace HuoYanGongNiu_3995
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
        /// <summary> 彩金游戏 </summary>
        GSJpGame = 5,

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

    public class MachineDataG3995Controller : MonoSingleton<MachineDataG3995Controller>
    {
        public List<SymbolInclude> jackpotSymbolInclude = new List<SymbolInclude>();
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

            result["WildData"] = new JSONArray();
            for(int i = 0; i < matrixLength; i++)
            {
                int id = data[pos++];
                result["WildData"].Add(id);
            }

            if(openType == (int)OpenType.OT_Give)
            {
                result["GoldBullCount"] = data[pos++];
            }

            if(resultType == (int)ResultType.RT_FreeWin)
            {
                result["TotalFreeTime"] = data[pos++];
                result["TotalFreeBet"] = data[pos++];

                int wheelLength = data[pos++];
                result["WheelTimes"] = wheelLength;
                Debug.LogError(wheelLength);
                result["WheelData"] = new JSONArray();
                for(int i = 0; i < wheelLength; i++)
                {
                    int id = data[pos++];
                    result["WheelData"].Add(id);
                }
            }

            if(resultType == (int)ResultType.RT_BonusWin || resultType == (int)ResultType.RT_Jackpot)
            {
                result["BonusBet"] = data[pos++];

                result["BonusData"] = new JSONArray();
                for(int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
                }

                if(resultType == (int)ResultType.RT_Jackpot)
                {
                    int length = data[pos++];
                    result["JPCount"] = length;

                    result["JPTypeArray"] = new JSONArray();
                    for(int i = 0; i < length; i++)
                    {
                        int id = data[pos++];
                        result["JPTypeArray"].Add(id);
                    }

                    result["JPBetArray"] = new JSONArray();
                    for (int i = 0; i < length; i++)
                    {
                        int id = data[pos++];
                        result["JPBetArray"].Add(id);
                    }
                }
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
            ContentModel.Instance.isJackpotSpinTrigger = false;

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
            int lineWin = 0;
            int betMul = MainModel.Instance.contentMD.betmultiple;
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            List<SymbolWin> winList = new List<SymbolWin>();
            JackpotRes jpGameRes = new JackpotRes();
            bool isFreeSpin = openType == (int)OpenType.OT_Give;
            if (isFreeSpin)
            {
                ContentModel.Instance.SpecialBullIcon.Clear();
            }

            if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            {
                ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                bool expectGiveSpin = ContentModel.Instance.freeSpinTotalTimes > 0 && ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes;
                if (expectGiveSpin && openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError(
                        $"[G3995] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
                    //FreeSpinSessionStoreG3995.Clear(SBoxModel.Instance.pid);
                    //FreeSpinSessionStoreG3995.ResetContentModelFreeStateToBaseGame();
                }
            }

            //判断普通奖
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;

                    //判断wild图标对应的是x2还是x3
                    if(int.Parse(res["Matrix"][index].Value) != 11)
                    {
                        //免费游戏金牛数量一定时替换图标
                        if (isFreeSpin)
                        {
                            if(ContentModel.Instance.stageIndex > 0)
                            {
                                for(int i = 1; i < ContentModel.Instance.stageIndex + 1; i++)
                                {
                                    if(int.Parse(res["Matrix"][index].Value) == 10 - i)
                                    {
                                        res["Matrix"][index].Value = "10";
                                    }
                                }
                            }
                        }

                        strDeckRowCol += res["Matrix"][index].Value;
                    }
                    else
                    {
                        int target = int.Parse(res["Matrix"][index].Value) * res["WildData"][index];
                        strDeckRowCol += target.ToString();
                    }
                    if (col < cols - 1)
                    {
                        strDeckRowCol += ","; // 列之间用逗号分隔
                    }

                    //免费游戏中判断金牛的位置
                    if (isFreeSpin && int.Parse(res["Matrix"][index].Value) == 14)
                    {
                        ContentModel.Instance.SpecialBullIcon.Add(new Cell()
                        {
                            column = col,
                            row = row,
                        });
                    }
                }

                if (row < rows - 1)
                {
                    strDeckRowCol += "#"; // 行之间用#号分隔
                }
            }
            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            //IDVec 
            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100; // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10; // 百位：消除数量（WinCount）
                int lineNumber = ID / 1000; // 万千位：线编号
                // 输出调试信息（可选）
                //Debug.Log($"ID: {ID}, Line: {lineNumber}, HitCount: {hitCount}, Symbol: {symbolNumber}");

                int lineIndex = lineNumber;
                int[] lineInfo = CustomModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> _cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                lineWin = GetLineOdds(symbolNumber, hitCount) * MainModel.Instance.contentMD.betmultiple;
                SymbolWin sw = new SymbolWin()
                {
                    earnCredit = lineWin,
                    multiplier = MainModel.Instance.contentMD.betmultiple,
                    lineNumber = lineNumber,
                    symbolNumber = symbolNumber,
                    cells = _cells,
                };
                winList.Add(sw);

                totalLineWin += lineWin;
            }
            ContentModel.Instance.winList = winList;
            totalLineWin = totalwin;
            ContentModel.Instance.baseGameWinCredit = totalLineWin;

            //检查算法结果
            CheckGameResult(strDeckRowCol, totalwin);

            //判断彩金
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


            List<int> deckRowCol = SlotTool.GetDeckRowCol(strDeckRowCol);
            int scatter = CustomModel.Instance.symbolNumber[12];
            int bonus = CustomModel.Instance.symbolNumber[13];

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
                for (int i = 0; i < CustomModel.Instance.freeGameConfig.Make2FreeGameCount.Length; ++i)
                {
                    if (scatterCount == CustomModel.Instance.freeGameConfig.Make2FreeGameCount[i])
                    {
                        isFree = true;
                        freeTime = CustomModel.Instance.freeGameConfig.FreeGameTime[i];
                    }
                }


                if (resultType == (int)ResultType.RT_FreeWin && isFree)        //&& (freeTime == (int)res["TotalFreeTime"])
                {
                    int TotalFreeTime = (int)res["TotalFreeTime"];
                    ContentModel.Instance.curReelStripsIndex = "BS";
                    ContentModel.Instance.nextReelStripsIndex = "FS";

                    ContentModel.Instance.isFreeSpinTrigger = true;
                    ContentModel.Instance.freeSpinTotalTimes = freeTime;
                    ContentModel.Instance.freeSpinPlayTimes = 0;

                    ContentModel.Instance.freeSpinTotalWinCredit = (int)res["TotalFreeBet"] * MainModel.Instance.contentMD.betmultiple;

                    ContentModel.Instance.wheelSpinTimes = (int)res["WheelTimes"];
                    ContentModel.Instance.wheelData.Clear();

                    for(int i = 0; i < ContentModel.Instance.wheelSpinTimes; i++)
                    {
                        ContentModel.Instance.wheelData.Add((int)res["WheelData"][i]);
                    }

                    ContentModel.Instance.newFreeOnceCredit.Clear();
                    for (int i = 0; i < TotalFreeTime; i++)
                    {
                        ContentModel.Instance.newFreeOnceCredit.Add((int)res["FreeBetArray"][i] * MainModel.Instance.contentMD.betmultiple);
                    }
                }
            }

            //判断赠送局
            if (openType == 1 && ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes)
            {
                if (openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError($"[G3995][CheckOpenType] 校验不一致，OpenType={(int)OpenType.OT_Give}");
                }

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
                ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" && ContentModel.Instance.nextReelStripsIndex == "BS";


                if (openType == (int)OpenType.OT_Give)
                {
                    totalLineWin = ContentModel.Instance.newFreeOnceCredit[ContentModel.Instance.freeSpinPlayTimes - 1] * ContentModel.Instance.betmultiple;
                    ContentModel.Instance.baseGameWinCredit = totalLineWin;
                }
            }

            //判断彩金游戏
            if (CustomModel.Instance.bonusGameconfig.IsHasBonusGame && !CustomModel.Instance.bonusGameconfig.IsBonusInLine)
            {
                int bonusCount = 0;
                bool isBonus = false;

                for (int i = 0; i < wheelChessNum; ++i)
                {
                    if (deckRowCol[i] == bonus)
                    {
                        bonusCount += 1;
                    }

                }
                for (int i = 0; i < CustomModel.Instance.jackpotGameConfig.Make2FreeGameCount.Length; ++i)
                {
                    if (bonusCount == CustomModel.Instance.jackpotGameConfig.Make2FreeGameCount[i])
                    {
                        isBonus = true;
                        bonusCount = CustomModel.Instance.jackpotGameConfig.FreeGameTime[i];
                        break;
                    }
                }

                if ((resultType == (int)ResultType.RT_BonusWin || resultType == (int)ResultType.RT_Jackpot) && isBonus)
                {
                    ContentModel.Instance.curReelStripsIndex = "BS";
                    ContentModel.Instance.nextReelStripsIndex = "JS";
                    ContentModel.Instance.isJackpotSpinTrigger = true;
                    ContentModel.Instance.jackpotSpinTotalTimes = bonusCount;
                    ContentModel.Instance.jackpotSpinPlayTimes = 0;
                    ContentModel.Instance.jackpotSpinWinCredit = 0;
                    jackpotSymbolInclude.Clear();
                    ContentModel.Instance.jackpotWin.Clear();

                    JSONArray bonusArray = res["BonusData"].AsArray;
                    totalLineWin += (int)res["BonusBet"] * MainModel.Instance.contentMD.betmultiple;

                    int[] bonusData = new int[bonusArray.Count];
                    for (int i = 0; i < bonusArray.Count; i++)
                    {
                        int data = bonusArray[i].AsInt;
                        bonusData[i] = data;
                    }

                    for (int i = 0; i < bonusData.Length; i++)
                    {
                        if (bonusData[i] == 0) continue;
                        ContentModel.Instance.jackpotWin[i] = bonusData[i].ToString();
                    }

                    if (resultType == (int)ResultType.RT_Jackpot)
                    {
                        ContentModel.Instance.jackpotSocre.Clear();
                        for (int i = 0; i < res["JPTypeArray"].Count; i++)
                        {
                            ContentModel.Instance.jackpotSocre[res["JPTypeArray"][i]] = res["JPBetArray"][i];
                        }

                        totalLineWin += res["TotalJackpotBet"];
                    }
                }
            }


            //赢分
            long creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (ContentModel.Instance.isSysCredit)
            {
                ContentModel.Instance.isSysCredit = false;
                creditBefore = ContentModel.Instance.realCredit;
            }
            long creditAfter = creditBefore - totalBet + totalLineWin;
            if (ContentModel.Instance.gameState == GameState.FreeSpin) creditAfter += totalBet;

            // 记录游戏数据到数据库
            Record(totalBet, res);

            //FreeSpinSessionStoreG3995.TryPersistOrClearSession();
        }


        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            ContentModel.Instance.SpecialBullIcon.Clear();
            //Matrix解析整列
            int rows = 4; // 3行
            int cols = 5; // 5列
            string strDeckRowCol = "";
            int MatrixLength = (int)res["MatrixLength"];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;
                    if (int.Parse(res["WildData"][index].Value) != 0)
                    {
                        // 获取原始值并加上 (WildData值 - 1)
                        int originalValue = int.Parse(res["Matrix"][index].Value);
                        int wildValue = int.Parse(res["WildData"][index].Value);
                        int newValue = originalValue + (wildValue - 1);
                        strDeckRowCol += newValue.ToString();
                    }
                    else
                    {
                        // 如果没有WildData，直接使用原值
                        strDeckRowCol += res["Matrix"][index].Value;
                    }

                    if (int.Parse(res["Matrix"][index].Value) == 15)
                    {
                        ContentModel.Instance.SpecialBullIcon.Add(new Cell()
                        { 
                            column = col,
                            row = row,
                        });
                    }

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

            //IDVec 中奖线
            int lineNum = (int)res["lineNum"];
            int totalEarnCredit = 0;
            int credit = 0;
            List<SymbolWin> winList = new List<SymbolWin>();
            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100; // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10; // 百位：消除数量（WinCount）
                int lineNumber = ID / 1000; // 万千位：线编号

                // 输出调试信息（可选）
                Debug.Log($"ID: {ID}, Line: {lineNumber}, HitCount: {hitCount}, Symbol: {symbolNumber}");

                int lineIndex = lineNumber;
                int[] lineInfo = CustomModel.Instance.payLines[lineIndex].ToArray();
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

            //判断彩金
            JackpotRes jpGameRes = new JackpotRes();
            bool isJackpotMajor = sboxJackpotData != null && (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 && sboxJackpotData.Lottery[0] == 1);
            bool isJackpotMinor = sboxJackpotData != null && (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 && sboxJackpotData.Lottery[1] == 1);
            bool isJackpotMini = sboxJackpotData != null && (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 2 && sboxJackpotData.Lottery[2] == 1);

            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 0
                ? sboxJackpotData.JackpotOut[0]
                : 0;
            jpGameRes.curJackpotMinior = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1
                ? sboxJackpotData.JackpotOut[1]
                : 0;
            jpGameRes.curJackpotMini = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2
                ? sboxJackpotData.JackpotOut[2]
                : 0;
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

            //判断免费奖或大奖
            int ResultType = (int)res["ResultType"];
            int OpenType = (int)res["OpenType"];
            int TotalFreeTime = (int)res["TotalFreeTime"];

            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.wheelData.Clear();
                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.freeSpinTotalTimes = TotalFreeTime;
                ContentModel.Instance.newFreeOnceCredit.Clear();
                ContentModel.Instance.freeSpinPlayTimes = 0;


                for(int i = 0; i < res["WheelData"].AsArray.Count; i++)
                {
                    ContentModel.Instance.wheelData.Add((int)res["WheelData"][i]);
                }

                for (int i = 0; i < TotalFreeTime; i++)
                {
                    ContentModel.Instance.newFreeOnceCredit.Add((int)res["FreeBetArray"][i]);
                }
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

            ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" &&
                                                     ContentModel.Instance.nextReelStripsIndex == "BS";


            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long creditBefore = MainBlackboardController.Instance.myRealCredit;
            //赢分
            long TotalWins = (int)res["TotalBet"] * MainModel.Instance.contentMD.betmultiple; //乘以下注倍数
            DebugUtils.Log("本局赢分TotalBet==" + TotalWins);
            long afterBetCredit = 0;
            if (OpenType == 1)
            {
                afterBetCredit = creditBefore - totalBet + TotalWins;
            }
            else
            {
                afterBetCredit = creditBefore - totalBet + TotalWins;
            }

            long creditAfter = afterBetCredit + totalEarnCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }

            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;
            if (OpenType == 1)
            {
                ContentModel.Instance.freeSpinTotalWinCredit = 0;
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCredit += totalEarnCredit;
                freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            }


            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);
            //bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;
            bool isReelsSlowMotion = false;
            ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;

            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();
            ContentModel.Instance.bonusResult = bonusResult;
            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;
            SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);

            // 记录游戏数据到数据库
            Record(totalBet, res);

            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
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
        List<int> wild = new List<int>() { CustomModel.Instance.symbolNumber[11],CustomModel.Instance.symbolNumber[22],CustomModel.Instance.symbolNumber[33], };
        private void CheckGameResult(string strDeckRowCol, int TotalWin)
        {
            List<List<int>> deckColRow = SlotTool.GetDeckColRow03(strDeckRowCol);
            int mult = 1;
            int scatter = CustomModel.Instance.symbolNumber[12];
            const int bonus = 13;
            int colCount = CustomModel.Instance.column;
            int calcTotalWin = 0; // 本地累计的总赢分（用于和服务器回包对比）
            List<List<int>> winLinesRule = CustomModel.Instance.payLines; // 中奖线
            List<PayTableSymbolInfo> payTable = CustomModel.Instance.payTableSymbolWin; // 赔率表

            if (deckColRow == null || deckColRow.Count == 0 || winLinesRule == null || payTable == null)
            {
                DebugUtils.LogError("[G3996][CheckGameResult] 数据为空，无法校验中奖结果。");
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
                             (currentSymbolType == firstSymbolType || wild.Contains(currentSymbolType)))
                    {
                        if (wild.Contains(currentSymbolType))
                        {
                            mult = mult > currentSymbolType / 11 ? mult : currentSymbolType / 11;
                        }
                        
                        sameTypeCount += 1;
                    }
                    // 第一个图标是 Wild，遇到可替代图标后以该图标作为基准
                    else if ((currentSymbolType != scatter && currentSymbolType != bonus) && wild.Contains(firstSymbolType))
                    {
                        mult = mult > firstSymbolType / 11 ? mult : firstSymbolType / 11;
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
                    int lineOdds = GetLineOdds(firstSymbolType, hitCount) * mult;
                    if (lineOdds > 0)
                    {
                        calcTotalWin += lineOdds; // 累加本地计算总赢分
                    }
                }
            }

            int diff = Math.Abs(calcTotalWin - TotalWin); // 计算本地校验值与算法差值
            if (diff != 0)
            {
                DebugUtils.LogError($"[G3996][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}");
            }
            else
            {
                DebugUtils.Log($"[G3996][CheckGameResult] 校验通过，TotalWin={TotalWin}");
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

            long jackpotWinCredit = 0;
            if (info.jpWinLst != null && info.jpWinLst.Count > 0)
            {
                JackpotWinInfo item = info.jpWinLst[0];
                gameSenceData.jpWinInfo = item;
                jackpotWinCredit = (long)item.winCredit;
                gameSenceData.jackpotWinCredit = jackpotWinCredit;
            }

            // 确定游戏类型
            int ResultType = res != null ? (int)res["ResultType"] : 0;
            int OpenType = res != null ? (int)res["OpenType"] : 0;


            // 构建记录对象
            TableSlotGameRecordItem slotGameRecordItem = new TableSlotGameRecordItem()
            {
                open_type = OpenType,
                result_type = ResultType,
                game_id = 3998,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet = totalBet,
                credit_before = creditBefore,
                credit_after = creditAfter,
                base_game_win_credit = totalEarnCredit,
                jackpot_win_credit = jackpotWinCredit,
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

            if (res.id != 3995) return;

            switch (res.name)
            {
                case GlobalEvent.GMFreeSpin:
                    nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMBigWin:
                    nextSpin = SpinDataType.BigWin;
                    break;
                case GlobalEvent.GMJpOnline:
                    //nextSpin = SpinDataType.JpOnline;
                    break;
                case GlobalEvent.GMJp1:
                    nextSpin = SpinDataType.JpSpin;
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
            JpSpin,
            JpOnline,
            Bonus1Ball,
        };

        private Dictionary<SpinDataType, List<string[]>> spinDatas = new Dictionary<SpinDataType, List<string[]>>()
        {
            [SpinDataType.FreeSpin] = new List<string[]>()
            {
               new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_6.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_7.json",
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin_free_8.json",
                },
            },
            [SpinDataType.Normal] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin__win0.json" },
            },
            [SpinDataType.JpSpin] = new List<string[]>()
            {
                new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g3995_real/g3995__slot_spin__jackpot.json" ,
                }
            },
            [SpinDataType.Bonus1Ball] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_1.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_2.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_3.json" }
            },
            [SpinDataType.BigWin] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3996_real/g3996__slot_spin__Bigwin_0.json" },
                //new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_1.json" },
                //new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_2.json" },
                //new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_3.json" },
            }

        };

        Queue<string> curDatas = new Queue<string>();


        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback,
            Action<BagelCodeError> errorCallback)
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

                    if (nextSpin != SpinDataType.None)
                    {
                        target = spinDatas[nextSpin];  // 使用指定的 spin 类型
                    }
                    else
                    {
                        target = spinDatas[SpinDataType.Normal];  // 使用默认的 Normal 类型
                    }

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


        // 三行五列的游戏结果矩阵（0=第一行，1=第二行，2=第三行）
        public List<List<int>> gameResultList = new List<List<int>>
        {
            new List<int>(new int[5]), // 第一行
            new List<int>(new int[5]), // 第二行
            new List<int>(new int[5]), // 第三行
            new List<int>(new int[5]) // 第四行
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
            for (int raw = 0; raw < 4; raw++)
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
                        rowIdx = UnityEngine.Random.Range(0, 4);
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
                        rowIdx = UnityEngine.Random.Range(0, 4);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }

                if (endlessLoop < 0)
                    DebugUtils.LogError($"【endless loop】: when add include symbol");

                gameResultList[rowIdx][colIdx] = symbolInclude.symbolNumber;
            }

            for (int i = 0; i < 4; i++)
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


            for (int i = 0; i < 4; i++)
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
    }
}
