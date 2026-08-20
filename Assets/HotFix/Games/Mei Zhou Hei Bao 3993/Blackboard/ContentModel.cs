using FairyGUI;
using GameMaker;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class ContentModel : MonoSingleton<ContentModel>, IContentModel
    {
        #region 观察者实例

        private Observer _observer;

        private Observer Observer
        {
            get
            {
                if (_observer != null) return _observer;

                string[] classNamePath = this.GetType().ToString().Split('.');
                _observer = new Observer(classNamePath[classNamePath.Length - 1]);

                return _observer;
            }
        }

        #endregion

        #region Panel 参数

        private GComponent _goAnchorPanel;
        [SerializeField] private long mTotalBet = 0;
        [SerializeField] private int mBetMultiple = 0;
        [SerializeField] private string mBtnSpinState = "Stop";

        public int betIndex { get; set; } = 0;

        /// <summary> 赢线 </summary>
        public List<SymbolWin> winList;

        public GComponent[] goPayTableLst { get; set; } = Array.Empty<GComponent>();
        public GComponent goAnthorPanel { get => _goAnchorPanel; set => _goAnchorPanel = value; }
        public long totalBet { get => mTotalBet; set => Observer.SetProperty(ref mTotalBet, value); }
        public int betmultiple { get => mBetMultiple; set => Observer.SetProperty(ref mBetMultiple, value); }
        public string btnSpinState { get => mBtnSpinState; set => Observer.SetProperty(ref mBtnSpinState, value); }

        #endregion

        #region 本局游戏数据

        // ------------------------ Normal Game -----------------------
        private string _gameState = GameState.Idle;
        public bool isSpin { get; set; }
        public bool isAuto { get; set; }
        public bool isRequestToStop { get; set; }
        public SlotGameEffect targetSlotGameEffect { get; set; }
        public PageName pageName => PageName.MeiZhouHeiBaoPageGameMain;
        public bool isRequestToRealCreditWhenStop { set => throw new System.NotImplementedException(); }
        public string gameState { get => _gameState; set => Observer.SetProperty(ref _gameState, value); }

        /// <summary> 算法卡数据 </summary>
        public string response;

        /// <summary> 基础游戏赢分（单局普通游戏 或 免费游戏） </summary>
        public long baseGameWinCredit;

        /// <summary> 单局结果界面 </summary>
        public string strDeckRowCol;

        /// <summary> 免费游戏加速框 </summary>
        public bool isFreeSlotTip;

        /// <summary> 是否长滚动 </summary>
        public bool isReelsSlowMotion;

        /// <summary> 普通局豹头收集奖金（盘面 ≥1 豹头且 1~5 个 Bonus，且算法 Panther==1）。 </summary>
        public bool isPantherWin;

        /// <summary> 本局豹头收集的奖金合计（Σ BonusData），不含线奖。 </summary>
        public int pantherBonusWin;


        // ------------------------ Free Game ------------------------
        private int _totalPlaySpins = 1;
        /// <summary> 剩余可玩旋转次数 </summary>
        private int _remainPlaySpins = 1;
        /// <summary> 收集黑豹图标 </summary>
        private int _totalPantherSymbolCount = 0;
        /// <summary> 与 Matrix 同序（行优先 3×5），WILD 携带的倍数：0 / 2 / 3 / 5。 </summary>
        private int[] _wildData = Array.Empty<int>();
        public int[] wildData { get => _wildData; set => _wildData = value ?? Array.Empty<int>(); }

        public int GetWildMul(int col, int row)
        {
            if (_wildData == null || _wildData.Length == 0) return 0;
            int index = row * CustomModel.Instance.column + col;
            if (index < 0 || index >= _wildData.Length) return 0;
            return _wildData[index];
        }

        public static string GetWildAnimName(int mul, bool isWin)
        {
            string suffix = isWin ? "win" : "roll";
            if (mul == 2 || mul == 3 || mul == 5)
                return $"X{mul}_{suffix}";
            return isWin ? "WILD_win" : "WILD_roll";
        }
        public bool isFreeSpin => curReelStripsIndex == "FS";
        public int totalPantherSymbolCount { get => _totalPantherSymbolCount; set => Observer.SetProperty(ref _totalPantherSymbolCount, value); }
        public int totalPlaySpins { get => _totalPlaySpins; set => Observer.SetProperty(ref _totalPlaySpins, value); }
        public int remainPlaySpins { get => _remainPlaySpins; set => Observer.SetProperty(ref _remainPlaySpins, value); }

        private int mFreeSpinPlayTimes = 0;
        private int mFreeSpinTotalTimes = 0;
        private int mShowFreeSpinRemainTime = 0;
        public string curReelStripsIndex = "BS";
        public string nextReelStripsIndex = "BS";

        /// <summary> 免费游戏触发  </summary>
        public bool isFreeSpinTrigger;

        /// <summary> 免费游戏加局 替换isFreeSpinAdd  </summary>
        public bool isFreeGameAdd;

        /// <summary> 免费游戏结束标识 替换isFreeSpinResult  </summary>
        public bool isFreeSpinFinish;

        /// <summary> 当前局，免费增加局数 </summary>
        public int freeSpinAddNum;

        /// <summary> 免费游戏总赢分  </summary>
        public long freeSpinTotalWinCredit;

        /// <summary>  触发免费游戏的编号 </summary>
        public int gameNumberFreeSpinTrigger;

        /// <summary> 是否等待下一局 Parse 校验（本地免费快照恢复后首局 Spin） </summary>
        public bool PendingFreeSpinReconnectValidation { get; set; }

        /// <summary> 触发免费游戏的线-（备份 winList 的数据） </summary>
        public SymbolWin winFreeSpinTriggerOrAddCopy;

        /// <summary>  这个已经改为：基本游戏+彩金了  </summary>
        public long totalEarnCoins; //totalEarnCredit;

        /// <summary> 当前本轮游戏编号 </summary>
        public long curGameNumber;

        /// <summary> 当前本轮游戏开始时间 </summary>
        public long curGameCreatTimeMS;

        /// <summary> 当前本轮游戏guid </summary>
        public string curGameGuid;

        public int freeSpinPlayTimes { get => mFreeSpinPlayTimes; set => Observer.SetProperty(ref mFreeSpinPlayTimes, value); }
        public int freeSpinTotalTimes { get => mFreeSpinTotalTimes; set => Observer.SetProperty(ref mFreeSpinTotalTimes, value); }
        public int ShowFreeSpinRemainTime { get => mShowFreeSpinRemainTime; set => Observer.SetProperty(ref mShowFreeSpinRemainTime, value); }

        // ------------------------ Small Game -----------------------
        public bool isSmallGameTrigger;
        public bool isSmallGameSpin;
        public bool isSmallGameFinish;
        private int _bonusSpinTime;
        public int bonusSpinTime { get => _bonusSpinTime; set => _bonusSpinTime = value; }

        private int _bonusBet;
        public int BonusBet { get => _bonusBet; set => _bonusBet = value;  }

        public int[] BonusData = new int[15];
        public List<List<int>> BonusRound = new List<List<int>>();

        /// <summary> ResultType == RT_Jackpot。同一套 15 轴，但会停出彩金图标。 </summary>
        public bool isJackpotGame;
        public int[] JPTypeArray = Array.Empty<int>();
        public int[] JPBetArray = Array.Empty<int>();
        public int TotalJackpotBet;

        public const int JackpotScoreBase = 4000;

        public static bool IsJackpotScore(int score) =>
            score >= JackpotScoreBase && score <= JackpotScoreBase + 2;

        public static int GetJackpotType(int score) => score - JackpotScoreBase;

        public static string GetJackpotTypeName(int jpType)
        {
            if (jpType == 1) return "minor";
            if (jpType == 2) return "mini";
            return "major";
        }

        public int GetJackpotBet(int jpType)
        {
            if (JPTypeArray == null || JPBetArray == null)
                return 0;

            for (int i = 0; i < JPTypeArray.Length && i < JPBetArray.Length; i++)
            {
                if (JPTypeArray[i] == jpType)
                    return JPBetArray[i];
            }

            if (JPTypeArray.Length == 1 && JPBetArray.Length == 1)
                return JPBetArray[0];
            return 0;
        }

        // ------------------------ Jackpot Data -----------------------

        /// <summary> 本局彩金结果 </summary>
        public JackpotRes jpGameRes;

        /// <summary> bonus数据 </summary>
        public Dictionary<int, JSONNode> bonusResult = new Dictionary<int, JSONNode>();


        [SerializeField] private JackpotInfo mUIGrandJp = new JackpotInfo()
        {
            name = "JPGrand",
            id = 0,
            nowCredit = 69000,
            curCredit = 69204,
            maxCredit = 11100000,
            minCredit = 0,
        };

        [SerializeField] private JackpotInfo mUIMajorJp = new JackpotInfo()
        {
            name = "JPMajor",
            id = 1,
            nowCredit = 15000,
            curCredit = 15134,
            maxCredit = 2500000,
            minCredit = 0,
        };

        [SerializeField] private JackpotInfo mUIMinorJp = new JackpotInfo()
        {
            name = "JPMinor",
            id = 2,
            nowCredit = 240000,
            curCredit = 244073,
            maxCredit = 300000,
            minCredit = 0,
        };

        [SerializeField] private JackpotInfo mUIMiniJp = new JackpotInfo()
        {
            name = "JPMini",
            id = 3,
            nowCredit = 10000,
            curCredit = 10581,
            maxCredit = 30000,
            minCredit = 0,
        };

        public JackpotInfo uiGrandJP { get => mUIGrandJp; set => mUIGrandJp = value; }
        public JackpotInfo uiMajorJP { get => mUIMajorJp; set => mUIMajorJp = value; }
        public JackpotInfo uiMinorJP { get => mUIMinorJp; set => mUIMinorJp = value; }
        public JackpotInfo uiMiniJP { get => mUIMiniJp; set => mUIMiniJp = value; }

        #endregion
    }
}