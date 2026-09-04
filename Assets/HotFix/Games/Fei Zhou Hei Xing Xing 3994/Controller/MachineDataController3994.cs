using GameMaker;
using GameUtil;
using HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FeiZhouHeiXingXing_3994
{
    public enum SpinDataType
    {
        None,
        AlwaysWin,
        Normal,
        FreeSpin,
        BonusSpin,
        Jackpot,
        BigWin
    }

    public class MachineDataController3994 : MonoSingleton<MachineDataController3994>
    {
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

        private const int GameId = 3994;
        private SpinDataType _nextSpinType = SpinDataType.None;
        private Queue<string> _currentDataQueue = new Queue<string>();

        private readonly Dictionary<SpinDataType, List<string>> _spinDataDic =
            new Dictionary<SpinDataType, List<string>>()
            {
                [SpinDataType.AlwaysWin] =
                    new List<string>()
                    {
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_0.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_1.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_2.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_3.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_4.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_5.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_6.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_7.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__win_8.json",
                    },
                [SpinDataType.Normal] =
                    new List<string>()
                    {
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_0.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_1.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_2.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_3.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_4.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_5.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_6.json",
                        "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__normal_7.json",
                    },
                [SpinDataType.FreeSpin] = new List<string>()
                {
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__freeTrigger.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_6.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_7.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_8.json",
                },
                [SpinDataType.BonusSpin] =
                    new List<string>() { "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__bonusTrigger.json", "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__jackpotTrigger.json", },
                [SpinDataType.Jackpot] =
                    new List<string>() { "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__jackpotTrigger.json", },
                [SpinDataType.BigWin] = new List<string>()
                {
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId +
                    "__slot_spin__bigWin.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId +
                    "__slot_spin__supperWin.json",
                    "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId +
                    "__slot_spin__megaWin.json",
                },
            };

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
            if (ApplicationSettings.Instance.isMock == false) return;
            if (res.id != GameId) return;

            _nextSpinType = res.name switch
            {
                GlobalEvent.GMSingleWinLine => SpinDataType.AlwaysWin,
                GlobalEvent.GMMultipleWinLine => SpinDataType.Normal,
                GlobalEvent.GMFreeSpin => SpinDataType.FreeSpin,
                GlobalEvent.GMBonus1 => SpinDataType.BonusSpin,
                GlobalEvent.GMJp1 => SpinDataType.Jackpot,
                GlobalEvent.GMBigWin => SpinDataType.BigWin,
                _ => _nextSpinType
            };
        }

        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback,
            Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (_currentDataQueue.Count == 0)
                {
                    List<string> target = _nextSpinType != SpinDataType.None
                        ? _spinDataDic[_nextSpinType]
                        : _spinDataDic[SpinDataType.Normal];
                    _nextSpinType = SpinDataType.None;
                    _currentDataQueue = new Queue<string>(target);
                }

                string path = _currentDataQueue.Dequeue();
                int resourcesIndex = path.IndexOf("Resources/", StringComparison.Ordinal); //path.IndexOf("Resources/");
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
                catch (Exception e)
                {
                    DebugUtils.LogError($"数据报错： {remainingPath}");
                }
            });
        }

        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sBoxJackpotData)
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

            // if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            // {
            //     ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
            //     bool expectGiveSpin = ContentModel.Instance.FreeSpinTotalTimes > 0 &&
            //                           ContentModel.Instance.FreeSpinPlayTimes <
            //                           ContentModel.Instance.FreeSpinTotalTimes;
            //     if (expectGiveSpin && openType != (int)OpenType.OT_Give)
            //     {
            //         DebugUtils.LogError(
            //             $"[G3994] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
            //         FreeSpinSessionStoreG3994.Clear(SBoxModel.Instance.pid);
            //         FreeSpinSessionStoreG3994.ResetContentModelFreeStateToBaseGame();
            //     }
            // }

            int resultType = (int)res["ResultType"];
            int lineNum = (int)res["lineNum"];
            int totalWin = (int)res["TotalBet"];
            int bonusBet = (int)res["BonusBet"];
            string bonusData = res["BonusData"].ToString();
            string bonusPos = res["BonusPos"].ToString();
            int totalJackpotBet = (int)res["TotalJackpotBet"];
            int rows = CustomModel.Instance.row; // 3行
            int cols = CustomModel.Instance.column; // 5列
            int wheelChessNum = rows * cols;
            string strDeckRowCol = "";
            int totalLineWin = 0;
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

                int lineIndex = lineNumber;
                int[] lineInfo = CustomModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    cells.Add(new Cell(colIdx, rowIdx));
                }

                int lineWin = GetLineOdds(symbolNumber, hitCount) * MainModel.Instance.contentMD.betmultiple;

                SymbolWin sw = new SymbolWin() { earnCredit = lineWin, multiplier = MainModel.Instance.contentMD.betmultiple, lineNumber = lineNumber, symbolNumber = symbolNumber, cells = cells, };
                winList.Add(sw);
                totalLineWin += lineWin;
            }

            ContentModel.Instance.winList = winList;
            ContentModel.Instance.baseGameWinCredit = totalLineWin;
            //检查算法结果
            CheckGameResult(strDeckRowCol, totalWin, isInFreeSpin);

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
            ContentModel.Instance.JpGameRes = jpGameRes;

            if (isJackpotMajor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo() { name = "major", id = 1, winCredit = sBoxJackpotData.Jackpotlottery[1], whenCredit = sBoxJackpotData.JackpotOld[1], curCredit = sBoxJackpotData.JackpotOut[1], });
            }

            if (isJackpotMinor)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo() { name = "minor", id = 1, winCredit = sBoxJackpotData.Jackpotlottery[1], whenCredit = sBoxJackpotData.JackpotOld[1], curCredit = sBoxJackpotData.JackpotOut[1], });
            }

            if (isJackpotMini)
            {
                int winCredit = (int)res["num"];
                jpGameRes.jpWinLst.Add(new JackpotWinInfo() { name = "mini", id = 1, winCredit = sBoxJackpotData.Jackpotlottery[2], whenCredit = sBoxJackpotData.JackpotOld[2], curCredit = sBoxJackpotData.JackpotOut[2], });
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

                // 更新剩余次数显示
                ContentModel.Instance.ShowFreeSpinRemainTime = ContentModel.Instance.FreeSpinTotalTimes -
                                                               ContentModel.Instance.FreeSpinPlayTimes;
                // 判断是否是最后一局免费游戏
                ContentModel.Instance.nextReelStripsIndex =
                    ContentModel.Instance.FreeSpinPlayTimes == ContentModel.Instance.FreeSpinTotalTimes ? "BS" : "FS";
                // 判断免费游戏结束
                ContentModel.Instance.isFreeSpinFinish = ContentModel.Instance.curReelStripsIndex == "FS" &&
                                                         ContentModel.Instance.nextReelStripsIndex == "BS";
            }

            // 判断大奖
            if (CustomModel.Instance.BonusGameConfig.IsHasBonusGame &&
                !CustomModel.Instance.BonusGameConfig.IsBonusInLine)
            {
                int bonusCount = 0;
                bool isBonus = false;

                for (int i = 0; i < wheelChessNum; ++i)
                {
                    if (deckRowCol[i] == bonus)
                    {
                        bonusCount += 1;
                    }

                    Debug.Log(deckRowCol[i]);
                }

                if (bonusCount >= CustomModel.Instance.BonusGameConfig.Make2BonusGameCount)
                    isBonus = true;

                if ((resultType == (int)ResultType.RT_BonusWin || resultType == (int)ResultType.RT_Jackpot)) // 中彩金奖
                {
                    if (isBonus)
                    {
                        List<int> bonusDataList;
                        List<int[]> bonusPosList;
                        ContentModel.Instance.BonusDataQueue.Clear();
                        if (ApplicationSettings.Instance.isMock)
                        {
                            bonusPosList = BonusParseController.ParseBonusPos(bonusPos);
                            bonusDataList = BonusParseController.ParseBonusData(bonusData);
                            ContentModel.Instance.BonusDataQueue =
                                BonusParseController.ParseAllSpins(bonusPosList, bonusDataList);
                        }
                        else
                        {
                            bonusPosList = ParseFromString(bonusPos);
                            bonusDataList = ParseBonusData(bonusData);
                            ContentModel.Instance.BonusDataQueue = ValidateAndQueue(bonusDataList, bonusPosList);
                        }

                        ContentModel.Instance.isSmallGameTrigger = true;
                        ContentModel.Instance.smallGameWinCredit = bonusBet + totalJackpotBet;
                    }
                    else
                    {
                        DebugUtils.LogError(
                            $"[G3994][CheckBonus] 校验不一致，算法回ResultType={resultType} ，本地计算isBonus={isBonus}");
                    }
                }
            }

            //赢分
            long creditAfter = 0, creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (ContentModel.Instance.isSmallGameTrigger)
                creditAfter = creditBefore + ContentModel.Instance.smallGameWinCredit - totalBet;
            else if (ContentModel.Instance.isFreeSpinTrigger || ContentModel.Instance.isFreeSpin)
            {
                // 免费游戏只有第一次需要扣积分
                if (ContentModel.Instance.FreeSpinPlayTimes == 0)
                    creditAfter = creditBefore + totalLineWin - totalBet;
                else
                    creditAfter = creditBefore + totalLineWin;
            }
            else if (!ContentModel.Instance.isFreeSpinTrigger && !ContentModel.Instance.isFreeSpin)
                creditAfter = creditBefore - totalBet + totalLineWin;

            ContentModel.Instance.isReelsSlowMotion = true;

            // 记录游戏数据到数据库（与上方分支结算的 creditBefore/creditAfter 一致）
            Record(totalBet, res, creditBefore, creditAfter);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {creditAfter}  totalWin={totalLineWin * MainModel.Instance.contentMD.betmultiple}    玩家真实金币={creditAfter}");

            FreeSpinSessionStoreG3994.TryPersistOrClearSession();
        }

        private void CheckGameResult(string strDeckRowCol, int totalWin, bool isInFreeSpin)
        {
            // 解析本局游戏
            strDeckRowCol = isInFreeSpin ? ChangeIcon(strDeckRowCol) : strDeckRowCol;
            List<List<int>> deckColRow = isInFreeSpin ? GetDeckColRow(strDeckRowCol) : SlotTool.GetDeckColRow03(strDeckRowCol);

            // 获取特殊图标
            const int bonus = 11;
            int wild = CustomModel.Instance.symbolNumber[9];
            int scatter = CustomModel.Instance.symbolNumber[10];

            int colCount = CustomModel.Instance.column;
            int calcTotalWin = 0; // 本地累计的总赢分（用于和服务器回包对比）
            List<List<int>> winLinesRule = CustomModel.Instance.payLines; // 中奖线
            List<PayTableSymbolInfo> payTable = CustomModel.Instance.payTableSymbolWin; // 赔率表
            if (deckColRow == null || deckColRow.Count == 0 || winLinesRule == null || payTable == null)
            {
                DebugUtils.LogError("[G3994][CheckGameResult] 数据为空，无法校验中奖结果。");
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
                    }
                }
            }

            int diff = Math.Abs(calcTotalWin - totalWin); // 计算本地校验值与算法差值
            if (diff != 0)
                DebugUtils.LogError($"[G3994][CheckGameResult] 中奖校验不一致，算法回包={totalWin}，本地计算={calcTotalWin}");
            else
                DebugUtils.Log($"[G3994][CheckGameResult] 校验通过，TotalWin={totalWin}");
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

            if (info == null) return 0;

            return hitCount switch
            {
                2 => info.x2,
                3 => info.x3,
                4 => info.x4,
                5 => info.x5,
                _ => 0
            };
        }

        private void Record(long totalBet, JSONNode res, long creditBefore, long creditAfter)
        {
            // 游戏场景记录
            GameSenceData gameSceneData = new GameSenceData();
            if (++MainModel.Instance.reportId < 0) MainModel.Instance.reportId = 1;

            gameSceneData.respone = ContentModel.Instance.response;
            gameSceneData.reportId = MainModel.Instance.reportId;
            gameSceneData.timeS = ContentModel.Instance.curGameCreatTimeMS / 1000;
            gameSceneData.gameNumber = MainModel.Instance.gameNumber;
            gameSceneData.gameNumberFreeSpinTrigger = ContentModel.Instance.isFreeSpin
                ? ContentModel.Instance.gameNumberFreeSpinTrigger
                : 0;
            gameSceneData.isFreeSpin = ContentModel.Instance.isFreeSpin;
            gameSceneData.freeSpinAddNum = ContentModel.Instance.freeSpinAddNum;
            gameSceneData.curStripsIndex = ContentModel.Instance.curReelStripsIndex;
            gameSceneData.nextStripsIndex = ContentModel.Instance.nextReelStripsIndex;
            gameSceneData.strDeckRowCol = ContentModel.Instance.strDeckRowCol;
            gameSceneData.deckRowCol = SlotTool.GetDeckRowCol(ContentModel.Instance.strDeckRowCol);
            gameSceneData.winFreeSpinTrigger = null;
            gameSceneData.winList = ContentModel.Instance.winList;
            gameSceneData.totalBet = totalBet;

            // 游戏类型
            int resultType = res != null ? (int)res["ResultType"] : 0;
            int openType = res != null ? (int)res["OpenType"] : 0;
            // 真实钱包本局扣费前 / 本局结算后（写入 SetMyRealCredit 的值）
            gameSceneData.creditBefore = creditBefore;
            gameSceneData.creditAfter = creditAfter;
            // 基础游戏赢分
            long totalEarnCredit = 0;
            if (ContentModel.Instance.winList != null)
            {
                totalEarnCredit += ContentModel.Instance.winList.Sum(win => win.earnCredit);
            }

            gameSceneData.baseGameWinCredit = totalEarnCredit;
            // 彩金赢分
            long jackpotWinCredit = res != null ? (int)res["TotalJackpotBet"] : 0;
            string jackpotType = "";
            // for (int i = 0; i < ContentModel.Instance.jpTypeArray.Count; i++)
            // {
            //     if (ContentModel.Instance.jpTypeArray[i] != "0")
            //     {
            //         jackpot_type = ContentModel.Instance.jpTypeArray[i].ToString();
            //         break;
            //     }
            // }


            //免费游戏赢分
            long freeGameWinCredit = res != null ? (int)res["TotalFreeBet"] : 0;
            long bonusGameWinCredit = res != null ? (int)res["BonusBet"] : 0;
            long totalWinCredit = totalEarnCredit + jackpotWinCredit + freeGameWinCredit + bonusGameWinCredit;
            // 构建记录对象
            TableSlotGameRecordItem slotGameRecordItem = new TableSlotGameRecordItem()
            {
                open_type = openType,
                result_type = resultType,
                jackpot_type = jackpotType,
                free_curtime = ContentModel.Instance.FreeSpinPlayTimes,
                free_totaltime = ContentModel.Instance.FreeSpinTotalTimes,
                game_id = GameId,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet = totalBet,
                credit_before = creditBefore,
                credit_after = creditAfter,
                base_game_win_credit = totalEarnCredit,
                jackpot_win_credit = jackpotWinCredit,
                free_spin_win_credit = freeGameWinCredit,
                bonus_game_win_credit = bonusGameWinCredit,
                total_win_credit = totalWinCredit,
                strDeckRowCol = ContentModel.Instance.strDeckRowCol,
                symbol_icon_mapping = JsonConvert.SerializeObject(CustomModel.Instance.symbolIcon)
            };

            // 场景数据存入数据库
            slotGameRecordItem.scene = JsonConvert.SerializeObject(gameSceneData);

            // 删除旧表
            string dropSql = $"DROP TABLE IF EXISTS {ConsoleTableName.TABLE_SLOT_GAME_RECORD}";
            SQLiteHelper.Instance.ExecuteNonQuery(dropSql);
            // 重建表
            string createSql =
                SQLiteHelper.SQLCreateTable<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD);
            SQLiteHelper.Instance.ExecuteNonQuery(createSql);

            // 插入数据
            string sql =
                SQLiteAsyncHelper.SQLInsertTableData(ConsoleTableName.TABLE_SLOT_GAME_RECORD, slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
        }


        /// <summary>
        /// 解析为本游戏 JSON，与 ParseSlotSpin 使用的字段一致。
        /// 注意：C 端第一个字段是 gameId，若调用方传入的 startPos 未跳过 gameId，
        ///       则需要在方法开头额外读一次 gameId。
        /// </summary>
        public static JSONNode ParseCoinPushSpinPayload(int[] data, int startPos)
        {
            JSONNode result = JSONNode.Parse("{}");
            if (data == null || startPos >= data.Length) return result;

            int pos = startPos;

            // 如果 startPos 指向的是 gameId（即未跳过），需要取消下面这行的注释
            // int gameId = data[pos++];

            int openType = data[pos++];
            int resultType = data[pos++];
            int lineNum = data[pos++];
            int totalBet = data[pos++];
            int matrixLength = data[pos++];

            result["OpenType"] = openType;
            result["ResultType"] = resultType;
            result["lineNum"] = lineNum;
            result["TotalBet"] = totalBet;
            result["MatrixLength"] = matrixLength;

            result["IDVec"] = new JSONArray();
            for (int i = 0; i < lineNum; i++)
            {
                result["IDVec"].Add(data[pos++]);
            }

            result["Matrix"] = new JSONArray();
            for (int i = 0; i < matrixLength; i++)
            {
                result["Matrix"].Add(data[pos++]);
            }

            switch (resultType)
            {
                case (int)ResultType.RT_FreeWin:
                    {
                        int totalFreeTime = data[pos++];
                        int totalFreeBet = data[pos++];
                        result["FreeBetArray"] = new JSONArray();
                        for (int i = 0; i < totalFreeTime; i++)
                        {
                            result["FreeBetArray"].Add(data[pos++]);
                        }

                        result["TotalFreeTime"] = totalFreeTime;
                        result["TotalFreeBet"] = totalFreeBet;
                        break;
                    }

                case (int)ResultType.RT_BonusWin:
                    {
                        // C 端写入顺序：BlindSymbol(旋转次数) -> nBonusBet
                        int totalBonusSpinTime = data[pos++]; // = BlindSymbol
                        int bonusBet = data[pos++]; // = nBonusBet

                        result["BonusData"] = new JSONArray();
                        for (int i = 0; i < matrixLength; i++)
                        {
                            result["BonusData"].Add(data[pos++]);
                        }

                        // ===== BonusPos 解析开始 =====
                        result["BonusPos"] = ParseBonusPos(data, ref pos, matrixLength, totalBonusSpinTime);
                        // ===== BonusPos 解析结束 =====

                        result["BonusBet"] = bonusBet;
                        result["TotalBonusSpinTime"] = totalBonusSpinTime;
                        break;
                    }

                case (int)ResultType.RT_Jackpot:
                    {
                        // C 端写入顺序：BlindSymbol(旋转次数) -> nBonusBet
                        int totalBonusSpinTime = data[pos++]; // = BlindSymbol
                        int bonusBet = data[pos++]; // = nBonusBet

                        result["BonusBet"] = bonusBet;
                        result["TotalBonusSpinTime"] = totalBonusSpinTime;

                        result["BonusData"] = new JSONArray();
                        for (int i = 0; i < matrixLength; i++)
                        {
                            result["BonusData"].Add(data[pos++]);
                        }

                        int jpCount = data[pos++];
                        result["JPCount"] = jpCount;

                        result["JPTypeArray"] = new JSONArray();
                        for (int i = 0; i < jpCount; i++) // 按实际写入个数读取
                        {
                            result["JPTypeArray"].Add(data[pos++]);
                        }

                        result["JPBetArray"] = new JSONArray();
                        for (int i = 0; i < jpCount; i++) // 按实际写入个数读取
                        {
                            result["JPBetArray"].Add(data[pos++]);
                        }

                        // ===== BonusPos 解析开始 =====
                        result["BonusPos"] = ParseBonusPos(data, ref pos, matrixLength, totalBonusSpinTime);
                        // ===== BonusPos 解析结束 =====

                        if (pos < data.Length)
                        {
                            result["TotalJackpotBet"] = data[pos];
                        }

                        break;
                    }
            }

            return result;
        }

        #region 免费数据校验方法

        private string ChangeIcon(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            // 1. 解析 strDeckRowCol 为 3行×5列 的二维数组
            string[] rows = strDeckRowCol.Split('#');
            int rowCount = rows.Length;
            int colCount = rows[0].Split(',').Length;

            int[,] grid = new int[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                string[] cols = rows[r].Split(',');
                for (int c = 0; c < colCount; c++)
                {
                    grid[r, c] = int.Parse(cols[c]);
                }
            }

            // 2. 找出所有需要被转换的位置（暂不修改 grid），从8向下逐级传播
            // 到4为止，4不将3转为4
            List<(int r, int c)> allChangedPositions = new List<(int, int)>();

            for (int sourceValue = 8; sourceValue >= 5; sourceValue--)
            {
                int targetValue = sourceValue - 1;
                HashSet<(int, int)> toUpgrade = new HashSet<(int, int)>();

                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        if (grid[r, c] != sourceValue) continue;

                        // 上
                        if (r > 0 && grid[r - 1, c] == targetValue)
                            toUpgrade.Add((r - 1, c));
                        // 下
                        if (r < rowCount - 1 && grid[r + 1, c] == targetValue)
                            toUpgrade.Add((r + 1, c));
                        // 左
                        if (c > 0 && grid[r, c - 1] == targetValue)
                            toUpgrade.Add((r, c - 1));
                        // 右
                        if (c < colCount - 1 && grid[r, c + 1] == targetValue)
                            toUpgrade.Add((r, c + 1));
                    }
                }

                // 标记升级（此时仅记录位置，不立即修改 grid，以免影响同级传播）
                foreach (var pos in toUpgrade)
                {
                    if (!allChangedPositions.Contains(pos))
                        allChangedPositions.Add(pos);
                    grid[pos.Item1, pos.Item2] = sourceValue;
                }
            }

            // 3. 先在转换位置播放特效，再切换图标
            if (allChangedPositions.Count <= 0)
            {
                return strDeckRowCol;
            }

            List<string> rowStrings = new List<string>();
            for (int r = 0; r < rowCount; r++)
            {
                List<string> colStrings = new List<string>();
                for (int c = 0; c < colCount; c++)
                {
                    colStrings.Add(grid[r, c].ToString());
                }

                rowStrings.Add(string.Join(",", colStrings));
            }

            strDeckRowCol = string.Join("#", rowStrings);
            return strDeckRowCol;
        }

        private List<List<int>> GetDeckColRow(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            string[] rows = strDeckRowCol.Split('#');
            int rowNum = rows.Length;
            int colNum = rows[0].Split(',').Length;

            // 1. 先解析成二维数组，方便整列修改
            int[,] matrix = new int[rowNum, colNum];
            for (int rowIndex = 0; rowIndex < rowNum; rowIndex++)
            {
                string[] cols = rows[rowIndex].Split(',');
                for (int colIndex = 0; colIndex < colNum; colIndex++)
                {
                    matrix[rowIndex, colIndex] = int.Parse(cols[colIndex]);
                }
            }

            // 2. 扫描每行：跳过每行第1个元素（colIndex = 0），如果其他位置有9，标记该列
            bool[] colToNine = new bool[colNum];
            for (int colIndex = 1; colIndex < colNum; colIndex++) // 从1开始，跳过每行第一个
            {
                if (matrix[1, colIndex] == 9)
                {
                    colToNine[colIndex] = true;
                }
            }

            // 3. 将标记的列全部改成9
            for (int colIndex = 0; colIndex < colNum; colIndex++)
            {
                if (colToNine[colIndex])
                {
                    for (int rowIndex = 0; rowIndex < rowNum; rowIndex++)
                    {
                        matrix[rowIndex, colIndex] = 9;
                    }
                }
            }


            // 4. 按列存入 List<List<int>>（保持和你原方法一致的返回结构）
            List<List<int>> colrowLst = new List<List<int>>();
            for (int colIndex = 0; colIndex < colNum; colIndex++)
            {
                List<int> _col = new List<int>();
                for (int rowIndex = 0; rowIndex < rowNum; rowIndex++)
                {
                    _col.Add(matrix[rowIndex, colIndex]);
                }

                colrowLst.Add(_col);
            }

            return colrowLst;
        }

        #endregion

        #region 彩金数据解析

        private const int GorillaValue = 20001;
        private const int BananaBase = 30000;

        /// <summary> 解析 BonusPos 多圈盘面数据。每圈：1 个 mask + nBanana 个金额值。</summary>
        private static JSONArray ParseBonusPos(int[] data, ref int pos, int matrixLength, int spinCount)
        {
            var bonusPos = new JSONArray();

            for (int s = 0; s < spinCount; s++)
            {
                if (pos >= data.Length)
                    break;

                uint mask = (uint)data[pos++];
                uint bananaMap = mask & 65535u;
                uint gorillaMap = mask >> 16;

                // 统计本圈有多少个香蕉（决定后面要读几个金额值）
                int nBanana = 0;
                for (int c = 0; c < matrixLength; c++)
                {
                    if ((bananaMap & (1u << c)) != 0)
                        nBanana++;
                }

                if (pos + nBanana > data.Length)
                    break;

                // 解码本圈 matrixLength 个格子
                var spinBoard = new JSONArray();
                int valuePos = pos; // 金额值起始位置

                for (int c = 0; c < matrixLength; c++)
                {
                    if ((gorillaMap & (1u << c)) != 0)
                    {
                        spinBoard.Add(GorillaValue); // 20001
                    }
                    else if ((bananaMap & (1u << c)) != 0)
                    {
                        spinBoard.Add(BananaBase + data[valuePos++]); // 30000 + 金额
                    }
                    else
                    {
                        spinBoard.Add(0); // 空
                    }
                }

                bonusPos.Add(spinBoard);
                pos += nBanana; // 跳过本圈已消费的金额值
            }

            return bonusPos;
        }

        /// <summary>
        /// 传入 BonusData JSON 字符串，返回 List&lt;int&gt;
        /// </summary>
        /// <param name="bonusDataJson">例如: "[750,825,0,425,1500,1000,1750,475,0,0,0,0,0,0,0]"</param>
        /// <returns>List&lt;int&gt; — 每圈得分</returns>
        private List<int> ParseBonusData(string bonusDataJson)
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

        private List<int[]> ParseFromString(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<int[]>();

            int[][] arrays = JsonConvert.DeserializeObject<int[][]>(json);
            return new List<int[]>(arrays);
        }

        /// <summary>
        /// 验证对应索引的 int[] 得分之和与 int 对应索引的值是否一致。
        /// 规则：数组中万位是3的数字，先对 10000 取余后再参与累加；其他数字直接累加。
        /// 一致则将 int[] 转为 List&lt;int&gt; 入队；不一致则输出日志。
        /// </summary>
        /// <param name="intList">int 类型的 List</param>
        /// <param name="intArrayList">int[] 类型的 List</param>
        /// <returns>Queue&lt;List&lt;int&gt;&gt; 队列</returns>
        public Queue<List<int>> ValidateAndQueue(List<int> intList, List<int[]> intArrayList)
        {
            var resultQueue = new Queue<List<int>>();

            if (intList == null || intArrayList == null)
            {
                Debug.LogError("[警告] 输入参数不能为 null");
                return resultQueue;
            }

            int minSize = Math.Min(intList.Count, intArrayList.Count);

            if (intList.Count != intArrayList.Count)
            {
                Debug.LogError(
                    $"[警告] 两个 List 长度不一致，intList.Count={intList.Count}, intArrayList.Count={intArrayList.Count}，将只处理前 {minSize} 个元素");
            }

            for (int i = 0; i < minSize; i++)
            {
                int expectedSum = intList[i];
                int[] array = intArrayList[i];

                // 计算实际得分之和
                int actualSum = CalculateSum(array);

                if (actualSum != expectedSum)
                {
                    Debug.LogError(
                        $"[不一致] 索引 [{i}]：期望和={expectedSum}，实际和={actualSum}，原始数组=[{string.Join(", ", array)}]");
                }
                else
                {
                    // 一致：将 int[] 转换为 List<int> 并入队
                    resultQueue.Enqueue(array.ToList());
                }
            }

            return resultQueue;
        }

        /// <summary>
        /// 计算数组的得分之和。
        /// 万位是3的数字：先 % 10000 再累加。
        /// 万位不是3的数字：直接累加。
        /// 
        /// 注：如果需求是"只有万位是3的数字才参与累加（其他跳过）"，
        ///     请将 else 分支中的 sum += num; 删除即可。
        /// </summary>
        private int CalculateSum(int[] array)
        {
            int sum = 0;
            foreach (int num in array)
            {
                if (GetWanWei(num) == 3)
                {
                    // 万位是3：对 10000 取余后累加
                    sum += num % 10000;
                }
            }

            return sum;
        }

        /// <summary>
        /// 获取数字的万位（从右往左第5位）。支持负数。
        /// 例如：30000→3, 35000→3, 130000→3, 9999→0, -30000→3
        /// </summary>
        private int GetWanWei(int num)
        {
            long absNum = Math.Abs((long)num);
            return (int)((absNum / 10000) % 10);
        }

        #endregion
    }
}