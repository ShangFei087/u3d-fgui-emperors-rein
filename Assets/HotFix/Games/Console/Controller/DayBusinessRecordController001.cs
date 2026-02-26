using FairyGUI;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class DayBusinessRecordController001
{



    // 使用 readonly 确保字段只能在构造函数中赋值
    // readonly GRichTextField _rtxtTotalBet, _rtxtTotalWin, _rtxtTotalProfitBet;

    GRichTextField _rtxtTotalBet, _rtxtTotalWin, _rtxtTotalProfitBet,

    _rtxtTotalCoinIn, _rtxtTotalCoinOut, _rtxtTotalProfitCoinInOut,

    _rtxtTotalScoreUp, _rtxtTotalScoreDown, _rtxtTotalProfitlScoreUpDown;


    GComboBox _comboDateBusinessDayRecord;



    public void InitParam (
        GRichTextField rtxtTotalBet,
        GRichTextField rtxtTotalWin,
        GRichTextField rtxtTotalProfitBet,
        GRichTextField rtxtTotalCoinIn,
        GRichTextField rtxtTotalCoinOut,
        GRichTextField rtxtTotalProfitCoinInOut,
        GRichTextField rtxtTotalScoreUp,
        GRichTextField rtxtTotalScoreDown,
        GRichTextField rtxtTotalProfitScoreUpDown,
        GComboBox comboDateBusinessDayRecord) // 参数名与字段名一致
    {
        _rtxtTotalBet = rtxtTotalBet;
        _rtxtTotalWin = rtxtTotalWin;
        _rtxtTotalProfitBet = rtxtTotalProfitBet;
        _rtxtTotalCoinIn = rtxtTotalCoinIn;
        _rtxtTotalCoinOut = rtxtTotalCoinOut;
        _rtxtTotalProfitCoinInOut = rtxtTotalProfitCoinInOut;
        _rtxtTotalScoreUp = rtxtTotalScoreUp;
        _rtxtTotalScoreDown = rtxtTotalScoreDown;
        _rtxtTotalProfitlScoreUpDown = rtxtTotalProfitScoreUpDown;
        _comboDateBusinessDayRecord = comboDateBusinessDayRecord;



        _comboDateBusinessDayRecord.onChanged.Clear();
        _comboDateBusinessDayRecord.onChanged.Add(OnDropdownChangedDateBusinessDayRecord);

        ClearAllUI();
  
        InitBusinessDayRecordInfo();
    }


    void OnDropdownChangedDateBusinessDayRecord()
    {
        //_comboDateBusinessDayRecord.selectedIndex
        //_comboDateBusinessDayRecord.value
        CheckBusinessDayRecord(_comboDateBusinessDayRecord.value);
    }


    const string FORMAT_DATE_DAY = "yyyy-MM-dd";
    List<string> dropdownDateLstBusniessDayRecord;
    void ClearAllUI()
    {
        _rtxtTotalBet.text = "0";
        _rtxtTotalWin.text = "0";
        _rtxtTotalProfitBet.text = "0";

        _rtxtTotalCoinIn.text = "0";
        _rtxtTotalCoinOut.text = "0";
        _rtxtTotalProfitCoinInOut.text = "0";

        _rtxtTotalScoreUp.text = "0";
        _rtxtTotalScoreDown.text = "0";
        _rtxtTotalProfitlScoreUpDown.text = "0";
    }

    void InitBusinessDayRecordInfo()
    {
        dropdownDateLstBusniessDayRecord = new List<string>();

        string sql = $"SELECT created_at FROM {ConsoleTableName.TABLE_BUSINESS_DAY_RECORD}";
        DebugUtils.Log(sql);
        List<long> date = new List<long>();

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

                if (!dropdownDateLstBusniessDayRecord.Contains(time))
                {
                    DebugUtils.Log($"时间搓：{timestamp} 时间 ：{time}");
                    dropdownDateLstBusniessDayRecord.Insert(0, time); //最新排在最前
                }
            }

            //combo.items = new string[] { "选项1", "选项2", "选项3" }; // 设置选项文本
            //combo.values = new string[] { "value1", "value2", "value3" }; // 可选：设置选项值

            _comboDateBusinessDayRecord.items = dropdownDateLstBusniessDayRecord.ToArray();
            _comboDateBusinessDayRecord.values = dropdownDateLstBusniessDayRecord.ToArray();

            if (dropdownDateLstBusniessDayRecord.Count > 0)
            {
                _comboDateBusinessDayRecord.selectedIndex = 0;
                CheckBusinessDayRecord(_comboDateBusinessDayRecord.values[0]);
            }

        });

    }

    void CheckBusinessDayRecord(string yyyyMMdd)
    {
        string dbName = ConsoleTableName.DB_NAME;
        string tableName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        if (!SQLiteAsyncHelper.Instance.isConnect)
        {
            DebugUtils.LogError($"【Check Record】{dbName} is close");
            return;
        }

        string sql = $"SELECT * FROM {tableName} WHERE DATE(DATETIME(created_at / 1000, 'unixepoch', 'localtime')) = '{yyyyMMdd}';";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, null, (sdr) =>
        {

            TableBussinessDayRecordItem bussinessDayRecord = new TableBussinessDayRecordItem();
            while (sdr.Read())
            {
                bussinessDayRecord = new TableBussinessDayRecordItem()
                {
                    credit_before = sdr.GetInt32(sdr.GetOrdinal("credit_before")),
                    credit_after = sdr.GetInt32(sdr.GetOrdinal("credit_after")),
                    total_bet_credit = sdr.GetInt32(sdr.GetOrdinal("total_bet_credit")),
                    total_win_credit = sdr.GetInt32(sdr.GetOrdinal("total_win_credit")),
                    total_coin_in_credit = sdr.GetInt32(sdr.GetOrdinal("total_coin_in_credit")),
                    total_coin_out_credit = sdr.GetInt32(sdr.GetOrdinal("total_coin_out_credit")),
                    total_score_up_credit = sdr.GetInt32(sdr.GetOrdinal("total_score_up_credit")),
                    total_score_down_credit = sdr.GetInt32(sdr.GetOrdinal("total_score_down_credit")),
                    total_bill_in_credit = sdr.GetInt32(sdr.GetOrdinal("total_bill_in_credit")),
                    total_bill_in_as_money = sdr.GetInt32(sdr.GetOrdinal("total_bill_in_as_money")),
                    total_printer_out_credit = sdr.GetInt32(sdr.GetOrdinal("total_printer_out_credit")),
                    total_printer_out_as_money = sdr.GetInt32(sdr.GetOrdinal("total_printer_out_as_money")),
                    created_at = sdr.GetInt32(sdr.GetOrdinal("created_at")),
                };
            }

            _rtxtTotalBet.text = $"{bussinessDayRecord.total_bet_credit}";

            _rtxtTotalWin.text = $"{bussinessDayRecord.total_win_credit}";

            _rtxtTotalProfitBet.text = $"{bussinessDayRecord.total_bet_credit - bussinessDayRecord.total_win_credit}";

            _rtxtTotalScoreDown.text = $"{bussinessDayRecord.total_score_down_credit}";

            _rtxtTotalScoreUp.text = $"{bussinessDayRecord.total_score_up_credit}";

            _rtxtTotalProfitlScoreUpDown.text = $"{bussinessDayRecord.total_score_up_credit - bussinessDayRecord.total_score_down_credit}";

            _rtxtTotalCoinOut.text = $"{bussinessDayRecord.total_coin_out_credit}";

            _rtxtTotalCoinIn.text = $"{bussinessDayRecord.total_coin_in_credit}";

            _rtxtTotalProfitCoinInOut.text = $"{bussinessDayRecord.total_coin_in_credit - bussinessDayRecord.total_coin_out_credit}";

        });
    }

}
