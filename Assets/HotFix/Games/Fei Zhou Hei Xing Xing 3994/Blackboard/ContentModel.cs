using FairyGUI;
using GameMaker;
using HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FeiZhouHeiXingXing_3994
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

        [SerializeField] private long mTotalBet = 0;
        [SerializeField] private int mBetMultiple = 0;
        [SerializeField] private string mBtnSpinState = "Stop";

        public int betIndex { get; set; } = 0;

        /// <summary> 赢线 </summary>
        public List<SymbolWin> winList;

        public GComponent[] goPayTableLst { get; set; } = Array.Empty<GComponent>();
        public GComponent goAnthorPanel { get; set; }

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
        public PageName pageName => PageName.FeiZhouHeiXingXingPageGameMain;
        public bool isRequestToRealCreditWhenStop { set => throw new NotImplementedException(); }
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


        // ------------------------ Free Game ------------------------
        private int _totalPlaySpins = 1;
        private int _remainPlaySpins = 1;
        public bool isFreeSpin => curReelStripsIndex == "FS";
        public int totalPlaySpins { get => _totalPlaySpins; set => Observer.SetProperty(ref _totalPlaySpins, value); }

        public int remainPlaySpins
        {
            get => _remainPlaySpins;
            set => Observer.SetProperty(ref _remainPlaySpins, value);
        }

        private int _mFreeSpinPlayTimes = 0;
        private int _mFreeSpinTotalTimes = 0;
        private int _mShowFreeSpinRemainTime = 0;
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
        public long freeSpinTotalWinCoins; // freeSpinTotalWinCredit

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

        public int FreeSpinPlayTimes
        {
            get => _mFreeSpinPlayTimes;
            set => Observer.SetProperty(ref _mFreeSpinPlayTimes, value);
        }

        public int FreeSpinTotalTimes
        {
            get => _mFreeSpinTotalTimes;
            set => Observer.SetProperty(ref _mFreeSpinTotalTimes, value);
        }

        public int ShowFreeSpinRemainTime
        {
            get => _mShowFreeSpinRemainTime;
            set => Observer.SetProperty(ref _mShowFreeSpinRemainTime, value);
        }

        // ------------------------ Small Game -----------------------
        public bool isSmallGameTrigger;
        public bool isSmallGameSpin;
        public bool IsSmallGameFinish => smallGameSpinCount < 0;

        /// <summary> SmallGame总赢分 </summary>
        public long smallGameWinCredit;

        /// <summary> SmallGame总局数 </summary>
        public int smallGameSpinCount = 3;

        /// <summary> 根据彩金触发局传入信息，解析好的彩金游戏数据队列 </summary>
        public Queue<List<int>> BonusDataQueue = new Queue<List<int>>();

        // ------------------------ Jackpot Data -----------------------
        /// <summary> 本局彩金结果 </summary>
        public JackpotRes JpGameRes;

        /// <summary> bonus数据 </summary>
        public readonly Dictionary<int, JSONNode> BonusResult = new Dictionary<int, JSONNode>();

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