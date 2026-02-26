using GameUtil;
using SimpleJSON;
using PusherEmperorsRein;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameMaker;
using SlotMaker;

namespace PusherEmperorsRein
{


    public class MockDataG200Controller : Singleton<MockDataG200Controller>
    {

        public void ParseSlotSpin(long totalBet, JSONNode res)
        {

            long afterBetCredit = 0;

            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;


            string curStripsIndex = res["contents"]["current_strips_index"];
            string nextStripsIndex = res["contents"]["next_strips_index"];

            long totalEarnCredit = res["contents"]["total_win_credit"];
            long creditBefore = MainBlackboardController.Instance.myRealCredit;

            bool isFreeSpin = curStripsIndex == "FS";
            bool isFreeSpinTrigger = curStripsIndex == "BS" && nextStripsIndex == "FS";

            ContentModel.Instance.creditBefore = creditBefore;
            long creditAfter = isFreeSpin ? creditBefore + totalEarnCredit : creditBefore - totalBet + totalEarnCredit;

            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            ContentModel.Instance.creditAfter = creditAfter;

            if (!isFreeSpin)
            {
                afterBetCredit = creditBefore - totalBet;
                MainBlackboardController.Instance.SetMyTempCredit(afterBetCredit, false);
            }
            else
            {
                // 免费游戏金钱的算法:可能是7局免费游戏先算完，再回到主游戏接着算当前局的金额
                // MainBlackboardController.Instance.SetMyCredit(creditBefore, false); 
                afterBetCredit = creditBefore;
            }


            //#  TableBusniessDayRecordManager.Instance.AddTotalBetWin(curStripsIndex == "FS" ? 0 : totalBet, totalEarnCredit, creditAfter);



            ContentModel.Instance.response = res.ToString(); //BlackboardUtils.SetValue("./response", res.ToString());

            #region 数据解析

            //List<int> middleIndexList = JSONNodeUtil.GetList<int>(res, "result/rng");  // "result/rng" ： 是“中间行”不是“起始行”



            if (isFreeSpinTrigger)
                ContentModel.Instance.gameNumberFreeSpinTrigger = MainModel.Instance.gameNumber;


            ContentModel.Instance.isFreeSpinTrigger = isFreeSpinTrigger;
            ContentModel.Instance.isFreeSpinResult = curStripsIndex == "FS" && nextStripsIndex == "BS";

            //ContentModel.Instance.middleIndexList = middleIndexList;
            ContentModel.Instance.curReelStripsIndex = curStripsIndex;
            ContentModel.Instance.nextReelStripsIndex = nextStripsIndex;
            ContentModel.Instance.totalEarnCoins = totalEarnCredit;

            ContentModel.Instance.baseGameWinCoins = totalEarnCredit;

            try
            {
                // 为null时，使用上次的数值

                int multiplier =
                    int.Parse(res["contents"]["multiplier_alone"].ToString()); //"contents/multiplier_alone"
                ContentModel.Instance.multiplierAlone = multiplier;
            }
            catch (Exception ex)
            {
                // BlackboardUtils.SetValue("./multiplierAlone", 1);  
            }

            int freeSpinTotalTimes = isFreeSpinTrigger ? (int)res["contents"]["free_spin_info"]["total_spin_count"] : 0;
            int freeSpinPlayTimes = isFreeSpin ? (int)res["contents"]["free_spin_info"]["current_spin_count"] : 0;



            if (ContentModel.Instance.isFreeSpin && ContentModel.Instance.freeSpinTotalTimes != 0)
            {
                //freeSpinTotalTimes = ContentModel.Instance.freeSpinTotalTimes + 7;
                ContentModel.Instance.freeSpinAddNum = freeSpinTotalTimes - ContentModel.Instance.freeSpinTotalTimes;
            }
            else
                ContentModel.Instance.freeSpinAddNum = 0;

            ContentModel.Instance.freeSpinTotalTimes = freeSpinTotalTimes;
            ContentModel.Instance.freeSpinPlayTimes = freeSpinPlayTimes;


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            if (!isFreeSpin)
            {
                ContentModel.Instance.freeSpinTotalWinCoins = 0;
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCoins += totalEarnCredit;
                freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCoins;
            }

            // 当前码表
            ///ReelStrips curReelStrips = ContentModel.Instance.curStripsIndex;
            //ReelStrips curReelStrips = curStripsIndex == "FS" ? ContentModel.Instance.FS : ContentModel.Instance.BS;


            //画面
            string strDeckRowCol = (string)res["contents"]["deck_row_column"];
            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);



            #region 设置采用的滚轮转动数据

            bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;

            ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            /*
              if (isReelsSlowMotion)
                  ContentModel.Instance.customDataName = REEL_SETTING_SLOW_MOTION;
              else
                  ContentModel.Instance.customDataName = REEL_SETTING_REGULAR;
              */

            //ContentModel.Instance.customDataName = REEL_SETTING_REGULAR;



            // ContentModel.Instance.isReelsSlowMotion = false;

            #endregion

            ContentModel.Instance.strDeckRowCol = strDeckRowCol;

            JSONNode winLineGroup = res["contents"]["win_line_group"];
            List<SymbolWin> winList = new List<SymbolWin>();
            foreach (JSONNode nd in winLineGroup)
            {
                List<Cell> _cells = new List<Cell>();
                foreach (JSONNode pos in nd["pos_xy"])
                {
                    int number = pos; // 示例数字

                    int rowNumb = number % 10; // 个位数 ones (这里值是：行号)
                    int colNumb = (number / 10) % 10; // 十位数 tens  (这里值是：列号)

                    int rowIdx = rowNumb - 1;
                    int colIdx = colNumb - 1;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                SymbolWin sw = new SymbolWin()
                {
                    earnCredit = nd["credit"],
                    multiplier = nd["multiplier"],
                    lineNumber = (int)nd["line_no"],
                    symbolNumber = nd["symbol_id"],
                    cells = _cells,
                };

                winList.Add(sw);
            }

            ContentModel.Instance.winList = winList;

            #endregion

            //JSONNode bonusResult = res["contents"]["bonus_result"];
            //Dictionary<int,List<SymbolBonus>> BounsDic = new Dictionary<int, List<SymbolBonus>>();
            //foreach (JSONNode nd in bonusResult)
            //{
            //    List<Cell> _cells = new List<Cell>();
            //    foreach (JSONNode pos in nd["pos_xy"])
            //    {
            //        int number = pos; // 示例数字

            //        int rowNumb = number % 10; // 个位数 ones (这里值是：行号)
            //        int colNumb = (number / 10) % 10; // 十位数 tens  (这里值是：列号)

            //        int rowIdx = rowNumb - 1;
            //        int colIdx = colNumb - 1;
            //        _cells.Add(new Cell(colIdx, rowIdx));
            //    }

            //    SymbolBonus sw = new SymbolBonus()
            //    {
            //        Credit = nd["credit"],
            //        cells = _cells,
            //    };

            //    winList.Add(sw);
            //}

            ContentModel.Instance.winList = winList;

            ContentModel.Instance.winFreeSpinTriggerOrAddCopy = null;


            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            ContentModel.Instance.curGameGuid = isFreeSpin
                ? $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}-{ContentModel.Instance.gameNumberFreeSpinTrigger}"
                : $"{MainModel.Instance.gameID}-{UnityEngine.Random.Range(100, 1000)}-{ContentModel.Instance.curGameCreatTimeMS}-{MainModel.Instance.gameNumber}";


            // bonus数据
            var bonusResult = new Dictionary<int, JSONNode>();

            if (res["contents"]["bonus_result"] != null && res["contents"]["bonus_result"].Count > 0)
            {
                foreach (JSONNode item in res["contents"]["bonus_result"])
                {
                    bonusResult.Add((int)item["bonus_id"], item);
                }
            }

            ContentModel.Instance.bonusResults = bonusResult;



            if (ContentModel.Instance.bonusResults.Count > 0)
            {
                ContentModel.Instance.targetSlotGameEffect = SlotGameEffect.Expectation02;
            }
            else
            {
                ContentModel.Instance.targetSlotGameEffect = isReelsSlowMotion ? SlotGameEffect.Expectation01 :
                    isFreeSpin ? SlotGameEffect.FreeSpin : SlotGameEffect.Default;
            }

            SlotGameEffectManager.Instance.SetEffect(ContentModel.Instance.targetSlotGameEffect);
        }








        List<string[]> dataFreeSpin = new List<string[]>()
        {
            new string[]
            {
                "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__0.json",
                "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__free_spin__1.json",
                "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__free_spin__2.json",
                "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__free_spin__3.json"
            },
            new string[] { "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__2.json" },
        };

        List<string[]> datasSpin = new List<string[]>()
        {
            new string[] { "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__00.json" },
            new string[] { "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__1.json" },
            new string[] { "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__2.json" },
            new string[] { "Assets/HotFix/Games/Mock/Resources/G200/g200__slot_spin__3.json" },
        };

        Queue<string> curDatas = new Queue<string>();


        public void RequestSlotSpin(long totalBet, Action<JSONNode> successCallback,
            Action<BagelCodeError> errorCallback)
        {
            Timer.DelayAction(0.2f, () =>
            {
                if (curDatas.Count == 0)
                {

                    int dataIndex = UnityEngine.Random.Range(0, 2);

                    List<string[]> target = dataIndex == 0 ? datasSpin : dataFreeSpin; // 选普通游戏 还是免费游戏

                    string[] strs = target[UnityEngine.Random.Range(0, 2)];

                    strs = dataFreeSpin[0];
                    //UnityEngine.Random.Range(0, target.Count)
                    curDatas = new Queue<string>(strs);
                }

                string path = curDatas.Dequeue();
                int resourcesIndex = path.IndexOf("Resources/");
                string remainingPath = path.Substring(resourcesIndex + "Resources/".Length);
                remainingPath = remainingPath.Split('.')[0];

                TextAsset jsn = Resources.Load<TextAsset>(remainingPath);
                if (jsn != null && jsn.text != null)
                {
                    JSONNode res = JSON.Parse(jsn.text);
                    //ParseSlotSpin(totalBet, res);
                    successCallback?.Invoke(res["data"]);
                }
                else
                {
                    BagelCodeError err = new BagelCodeError() { code = 404, msg = $"找不到数据: {path}" };
                    errorCallback?.Invoke(err);
                }

            });
        }

    }
}