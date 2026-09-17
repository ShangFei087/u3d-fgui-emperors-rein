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
        //注册断电重连
        static FreeSpinSessionStoreG3993()
        {
            EnsureRegistered();
        }

        /// <summary> 向 20200 注册本机台 lastAck，进台前调用一次即可。 </summary>
        public static void EnsureRegistered()
        {
            SBoxIdea.RegisterFreeSpinLastAck(GameId, GetLastAckOrUnconfirmed);
        }

        /// <summary> 免费重连存档：按游戏 ID 区分。 </summary>
        static string BuildKey()=> $"SlotFreeSpinSession_v4_{GameId}";

        /// <summary>
        ///  lastAck：已完整收到的赠送下标；从未确认过返回 -1。
        /// </summary>
        public static int GetLastAckOrUnconfirmed(int playerId)
        {
            var snap = TryLoad();
            if (snap == null)
                return -1;
            return snap.LastAck;
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
        /// 保存或清除免费重连数据
        /// </summary>
        public static void TryPersistOrClearSession(Action onComplete = null)
        {
            if (ApplicationSettings.Instance.isMock)
            {
                DebugUtils.Log("[G3993] 跳过免费重连：Mock");
                onComplete?.Invoke();
                return;
            }

            if (!SQLitePlayerPrefs03.Instance.isInit)
            {
                DebugUtils.LogError("[G3993] 跳过免费重连：SQLite 未初始化");
                onComplete?.Invoke();
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
                onComplete?.Invoke();
                return;
            }

            //保存免费数据
            int lastAck = cm.freeSpinPlayTimes;
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
                onComplete?.Invoke();
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

        /// <summary> 重连时用本地屏幕总积分覆盖展示（免费未结束前不跟算法分）。 </summary>
        public static void ApplyDisplayCreditToUi(long displayCredit)
        {
            MainBlackboardController.Instance.SetMyRealCredit(displayCredit);
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
        }

        /// <summary> 读取数据库的免费重连数据 </summary>
        static FreeSpinSessionSnapshotG3993 TryDeserialize()
        {
            string json = SQLitePlayerPrefs03.Instance.GetString(BuildKey(), "");
            if (string.IsNullOrEmpty(json))
                return null;
            return JsonConvert.DeserializeObject<FreeSpinSessionSnapshotG3993>(json);
        }

        /// <summary> 应用算法免费数据(当本地数据失效时) </summary>
        public static void ApplyAlgoProgressToContentModel()
        {
            var cm = ContentModel.Instance;
            var reset = SBoxIdea.LastResetData;
            if (reset == null || reset.FreePersistGameId != GameId || !reset.HasFreePersistSnapshot)
                return;

            //bug:一般来说本地数据失效都是触发局没有保存导致的,所以此函数暂时只针对触发局没有保存本地免费数据。
            cm.freeSpinTotalTimes = reset.FreePersistTotalTimes;
            cm.freeSpinPlayTimes = reset.FreePersistCurIdx;
            cm.ShowFreeSpinRemainTime = reset.FreePersistRemain;
            bool inFree = reset.FreePersistRemain > 0;
            cm.PendingAlgoFreeSettle = !inFree && reset.FreePersistCurIdx > 0;
            cm.nextReelStripsIndex = inFree ? "FS" : "BS";
            // 一局免费都没触发：当前盘还是触发局 BS；打过免费：当前也是 FS
            cm.curReelStripsIndex = (!inFree || reset.FreePersistCurIdx == 0) ? "BS" : "FS";
            if (reset.FreePersistCurIdx == 0 && inFree)
                cm.curReelStripsIndex = "BS";
            else if (inFree)
                cm.curReelStripsIndex = "FS";
            else
                cm.curReelStripsIndex = "BS";

            int lineNum = MainModel.Instance.lineNum;
            if (reset.FreePersistBet > 0)
            {
                cm.betmultiple = reset.FreePersistBet;
                long totalBet = (long)reset.FreePersistBet * lineNum;
                cm.totalBet = totalBet;
                var betList = SBoxModel.Instance.betList;
                if (betList != null)
                {
                    for (int i = 0; i < betList.Count; i++)
                    {
                        if (betList[i] == totalBet)
                        {
                            cm.betIndex = i;
                            break;
                        }
                    }
                }
            }
          
            //--TotalPantherSymbolCount
            //cm.triggerWinCredit 
            //--DisplayCredit

            DebugUtils.Log($"[G3993] 无本地免费数据，用 20000 补 playTimes={cm.freeSpinPlayTimes}/{cm.freeSpinTotalTimes} remain={cm.ShowFreeSpinRemainTime} pendingSettle={cm.PendingAlgoFreeSettle}");
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
