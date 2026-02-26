using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SlotMaker
{


    /// <summary> 单线赢的数据 </summary>
    [System.Serializable]
    public class SymbolWin
    {
        /// <summary> 中奖积分 或 币数</summary>
        public long earnCredit = 0;
        public long multiplier = 1;
        /// <summary> 中单线线号 </summary>
        [FormerlySerializedAs("lineIndex")] public int lineNumber = -1;
        [FormerlySerializedAs("symbolIndex")] public int symbolNumber = -1;
        public List<Cell> cells = new List<Cell>();
        public string customData = "";
    }


    /// <summary> 所有线赢的数据 </summary>
    public class TotalSymbolWin : SymbolWin
    {
        public List<int> lineNumbers;
    }

    /// <summary> 单个Bonus赢的数据</summary>
    public class BonusWin
    {
        public long earnCredit = 0;
        public long multiplier = 1;
        /// <summary> 中单线线号 </summary>
        //[FormerlySerializedAs("lineIndex")] public int lineNumber = -1;
        [FormerlySerializedAs("symbolIndex")] public int symbolNumber = 11;
        public Cell cell = new Cell();
        public string customData = "";
    }
    // List<BonusWin>
}