using System;
using System.Data;
using UnityEngine;

/// <summary>
/// 游戏记录表（slot_game_record）容量：查询条数、调整上限、立即按上限整理溢出数据。
/// </summary>
public static class SlotGameRecordCapacityUtils
{
    public const string ExpandMenuManual = "manual";
    public const string ExpandMenuToDefault = "expand_default";
    public const string ExpandMenuDouble = "expand_double";
    public const string ExpandMenuTrimNow = "trim_now";

    /// <summary>查询 slot_game_record 当前条数；-1 表示数据库未就绪。</summary>
    public static void QuerySlotGameRecordCount(Action<int> onCount)
    {
        if (SQLiteAsyncHelper.Instance == null || !SQLiteAsyncHelper.Instance.isConnect)
        {
            onCount?.Invoke(-1);
            return;
        }

        string sql = $"SELECT COUNT(*) FROM {ConsoleTableName.TABLE_SLOT_GAME_RECORD}";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, (DataTable dt) =>
        {
            int count = 0;
            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
                count = Convert.ToInt32(dt.Rows[0][0]);
            onCount?.Invoke(count);
        });
    }

    /// <summary>扩容至当前工程默认上限。</summary>
    public static int CalcExpandToDefault() => DefaultSettingsUtils.defMaxGameRecord;

    /// <summary>在当前上限基础上翻倍（受 min/max 钳制）。</summary>
    public static int CalcExpandDouble()
    {
        long doubled = SBoxModel.Instance.gameRecordMax * 2L;
        return (int)Mathf.Clamp(doubled, DefaultSettingsUtils.minMaxGameRecord, DefaultSettingsUtils.maxMaxGameRecord);
    }

    /// <summary>
    /// 设置游戏记录上限；<paramref name="trimOverflowNow"/> 为 true 时立即按新上限删除最旧溢出记录。
    /// </summary>
    public static void ApplyMaxGameRecord(int newMax, bool trimOverflowNow, Action<bool, string> onDone)
    {
        newMax = Mathf.Clamp(newMax, DefaultSettingsUtils.minMaxGameRecord, DefaultSettingsUtils.maxMaxGameRecord);
        long curMax = SBoxModel.Instance.gameRecordMax;

        if (newMax == curMax && !trimOverflowNow)
        {
            onDone?.Invoke(true, null);
            return;
        }

        SBoxModel.Instance.gameRecordMax = newMax;

        if (!trimOverflowNow)
        {
            onDone?.Invoke(true, null);
            return;
        }

        TrimSlotGameRecordOverflow(newMax, onDone);
    }

    /// <summary>按当前上限立即整理 slot_game_record（删除超出 created_at 最旧的记录）。</summary>
    public static void TrimSlotGameRecordOverflowNow(Action<bool, string> onDone)
    {
        TrimSlotGameRecordOverflow((int)SBoxModel.Instance.gameRecordMax, onDone);
    }

    static void TrimSlotGameRecordOverflow(int maxRecord, Action<bool, string> onDone)
    {
        if (SQLiteAsyncHelper.Instance == null || !SQLiteAsyncHelper.Instance.isConnect)
        {
            onDone?.Invoke(false, "数据库未就绪");
            return;
        }

        SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(
            ConsoleTableName.TABLE_SLOT_GAME_RECORD,
            maxRecord,
            "created_at",
            (object[] res) =>
            {
                if (res != null && res.Length > 0 && Convert.ToInt32(res[0]) != 0)
                {
                    string msg = res.Length > 1 ? res[1]?.ToString() : "整理失败";
                    onDone?.Invoke(false, msg);
                    return;
                }

                onDone?.Invoke(true, null);
            });
    }
}
