using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace SlotZhuZaiJinBi1700
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

    public class MachineDataG1700Controller : MonoSingleton<MachineDataG1700Controller>
    {
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
            Bonus1Ball,
        };

        SpinDataType nextSpin = SpinDataType.None;
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

        public List<SymbolInclude> symbolInclude;
        Queue<string> curDatas = new Queue<string>();
        void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

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
            result["MatrixLength"] = matrixLength;
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

            if (resultType == (int)ResultType.RT_FreeWin)
            {
                int bonusBet = data[pos++];
                int bonusType = data[pos++];
                result["BonusData"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
                }
                result["BonusBet"] = bonusBet;
                result["BonusType"] = bonusType;
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

            int openType = (int)res["OpenType"];
            if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            {
                ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                bool expectGiveSpin = ContentModel.Instance.freeSpinTotalTimes > 0 && ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes;
                if (expectGiveSpin && openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError(
                        $"[G1700] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
                    FreeSpinSessionStoreG1700.Clear(SBoxModel.Instance.pid);
                    FreeSpinSessionStoreG1700.ResetContentModelFreeStateToBaseGame();
                }
            }

            int resultType = (int)res["ResultType"];
            int lineNum = (int)res["lineNum"];
            int totalwin = (int)res["TotalBet"];
            int matrixLength = (int)res["MatrixLength"];
            int rows = CustomModel.Instance.row; // 3行
            int cols = CustomModel.Instance.column; // 5列
            int wheelChessNum = rows * cols;
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            string strDeckRowCol = "";
            int totalLineWin = 0;
            int lineWin = 0;
            List<SymbolWin> winList = new List<SymbolWin>();
            JackpotRes jpGameRes = new JackpotRes();

            //判断普通奖
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

            long creditBefore = 0; 
            long creditAfter = 0; 
            //判断赠送局(未完成免费序列的每一局，算法 OpenType 为赠送)
            if (ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes)
            {
                if (openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError($"[G1700][CheckOpenType] 校验不一致，OpenType={(int)OpenType.OT_Give}");
                }

                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.freeSpinPlayTimes += 1;
                ContentModel.Instance.freeSpinTotalWinCredit += totalLineWin;

                if (ContentModel.Instance.freeSpinTotalTimes == ContentModel.Instance.freeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
                ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" && ContentModel.Instance.nextReelStripsIndex == "BS";

                //赢分
                creditBefore = MainBlackboardController.Instance.myRealCredit;
                creditAfter = creditBefore + totalLineWin;
            }
            else
            {


                List<int> deckRowCol = SlotTool.GetDeckRowCol(strDeckRowCol);
                int wild = CustomModel.Instance.symbolNumber[9];
                int scatter = CustomModel.Instance.symbolNumber[10];
                const int bonus = 11;


                //判断免费奖
                if (CustomModel.Instance.freeGameConfig.IsHasFreeGame && !CustomModel.Instance.freeGameConfig.IsScatterInLine)
                {
                    int scatterCount = 0;
                    bool isFree = false;
                    int customfreeTime = 0;
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
                            customfreeTime = CustomModel.Instance.freeGameConfig.FreeGameTime[i];
                        }
                    }

                    if (isFree)
                    {
                        if (resultType == (int)ResultType.RT_FreeWin && (customfreeTime == (int)res["TotalFreeTime"]))
                        {
                            int TotalFreeTime = (int)res["TotalFreeTime"];
                            ContentModel.Instance.curReelStripsIndex = "BS";
                            ContentModel.Instance.nextReelStripsIndex = "FS";
                            ContentModel.Instance.isFreeSpinTrigger = true;
                            ContentModel.Instance.gameNumberFreeSpinTrigger = MainModel.Instance.gameNumber;
                            ContentModel.Instance.freeSpinTotalTimes = TotalFreeTime;
                            ContentModel.Instance.freeSpinPlayTimes = 0;
                            ContentModel.Instance.freeSpinTotalWinCredit = 0;

                        }
                        else
                        {
                            DebugUtils.LogError($"[G1700][CheckFree] 校验不一致，算法回ResultType={resultType} ，本地计算isFree={isFree},算法FreeTime={(int)res["TotalFreeTime"]},本地计算freeTime={customfreeTime}");
                        }
                    }
                }

                //判断大奖
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

                    //....................
                }

                //赢分
                creditBefore = MainBlackboardController.Instance.myRealCredit;
                creditAfter = creditBefore - totalBet + totalLineWin;
            }


            //List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);
            ////bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;
            //bool isReelsSlowMotion = false;
            //ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            //// bonus数据
            //var bonusResult = new Dictionary<int, JSONNode>();
            //ContentModel.Instance.bonusResult = bonusResult;
            //ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;
            //SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);
          
            string machineId = string.IsNullOrEmpty(SBoxModel.Instance.MachineId) ? "00000000" : SBoxModel.Instance.MachineId;
            string algorithmVer = string.IsNullOrEmpty(SBoxModel.Instance.AlgorithmVer) ? "0_0_0" : SBoxModel.Instance.AlgorithmVer.Replace(".", "_");
            ContentModel.Instance.curGameGuid = $"{MainModel.Instance.gameID}-{ContentModel.Instance.curGameCreatTimeMS}-{machineId}-A{algorithmVer}";

            // 记录游戏数据到数据库
            Record(totalBet, res);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            DebugUtils.Log($"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {creditAfter}  totalWin={totalLineWin * MainModel.Instance.contentMD.betmultiple} ");

            FreeSpinSessionStoreG1700.TryPersistOrClearSession();
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
            int wild = CustomModel.Instance.symbolNumber[9];
            int scatter = CustomModel.Instance.symbolNumber[10];
            const int bonus = 11;
            int colCount = CustomModel.Instance.column;
            int calcTotalWin = 0; // 本地累计的总赢分（用于和服务器回包对比）
            List<List<int>> winLinesRule = CustomModel.Instance.payLines; // 中奖线
            List<PayTableSymbolInfo> payTable = CustomModel.Instance.payTableSymbolWin; // 赔率表

            if (deckColRow == null || deckColRow.Count == 0 || winLinesRule == null || payTable == null)
            {
                DebugUtils.LogError("[G1700][CheckGameResult] 数据为空，无法校验中奖结果。");
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
                DebugUtils.LogError($"[G1700][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}");
            }
            else
            {
                DebugUtils.Log($"[G1700][CheckGameResult] 校验通过，TotalWin={TotalWin}");
            }
        }

        void OnGMEvent(EventData res)
        {
            if (ApplicationSettings.Instance.isMock == false) return;

            if (res.id != 1700) return;

            switch (res.name)
            {
                case GlobalEvent.GMFreeSpin:
                    nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMBigWin:
                    nextSpin = SpinDataType.BigWin;
                    break;
                case GlobalEvent.GMJp1:
                    //nextSpin = SpinDataType.Jp1;
                    //GlobalJackpotConsole.NetClientManager.Instance.testIsHitJpGrandNext = true;
                    break;
                case GlobalEvent.GMJp2:
                    //nextSpin = SpinDataType.Jp2;
                    // GlobalJackpotConsole.NetClientManager.Instance.testIsHitJpMajorNext = true;
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
                    nextSpin = SpinDataType.Bonus1Ball;

                    break;
            }
        }

        private Dictionary<SpinDataType, List<string[]>> spinDatas = new Dictionary<SpinDataType, List<string[]>>()
        {
            [SpinDataType.FreeSpin] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_0.json",
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_1.json",
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_2.json",
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_3.json",
                    },
                },
            [SpinDataType.Normal] =
                new List<string[]>()
                {
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__win_4.json" },
                },
            [SpinDataType.Jp1] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_grand.json"
                    },
                },
            [SpinDataType.Jp2] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_major.json"
                    },
                },
            [SpinDataType.Jp3] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_minor.json"
                    },
                },
            [SpinDataType.Jp4] =
                new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_mini.json"
                    },
                },
            [SpinDataType.Bonus1Ball] =
                new List<string[]>()
                {
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_0.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_1.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_2.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__ball_3.json" }
                },
            [SpinDataType.BigWin] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_0.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_1.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_2.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_3.json" },
            }
        };

        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback,Action<BagelCodeError> errorCallback)
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
                        target = spinDatas[nextSpin]; // 使用指定的 spin 类型
                    }
                    else
                    {
                        target = spinDatas[SpinDataType.Normal]; // 使用默认的 Normal 类型
                    }

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

        /// <summary>
        /// 记录游戏数据到数据库
        /// </summary>
        private void Record(long totalBet, JSONNode res)
        {
            // 游戏场景记录
            GameSenceData gameSenceData = new GameSenceData();

            if (++MainModel.Instance.reportId < 0) MainModel.Instance.reportId = 1;

            gameSenceData.respone = ContentModel.Instance.response;
            gameSenceData.reportId = MainModel.Instance.reportId;
            gameSenceData.timeS = ContentModel.Instance.curGameCreatTimeMS / 1000;
            gameSenceData.gameNumber = MainModel.Instance.gameNumber;
            gameSenceData.gameNumberFreeSpinTrigger = ContentModel.Instance.isFreeSpin
                ? ContentModel.Instance.gameNumberFreeSpinTrigger
                : 0;
            gameSenceData.isFreeSpin = ContentModel.Instance.isFreeSpin;
            gameSenceData.freeSpinAddNum = ContentModel.Instance.freeSpinAddNum;

            gameSenceData.curStripsIndex = ContentModel.Instance.curReelStripsIndex;
            gameSenceData.nextStripsIndex = ContentModel.Instance.nextReelStripsIndex;
            gameSenceData.strDeckRowCol = ContentModel.Instance.strDeckRowCol;
            gameSenceData.deckRowCol = SlotTool.GetDeckRowCol(ContentModel.Instance.strDeckRowCol);

            gameSenceData.winFreeSpinTrigger = null;
            gameSenceData.winList = ContentModel.Instance.winList;
            gameSenceData.freeSpinPlayTimes = ContentModel.Instance.freeSpinPlayTimes;
            gameSenceData.freeSpinTotalTimes = ContentModel.Instance.freeSpinTotalTimes;
            gameSenceData.freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            gameSenceData.totalBet = totalBet;

            // 计算赢分
            long totalEarnCredit = 0;
            totalEarnCredit = (long)res["TotalBet"];
            gameSenceData.baseGameWinCredit = totalEarnCredit;

            // 获取游戏前后的分数
            long creditBefore = MainBlackboardController.Instance.myRealCredit;
            long creditAfter = MainBlackboardController.Instance.myRealCredit - totalBet + totalEarnCredit;

            gameSenceData.creditBefore = creditBefore;
            gameSenceData.creditAfter = creditAfter;

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
                free_curtime= ContentModel.Instance.freeSpinPlayTimes,
                free_totaltime= ContentModel.Instance.freeSpinTotalTimes,
                game_id = 1700,
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

            ////删除旧表
            //string dropSql = $"DROP TABLE IF EXISTS {ConsoleTableName.TABLE_SLOT_GAME_RECORD}";
            //SQLiteHelper.Instance.ExecuteNonQuery(dropSql);
            ////重建表
            //string createSql = SQLiteHelper.SQLCreateTable<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD);
            //SQLiteHelper.Instance.ExecuteNonQuery(createSql);


            // 插入数据
            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableSlotGameRecordItem>(
                ConsoleTableName.TABLE_SLOT_GAME_RECORD,
                slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //DebugUtils.Log($"[G1700] 游戏记录已写入数据库: gameType={gameType}, game_uid={ContentModel.Instance.curGameGuid}");
        }
    }
}