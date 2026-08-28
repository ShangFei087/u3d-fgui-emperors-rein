using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using SlotZhuZaiJinBi1700;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MeiZhouHeiBao_3993
{
    public enum SpinDataType
    {
        None,
        NormalWin,
        PantherNormalWin,
        BigWin,
        FreeSpin,
        BonusSpin,
        JP1, JP2, JP3,
    }

    public class MachineDataController3993 : MonoSingleton<MachineDataController3993>

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

        private const int GameId = 3993;
        private SpinDataType _nextSpinType = SpinDataType.None;
        private Queue<string> _currentDataQueue = new Queue<string>();

        private readonly Dictionary<SpinDataType, List<string>> _spinDataDic = new Dictionary<SpinDataType, List<string>>()
        {
            [SpinDataType.NormalWin] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_0.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_1.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_2.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_3.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_4.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_5.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_6.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_7.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_8.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Win_9.json",
            },
            [SpinDataType.PantherNormalWin] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__pantherwin_0.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__pantherwin_1.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__pantherwin_2.json",
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
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_9.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_10.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_11.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_12.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_13.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_14.json"

            },
            [SpinDataType.BonusSpin] = new List<string>() 
            { 
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__bonusTrigger.json",
            },
            [SpinDataType.BigWin] = new List<string>() 
            { 
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__bigWin.json", 
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__superWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__megaWin.json", 
            },
            [SpinDataType.JP1] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__jackpotMini.json",
            },
            [SpinDataType.JP2] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__jackpotMinor.json",
            },
            [SpinDataType.JP3] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__jackpotMajor.json",
            }
        };

        private void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        private void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_GM_EVENT, OnGMEvent);
        }

        public void init()
        {
            if (_currentDataQueue.Count == 0 && _nextSpinType == SpinDataType.None)
            {
                List<string> target = _nextSpinType != SpinDataType.None ? _spinDataDic[_nextSpinType] : _spinDataDic[SpinDataType.NormalWin];
                _nextSpinType = SpinDataType.NormalWin;
                _currentDataQueue = new Queue<string>(target);
            }
        }

        private void OnGMEvent(EventData res)
        {
            if (ApplicationSettings.Instance.isMock == false) return;
            if (res.id != GameId) return;

            _nextSpinType = res.name switch
            {
                GlobalEvent.GMSingleWinLine => SpinDataType.NormalWin,
                GlobalEvent.GMMultipleWinLine => SpinDataType.PantherNormalWin,
                GlobalEvent.GMFreeSpin => SpinDataType.FreeSpin,
                GlobalEvent.GMBonus1 => SpinDataType.BonusSpin,
                GlobalEvent.GMJp1 => SpinDataType.JP1,
                GlobalEvent.GMJp2 => SpinDataType.JP2,
                GlobalEvent.GMJp3 => SpinDataType.JP3,
                GlobalEvent.GMBigWin => SpinDataType.BigWin,
                _ => _nextSpinType
            };

            _currentDataQueue = new Queue<string>(_spinDataDic[_nextSpinType]);
        }

        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback, Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (_currentDataQueue.Count == 0)
                {
                    List<string> target = _nextSpinType != SpinDataType.None? _spinDataDic[_nextSpinType] : _spinDataDic[SpinDataType.NormalWin];
                    _nextSpinType = SpinDataType.None; // 播完当前序列后回落到普通赢
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
            int panther = data[pos++];
            result["OpenType"] = openType;
            result["ResultType"] = resultType;
            result["lineNum"] = winlineNum;
            result["TotalBet"] = totalBet;
            result["MatrixLength"] = matrixLength;
            result["Panther"] = panther;
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

            if (panther == 1)
            {
                result["BonusData"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
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

            if (openType == (int)OpenType.OT_Give)
            {
                int pantherCount = data[pos++];
                result["Panther"] = pantherCount;
                result["WildData"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["WildData"].Add(id);
                }
            }

            if (resultType == (int)ResultType.RT_BonusWin)
            {
                int bonusBet = data[pos++];

                result["BonusBet"] = bonusBet;
                result["BonusData"] = new JSONArray();
                for (int i = 0; i < matrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
                }
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
                int JPCount = data[pos++];
                result["JPCount"] = JPCount;
                result["nJPCount"] = JPCount;

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

                int TotalJackpotBet = data[pos++];
                result["TotalJackpotBet"] = TotalJackpotBet;
                result["nTotalJackpotBet"] = TotalJackpotBet;
            }

            return result;
        }

        public bool ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (++MainModel.Instance.gameNumber < 0) MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();
            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";
            ContentModel.Instance.isFreeSpinTrigger = false;
            ContentModel.Instance.isSmallGameTrigger = false;
            ContentModel.Instance.isJackpotGame = false;
            ContentModel.Instance.isPantherWin = false;
            ContentModel.Instance.pantherBonusWin = 0;
            ContentModel.Instance.TotalJackpotBet = 0;
            ContentModel.Instance.JPTypeArray = Array.Empty<int>();
            ContentModel.Instance.JPBetArray = Array.Empty<int>();

            int openType = (int)res["OpenType"];
            int resultType = (int)res["ResultType"];
            int lineNum = (int)res["lineNum"];
            int totalwin = (int)res["TotalBet"];
            int matrixLength = (int)res["MatrixLength"];
            int rows = CustomModel.Instance.row; // 3行
            int cols = CustomModel.Instance.column; // 5列
            int wheelChessNum = rows * cols;
            bool isCheckGameResult;
            ContentModel.Instance.BonusData = new int[wheelChessNum];
            int[] wildData = new int[wheelChessNum];
            if (res["WildData"] != null)
            {
                int n = Math.Min(wheelChessNum, res["WildData"].Count);
                for (int i = 0; i < n; i++) wildData[i] = (int)res["WildData"][i];
            }
            ContentModel.Instance.wildData = wildData;

            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            string strDeckRowCol = "";
            int totalLineWin = 0;
            int lineWin = 0;
            List<SymbolWin> winList = new List<SymbolWin>();
            JackpotRes jpGameRes = new JackpotRes();

            if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            {
                //ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                //bool expectGiveSpin = ContentModel.Instance.freeSpinTotalTimes > 0 && ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes;
                //if (expectGiveSpin && openType != (int)OpenType.OT_Give)
                //{
                //    DebugUtils.LogError($"[G1700] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
                //    FreeSpinSessionStoreG1700.Clear(SBoxModel.Instance.pid);
                //    FreeSpinSessionStoreG1700.ResetContentModelFreeStateToBaseGame();
                //}
            }

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

                int wildMul = GetLineWildMul(_cells, wildData, cols);
                int lineOdds = GetLineOdds(symbolNumber, hitCount) * wildMul;
                lineWin = lineOdds * MainModel.Instance.contentMD.betmultiple;
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

            List<int> deckRowCol = SlotTool.GetDeckRowCol(strDeckRowCol);
            bool inFreeGive = ContentModel.Instance.freeSpinPlayTimes < ContentModel.Instance.freeSpinTotalTimes;
            if (!inFreeGive) TryApplyPantherWin(res, deckRowCol, wheelChessNum);

            ContentModel.Instance.baseGameWinCredit = totalLineWin + ContentModel.Instance.pantherBonusWin;
            //检查算法结果（免费局按变豹后盘面校验）
            isCheckGameResult=CheckGameResult(strDeckRowCol, totalwin, inFreeGive);

            //获取本地彩金彩金
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 0 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMinior = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMini = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[2] : 0;
            ContentModel.Instance.jpGameRes = jpGameRes;


            long creditBefore = 0;
            long creditAfter = 0;
            //判断赠送局(未完成免费序列的每一局，算法 OpenType 为赠送)
            if (inFreeGive)
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
                ContentModel.Instance.isFreeSpinFinish = ContentModel.Instance.curReelStripsIndex == "FS" && ContentModel.Instance.nextReelStripsIndex == "BS";

                //赢分
                creditBefore = MainBlackboardController.Instance.myRealCredit;
                creditAfter = creditBefore + totalLineWin;
            }
            else
            {
                int wild = CustomModel.Instance.symbolNumber[10];
                int scatter = CustomModel.Instance.symbolNumber[11];
                int bonus = CustomModel.Instance.symbolNumber[12];

                //判断免费奖
                if (CustomModel.Instance.FreeGameConfig.IsHasFreeGame && !CustomModel.Instance.FreeGameConfig.IsScatterInLine)
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
                    for (int i = 0; i < CustomModel.Instance.FreeGameConfig.Make2FreeGameCount.Length; ++i)
                    {
                        if (scatterCount == CustomModel.Instance.FreeGameConfig.Make2FreeGameCount[i])
                        {
                            isFree = true;
                            customfreeTime = CustomModel.Instance.FreeGameConfig.FreeGameTime[i];
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
                            ContentModel.Instance.totalPantherSymbolCount = 0;
                        }
                        else
                        {
                            DebugUtils.LogError($"[3993][CheckFree] 校验不一致，算法回ResultType={resultType} ，本地计算isFree={isFree},算法FreeTime={(int)res["TotalFreeTime"]},本地计算freeTime={customfreeTime}");
                        }
                    }
                }

                //判断大奖
                if (CustomModel.Instance.BonusGameConfig.IsHasBonusGame && !CustomModel.Instance.BonusGameConfig.IsBonusInLine)
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

                    if (bonusCount >= CustomModel.Instance.BonusGameConfig.Make2BonusGameCount)
                    {
                        isBonus = true;
                    }

                    bool isJackpotResult = resultType == (int)ResultType.RT_Jackpot;
                    bool isBonusWinResult = resultType == (int)ResultType.RT_BonusWin;
                    if (isJackpotResult && !isBonus)
                    {
                        DebugUtils.LogError(
                            $"[3993][CheckJackpot] 校验不一致，ResultType=RT_Jackpot 但 Bonus 数量={bonusCount}");
                    }
                    else if (isBonusWinResult && !isBonus)
                    {
                        DebugUtils.LogError(
                            $"[3993][CheckBonus] 校验不一致，ResultType=RT_BonusWin 但 Bonus 数量={bonusCount}");
                    }

                    if (isBonus && (isJackpotResult || isBonusWinResult))
                    {
                        ContentModel.Instance.isSmallGameTrigger = true;
                        ContentModel.Instance.isJackpotGame = isJackpotResult;
                        ContentModel.Instance.isSmallGameSpin = false;
                        ContentModel.Instance.isSmallGameFinish = false;
                        ContentModel.Instance.bonusSpinTime = 3;
                        ContentModel.Instance.BonusBet = res["BonusBet"] != null ? (int)res["BonusBet"] : 0;
                        CopyBonusData(res, wheelChessNum);

                        if (isJackpotResult)
                            ApplyJackpotResult(res, jpGameRes);

                        ApplyBonusRoundPlan(res, wheelChessNum);
                    }
                    else
                    {
                        ContentModel.Instance.BonusRound?.Clear();
                    }

                }
                //赢分
                creditBefore = MainBlackboardController.Instance.myRealCredit;
                if (ContentModel.Instance.isSmallGameTrigger)
                    creditAfter = creditBefore - totalBet + ContentModel.Instance.BonusBet + ContentModel.Instance.TotalJackpotBet;
                else
                    creditAfter = creditBefore - totalBet + totalLineWin + ContentModel.Instance.pantherBonusWin;
            }

            string machineId = string.IsNullOrEmpty(SBoxModel.Instance.MachineId) ? "00000000" : SBoxModel.Instance.MachineId;
            string algorithmVer = string.IsNullOrEmpty(SBoxModel.Instance.AlgorithmVer) ? "0_0_0" : SBoxModel.Instance.AlgorithmVer.Replace(".", "_");
            ContentModel.Instance.curGameGuid = $"{MainModel.Instance.gameID}-{ContentModel.Instance.curGameCreatTimeMS}-{machineId}-A{algorithmVer}";

            // 记录游戏数据到数据库
            Record(totalBet, res);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            DebugUtils.Log($"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {creditAfter}  totalWin={totalLineWin * MainModel.Instance.contentMD.betmultiple} ");
            FreeSpinSessionStoreG3993.TryPersistOrClearSession();
            return isCheckGameResult;
        }

        private void ApplyBonusRoundPlan(JSONNode res, int wheelChessNum)
        {
            ContentModel.Instance.BonusRound = BonusRoundHelper3993.ParseFromJson(res["BonusRound"]);

            var matrix = new List<int>(wheelChessNum);
            if (res["Matrix"] != null)
            {
                int n = Math.Min(wheelChessNum, res["Matrix"].Count);
                for (int i = 0; i < n; i++)
                    matrix.Add((int)res["Matrix"][i]);
            }

            if (ContentModel.Instance.BonusRound == null || ContentModel.Instance.BonusRound.Count == 0)
            {
                ContentModel.Instance.BonusRound = BonusRoundHelper3993.Build(matrix, ContentModel.Instance.BonusData);
                DebugUtils.Log("[3993] BonusRound 协议缺失，已本地模拟生成");
            }
            else if (!BonusRoundHelper3993.Validate(matrix, ContentModel.Instance.BonusData, ContentModel.Instance.BonusRound))
            {
                ContentModel.Instance.BonusRound = BonusRoundHelper3993.Build(matrix, ContentModel.Instance.BonusData);
                DebugUtils.LogWarning("[3993] BonusRound 校验失败，已回退本地模拟");
            }
        }

        private static void ApplyJackpotResult(JSONNode res, JackpotRes jpGameRes)
        {
            int[] types = ParseIntArray(res["JPTypeArray"]);
            int[] bets = ParseIntArray(res["JPBetArray"]);
            var typeList = new List<int>();
            var betList = new List<int>();
            int n = Math.Min(types.Length, bets.Length);
            for (int i = 0; i < n; i++)
            {
                if (bets[i] <= 0)
                    continue;
                typeList.Add(types[i]);
                betList.Add(bets[i]);
            }

            ContentModel.Instance.JPTypeArray = typeList.ToArray();
            ContentModel.Instance.JPBetArray = betList.ToArray();

            int totalJp = 0;
            if (res["TotalJackpotBet"] != null)
                totalJp = (int)res["TotalJackpotBet"];
            else if (res["nTotalJackpotBet"] != null)
                totalJp = (int)res["nTotalJackpotBet"];
            if (totalJp <= 0)
            {
                for (int i = 0; i < betList.Count; i++)
                    totalJp += betList[i];
            }

            ContentModel.Instance.TotalJackpotBet = totalJp;
            jpGameRes.jpWinLst.Clear();
            for (int i = 0; i < typeList.Count; i++)
            {
                jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                {
                    name = ContentModel.GetJackpotTypeName(typeList[i]),
                    id = typeList[i],
                    winCredit = betList[i],
                });
            }

            DebugUtils.Log(
                $"[3993][Jackpot] types=[{string.Join(",", typeList)}] bets=[{string.Join(",", betList)}] total={totalJp}");
        }

        //判断是否为特殊普通奖
        private void TryApplyPantherWin(JSONNode res, List<int> deckRowCol, int wheelChessNum)
        {
            if (deckRowCol == null || deckRowCol.Count == 0)
                return;

            int pantherId = CustomModel.Instance.symbolNumber[9];
            int bonusId = CustomModel.Instance.symbolNumber[12];
            int pantherCount = 0; //黑豹数量
            int bonusCount = 0; //bonus数量
            int n = Math.Min(wheelChessNum, deckRowCol.Count);
            for (int i = 0; i < n; i++)
            {
                if (deckRowCol[i] == pantherId)
                    pantherCount++;
                else if (deckRowCol[i] == bonusId)
                    bonusCount++;
            }

        
            //前端判断
            bool localPantherWin = pantherCount >= 1 && bonusCount >= 1 && bonusCount < 6;
            //后端判断
            bool serverPantherWin = res["Panther"] != null && (int)res["Panther"] == 1;
            if (localPantherWin != serverPantherWin)
            {
                DebugUtils.LogError($"[3993][CheckPanther] 校验不一致，本地={localPantherWin}(panther={pantherCount},bonus={bonusCount}) 算法Panther={res["Panther"]}");
                return;
            }

            if (!localPantherWin)
                return;

            CopyBonusData(res, wheelChessNum);
            ContentModel.Instance.isPantherWin = true;
            ContentModel.Instance.pantherBonusWin = SumBonusData();
            DebugUtils.Log($"[3993][PantherWin] panther={pantherCount} bonus={bonusCount} win={ContentModel.Instance.pantherBonusWin}");
        }

        private static void CopyBonusData(JSONNode res, int wheelChessNum)
        {
            ContentModel.Instance.BonusData = new int[wheelChessNum];
            if (res["BonusData"] == null)
                return;

            int n = Math.Min(wheelChessNum, res["BonusData"].Count);
            for (int i = 0; i < n; i++)
                ContentModel.Instance.BonusData[i] = (int)res["BonusData"][i];
        }

        private static int SumBonusData()
        {
            return ContentModel.SumBonusAmounts(ContentModel.Instance.BonusData);
        }

        private static int[] ParseIntArray(JSONNode node)
        {
            if (node == null || !node.IsArray)
                return Array.Empty<int>();

            int[] arr = new int[node.Count];
            for (int i = 0; i < node.Count; i++)
                arr[i] = (int)node[i];
            return arr;
        }

        private int GetLineWildMul(List<Cell> cells, int[] wildData, int cols)
        {
            if (cells == null || wildData == null || wildData.Length == 0) return 1;
            int sum = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                int index = cells[i].row * cols + cells[i].column;
                if (index >= 0 && index < wildData.Length)
                    sum += wildData[index];
            }
            return sum > 0 ? sum : 1;
        }
        private int GetCellWildMul(int col, int row)
        {
            return ContentModel.Instance.GetWildMul(col, row);
        }

        //检查算法结果
        private bool CheckGameResult(string strDeckRowCol, int TotalWin, bool inFreeGive)
        {
            List<List<int>> deckColRow = SlotTool.GetDeckColRow03(strDeckRowCol);
            int wild = CustomModel.Instance.symbolNumber[10];
            int scatter = CustomModel.Instance.symbolNumber[11];
            int bonus = CustomModel.Instance.symbolNumber[12];
            int colCount = CustomModel.Instance.column;
            int calcTotalWin = 0; // 本地累计的总赢分（用于和服务器回包对比）
            List<List<int>> winLinesRule = CustomModel.Instance.payLines; // 中奖线
            List<PayTableSymbolInfo> payTable = CustomModel.Instance.payTableSymbolWin; // 赔率表

            if (deckColRow == null || deckColRow.Count == 0 || winLinesRule == null || payTable == null)
            {
                DebugUtils.LogError("[G1700][CheckGameResult] 数据为空，无法校验中奖结果。");
                return false;
            }

            // 免费局 Matrix 是变豹前盘面，校验按升级后的黑豹算
            if (inFreeGive)
                CustomModel.Instance.ApplyFreePantherUpgrade(deckColRow, ContentModel.Instance.totalPantherSymbolCount);

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
                        //免费wild玩法
                        int wildSum = 0;
                        for (int n = 0; n < hitCount; n++)
                            wildSum += GetCellWildMul(n, currentLineRule[n]);
                        int wildMul = wildSum > 0 ? wildSum : 1;

                        calcTotalWin += lineOdds * wildMul; // 累加本地计算总赢分
                    }
                }
            }

            calcTotalWin += ContentModel.Instance.pantherBonusWin;

            int diff = Math.Abs(calcTotalWin - TotalWin); // 计算本地校验值与算法差值
            if (diff != 0)
            {
                DebugUtils.LogError($"[G1700][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}");
                return false;
            }
            else
            {
                DebugUtils.Log($"[G1700][CheckGameResult] 校验通过，TotalWin={TotalWin}");
                return true;
            }
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