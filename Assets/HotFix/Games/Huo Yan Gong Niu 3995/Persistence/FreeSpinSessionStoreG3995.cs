using Newtonsoft.Json;
using SBoxApi;
using UnityEngine;

namespace HuoYanGongNiu_3995
{
    public static class FreeSpinSessionStoreG3996
    {
        /// <summary>
        /// 当前游戏 ID（朱再金币 1700）。
        /// </summary>
        const int GameId = 3995;

        /// <summary>
        /// 生成免费局会话存档键，按「会话版本 + 游戏 ID + 玩家 ID」区分。
        /// </summary>
        static string BuildKey(int playerId) => $"SlotFreeSpinSession_v{FreeSpinSessionSnapshotG3995.CurrentSessionVersion}_{GameId}_{playerId}";

        /// <summary>
        /// 是否需要持久化当前免费局状态。
        /// </summary>
        /// <returns>
        /// true：仍在免费局流程中，或首帧已进入 FS 但尚未开转；
        /// false：无需保存。
        /// </returns>
        public static bool ShouldPersistSession()
        {
            var cm = ContentModel.Instance;
            if (cm.freeSpinTotalTimes <= 0)
                return false;

            if (cm.freeSpinPlayTimes < cm.freeSpinTotalTimes)
                return true;

            if (cm.freeSpinPlayTimes == 0 && cm.nextReelStripsIndex == "FS")
                return true;

            return false;
        }

        /// <summary>
        /// 是否应主动清除会话。
        /// </summary>
        /// <remarks>
        /// 当免费局已打完并且下一盘回到主游戏（BS）时，存档应删除。
        /// </remarks>
        public static bool ShouldClearSession()
        {
            var cm = ContentModel.Instance;
            return cm.freeSpinTotalTimes > 0 && cm.freeSpinPlayTimes >= cm.freeSpinTotalTimes && cm.nextReelStripsIndex == "BS";
        }

        /// <summary>
        /// 根据当前状态尝试保存或清除免费局会话。
        /// </summary>
        public static void TryPersistOrClearSession()
        {
            if (ApplicationSettings.Instance.isMock)
                return;

            if (!SQLitePlayerPrefs03.Instance.isInit)
                return;

            int pid = SBoxModel.Instance.pid;
            string key = BuildKey(pid);

            if (ShouldClearSession() || !ShouldPersistSession())
            {
                SQLitePlayerPrefs03.Instance.DeleteKey(key);
                return;
            }

            var cm = ContentModel.Instance;
            var snap = new FreeSpinSessionSnapshotG3995
            {
                PlayerId = pid,
                FreeSpinTotalTimes = cm.freeSpinTotalTimes,
                FreeSpinPlayTimes = cm.freeSpinPlayTimes,
                FreeSpinTotalWinCredit = cm.freeSpinTotalWinCredit,
                CurReelStripsIndex = cm.curReelStripsIndex,
                NextReelStripsIndex = cm.nextReelStripsIndex,
                BetIndex = cm.betIndex,
                BetMultiple = cm.betmultiple,
                TotalBet = cm.totalBet,
                GameNumberFreeSpinTrigger = cm.gameNumberFreeSpinTrigger,
                StrDeckRowCol = cm.strDeckRowCol,
                realCredit = cm.realCredit,

                //wildNum = cm.wildNums,
                //curFreeCredit = cm.curFreeCredit,
                //newFreeOnceCredit = cm.newFreeOnceCredit,
                //tempFreeTotalTimes = cm.tempFreeTotalTimes,

                SavedUtcMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            string json = JsonConvert.SerializeObject(snap);
            SQLitePlayerPrefs03.Instance.SetString(key, json);
        }

        /// <summary>
        /// 尝试读取指定玩家的免费局快照。
        /// </summary>
        /// <param name="playerId">玩家 ID。</param>
        /// <returns>有效快照返回对象；无数据或校验失败返回 null。</returns>
        public static FreeSpinSessionSnapshotG3995 TryLoad(int playerId)
        {
            if (ApplicationSettings.Instance.isMock)
                return null;

            if (!SQLitePlayerPrefs03.Instance.isInit)
                return null;

            string key = BuildKey(playerId);
            string json = SQLitePlayerPrefs03.Instance.GetString(key, "");
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var snap = JsonConvert.DeserializeObject<FreeSpinSessionSnapshotG3995>(json);
                if (snap == null || snap.SessionVersion != FreeSpinSessionSnapshotG3995.CurrentSessionVersion)
                    return null;
                if (snap.GameId != GameId || snap.PlayerId != playerId)
                    return null;
                return snap;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[G3996] FreeSpinSession 反序列化失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清除指定玩家的免费局本地会话。
        /// </summary>
        public static void Clear(int playerId)
        {
            if (!SQLitePlayerPrefs03.Instance.isInit)
                return;
            SQLitePlayerPrefs03.Instance.DeleteKey(BuildKey(playerId));
        }

        /// <summary> 与算法回包不一致时清空免费相关运行时状态，回到主游戏。 </summary>
        public static void ResetContentModelFreeStateToBaseGame()
        {
            var cm = ContentModel.Instance;
            cm.freeSpinTotalTimes = 0;
            cm.freeSpinPlayTimes = 0;
            cm.freeSpinTotalWinCredit = 0;
            cm.curReelStripsIndex = "BS";
            cm.nextReelStripsIndex = "BS";
            cm.isFreeSpinTrigger = false;
            cm.isFreeSpinResult = false;
            cm.isFreeSpinAdd = false;
            cm.freeSpinAddNum = 0;
            cm.showFreeSpinRemainTime = 0;
            cm.gameNumberFreeSpinTrigger = 0;
            cm.PendingFreeSpinReconnectValidation = false;
            cm.newFreeOnceCredit.Clear();
            cm.realCredit = 0;


            //cm.wildNums = 0;
            //cm.curFreeCredit = 0;
            //cm.tempFreeTotalTimes = 0;
        }
    }
}
