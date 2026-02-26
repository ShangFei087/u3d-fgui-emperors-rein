using FairyGUI;
using GameMaker;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using SlotMaker;

namespace SlotZhuZaiJinBi1700
{
    public class ContentModel : MonoSingleton<ContentModel>, IContentModel
    {
      
        //private static object _mutex = new object();
        //static ContentModel _instance;
        //public static ContentModel Instance
        //{
        //    get
        //    {

        //        lock (_mutex)
        //        {
        //            if (_instance == null)
        //            {
        //                _instance = FindObjectOfType<ContentModel>();
        //                // FindObjectOfType(typeof(DevicePrinterOut)) as DevicePrinterOut;
        //                if (_instance == null)
        //                {
        //                    // Debug.LogError();
        //                }
        //            }
        //            return _instance;
        //        }
        //    }
        //}


        Observer m_Observable;
        public Observer observable
        {
            get
            {
                if (m_Observable == null)
                {
                    string[] classNamePath = this.GetType().ToString().Split('.');
                    m_Observable = new Observer(classNamePath[classNamePath.Length - 1]);
                }
                return m_Observable;
            }
        }
        public PageName pageName => PageName.SlotZhuZaiJinBiPageGameMain;

        #region 游戏对象节点
        public GComponent goAnthorPanel
        {
            get => _goPanel;
            set => _goPanel = value;
        }
        GComponent _goPanel;


        public GComponent mainPanel
        {
            get => gameMainPanel;
            set => gameMainPanel = value;
        }
        GComponent gameMainPanel;

        #endregion
        
        
        
        
        #region 本剧游戏数据
        public bool isAuto
        {
            get => _isAuto;
            set => _isAuto = value;
        }
        public bool _isAuto;

        
        public bool isSpin
        {
            get => _isSpin;
            set => _isSpin = value;
        }
        public bool _isSpin;

        /// <summary> 目标特效 </summary>
        public SlotGameEffect targetSlotGameEffect
        {
            get => m_TargetSlotGameEffect;
            set => m_TargetSlotGameEffect = value;
        }
        private SlotGameEffect m_TargetSlotGameEffect;



        /// <summary> 请求停止游戏 </summary>
        public bool isRequestToStop
        {
            get => _isRequestToStop;
            set => _isRequestToStop = value;
        }
        public bool _isRequestToStop = false;


        /// <summary> 总的游玩次数 </summary>
        public int totalPlaySpins
        {
            get => _totalPlaySpins;
            set => _totalPlaySpins = value;
        }
        public int _totalPlaySpins = 1;

        /// <summary> 剩余的游玩次数 </summary>
        public int remainPlaySpins
        {
            get => _remainPlaySpins;
            set => observable.SetProperty(ref _remainPlaySpins, value); 
        }
        private int _remainPlaySpins = 1;



        /// <summary> 游戏状态 </summary>
        public string _gameState = GameState.Idle;// "Idle";
        public string gameState
        {
            get => _gameState;
            set => observable.SetProperty(ref _gameState, value); 
        }

        public string curReelStripsIndex = "BS";

        public string nextReelStripsIndex = "BS"; // "FS"



        /// <summary>  这个已经改为：基本游戏+彩金了  </summary>
        public long totalEarnCredit;



        /// <summary> 基础游戏赢分（单局普通游戏 或 免费游戏） </summary>
        public long baseGameWinCredit;


        /// <summary> 免费游戏总赢分  </summary>
        public long freeSpinTotalWinCredit;


        /// <summary> 算法卡数据 </summary>
        public string response;


        /// <summary> 单局结果界面 </summary>
        public string strDeckRowCol;
        
        /// <summary> 赢线 </summary>
        public List<SymbolWin> winList;


        /// <summary> 触发免费游戏的线-（备份 winList 的数据） </summary>
        public SymbolWin winFreeSpinTriggerOrAddCopy;


        /// <summary> 是否长滚动 </summary>
        public bool isReelsSlowMotion;
            
            
        /// <summary> 免费游戏开始 </summary>
        public bool isFreeSpinTrigger;


        /// <summary> 免费游戏结束 </summary>
        public bool isFreeSpinResult;


        /// <summary> 当前局，免费增加局数 </summary>
        public int freeSpinAddNum;

        /// <summary> 当前大奖类型 </summary>
        public string bonusType;

        /// <summary> 额外添加免费游戏 </summary>
        public bool isFreeSpinAdd;

        public bool isFreeSpin => curReelStripsIndex == "FS";
        

        /// <summary> 奖励的赢分乘数  </summary>
        public int multiplierAlone
        {
            get => m_MultiplierAlone;
            set => observable.SetProperty(ref m_MultiplierAlone, value);
        }
        private int m_MultiplierAlone = 0;






        /// <summary> 当前免费游戏轮数  </summary>
        public int freeSpinPlayTimes
        {
            get => m_FreeSpinPlayTimes;
            set => observable.SetProperty(ref m_FreeSpinPlayTimes, value);
        }
        private int m_FreeSpinPlayTimes = 0;



        /// <summary> 免费游戏总次数  </summary>
        public int freeSpinTotalTimes
        {
            get => m_FreeSpinTotalTimes;
            set => observable.SetProperty(ref m_FreeSpinTotalTimes, value);
        }
        private int m_FreeSpinTotalTimes = 0;


        /// <summary>
        /// 免费游戏显示剩余多少次
        /// </summary>
        public int showFreeSpinRemainTime
        {
            get => m_showFreeSpinRemainTime;
            set => observable.SetProperty(ref m_showFreeSpinRemainTime, value);
        }
        public int m_showFreeSpinRemainTime = 0;


        /// <summary>
        /// 免费游戏单局赢分
        /// </summary>
        public float freeOnceCredit
        {
            get => m_freeOnceCredit;
            set => observable.SetProperty(ref m_freeOnceCredit, value);
        }
        public float m_freeOnceCredit = 0;

        /// <summary> 游戏前 </summary>
        public long creditBefore;

        /// <summary> 游戏后 </summary>
        public long creditAfter;

        /// <summary> 当前本轮游戏开始时间 </summary>
        public long curGameCreatTimeMS;


        /// <summary> 当前本轮游戏guid </summary>
        public string curGameGuid;


        /// <summary> 当前本轮游戏编号 </summary>
        public long curGameNumber;

        /// <summary>  触发免费游戏的编号 </summary>
        public int gameNumberFreeSpinTrigger;


        #endregion
        
        

        #region Jackpot 参数

        [SerializeField]
        JackpotInfo m_UIGrandJP = new JackpotInfo()
        {
            name = "JPGrand",
            id = 0,
            nowCredit = 69000,
            curCredit = 69204,
            maxCredit = 11100000,
            minCredit = 0,
        };
        public JackpotInfo uiGrandJP
        {
            get => m_UIGrandJP;
            set => m_UIGrandJP = value;
        }


        [SerializeField]
        JackpotInfo m_UIMegaJP = new JackpotInfo()
        {
            name = "JPMega",
            id = 1,
            nowCredit = 15000,
            curCredit = 15134,
            maxCredit = 2500000,
            minCredit = 0,
        };
        public JackpotInfo uiMegaJP
        {
            get => m_UIMegaJP;
            set => m_UIMegaJP = value;
        }




        [SerializeField]
        JackpotInfo m_UIMajorJP = new JackpotInfo()
        {
            name = "JPMajor",
            id = 1,
            nowCredit = 15000,
            curCredit = 15134,
            maxCredit = 2500000,
            minCredit = 0,
        };
        public JackpotInfo uiMajorJP
        {
            get => m_UIMajorJP;
            set => m_UIMajorJP = value;
        }


        [SerializeField]
        JackpotInfo m_UIMinorJP = new JackpotInfo()
        {
            name = "JPMinor",
            id = 2,
            nowCredit = 240000,
            curCredit = 244073,
            maxCredit = 300000,
            minCredit = 0,
        };
        public JackpotInfo uiMinorJP
        {
            get => m_UIMinorJP;
            set => m_UIMinorJP = value;
        }


        [SerializeField]
        JackpotInfo m_UIMiniJP = new JackpotInfo()
        {
            name = "JPMini",
            id = 3,
            nowCredit = 10000,
            curCredit = 10581,
            maxCredit = 30000,
            minCredit = 0,
        };
        public JackpotInfo uiMiniJP
        {
            get => m_UIMiniJP;
            set => m_UIMiniJP = value;
        }


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

        /// <summary> 大厅彩金中奖数据  </summary>
        public List<WinJackpotInfo> jpOnlineWin = new List<WinJackpotInfo>();
        #endregion



        #region Panel 参数


        /// <summary> 押注分数索引 </summary>
        public int betIndex
        {
            get => _betIndex;
            set => _betIndex = value;
        }
        public int _betIndex = 0;


        /// <summary> Spin 按钮状态 </summary>
        public string btnSpinState
        {
            get => m_BtnSpinState;
            set => observable.SetProperty(ref m_BtnSpinState, value);
        }
        [SerializeField]
        private string m_BtnSpinState = "Stop";


        /// <summary> 总押注 </summary>
        public long totalBet
        {
            get => m_TotalBet;
            set => observable.SetProperty(ref m_TotalBet, value);
        }
        [SerializeField]
        private long m_TotalBet = 0;
       

        /// <summary> 选择的线数 </summary>
        public int selectLine
        {
            get => m_SelectLine;
            set => observable.SetProperty(ref m_SelectLine, value);
        }
        [SerializeField]
        private int m_SelectLine;



        /// <summary> 单线押注分数 </summary>
        public long apostarCredit
        {
            get => m_ApostarCredit;
            set => observable.SetProperty(ref m_ApostarCredit, value);
        }
        [SerializeField]
        private long m_ApostarCredit;



        #endregion


        #region 读取配置表

        
        
        public List<PayTableSymbolInfo> m_PayTableSymbolWin = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo()
            {
                symbol = 0,
                x5 = 0,
                x4 = 0,
                x3 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 1,
                x5 = 0,
                x4 = 0,
                x3 = 2,
            },
            new PayTableSymbolInfo()
            {
                symbol = 2,
                x5 = 10,
                x4 = 4,
                x3 = 1,
            },
            new PayTableSymbolInfo()
            {
                symbol = 3,
                x5 = 6,
                x4 = 2,
                x3 = 0.6,
            },
            new PayTableSymbolInfo()
            {
                symbol = 4,
                x5 = 2,
                x4 = 1,
                x3 = 0.5,
            },
            new PayTableSymbolInfo()
            {
                symbol = 5,
                x5 = 1.6,
                x4 = 0.6,
                x3 = 0.3,
            },
            new PayTableSymbolInfo()
            {
                symbol = 6,
                x5 = 1,
                x4 = 0.3,
                x3 = 0.2,
            },
            new PayTableSymbolInfo()
            {
                symbol = 7,
                x5 = 0.6,
                x4 = 0.2,
                x3 = 0.16,
            },
            new PayTableSymbolInfo()
            {
                symbol = 8,
                x5 = 0.6,
                x4 = 0.2,
                x3 = 0.16,
            },
            new PayTableSymbolInfo()
            {
                symbol = 9,
                x5 = 0.4,
                x4 = 0.2,
                x3 = 0.1,
            },
            new PayTableSymbolInfo()
            {
                symbol = 10,
                x5 = 0.4,
                x4 = 0.2,
                x3 = 0.1,
            },
            new PayTableSymbolInfo()
            {
                symbol = 11,
                x5 = 0.4,
                x4 = 0.2,
                x3 = 0.1,
            },
        };

        public List<PayTableSymbolInfo> payTableSymbolWin
        {
            get => m_PayTableSymbolWin;
            set => observable.SetProperty(ref m_PayTableSymbolWin, value);
        }

        
        public GComponent[] goPayTableLst {
            get => m_GoPayTable;
            set => m_GoPayTable = value;
        }
        GComponent[] m_GoPayTable;

        public List<List<int>> payLines
        {
            get => m_payLines;
            set => m_payLines = value; 
        }
        List<List<int>> m_payLines;

        
        public List<WinMultiple> winLevelMultiple{
            get => _winMultipleList;
            set => _winMultipleList = value; 
        }
        public bool isRequestToRealCreditWhenStop { set => throw new System.NotImplementedException(); }

        public List<WinMultiple> _winMultipleList = new List<WinMultiple>();        
        
        #endregion


        /// <summary> bonus数据 </summary>
        public Dictionary<int, JSONNode> bonusResult = new Dictionary<int, JSONNode>();
        
        
        

    }
}