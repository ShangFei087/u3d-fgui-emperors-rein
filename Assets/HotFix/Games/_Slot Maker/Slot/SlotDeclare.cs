namespace SlotMaker
{
    public static class GameState
    {
        public const string Idle = "Idle";
        public const string Spin = "Spin";
        public const string FreeSpin = "FreeSpin";
        public const string MiniGame = "MiniGame";
    }


    public static class SpinButtonState
    {
        /// <summary> 游戏停止 </summary>
        public const string Stop = "Stop";
        /// <summary> 游戏游玩中 </summary>
        public const string Spin = "Spin";
        /// <summary> 游戏自动游玩中 </summary>
        public const string Auto = "Auto";
        /// <summary> 游戏按钮置灰中 </summary>
        public const string Hui = "Hui";
    }
}