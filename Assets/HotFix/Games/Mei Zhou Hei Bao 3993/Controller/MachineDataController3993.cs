using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class MachineDataController3993 : MonoSingleton<MachineDataController3993>
    {
        private SpinDataType _nextSpin = SpinDataType.None;

        enum SpinDataType
        {
            None,
            Normal,
            FreeSpin,
            Bonus // cwy 新增
        };

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

            if (res.id != 3993) return;

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
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_0.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_1.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_2.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_3.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_4.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_5.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_6.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_7.json",
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__free_8.json",
                    },
                },
                [SpinDataType.Bonus] =
                    new List<string[]>()
                    {
                        new string[]
                        {
                            "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__bonus_0.json"
                        },
                    },
                [SpinDataType.Normal] = new List<string[]>()
                {
                    new string[]
                    {
                        "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__null_0.json"
                    }, // 不中
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__win_1.json" }, // 单线
                    new string[] { "Assets/HotFix/Games/Mock/Resources/g3993_real/g3993__slot_spin__win_2.json" }, // 多线
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
            new Dictionary<int, int>() { { 3, 8 }, { 4, 15 }, { 5, 20 } };

        private int CalculateLineWinCredit(int symbolNumber, int hitCount)
        {
            try
            {
                if (hitCount < 3)
                    return 0;

                List<PayTableSymbolInfo> payTable = CustomModel.Instance?.payTableSymbolWin;
                if (payTable == null || symbolNumber < 0 || symbolNumber >= payTable.Count)
                {
                    DebugUtils.LogError($"[g3993] 计算单线赢分失败，paytable越界。symbol={symbolNumber}, hit={hitCount}");
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
                DebugUtils.LogError($"[g3993] 计算单线赢分异常: {ex.Message}");
                return 0;
            }
        }

        private long _countFreeGetCredit = 0;

        /// <summary>
        /// 算法解析
        /// </summary>
        /// <param name="totalBet"></param>
        /// <param name="res"></param>
        /// <param name="sboxJackpotData"></param>
        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
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

            if (ContentModel.Instance.currentBootCount >= 4)
            {
                strDeckRowCol = strDeckRowCol.Replace("12", "8");
            }

            if (ContentModel.Instance.currentBootCount >= 10)
            {
                strDeckRowCol = strDeckRowCol.Replace("12", "8");
                strDeckRowCol = strDeckRowCol.Replace("11", "8");
            }

            if (ContentModel.Instance.currentBootCount >= 18)
            {
                strDeckRowCol = strDeckRowCol.Replace("12", "8");
                strDeckRowCol = strDeckRowCol.Replace("11", "8");
                strDeckRowCol = strDeckRowCol.Replace("9", "8");
            }

            if (ContentModel.Instance.currentBootCount >= 28)
            {
                strDeckRowCol = strDeckRowCol.Replace("12", "8");
                strDeckRowCol = strDeckRowCol.Replace("11", "8");
                strDeckRowCol = strDeckRowCol.Replace("9", "8");
                strDeckRowCol = strDeckRowCol.Replace("7", "8");
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
                DebugUtils.LogWarning($"[g3993] 线赢分校正: delta={fixDelta}, lineNum={lineNum}");
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
            int TotalFreeTime = (int)res["TotalFreeTime"];
            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.FreeSpinTotalTimes = TotalFreeTime;
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

                List<List<int>> currentStrDeck = SlotTool.GetDeckColRow03(strDeckRowCol); // 获取一局免费游戏图标
                ContentModel.Instance.currentBootList.Clear();

                for (int i = 0; i < currentStrDeck.Count; i++)
                {
                    for (int j = 0; j < currentStrDeck[i].Count; j++)
                    {
                        if (currentStrDeck[i][j] == 8)
                        {
                            ContentModel.Instance.currentBootList.Add(new Cell(i, j));
                        }
                    }
                }

                for (int m = 0; m < ContentModel.Instance.currentBootList.Count; m++)
                {
                    Debug.LogError(ContentModel.Instance.currentBootList[m].column + " " +
                                   ContentModel.Instance.currentBootList[m].row);
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

            // // 大奖
            // if (ResultType == 3)
            // {
            //     ContentModel.Instance.bonusTotalBet = (int)res["BonusBet"];
            //     ContentModel.Instance.IsBonusTrigger = true;
            // }

            long creditBefore =
                MainBlackboardController.Instance.myRealCredit; //myTempCredit 这是显示在UI上的的数值  myRealCredit是玩家的真实数据
            //赢分
            long TotalBet = (int)res["TotalBet"] * MainModel.Instance.contentMD.betmultiple;
            // if (ResultType == 3) TotalBet = (int)res["BonusBet"];
            // DebugUtils.Log("本局赢分TotalBet==" + TotalBet);

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

            if (OpenType == 1)
            {
                ContentModel.Instance.freeSpinTotalWinCoins = 0; //freeSpinTotalWinCredit 修改
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCoins += totalEarnCredit;
            }

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
                game_id = 3993,
                game_uid = "", //ContentModel.Instance.curGameGuid
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
            string sql = SQLiteAsyncHelper.SQLInsertTableData(
                ConsoleTableName.TABLE_SLOT_GAME_RECORD,
                slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
        }
    }
}