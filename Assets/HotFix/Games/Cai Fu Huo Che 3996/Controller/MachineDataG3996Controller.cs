using GameMaker;
using GameUtil;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaiFuHuoChe_3996
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
        /// <summary> 彩金游戏 </summary>
        GSJpGame = 5,

        GSOperater = 9
    }

    public class MachineDataG3996Controller : MonoSingleton<MachineDataG3996Controller>
    {
        public List<SymbolInclude> jackpotSymbolInclude = new List<SymbolInclude>();

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

            bool isFreeSpinTrigger = false;
            bool isFreeSpinResult = false;
            bool isFreeSpin = false;
            int freeSpinTotalTimes = 0;
            int freeSpinPlayTimes = 0;

            if (gameState == SBoxGameState.GSFreeGame)
            {
                freeSpinTotalTimes = (int)res["maxRound"];
                freeSpinPlayTimes = (int)res["curRound"];

                isFreeSpinTrigger = freeSpinPlayTimes == 0 && freeSpinTotalTimes > 0;

                isFreeSpinResult = freeSpinTotalTimes > 0 && freeSpinPlayTimes == freeSpinTotalTimes;

                isFreeSpin = freeSpinPlayTimes > 0 && freeSpinTotalTimes > 0;
            }
            

            bool isJackpotSpinTrigger = false;
            bool isJackpotSpinResult = false;
            bool isJackpotSpin = false;
            int jackpotSpinTotalTimes = 0;
            int jackpotSpinPlayTimes = 0;

            if (gameState == SBoxGameState.GSJpGame)
            {
                jackpotSpinTotalTimes = (int)res["maxRound"];
                jackpotSpinPlayTimes = (int)res["curRound"];

                isJackpotSpinTrigger = jackpotSpinPlayTimes == 0 && jackpotSpinTotalTimes > 0;

                isJackpotSpinResult = jackpotSpinTotalTimes > 0 && (jackpotSpinPlayTimes == jackpotSpinTotalTimes || jackpotSymbolInclude.Count >= 15);

                isJackpotSpin = jackpotSpinTotalTimes > 0 && jackpotSpinTotalTimes > 0 && jackpotSymbolInclude.Count < 15;
            }
            
            string strDeckRowCol = "";


            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();

            JackpotRes jpGameRes = new JackpotRes();
            ContentModel.Instance.jpGameRes = jpGameRes;
            jpGameRes.curJackpotGrand = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[1] : 0;
            jpGameRes.curJackpotMinior = 1000;
            jpGameRes.curJackpotMini = 500;


            if (isJackpotSpinTrigger)
            {
                for(int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 11 });
                }

                jackpotSymbolInclude.Clear();
                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 9, 11, 10, 12 }, symbolInclude);
            }
            else if (isJackpotSpin)
            {
                foreach(int key in ContentModel.Instance.itemPos.Keys)
                {
                    ContentModel.Instance.itemPos[key].Clear();
                }

                if(res["jackpotNums"] != null)
                {
                    int jackpotNum = (int)res["jackpotNums"];
                    while (jackpotNum > 0)
                    {
                        int temp = jackpotNum % 100;
                        symbolInclude.Add(new SymbolInclude() { symbolNumber = temp });
                        jackpotNum = jackpotNum / 100;
                    }
                }
                
                strDeckRowCol = GenerateGameArray(
                   ContentModel.Instance.payLines,
                   CustomModel.Instance.symbolNumber, winningLines, new int[] { 9, 11, 10, 12 }, symbolInclude, jackpotSymbolInclude);
            }
            else if (isFreeSpinTrigger)
            {

                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 10 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] {9, 11, 10, 12}, symbolInclude);
            }
            else if (isFreeSpin)
            {
                for (int i = 0; i < (int)res["num"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 10 });
                }

                for(int i = 0; i < (int)res["wildNum"]; i++)
                {
                    symbolInclude.Add(new SymbolInclude() { symbolNumber = 9 });
                }

                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 9, 11, 10, 12 }, symbolInclude);
            }
            else
            {
                strDeckRowCol = AdvancedLineGenerator.Instance.GenerateGameArray(
                    ContentModel.Instance.payLines,
                    CustomModel.Instance.symbolNumber, winningLines, new int[] { 9, 11, 10, 12 }, symbolInclude);
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

            if (isJackpotSpin)
            {
                ContentModel.Instance.showJackpotSpinRemainTime = ContentModel.Instance.jackpotSpinTotalTimes -
                                                               ContentModel.Instance.jackpotSpinPlayTimes - 1;
            }
            else
            {
                ContentModel.Instance.showJackpotSpinRemainTime = 0;
            }


            ContentModel.Instance.freeSpinTotalTimes = freeSpinTotalTimes;
            ContentModel.Instance.freeSpinPlayTimes = freeSpinPlayTimes;


            ContentModel.Instance.isFreeSpinTrigger = isFreeSpinTrigger;
            ContentModel.Instance.isFreeSpinResult = isFreeSpinResult;


            ContentModel.Instance.jackpotSpinTotalTimes = jackpotSpinTotalTimes;
            ContentModel.Instance.jackpotSpinPlayTimes = jackpotSpinPlayTimes;


            ContentModel.Instance.isJackpotSpinTrigger = isJackpotSpinTrigger;
            ContentModel.Instance.isJackpotSpinResult = isJackpotSpinResult;


            if (isJackpotSpinTrigger)
            {
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "JS";
            }
            else if (isJackpotSpinResult)
            {
                ContentModel.Instance.curReelStripsIndex = "JS";
                ContentModel.Instance.nextReelStripsIndex = "BS";
            }
            else if (isJackpotSpin)
            {
                ContentModel.Instance.curReelStripsIndex = "JS";
                ContentModel.Instance.nextReelStripsIndex = "JS";
            }
            else if (isFreeSpinTrigger)
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

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            DebugUtils.Log($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");


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
            if (isFreeSpinTrigger)
            {
                bool isReelsSlowMotion = ((int)res["num"] > 2) ? true : false;
                ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            }

            if (isJackpotSpinTrigger)
            {
                bool isReelsSlowMotion = ((int)res["num"] > 2) ? true : false;
                ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            }


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


        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            if(ContentModel.Instance.nextReelStripsIndex == "JS")
            {
                nextSpin = SpinDataType.JpSpin;
                ParseSlotSpin(totalBet, res, sboxJackpotData);
                return;
            }

            bool isJackpotGrand = sboxJackpotData == null ? false : sboxJackpotData.Lottery[0] == 1;
            bool isJackpotMajor = sboxJackpotData == null ? false : sboxJackpotData.Lottery[1] == 1;
            bool isJackpotMinMinor = sboxJackpotData == null ? false : sboxJackpotData.Lottery[2] == 1;

            //IDVec
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

                int lineIndex = lineNumber - 1;
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

            //Matrix
            int rows = 3;    // 3行
            int cols = 5;    // 5列
            string strDeckRowCol = "";
            int MatrixLength = (int)res["MatrixLength"];
            int freeGameIconNum = 0;
            int jackpotGameIconNum = 0;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;
                    strDeckRowCol += res["Matrix"][index].Value;
                    if (int.Parse(res["Matrix"][index].Value) == 10)
                    {
                        freeGameIconNum++;
                    }
                    else if(int.Parse(res["Matrix"][index].Value) == 11)
                    {
                        jackpotGameIconNum++;
                    }


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

            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
            JackpotRes jpGameRes = new JackpotRes();
            ContentModel.Instance.jpGameRes = jpGameRes;
            jpGameRes.curJackpotGrand = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1 ? sboxJackpotData.JackpotOut[0] : 0;
            jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2 ? sboxJackpotData.JackpotOut[1] : 0;
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
            }
            long creditBefore = MainBlackboardController.Instance.myRealCredit;
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
            ContentModel.Instance.isJackpotSpinTrigger = false;

            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;
                ContentModel.Instance.freeSpinTotalTimes = 2 * freeGameIconNum + 2;
                ContentModel.Instance.freeSpinPlayTimes = 0;
                ContentModel.Instance.newFreeOnceCredit.Clear();
                for (int i = 0; i < TotalFreeTime; i++)
                {
                    ContentModel.Instance.newFreeOnceCredit.Add((int)res["FreeBetArray"][i]);
                }
            }
            else if(ResultType == 3 && ResultType == 3)
            {
                Debug.Log("-------彩金奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "JS";

                ContentModel.Instance.isJackpotSpinTrigger = true;
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
            long afterBetCredit = 0;
            if (OpenType == 1)
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

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            DebugUtils.Log($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");


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

            if (res.id != 3996) return;

            switch (res.name)
            {
                case GlobalEvent.GMFreeSpin:
                    nextSpin = SpinDataType.FreeSpin;
                    break;
                case GlobalEvent.GMBigWin:
                    nextSpin = SpinDataType.BigWin;
                    break;
                case GlobalEvent.GMJpOnline:
                    //nextSpin = SpinDataType.JpOnline;
                    break;
                case GlobalEvent.GMJp1:
                    nextSpin = SpinDataType.JpSpin;
                    break;
            }

        }


        SpinDataType nextSpin = SpinDataType.Normal;


        enum SpinDataType
        {
            None,
            Normal,
            FreeSpin,
            BigWin,
            JpSpin,
            JpOnline,
            Bonus1Ball,
        };

        private Dictionary<SpinDataType, List<string[]>> spinDatas = new Dictionary<SpinDataType, List<string[]>>()
        {
            [SpinDataType.FreeSpin] = new List<string[]>()
            {
               new string[]
                {
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_0.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_1.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_2.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_3.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_4.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_5.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_6.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_7.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_8.json",
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin_free_9.json",
                },
            },
            [SpinDataType.Normal] = new List<string[]>()
            {
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__win0.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__win1.json" },
                new string[] { "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__win2.json" },
            },
            [SpinDataType.JpSpin] = new List<string[]>()
            {
                new string[]
                { 
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__jackpot0.json" ,
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__jackpot1.json" ,
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__jackpot2.json" ,
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__jackpot3.json" ,
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__jackpot4.json" ,
                    "Assets/HotFix/Games/Mock/Resources/g3996_real/g3996__slot_spin__jackpot5.json" ,
                }
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
                new string[] { "Assets/HotFix/Games/_Mock/Resources/g3996_real/g3996__slot_spin__Bigwin_0.json" },
                //new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_1.json" },
                //new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_2.json" },
                //new string[] { "Assets/HotFix/Games/_Mock/Resources/g1700_real/g200__slot_spin__Bigwin_3.json" },
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


        #region 可以对之前的转盘图标进行存储和其他操作，对 GenerateGameArray 进行了重载操作

        public List<List<int>> gameResultList = new List<List<int>>
        {
            new List<int>(new int[5]), // 第一行
            new List<int>(new int[5]), // 第二行
            new List<int>(new int[5]) // 第三行
        };

        public string strDeckRowCol;
        public string GenerateGameArray(List<List<int>> allLines, List<int> symbolNumber, List<WinningLineInfo> winningLines, int[] exclude, List<SymbolInclude> include, List<SymbolInclude> jackpotInclude)
        {
            if (winningLines == null)
                winningLines = new List<WinningLineInfo>();
            // 初始化游戏结果矩阵
            gameResultList = new List<List<int>>();
            for (int raw = 0; raw < 3; raw++)
            {
                // 为每行创建一个包含5个0的 List<int>，避免空引用
                List<int> row = new List<int>();
                for (int col = 0; col < 5; col++)
                {
                    row.Add(-1);
                }

                gameResultList.Add(row); // 将行添加到矩阵中
            }

            List<int> excludeLst = new List<int>();
            excludeLst.AddRange(exclude);

            foreach (WinningLineInfo item in winningLines)
            {
                excludeLst.Add(item.SymbolNumber);

                int lineIndex = item.LineNumber - 1;

                List<int> line = allLines[lineIndex];

                for (int cIndex = 0; cIndex < item.WinCount; cIndex++)
                {
                    int rIndex = line[cIndex];
                    gameResultList[rIndex][cIndex] = item.SymbolNumber;
                }
            }

            //先将已经存在的彩金图标先放入
            foreach(SymbolInclude symbolInclude in jackpotInclude)
            {
                gameResultList[symbolInclude.rowIdx][symbolInclude.colIdx] = symbolInclude.symbolNumber;
            }

            //将新加入的彩金元素随机放入位置并记录
            foreach (SymbolInclude symbolInclude in include)
            {
                int colIdx = symbolInclude.colIdx;
                int rowIdx = symbolInclude.rowIdx;
                int endlessLoop = 1000;
                if (colIdx == -1 && rowIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }
                else if (colIdx == -1)
                {
                    do
                    {
                        colIdx = UnityEngine.Random.Range(0, 5);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }
                else if (rowIdx == -1)
                {
                    do
                    {
                        rowIdx = UnityEngine.Random.Range(0, 3);
                    } while (gameResultList[rowIdx][colIdx] != -1 && --endlessLoop >= 0);
                }

                if (endlessLoop < 0)
                    DebugUtils.LogError($"【endless loop】: when add include symbol");

                symbolInclude.rowIdx = rowIdx;
                symbolInclude.colIdx = colIdx;

                gameResultList[rowIdx][colIdx] = symbolInclude.symbolNumber;

                if (!ContentModel.Instance.itemPos.ContainsKey(colIdx))
                {
                    ContentModel.Instance.itemPos[colIdx] = new List<int>();
                }
                ContentModel.Instance.itemPos[colIdx].Add(rowIdx);

                jackpotInclude.Add(symbolInclude);
            }

            for (int i = 0; i < 3; i++)
            {
                if (gameResultList[i][2] == -1)
                {
                    int middleSymbolNumber = -1;
                    int endlessLoop = 1000;

                    do
                    {
                        int symbolIdx = UnityEngine.Random.Range(0, symbolNumber.Count);
                        middleSymbolNumber = symbolNumber[symbolIdx];
                    } while (excludeLst.Contains(middleSymbolNumber) && --endlessLoop >= 0);

                    if (endlessLoop < 0)
                    {
                        DebugUtils.LogError($"【endless loop】: when add middle col symbol");
                    }

                    excludeLst.Add(middleSymbolNumber);

                    gameResultList[i][2] = middleSymbolNumber;
                }
            }


            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (gameResultList[i][j] == -1)
                    {
                        int tempSymbolNumber = -1;
                        int endlessLoop = 1000;
                        do
                        {
                            int symbolIdx = UnityEngine.Random.Range(0, symbolNumber.Count);
                            tempSymbolNumber = symbolNumber[symbolIdx];
                        } while (excludeLst.Contains(tempSymbolNumber) && --endlessLoop >= 0);

                        if (endlessLoop < 0)
                        {
                            DebugUtils.LogError($"【endless loop】: when add remain symbol");
                        }
                        gameResultList[i][j] = tempSymbolNumber;
                    }
                }
            }

            string strDeckRowCol = SlotTool.GetDeckColRow(gameResultList);
            return strDeckRowCol;

        }
        #endregion
    }
}
