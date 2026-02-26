using System.Collections;
using UnityEngine;

public class DatabaseCheck : CoBehaviour02
{

    Coroutine coCheckRecord = null;
    void OnEnable()
    {
        ClearCo(coCheckRecord);
        coCheckRecord = StartCoroutine(CheckRecord());
    }

    void OnDisable()
    {
        ClearCo(coCheckRecord);
    }



    IEnumerator CheckRecord()
    {

        string dbName = ConsoleTableName.DB_NAME;
        string tableName = ConsoleTableName.TABLE_COIN_IN_OUT_RECORD;
        string rowName = "created_at";


        while (true)
        {
            //yield return new WaitForSecondsRealtime(3600f); //1小时
            yield return new WaitForSecondsRealtime(600f);

            DebugUtils.Log("【Check Record】check sql record");

            bool isNext = false;


            if (!SQLiteAsyncHelper.Instance.isConnect)
            {
                DebugUtils.LogError($"【Check Record】{dbName} is close");
                yield break;
            }



            tableName = ConsoleTableName.TABLE_COIN_IN_OUT_RECORD;// "coin_in_out_record";
            SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(tableName, (int)SBoxModel.Instance.coinInOutRecordMax, rowName,
                (res) =>
                {
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            tableName = ConsoleTableName.TABLE_SLOT_GAME_RECORD;
            SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(tableName, (int)SBoxModel.Instance.gameRecordMax, rowName,
            (res) =>
            {
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            tableName = ConsoleTableName.TABLE_PUSHER_GAME_RECORD;
            SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(tableName, (int)SBoxModel.Instance.gameRecordMax, rowName,
            (res) =>
            {
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            tableName = ConsoleTableName.TABLE_LOG_ERROR_RECORD;
            SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(tableName, (int)SBoxModel.Instance.errorRecordMax, rowName,
            (res) =>
            {
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;



            tableName = ConsoleTableName.TABLE_LOG_EVENT_RECORD;
            SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(tableName, (int)SBoxModel.Instance.eventRecordMax, rowName,
            (res) =>
            {
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;



            tableName = ConsoleTableName.TABLE_BUSINESS_DAY_RECORD;
            SQLiteAsyncHelper.Instance.ExecuteDeleteOverflowAsync(tableName, (int)SBoxModel.Instance.businiessDayRecordMax, rowName,
            (res) =>
            {
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;


        }
    }
}
