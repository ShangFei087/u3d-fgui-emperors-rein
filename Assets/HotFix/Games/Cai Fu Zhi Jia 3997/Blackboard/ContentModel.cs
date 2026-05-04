using FairyGUI;
using GameMaker;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaiFuZhiJia_3997
{
    public class ContentModel : MonoSingleton<ContentModel>, IContentModel
    {
        #region 初始化观察者实例

        private Observer _observer;

        public Observer Observable
        {
            get
            {
                if (_observer == null)
                {
                    string[] classNamePath = this.GetType().ToString().Split('.'); // 这里通过以.为分隔符分割开命名空间和类名
                    _observer = new Observer(classNamePath[classNamePath.Length - 1]); // 创建观察者，名字是ContentModel
                }

                return _observer;
            }
        }

        #endregion

        #region 游戏对象节点

        private GComponent _goPanel;

        public GComponent goAnthorPanel
        {
            get => _goPanel;
            set => _goPanel = value;
        }

        #endregion

        public PageName pageName => PageName.CaiFuZhiJiaPageGameMain;

        /// <summary>当游戏结束时自动计算游戏资金和现实资金的开关</summary>
        public bool isRequestToRealCreditWhenStop { set => throw new System.NotImplementedException(); }

        #region 本局游戏数据
        
        /// <summary> 基础游戏赢分（单局普通游戏 或 免费游戏） </summary>
        public long baseGameWinCredit;
        
        /// <summary> 免费游戏加速框 </summary>
        public bool isFreeSlotTip;

        /// <summary>是否开启自动</summary>
        public bool isAuto { get; set; }

        /// <summary>是否是免费旋转</summary>
        public bool isFreeSpin { get; set; }

        /// <summary>是否开启旋转</summary>
        public bool isSpin { get; set; }

        /// <summary>目标特效</summary>
        public SlotGameEffect targetSlotGameEffect { get; set; }

        /// <summary>请求停止游戏</summary>
        public bool isRequestToStop { get; set; }

        /// <summary>游戏状态</summary>
        public string gameState
        {
            get => _gameState;
            set => Observable.SetProperty(ref _gameState, value);
        }

        private string _gameState = GameState.Idle;

        /// <summary>免费游戏总局数</summary>
        public int totalPlaySpins
        {
            get => _totalPlaySpins;
            set => Observable.SetProperty(ref _totalPlaySpins, value);
        }

        private int _totalPlaySpins = 1;

        /// <summary>免费游戏剩余局数</summary>
        public int remainPlaySpins
        {
            get => _remainPlaySpins;
            set => Observable.SetProperty(ref _remainPlaySpins, value);
        }

        private int _remainPlaySpins = 1;

        // public bool isMain = true;

        /// <summary> 算法卡数据 </summary>
        public string response;

        /// <summary> 单局结果界面 </summary>
        public string strDeckRowCol;

        /// <summary> 赢线 </summary>
        public List<SymbolWin> winList;

        /// <summary> 免费游戏开始 </summary>
        public bool isFreeSpinTrigger;

        /// <summary> 免费游戏结束 </summary>
        public bool isFreeSpinResult;

        /// <summary> 额外添加免费游戏 </summary>
        public bool isFreeSpinAdd;

        /// <summary> 断电前免费得分 </summary>
        [FormerlySerializedAs("CurrentWinBet")]
        public long currentWinBet;

        /// <summary> 当前局，免费增加局数 </summary>
        public int freeSpinAddNum;

        /// <summary> 免费游戏总次数  </summary>
        public int FreeSpinTotalTimes
        {
            get => _mFreeSpinTotalTimes;
            set => Observable.SetProperty(ref _mFreeSpinTotalTimes, value);
        }

        private int _mFreeSpinTotalTimes = 0;


        /// <summary>
        /// 免费游戏显示剩余多少次
        /// </summary>
        public int ShowFreeSpinRemainTime
        {
            get => _mShowFreeSpinRemainTime;
            set => Observable.SetProperty(ref _mShowFreeSpinRemainTime, value);
        }

        private int _mShowFreeSpinRemainTime = 0;

        /// <summary> 当前免费游戏轮数  </summary>
        public int FreeSpinPlayTimes
        {
            get => _mFreeSpinPlayTimes;
            set => Observable.SetProperty(ref _mFreeSpinPlayTimes, value);
        }

        private int _mFreeSpinPlayTimes = 0;

        /// <summary> 本局彩金结果 </summary>
        public JackpotRes JpGameRes;

        /// <summary> 额外奖 - 掉球 </summary>
        public bool isBonus1 = false;

        /// <summary> 掉球个数 </summary>
        public int hitBallCount = 0;

        public bool isHitJackpotGame;

        public bool isHaveWildSymbol;

        /// <summary>彩金游戏触发</summary>
        public bool IsBonusTrigger { get; set; }

        public bool IsJackpotTrigger { get; set; }

        /// <summary>
        /// 玩家没中奖计数
        /// </summary>
        public int noWinCount = 0;
        
        /// <summary>
        /// 一局游戏中wild图标的行列索引信息
        /// </summary>
        public List<Cell> currentWildList = new List<Cell>();

        /// <summary>
        /// 彩金游戏总得分
        /// </summary>
        public long totalBonusReward = 0;

        /// <summary>
        /// 彩金得分数组
        /// </summary>
        public List<string> jpBetArray = new List<string>();

        /// <summary>
        /// 彩金类型数组
        /// </summary>
        public List<string> jpTypeArray = new List<string>();

        /// <summary>
        /// 彩金得分类型对照表
        /// </summary>
        public Dictionary<string, string> JpBetDic = new Dictionary<string, string>();

        /// <summary>
        /// 大奖游戏中中Jackpot奖弹窗显示的数据
        /// </summary>
        public int currentShowJpBet = 0;

        /// <summary>
        /// 大奖游戏中中Jackpot奖弹窗显示的Spine动画索引
        /// </summary>
        public int currentJpSpineIndex = -1;

        /// <summary>
        /// 本局Jackpot索引
        /// </summary>
        public List<int> currentJpIndexList = new List<int>();

        /// <summary>
        /// 一局彩金游戏的数据
        /// </summary>
        public List<string> currentBonusDataList = new List<string>();

        /// <summary>  本局普通线赢得分 </summary>
        public long normalWinBet = 0;

        public string curReelStripsIndex = "BS";

        public string nextReelStripsIndex = "BS";

        /// <summary>  这个已经改为：基本游戏+彩金了  </summary>
        public long totalEarnCoins; //totalEarnCredit;

        /// <summary> 基础游戏赢分（单局普通游戏 或 免费游戏） </summary>
        public long baseGameWinCoins; //baseGameWinCredit;

        /// <summary> 当前本轮游戏开始时间 </summary>
        public long curGameCreatTimeMS;

        /// <summary> 当前本轮游戏guid </summary>
        public string curGameGuid;

        /// <summary> 当前本轮游戏guid </summary>
        public string freeSpinTriggerGuid;

        /// <summary> 当前本轮游戏编号 </summary>
        public long curGameNumber;

        /// <summary>  触发免费游戏的编号 </summary>
        public int gameNumberFreeSpinTrigger;

        /// <summary> 是否等待下一局 Parse 校验（本地免费快照恢复后首局 Spin） </summary>
        public bool PendingFreeSpinReconnectValidation { get; set; }

        /// <summary> 免费游戏总赢分  </summary>
        public long freeSpinTotalWinCoins; // freeSpinTotalWinCredit;

        /// <summary> 是否发生断电重连  </summary>
        public bool isPowerTrigger;

        /// <summary> 是否长滚动 </summary>
        public bool isReelsSlowMotion;

        /// <summary> bonus数据 </summary>
        public Dictionary<int, JSONNode> BonusResults = new Dictionary<int, JSONNode>();

        /// <summary> 触发免费游戏的线-（备份 winList 的数据） </summary>
        public SymbolWin winFreeSpinTriggerOrAddCopy;

        /// <summary> 本局彩金结果 </summary>
        public JackpotRes jpGameRes;

        /// <summary> 游戏彩金中奖数据 </summary>
        public List<JackpotWinInfo> jpGameWinLst => jpGameRes.jpWinLst;

        /// <summary> 中奖前的彩金值 </summary>
        public List<float> jpGameWhenCreditLst
        {
            get
            {
                List<float> jps = new List<float>()
                {
                    jpGameRes.curJackpotGrand,
                    jpGameRes.curJackpotMajor,
                    jpGameRes.curJackpotMinior,
                    jpGameRes.curJackpotMini,
                };
                foreach (JackpotWinInfo item in jpGameRes.jpWinLst)
                {
                    jps[item.id] = item.whenCredit;
                }

                return jps;
            }
        }

        public float freeOnceCredit
        {
            get => m_freeOnceCredit;
            set => Observable.SetProperty(ref m_freeOnceCredit, value);
        }

        public float m_freeOnceCredit = 0;

        /// <summary>免费游戏分数倍率</summary>
        [FormerlySerializedAs("FreeGameScoreMultiply")]
        public int freeGameScoreMultiply = 2;

        /// <summary>彩金游戏钻石得分模拟</summary>
        [FormerlySerializedAs("BonusGameRewardList")]
        public List<string> bonusGameRewardList = new List<string>() { "200", "1400", "2600", };

        /// <summary>彩金游戏是否中奖</summary>
        public bool isWinning = false;

        #endregion

        #region Panel 参数

        /// <summary>总押注</summary>
        public long totalBet { get => mTotalBet; set => Observable.SetProperty(ref mTotalBet, value); }

        [SerializeField] private long mTotalBet = 0;

        /// <summary> 下注倍数 </summary>
        public int betmultiple
        {
            get => m_BetMultiple;
            set => Observable.SetProperty(ref m_BetMultiple, value);
        }

        [SerializeField] private int m_BetMultiple = 0;

        /// <summary>押注分数索引</summary>
        public int betIndex { get; set; } = 0;

        /// <summary>Spin按钮状态</summary>
        public string btnSpinState { get => mBtnSpinState; set => Observable.SetProperty(ref mBtnSpinState, value); }

        [SerializeField] private string mBtnSpinState = "Stop";

        #endregion

        #region Jackpot 参数

        /// <summary>巨奖</summary>
        public JackpotInfo uiGrandJP { get => mUIGrandJp; set => mUIGrandJp = value; }

        [SerializeField] private JackpotInfo mUIGrandJp = new JackpotInfo()
        {
            name = "JPGrand",
            id = 0,
            nowCredit = 69000,
            curCredit = 69204,
            maxCredit = 11100000,
            minCredit = 0,
        };

        /// <summary>大奖</summary>
        public JackpotInfo uiMajorJP { get => mUIMajorJp; set => mUIMajorJp = value; }

        [SerializeField] private JackpotInfo mUIMajorJp = new JackpotInfo()
        {
            name = "JPMajor",
            id = 1,
            nowCredit = 15000,
            curCredit = 15134,
            maxCredit = 2500000,
            minCredit = 0,
        };

        /// <summary>中奖</summary>
        public JackpotInfo uiMinorJP { get => mUIMinorJp; set => mUIMinorJp = value; }

        [SerializeField] private JackpotInfo mUIMinorJp = new JackpotInfo()
        {
            name = "JPMinor",
            id = 2,
            nowCredit = 240000,
            curCredit = 244073,
            maxCredit = 300000,
            minCredit = 0,
        };

        /// <summary>小奖</summary>
        public JackpotInfo uiMiniJP { get => mUIMiniJp; set => mUIMiniJp = value; }

        [SerializeField] private JackpotInfo mUIMiniJp = new JackpotInfo()
        {
            name = "JPMini",
            id = 3,
            nowCredit = 10000,
            curCredit = 10581,
            maxCredit = 30000,
            minCredit = 0,
        };

        #endregion

        /// <summary>赔付表说明UI集合</summary>
        public GComponent[] goPayTableLst { get; set; } = Array.Empty<GComponent>();

        /// <summary> 大厅彩金中奖数据  </summary>
        public List<WinJackpotInfo> jpOnlineWin = new List<WinJackpotInfo>();
    }
}