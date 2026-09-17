using System;

namespace MeiZhouHeiBao_3993
{
    /// <summary>
    /// 免费重连本地数据（断电/强退后恢复 UI 与局数进度）。
    /// 算法卡是否在断电后保持同一免费序列需与硬件确认；若不一致，首局 Spin 会校验并清除。
    /// </summary>
    [Serializable]
    public class FreeSpinSessionSnapshotG3993
    {
        /// <summary> 游戏 ID（用于跨游戏隔离校验）。 </summary>
        public int GameId = 3993;

        /// <summary> 免费局总次数。 </summary>
        public int FreeSpinTotalTimes;
        /// <summary> 当前已进行的免费局次数（与算法 nCurFreeIdx 对齐：已完整收到手数）。 </summary>
        public int FreeSpinPlayTimes;
        /// <summary> 前端已完整收到的赠送下标；-1 表示从未确认。 </summary>
        public int LastAck = -1;
        /// <summary> 最后一手已发出、等 20102 入账清快照。 </summary>
        public bool PendingAlgoFreeSettle;
        /// <summary> 免费局累计赢分 </summary>
        public long FreeSpinTotalWinCredit;

        /// <summary> 触发免费那一句的线奖+豹奖，进免费后不再改。 </summary>
        public long TriggerWinCredit;

        /// <summary> 基础游戏赢分（单局普通游戏） </summary>
        public long BaseGameWinCredit;

        /// <summary> 断电时屏幕上的总积分（已扣触发押注、未合入免费赢分）。 </summary>
        public long DisplayCredit;

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

        /// <summary> 行优先 3×5 Wild 倍数（0/2/3/5），重连后刷 X2/X3/X5 静图。 </summary>
        public int[] WildData;

        /// <summary> 已收集黑豹数量（恢复收集盒进度）。 </summary>
        public int TotalPantherSymbolCount;

        /// <summary> 快照保存时间（UTC 毫秒时间戳）。 </summary>
        public long SavedUtcMs;
    }
}


