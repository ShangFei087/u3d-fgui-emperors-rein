using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class MachineDataController3999 : MonoSingleton<MachineDataController3999>
    {
        private SpinDataType _nextSpin = SpinDataType.None;

        enum SpinDataType
        {
            None,
            Normal,
            FreeSpin,
            Bonus // cwy 新增
        };

        enum OpenType
        {
            OT_Normal,
            OT_Give,
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

            if (res.id != 3999) return;

            switch (res.name)
            {
                // cwy gm测试
                case GlobalEvent.GMBonus1:
                    _nextSpin = SpinDataType.Bonus;
                    break;
                case GlobalEvent.GMFreeSpin:
                    _nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMMultipleWinLine:
                    _nextSpin = SpinDataType.Normal;
                    break;
            }
        }

        private readonly Dictionary<SpinDataType, List<string[]>> _spineDataDic =
            new Dictionary<SpinDataType, List<string[]>>()
            {
                [SpinDataType.FreeSpin] = new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_0.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_1.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_2.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_3.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_4.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_5.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_6.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_7.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_8.json",
                        "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__free_9.json",
                    },
                },
                [SpinDataType.Bonus] =
                    new List<string[]>()
                    {
                        new string[]
                        {
                            "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__bonus_0.json"
                        },
                    },
                [SpinDataType.Normal] = new List<string[]>()
                {
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__null_0.json" },
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__win_1.json" }, //单线
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3999_real/g3999__slot_spin__win_2.json" }, //多线
                },
            };

        private Queue<string> _curDataQueue = new Queue<string>();

        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback,
            Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (_curDataQueue.Count == 0)
                {
                    List<string[]> target = null;
                    target = _nextSpin != SpinDataType.None
                        ? _spineDataDic[_nextSpin]
                        : _spineDataDic[SpinDataType.Normal];
                    _nextSpin = SpinDataType.None;

                    string[] strs = target[UnityEngine.Random.Range(0, target.Count)];
                    _curDataQueue = new Queue<string>(strs); // 会改变引用数据  
                }

                string path = _curDataQueue.Dequeue();
                int resourcesIndex = path.IndexOf("Resources/", StringComparison.Ordinal);
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

        private readonly Dictionary<int, int> _freeRoundDic =
            new Dictionary<int, int>() { { 3, 8 }, { 4, 10 }, { 5, 12 } };

        private int CalculateLineWinCredit(int symbolNumber, int hitCount)
        {
            try
            {
                if (hitCount < 3)
                    return 0;

                List<PayTableSymbolInfo> payTable = MainModel.Instance.cutomMD?.payTableSymbolWin;
                if (payTable == null || symbolNumber < 0 || symbolNumber >= payTable.Count)
                {
                    DebugUtils.LogError($"[G3999] 计算单线赢分失败，paytable越界。symbol={symbolNumber}, hit={hitCount}");
                    return 0;
                }

                PayTableSymbolInfo info = payTable[symbolNumber];
                double odd = 0;
                if (hitCount >= 5)
                {
                    odd = info.x5;
                }
                else if (hitCount == 4)
                {
                    odd = info.x4;
                }
                else
                {
                    odd = info.x3;
                }

                // 算法返回的 TotalBet 是未乘 betmultiple 的单位，单线赢分也保持同一单位。
                int lineWin = Mathf.Max(0, Mathf.RoundToInt((float)odd));
                return lineWin;
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"[G3999] 计算单线赢分异常: {ex.Message}");
                return 0;
            }
        }

        private long _countFreeGetCredit = 0;

        /// <summary>
        ///解析为本游戏 JSON与 <"ParseSlotSpin"/> 使用的字段一致。
        /// </summary>
        public static JSONNode ParseCoinPushSpinPayload(int[] data, int startPos)
        {
            JSONNode result = JSONNode.Parse("{}");
            if (data == null || startPos >= data.Length)
                return result;

            int pos = startPos;
            int OpenType = data[pos++];
            int ResultType = data[pos++];
            int WinlineNum = data[pos++];
            int TotalBet = data[pos++];
            int MatrixLength = data[pos++];
            result["OpenType"] = OpenType;
            result["ResultType"] = ResultType;
            result["lineNum"] = WinlineNum;
            result["TotalBet"] = TotalBet;
            result["IDVec"] = new JSONArray();
            for (int i = 0; i < WinlineNum; i++)
            {
                int id = data[pos++];
                result["IDVec"].Add(id);
            }

            result["Matrix"] = new JSONArray();
            for (int i = 0; i < MatrixLength; i++)
            {
                int id = data[pos++];
                result["Matrix"].Add(id);
            }

            if (OpenType == 2)
            {
                int TotalFreeTime = data[pos++];
                int TotalFreeBet = data[pos++];
                result["FreeBetArray"] = new JSONArray();
                for (int i = 0; i < TotalFreeTime; i++)
                {
                    int id = data[pos++];
                    result["FreeBetArray"].Add(id);
                }

                result["TotalFreeTime"] = TotalFreeTime;
                result["TotalFreeBet"] = TotalFreeBet;
            }

            if (OpenType == 3)
            {
                int BonusBet = data[pos++];
                int BonusType = data[pos++];
                result["BonusData"] = new JSONArray();
                for (int i = 0; i < MatrixLength; i++)
                {
                    int id = data[pos++];
                    result["BonusData"].Add(id);
                }

                result["BonusBet"] = BonusBet;
                result["BonusType"] = BonusType;
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
                DebugUtils.LogError("[G3999][CheckGameResult] 数据为空，无法校验中奖结果。");
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
                DebugUtils.LogError($"[G3999][CheckGameResult] 中奖校验不一致，算法回包={TotalWin}，本地计算={calcTotalWin}");
            }
            else
            {
                DebugUtils.Log($"[G3999][CheckGameResult] 校验通过，TotalWin={TotalWin}");
            }
        }

        // /// <summary>
        // /// 算法解析
        // /// </summary>
        // /// <param name="totalBet"></param>
        // /// <param name="res"></param>
        // /// <param name="sboxJackpotData"></param>
        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            //Matrix
            int rows = 3; // 3行
            int cols = 5; // 5列
            string strDeckRowCol = "";
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

            //IDVec 中奖线
            int lineNum = (int)res["lineNum"];
            int totalEarnCredit = 0;
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
                int[] lineInfo = CustomModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> _cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                int lineWinCredit = CalculateLineWinCredit(symbolNumber, hitCount);
                SymbolWin sw = new SymbolWin()
                {
                    earnCredit = lineWinCredit,
                    multiplier = 1,
                    lineNumber = lineNumber,
                    symbolNumber = symbolNumber,
                    cells = _cells,
                };
                winList.Add(sw);

                totalEarnCredit += lineWinCredit;
            }

            int serverTotalEarnCredit = (int)res["TotalBet"];
            if (lineNum > 0 && totalEarnCredit != serverTotalEarnCredit)
            {
                // 保障总赢分与算法返回一致，避免结算和表现出现偏差。
                int fixDelta = serverTotalEarnCredit - totalEarnCredit;
                winList[0].earnCredit += fixDelta;
                totalEarnCredit = serverTotalEarnCredit;
                DebugUtils.LogWarning($"[G3999] 线赢分校正: delta={fixDelta}, lineNum={lineNum}");
            }

            ContentModel.Instance.winList = winList;

            // 判断彩金
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

            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();

            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";
            //判断免费奖或大奖
            int ResultType = (int)res["ResultType"];
            int OpenType = (int)res["OpenType"];
            string matrixArray = res["Matrix"].ToString();
            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.FreeSpinTotalTimes = _freeRoundDic[CountTensEfficient(matrixArray)];
                ContentModel.Instance.FreeSpinPlayTimes = 0;
                ContentModel.Instance.freeTotalBet =
                    (int)res["TotalFreeBet"] * MainModel.Instance.contentMD.betmultiple;
            }

            //赠送局
            if (OpenType == 1)
            {
                Debug.Log("-------赠送局--------");
                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.ShowFreeSpinRemainTime = (ContentModel.Instance.FreeSpinTotalTimes -
                                                                ContentModel.Instance.FreeSpinPlayTimes - 1);
                ContentModel.Instance.FreeSpinPlayTimes += 1;

                // 免费加局实现
                int specialCount = CountTensEfficient(matrixArray);
                if (specialCount > 0)
                {
                    ContentModel.Instance.isFreeSpinAdd = true;
                    ContentModel.Instance.FreeSpinTotalTimes += specialCount;
                }

                if (ContentModel.Instance.FreeSpinTotalTimes == ContentModel.Instance.FreeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
            }

            // 大奖
            if (ResultType == 3)
            {
                ContentModel.Instance.bonusTotalBet = (int)res["BonusBet"];
                ContentModel.Instance.IsBonusTrigger = true;
            }

            long creditBefore =
                MainBlackboardController.Instance.myRealCredit; //myTempCredit 这是显示在UI上的的数值  myRealCredit是玩家的真实数据
            //赢分
            long TotalBet = (int)res["TotalBet"] * MainModel.Instance.contentMD.betmultiple;
            if (ResultType == 3) TotalBet = (int)res["BonusBet"];
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

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            DebugUtils.Log($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");

            // 免费游戏累计总赢 暂时没用就先注释
            // long freeSpinTotalWinCredit = 0;

            if (OpenType == 1)
            {
                ContentModel.Instance.freeSpinTotalWinCoins = 0; //freeSpinTotalWinCredit 修改
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCoins += totalEarnCredit;
                // freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCoins;
            }

            // List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);// 暂时没用就先注释
            // 原代码
            // bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;
            // bool isReelsSlowMotion = false;
            // ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            ContentModel.Instance.isReelsSlowMotion = true;

            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();
            ContentModel.Instance.bonusResults = bonusResult; //bonusResults 替换bonusResult
            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;
            SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);

            // 记录游戏数据到数据库
            Record(totalBet, res);
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

            // 判断是否处于免费游戏状态 修改代码
            bool isInFreeSpin = ContentModel.Instance.FreeSpinPlayTimes < ContentModel.Instance.FreeSpinTotalTimes;


            int openType = (int)res["OpenType"];
            int resultType = (int)res["ResultType"];
            int lineNum = (int)res["lineNum"];
            int totalwin = (int)res["TotalBet"];
            int matrixLength = (int)res["MatrixLength"];
            int bonusBet = (int)res["BonusBet"];
            string matrixArray = res["Matrix"].ToString();
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
                    if (scatterCount == CustomModel.Instance.FreeGameConfig.Make2FreeGameCount[i])
                    {
                        isFree = true;
                        freeTime = CustomModel.Instance.FreeGameConfig.FreeGameTime[i];
                    }
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
                    ContentModel.Instance.freeTotalBet =
                        (int)res["TotalFreeBet"] * MainModel.Instance.contentMD.betmultiple;

                    // 立即更新剩余次数显示 修改代码
                    ContentModel.Instance.ShowFreeSpinRemainTime = totalFreeTime;
                }
                else if (!isInFreeSpin)
                {
                    DebugUtils.LogError(
                        $"[G3999][CheckFree] 校验不一致，算法回ResultType={resultType} ，本地计算isFree={isFree},算法FreeTime={(int)res["TotalFreeTime"]},本地计算freeTime={freeTime}");
                }
            }

            // 判断赠送局
            if (isInFreeSpin)
            {
                // 验证OpenType是否为赠送局
                if (openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError($"[G3999][CheckOpenType] 校验不一致，当前处于免费游戏但OpenType={openType}");
                }

                ContentModel.Instance.curReelStripsIndex = "FS";
                ContentModel.Instance.FreeSpinPlayTimes += 1;
                ContentModel.Instance.freeSpinTotalWinCoins += totalLineWin;

                // 更新剩余次数显示
                ContentModel.Instance.ShowFreeSpinRemainTime = ContentModel.Instance.FreeSpinTotalTimes -
                                                               ContentModel.Instance.FreeSpinPlayTimes;

                // 免费加局实现
                int specialCount = CountTensEfficient(matrixArray);
                if (specialCount > 0)
                {
                    ContentModel.Instance.isFreeSpinAdd = true;
                    ContentModel.Instance.FreeSpinTotalTimes += specialCount;
                }

                // 判断是否是最后一局免费游戏
                ContentModel.Instance.nextReelStripsIndex =
                    ContentModel.Instance.FreeSpinPlayTimes == ContentModel.Instance.FreeSpinTotalTimes ? "BS" : "FS";

                ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" &&
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
                }

                if (bonusCount >= CustomModel.Instance.BonusGameConfig.Make2BonusGameCount)
                {
                    isBonus = true;
                }

                if (resultType == (int)ResultType.RT_BonusWin && isBonus)
                {
                    ContentModel.Instance.IsBonusTrigger = true;
                    ContentModel.Instance.bonusTotalBet = bonusBet;
                }
                else
                {
                    DebugUtils.LogError(
                        $"[G3999][CheckBonus] 校验不一致，算法回ResultType={resultType} ，本地计算isFree={isBonus}");
                }
            }

            //赢分
            long creditAfter = 0, creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (ContentModel.Instance.IsBonusTrigger)
                creditAfter = creditBefore + bonusBet - totalBet;
            else if (ContentModel.Instance.isFreeSpinTrigger)
            {
                // 免费游戏只有第一次需要扣积分
                if (ContentModel.Instance.FreeSpinPlayTimes == 1)
                    creditAfter = creditBefore + totalwin - totalBet;
                else
                    creditAfter = creditBefore + totalwin;
            }
            else if (!ContentModel.Instance.IsBonusTrigger && !ContentModel.Instance.isFreeSpinTrigger)
                creditAfter = creditBefore - totalBet + totalLineWin;

            ContentModel.Instance.isReelsSlowMotion = true;
            // 记录游戏数据到数据库
            Record(totalBet, res);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {creditAfter}  totalWin={totalLineWin * MainModel.Instance.contentMD.betmultiple} ");
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
                game_id = 3999,
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

            //DebugUtils.Log($"[G3999] 游戏记录已写入数据库: gameType={gameType}, game_uid={ContentModel.Instance.curGameGuid}");
        }


        #region 辅助方法

        /// <summary>
        /// 免费加局辅助方法
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        int CountTensEfficient(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            string trimmed = str.Trim();
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }

            // 分割字符串
            string[] parts = trimmed.Split(',');

            // 直接统计而不创建List
            int count = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                // 去除可能存在的空格并转换为整数
                int number = int.Parse(parts[i].Trim());

                // 检查是否等于10
                if (number == 10)
                {
                    count++;
                }
            }

            return count;
        }

        List<int> GetFreeRewardList(string str)
        {
            List<int> tempList = new List<int>();
            if (string.IsNullOrEmpty(str)) return null;
            string trimmed = str.Trim();
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }

            // 分割字符串
            string[] parts = trimmed.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                tempList.Add(int.Parse(parts[i]));
            }

            return tempList;
        }

        #endregion
    }
}