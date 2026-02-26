using GameMaker;
using GameUtil;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SlotEmperorsRein
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

    public class MachineDataG300Controller : MonoSingleton<MachineDataG300Controller>
    {
        public List<SymbolInclude> symbolInclude;

        public void ParseSlotSpin(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            SBoxGameState gameState = (SBoxGameState)((int)res["gameState"]);

            List<int> LineNumbers = new List<int>();
            int lineMark = (int)res["lineMark"];

            // lineMark /10000 / 100  // 3个线的数据

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
                //int credit = node["reward"];
                int lineNumber = LineNumbers[i];

                winningLines.Add(new WinningLineInfo()
                {
                    LineNumber = lineNumber,
                    SymbolNumber = symbolNumber,
                    WinCount = hitCount,
                });
            }



            bool isJackpotGrand = sboxJackpotData == null ? false : sboxJackpotData.Lottery[0] == 1;

            bool isJackpotMajor = sboxJackpotData == null ? false : sboxJackpotData.Lottery[1] == 1;

            bool isJackpotMinMinor = gameState == SBoxGameState.GSJpSmalm;


            bool isBonusBall = gameState == SBoxGameState.GSBonus;

            int freeSpinTotalTimes = (int)res["maxRound"];
            int freeSpinPlayTimes = (int)res["curRound"];

            bool isFreeSpinTrigger = freeSpinPlayTimes == 0 && freeSpinTotalTimes > 0;

            bool isFreeSpinResult = freeSpinTotalTimes > 0 && freeSpinPlayTimes == freeSpinTotalTimes;

            bool isFreeSpin = freeSpinPlayTimes > 0 && freeSpinTotalTimes > 0;

            string strDeckRowCol = "";



            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();

            JackpotRes jpGameRes = new JackpotRes();
            ContentModel.Instance.jpGameRes = jpGameRes;
            jpGameRes.curJackpotGrand = sboxJackpotData.JackpotOut[0];
            jpGameRes.curJackpotMajor = sboxJackpotData.JackpotOut[1];
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

                /*
                List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
                for (int i = 0; i < 5; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 12 });
                }
                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines, //ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);*/
            }

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

                if (winCredit == 1000)
                {
                    jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                    {
                        name = "minor",
                        id = 3,
                        winCredit = 1000,
                        whenCredit = 1000,
                        curCredit = 1000,
                    });
                }
                else if (winCredit == 500)
                {
                    jpGameRes.jpWinLst.Add(new JackpotWinInfo()
                    {
                        name = "mini",
                        id = 4,
                        winCredit = 500,
                        whenCredit = 500,
                        curCredit = 500,
                    });
                }


                if (!isJackpotGrand && !isJackpotMajor)
                {
                    symbolInclude = new List<SymbolInclude>();
                    for (int i = 0; i < 3; i++)
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

                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 11 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }
            else if (isFreeSpinTrigger)
            {

                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 10 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }
            else
            {
                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 11, 10, 12 }, symbolInclude);
            }

            long creditBefore = MainBlackboardController.Instance.myRealCredit;

            long afterBetCredit = 0;
            long totalEarnCredit = 0;

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
                int credit = lineNode["reward"];
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



            if (isFreeSpin && freeSpinTotalTimes != 0)
            {
                ContentModel.Instance.freeSpinAddNum =
                    freeSpinTotalTimes - ContentModel.Instance.freeSpinTotalTimes;
            }
            else
                ContentModel.Instance.freeSpinAddNum = 0;


            if (isFreeSpin)
            {
                ContentModel.Instance.showFreeSpinRemainTime = ContentModel.Instance.freeSpinTotalTimes -
                                                               ContentModel.Instance.freeSpinPlayTimes - 1;
            }
            else
            {
                ContentModel.Instance.showFreeSpinRemainTime = 0;
            }


            ContentModel.Instance.freeSpinTotalTimes = freeSpinTotalTimes;
            ContentModel.Instance.freeSpinPlayTimes = freeSpinPlayTimes;


            ContentModel.Instance.isFreeSpinTrigger = isFreeSpinTrigger;
            ContentModel.Instance.isFreeSpinResult = isFreeSpinResult;

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

            ContentModel.Instance.totalEarnCredit = totalEarnCredit;
            ContentModel.Instance.baseGameWinCredit = totalEarnCredit;


            //ContentModel.Instance.winFreeSpinTriggerOrAddCopy = null;


            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            ContentModel.Instance.curGameGuid = isFreeSpin
                ? $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}-{ContentModel.Instance.gameNumberFreeSpinTrigger}"
                : $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}";


            if (!isFreeSpin)
            {
                afterBetCredit = creditBefore - totalBet;
            }
            else
            {
                afterBetCredit = creditBefore;
            }
            // ## MainBlackboardController.Instance.SetMyTempCredit(afterBetCredit, false); // 不同步给ui



            long creditAfter = afterBetCredit + totalEarnCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }
            // ## MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.LogWarning(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            DebugUtils.LogWarning($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            if (!isFreeSpin)
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
            /*
            if (res["contents"]["bonus_result"] != null && res["contents"]["bonus_result"].Count > 0)
            {
               foreach (JSONNode item in res["contents"]["bonus_result"])
               {
                   bonusResult.Add((int)item["bonus_id"],item);
               }
            }*/
            ContentModel.Instance.bonusResult = bonusResult;


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

        /*
        public void Record()
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

            gameSenceData.winFreeSpinTrigger = ContentModel.Instance.winFreeSpinTriggerOrAddCopy;
            gameSenceData.winList = ContentModel.Instance.winList;
            gameSenceData.freeSpinPlayTimes = ContentModel.Instance.freeSpinPlayTimes;
            gameSenceData.freeSpinTotalTimes = ContentModel.Instance.freeSpinTotalTimes;
            gameSenceData.freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            gameSenceData.totalBet = ContentModel.Instance.totalBet;
            gameSenceData.creditBefore = ContentModel.Instance.creditBefore;
            gameSenceData.creditAfter = ContentModel.Instance.creditAfter; // 这是基础游戏+彩金【外设彩金-需要修改】
            gameSenceData.jackpotWinCredit = 0;  //【外设彩金-需要修改】
            gameSenceData.baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;


            TableSlotGameRecordItem slotGameRecordItem = new TableSlotGameRecordItem()
            {
                game_type = ContentModel.Instance.isFreeSpin ? "free_spin" : ContentModel.Instance.isFreeSpinTrigger ? "free_spin_trigger" : "spin",
                game_id = ConfigUtils.curGameId,
                game_uid = ContentModel.Instance.curGameGuid,
                created_at = ContentModel.Instance.curGameCreatTimeMS,
                total_bet = ContentModel.Instance.totalBet,
                credit_before = gameSenceData.creditBefore,
            };

            // 本剧数据存入数据库
            slotGameRecordItem.credit_after = gameSenceData.creditAfter;  //【外设彩金-需要修改】
            slotGameRecordItem.base_game_win_credit = gameSenceData.baseGameWinCredit;



            // 彩金数据
            JackpotRes info = ContentModel.Instance.jackpotRes;


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

                slotGameRecordItem.jackpot_win_credit = winJPCredit;
                gameSenceData.jackpotWinCredit = winJPCredit;


                long creditBefore = ContentModel.Instance.creditAfter;
                long creditAfter = ContentModel.Instance.creditAfter += winJPCredit;

                ContentModel.Instance.creditAfter = creditAfter;
                gameSenceData.creditAfter = creditAfter;
                slotGameRecordItem.credit_after = creditAfter;


                // 通知算法卡
                MachineDataManager.Instance.NotifyGameJackpot(winJPCredit);
                SBoxModel.Instance.myCredit += winJPCredit;


                // 游戏彩金记录
                TableJackpotRecordAsyncManager.Instance.AddJackpotRecord(item.id, item.name, winJPCredit,
                creditBefore, creditAfter,
                ContentModel.Instance.curGameGuid, ContentModel.Instance.curGameCreatTimeMS);


                // 额外奖上报
                DeviceBonusReport.Instance.ReportBonus(item.name, item.name, winJPCredit, -1, (msg) => { }, (err) => { });

            }

            // 每日营收统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalBetWin(
                ContentModel.Instance.curReelStripsIndex == "FS" ? 0 : ContentModel.Instance.totalBet,
             ContentModel.Instance.baseGameWinCredit + gameSenceData.jackpotWinCredit, SBoxModel.Instance.myCredit);


            ContentModel.Instance.totalEarnCredit = ContentModel.Instance.baseGameWinCredit + gameSenceData.jackpotWinCredit;


            slotGameRecordItem.scene = JsonConvert.SerializeObject(gameSenceData);
            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD, slotGameRecordItem);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);


          
            //try
            //{
            //    // 数据数据上报
            //    string str = ReportDataUtils.CreatReportData(gameSenceData, SBoxModel.Instance.sboxPlayerInfo);
            //    DebugUtils.Log($"数据上报成功 {str}");
            //    ReportManager.Instance.SendData(str, null, null);
            //}
            //catch (Exception ex) { }
            

        }
        */

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

            if (res.id != 200) return;

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
               new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__free_6.json",
                },
            },
            [SpinDataType.Normal] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__null_0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__win_1.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g200_real/g200__slot_spin__win_2.json" },
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
