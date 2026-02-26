using GameUtil;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameMaker;
using SlotMaker;
using SBoxApi;


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
        public List<SymbolInclude> symbolInclude;

        public void ParseSlotSpinMachine(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            //Matrix解析整列
            int rows = 3;    // 3行
            int cols = 5;    // 5列
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
                        strDeckRowCol += ",";  // 列之间用逗号分隔
                    }
                }

                if (row < rows - 1)
                {
                    strDeckRowCol += "#";  // 行之间用#号分隔
                }
            }
            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            //IDVec 中奖线
            int lineNum = (int)res["lineNum"];
            int totalEarnCredit = 0;
            int credit = 0;
            List<SymbolWin> winList = new List<SymbolWin>();
            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100;          // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10;       // 百位：消除数量（WinCount）
                int lineNumber = ID / 1000;           // 万千位：线编号

                // 输出调试信息（可选）
                Debug.Log($"ID: {ID}, Line: {lineNumber}, HitCount: {hitCount}, Symbol: {symbolNumber}");

                int lineIndex = lineNumber;
                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();
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

            //判断彩金
            JackpotRes jpGameRes = new JackpotRes();
            bool isJackpotMajor = sboxJackpotData == null ? false :(sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 ? sboxJackpotData.Lottery[0] == 1 : false);
            bool isJackpotMinor = sboxJackpotData == null ? false :(sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 ? sboxJackpotData.Lottery[1] == 1 : false);
            bool isJackpotMini = sboxJackpotData == null ? false :(sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 2 ? sboxJackpotData.Lottery[2] == 1 : false);
          
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 0 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMinior = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMini = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[2] : 0;
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
            int TotalFreeTime = (int)res["TotalFreeTime"];
            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.freeSpinTotalTimes = TotalFreeTime;
                ContentModel.Instance.freeSpinPlayTimes = 0;
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
                ContentModel.Instance.freeSpinPlayTimes += 1;
                if (ContentModel.Instance.freeSpinTotalTimes == ContentModel.Instance.freeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
            }
            ContentModel.Instance.isFreeSpinResult = ContentModel.Instance.curReelStripsIndex == "FS" && ContentModel.Instance.nextReelStripsIndex == "BS";


            long creditBefore = MainBlackboardController.Instance.myTempCredit;
            //赢分
            long TotalBet = (int)res["TotalBet"];
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

            long creditAfter = afterBetCredit + totalEarnCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log($"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            if (OpenType == 1)
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
     
            ContentModel.Instance.bonusResult = bonusResult;

            ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Default;
            SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);
        }


        public void ParseSlotSpinMock(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            //Matrix解析整列
            int rows = 3;    // 3行
            int cols = 5;    // 5列
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
                        strDeckRowCol += ",";  // 列之间用逗号分隔
                    }
                }

                if (row < rows - 1)
                {
                    strDeckRowCol += "#";  // 行之间用#号分隔
                }
            }
            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            //IDVec 中奖线
            int lineNum = (int)res["lineNum"];
            int totalEarnCredit = 0;
            int credit = 0;
            List<SymbolWin> winList = new List<SymbolWin>();
            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100;          // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10;       // 百位：消除数量（WinCount）
                int lineNumber = ID / 1000;           // 万千位：线编号

                // 输出调试信息（可选）
                Debug.Log($"ID: {ID}, Line: {lineNumber}, HitCount: {hitCount}, Symbol: {symbolNumber}");

                int lineIndex = lineNumber;
                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();
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

            //判断彩金
            JackpotRes jpGameRes = new JackpotRes();
            bool isJackpotMajor = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 0 ? sboxJackpotData.Lottery[0] == 1 : false);
            bool isJackpotMinor = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 1 ? sboxJackpotData.Lottery[1] == 1 : false);
            bool isJackpotMini = sboxJackpotData == null ? false : (sboxJackpotData.Lottery != null && sboxJackpotData.Lottery.Length > 2 ? sboxJackpotData.Lottery[2] == 1 : false);

            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 0 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMinior = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMini = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[2] : 0;
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
            int TotalFreeTime = (int)res["TotalFreeTime"];
            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.freeSpinTotalTimes = TotalFreeTime;
                ContentModel.Instance.freeSpinPlayTimes = 0;
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
                ContentModel.Instance.freeSpinPlayTimes += 1;
                if (ContentModel.Instance.freeSpinTotalTimes == ContentModel.Instance.freeSpinPlayTimes)
                {
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                }
                else
                {
                    ContentModel.Instance.nextReelStripsIndex = "FS";
                }
            }


            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long creditBefore = MainBlackboardController.Instance.myTempCredit;
            //赢分
            long TotalBet = (int)res["TotalBet"];
            DebugUtils.Log("本局赢分TotalBet==" + TotalBet);
            long afterBetCredit = 0;
            if (OpenType == 1)
            {
                afterBetCredit = creditBefore+ TotalBet;
            }
            else
            {
                afterBetCredit = creditBefore + TotalBet;
               
            }
      
            long creditAfter = afterBetCredit + totalEarnCredit;

            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log($"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");

            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;
            if (OpenType == 1)
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
                    "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__free_3.json",
                },
            },
        [SpinDataType.Normal] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__win_4.json" },
            },
        [SpinDataType.Jp1] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_grand.json" },
            },
        [SpinDataType.Jp2] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_major.json" },
            },
        [SpinDataType.Jp3] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_minor.json" },
            },
        [SpinDataType.Jp4] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g1700_real/g200__slot_spin__jackpot_mini.json" },
            },
        [SpinDataType.Bonus1Ball] = new List<string[]>()
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
              
                if (nextSpin != SpinDataType.None)
                {
                    target = spinDatas[nextSpin];  // 使用指定的 spin 类型
                }
                else
                {
                    target = spinDatas[SpinDataType.Normal];  // 使用默认的 Normal 类型
                }

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


}
}