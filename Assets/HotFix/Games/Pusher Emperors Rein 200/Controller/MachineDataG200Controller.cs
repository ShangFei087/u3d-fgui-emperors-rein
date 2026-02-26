using GameMaker;
using GameUtil;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace PusherEmperorsRein
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

    public class MachineDataG200Controller : MonoSingleton<MachineDataG200Controller>
    {
        public List<SymbolInclude> symbolInclude;


        long TotalBet => (long)SBoxModel.Instance.CoinInScale; // ContentModel.Instance.totalBet


        public void ParseJackpotGame(SBoxJackpotData sboxJackpotData)
        {
            bool isJackpotGrand = sboxJackpotData == null ? false :
            (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 ? sboxJackpotData.Lottery[0] == 1 : false);

            bool isJackpotMajor = sboxJackpotData == null ? false :
                (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 ? sboxJackpotData.Lottery[1] == 1 : false);

            JackpotRes jpGameRes = new JackpotRes();
            ContentModel.Instance.jpGameRes = jpGameRes;

            jpGameRes.curJackpotGrand = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMinior = 1000;
            jpGameRes.curJackpotMini = 500;
        }


        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {

            SBoxGameState gameState = (SBoxGameState)((int)res["gameState"]);

            List<int> LineNumbers = new List<int>();
            int lineMark  = (int)res["lineMark"];

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
                    LineNumber = lineNumber,
                    SymbolNumber = symbolNumber,
                    WinCount = hitCount
                });
            }

            bool isJackpotGrand = sboxJackpotData == null ? false :
                (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 ? sboxJackpotData.Lottery[0] == 1 : false);

            bool isJackpotMajor = sboxJackpotData == null ? false :
                (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 ? sboxJackpotData.Lottery[1] == 1 : false);

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
            ContentModel.Instance.jpGameRes = jpGameRes;

            jpGameRes.curJackpotGrand = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[0] :0;
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ?  sboxJackpotData.JackpotOut[1] : 0;
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
                        count = 5; break;
                    case 9:
                        count = 4; break;
                    case 6:
                    default:
                        count = 3; break;
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
            ContentModel.Instance.strDeckRowCol = strDeckRowCol;


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
                    freeSpinTotalTimes - ContentModel.Instance.freeSpinTotalTimes;
            }
            else
                ContentModel.Instance.freeSpinAddNum = 0;

            ContentModel.Instance.showFreeSpinRemainTime = isFreeSpin
                ? (ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes - 1)
                : 0;

            ContentModel.Instance.freeSpinTotalTimes = freeSpinTotalTimes;
            ContentModel.Instance.freeSpinPlayTimes = freeSpinPlayTimes;
            ContentModel.Instance.isFreeSpinTrigger = isFreeSpinTrigger;
            ContentModel.Instance.isFreeSpinResult = isFreeSpinResult;
            ContentModel.Instance.isBonus1 = isBonusBall;
            ContentModel.Instance.hitBallCount = hitBallCount;
            ContentModel.Instance.isHitJackpotGame = isJackpotGrand || isJackpotMajor || isJackpotMinMinor;

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
            //long creditAfter = afterBetCredit + totalEarnCoins;

            /*
            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }*/

            /*
            // 记录最终计算结果
            DebugUtils.LogWarning(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCoins} ");
            DebugUtils.LogWarning($"本次计算 creditAfter= {afterBetCredit + totalEarnCoins}；  算法卡 creditAfter={creditAfter}");
            */


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
            bool isReelsSlowMotion = false;
            ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;

            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();
            ContentModel.Instance.bonusResults = bonusResult;
            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;

            //SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance?.targetSlotGameEffect ?? SlotGameEffect.Default);


            if (ContentModel.Instance.isFreeSpin)
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
            }
            else if(ContentModel.Instance.isAuto || ContentModel.Instance.totalPlaySpins >1)
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.AutoSpin);
            }
            else
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
            }
        }

        int moreTurn = 0;
        public void TestRecord()
        {
            if(ContentModel.Instance.isFreeSpinTrigger || ContentModel.Instance.isFreeSpin)
            {
                moreTurn = 3;  // 多保留2局
                DebugUtils.Save((ContentModel.Instance.isFreeSpinTrigger?"免费游戏触发局":"免费游戏") +  $" game guid:{ContentModel.Instance.curGameGuid} "  +  ContentModel.Instance.response);
            }else if (--moreTurn>0)
            {
                DebugUtils.Save("免费游戏后" + $" game guid:{ContentModel.Instance.curGameGuid} " + ContentModel.Instance.response);
            }
            else
            {
                moreTurn = 0;
            }

            if (ContentModel.Instance.isHitJackpotGame)
            {
                DebugUtils.Save("中游戏彩金" + $" game guid:{ContentModel.Instance.curGameGuid} " + ContentModel.Instance.response);
            }
        }
        public void Record()
        {

            // 游戏场景记录
            PusherMaker.GameSenceData gameSenceData = new PusherMaker.GameSenceData();

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

            gameSenceData.winFreeSpinTrigger = null; // ContentModel.Instance.winFreeSpinTriggerOrAddCopy;
            gameSenceData.winList = ContentModel.Instance.winList;
            gameSenceData.freeSpinPlayTimes = ContentModel.Instance.freeSpinPlayTimes;
            gameSenceData.freeSpinTotalTimes = ContentModel.Instance.freeSpinTotalTimes;
            gameSenceData.freeSpinTotalWinCoins = ContentModel.Instance.freeSpinTotalWinCoins;
            gameSenceData.totalBet = TotalBet;
            gameSenceData.creditPerCoinIn = SBoxModel.Instance.CoinInScale;
            gameSenceData.jackpotWinCoins = 0;  //【外设彩金-需要修改】
            gameSenceData.baseGameWinCoins = ContentModel.Instance.baseGameWinCoins;


            TablePusherGameRecordItem slotGameRecordItem = new TablePusherGameRecordItem()
            {
                game_type = ContentModel.Instance.isFreeSpin ? "free_spin" :
                    ContentModel.Instance.isFreeSpinTrigger ? "free_spin_trigger" :
                    ContentModel.Instance.isBonus1? "bonus1" :
                    "spin",
                game_id = ConfigUtils.curGameId,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet =  TotalBet ,
                credit_per_coin_in = SBoxModel.Instance.CoinInScale,
            };

            // 本剧数据存入数据库
            slotGameRecordItem.base_game_win_coins = 0; //gameSenceData.baseGameWinCredit;


            // 彩金数据
            JackpotRes info = ContentModel.Instance.jpGameRes;


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

                slotGameRecordItem.jackpot_win_coins = winJPCredit;
                slotGameRecordItem.jackpot_type = item.name;
                gameSenceData.jackpotWinCoins = winJPCredit;


                // 游戏彩金记录
                TableJackpotRecordAsyncManager.Instance.AddJackpotRecord(item.id, item.name, winJPCredit, -1, -1, ContentModel.Instance.curGameGuid, ContentModel.Instance.curGameCreatTimeMS);

                // 额外奖上报(暂时不用)
                //#seaweed# DeviceBonusReport.Instance.ReportBonus(item.name, item.name, winJPCredit, -1, (msg) => { }, (err) => { });

            }

            // 每日营收统计(暂时不用)
            /*TableBusniessDayRecordAsyncManager.Instance.AddTotalBetWin(
                ContentModel.Instance.curReelStripsIndex == "FS" ? 0 : TotalBet,
             ContentModel.Instance.baseGameWinCoins + gameSenceData.jackpotWinCoins, SBoxModel.Instance.myCredit);
            */


            ContentModel.Instance.totalEarnCoins = ContentModel.Instance.baseGameWinCoins + gameSenceData.jackpotWinCoins;


            slotGameRecordItem.scene = JsonConvert.SerializeObject(gameSenceData);
            string sql = SQLiteAsyncHelper.SQLInsertTableData<TablePusherGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD, slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

        }




        public void Report()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;
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
                    ["total_times"] = ContentModel.Instance.freeSpinTotalTimes,
                };
                NetCmdManager.Instance.RpcUpReportWin(req);
            }

            if (ContentModel.Instance.isFreeSpinResult)
            {
                Dictionary<string, object> req = new Dictionary<string, object>()
                {
                    ["type"] = "FreeSpinResult",
                    ["game_number"] = ContentModel.Instance.freeSpinTriggerGuid,
                    ["total_times"] = ContentModel.Instance.freeSpinTotalTimes,
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
            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_GM_EVENT,OnGMEvent);
        }

        void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_GM_EVENT,OnGMEvent);
        }

        void OnGMEvent(EventData res)
        {
            if (ApplicationSettings.Instance.isMock == false) return;
            
            if(res.id != 200) return;

            switch (res.name)
            {
                case GlobalEvent.GMSingleWinLine:
                    nextSpin = SpinDataType.SingleWinLine;
                    break;
                case GlobalEvent.GMMultipleWinLine:
                    nextSpin = SpinDataType.MultipleWinLine;
                    break;
                case GlobalEvent.GMFreeSpin:
                    nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMBigWin:
                    nextSpin = SpinDataType.BigWin;
                    break;
                case GlobalEvent.GMJp1:
                    //nextSpin = SpinDataType.Jp1;
                    GlobalJackpotConsole.NetClientHelper02.Instance.testIsHitJpGrandNext = true;
                    break;
                case GlobalEvent.GMJp2:
                    //nextSpin = SpinDataType.Jp2;
                    GlobalJackpotConsole.NetClientHelper02.Instance.testIsHitJpMajorNext = true;
                    break;
                case GlobalEvent.GMJp3:
                    nextSpin = SpinDataType.Jp3;
                    break;
                case GlobalEvent.GMJp4:
                    nextSpin = SpinDataType.Jp4;

                    break;
                case GlobalEvent.GMJpOnline:
                    //nextSpin = SpinDataType.JpOnline;
                    MachineDataManager02.Instance.testIsHitJackpotOnLine = true;
                    break;
                case GlobalEvent.GMBonus1:
                    nextSpin = SpinDataType.Bonus1Ball;
                    break;
            }
            
        }


        SpinDataType nextSpin = SpinDataType.None;


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
        };

        private Dictionary<SpinDataType, List<string[]>> spinDatas = new Dictionary<SpinDataType, List<string[]>>()
        {
            [SpinDataType.FreeSpin] = new List<string[]>()
            {
               /*new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_6.json",
                },*/

               new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_6.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_7.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_8.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_9.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_10.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_11.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__20251030__free_12.json",
                },


               
            },
            [SpinDataType.Normal] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__null_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__win_1.json" },//单线
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__win_2.json" },//多线
            },
            [SpinDataType.SingleWinLine] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__win_1.json" },//单线
            },
            [SpinDataType.MultipleWinLine] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__win_2.json" },//多线
            },
            [SpinDataType.Jp1] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__jackpot_grand.json" },
            },
            [SpinDataType.Jp2] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__jackpot_major.json" },
            },
            [SpinDataType.Jp3] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__jackpot_minor.json" },
            },
            [SpinDataType.Jp4] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__jackpot_mini.json" },
            },
            [SpinDataType.Bonus1Ball] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__ball_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__ball_1.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__ball_2.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__ball_3.json" }
            },
            [SpinDataType.BigWin] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g200_real/g200__slot_spin__Bigwin_0.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g200_real/g200__slot_spin__Bigwin_1.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g200_real/g200__slot_spin__Bigwin_2.json" },
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g200_real/g200__slot_spin__Bigwin_3.json" },
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
                    curDatas = new Queue<string>(strs);  // 会改变引用数据  
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