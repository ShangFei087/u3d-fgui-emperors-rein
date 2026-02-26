using FairyGUI;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

public class InOutRecordController
{


    const int perPageNumCoinInOut = 10;
    int fromIdxCoinInOut = 0;
    string TABLE_COIN_IN_OUT_RECORD => ConsoleTableName.TABLE_COIN_IN_OUT_RECORD;    //const string TABLE_COIN_IN_OUT_RECORD = "coin_in_out_record";

    const string FORMAT_DATE_SECOND = "yyyy-MM-dd HH:mm:ss";
    const string FORMAT_DATE_DAY = "yyyy-MM-dd";
    List<string> dropdownDateLst;
    List<TableCoinInOutRecordItem> resCoinInOutRecord = new List<TableCoinInOutRecordItem>();

    int totalPageCount;
    int curPageIndex;
    Action<int, int> onPageIndexChange;

    GRichTextField _rtxtDayInOutTotalCoinIn, _rtxtDayInOutTotalCoinOut, _rtxtDayInOutTotalProfitlCoinInOut,
        _rtxtDayInOutTotalScoreUp, _rtxtDayInOutTotalScoreDown, _rtxtDayInOutTotalProfitlScoreUpDown;

    GComboBox _comboDateInOut;

    List<GComponent> _rows = new List<GComponent>();

    public void InitParam (

        GRichTextField rtxtDayInOutTotalCoinIn,
        GRichTextField rtxtDayInOutTotalCoinOut,
        GRichTextField rtxtDayInOutTotalProfitlCoinInOut,
        GRichTextField rtxtDayInOutTotalScoreUp,
        GRichTextField rtxtDayInOutTotalScoreDown,
        GRichTextField rtxtDayInOutTotalProfitlScoreUpDown,
        List<GComponent> rows,
        GComboBox comboDateInOut,
        Action<int,int> onInOutPageIndexChange)
    {
        _rtxtDayInOutTotalCoinIn = rtxtDayInOutTotalCoinIn;
        _rtxtDayInOutTotalCoinOut = rtxtDayInOutTotalCoinOut;
        _rtxtDayInOutTotalProfitlCoinInOut = rtxtDayInOutTotalProfitlCoinInOut;
        _rtxtDayInOutTotalScoreUp = rtxtDayInOutTotalScoreUp;
        _rtxtDayInOutTotalScoreDown = rtxtDayInOutTotalScoreDown;
        _rtxtDayInOutTotalProfitlScoreUpDown = rtxtDayInOutTotalProfitlScoreUpDown;
        _rows = rows;
        _comboDateInOut = comboDateInOut;
        onPageIndexChange = onInOutPageIndexChange;


        ClearTotalData();


        foreach (GComponent item in _rows)
        {
            item.visible = false;
        }

        _comboDateInOut.onChanged.Clear();
        _comboDateInOut.onChanged.Add(OnDropdownChangedDateCoinInOut);


        InitCoinInOutRecordInfo();
    }

    void ClearTotalData()
    {
        _rtxtDayInOutTotalCoinIn.text = "";
        _rtxtDayInOutTotalCoinOut.text = "";
        _rtxtDayInOutTotalProfitlCoinInOut.text = "";
        _rtxtDayInOutTotalScoreUp.text = "";
        _rtxtDayInOutTotalScoreDown.text = "";
        _rtxtDayInOutTotalProfitlScoreUpDown.text = "";
    }

    /// <summary>
    /// 选择日期
    /// </summary>
    void OnDropdownChangedDateCoinInOut()
    {
        int index = _comboDateInOut.selectedIndex;

        if (dropdownDateLst == null || dropdownDateLst.Count == 0 || index > dropdownDateLst.Count)
            return;
      
        //_comboDateInOut.value

        string sql2 = $"SELECT * FROM {TABLE_COIN_IN_OUT_RECORD} WHERE DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{dropdownDateLst[index]}'"; //可以用
                                                                                                                                                                  //string sql = $"SELECT * FROM {TABLE_COIN_IN_OUT_RECORD} WHERE DATE(created_at) = '{dropdownDateLst[index]}'";  //不可以用
        //DebugUtils.Log(sql2);
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql2, null, (SqliteDataReader sdr) =>
        {

            resCoinInOutRecord = new List<TableCoinInOutRecordItem>();

            int i = 0;
            while (sdr.Read())
            {

                resCoinInOutRecord.Insert(0,
                new TableCoinInOutRecordItem()
                {
                    device_type = sdr.GetString(sdr.GetOrdinal("device_type")),
                    count = sdr.GetInt64(sdr.GetOrdinal("count")),
                    as_money = sdr.GetInt64(sdr.GetOrdinal("as_money")),
                    credit = sdr.GetInt64(sdr.GetOrdinal("credit")),
                    credit_before = sdr.GetInt64(sdr.GetOrdinal("credit_before")),
                    credit_after = sdr.GetInt64(sdr.GetOrdinal("credit_after")),
                    order_id = sdr.GetString(sdr.GetOrdinal("order_id")),
                    created_at = sdr.GetInt64(sdr.GetOrdinal("created_at")),
                    in_out = sdr.GetInt32(sdr.GetOrdinal("in_out")),
                });
            }


            /*
            pageButtomInfo[1].curPageIndex = 0;
            int totalPageCount = (resCoinInOutRecord.Count + (perPageNumCoinInOut - 1)) / perPageNumCoinInOut; //向上取整
            pageButtomInfo[1].totalPageCount = totalPageCount;
            fromIdxCoinInOut = 0;
            SetUICoinInOut();

            ChanageButtonUI(pageIndex);
            */


            curPageIndex = 0;
            totalPageCount = (resCoinInOutRecord.Count + (perPageNumCoinInOut - 1)) / perPageNumCoinInOut; //向上取整
            fromIdxCoinInOut = 0;

            onPageIndexChange?.Invoke(curPageIndex, totalPageCount);
            SetUICoinInOut();
            SetInOutTotal(dropdownDateLst[index]);
        });
    }


    /// <summary>
    /// 投退币历史记录
    /// </summary>
    void InitCoinInOutRecordInfo()
    {
        //string sql = $"select distinct date(created_at) from {tableName}";
        string sql = $"SELECT created_at FROM {TABLE_COIN_IN_OUT_RECORD}";

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
                //DebugUtil.Log($"时间搓：{timestamp}");
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                //DateTime utcDateTime = dateTimeOffset.UtcDateTime;
                //string time = utcDateTime.ToString(FORMAT_DATE_DAY);

                DateTime localDateTime = dateTimeOffset.LocalDateTime;
                string time = localDateTime.ToString(FORMAT_DATE_DAY);

                if (!dropdownDateLst.Contains(time))
                {
                    //dropdownDateLst.Add(time);
                    DebugUtils.Log($"时间搓：{timestamp} 时间 ：{time}");
                    dropdownDateLst.Insert(0, time); //最新排在最前
                }
            }

            _comboDateInOut.items = dropdownDateLst.ToArray();
            _comboDateInOut.values = dropdownDateLst.ToArray();
            _comboDateInOut.selectedIndex = 0;
            OnDropdownChangedDateCoinInOut();
        });


    }


  
    public void OnNextCoinInOutRecord()
    {
        /*
        if (fromIdxCoinInOut + perPageNumCoinInOut >= resCoinInOutRecord.Count)
            return;

        fromIdxCoinInOut += perPageNumCoinInOut;
        pageButtomInfo[1].curPageIndex++;
        SetUICoinInOut();
        */

        if (fromIdxCoinInOut + perPageNumCoinInOut >= resCoinInOutRecord.Count)
            return;

        fromIdxCoinInOut += perPageNumCoinInOut;
        curPageIndex++;
        onPageIndexChange?.Invoke(curPageIndex, totalPageCount);
        SetUICoinInOut();
    }

    public void OnPrevCoinInOutRecord()
    {
        /*
        if (fromIdxCoinInOut <= 0)
            return;

        fromIdxCoinInOut -= perPageNumCoinInOut;
        pageButtomInfo[1].curPageIndex--;
        if (fromIdxCoinInOut < 0)
        {
            pageButtomInfo[1].curPageIndex = 0;
            fromIdxCoinInOut = 0;
        }
        SetUICoinInOut();
        */

        if (fromIdxCoinInOut <= 0)
            return;

        fromIdxCoinInOut -= perPageNumCoinInOut;
        curPageIndex--;
        if (fromIdxCoinInOut < 0)
        {
            curPageIndex = 0;
            fromIdxCoinInOut = 0;
        }
        onPageIndexChange?.Invoke(curPageIndex,totalPageCount);
        SetUICoinInOut();
    }



    /// <summary>
    /// 显示投退币内容
    /// </summary>
    void SetUICoinInOut()
    {
        int lastIdx = fromIdxCoinInOut + perPageNumCoinInOut - 1;
        if (lastIdx > resCoinInOutRecord.Count - 1)
        {
            lastIdx = resCoinInOutRecord.Count - 1;
        }

        foreach (GComponent item in _rows)
        {
            item.visible = false;
        }
        for (int i = 0; i <= lastIdx - fromIdxCoinInOut; i++)
        {
            GComponent item = _rows[i];
            item.visible = true;
            TableCoinInOutRecordItem res = resCoinInOutRecord[i + fromIdxCoinInOut];

            item.GetChild("col0").asTextField.text = I18nMgr.T(res.in_out == 1 ? "In" : "Out");
            item.GetChild("col1").asTextField.text = res.credit.ToString();
            item.GetChild("col2").asTextField.text = I18nMgr.T(res.device_type);
            item.GetChild("col3").asTextField.text = res.credit_before.ToString();
            item.GetChild("col4").asTextField.text = res.credit_after.ToString();
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(res.created_at);
            DateTime localDateTime = dateTimeOffset.LocalDateTime;
            item.GetChild("col5").asTextField.text = localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    /// <summary>
    /// 投退汇总
    /// </summary>
    /// <param name="yyyyMMdd"></param>
    async void SetInOutTotal(string yyyyMMdd)
    {

        string dbName = ConsoleTableName.DB_NAME;
        string tableName = ConsoleTableName.TABLE_COIN_IN_OUT_RECORD;

        bool isNext = false;

        if (!SQLiteAsyncHelper.Instance.isConnect)
        {
            DebugUtils.LogError($"【Check Record】{dbName} is close");
            return;
        }


        long totalScoreUp = 0;
        string sql = $"SELECT SUM(credit) AS total_credit FROM {tableName} WHERE in_out = 1 AND device_type = 'score_up' AND DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{yyyyMMdd}';";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (dataReader) =>
        {
            while (dataReader.Read())
            {
                try
                {
                    totalScoreUp = dataReader.GetInt64(0);
                }
                catch
                {
                    totalScoreUp = 0;
                }
            }
            _rtxtDayInOutTotalScoreUp.text = $"{totalScoreUp}";
            isNext = true;
        });
        await WaitUntil(() => isNext == true);
        isNext = false;


        long totalScoreDown = 0;
        sql = $"SELECT SUM(credit) AS total_credit FROM {tableName} WHERE in_out = 0 AND device_type = 'score_down' AND DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{yyyyMMdd}';";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (dataReader) =>
        {
            while (dataReader.Read())
            {
                try
                {
                    totalScoreDown = dataReader.GetInt64(0);
                }
                catch
                {
                    totalScoreDown = 0;
                }
            }
            _rtxtDayInOutTotalScoreDown.text = $"{totalScoreDown}";

            _rtxtDayInOutTotalProfitlScoreUpDown.text = $"{totalScoreUp - totalScoreDown}";
            isNext = true;
        });
        await WaitUntil(() => isNext == true);
        isNext = false;


        long totalCoinIn = 0;
        sql = $"SELECT SUM(credit) AS total_credit FROM {tableName} WHERE in_out = 1 AND device_type != 'score_up' AND DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{yyyyMMdd}';";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (dataReader) =>
        {
            while (dataReader.Read())
            {
                try
                {
                    totalCoinIn = dataReader.GetInt64(0);
                }
                catch
                {
                    totalCoinIn = 0;
                }
            }
            _rtxtDayInOutTotalCoinIn.text = $"{totalCoinIn}";
            isNext = true;
        });
        await WaitUntil(() => isNext == true);
        isNext = false;


        long totalCoinOut = 0;
        sql = $"SELECT SUM(credit) AS total_credit FROM {tableName} WHERE in_out = 0 AND device_type != 'score_down' AND DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{yyyyMMdd}';";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (dataReader) =>
        {
            while (dataReader.Read())
            {
                try
                {
                    totalCoinOut = dataReader.GetInt64(0);
                }
                catch
                {
                    totalCoinOut = 0;
                }
            }
            _rtxtDayInOutTotalCoinOut.text = $"{totalCoinOut}";

            _rtxtDayInOutTotalProfitlCoinInOut.text = $"{totalCoinIn - totalCoinOut}";

            isNext = true;
        });
        await WaitUntil(() => isNext == true);
        isNext = false;

    }


    private static async Task WaitUntil(Func<bool> condition)
    {
        while (!condition())
        {
            await Task.Delay(10);  // 每10ms检查一次
            // 避免“编辑器-非运行”模式下，死循环导致u3d编辑器卡死
            if (Application.isEditor && !Application.isPlaying)  return;
        }
    }

}
