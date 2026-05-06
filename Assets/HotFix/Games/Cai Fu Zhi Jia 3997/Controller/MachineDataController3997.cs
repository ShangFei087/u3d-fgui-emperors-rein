using GameMaker;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using GameUtil;
using Newtonsoft.Json;
using System.Linq;

namespace CaiFuZhiJia_3997
{
    enum SpinDataType
    {
        None,
        Normal,
        FreeSpin,
        Bonus, // cwy 新增
        Jackpot, // cwy 新增
        BigWin // cwy 新增
    };

    public class MachineDataController3997 : MonoSingleton<MachineDataController3997>
    {
        private SpinDataType _nextSpin = SpinDataType.None;
        private long TotalBet => SBoxModel.Instance.CoinInScale;

        private readonly Dictionary<SpinDataType, List<string[]>> _spinDataDic =
            new Dictionary<SpinDataType, List<string[]>>()
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
                        new string[]
                        {
                            "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__bonus_0.json"
                        },
                    },
                [SpinDataType.BigWin] =
                    new List<string[]>()
                    {
                        new string[]
                        {
                            "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__bigwin.json"
                        },
                    },
                [SpinDataType.Jackpot] =
                    new List<string[]>()
                    {
                        new string[]
                        {
                            "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__jackpot_0.json"
                        },
                    },
                [SpinDataType.Normal] = new List<string[]>()
                {
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__null_0.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__null_1.json" }, //单线
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3997_real/g3997__slot_spin__null_2.json" }, //多线
                },
            };

        private Queue<string> _currentDataQueue = new Queue<string>();

        private enum OpenType
        {
            OT_Normal,
            OT_Give,
        }

        private enum ResultType
        {
            RT_Lose,
            RT_Win,
            RT_FreeWin,
            RT_BonusWin,
            RT_Jackpot,
            RT_JackpotOnline,
        }

        /// <summary>
        /// 算法解析
        /// </summary>
        /// <param name="totalBet"></param>
        /// <param name="res"></param>
        /// <param name="sBoxJackpotData"></param>
        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sBoxJackpotData)
        {
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (++MainModel.Instance.gameNumber < 0) MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();
            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";
            ContentModel.Instance.baseGameWinCredit = 0;

            // 判断是否处于免费游戏状态 修改代码
            bool isInFreeSpin = ContentModel.Instance.FreeSpinPlayTimes < ContentModel.Instance.FreeSpinTotalTimes;

            int openType = (int)res["OpenType"];

            if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            {
                ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                bool expectGiveSpin = ContentModel.Instance.FreeSpinTotalTimes > 0 &&
                                      ContentModel.Instance.FreeSpinPlayTimes <
                                      ContentModel.Instance.FreeSpinTotalTimes;
                if (expectGiveSpin && openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError(
                        $"[G3997] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
                    FreeSpinSessionStoreG3997.Clear(SBoxModel.Instance.pid);
                    FreeSpinSessionStoreG3997.ResetContentModelFreeStateToBaseGame();
                }
            }

            int resultType = (int)res["ResultType"];
            int lineNum = (int)res["lineNum"];
            int totalwin = (int)res["TotalBet"];
            string jpBetArray = res["JPBetArray"].ToString();
            string jPTypeArray = res["JPTypeArray"].ToString();
            int matrixLength = (int)res["MatrixLength"];
            int bonusBet = (int)res["BonusBet"];
            int totalJackpotBet = (int)res["TotalJackpotBet"];
            string matrixArray = res["Matrix"].ToString();
            string bonusData = res["BonusData"].ToString();
            int rows = CustomModel.Instance.row; // 3行
            int cols = CustomModel.Instance.column; // 5列
            int wheelChessNum = rows * cols;
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            string strDeckRowCol = "";
            int totalLineWin = 0;
            int lineWin = 0;
            List<SymbolWin> winList = new List<SymbolWin>();
            JackpotRes jpGameRes = new JackpotRes();

            // ContentModel.Instance.baseGameWinCredit = totalwin;// 主要用作BigWin计算

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


            if (isInFreeSpin)
            {
                // 免费游戏积分倍数增加
                int currentWildCount = strDeckRowCol.Count(c => c == '9');
                if (currentWildCount > 0)
                {
                    ContentModel.Instance.isHaveWildSymbol = true;
                    ContentModel.Instance.freeGameScoreMultiply += currentWildCount;
                }
            }

            //IDVec 
            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100; // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10; // 百位：消除数量（WinCount）
                int lineNumber = ID / 1000; // 万千位：线编号

                int lineIndex = lineNumber;
                int[] lineInfo = CustomModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> _cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                if (isInFreeSpin)
                    lineWin = GetLineOdds(symbolNumber, hitCount) * MainModel.Instance.contentMD.betmultiple *
                              ContentModel.Instance.freeGameScoreMultiply;
                else
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
            ContentModel.Instance.baseGameWinCredit = totalLineWin;
            //检查算法结果
            CheckGameResult(strDeckRowCol, totalwin, isInFreeSpin);

            //判断彩金
            bool isJackpotMajor = sBoxJackpotData == null
                ? false
                : (sBoxJackpotData.Lottery != null && sBoxJackpotData.Lottery.Length > 0
                    ? sBoxJackpotData.Lottery[0] == 1
                    : false);
            bool isJackpotMinor = sBoxJackpotData == null
                ? false
                : (sBoxJackpotData.Lottery != null && sBoxJackpotData.Lottery.Length > 1
                    ? sBoxJackpotData.Lottery[1] == 1
                    : false);
            bool isJackpotMini = sBoxJackpotData == null
                ? false
                : (sBoxJackpotData.Lottery != null && sBoxJackpotData.Lottery.Length > 2
                    ? sBoxJackpotData.Lottery[2] == 1
                    : false);

            jpGameRes.curJackpotMajor = sBoxJackpotData != null && sBoxJackpotData.JackpotOut.Length >= 0
                ? sBoxJackpotData.JackpotOut[0]
                : 0;
            jpGameRes.curJackpotMinior = sBoxJackpotData != null && sBoxJackpotData.JackpotOut.Length >= 1
                ? sBoxJackpotData.JackpotOut[1]
                : 0;
            jpGameRes.curJackpotMini = sBoxJackpotData != null && sBoxJackpotData.JackpotOut.Length >= 2
                ? sBoxJackpotData.JackpotOut[2]
                : 0;
            ContentModel.Instance.jpGameRes = jpGameRes;

            if (isJackpotMajor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "major",
                    id = 1,
                    winCredit = sBoxJackpotData.Jackpotlottery[1],
                    whenCredit = sBoxJackpotData.JackpotOld[1],
                    curCredit = sBoxJackpotData.JackpotOut[1],
                });
            }

            if (isJackpotMinor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "minor",
                    id = 1,
                    winCredit = sBoxJackpotData.Jackpotlottery[1],
                    whenCredit = sBoxJackpotData.JackpotOld[1],
                    curCredit = sBoxJackpotData.JackpotOut[1],
                });
            }

            if (isJackpotMini)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = "mini",
                    id = 1,
                    winCredit = sBoxJackpotData.Jackpotlottery[2],
                    whenCredit = sBoxJackpotData.JackpotOld[2],
                    curCredit = sBoxJackpotData.JackpotOut[2],
                });
            }

            List<int> deckRowCol = SlotTool.GetDeckRowCol(strDeckRowCol);
            int wild = CustomModel.Instance.symbolNumber[9];
            int scatter = CustomModel.Instance.symbolNumber[10];
            const int bonus = 11;

            //判断免费奖
            if (CustomModel.Instance.FreeGameConfig.IsHasFreeGame &&
                !CustomModel.Instance.FreeGameConfig.IsScatterInLine)
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

                for (int i = 0; i < CustomModel.Instance.FreeGameConfig.Make2FreeGameCount.Length; ++i)
                {
                    if (scatterCount < CustomModel.Instance.FreeGameConfig.Make2FreeGameCount[i])
                        continue;

                    isFree = true;
                    freeTime = CustomModel.Instance.FreeGameConfig.FreeGameTime[i];
                }

                if (resultType == (int)ResultType.RT_FreeWin && isFree && (freeTime == (int)res["TotalFreeTime"]))
                {
                    int totalFreeTime = (int)res["TotalFreeTime"];
                    ContentModel.Instance.curReelStripsIndex = "BS";
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                    ContentModel.Instance.isFreeSpinTrigger = true;
                    ContentModel.Instance.FreeSpinTotalTimes = totalFreeTime;
                    ContentModel.Instance.FreeSpinPlayTimes = 0;
                    ContentModel.Instance.freeSpinTotalWinCoins = 0;

                    // 立即更新剩余次数显示 修改代码
                    ContentModel.Instance.ShowFreeSpinRemainTime = totalFreeTime;
                }
                // else if (!isInFreeSpin)
                //     DebugUtils.LogError(
                //         $"[G3997][CheckFree] 校验不一致，算法回ResultType={resultType} ，本地计算isFree={isFree},算法FreeTime={(int)res["TotalFreeTime"]},本地计算freeTime={freeTime}");
            }

            // 判断赠送局
            if (isInFreeSpin)
            {
                // 验证OpenType是否为赠送局
                if (openType != (int)OpenType.OT_Give)
                    DebugUtils.LogError($"[G3997][CheckOpenType] 校验不一致，当前处于免费游戏但OpenType={openType}");

                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.FreeSpinPlayTimes += 1;
                ContentModel.Instance.freeSpinTotalWinCoins += totalLineWin;

                List<List<int>> currentStrDeck = SlotTool.GetDeckColRow03(strDeckRowCol); // 获取一局免费游戏图标
                ContentModel.Instance.currentWildList.Clear();

                for (int i = 0; i < currentStrDeck.Count; i++)
                {
                    for (int j = 0; j < currentStrDeck[i].Count; j++)
                    {
                        if (currentStrDeck[i][j] == 9)
                        {
                            ContentModel.Instance.currentWildList.Add(new Cell(i, j));
                        }
                    }
                }

                // 更新剩余次数显示
                ContentModel.Instance.ShowFreeSpinRemainTime = ContentModel.Instance.FreeSpinTotalTimes -
                                                               ContentModel.Instance.FreeSpinPlayTimes;

                // 判断是否是最后一局免费游戏
                ContentModel.Instance.nextReelStripsIndex =
                    ContentModel.Instance.FreeSpinPlayTimes == ContentModel.Instance.FreeSpinTotalTimes ? "BS" : "FS";

                ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" &&
                                                         ContentModel.Instance.nextReelStripsIndex == "BS";

                // ContentModel.Instance.baseGameWinCredit = totalLineWin;
            }


            // 判断大奖
            if (CustomModel.Instance.BonusGameConfig.IsHasBonusGame &&
                !CustomModel.Instance.BonusGameConfig.IsBonusInLine)
            {
                int bonusCount = 0;
                bool isBonus = false;

                for (int i = 0; i < wheelChessNum; ++i)
                    if (deckRowCol[i] == bonus)
                        bonusCount += 1;

                if (bonusCount >= CustomModel.Instance.BonusGameConfig.Make2BonusGameCount)
                    isBonus = true;

                if ((resultType == (int)ResultType.RT_BonusWin || resultType == (int)ResultType.RT_Jackpot) &&
                    isBonus) // 中彩金奖
                {
                    ContentModel.Instance.IsBonusTrigger = true;
                    if (resultType == (int)ResultType.RT_Jackpot)
                        ContentModel.Instance.IsJackpotTrigger = true;
                    ContentModel.Instance.currentBonusDataList.Clear();
                    ContentModel.Instance.currentBonusDataList = bonusData.Trim('[', ']').Split(',').ToList();

                    ContentModel.Instance.currentJpIndexList.Clear();
                    ContentModel.Instance.currentJpIndexList = ContentModel.Instance.currentBonusDataList
                        .Select((value, index) => new { value, index })
                        .Where(item => int.Parse(item.value) > 4000)
                        .Select(item => item.index)
                        .ToList();
                    ContentModel.Instance.jpBetArray.Clear();
                    ContentModel.Instance.jpTypeArray.Clear();
                    ContentModel.Instance.JpBetDic.Clear();
                    // Debug.LogError("jpBetArray:" + jpBetArray);
                    ContentModel.Instance.jpBetArray = jpBetArray.Trim('[', ']').Split(',').ToList();
                    ContentModel.Instance.jpTypeArray = jPTypeArray.Trim('[', ']').Split(',').ToList();
                    for (int i = 0; i < ContentModel.Instance.jpTypeArray.Count; i++)
                    {
                        if (ContentModel.Instance.jpTypeArray[i] != "0")
                            ContentModel.Instance.JpBetDic.Add(ContentModel.Instance.jpTypeArray[i],
                                ContentModel.Instance.jpBetArray[i]);
                    }
                }
                // else
                //     DebugUtils.LogError(
                //         $"[G3997][CheckBonus] 校验不一致，算法回ResultType={resultType} ，本地计算isBonus={isBonus}");
            }

            //赢分
            long creditAfter = 0, creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (ContentModel.Instance.IsBonusTrigger)
                creditAfter = creditBefore + bonusBet - totalBet + totalJackpotBet;
            else if (ContentModel.Instance.isFreeSpinTrigger || ContentModel.Instance.isFreeSpin)
            {
                // 免费游戏只有第一次需要扣积分
                if (ContentModel.Instance.FreeSpinPlayTimes == 0)
                    creditAfter = creditBefore + totalLineWin - totalBet;
                else
                    creditAfter = creditBefore + totalLineWin;

                // 断电重连之后，加上之前得到的分数
                if (ContentModel.Instance.isPowerTrigger)
                    creditAfter = creditAfter + ContentModel.Instance.freeSpinTotalWinCoins - totalLineWin;
            }
            else if (!ContentModel.Instance.IsBonusTrigger && !ContentModel.Instance.isFreeSpin)
                creditAfter = creditBefore - totalBet + totalLineWin;

            ContentModel.Instance.isReelsSlowMotion = true;

            // 记录游戏数据到数据库
            Record(totalBet, res);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {creditAfter}  totalWin={totalLineWin * MainModel.Instance.contentMD.betmultiple}    玩家真实金币={creditAfter}");

            FreeSpinSessionStoreG3997.TryPersistOrClearSession();
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        private void OnGMEvent(EventData res)
        {
            if (ApplicationSettings.Instance.isMock == false)
                return;

            if (res.id != 3997) return;

            switch (res.name)
            {
                case GlobalEvent.GMBonus1:
                    _nextSpin = SpinDataType.Bonus;
                    break;
                case GlobalEvent.GMFreeSpin:
                    _nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMMultipleWinLine:
                    _nextSpin = SpinDataType.Normal;
                    break;
                case GlobalEvent.GMJp1:
                    _nextSpin = SpinDataType.Jackpot;
                    break;
                case GlobalEvent.GMBigWin:
                    _nextSpin = SpinDataType.BigWin;
                    break;
            }
        }


        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback,
            Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (_currentDataQueue.Count == 0)
                {
                    List<string[]> target = null;
                    target = _nextSpin != SpinDataType.None
                        ? _spinDataDic[_nextSpin]
                        : _spinDataDic[SpinDataType.Normal];
                    _nextSpin = SpinDataType.None;

                    string[] strs = target[UnityEngine.Random.Range(0, target.Count)];
                    _currentDataQueue = new Queue<string>(strs); // 会改变引用数据  
                }

                string path = _currentDataQueue.Dequeue();
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

            if (resultType == (int)OpenType.OT_Give)
            {
                int wildMultiply = data[pos++];
                result["WildMultiply"] = wildMultiply;
            }

            if (resultType == (int)ResultType.RT_BonusWin)
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

            if (resultType == (int)ResultType.RT_Jackpot)
            {
                int bonusBet = data[pos++];
                result["BonusBet"] = bonusBet;

                result["BonusData"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
                }

                int jpCount = data[pos++];
                result["JPCount"] = jpCount;

                result["JPTypeArray"] = new JSONArray();
                for (int i = 0; i < 3; i++)
                {
                    int id = data[pos++];
                    result["JPTypeArray"].Add(id);
                }

                result["JPBetArray"] = new JSONArray();
                for (int i = 0; i < 3; i++)
                {
                    int id = data[pos++];
                    result["JPBetArray"].Add(id);
                }

                int totalJackpotBet = data[pos++];
                result["TotalJackpotBet"] = totalJackpotBet;
            }

            return result;
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
                case 2: return info.x2;
                case 3: return info.x3;
                case 4: return info.x4;
                case 5: return info.x5;
                default: return 0;
            }
        }

        //检查算法结果
        private void CheckGameResult(string strDeckRowCol, int TotalWin, bool isInFreeSpin)
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
                DebugUtils.LogError("[G3997][CheckGameResult] 数据为空，无法校验中奖结果。");
                return;
            }

            // // 新增中奖线输出
            // List<int> currentLines = new List<int>();
            // currentLines.Clear();

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
                    if ((firstSymbolType == scatter || firstSymbolType == bonus) &&
                        currentSymbolType == firstSymbolType)
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
                if (firstSymbolType != scatter && firstSymbolType != bonus && hitCount >= 2)
                {
                    int lineOdds = GetLineOdds(firstSymbolType, hitCount);
                    if (lineOdds > 0)
                    {
                        calcTotalWin += lineOdds; // 累加本地计算总赢分
                        Debug.Log("当前中奖线：" + i + "   中奖图标：" + firstSymbolType + "   中奖个数：" + hitCount + "  中奖得分：" +
                                  lineOdds);
                    }

                    // currentLines.Add(i);
                }
            }

            if (isInFreeSpin)
            {
                // calcTotalWin = calcTotalWin * MainModel.Instance.contentMD.betmultiple *
                //                ContentModel.Instance.freeGameScoreMultiply;

                calcTotalWin = calcTotalWin * ContentModel.Instance.freeGameScoreMultiply;
            }

            int diff = Math.Abs(calcTotalWin - TotalWin); // 计算本地校验值与算法差值
            if (diff != 0)
            {
                // DebugUtils.LogError(
                //     $"[G3997][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}，正常倍率是={MainModel.Instance.contentMD.betmultiple}，免费额外倍率是={ContentModel.Instance.freeGameScoreMultiply}");
                DebugUtils.LogError(
                    $"[G3997][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}，免费额外倍率是={ContentModel.Instance.freeGameScoreMultiply}");
            }
            else
            {
                DebugUtils.Log($"[G3997][CheckGameResult] 校验通过，TotalWin={TotalWin}");
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
            // gameSenceData.freeSpinPlayTimes = ContentModel.Instance.FreeSpinPlayTimes;
            // gameSenceData.freeSpinTotalTimes = ContentModel.Instance.FreeSpinTotalTimes;
            // gameSenceData.freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCoins;// freeSpinTotalWinCredit
            gameSenceData.totalBet = totalBet;

            // 计算赢分
            long totalEarnCredit = 0;
            if (ContentModel.Instance.winList != null)
            {
                foreach (var win in ContentModel.Instance.winList)
                {
                    totalEarnCredit += win.earnCredit;
                }
            }
            // totalEarnCredit = (long)res["TotalBet"];// 新增代码

            gameSenceData.baseGameWinCredit = totalEarnCredit;

            // 获取游戏前后的分数
            long creditBefore = MainBlackboardController.Instance.myTempCredit + totalBet; // 修改数据库中记录的是扣分之后的值
            long creditAfter = MainBlackboardController.Instance.myRealCredit;

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
                free_curtime = ContentModel.Instance.FreeSpinPlayTimes,
                free_totaltime = ContentModel.Instance.FreeSpinTotalTimes,
                game_id = 3997,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet = totalBet,
                credit_before = creditBefore,
                credit_after = creditAfter,
                base_game_win_credit = totalEarnCredit,
                jackpot_win_credit = jackpotWinCredit,
                strDeckRowCol = ContentModel.Instance.strDeckRowCol,
                symbol_icon_mapping = JsonConvert.SerializeObject(CustomModel.Instance.symbolIcon)
            };

            // 场景数据存入数据库
            slotGameRecordItem.scene = JsonConvert.SerializeObject(gameSenceData);

            // // 删除旧表
            // string dropSql = $"DROP TABLE IF EXISTS {ConsoleTableName.TABLE_SLOT_GAME_RECORD}";
            // SQLiteHelper.Instance.ExecuteNonQuery(dropSql);
            // // 重建表
            // string createSql = SQLiteHelper.SQLCreateTable<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD);
            // SQLiteHelper.Instance.ExecuteNonQuery(createSql); 

            // 插入数据
            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableSlotGameRecordItem>(
                ConsoleTableName.TABLE_SLOT_GAME_RECORD,
                slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //DebugUtils.Log($"[G3997] 游戏记录已写入数据库: gameType={gameType}, game_uid={ContentModel.Instance.curGameGuid}");
        }
    }
}