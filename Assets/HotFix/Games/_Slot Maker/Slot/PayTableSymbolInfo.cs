namespace SlotMaker
{
    [System.Serializable]
 
    public class PayTableSymbolInfo
    {
        public int symbol;
        public int x2;
        public int x3;
        public int x4;
        public int x5;
    }

    /// <summary>
    /// 触发免费奖方式
    /// </summary>
    public enum MakeFreeGameType
    {
        OnScatter,      // 集齐Scatter图标触发免费奖
        OnCharge,       // 充能方式触发免费奖
    }

    /// <summary>
    /// 免费奖配置
    /// </summary>
    public class FreeGameConfig
    {
        /// <summary>
        /// 是否使用公共的免费次数框
        /// </summary>
        public bool IsUseCommonFreeTimes { get; set; } = false;

        /// <summary>
        /// 是否有免费奖
        /// </summary>
        public bool IsHasFreeGame { get; set; } = true;

        /// <summary>
        /// 触发免费奖方式
        /// </summary>
        public MakeFreeGameType FreeGameType { get; set; } = MakeFreeGameType.OnScatter;

        /// <summary>
        /// Scatter图标是否依赖中奖线
        /// </summary>
        public bool IsScatterInLine { get; set; } = true;

        /// <summary>
        /// 触发免费奖所需数量(Scatter图标/充能)
        /// </summary>
        public int[] Make2FreeGameCount { get; set; } = new int[0];

        /// <summary>
        /// 免费次数
        /// </summary>
        public int[] FreeGameTime { get; set; } = new int[0];

    }

    /// <summary>
    /// 触发大奖方式
    /// </summary>
    public enum MakeBonusGameType
    {
        OnBonus,        // 集齐Bonus图标触发大奖
                        // 可根据需要添加其他触发方式
    }


    /// <summary>
    /// 大奖配置
    /// </summary>
    public class BonusGameConfig
    {
        /// <summary>
        /// 是否有大奖
        /// </summary>
        public bool IsHasBonusGame { get; set; } = true;

        /// <summary>
        /// 触发大奖方式
        /// </summary>
        public MakeBonusGameType BonusGameType { get; set; } = MakeBonusGameType.OnBonus;

        /// <summary>
        /// Bonus图标是否依赖中奖线
        /// </summary>
        public bool IsBonusInLine { get; set; } = true;

        /// <summary>
        /// 触发大奖所需数量(Bonus图标)
        /// </summary>
        public int Make2BonusGameCount { get; set; } = 3;
    }
}
