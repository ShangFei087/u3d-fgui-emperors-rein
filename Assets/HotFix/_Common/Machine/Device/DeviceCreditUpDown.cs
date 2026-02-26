#define SQLITE_ASYNC
using GameMaker;
using Sirenix.OdinInspector;
using System;
using UnityEngine;


// 上下分短按: 按 投币比例
// 上分长按 ： 1000；
// 下分长按 请0


public class DeviceCreditUpDown : MonoSingleton<DeviceCreditUpDown>
{

    [Button]
    public void CreditUp(bool isLongClick = false)
    {
        if (!SBoxModel.Instance.isMachineActive)
        {
            DebugUtils.LogWarning("Machine not activated");
            return;
        }

        /*if (BlackboardUtils.GetValue<bool>("./isSpin"))
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("<size=26>Cannot recharge during the game period</size>"));
            return;
        }*/

        int scoreUpCredit =
            1 * (isLongClick ? SBoxModel.Instance.ScoreUpScaleLongClick : SBoxModel.Instance.ScoreUpDownScale);

        MachineDataManager02.Instance.RequestScoreUp(scoreUpCredit, (Action<object>)((object res) =>
        {
            int credit = (int)res;
            string orderId = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.ScoreUp); 

            TableCoinInOutRecordItem record = new TableCoinInOutRecordItem()
            {
                device_type = "score_up",
                device_number = -1,
                order_id = orderId,
                count = 1,
                credit_before = SBoxModel.Instance.myCredit,
                in_out = 1,
                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };


            SBoxModel.Instance.myCredit += credit;

            MainBlackboardController.Instance.AddOrSyncMyCreditToReal(credit);


            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.MachineCoinIn);


            record.credit_after = SBoxModel.Instance.myCredit;
            record.credit = record.credit_after - record.credit_before;


            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                ConsoleTableName.TABLE_COIN_IN_OUT_RECORD, record);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //每日统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalScoreUp(record.credit, record.credit_after);

        }));


    }


    /// <summary>
    /// 长按下分清0
    /// </summary>
    public void CreditAllDown()
    {
        if (!SBoxModel.Instance.isMachineActive)
        {
            DebugUtils.LogWarning("Machine not activated");
            return;
        }

        if (SBoxModel.Instance.myCredit <= 0)
            return;

        //if (MainModel.Instance.isSpin)
        if (MainModel.Instance.contentMD?.isSpin ?? false)
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Cannot score down during the game period"));
            return;
        }


        int scoreDownCredit = (int)SBoxModel.Instance.myCredit;


        MachineDataManager02.Instance.RequestScoreDown(scoreDownCredit, (Action<object>)((res) =>
        {
            int credit = (int)res;

            string orderId = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.ScoreDown);

            TableCoinInOutRecordItem record = new TableCoinInOutRecordItem()
            {
                device_type = "score_down",
                device_number = -1,
                //order_id = Guid.NewGuid().ToString(),
                order_id = orderId,
                count = 1,
                credit_before = SBoxModel.Instance.myCredit,
                in_out = 0,
                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };



            long myCredit = SBoxModel.Instance.myCredit;
            if (credit > myCredit)
                SBoxModel.Instance.myCredit = 0;
            else
                SBoxModel.Instance.myCredit = myCredit - credit;

            //EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT, new EventData<bool>(MetaUIEvent.UpdateNaviCredit, true));

            MainBlackboardController.Instance.MinusOrSyncMyCreditToReal(credit);

            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.MachineCoinIn);

            record.credit_after = SBoxModel.Instance.myCredit;
            record.credit = record.credit_before - record.credit_after;

            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                ConsoleTableName.TABLE_COIN_IN_OUT_RECORD, record);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //每日统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalScoreDown(record.credit, record.credit_after);
        }));

    }

    [Button]
    public void CreditDown()
    {
        if (!SBoxModel.Instance.isMachineActive)
        {
            DebugUtils.LogWarning("Machine not activated");
            return;
        }

        if (SBoxModel.Instance.myCredit <= 0)
            return;

        if (MainModel.Instance.isSpin)
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Cannot score down during the game period"));
            return;
        }

        int scoreDownCredit = SBoxModel.Instance.ScoreUpDownScale * 1;

        if (scoreDownCredit > MainModel.Instance.myCredit)
            scoreDownCredit = (int)MainModel.Instance.myCredit;


        MachineDataManager02.Instance.RequestScoreDown(scoreDownCredit, (Action<object>)((res) =>
        {
            int credit = (int)res;

            string orderId = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.ScoreDown);

            TableCoinInOutRecordItem record = new TableCoinInOutRecordItem()
            {
                device_type = "score_down",
                device_number = -1,
                order_id = orderId,// Guid.NewGuid().ToString(),
                count = 1,
                credit_before = SBoxModel.Instance.myCredit,
                in_out = 0,
                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };


            long myCredit = SBoxModel.Instance.myCredit;
            if (credit > myCredit)
                SBoxModel.Instance.myCredit = 0;
            else
                SBoxModel.Instance.myCredit = myCredit - credit;


            MainBlackboardController.Instance.MinusOrSyncMyCreditToReal(credit);

            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.MachineCoinIn);

            record.credit_after = SBoxModel.Instance.myCredit;
            record.credit = record.credit_before - record.credit_after;

            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                ConsoleTableName.TABLE_COIN_IN_OUT_RECORD, record);
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //每日统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalScoreDown(record.credit, record.credit_after);
        }));

    }
}
