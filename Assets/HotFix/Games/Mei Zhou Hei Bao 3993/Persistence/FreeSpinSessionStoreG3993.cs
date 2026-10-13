using GameMaker;
using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Security.Cryptography;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class FreeSpinSessionStoreG3993
    {
        /// <summary>
        /// 当前游戏 ID。
        /// </summary>
        public const int GameId = 3993;

        /// <summary> 免费重连存档：按游戏 ID 区分。 </summary>
        static string BuildKey()=> $"SlotFreeSpinSession_v4_{GameId}";

        /// <summary>
        /// 异常 1.未确认触发、2.没有有效档，3.或本地进度超前于 20000 游标时回 -1，算法把赠送退回第一手。
        /// 正常 本地进度没有超前时，回最后一次 20101 的下标。
        /// </summary>
        public static int GetLastAckOrUnconfirmed(int playerId)
        {
            if (IsAlgoTriggerUnacked())
                return -1;
            var snap = TryLoad();
            if (snap == null || !IsSessionStillValid(snap) || snap.FreeSpinPlayTimes <= 0)
                return -1;
            var reset = SBoxIdea.LastResetData;
            if (reset != null && reset.HasFreePersistSnapshot && reset.FreePersistGameId == GameId
                && snap.FreeSpinPlayTimes > reset.FreePersistCurIdx)
                return -1;
            return snap.LastAck > 0 ? snap.LastAck : snap.FreeSpinPlayTimes;
        }

        /// <summary>算法有免费数据，且触发局尚未被确认。 </summary>
        public static bool IsAlgoTriggerUnacked()
        {
            var reset = SBoxIdea.LastResetData;
            return reset != null && reset.FreePersistGameId == GameId && reset.IsTriggerUnacked;
        }

        /// <summary> 算法重启是否带有免费重连数据。 </summary>
        public static bool HasAlgoFreePersistSnapshot()
        {
            var reset = SBoxIdea.LastResetData;
            return reset != null && reset.FreePersistGameId == GameId && reset.HasFreePersistSnapshot;
        }

        /// <summary> 本地数据是否带有免费重连数据 </summary>
        public static bool IsSessionStillValid(FreeSpinSessionSnapshotG3993 snap)
        {
            if (snap == null)
                return false;

            if (snap.PendingAlgoFreeSettle)
                return true;

            if (snap.FreeSpinTotalTimes <= 0)
                return false;

            if (snap.FreeSpinPlayTimes < snap.FreeSpinTotalTimes)
                return true;

            return snap.FreeSpinPlayTimes == 0 && snap.NextReelStripsIndex == "FS";
        }

        /// <summary>
        /// 是否需要保存免费重连。
        /// </summary>
        public static bool ShouldPersistSession()
        {
            var cm = ContentModel.Instance;
            if (cm.freeSpinTotalTimes <= 0)
                return false;

            if (cm.PendingAlgoFreeSettle)
                return true;

            if (cm.nextReelStripsIndex == "FS")
                return true;

            if (cm.freeSpinPlayTimes == 0 && cm.isFreeSpinTrigger)
                return true;

            return false;
        }

        /// <summary>
        /// 是否应清除免费重连
        /// </summary>
        public static bool ShouldClearSession()
        {
            var cm = ContentModel.Instance;
            if (cm.PendingAlgoFreeSettle)
                return false;
            return cm.freeSpinTotalTimes > 0 && cm.freeSpinPlayTimes >= cm.freeSpinTotalTimes && cm.nextReelStripsIndex == "BS";
        }

        /// <summary>
        /// 保存或清除免费重连数据。onComplete 的参数为是否可以继续发 20102。
        /// 免费进行中且没写进库时为 false，调用方必须重试，不能确认这手。
        /// </summary>
        public static void TryPersistOrClearSession(Action<bool> onComplete = null)
        {
            if (ApplicationSettings.Instance.isMock)
            {
                DebugUtils.Log("[G3993] 跳过免费重连：Mock");
                onComplete?.Invoke(true);
                return;
            }

            if (!SQLitePlayerPrefs03.Instance.isInit)
            {
                bool mustSave = ShouldPersistSession();
                DebugUtils.LogError("[G3993] 跳过免费重连：SQLite 未初始化");
                onComplete?.Invoke(!mustSave);
                return;
            }

            string key = BuildKey();
            var cm = ContentModel.Instance;

            //判断是否保存免费奖
            if (ShouldClearSession() || !ShouldPersistSession())
            {
                //清除免费数据
                SQLitePlayerPrefs03.Instance.DeleteKey(key);
                DebugUtils.Log($"[G3993] 已清除免费重连 playTimes={cm.freeSpinPlayTimes}/{cm.freeSpinTotalTimes} next={cm.nextReelStripsIndex} pendingSettle={cm.PendingAlgoFreeSettle}");
                onComplete?.Invoke(true);
                return;
            }

            // 赠送下标。触发局 playTimes 仍是 0，不能把 0 当成已确认的赠送。
            int lastAck = cm.freeSpinPlayTimes > 0 ? cm.freeSpinPlayTimes : -1;
            var snap = new FreeSpinSessionSnapshotG3993
            {
                FreeSpinTotalTimes = cm.freeSpinTotalTimes,//免费总次数
                FreeSpinPlayTimes = cm.freeSpinPlayTimes,//免费触发次数
                LastAck = lastAck,//免费触发次数下标
                PendingAlgoFreeSettle = cm.PendingAlgoFreeSettle,//算法确认免费是否结束
                FreeSpinTotalWinCredit = cm.freeSpinTotalWinCredit,//免费总赢
                TriggerWinCredit = cm.triggerWinCredit,//触发局赢分
                DisplayCredit = MainBlackboardController.Instance.myTempCredit, //免费游戏进去前的总积分
                CurReelStripsIndex = cm.curReelStripsIndex,
                NextReelStripsIndex = cm.nextReelStripsIndex,
                BetIndex = cm.betIndex,
                BetMultiple = cm.betmultiple,
                TotalBet = cm.totalBet,
                GameNumberFreeSpinTrigger = cm.gameNumberFreeSpinTrigger,
                StrDeckRowCol = cm.strDeckRowCol,
                WildData = cm.wildData != null && cm.wildData.Length > 0
                    ? (int[])cm.wildData.Clone()
                    : null,
                TotalPantherSymbolCount = cm.totalPantherSymbolCount,
                SavedUtcMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            string json = JsonConvert.SerializeObject(snap);
            SQLitePlayerPrefs03.Instance.SetStringDurable(key, json, saved =>
            {
                if (!saved)
                    DebugUtils.LogError($"[G3993] 免费重连写入失败 playTimes={cm.freeSpinPlayTimes}/{cm.freeSpinTotalTimes} lastAck={lastAck} panther={cm.totalPantherSymbolCount}");
                else
                    DebugUtils.Log($"[G3993] 免费重连已保存 playTimes={cm.freeSpinPlayTimes}/{cm.freeSpinTotalTimes} lastAck={lastAck} remain={cm.freeSpinTotalTimes - cm.freeSpinPlayTimes} panther={cm.totalPantherSymbolCount}");
                onComplete?.Invoke(saved);
            });
        }

        /// <summary>
        /// 读取本地 3993 免费重连数据。
        /// </summary>
        public static FreeSpinSessionSnapshotG3993 TryLoad()
        {
            if (ApplicationSettings.Instance.isMock)
                return null;

            if (!SQLitePlayerPrefs03.Instance.isInit)
                return null;

            try
            {
                var snap = TryDeserialize();
                if (snap == null)
                    return null;
                if (snap.GameId != GameId)
                    return null;
                return snap;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[G3993] 免费重连反序列化失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清除本地 3993 免费重连数据。
        /// </summary>
        public static void Clear()
        {
            if (!SQLitePlayerPrefs03.Instance.isInit)
                return;
            SQLitePlayerPrefs03.Instance.DeleteKey(BuildKey());
        }

        /// <summary> 读取数据库的免费重连数据 </summary>
        static FreeSpinSessionSnapshotG3993 TryDeserialize()
        {
            string json = SQLitePlayerPrefs03.Instance.GetString(BuildKey(), "");
            if (string.IsNullOrEmpty(json))
                return null;
            return JsonConvert.DeserializeObject<FreeSpinSessionSnapshotG3993>(json);
        }

        /// <summary> 正常重连：进度、赢分、押注、百搭和盘面字符串都用本地档。 </summary>
        public static void ApplyLocalSnapshot(FreeSpinSessionSnapshotG3993 snap)
        {
            if (snap == null)
                return;
            var cm = ContentModel.Instance;
            cm.freeSpinTotalTimes = snap.FreeSpinTotalTimes;
            cm.freeSpinPlayTimes = snap.FreeSpinPlayTimes;
            cm.freeSpinTotalWinCredit = snap.FreeSpinTotalWinCredit;
            cm.triggerWinCredit = snap.TriggerWinCredit;
            cm.curReelStripsIndex = snap.CurReelStripsIndex;
            cm.nextReelStripsIndex = snap.NextReelStripsIndex;
            cm.gameNumberFreeSpinTrigger = snap.GameNumberFreeSpinTrigger;
            cm.totalPantherSymbolCount = snap.TotalPantherSymbolCount;
            cm.PendingAlgoFreeSettle = snap.PendingAlgoFreeSettle;
            cm.ShowFreeSpinRemainTime = cm.freeSpinTotalTimes - cm.freeSpinPlayTimes;

            if (snap.BetIndex >= 0 && SBoxModel.Instance.betList != null
                && snap.BetIndex < SBoxModel.Instance.betList.Count)
            {
                cm.betIndex = snap.BetIndex;
                cm.totalBet = SBoxModel.Instance.betList[cm.betIndex];
            }
            else
            {
                cm.totalBet = snap.TotalBet;
            }

            cm.betmultiple = snap.BetMultiple;
            cm.wildData = snap.WildData != null && snap.WildData.Length > 0
                ? (int[])snap.WildData.Clone()
                : Array.Empty<int>();
            if (!string.IsNullOrEmpty(snap.StrDeckRowCol))
                cm.strDeckRowCol = snap.StrDeckRowCol;
        }

        /// <summary>
        /// 异常重连：只用 20200 同步后的游标。游标还没回到就直接返回，不用退回前的 20000 下标。
        /// </summary>
        public static void ApplyAlgoProgressToContentModel()
        {
            var cm = ContentModel.Instance;
            var reset = SBoxIdea.LastResetData;
            if (reset == null || reset.FreePersistGameId != GameId || !reset.HasFreePersistSnapshot)
                return;

            int playTimes = SBoxIdea.LastGameSwitchSyncedFreeIdx;
            if (playTimes < 0)
                return;
            if (reset.FreePersistTotalTimes > 0 && playTimes > reset.FreePersistTotalTimes)
                playTimes = reset.FreePersistTotalTimes;

            cm.freeSpinTotalTimes = reset.FreePersistTotalTimes;
            cm.freeSpinPlayTimes = playTimes;
            cm.ShowFreeSpinRemainTime = reset.FreePersistTotalTimes - playTimes;
            bool inFree = playTimes < reset.FreePersistTotalTimes;
            cm.PendingAlgoFreeSettle = !inFree && playTimes > 0;
            cm.nextReelStripsIndex = inFree ? "FS" : "BS";
            cm.curReelStripsIndex = inFree && playTimes > 0 ? "FS" : "BS";

            ApplyAlgoBet(cm, reset);
            cm.freeSpinTotalWinCredit = 0;
            cm.totalPantherSymbolCount = 0;
            cm.triggerWinCredit = (long)reset.FreePersistBet * reset.BaseGameWinCredit;

            DebugUtils.Log($"[G3993] 按同步游标从算法恢复 playTimes={cm.freeSpinPlayTimes}/{cm.freeSpinTotalTimes} remain={cm.ShowFreeSpinRemainTime} pendingSettle={cm.PendingAlgoFreeSettle} triggerLine={cm.triggerWinCredit}");
        }

        /// <summary>
        /// 触发局未确认：丢掉本地「已在免费中」的进度，停在普通局。
        /// 押注沿用 20000，并标记本把不再扣展示分。
        /// </summary>
        public static void PrepareUnackedTriggerReplay()
        {
            var cm = ContentModel.Instance;
            ResetContentModelFreeStateToBaseGame();
            var reset = SBoxIdea.LastResetData;
            if (reset != null)
                ApplyAlgoBet(cm, reset);
            cm.SuppressBetDeduct = true;
            cm.SpinBetDeducted = true;
        }

        static void ApplyAlgoBet(ContentModel cm, SBoxResetData reset)
        {
            if (cm == null || reset == null || reset.FreePersistBet <= 0)
                return;
            int lineNum = MainModel.Instance.lineNum;
            cm.betmultiple = reset.FreePersistBet;
            long totalBet = (long)reset.FreePersistBet * lineNum;
            cm.totalBet = totalBet;
            var betList = SBoxModel.Instance.betList;
            if (betList == null)
                return;
            for (int i = 0; i < betList.Count; i++)
            {
                if (betList[i] == totalBet)
                {
                    cm.betIndex = i;
                    break;
                }
            }
        }

        /// <summary> 清空免费相关运行时状态，回到主游戏。 </summary>
        public static void ResetContentModelFreeStateToBaseGame()
        {
            var cm = ContentModel.Instance;
            cm.freeSpinTotalTimes = 0;
            cm.freeSpinPlayTimes = 0;
            cm.freeSpinTotalWinCredit = 0;
            cm.triggerWinCredit = 0;
            cm.curReelStripsIndex = "BS";
            cm.nextReelStripsIndex = "BS";
            cm.isFreeSpinTrigger = false;
            cm.isFreeSpinFinish = false;
            cm.isFreeGameAdd = false;
            cm.freeSpinAddNum = 0;
            cm.ShowFreeSpinRemainTime = 0;
            cm.gameNumberFreeSpinTrigger = 0;
            cm.PendingFreeSpinReconnectValidation = false;
            cm.PendingAlgoFreeSettle = false;
            cm.totalPantherSymbolCount = 0;
            cm.wildData = Array.Empty<int>();
            cm.SpinBetDeducted = false;
            cm.SuppressBetDeduct = false;
        }

        /// <summary>
        /// 免费已结束：删本地免费数据、清算法免费数据。
        /// resetRuntime 为 false 时保留 ContentModel（结算弹窗还要读次数和赢分）。
        /// </summary>
        public static void FinishAndClearAll(bool resetRuntime = true)
        {
            Clear();
            SBoxIdea.ClearFreePersistSnapshot();
            if (resetRuntime)
                ResetContentModelFreeStateToBaseGame();
        }
    }
}
