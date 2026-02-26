
using System;

namespace GameMaker
{
    public enum WinLevelType
    {
        None = 0,
        BIG,
        HUGE,
        MASSIVE,
        LEGENDARY,
    }
    
    [System.Serializable]
    public class WinMultiple
    {
        public WinLevelType winLevelType;
        public long multiple;

        public WinMultiple(string winLevelType, long mul)
        {
            // 尝试将字符串转换为枚举，失败时使用默认值并报错
            if (Enum.TryParse(winLevelType, out WinLevelType parsedType))
            {
                this.winLevelType = parsedType;
            }
            else
            {
                this.winLevelType = WinLevelType.None; // 设为默认值
                DebugUtils.LogError($"无效的WinLevelType字符串: {winLevelType}");
            }
            this.multiple = mul;
        }
    }
    
}
