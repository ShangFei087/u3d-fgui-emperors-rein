using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

//TabLogRecordController
/// <summary>
/// 事件和报错日志控制类
/// </summary>
public class LogRecordDataController
{

   
    void Init() { }


    string tabName = ConsoleTableName.TABLE_LOG_ERROR_RECORD;

    const string FORMAT_DATE_DAY = "yyyy-MM-dd";

    // 每页总条数
    int totalCountPerPage = 11;

    // 当前页数
    int curPageIndex = 0;

    // 总的页数
    int totalPageCount = 0;


    int fromIdx = 0;

    // 选择的日期索引
    int dateIndex = 0;
    List<string> dropdownDateLst;
    List<TableLogRecordItem> resLogEventRecord;


    public void InitParam(string tabName, int totalCountPerPage, Action<List<string>> onDatesChange, Action<LogPageInfo> onPageChagne)
    {
        this.tabName = tabName;
        this.totalCountPerPage = totalCountPerPage;

        this.onDatesChange = onDatesChange;
        this.onPageChagne = onPageChagne;

        InitLogInfo();
    }


    void InitLogInfo()
    {

        string sql = $"SELECT created_at FROM {tabName}";

        DebugUtils.Log(sql);
        List<long> date = new List<long>();
        dropdownDateLst = new List<string>();
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (SqliteDataReader sdr) =>
        {
            while (sdr.Read())
            {
                long d = sdr.GetInt64(0);
                date.Add(d);
            }
            foreach (long timestamp in date)
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                DateTime localDateTime = dateTimeOffset.LocalDateTime;
                string time = localDateTime.ToString(FORMAT_DATE_DAY);

                if (!dropdownDateLst.Contains(time))
                {
                    dropdownDateLst.Insert(0, time);
                    //DebugUtils.Log($"时间搓：{timestamp} 时间 ：{time}");
                }
            }

            onDatesChange?.Invoke(dropdownDateLst);
            //if (dropdownDateLst.Count > 0) SetDate(0);
        });
    }

    void OnDropdownChangedDate(int indexDate)
    {

        if (indexDate > dropdownDateLst.Count)
            return;


        string sql2 = $"SELECT * FROM {tabName} WHERE DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{dropdownDateLst[indexDate]}'"; //可以用

        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql2, null, (SqliteDataReader sdr) =>
        {
            resLogEventRecord = new List<TableLogRecordItem>();
            while (sdr.Read())
            {
                resLogEventRecord.Insert(0,
                new TableLogRecordItem()
                {
                    log_type = sdr.GetString(sdr.GetOrdinal("log_type")),
                    log_content = sdr.GetString(sdr.GetOrdinal("log_content")),
                    log_stacktrace = sdr.GetString(sdr.GetOrdinal("log_stacktrace")),
                    log_tag = sdr.GetString(sdr.GetOrdinal("log_tag")),
                    created_at = sdr.GetInt64(sdr.GetOrdinal("created_at")),
                });
            }

            curPageIndex = 0;
            totalPageCount = (resLogEventRecord.Count + (totalCountPerPage - 1)) / totalCountPerPage; //向上取整
            fromIdx = 0;
            SetUIPage();
        });
    }



    /// <summary>
    /// 日期改变 -- 数据改变
    /// </summary>
    /// <param name="index"></param>
    void SetDate(int index)
    {
        dateIndex = index;
        OnDropdownChangedDate(index);
        //txtDate.text = dropdownDateLst[index].ToString();
    }

    void SetUIPage()
    {
        int lastIdx = fromIdx + totalCountPerPage - 1;
        if (lastIdx > resLogEventRecord.Count - 1)
        {
            lastIdx = resLogEventRecord.Count - 1;
        }

        int endIndex = lastIdx - fromIdx;

        List<LogRecord> logRecords = new List<LogRecord>(); 
        for (int i = 0; i <= endIndex; i++)
        {
            int indexRecord = i + fromIdx;
            TableLogRecordItem res = resLogEventRecord[indexRecord];

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(res.created_at);
            DateTime localDateTime = dateTimeOffset.LocalDateTime;

            logRecords.Add(new LogRecord()
            {
                timeStr = localDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                tag = res.log_tag,
                content = Encoding.UTF8.GetString(Convert.FromBase64String(res.log_content)),
                detail = GetDetail(indexRecord),
            });
        }

        //pageText.text = (curPageIndex + 1) + "/" + totalPageCount;

        onPageChagne?.Invoke(new LogPageInfo()
        {
            curPageNumber = curPageIndex + 1,
            totalPageCount = totalPageCount,
            logRecords = logRecords,
        });
    }

    string GetDetail(int index)
    {
        TableLogRecordItem res = resLogEventRecord[index];
        string content = Encoding.UTF8.GetString(Convert.FromBase64String(res.log_content));

        return $"[content]:\n\n{content}\n\n[stacktrace]:\n\n" +
                 Encoding.UTF8.GetString(Convert.FromBase64String(res.log_stacktrace));
    }


    Action<List<string>> onDatesChange = null;
    Action<LogPageInfo> onPageChagne = null;

    public void ChangeDate(int? index)
    {
        if (dropdownDateLst.Count <= 0) return;

        if (index != null)
        {
            dateIndex = (int)index;
        }
        else
        {
            if (++dateIndex >= dropdownDateLst.Count)
                dateIndex = 0;
        }

        SetDate(dateIndex);
    }


}
public class LogRecord
{
    public string timeStr;
    public string tag;
    public string content;
    public string detail;
}

public class LogPageInfo
{
    public int curPageNumber;
    public int totalPageCount;
    public List<LogRecord> logRecords;
}


// 页面日志变化委托
//
//