using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameHistoryInfo
{
    public int curPageNumber;           // 当前页码
    public int totalPageCount;           // 总页数
    public int currentIndex;              // 当前记录在列表中的索引
    public int totalRecordCount;          // 总记录数
    public TableSlotGameRecordItem currentRecord;  // 当前显示的记录
    public List<TableSlotGameRecordItem> allRecords; // 所有记录
    public long currentGameId;            // 当前游戏ID
    public string currentDateTime;        // 当前日期时间（精确到秒）
}

public class GameHistoryDataController : MonoBehaviour
{
    string tabName = ConsoleTableName.TABLE_SLOT_GAME_RECORD;
    // 当前页数（从0开始）
    int curPageIndex = 0;
    // 总的页数
    int totalPageCount = 0;
    // 总记录数
    int totalRecordCount = 0;

    const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
    List<string> dropdownDateTimeLst;
    List<long> dropdownGameIdLst;

    Action<List<string>> onDatesChange = null;
    Action<List<long>> onGameIdsChange = null;
    Action<GameHistoryInfo> onPageChagne = null;

    // 存储当前选中的日期时间和游戏ID
    string currentSelectedDateTime = "";
    long currentGameId = 0;
    // 当前查询的所有记录
    List<TableSlotGameRecordItem> allRecords = new List<TableSlotGameRecordItem>();

    public void InitParam(string tabName,Action<List<string>> onDatesChange,Action<List<long>> onGameIdsChange, Action<GameHistoryInfo> onPageChagne)
    {
        this.tabName = tabName;
        this.onDatesChange = onDatesChange;
        this.onGameIdsChange = onGameIdsChange;
        this.onPageChagne = onPageChagne;

        // 初始化时加载所有游戏ID
        LoadAllGameIds();
    }

    // 加载所有有记录的游戏ID
    void LoadAllGameIds()
    {
        string sql = $"SELECT DISTINCT game_id FROM {tabName} ORDER BY game_id";

        dropdownGameIdLst = new List<long>();

        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (SqliteDataReader sdr) =>
        {
            while (sdr.Read())
            {
                long gameId = sdr.GetInt64(0);
                dropdownGameIdLst.Add(gameId);
            }

            onGameIdsChange?.Invoke(dropdownGameIdLst);
        });
    }

    // 根据游戏ID加载对应的日期时间列表（精确到秒）
    public void LoadDateTimesByGameId(long gameId)
    {
        currentGameId = gameId;

        string sql = $@"
            SELECT DISTINCT DATETIME(created_at / 1000, 'unixepoch', 'localtime') as datetime 
            FROM {tabName} 
            WHERE game_id = {gameId} 
            ORDER BY datetime DESC";

        dropdownDateTimeLst = new List<string>();

        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (SqliteDataReader sdr) =>
        {
            while (sdr.Read())
            {
                string datetime = sdr.GetString(0);
                // 确保格式为 yyyy-MM-dd HH:mm:ss
                if (!string.IsNullOrEmpty(datetime))
                {
                    // 如果长度不够，补全秒数
                    if (datetime.Length == 16) // yyyy-MM-dd HH:mm
                    {
                        datetime += ":00";
                    }
                    dropdownDateTimeLst.Add(datetime);
                }
            }

            onDatesChange?.Invoke(dropdownDateTimeLst);
        });
    }

    /// <summary>
    /// 根据game_id和日期时间（精确到秒）查询数据
    /// </summary>
    public void QueryByGameIdAndDateTime(long gameId, string dateTime, int pageIndex = 0)
    {
        currentGameId = gameId;
        currentSelectedDateTime = dateTime;

        // 解析日期时间
        DateTime dt;
        if (!DateTime.TryParseExact(dateTime, DATETIME_FORMAT, null, System.Globalization.DateTimeStyles.None, out dt))
        {
            DebugUtils.LogError($"日期时间格式错误：{dateTime}");
            return;
        }

        // 计算当天的开始和结束时间戳
        DateTime startOfDay = dt.Date; // 当天的 00:00:00
        DateTime endOfDay = startOfDay.AddDays(1).AddSeconds(-1); // 当天的 23:59:59

        long startTimestamp = (long)(startOfDay.Subtract(new DateTime(1970, 1, 1))).TotalSeconds * 1000;
        long endTimestamp = (long)(endOfDay.Subtract(new DateTime(1970, 1, 1))).TotalSeconds * 1000;

        int viewMax = DefaultSettingsUtils.defMaxGameRecordView;

        // 先查询总记录数（后台仅展示最近 viewMax 条）
        string countSql = $@"
            SELECT COUNT(*) FROM {ConsoleTableName.TABLE_SLOT_GAME_RECORD} 
            WHERE game_id = {gameId} 
            AND created_at >= {startTimestamp}
            AND created_at <= {endTimestamp}";

        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(countSql, null, (SqliteDataReader countReader) =>
        {
            if (countReader.Read())
            {
                int dbCount = countReader.GetInt32(0);
                totalRecordCount = Mathf.Min(dbCount, viewMax);
                totalPageCount = totalRecordCount; // 每页一条，所以页数等于记录数
                curPageIndex = totalPageCount > 0
                    ? Mathf.Clamp(pageIndex, 0, totalPageCount - 1)
                    : 0;
            }

            // 仅加载后台可查看的最近记录
            string allSql = $@"
                SELECT * FROM {ConsoleTableName.TABLE_SLOT_GAME_RECORD} 
                WHERE game_id = {gameId} 
                AND created_at >= {startTimestamp}
                AND created_at <= {endTimestamp}
                ORDER BY created_at DESC
                LIMIT {viewMax}";

            SQLiteAsyncHelper.Instance.ExecuteQueryAsync(allSql, null, (SqliteDataReader sdr) =>
            {
                allRecords.Clear();

                while (sdr.Read())
                {
                    TableSlotGameRecordItem record = new TableSlotGameRecordItem()
                    {
                        open_type = sdr.GetInt32(sdr.GetOrdinal("open_type")),
                        result_type = sdr.GetInt32(sdr.GetOrdinal("result_type")),
                        free_curtime = sdr.GetInt32(sdr.GetOrdinal("free_curtime")),
                        free_totaltime = sdr.GetInt32(sdr.GetOrdinal("free_totaltime")),
                        game_id = sdr.GetInt64(sdr.GetOrdinal("game_id")),
                        game_uid = sdr.GetString(sdr.GetOrdinal("game_uid")),
                        created_at = sdr.GetInt64(sdr.GetOrdinal("created_at")),
                        total_bet = sdr.GetInt64(sdr.GetOrdinal("total_bet")),
                        credit_before = sdr.GetInt64(sdr.GetOrdinal("credit_before")),
                        credit_after = sdr.GetInt64(sdr.GetOrdinal("credit_after")),
                        base_game_win_credit = sdr.GetInt64(sdr.GetOrdinal("base_game_win_credit")),
                        jackpot_win_credit= sdr.GetInt64(sdr.GetOrdinal("jackpot_win_credit")),
                        bonus_game_win_credit = sdr.GetInt64(sdr.GetOrdinal("bonus_game_win_credit")),
                        free_spin_win_credit = sdr.GetInt64(sdr.GetOrdinal("free_spin_win_credit")),
                        total_win_credit = sdr.GetInt64(sdr.GetOrdinal("total_win_credit")),
                        strDeckRowCol = sdr.GetString(sdr.GetOrdinal("strDeckRowCol"))
                    };

                    // 尝试读取 symbol_icon_mapping 字段（如果存在）
                    try
                    {
                        int symbolIconIndex = sdr.GetOrdinal("symbol_icon_mapping");
                        if (!sdr.IsDBNull(symbolIconIndex))
                        {
                            record.symbol_icon_mapping = sdr.GetString(symbolIconIndex);
                        }
                    }
                    catch
                    {
                        // 如果字段不存在，忽略
                        record.symbol_icon_mapping = null;
                    }
                    allRecords.Add(record);
                }

                // 获取当前页的记录
                TableSlotGameRecordItem currentRecord = null;
                if (allRecords.Count > 0 && curPageIndex < allRecords.Count)
                {
                    currentRecord = allRecords[curPageIndex];
                }

                SetUIPage(currentRecord);
            });
        });
    }

    // 下一页
    public void NextPage()
    {
        if (curPageIndex < totalPageCount - 1)
        {
            curPageIndex++;
            ShowCurrentPageRecord();
        }
    }

    // 上一页
    public void PrevPage()
    {
        if (curPageIndex > 0)
        {
            curPageIndex--;
            ShowCurrentPageRecord();
        }
    }

    // 跳转到指定页
    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < totalPageCount)
        {
            curPageIndex = pageIndex;
            ShowCurrentPageRecord();
        }
    }

    // 显示当前页的记录
    void ShowCurrentPageRecord()
    {
        TableSlotGameRecordItem currentRecord = null;
        if (allRecords.Count > 0 && curPageIndex < allRecords.Count)
        {
            currentRecord = allRecords[curPageIndex];
        }
        SetUIPage(currentRecord);
    }

    void SetUIPage(TableSlotGameRecordItem currentRecord)
    {
        GameHistoryInfo info = new GameHistoryInfo()
        {
            curPageNumber = curPageIndex + 1,
            totalPageCount = totalPageCount,
            currentIndex = curPageIndex,
            totalRecordCount = totalRecordCount,
            currentRecord = currentRecord,
            allRecords = allRecords,
            currentGameId = currentGameId,
            currentDateTime = currentSelectedDateTime
        };

        onPageChagne?.Invoke(info);
    }

    // 清空显示（当没有数据时）
    public void ClearDisplay()
    {
        GameHistoryInfo info = new GameHistoryInfo()
        {
            curPageNumber = 0,
            totalPageCount = 0,
            currentIndex = -1,
            totalRecordCount = 0,
            currentRecord = null,
            allRecords = new List<TableSlotGameRecordItem>(),
            currentGameId = currentGameId,
            currentDateTime = currentSelectedDateTime
        };

        onPageChagne?.Invoke(info);
    }

    // 获取第一条记录
    public void FirstPage()
    {
        if (totalPageCount > 0)
        {
            curPageIndex = 0;
            ShowCurrentPageRecord();
        }
    }

    // 获取最后一条记录
    public void LastPage()
    {
        if (totalPageCount > 0)
        {
            curPageIndex = totalPageCount - 1;
            ShowCurrentPageRecord();
        }
    }

    // 是否有上一页
    public bool HasPrevPage()
    {
        return curPageIndex > 0;
    }

    // 是否有下一页
    public bool HasNextPage()
    {
        return curPageIndex < totalPageCount - 1;
    }
}