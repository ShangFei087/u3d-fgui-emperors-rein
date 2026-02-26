#define SQLITE_ASYNC_0
using GameMaker;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;




#region 删除或清除表
public static partial class ConsoleTableUtils
{

    /// <summary>
    /// 删除多个表
    /// </summary>
    public static void DeleteTables()
    {
        bool isVerChange = false;
        string cacherStr = SQLitePlayerPrefs03.Instance.GetString(ConsoleTableName.TABLE_VER_CACHER, "{}");
        if (string.IsNullOrEmpty(cacherStr))
            cacherStr = "{}";

        DebugUtils.Log($"old table ver： {cacherStr}");
        JSONNode tableVerCacher = JSONNode.Parse(cacherStr);

        DeleteTable(ConsoleTableName.TABLE_COIN_IN_OUT_RECORD, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_SYS_SETTING, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_SLOT_GAME_RECORD, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_PUSHER_GAME_RECORD, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_LOG_ERROR_RECORD, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_LOG_EVENT_RECORD, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_BET, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_BUSINESS_DAY_RECORD, ref tableVerCacher, ref isVerChange);
        DeleteTable(ConsoleTableName.TABLE_JACKPOT_RECORD, ref tableVerCacher, ref isVerChange);
        if (isVerChange)
        {
            string tableVerStr = tableVerCacher.ToString();
            DebugUtils.Log($"new table ver： {tableVerStr}");
            SQLitePlayerPrefs03.Instance.SetString(ConsoleTableName.TABLE_VER_CACHER, tableVerStr);
        }

    }


    //ref和out是用于传递参数引用的两种关键字
    //ref 参数在方法调用前必须被初始化。
    //out 参数在方法调用前不需要被初始化，且通常是由方法内部进行初始化的。
    /// <summary>
    /// 删除表
    /// </summary>
    public static void DeleteTable(string tableName, ref JSONNode tableVerCacher, ref bool isVerChange , Action<object[]> ononFinishCallback = null)
    {
        if (!tableVerCacher.HasKey(tableName) ||
            (string)tableVerCacher[tableName] !=
            ConsoleTableName.tableVer[tableName]
        )
        {
            isVerChange = true;
            tableVerCacher[tableName] = ConsoleTableName.tableVer[tableName];
#if !SQLITE_ASYNC || true
            if (SQLiteHelper.Instance.CheckTableExists(tableName))
            {
                string sql = $"DROP TABLE {tableName};";
                DebugUtils.LogWarning($"delete table：{tableName} ！   upadte to version：{ConsoleTableName.tableVer[tableName]}");
                SQLiteHelper.Instance.ExecuteNonQuery(sql);//刪除表
            }

            ononFinishCallback?.Invoke(new object[] { 0 });
#else
           SQLiteAsyncHelper.Instance.ExecuteDropTableAsync(tableName, ononFinishCallback); // 可以用

            // SQLiteAsyncHelper.Instance.ExecuteDropTableAsync02(tableName, ononFinishCallback);// 可以用
#endif
        }
    }
}

#endregion





#region 创建默认表
public static partial class ConsoleTableUtils
{

    public static async Task CheckOrCreatTableBet(Action<object[]> ononFinishCallback = null)
    {


#if !SQLITE_ASYNC || true

        if (!SQLiteHelper.Instance.CheckTableExists(ConsoleTableName.TABLE_BET))
        {
            string sql = SQLiteHelper.SQLCreateTable<TableBetItem>(ConsoleTableName.TABLE_BET);
            SQLiteHelper.Instance.ExecuteNonQuery(sql);

            TableBetItem[] defaultTable = await TableBetItem.DefaultTable();
            foreach (TableBetItem item in defaultTable)
            {
                sql = SQLiteHelper.SQLInsertTableData<TableBetItem>(ConsoleTableName.TABLE_BET, item);
                SQLiteHelper.Instance.ExecuteNonQuery(sql);
            }

        }
        ononFinishCallback?.Invoke(new object[] { 0 });
#else
        SQLiteAsyncHelper.Instance.CheckOrCreatTableAsync<TableBetItem>(ConsoleTableName.TABLE_BET, await TableBetItem.DefaultTable(), ononFinishCallback);
#endif

    }



    public static void CheckOrCreatTableCoinInOutRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TableCoinInOutRecordItem>(ConsoleTableName.TABLE_COIN_IN_OUT_RECORD, ononFinishCallback);




    public static void CheckOrCreatTableSysSetting(Action<object[]> ononFinishCallback = null)
    {

#if !SQLITE_ASYNC || true

        if (!SQLiteHelper.Instance.CheckTableExists(ConsoleTableName.TABLE_SYS_SETTING))
        {
            string sql = SQLiteHelper.SQLCreateTable<TableSysSettingItem>(ConsoleTableName.TABLE_SYS_SETTING);
            SQLiteHelper.Instance.ExecuteNonQuery(sql);

            TableSysSettingItem[] defaultTable = TableSysSettingItem.DefaultTable();
            foreach (TableSysSettingItem item in defaultTable)
            {
                sql = SQLiteHelper.SQLInsertTableData<TableSysSettingItem>(ConsoleTableName.TABLE_SYS_SETTING, item);
                SQLiteHelper.Instance.ExecuteNonQuery(sql);
            }
        }
        ononFinishCallback?.Invoke(new object[] { 0 });

#else
        SQLiteAsyncHelper.Instance.CheckOrCreatTableAsync<TableSysSettingItem>(ConsoleTableName.TABLE_SYS_SETTING, TableSysSettingItem.DefaultTable(), ononFinishCallback);
#endif

    }



    public static void CheckOrCreatTableJackpotRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TableJackpotRecordItem>(ConsoleTableName.TABLE_JACKPOT_RECORD, ononFinishCallback);



    public static void CheckOrCreatTableSlotGameRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TableSlotGameRecordItem>(ConsoleTableName.TABLE_SLOT_GAME_RECORD, ononFinishCallback);

    public static void CheckOrCreatTablePusherGameRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TablePusherGameRecordItem>(ConsoleTableName.TABLE_PUSHER_GAME_RECORD, ononFinishCallback);


    public static void CheckOrCreatTableLogEventRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TableLogRecordItem>(ConsoleTableName.TABLE_LOG_EVENT_RECORD, ononFinishCallback);
    public static void CheckOrCreatTableLogErrorRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TableLogRecordItem>(ConsoleTableName.TABLE_LOG_ERROR_RECORD, ononFinishCallback);

    public static void CheckOrCreatTableBussinessDayRecord(Action<object[]> ononFinishCallback = null) => CheckOrCreatTable<TableBussinessDayRecordItem>(ConsoleTableName.TABLE_BUSINESS_DAY_RECORD, ononFinishCallback);
    public static void CheckOrCreatTable<T>(string tableName, Action<object[]> ononFinishCallback = null)
    {

#if !SQLITE_ASYNC || true
        if (!SQLiteHelper.Instance.CheckTableExists(tableName))
        {
            string sql = SQLiteHelper.SQLCreateTable<T>(tableName);
            SQLiteHelper.Instance.ExecuteNonQuery(sql);
        }
        ononFinishCallback?.Invoke(new object[] {0});
#else
        SQLiteAsyncHelper.Instance.CheckOrCreatTableAsync<T>(tableName, null, ononFinishCallback);
#endif
    }
}

#endregion






#region 获取表数据


public static partial class ConsoleTableUtils
{
    public static void GetTableBet(Action<TableBetItem> onFinishCallback = null)
    {
#if !SQLITE_ASYNC
        string sql = $"SELECT* FROM {ConsoleTableName.TABLE_BET} WHERE game_id = {MainModel.Instance.gameID}";
        SQLiteHelper.Instance.ConvertSqliteToJsonAfterOpenDB(ConsoleTableName.DB_NAME, sql, (res) =>
        {

            if (res.StartsWith("[") && res.EndsWith("]"))
            {
                res = res.Substring(1, res.Length - 2);
            }

            TableBetItem tBet = JsonConvert.DeserializeObject<TableBetItem>(res);
            SBoxModel.Instance.tableBet = tBet;
            List<BetAllow> betAllowList = new List<BetAllow>();
            JSONNode node = JSONNode.Parse(tBet.bet_list);
            foreach (JSONNode nd in node)
            {
                BetAllow betAllow = JsonConvert.DeserializeObject<BetAllow>(nd.ToString());
                betAllowList.Add(betAllow);
            }

            SBoxModel.Instance.betAllowList = betAllowList;


            List<long> betList = new List<long>();
            foreach (BetAllow nd in betAllowList)
            {
                if (nd.allowed == 1)
                    betList.Add(nd.value);
            }

            //DebugUtils.LogError($"###### -- ######  betList.Count = {betList.Count}");

            SBoxModel.Instance.betList = betList;
            onFinishCallback?.Invoke(tBet);
        });

#else
        string sql = $"SELECT* FROM {ConsoleTableName.TABLE_BET} WHERE game_id = {HotfixSettings.gameId}";
        SQLiteAsyncHelper.Instance.ConvertSqliteToJsonAsync(sql, (res) =>
        {
            if (res.StartsWith("[") && res.EndsWith("]"))
            {
                res = res.Substring(1, res.Length - 2);
            }


            TableBetItem tBet = JsonConvert.DeserializeObject<TableBetItem>(res);
            SBoxModel.Instance.tableBet = tBet;


            List<BetAllow> betAllowList = new List<BetAllow>();
            JSONNode node = JSONNode.Parse(tBet.bet_list);
            foreach (JSONNode nd in node)
            {
                BetAllow betAllow = JsonConvert.DeserializeObject<BetAllow>(nd.ToString());
                betAllowList.Add(betAllow);
            }
            SBoxModel.Instance.betAllowList = betAllowList;


            List<long> betList = new List<long>();
            foreach (BetAllow nd in betAllowList)
            {
                if (nd.allowed == 1)
                    betList.Add(nd.value);
            }
            SBoxModel.Instance.betList = betList;

            onFinishCallback?.Invoke(tBet);
        });

#endif


    }

    /*public static void GetTableButtons(Action<List<TableButtonsItem>> onFinishCallback = null)
    {

        string sql = $"SELECT* FROM {ConsoleTableName.TABLE_BUTTONS}";
        SQLiteHelper02.Instance.ConvertSqliteToJsonAfterOpenDB(ConsoleTableName.DB_NAME, sql, (res) =>
        {

            JSONNode node = JSONNode.Parse(res);

            List<TableButtonsItem> lst = new List<TableButtonsItem>();
            foreach (JSONNode nd in node)
            {
                TableButtonsItem temp = JsonConvert.DeserializeObject<TableButtonsItem>(nd.ToString());
                lst.Add(temp);
            }

            SBoxModel.Instance.tableButtons = lst;

            onFinishCallback?.Invoke(lst);

        });
    }*/



    /*public static void GetTableCoinInOutRecord(Action<List<TableCoinInOutRecordItem>> onFinishCallback = null)
    {

        //string sql = $"SELECT* FROM {ConsoleTableName.TABLE_COIN_IN_OUT_RECORD} WHERE game_id = {HotfixSettings.gameId}";
        string sql = $"SELECT* FROM {ConsoleTableName.TABLE_COIN_IN_OUT_RECORD}";
        SQLiteHelper02.Instance.ConvertSqliteToJsonAfterOpenDB(ConsoleTableName.DB_NAME, sql, (res) =>
        {

            JSONNode node = JSONNode.Parse(res);

            List<TableCoinInOutRecordItem> lst = new List<TableCoinInOutRecordItem>();
            foreach (JSONNode nd in node)
            {
                TableCoinInOutRecordItem temp = JsonConvert.DeserializeObject<TableCoinInOutRecordItem>(nd.ToString());
                lst.Add(temp);
            }

            SBoxModel.Instance.tableCoinInOutRecord = lst;

            onFinishCallback?.Invoke(lst);
        });
    }*/






    public static void GetTableSysSetting(Action<TableSysSettingItem> onFinishCallback = null)
    {

#if !SQLITE_ASYNC

        string sql = $"SELECT* FROM {ConsoleTableName.TABLE_SYS_SETTING}";
        SQLiteHelper.Instance.ConvertSqliteToJsonAfterOpenDB(ConsoleTableName.DB_NAME, sql, (res) =>
        {

            if (res.StartsWith("[") && res.EndsWith("]"))
            {
                res = res.Substring(1, res.Length - 2);
            }

            TableSysSettingItem temp = JsonConvert.DeserializeObject<TableSysSettingItem>(res);
            SBoxModel.Instance.tableSysSetting = temp;


            onFinishCallback?.Invoke(temp);
        });

#else
        string sql = $"SELECT* FROM {ConsoleTableName.TABLE_SYS_SETTING}";
        SQLiteAsyncHelper.Instance.ConvertSqliteToJsonAsync(sql, (res) =>
        {

            if (res.StartsWith("[") && res.EndsWith("]"))
            {
                res = res.Substring(1, res.Length - 2);
            }

            TableSysSettingItem temp = JsonConvert.DeserializeObject<TableSysSettingItem>(res);
            SBoxModel.Instance.tableSysSetting = temp;


            onFinishCallback?.Invoke(temp);
        });
#endif
    }



}

 #endregion
