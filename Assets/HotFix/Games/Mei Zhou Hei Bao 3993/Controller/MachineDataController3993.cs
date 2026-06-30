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

namespace MeiZhouHeiBao_3993
{
    public enum SpinDataType
    {
        None,
        AlwaysWin,
        Normal,
        FreeSpin,
        BonusSpin,
        BigWin
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
            [SpinDataType.AlwaysWin] = new List<string>()
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
            [SpinDataType.Normal] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_0.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_1.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_2.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_3.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_4.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_5.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__notWin.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_6.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__Normal_7.json",
            },
            [SpinDataType.FreeSpin] = new List<string>()
            {
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__freeTrigger.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_1.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_2.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_3.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_4.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_5.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_6.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_7.json",
                "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__free_8.json",
            },
            [SpinDataType.BonusSpin] = new List<string>() { "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__bonusTrigger.json", "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__jackpotTrigger.json", },
            [SpinDataType.BigWin] = new List<string>() { "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__bigWin.json", "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__supperWin.json", "Assets/HotFix/Games/Mock/Resources/g" + GameId + "_real/g" + GameId + "__slot_spin__megaWin.json", },
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
                GlobalEvent.GMBigWin => SpinDataType.BigWin,
                _ => _nextSpinType
            };
        }

        public void RequestSlotSpinFromMock(long totalBet, Action<JSONNode> successCallback, Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (_currentDataQueue.Count == 0)
                {
                    List<string> target = _nextSpinType != SpinDataType.None ? _spinDataDic[_nextSpinType] : _spinDataDic[SpinDataType.Normal];
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

            if (ContentModel.Instance.PendingFreeSpinReconnectValidation)
            {
                ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                bool expectGiveSpin = ContentModel.Instance.FreeSpinTotalTimes > 0 && ContentModel.Instance.FreeSpinPlayTimes < ContentModel.Instance.FreeSpinTotalTimes;
                if (expectGiveSpin && openType != (int)OpenType.OT_Give)
                {
                    DebugUtils.LogError($"[G3993] 免费局重连校验失败：预期赠送局 OpenType={(int)OpenType.OT_Give}，实际={openType}。已清除本地快照并回退主游戏。");
                    FreeSpinSessionStoreG3993.Clear(SBoxModel.Instance.pid);
                    FreeSpinSessionStoreG3993.ResetContentModelFreeStateToBaseGame();
                }
            }
        }

        private void CheckGameResult(string strDeckRowCol, int totalWin, bool isInFreeSpin)
        {
            // 解析本局游戏
            List<List<int>> deckColRow = SlotTool.GetDeckColRow03(strDeckRowCol);
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
                DebugUtils.LogError("[G3993][CheckGameResult] 数据为空，无法校验中奖结果。");
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
                        Debug.Log("当前中奖线：" + i + "   中奖图标：" + firstSymbolType + "   中奖个数：" + hitCount + "  中奖得分：" +
                                  lineOdds);
                    }
                }
            }

            // if (isInFreeSpin)
            //     calcTotalWin *= ContentModel.Instance.freeGameScoreMultiply;

            int diff = Math.Abs(calcTotalWin - totalWin); // 计算本地校验值与算法差值
            if (diff != 0)
                DebugUtils.LogError($"[G3993][CheckGameResult] 中奖校验不一致，算法回包={totalWin}，本地计算={calcTotalWin}");
            else
                DebugUtils.Log($"[G3993][CheckGameResult] 校验通过，TotalWin={totalWin}");
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
            gameSceneData.gameNumberFreeSpinTrigger = ContentModel.Instance.isFreeSpin ? ContentModel.Instance.gameNumberFreeSpinTrigger : 0;
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

            //// 删除旧表
            //string dropSql = $"DROP TABLE IF EXISTS {ConsoleTableName.TABLE_SLOT_GAME_RECORD}";
            //SQLiteHelper.Instance.ExecuteNonQuery(dropSql);
            //// 重建表
            //string createSql = SQLiteHelper.SQLCreateTable<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD);
            //SQLiteHelper.Instance.ExecuteNonQuery(createSql);

            // 插入数据
            string sql = SQLiteAsyncHelper.SQLInsertTableData(ConsoleTableName.TABLE_SLOT_GAME_RECORD, slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
        }
    }
}