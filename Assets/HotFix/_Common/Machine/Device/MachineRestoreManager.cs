#define SQLITE_ASYNC
using GameMaker;
using Mono.Data.Sqlite;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// 机台数据恢复
/// </summary>
public class MachineRestoreManager : MonoSingleton<MachineRestoreManager>
{

    //const string COR_CLEAR_SQL_TABLE = "COR_CLEAR_SQL_TABLE";


    public  void Start()
    {

    }

    protected override void OnDestroy()
    {
        //CoroutineAssistant.ClearCo(COR_CLEAR_SQL_TABLE);

        ClearCo(corClearSqlTable);

        base.OnDestroy();
    }
    /*
    public void OnFirstInstall()
    {
        DebugUtils.Log("首次安装，复位所有参数");
        RestoreToFactorySettings();
    }*/


    /// <summary>
    /// 清除“游戏记录”，“投退币记录”数据。
    /// </summary>
    [Button]
    public void ClearRecordWhenCoding(Action callback = null)
    {
       //DebugUtils.LogError(" 调用 ClearRecordWhenCoding");
        // GameJackpotCreator.Instance.ResetGameJackpot();  //彩金生成器复位


        //CoroutineAssistant.DoCo(COR_CLEAR_SQL_TABLE, ClearSQLTable(callback));

        ClearCo(corClearSqlTable);
        corClearSqlTable = StartCoroutine(ClearSQLTable(callback));
    }



    void ClearCo(Coroutine co)
    {
        if (co != null)
            StopCoroutine(co);
        co = null;
    }
    IEnumerator DelayTask(Action task, int timeMS)
    {
        yield return new WaitForSeconds((float)timeMS / 1000f);
        task?.Invoke();
    }
    IEnumerator RepeatTask(Action task, int timeMS)
    {
        while (true)
        {
            yield return new WaitForSeconds((float)timeMS / 1000f);
            task?.Invoke();
        }
    }
    Coroutine corClearSqlTable;  

    /*
    /// <summary>
    /// 清除码表（码表目前无法清除）
    /// </summary>
    void ClearMachineCounter()
    {
        MachineDataManager02.Instance.RequestCounter(0, 0, 1, (res) =>
        {
            int resault = (int)res;

            DebugUtils.Log($"清除投币码表 : {resault}");

            // 这里必须嵌套，MachineDataManager的方法无法重复调用，响应回调会被覆盖
            MachineDataManager02.Instance.RequestCounter(1, 0, 1, (res) =>
            {
                int resault = (int)res;
                DebugUtils.Log($"清除退币码表 : {resault}");
            });
        });
    }
    */


    IEnumerator ClearSQLTable(Action callback = null)
    {

        string dbName = ConsoleTableName.DB_NAME;
        string tableName = ConsoleTableName.TABLE_COIN_IN_OUT_RECORD;
        string rowName = "created_at";

        bool isNext = false;


        if (!SQLiteAsyncHelper.Instance.isConnect)
        {
            DebugUtils.LogError($"【Check Record】{dbName} is close");

            callback?.Invoke();
            yield break;
        }

        // 清除投退币
        tableName = ConsoleTableName.TABLE_COIN_IN_OUT_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_COIN_IN_OUT_RECORD}");


        // 清除每日营收记录
        tableName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_BUSINESS_DAY_RECORD}");

        // 清除总营收记录
        tableName = ConsoleTableName.TABLE_BUSINESS_TOTAL_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_BUSINESS_TOTAL_RECORD}");

        // 置位总营收数据
        TableBusniessTotalRecordAsyncManager.Instance.GetTotalBusniess();

        // 清除彩金记录
        tableName = ConsoleTableName.TABLE_JACKPOT_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_JACKPOT_RECORD}");

        // 清除报错日志
        tableName = ConsoleTableName.TABLE_LOG_ERROR_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_LOG_ERROR_RECORD}");

        // 清掉事件日志
        tableName = ConsoleTableName.TABLE_LOG_EVENT_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_LOG_EVENT_RECORD}");


        // 清除游戏记录
        tableName = ConsoleTableName.TABLE_SLOT_GAME_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_SLOT_GAME_RECORD}");

        tableName = ConsoleTableName.TABLE_PUSHER_GAME_RECORD;
        SQLiteAsyncHelper.Instance.ExecuteDeleteAsync(tableName, (res) =>
        {
            isNext = true;
        });
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        DebugUtils.Log($"【Clear Record】清除 {ConsoleTableName.TABLE_PUSHER_GAME_RECORD}");


        if (ApplicationSettings.Instance.isMock)
        {
            // 清掉玩家金额，赢分，历史总投退数据
            SBoxModel.Instance.SboxPlayerAccount = MachineDataManager02.Instance.RequestClearPlayerAccountWhenMock();
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            /* // 清掉游戏jackpot*/
        }

        // 删除编号
        MainBlackboardController.Instance.ClearGameNumber();
        MainBlackboardController.Instance.ClearReportId();

        // 删除订单缓存
        EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT, new EventData(GlobalEvent.ClearAllOrderCache));


        DebugUtils.Log("【Clear Record】清掉所有本地游戏记录数据");
        callback?.Invoke();
    }


/*

    /// <summary>
    /// 恢复出厂设置
    /// </summary>
    public void RestoreToFactorySettings()
    {
        // 清楚记录
        DoCo(COR_CLEAR_SQL_TABLE, ClearSQLTable());

        // 恢复投退币参数设置
        SBoxModel.Instance.billInScale = DefaultSettingsUtils.defBillInScale;
        SBoxModel.Instance.printOutScale = DefaultSettingsUtils.defPrintOutScale;
        SBoxModel.Instance.coinInScale = DefaultSettingsUtils.defCoinInScale;
        SBoxModel.Instance.coinOutScaleCreditPerTicket = DefaultSettingsUtils.defCoinOutPerTicket2Credit;
        SBoxModel.Instance.coinOutScaleTicketPerCredit = DefaultSettingsUtils.defCoinOutPerCredit2Ticket;
        SBoxModel.Instance.scoreUpDownScale = DefaultSettingsUtils.defScoreUpDownScale;

        // 默认语言
        SBoxModel.Instance.language = DefaultSettingsUtils.defLanguage;

        // 默认密码
        SBoxModel.Instance.passwordAdmin = DefaultSettingsUtils.passwordAdmin;
        SBoxModel.Instance.passwordManager = DefaultSettingsUtils.passwordManager;
        SBoxModel.Instance.passwordShift = DefaultSettingsUtils.passwordShift;

        // 记录最大次数
        SBoxModel.Instance.errorRecordMax = DefaultSettingsUtils.defMaxErrorRecord;
        SBoxModel.Instance.eventRecordMax = DefaultSettingsUtils.defMaxEventRecord;
        SBoxModel.Instance.jackpotRecordMax = DefaultSettingsUtils.defMaxJackpotRecord;
        SBoxModel.Instance.businiessDayRecordMax = DefaultSettingsUtils.defMaxBusinessDayRecord;
    }
*/
}
