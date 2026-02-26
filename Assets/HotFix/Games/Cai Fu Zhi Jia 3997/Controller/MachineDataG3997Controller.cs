using GameMaker;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtil;
using System.Linq;

namespace CaiFuZhiJia_3997
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

        GSOperater = 9
    }

    enum SpinDataType
    {
        None,
        SingleWinLine,
        MultipleWinLine,
        Normal,
        FreeSpin,
        BigWin,
        Jp1,
        Jp2,
        Jp3,
        Jp4,
        JpOnline,
        Bonus1Ball,
        Bonus // cwy 新增
    };

    public class MachineDataG3997Controller : MonoSingleton<MachineDataG3997Controller>
    {
        private long TotalBet => SBoxModel.Instance.CoinInScale;
        SpinDataType nextSpin = SpinDataType.None;

        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            SBoxGameState gameState = (SBoxGameState)((int)res["gameState"]);

            List<int> LineNumbers = new List<int>();
            int lineMark = (int)res["lineMark"];

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
                int lineNumber = LineNumbers[i];

                winningLines.Add(new WinningLineInfo()
                {
                    LineNumber = lineNumber, SymbolNumber = symbolNumber, WinCount = hitCount
                });
            }

            bool isJackpotGrand = sboxJackpotData == null
                ? false
                : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0
                    ? sboxJackpotData.Lottery[0] == 1
                    : false);

            bool isJackpotMajor = sboxJackpotData == null
                ? false
                : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1
                    ? sboxJackpotData.Lottery[1] == 1
                    : false);

            bool isJackpotMinMinor = gameState == SBoxGameState.GSJpSmalm;
            bool isBonusBall = gameState == SBoxGameState.GSBonus;
            int hitBallCount = 0;

            int freeSpinTotalTimes, freeSpinPlayTimes;
            freeSpinTotalTimes = (int)res["maxRound"];
            freeSpinPlayTimes = (int)res["curRound"];


            bool isFreeSpinTrigger = freeSpinPlayTimes == 0 && freeSpinTotalTimes > 0;
            bool isFreeSpinResult = freeSpinTotalTimes > 0 && freeSpinPlayTimes == freeSpinTotalTimes;
            bool isFreeSpin = freeSpinPlayTimes > 0 && freeSpinTotalTimes > 0;

            string strDeckRowCol = "";
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();

            JackpotRes jpGameRes = new JackpotRes();
            ContentModel.Instance.JpGameRes = jpGameRes;

            jpGameRes.curJackpotGrand = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1
                ? sboxJackpotData.JackpotOut[0]
                : 0;
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2
                ? sboxJackpotData.JackpotOut[1]
                : 0;
            jpGameRes.curJackpotMinior = 1000;
            jpGameRes.curJackpotMini = 500;

            if (isJackpotGrand)
            {
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "grand",
                    id = 0,
                    winCredit = sboxJackpotData.Jackpotlottery[0],
                    whenCredit = sboxJackpotData.JackpotOld[0],
                    curCredit = sboxJackpotData.JackpotOut[0],
                });

                symbolInclude = new List<SymbolInclude>();
                for (int i = 0; i < 5; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 12 });
                }
            }

            if (isJackpotMajor)
            {
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "major",
                    id = 1,
                    winCredit = sboxJackpotData.Jackpotlottery[1],
                    whenCredit = sboxJackpotData.JackpotOld[1],
                    curCredit = sboxJackpotData.JackpotOut[1],
                });

                if (!isJackpotGrand)
                {
                    symbolInclude = new List<SymbolInclude>();
                    for (int i = 0; i < 4; i++)
                    {
                        symbolInclude.Add(new SymbolInclude() { symbolNumber = 12 });
                    }
                }
            }

            if (isJackpotMinMinor)
            {
                int winCredit = (int)res["num"];

                int ballCount = 0;

                if (winCredit == 1000)
                {
                    ballCount = 3;
                    jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                    {
                        name = "minor",
                        id = 2,
                        winCredit = 1000,
                        whenCredit = 1000,
                        curCredit = 1000,
                    });
                }
                else if (winCredit == 500)
                {
                    ballCount = 2;
                    jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                    {
                        name = "mini",
                        id = 3,
                        winCredit = 500,
                        whenCredit = 500,
                        curCredit = 500,
                    });
                }

                if (!isJackpotGrand && !isJackpotMajor)
                {
                    symbolInclude = new List<SymbolInclude>();
                    for (int i = 0; i < ballCount; i++)
                    {
                        symbolInclude.Add(new SymbolInclude() { symbolNumber = 12 });
                    }
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }
            else if (isBonusBall)
            {
                hitBallCount = (int)res["num"];

                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 11 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance?.payLines,
                    CustomModel.Instance?.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }
            else if (isFreeSpinTrigger)
            {
                int count = 3;

                switch ((int)res["num"])
                {
                    case 12:
                        count = 5;
                        break;
                    case 9:
                        count = 4;
                        break;
                    case 6:
                    default:
                        count = 3;
                        break;
                }

                for (int i = 0; i < count; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 10 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance?.payLines,
                    CustomModel.Instance?.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }
            else
            {
                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance?.payLines,
                    CustomModel.Instance?.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }


            long creditBefore = MainBlackboardController.Instance.myRealCredit;
            long totalEarnCoins = 0;


            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;


            ContentModel.Instance.response = res.ToString();
            strDeckRowCol = res["DeckRowCol"];
            ContentModel.Instance.strDeckRowCol = strDeckRowCol;


            // Debug.LogError("strDeckRowCol:" + strDeckRowCol);


            List<SymbolWin> winList = new List<SymbolWin>();
            for (int i = 0; i < LineNumbers.Count; i++)
            {
                int lineNumber = LineNumbers[i];
                int lineIndex = lineNumber - 1;

                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();

                JSONNode lineNode = res["lineData"][i];

                int rewardCoins = lineNode["reward"];
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
                    earnCredit = rewardCoins,
                    multiplier = 1,
                    lineNumber = lineNumber,
                    symbolNumber = symbolNumber,
                    cells = _cells,
                };
                winList.Add(sw);

                totalEarnCoins += rewardCoins;
            }

            ContentModel.Instance.winList = winList;

            if (isFreeSpin && freeSpinTotalTimes != 0)
            {
                ContentModel.Instance.freeSpinAddNum =
                    freeSpinTotalTimes - ContentModel.Instance.FreeSpinTotalTimes;
            }
            else
                ContentModel.Instance.freeSpinAddNum = 0;

            ContentModel.Instance.ShowFreeSpinRemainTime = isFreeSpin
                ? (ContentModel.Instance.FreeSpinTotalTimes - ContentModel.Instance.FreeSpinPlayTimes - 1)
                : 0;

            ContentModel.Instance.FreeSpinTotalTimes = freeSpinTotalTimes;
            ContentModel.Instance.FreeSpinPlayTimes = freeSpinPlayTimes;
            ContentModel.Instance.isFreeSpinTrigger = isFreeSpinTrigger;
            ContentModel.Instance.isFreeSpinResult = isFreeSpinResult;
            ContentModel.Instance.isBonus1 = isBonusBall;
            ContentModel.Instance.hitBallCount = hitBallCount;
            ContentModel.Instance.isHitJackpotGame = isJackpotGrand || isJackpotMajor || isJackpotMinMinor;

            // ContentModel.Instance.bonusCount = bonusCount;

            // if (res["num"] != 0 && res["curRound"] > 0)
            // {
            //     ContentModel.Instance.isFreeSpinAdd = true;
            //     ContentModel.Instance.FreeSpinTotalTimes = res["maxRound"];
            // }

            if (res["bonus"] != null)
            {
                int bonusTest = (int)res["bonus"];
                if (bonusTest > 5)
                    ContentModel.Instance.IsBonusTrigger = true;
            }

            if (isFreeSpinTrigger)
            {
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";
            }
            else if (isFreeSpinResult)
            {
                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.nextReelStripsIndex = "BS";
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

            ContentModel.Instance.totalEarnCoins = totalEarnCoins;
            ContentModel.Instance.baseGameWinCoins = totalEarnCoins;
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


            ContentModel.Instance.curGameGuid = isFreeSpin
                ? $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}-{ContentModel.Instance.gameNumberFreeSpinTrigger}"
                : $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}";

            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                ContentModel.Instance.gameNumberFreeSpinTrigger = MainModel.Instance.gameNumber;
                ContentModel.Instance.freeSpinTriggerGuid = ContentModel.Instance.curGameGuid;
            }

            long afterBetCredit = !isFreeSpin ? creditBefore - totalBet : creditBefore;


            // 免费游戏累计总赢
            if (isFreeSpin)
            {
                ContentModel.Instance.freeSpinTotalWinCoins += totalEarnCoins;
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCoins = 0;
            }

            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);
            bool isReelsSlowMotion = true;
            ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;

            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();
            ContentModel.Instance.BonusResults = bonusResult;
            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;


            if (ContentModel.Instance.isFreeSpin)
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
            }
            else if (ContentModel.Instance.isAuto || ContentModel.Instance.totalPlaySpins > 1)
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.AutoSpin);
            }
            else
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
            }
        }

        /// <summary>
        /// 算法解析
        /// </summary>
        /// <param name="totalBet"></param>
        /// <param name="res"></param>
        /// <param name="sBoxJackpotData"></param>
        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            //Matrix
            int rows = 3; // 3行
            int cols = 5; // 5列
            string strDeckRowCol = "";
            int MatrixLength = (int)res["MatrixLength"];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;
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

            //IDVec
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

                int lineIndex = lineNumber; // 注：中奖线索引从0开始
                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> _cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                // Todo:免费游戏出现报错
                if (!string.IsNullOrEmpty(res["TotalBet"]))
                    credit = res["TotalBet"];

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

            JackpotRes jpGameRes = new JackpotRes();
            bool isJackpotMajor = sboxJackpotData == null
                ? false
                : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0
                    ? sboxJackpotData.Lottery[0] == 1
                    : false);
            bool isJackpotMinor = sboxJackpotData == null
                ? false
                : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1
                    ? sboxJackpotData.Lottery[1] == 1
                    : false);
            bool isJackpotMini = sboxJackpotData == null
                ? false
                : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 2
                    ? sboxJackpotData.Lottery[2] == 1
                    : false);

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

            // long creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();

            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";

            //判断免费奖或大奖
            int ResultType = (int)res["ResultType"];
            int OpenType = (int)res["OpenType"];
            int TotalFreeTime = (int)res["TotalFreeTime"];
            string matrixArray = res["Matrix"].ToString();

            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;

                // 注：根据随机出的图标次数显示当前游戏总局数
                ContentModel.Instance.FreeSpinTotalTimes = TotalFreeTime;
                ContentModel.Instance.FreeSpinPlayTimes = 0;

                //for (int i = 0; i < TotalFreeTime; i++)
                //{
                //    int ID = (int)res["FreeBetArray"][i];
                //}
            }

            //赠送局
            if (OpenType == 1)
            {
                Debug.Log("-------赠送局--------");
                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.ShowFreeSpinRemainTime = (ContentModel.Instance.FreeSpinTotalTimes -
                                                                ContentModel.Instance.FreeSpinPlayTimes - 1);
                ContentModel.Instance.FreeSpinPlayTimes += 1;

                if (ContentModel.Instance.FreeSpinTotalTimes == ContentModel.Instance.FreeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
            }

            // 大奖 (目前是彩金游戏触发条件)
            int BonusType = (int)res["BonusType"];
            int BonusBet = (int)res["BonusBet"];
            if (ResultType == 3)
            {
                ContentModel.Instance.IsBonusTrigger = true;
            }


            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            long creditBefore = MainBlackboardController.Instance.myTempCredit;
            //赢分
            long TotalBet = (int)res["TotalBet"];
            // if (ResultType == 3) TotalBet = (int)res["BonusBet"]; //大奖得分
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

            // ## MainBlackboardController.Instance.SetMyTempCredit(afterBetCredit, false); // 不同步给ui
            // long creditAfter = afterBetCredit + totalEarnCredit;

            long creditAfter = afterBetCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }

            // ## MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            DebugUtils.Log($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            if (OpenType == 1)
            {
                ContentModel.Instance.freeSpinTotalWinCoins = 0; //freeSpinTotalWinCredit 修改
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCoins += totalEarnCredit;
                freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCoins;
            }


            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);

            // 原代码
            //bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;
            // bool isReelsSlowMotion = false;
            // ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            ContentModel.Instance.isReelsSlowMotion = true;

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
            ContentModel.Instance.BonusResults = bonusResult; //bonusResults 替换bonusResult


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

        public void Report()
        {
            JackpotRes info = ContentModel.Instance.JpGameRes;
            if (info.jpWinLst != null && info.jpWinLst.Count > 0)
            {
                JackpotWinInfo item = info.jpWinLst[0];

                Dictionary<string, object> req = new Dictionary<string, object>()
                {
                    ["type"] = "JackpotGame",
                    ["game_number"] = ContentModel.Instance.curGameGuid,
                    ["jp_type"] = $"jp{item.id + 1}",
                    ["coins"] = (int)item.winCredit,
                };
                NetCmdManager.Instance.RpcUpReportWin(req);
            }

            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                Dictionary<string, object> req = new Dictionary<string, object>()
                {
                    ["type"] = "FreeSpinTrigger",
                    ["game_number"] = ContentModel.Instance.curGameGuid,
                    ["total_times"] = ContentModel.Instance.FreeSpinTotalTimes,
                };
                NetCmdManager.Instance.RpcUpReportWin(req);
            }

            if (ContentModel.Instance.isFreeSpinResult)
            {
                Dictionary<string, object> req = new Dictionary<string, object>()
                {
                    ["type"] = "FreeSpinResult",
                    ["game_number"] = ContentModel.Instance.freeSpinTriggerGuid,
                    ["total_times"] = ContentModel.Instance.FreeSpinTotalTimes,
                };
                NetCmdManager.Instance.RpcUpReportWin(req);
            }

            if (ContentModel.Instance.isBonus1)
            {
                Dictionary<string, object> req = new Dictionary<string, object>()
                {
                    ["type"] = "Bonus1",
                    ["game_number"] = ContentModel.Instance.curGameGuid,
                    ["count"] = ContentModel.Instance.hitBallCount,
                };
                NetCmdManager.Instance.RpcUpReportWin(req);
            }
            //try
            //{
            //    // 数据数据上报
            //    string str = ReportDataUtils.CreatReportData(gameSenceData, SBoxModel.Instance.sboxPlayerInfo);
            //    DebugUtils.Log($"数据上报成功 {str}");
            //    ReportManager.Instance.SendData(str, null, null);
            //}
            //catch (Exception ex) { }
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
            if (ApplicationSettings.Instance.isMock == false)
                return;

            if (res.id != 3997) return;

            switch (res.name)
            {
                case GlobalEvent.GMSingleWinLine:
                    nextSpin = SpinDataType.SingleWinLine;
                    break;
                case GlobalEvent.GMBigWin:
                    nextSpin = SpinDataType.BigWin;
                    break;
                case GlobalEvent.GMJp1:
                    GlobalJackpotConsole.NetClientHelper02.Instance.testIsHitJpGrandNext = true;
                    break;
                case GlobalEvent.GMJp2:
                    GlobalJackpotConsole.NetClientHelper02.Instance.testIsHitJpMajorNext = true;
                    break;
                case GlobalEvent.GMJp3:
                    nextSpin = SpinDataType.Jp3;
                    break;
                case GlobalEvent.GMJp4:
                    nextSpin = SpinDataType.Jp4;

                    break;
                case GlobalEvent.GMJpOnline:
                    MachineDataManager02.Instance.testIsHitJackpotOnLine = true;
                    break;
                // cwy gm测试
                case GlobalEvent.GMBonus1:
                    nextSpin = SpinDataType.Bonus;
                    break;
                case GlobalEvent.GMFreeSpin:
                    nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMMultipleWinLine:
                    nextSpin = SpinDataType.Normal;
                    break;
            }
        }

        private Dictionary<SpinDataType, List<string[]>> spinDatas = new Dictionary<SpinDataType, List<string[]>>()
        {
            [SpinDataType.FreeSpin] = new List<string[]>()
            {
                new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_6.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_7.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_8.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_9.json",
                    "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__free_10.json",
                },
            },
            [SpinDataType.Bonus] =
                new List<string[]>()
                {
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__bonus_0.json" },
                },
            [SpinDataType.Normal] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__null_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__null_1.json" }, //单线
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__null_2.json" }, //多线
            },
            [SpinDataType.SingleWinLine] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__win_1.json" }, //单线
            },
            [SpinDataType.MultipleWinLine] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__win_2.json" }, //多线
            },
            [SpinDataType.Jp1] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__jackpot_grand.json"
                    },
                },
            [SpinDataType.Jp2] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__jackpot_major.json"
                    },
                },
            [SpinDataType.Jp3] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__jackpot_minor.json"
                    },
                },
            [SpinDataType.Jp4] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__jackpot_mini.json"
                    },
                },
            [SpinDataType.Bonus1Ball] =
                new List<string[]>()
                {
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__ball_0.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__ball_1.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__ball_2.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__ball_3.json" }
                },
            [SpinDataType.BigWin] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3997_real/g3997__slot_spin__Bigwin_0.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3997_real/g3997__slot_spin__Bigwin_1.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3997_real/g3997__slot_spin__Bigwin_2.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3997_real/g3997__slot_spin__Bigwin_3.json" },
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
                    target = nextSpin != SpinDataType.None ? spinDatas[nextSpin] : spinDatas[SpinDataType.Normal];
                    nextSpin = SpinDataType.None;

                    string[] strs = target[UnityEngine.Random.Range(0, target.Count)];
                    curDatas = new Queue<string>(strs); // 会改变引用数据  
                }

                string path = curDatas.Dequeue();
                int resourcesIndex = path.IndexOf("Resources/");
                string remainingPath = path.Substring(resourcesIndex + "Resources/".Length);
                remainingPath = remainingPath.Split('.')[0];

                try
                {
                    DebugUtils.LogWarning($"<color=yellow>mock down</color>: 使用数据: {remainingPath}");
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
                    DebugUtils.LogError($"数据报错： {remainingPath}");
                }
            });
        }
    }
}