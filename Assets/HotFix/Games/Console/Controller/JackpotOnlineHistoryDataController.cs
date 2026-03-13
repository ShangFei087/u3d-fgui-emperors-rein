using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using UnityEngine;

public class JackpotOnlineHistoryPageInfo
{
    public int curPageNumber; // 当前页码（从1开始）
    public int totalPageCount; // 总页数
    public int currentIndex; // 当前页索引（从0开始）
    public int totalRecordCount; // 总记录数
    public string currentDate; // 当前日期
    public List<TableJackpotRecordItem> pageRecords; // 当前页记录
}

public class JackpotOnlineHistoryDataController
{
    private const int PerPageCount = 20;
    private const long MillisecondThreshold = 1000000000000L;
    private string _tabName = ConsoleTableName.TABLE_JACKPOT_RECORD;
    private readonly List<TableJackpotRecordItem> _allRecords = new List<TableJackpotRecordItem>();
    private List<string> _dropDownDateList = new List<string>();
    private int _curPageIndex = 0, _totalPageCount = 0, _totalRecordCount = 0;
    private string _currentSelectDate = "";

    private Action<List<string>> _onDatesChanged;
    private Action<JackpotOnlineHistoryPageInfo> _onPageChanged;

    public void InitParam(string tabName, Action<List<string>> onDatesChange, Action<JackpotOnlineHistoryPageInfo> onPageChanged)
    {
        _tabName = tabName;
        _onDatesChanged = onDatesChange;
        _onPageChanged = onPageChanged;
        LoadDateTimes();
    }

    // 加载日期列表（yyyy-MM-dd）
    public void LoadDateTimes()
    {
        string createdAtMsExpr = $"(CASE WHEN created_at < {MillisecondThreshold} THEN created_at * 1000 ELSE created_at END)";
        string sql = $@"
            SELECT DISTINCT DATE(DATETIME({createdAtMsExpr} / 1000, 'unixepoch', 'localtime')) AS date
            FROM {_tabName}
            ORDER BY date DESC";

        _dropDownDateList = new List<string>();

        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (SqliteDataReader sdr) =>
        {
            try
            {
                while (sdr.Read())
                {
                    if (sdr.IsDBNull(0))
                    {
                        continue;
                    }

                    string date = sdr.GetString(0);
                    if (string.IsNullOrEmpty(date))
                    {
                        continue;
                    }

                    if (!_dropDownDateList.Contains(date))
                    {
                        _dropDownDateList.Add(date);
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.LogError($"LoadDateTimes 读取数据失败: {e.Message}");
            }
            finally
            {
                _onDatesChanged?.Invoke(_dropDownDateList);
            }
        });
    }

    /// <summary>
    /// 按日期查询联网彩金记录，单页最多20条
    /// </summary>
    public void QueryByDate(string date, int pageIndex = 0)
    {
        _currentSelectDate = date;
        _curPageIndex = Mathf.Max(0, pageIndex);

        if (string.IsNullOrEmpty(date))
        {
            _allRecords.Clear();
            _totalRecordCount = 0;
            _totalPageCount = 0;
            NotifyPageChanged();
            return;
        }

        string createdAtMsExpr = $"(CASE WHEN created_at < {MillisecondThreshold} THEN created_at * 1000 ELSE created_at END)";
        string countSql = $@"
            SELECT COUNT(*) FROM {_tabName}
            WHERE DATE(DATETIME({createdAtMsExpr} / 1000, 'unixepoch', 'localtime')) = '{date}'";

        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(countSql, null, (SqliteDataReader countReader) =>
        {
            try
            {
                _totalRecordCount = countReader.Read() ? Convert.ToInt32(countReader[0]) : 0;
            }
            catch (Exception e)
            {
                DebugUtils.LogError($"QueryByDate count 查询失败: {e.Message}");
                _totalRecordCount = 0;
            }

            _totalPageCount = (_totalRecordCount + PerPageCount - 1) / PerPageCount;
            if (_totalPageCount <= 0)
            {
                _curPageIndex = 0;
                _allRecords.Clear();
                NotifyPageChanged();
                return;
            }

            _curPageIndex = Mathf.Clamp(_curPageIndex, 0, _totalPageCount - 1);
            int offset = _curPageIndex * PerPageCount;

            string pageSql = $@"
                SELECT * FROM {_tabName}
                WHERE DATE(DATETIME({createdAtMsExpr} / 1000, 'unixepoch', 'localtime')) = '{date}'
                ORDER BY {createdAtMsExpr} DESC
                LIMIT {PerPageCount} OFFSET {offset}";

            SQLiteAsyncHelper.Instance.ExecuteQueryAsync(pageSql, null, (SqliteDataReader sdr) =>
            {
                _allRecords.Clear();
                try
                {
                    while (sdr.Read())
                    {
                        long GetLong(string columnName)
                        {
                            int ordinal = sdr.GetOrdinal(columnName);
                            if (sdr.IsDBNull(ordinal))
                            {
                                return 0;
                            }
                            return Convert.ToInt64(sdr.GetValue(ordinal));
                        }

                        string GetString(string columnName)
                        {
                            int ordinal = sdr.GetOrdinal(columnName);
                            if (sdr.IsDBNull(ordinal))
                            {
                                return string.Empty;
                            }
                            return Convert.ToString(sdr.GetValue(ordinal));
                        }

                        TableJackpotRecordItem record = new TableJackpotRecordItem()
                        {
                            id = GetLong("id"),
                            user_id = GetString("user_id"),
                            game_id = GetLong("game_id"),
                            game_uid = GetString("game_uid"),
                            jp_name = GetString("jp_name"),
                            jp_level = GetLong("jp_level"),
                            win_credit = GetLong("win_credit"),
                            credit_before = GetLong("credit_before"),
                            credit_after = GetLong("credit_after"),
                            custom_data = GetString("custom_data"),
                            created_at = NormalizeToMilliseconds(GetLong("created_at")),
                        };
                        _allRecords.Add(record);
                    }
                }
                catch (Exception e)
                {
                    DebugUtils.LogError($"QueryByDate 数据读取失败: {e.Message}");
                }

                NotifyPageChanged();
            });
        });
    }

    public void PrevPage()
    {
        if (_curPageIndex <= 0)
        {
            return;
        }

        QueryByDate(_currentSelectDate, _curPageIndex - 1);
    }

    public void NextPage()
    {
        if (_curPageIndex >= _totalPageCount - 1)
        {
            return;
        }

        QueryByDate(_currentSelectDate, _curPageIndex + 1);
    }

    private long NormalizeToMilliseconds(long timestamp)
    {
        if (timestamp <= 0)
        {
            return timestamp;
        }

        return timestamp < MillisecondThreshold ? timestamp * 1000 : timestamp;
    }

    private void NotifyPageChanged()
    {
        JackpotOnlineHistoryPageInfo info = new JackpotOnlineHistoryPageInfo()
        {
            curPageNumber = _totalPageCount > 0 ? _curPageIndex + 1 : 0,
            totalPageCount = _totalPageCount,
            currentIndex = _curPageIndex,
            totalRecordCount = _totalRecordCount,
            currentDate = _currentSelectDate,
            pageRecords = new List<TableJackpotRecordItem>(_allRecords),
        };

        _onPageChanged?.Invoke(info);
    }
}
