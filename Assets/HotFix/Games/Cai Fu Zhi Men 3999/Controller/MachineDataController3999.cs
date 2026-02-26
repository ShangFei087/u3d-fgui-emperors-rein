using GameMaker;
using GameUtil;
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

        /// <summary>
        /// 算法解析
        /// </summary>
        /// <param name="totalBet"></param>
        /// <param name="res"></param>
        /// <param name="sboxJackpotData"></param>
        public void ParseSlotSpin02(long totalBet, JSONNode res, SBoxJackpotData sboxJackpotData)
        {
            List<SymbolInclude> symbolInclude = new List<SymbolInclude>();
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


            //IDVec
            int lineNum = (int)res["lineNum"];
            int totalEarnCredit = 0;
            int credit = 0;
            List<SymbolWin> winList = new List<SymbolWin>();

            for (int i = 0; i < lineNum; i++)
            {
                //-IDVec:万千位标识线， 百位标识消除多少个， 十个位标识ID。
                int ID = (int)res["IDVec"][i];

                int symbolNumber = ID % 100; // 十个位：Symbol ID
                int hitCount = (ID / 100) % 10; // 百位：消除数量（WinCount）
                // int lineNumber = 0;
                // if ((ID / 100) < 10)
                //     lineNumber = 0;
                // else
                int lineNumber = ID / 1000; // 万千位：线编号

                // 输出调试信息（可选）
                Debug.Log($"ID: {ID}, Line: {lineNumber}, HitCount: {hitCount}, Symbol: {symbolNumber}");

                int lineIndex = lineNumber; // 注：中奖线索引从0开始
                int[] lineInfo = ContentModel.Instance.payLines[lineIndex].ToArray();
                List<Cell> _cells = new List<Cell>();

                for (int c = 0; c < hitCount; c++)
                {
                    int rowIdx = lineInfo[c];
                    int colIdx = c;
                    _cells.Add(new Cell(colIdx, rowIdx));
                }

                // TODO:单局得分暂时先用这个，后续再根据赔率进行修改
                credit = (int)res["TotalBet"];
                // credit = 0;
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

            // jpGameRes.curJackpotGrand = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 1
            //     ? sboxJackpotData.JackpotOut[0]
            //     : 0;
            // jpGameRes.curJackpotMajor = sboxJackpotData != null && sboxJackpotData.JackpotOut.Length >= 2
            //     ? sboxJackpotData.JackpotOut[1]
            //     : 0;
            // jpGameRes.curJackpotMinior = 1000;
            // jpGameRes.curJackpotMini = 500;
            //
            // if (isJackpotMajor)
            // {
            //     int winCredit = (int)res["num"];
            //     jpGameRes.jpWinLst.Add(new JackpotWinInfo()
            //     {
            //         name = "major",
            //         id = 1,
            //         winCredit = sboxJackpotData.Jackpotlottery[1],
            //         whenCredit = sboxJackpotData.JackpotOld[1],
            //         curCredit = sboxJackpotData.JackpotOut[1],
            //     });
            // }

            // long creditBefore = MainBlackboardController.Instance.myRealCredit;
            if (++MainModel.Instance.gameNumber < 0)
                MainModel.Instance.gameNumber = 1;
            ContentModel.Instance.response = res.ToString();

            ContentModel.Instance.curReelStripsIndex = "BS";
            ContentModel.Instance.nextReelStripsIndex = "BS";
            //判断免费奖或大奖
            int ResultType = (int)res["ResultType"];
            int OpenType = (int)res["OpenType"];
            // int TotalFreeTime = (int)res["TotalFreeTime"];
            string matrixArray = res["Matrix"].ToString();
            //免费奖
            ContentModel.Instance.isFreeSpinTrigger = false;
            if (ResultType == 2)
            {
                Debug.Log("-------免费奖--------");
                ContentModel.Instance.curReelStripsIndex = "BS";
                ContentModel.Instance.nextReelStripsIndex = "FS";

                ContentModel.Instance.isFreeSpinTrigger = true;

                // 注：根据随机出的图标次数显示当前游戏总局数
                // ContentModel.Instance.freeSpinTotalTimes = TotalFreeTime;
                ContentModel.Instance.FreeSpinTotalTimes = _freeRoundDic[CountTensEfficient(matrixArray)];
                ContentModel.Instance.FreeSpinPlayTimes = 0;
                ContentModel.Instance.freeTotalBet = (int)res["TotalFreeBet"];


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
            int BonusType = (int)res["BonusType"];
            int BonusBet = (int)res["BonusBet"];
            if (ResultType == 3)
            {
                ContentModel.Instance.bonusTotalBet = (int)res["BonusBet"];
                ContentModel.Instance.IsBonusTrigger = true;
            }

            ContentModel.Instance.curGameCreatTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            long creditBefore = MainBlackboardController.Instance.myTempCredit;
            //赢分
            long TotalBet = (int)res["TotalBet"];
            if (ResultType == 3) TotalBet = (int)res["BonusBet"]; //大奖得分
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

            // ## MainBlackboardController.Instance.SetMyTempCredit(afterBetCredit, false); // 不同步给ui
            //long creditAfter = afterBetCredit + totalEarnCredit;
            long creditAfter = afterBetCredit;
            if (res.HasKey("creditAfter"))
            {
                creditAfter = res["creditAfter"];
            }

            // ## MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
            MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

            DebugUtils.Log(
                $"押注前分数：creditBefore = {creditBefore} 押注分数：{totalBet} 押注后分数:  afterBetCredit = {afterBetCredit}  totalEarnCredit={totalEarnCredit} ");
            DebugUtils.Log($"本次计算 creditAfter= {afterBetCredit + totalEarnCredit}；  算法卡 creditAfter={creditAfter}");


            // 免费游戏累计总赢
            long freeSpinTotalWinCredit = 0;

            if (OpenType == 1)
            {
                ContentModel.Instance.freeSpinTotalWinCoins = 0; //freeSpinTotalWinCredit 修改
            }
            else
            {
                ContentModel.Instance.freeSpinTotalWinCoins += totalEarnCredit;
                freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCoins;
            }


            List<List<int>> deckColRow = SlotTool.GetDeckColRow02(strDeckRowCol);
            // 原代码
            //bool isReelsSlowMotion = (deckColRow[0].Contains(10) && deckColRow[1].Contains(10)) ? true : false;
            // bool isReelsSlowMotion = false;
            // ContentModel.Instance.isReelsSlowMotion = isReelsSlowMotion;
            ContentModel.Instance.isReelsSlowMotion = true;

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
            ContentModel.Instance.bonusResults = bonusResult; //bonusResults 替换bonusResult


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