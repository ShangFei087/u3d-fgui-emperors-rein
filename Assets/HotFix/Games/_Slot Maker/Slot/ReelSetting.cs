using System;
using UnityEngine;

namespace SlotMaker
{
    [Serializable]
    public class ReelSetting
    {
        public const int NONE = -99999;
        /// <summary>单圈首次转动延时 </summary>
        [Tooltip("单圈首次转动延时")]
        public float timeTurnStartDelay = NONE;

        /// <summary>单圈转动时间 </summary>
        [Tooltip("单圈转动时间")]
        public float timeTurnOnce = NONE;

        /// <summary>单列滚轮首次转动时，回弹的时间 </summary>
        [Tooltip("单列滚轮首次转动时，回弹的时间")]
        public float timeReboundStart = NONE;

        /// <summary>单列滚轮结束转动时，回弹的时间 </summary>
        [Tooltip("单列滚轮结束转动时，回弹的时间")]
        public float timeReboundEnd = NONE;

        /// <summary>单列滚轮首次转动时，回弹的偏移量 </summary>
        [Tooltip("单列滚轮首次转动时，回弹的偏移量")]
        public float offsetYReboundStart = NONE;

        /// <summary>单列滚轮结束转动时，回弹的偏移量 </summary>
        [Tooltip("单列滚轮结束转动时，回弹的偏移量")]
        public float offsetYReboundEnd = NONE;

        /// <summary> 单列滚轮转动的圈数 </summary>
        [Tooltip("单列滚轮转动的圈数")]
        public int numReelTurn = NONE;

        /// <summary> 单列滚轮,比前一列滚轮多转动的圈数（区分多列滚轮间的转动） </summary>
        [Tooltip("单列滚轮多转动的圈数")]
        public int numReelTurnGap = NONE;
        // public int numReelTurnGapLow = -1;
        // public int numReelTurnGapMedium = -1;
        // public int numReelTurnGapHigh = -1;
    }

    public enum SymbolLineShowType
    {
        /// <summary> 不显示 </summary>
        None = 0,

        /// <summary> 显示中奖图标，不带动画 </summary>
        SymbolLine,

        /// <summary> 显示中奖图标，带动画 </summary>
        SymbolLineAnim,

        /// <summary> 所有“中奖图标和线”全显示，不带动画 </summary>
        AllSymbolLine,

        /// <summary> 所有“中奖图标和线”全显示，带动画 </summary>
        AllSymbolLineAnim,

        /// <summary> 所有“中奖图标和线”轮流显示，不带动画 </summary>
        PerSymbolLine,

        /// <summary> 所有“中奖图标和线”轮流显示，带动画 </summary>
        PerSymbolLineAnim,
    }

    [Serializable]
    public class SymbolLineShowInfo
    {
        /// <summary>显示的类型 </summary>
        [Tooltip("显示的类型")]
        public SymbolLineShowType showType = SymbolLineShowType.None;

        /// <summary>显示时间 </summary>
        [Tooltip("显示时间,单位秒")]
        public float showTimeS = 0f;
    }

}
