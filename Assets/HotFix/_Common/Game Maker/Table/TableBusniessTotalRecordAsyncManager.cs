using GameMaker;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 这个脚本用于统计日营收记录和总营收记录
/// </summary>
public class TableBusniessTotalRecordAsyncManager : Singleton<TableBusniessTotalRecordAsyncManager>
{

    /// <summary> 历史总压注 </summary>
    public long historyTotalBet
    {
        get => targer.total_bet_credit;
    }

    /// <summary> 历史总赢 </summary>
    public long historyTotalWin
    {
        get => targer.total_win_credit;
    }

    /// <summary> 历史总投币 </summary>
    public long historyTotalCoinInCredit
    {
        get => targer.total_coin_in_credit;
    }


    /// <summary> 历史总退票 </summary>
    public long historyTotalCoinOutCredit
    {
        get => targer.total_coin_out_credit;
    }


    /// <summary> 历史总上分 </summary>
    public long historyTotalScoreUpCredit
    {
        get => targer.total_score_up_credit;
    }
 

    /// <summary> 历史总下分 </summary>
    public long historyTotalScoreDownCredit
    {
        get => targer.total_score_down_credit;
    }


    TableBussinessTotalRecordItem targer = null;




    public void GetTotalBusniess()
    {
        SQLiteAsyncHelper.Instance.GetDataAsync<TableBussinessTotalRecordItem>(
            ConsoleTableName.TABLE_BUSINESS_TOTAL_RECORD,
            $"SELECT * FROM bussiness_total_record WHERE id = 1 ",  //{ConsoleTableName.TABLE_BUSINESS_TOTAL_RECORD}
            TableBussinessTotalRecordItem.DefaultTable(),
            (object[] res ) =>
            {
                if ( (int)res[0] == 0)
                {
                    List<TableBussinessTotalRecordItem> lst = JsonConvert.DeserializeObject<List<TableBussinessTotalRecordItem>>((string)res[1]);
                    for (int i=0; i< lst.Count; i++)
                    {
                        if (lst[i].id == 1)
                        {
                            targer = lst[i];
                            break;
                        }
                    }
                    if(targer == null)
                    {
                        DebugUtils.LogError($"没有获取目标对象: 历史总投退、总押总赢");
                    }
                }
                else
                {
                    DebugUtils.LogError((string)res[1]);
                }
            }

            );
            
    }



    public void AddTotalCoinIn(long credit, long creditAfter)
    {
        long creditBefore = creditAfter - credit;

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string selectQuery = $" SELECT * FROM bussiness_total_record WHERE id = 1 ";

        string updateQuery = $"UPDATE bussiness_total_record " +
                            $"SET total_coin_in_credit = total_coin_in_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE id = 1 ";

        string insertQuery = $"INSERT INTO bussiness_total_record ( created_at, total_coin_in_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        targer.total_coin_in_credit += credit;
    }




    [Button]
    public void AddTotalCoinOut(long credit, long creditAfter)
    {
        long creditBefore = creditAfter + credit;

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string selectQuery = $" SELECT * FROM bussiness_total_record WHERE id = 1 ";


        string updateQuery = $"UPDATE bussiness_total_record " +
                            $"SET total_coin_out_credit = total_coin_out_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE id = 1 ";

        string insertQuery = $"INSERT INTO bussiness_total_record ( created_at, total_coin_out_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} ) ";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        targer.total_coin_out_credit += credit;
    }



    [Button]
    public void AddTotalScoreUp(long credit, long creditAfter)
    {
        long creditBefore = creditAfter - credit;

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string selectQuery = $" SELECT * FROM bussiness_total_record WHERE id = 1 ";

        string updateQuery = $"UPDATE bussiness_total_record " +
                            $"SET total_score_up_credit = total_score_up_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE id = 1 ";

        string insertQuery = $"INSERT INTO bussiness_total_record ( created_at, total_score_up_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });
        targer.total_score_up_credit += credit;
    }


    [Button]
    public void AddTotalScoreDown(long credit, long creditAfter)
    {
        long creditBefore = creditAfter + credit;

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string selectQuery = $" SELECT * FROM bussiness_total_record WHERE id = 1 ";


        string updateQuery = $"UPDATE bussiness_total_record " +
                            $"SET total_score_down_credit = total_score_down_credit + {credit}, " +
                                $"credit_after = {creditAfter} " +
                            $"WHERE id = 1  ";

        string insertQuery = $"INSERT INTO bussiness_total_record ( created_at, total_score_down_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {credit},  {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        targer.total_score_down_credit += credit;
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

        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string selectQuery = $" SELECT * FROM bussiness_total_record WHERE id = 1 ";

        string updateQuery = $"UPDATE bussiness_total_record " +
                        $"SET total_bet_credit = total_bet_credit + {bet}, " +
                            $"total_win_credit = total_win_credit + {win}, " +
                            $"credit_after = {creditAfter} " +
                        $"WHERE id = 1 ";

        string insertQuery = $"INSERT INTO bussiness_total_record ( created_at, total_bet_credit, total_win_credit, credit_before, credit_after  ) " +
            $"VALUES ( {createdAt}, {bet}, {win}, {creditBefore}, {creditAfter} )";

        SQLiteAsyncHelper.Instance.ExecuteUpdateOrInsertAsync(selectQuery, updateQuery, insertQuery, (isOk) =>
        {

        });

        targer.total_bet_credit += bet;

        targer.total_win_credit += win;
    }


}
