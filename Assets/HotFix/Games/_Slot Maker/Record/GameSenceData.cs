using GameMaker;
using System.Collections.Generic;

namespace SlotMaker
{
    [System.Serializable]
    public class GameSenceData
    {
        /// <summary> 算分开下行数据 </summary>
        public string respone;

        /// <summary> 本次数据上报的id号（累加） </summary>
        public int reportId = 74282;

        /// <summary> 本次数据的时间（秒级） </summary>
        public long timeS = 0;

        /// <summary> 游戏编号（第几局游戏） </summary>
        public long gameNumber = 0;  //guid

        /// <summary> 开启免费游戏的 正规游戏id </summary>
        public long gameNumberFreeSpinTrigger = 0; //guid


        /// <summary> 当前是否是免费游戏 </summary>
        public bool isFreeSpin;

        /// <summary> 当前免费游戏增加的次数 </summary>
        public int freeSpinAddNum;

        /// <summary> 当前滚轮 </summary>
        public string curStripsIndex; // "BS" "FS"

        /// <summary> 下一局滚轮 </summary>
        public string nextStripsIndex;

        /// <summary> 滚轮界面数据 </summary>
        public string strDeckRowCol;

        /// <summary> 滚轮界面数据 - 数组 </summary>
        public List<int> deckRowCol;

        /// <summary> 免费单局局数 </summary>
        public int freeSpinPlayTimes;

        /// <summary> 免费游戏总局数 </summary>
        public int freeSpinTotalTimes;

        /// <summary> 免费游戏累计总赢 </summary>
        public long freeSpinTotalWinCredit;

        /// <summary> 所有赢线数据 </summary>
        public List<SymbolWin> winList;

        /// <summary> 5连线数据 </summary>
        //public SymbolWin win5Kind;

        /// <summary> 触发免费游戏数据 </summary>
        public SymbolWin winFreeSpinTrigger;

        /// <summary> 本局总赢 </summary>
        //public long totalWinCredit;

        /// <summary> 基本游戏赢 </summary>
        public long baseGameWinCredit;

        /// <summary> 彩金赢 </summary>
        public long jackpotWinCredit;

        /// <summary> 游戏前积分 </summary>
        public long creditBefore;

        /// <summary> 游戏后积分 </summary>
        public long creditAfter;

        /// <summary> 本局总压 </summary>
        public long totalBet;

        /// <summary> 巨奖值 </summary>
        public float jpGrand;
        /// <summary> 头奖值 </summary>
        public float jpMajor;
        /// <summary> 大奖值 </summary>
        public float jpMinor;
        /// <summary> 小奖值 </summary>
        public float jpMini;

        /// <summary> 彩金中奖赢分 </summary>
        // public long jpWinCredit;

        /// <summary> 彩金中名称 </summary>
        //public string jpWinName;

        /// <summary> 彩金中等级 </summary>
        //public string jpWinLevel;

        /// <summary> 彩金中奖信息 </summary>
        public JackpotWinInfo jpWinInfo;
    }
}