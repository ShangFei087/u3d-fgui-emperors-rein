using SimpleJSON;
using System;
using GameMaker;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Sirenix.OdinInspector;

public class DeviceRemoteInOut : MonoSingleton<DeviceRemoteInOut>
{
    /*
    const string DEVICE_REMOTE_OUT_ORDER = "device_remote_out_order";
    const string DEVICE_REMOTE_IN_ORDER = "device_remote_in_order";

    JSONNode cacheRemoteInOrder
    {
        get
        {
            if (_cacheRemoteInOrder == null)
                _cacheRemoteInOrder = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DEVICE_REMOTE_IN_ORDER, "{}"));
            return _cacheRemoteInOrder;
        }
    }
    JSONNode _cacheRemoteOutOrder;


    JSONNode cacheRemoteOutOrder
    {
        get
        {
            if (_cacheRemoteOutOrder == null)
                _cacheRemoteOutOrder = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DEVICE_REMOTE_OUT_ORDER, "{}"));
            return _cacheRemoteOutOrder;
        }
    }
    JSONNode _cacheRemoteInOrder;
    */



    Dictionary<string,float> orderIdDict = new Dictionary<string, float>();




    bool isDirty = true;
    private void Update()
    {
        if (isDirty)
        {
            isDirty = false;

            if (orderIdDict.Count > 0)
            {
                int index = orderIdDict.Count;
                while (--index > 0)
                {
                    KeyValuePair<string, float> kv = orderIdDict.ElementAt(index);
                    if (Time.unscaledTime - kv.Value > 301)  //5分钟1秒
                    {
                        orderIdDict.Remove(kv.Key);
                    }
                }
            }

            isDirty = true; 
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="orderId"></param>
    /// <param name="cedit"></param>
    /// <returns></returns>
    /// <remarks>
    /// * 点单号5分钟超时
    /// * 服务器订单id#时间戳#签名 = md5码
    /// * 服务器订单id#时间戳#md5码
    /// </remarks>
    object[] CheckOrderId(string type, string orderId, int cedit)
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            ["order_id"] = orderId,
            ["credit"] = cedit,
            ["type"] = type,
            ["credit_after"] = 0
        };

        if (!SBoxModel.Instance.isMachineActive)
        {
            return new object[] { 1, "Machine not activated.", data };
        }

        string[] strs = orderId.Split('#');
        if (strs.Length != 3)
        {
            return new object[] { 1, "The order id is invalid.", data };
        }

        string checkId = $"{strs[0]}#{strs[1]}#{SBoxModel.Instance.remoteSecretkey}";
        checkId = GameCommon.Utils.ComputeMD5ForStr(checkId);
        if (strs[2] != checkId)
        {
            return new object[] { 1, "The order id is invalid.", data };
        }

        /*
         // 等机台支持设置本地时间，才打开该功能
        try
        {
            long theTimeStamp = long.Parse(strs[1]);
            long nowTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (nowTimeStamp < theTimeStamp || nowTimeStamp - theTimeStamp > 1000 * 60 * 5) // 5分钟
            {
                return new object[] { 1, $"The order id is over time. {nowTimeStamp}", data };
            }
        }
        catch (Exception e)
        {
            return new object[] { 1, "The order id is invalid.", data };
        }
        */

        foreach (string str in orderIdDict.Keys)
        {
            if (str.StartsWith(strs[0]))
            {
                return new object[] { 1, "The order has been processed.", data };
            }
        }
        
        /*
        if (orderIdDict.Keys.Contains(orderId))
        {
            return new object[] { 1, "The order has been processed.", data };
        }*/

        return new object[] { 0, "" };
    }

    public void CreditUp( string type , string orderId, int scoreUpCredit, Action<object[]> callback)
    {
        if (!SBoxModel.Instance.isMachineActive)
        {
            callback?.Invoke(new object[] { 1, "Machine not activated" });
            DebugUtils.LogWarning("Machine not activated");
            return;
        }


        object[] result = CheckOrderId(type, orderId, scoreUpCredit);
        if ((int)result[0] != 0)
        {
            callback?.Invoke(result);
            return;
        }
        orderIdDict.Add(orderId, Time.unscaledTime);


        if (MainModel.Instance.isSpin)
            MainModel.Instance.isRequestToRealCreditWhenStop = true;



        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            ["order_id"] = orderId,
            ["credit"] = scoreUpCredit,
            ["type"] = type,
            ["credit_after"] = 0
        };

        string[] strs = orderId.Split('#');
        string oid = strs[0];
  
        MachineDataManager02.Instance.RequestScoreUp(scoreUpCredit, (Action<object>)((object res) =>
        {
            int credit = (int)res;
            //string orderId = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.ScoreUp);

            TableCoinInOutRecordItem record = new TableCoinInOutRecordItem()
            {
                device_type = type, // "remote_in", 
                device_number = -1,
                order_id = oid, 
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

   
            TableBusniessDayRecordAsyncManager.Instance.AddTotalScoreUp(record.credit, record.credit_after);


            data["credit_after"] = record.credit_after;
            callback?.Invoke(new object[] { 0, "", data});
        }));

 
    }


    Coroutine coCreditDown = null;


    IEnumerator DoCoCreditDown(string type, string orderId, int scoreDownCredit, Action<object[]> callback)
    {

        if (MainModel.Instance.contentMD != null)
        {
            if (MainModel.Instance.contentMD.isSpin) {
                TipPopupHandler02.Instance.OpenPopup("【温馨提示】：即将停止游戏，进行退票。");
                MainModel.Instance.contentMD.isRequestToStop = true;
            }

            yield return new WaitUntil(()=> MainModel.Instance.contentMD.isSpin == false);

        }

        object[] result = CheckOrderId(type, orderId, scoreDownCredit);
        if ((int)result[0] != 0)
        {
            callback?.Invoke(result);
            yield break;
        }

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            ["order_id"] = orderId,
            ["credit"] = scoreDownCredit,
            ["type"] = type,
            ["credit_after"] = 0
        };


        if (MainModel.Instance.isSpin)
        {
            callback?.Invoke(new object[] { 1, "Cannot score down during the game period", data });
            yield break;
        }



        orderIdDict.Add(orderId, Time.unscaledTime);


        string[] strs = orderId.Split('#');
        string oid = strs[0];


        MachineDataManager02.Instance.RequestScoreDown(scoreDownCredit, (Action<object>)((res) =>
        {
            int credit = (int)res;

            //string orderId = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.ScoreDown);

            TableCoinInOutRecordItem record = new TableCoinInOutRecordItem()
            {
                device_type = "score_down",
                device_number = -1,
                order_id = oid,// Guid.NewGuid().ToString(),
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


            TableBusniessDayRecordAsyncManager.Instance.AddTotalScoreDown(record.credit, record.credit_after);


            data["credit_after"] = record.credit_after;
            callback?.Invoke(new object[] { 0, "", data });
        }));
    }


    public void CreditDown(string type, string orderId, int scoreDownCredit, Action<object[]> callback)
    {

        if (coCreditDown != null)
            StopCoroutine(coCreditDown);
        coCreditDown = null;
        coCreditDown = StartCoroutine(DoCoCreditDown(type, orderId, scoreDownCredit, callback));
    }







    [Button]
    void TestScoreUp(int credit = 1000)
    {
        orderId++;
        long timeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string orderIdStr = $"{orderId}#{timeMS}#{SBoxModel.Instance.remoteSecretkey}";
        string checkId = GameCommon.Utils.ComputeMD5ForStr(orderIdStr);
        orderIdStr = $"{orderId}#{timeMS}#{checkId}";

        DeviceRemoteInOut.Instance.CreditUp(
        "test_in", orderIdStr, credit,
        (result) =>
        {
        });
    }


    int orderId = 0;
    [Button]
    void TestScoreDown(int credit = -1)
    {

        int targetCredit = 0;

        if (credit == -1)
        {
            targetCredit = (int)SBoxModel.Instance.myCredit;
        }
        else if (credit > 0)
        {
            targetCredit = credit;
        }
        orderId++;
        long timeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string orderIdStr = $"{orderId}#{timeMS}#{SBoxModel.Instance.remoteSecretkey}";
        string checkId = GameCommon.Utils.ComputeMD5ForStr(orderIdStr);
        orderIdStr = $"{orderId}#{timeMS}#{checkId}";

        DeviceRemoteInOut.Instance.CreditDown(
        "test_out", orderIdStr, targetCredit,
        (result) =>
        {
        });
    }
}
