using Sirenix.OdinInspector;
using System;
using UnityEngine;

public partial class TableBusniessDayRecordAsyncManager : MonoSingleton<TableBusniessDayRecordAsyncManager>
{

    [Button]
    public void AddTotalPrinterOut(long credit, long money, long creditAfter)
    {
        //Printer Out 的账目归到 Coin Out 中

        return;

        long creditBefore = creditAfter - credit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";

        string updateQuery = $"UPDATE bussiness_day_record " +
                            $"SET total_printer_out_credit = total_printer_out_credit + {credit}, " +
                                $"total_printer_out_as_money = total_printer_out_as_money + {money}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_printer_out_credit, total_printer_out_as_money, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit}, {money}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });
        DebugUtils.LogError("没有进行营收总统计");
    }

    [Button]
    public void AddTotalBillIn(long credit, long money, long creditAfter)
    {

        //Bill In 的账目归到 Coin In 中

        return;

        long creditBefore = creditAfter - credit;



        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";

        string updateQuery = $"UPDATE bussiness_day_record " +
                            $"SET total_bill_in_credit = total_bill_in_credit + {credit}, " +
                                $"total_bill_in_as_money = total_bill_in_as_money + {money}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_bill_in_credit, total_bill_in_as_money, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit}, {money}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });
        DebugUtils.LogError("没有进行营收总统计");
    }



    public void AddTotalCoinIn(long credit, long creditAfter)
    {
        long creditBefore = creditAfter - credit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";


        string updateQuery = $"UPDATE bussiness_day_record " +
                            $"SET total_coin_in_credit = total_coin_in_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_coin_in_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        TableBusniessTotalRecordAsyncManager.Instance.AddTotalCoinIn(credit, creditAfter);
    }




    [Button]
    public void AddTotalCoinOut(long credit, long creditAfter)
    {
        long creditBefore = creditAfter + credit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";


        string updateQuery = $"UPDATE bussiness_day_record " +
                            $"SET total_coin_out_credit = total_coin_out_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_coin_out_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} ) ";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });


        TableBusniessTotalRecordAsyncManager.Instance.AddTotalCoinOut(credit, creditAfter);
    }



    [Button]
    public void AddTotalScoreUp(long credit, long creditAfter)
    {
        long creditBefore = creditAfter - credit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";


        string updateQuery = $"UPDATE bussiness_day_record " +
                            $"SET total_score_up_credit = total_score_up_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";


        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_score_up_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        TableBusniessTotalRecordAsyncManager.Instance.AddTotalScoreUp(credit, creditAfter);
    }


    [Button]
    public void AddTotalScoreDown(long credit, long creditAfter)
    {
        //long creditBefore = creditAfter - credit;  // 旧版本有bug待测
        long creditBefore = creditAfter + credit;

        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";


        string updateQuery = $"UPDATE bussiness_day_record " +
                            $"SET total_score_down_credit = total_score_down_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_score_down_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        TableBusniessTotalRecordAsyncManager.Instance.AddTotalScoreDown(credit, creditAfter);
    }


    [Button]
    public void AddTotalBet(long credit, long creditAfter)
    {
        long creditBefore = creditAfter - credit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";

        string updateQuery = $"UPDATE bussiness_day_record " +
                        $"SET total_bet_credit = total_bet_credit + {credit}, " +
                            $"credit_after = {creditAfter} " +
                        $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";


        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_bet_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });
        DebugUtils.LogError("没有进行营收总统计");
    }


    [Button]
    public void AddTotalWin(long credit, long creditAfter)
    {
        long creditBefore = creditAfter - credit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";

        string updateQuery = $"UPDATE bussiness_day_record " +
                        $"SET total_win_credit = total_win_credit + {credit}, " +
                            $"credit_after = {creditAfter} " +
                        $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_win_credit,  credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });
        DebugUtils.LogError("没有进行营收总统计");
    }



    [Button]
    void TestTimestamp()
    {

        // 指定日期
        DateTime targetDate = new DateTime(2025, 2, 17);
        //  DateTime targetDate = DateTime.Today;

        // 获取该日期的零点时间
        DateTime startOfDay = targetDate.Date;

        // 获取该日期的结束时间（即第二天的零点前一毫秒）
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);

        // 将零点时间转换为毫秒级时间戳
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);

        // 将结束时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);

        DebugUtils.Log($" startTimestamp = {startTimestamp}  endTimestamp ={endTimestamp} ");
    }

    // 将 DateTime 转换为毫秒级时间戳的方法
    static long GetMillisecondsTimestamp(DateTime dateTime)
    {
        // 定义 Unix 时间戳的起始时间
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 将输入的日期时间转换为 UTC 时间
        DateTime utcDateTime = dateTime.ToUniversalTime();

        // 计算时间差并转换为毫秒
        return (long)(utcDateTime - unixEpoch).TotalMilliseconds;
    }




    public void AddTotalBetWin(long bet, long win, long creditAfter)
    {

        long creditBefore = creditAfter - win + bet;

        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";

        string updateQuery = $"UPDATE bussiness_day_record " +
                        $"SET total_bet_credit = total_bet_credit + {bet}, " +
                            $"total_win_credit = total_win_credit + {win}, " +
                            $"credit_after = {creditAfter} " +
                        $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_bet_credit, total_win_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {bet}, {win}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        TableBusniessTotalRecordAsyncManager.Instance.AddTotalBetWin(bet, win, creditAfter);
    }



    [Button]
    public void AddJackpotWin(long win, long myCredit)
    {
        long creditBefore = myCredit - win;
        long creditAfter = myCredit;


        DateTime targetDate = DateTime.Today;
        DateTime startOfDay = targetDate.Date;// 获取该日期的零点时间
        DateTime endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);// 获取该日期的结束时间（即第二天的零点前一毫秒）
        long startTimestamp = GetMillisecondsTimestamp(startOfDay);// 将零点时间转换为毫秒级时间戳
        long endTimestamp = GetMillisecondsTimestamp(endOfDay);// 将结束时间转换为毫秒级时间戳

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //string tabName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;

        string selectQuery = $" SELECT * FROM bussiness_day_record WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp}";


        string updateQuery = $"UPDATE bussiness_day_record " +
                        $"SET total_win_credit = total_win_credit + {win}, " +
                            $"credit_after = {creditAfter} " +
                        $"WHERE created_at >= {startTimestamp} AND created_at < {endTimestamp} ";

        string insertQuery = $"INSERT INTO bussiness_day_record ( created_at, total_win_credit,  credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {win}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        DebugUtils.LogError("没有进行营收总统计");
    }

}
