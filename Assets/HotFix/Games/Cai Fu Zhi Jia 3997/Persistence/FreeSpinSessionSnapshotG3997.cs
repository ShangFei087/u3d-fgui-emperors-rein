namespace CaiFuZhiJia_3997
{
    public class FreeSpinSessionSnapshotG3997
    {
        /// <summary>
        /// 当前快照结构版本。字段结构有变更时需递增。
        /// </summary>
        public const int CurrentSessionVersion = 1;

        /// <summary> 快照版本号（用于反序列化后兼容校验）。 </summary>
        public int SessionVersion = CurrentSessionVersion;
        /// <summary> 游戏 ID（用于跨游戏隔离校验）。 </summary>
        public int GameId = 3997;
        /// <summary> 玩家 ID（用于跨账号隔离校验）。 </summary>
        public int PlayerId;

        /// <summary> 免费局总次数。 </summary>
        public int FreeSpinTotalTimes;
        /// <summary> 当前已进行的免费局次数。 </summary>
        public int FreeSpinPlayTimes;
        /// <summary> 免费局累计赢分。 </summary>
        public long FreeSpinTotalWinCredit;

        /// <summary> 当前局使用的轴带标识（BS/FS）。 </summary>
        public string CurReelStripsIndex = "BS";
        /// <summary> 下一局将使用的轴带标识（BS/FS）。 </summary>
        public string NextReelStripsIndex = "BS";

        /// <summary> 下注档位索引。 </summary>
        public int BetIndex;
        /// <summary> 下注倍数。 </summary>
        public int BetMultiple;
        /// <summary> 总下注额（用于恢复展示和校验）。 </summary>
        public long TotalBet;

        /// <summary> 触发免费局时对应的局号。 </summary>
        public int GameNumberFreeSpinTrigger;

        /// <summary> 最后一局已落地的盘面，用于恢复滚轮显示 </summary>
        public string StrDeckRowCol;

        /// <summary> 快照保存时间（UTC 毫秒时间戳）。 </summary>
        public long SavedUtcMs;

        /// <summary> 免费游戏倍率。 </summary>
        public int FreeGameScoreMultiply;

        /// <summary> 断电前免费得分 </summary>
        public long CurrentWinBet;
    }
}


