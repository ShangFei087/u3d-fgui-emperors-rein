#define SQLITE_ASYNC
using GameMaker;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 投币
/// </summary>
public partial class DeviceCoinIn : MonoSingleton<DeviceCoinIn>
{

    const string COR_COIN_IN_OUT_TIME = "COR_COIN_IN_OUT_TIME";
    const string DEVICE_COIN_IN_ORDER = "device_coin_in_order";
    const string DEVICE_COIN_IN_NUM = "device_coin_in_num";

    string orderIdCoinIn;
    int lastCoinInId = -1;


    JSONNode _cacheCoinInOrder = null;
    JSONNode cacheCoinInOrder
    {
        get
        {
            if (_cacheCoinInOrder == null)
                _cacheCoinInOrder = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DEVICE_COIN_IN_ORDER, "{}"));
            return _cacheCoinInOrder;
        }
        //set => _cacheCoinInOrder = value;
    }

    JSONNode _cacheCoinInNum = null;
    JSONNode cacheCoinInNum
    {
        get
        {
            if (_cacheCoinInNum == null)
                _cacheCoinInNum = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DEVICE_COIN_IN_NUM, "{}"));
            return _cacheCoinInNum;
        }
    }

    public bool isRegularCoinIning
    {
        get => coCoinInOutTime != null; // IsCo(COR_COIN_IN_OUT_TIME);
    }
    private void OnEnable()
    {
        //if (!ApplicationSettings.Instance.isMachine) return;

        EventCenter.Instance.AddEventListener<CoinInData>(SBoxSanboxEventHandle.COIN_IN, OnHardwareCoinIn);
        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT, OnCLearAllOrderCache);
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<CoinInData>(SBoxSanboxEventHandle.COIN_IN, OnHardwareCoinIn);
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT, OnCLearAllOrderCache);
    }

    private void OnCLearAllOrderCache(EventData res)
    {
        if (res.name == GlobalEvent.ClearAllOrderCache)
        {
            _cacheCoinInOrder = null;
            _cacheCoinInNum = null;
            SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_ORDER, "{}");
            SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_NUM, "{}");
        }
    }


    private void OnHardwareCoinIn(CoinInData coinInData)
    {

        DebugUtils.LogWarning($"CoinIn id = {coinInData.id} value = {coinInData.coinNum}");

        if (coinInData.coinNum <= 0)
        {
            return;
        }

        if ( MainModel.Instance.isSpin)
        {
            MainModel.Instance.isRequestToRealCreditWhenStop = true;
        }


        MachineDataManager02.Instance.RequestCounter(0, coinInData.coinNum, 2, (res) =>
        {
            int resault = (int)res;
            if (resault < 0)
                DebugUtils.LogError($"投币码表 : 返回状态：{resault}  投币个数：{coinInData.coinNum}");
            else
                DebugUtils.Log($"投币码表 : 返回状态：{resault}  投币个数：{coinInData.coinNum}");
        });


        DeviceOrderReship.Instance.DelayReshipOrderRepeat();


        string deviceId = $"{coinInData.id}";
        /*if (!cacheCoinInNum.HasKey(deviceId))
        {
            JSONNode nodeOrder = JSONNode.Parse("{}");
            nodeOrder["count"] = coinInData.coinNum;
            nodeOrder["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            cacheCoinInNum.Add(deviceId, nodeOrder);
        }
        else
        {
            cacheCoinInNum[deviceId]["count"] += coinInData.coinNum;
            cacheCoinInNum[deviceId]["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }*/

        cacheCoinInNum[deviceId]["count"] += coinInData.coinNum;
        cacheCoinInNum[deviceId]["scale"] = SBoxModel.Instance.CoinInScale;
        cacheCoinInNum[deviceId]["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.MachineCoinIn);

        SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_NUM, cacheCoinInNum.ToString());


        // 加钱动画 
        int addCredit = coinInData.coinNum * SBoxModel.Instance.CoinInScale;
        DebugUtils.Log($"当前金额：{SBoxModel.Instance.myCredit} +  添加金额：{addCredit} = {SBoxModel.Instance.myCredit + addCredit}");
        MainBlackboardController.Instance.AddMyTempCredit(addCredit, true, false);


        if (lastCoinInId != -1 && lastCoinInId != coinInData.id)
        {
            ClearCo(coCoinInOutTime);
            AddCoin(lastCoinInId);
            lastCoinInId = coinInData.id;
        }

        lastCoinInId = coinInData.id;


        ClearCo(coCoinInOutTime);
        coCoinInOutTime = StartCoroutine(DelayTask(
        () =>{
            AddCoin(lastCoinInId);
            lastCoinInId = -1;

            coCoinInOutTime = null;
        }, 301));

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
    Coroutine coCoinInOutTime = null;



    void AddCoin(int deviceNumber)
    {

        /* 如果在退票中，则投币不立马向算法卡入账，只是做ui界面的投币加钱动画
         * 等订单补发时才进行入账处理。
         */
        if (DeviceCoinOut.Instance.isRegularCoinOuting)
        {
            return;
        }


        int count = cacheCoinInNum[$"{deviceNumber}"]["count"];
        int scale = cacheCoinInNum[$"{deviceNumber}"]["scale"];
        cacheCoinInNum[$"{deviceNumber}"]["count"] = 0;
        cacheCoinInNum[$"{deviceNumber}"]["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_NUM, cacheCoinInNum.ToString());

        //JSONNode nd = SimpleJSON.JSONNode.Parse(string.Format("{{\"timestamp\":{0},\"id\":{1},\"count\":{2}}}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), id, count));

        int coinInCredit = scale * count;

        //订单：放入缓存:
        orderIdCoinIn = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinIn); //Guid.NewGuid().ToString();
        JSONNode nodeOrder = JSONNode.Parse("{}");
        nodeOrder["type"] = "coin_in";
        nodeOrder["order_id"] = orderIdCoinIn;
        nodeOrder["count"] = count;
        nodeOrder["scale"] = scale;
        nodeOrder["credit"] = coinInCredit;
        nodeOrder["device_number"] = deviceNumber;
        nodeOrder["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        cacheCoinInOrder[orderIdCoinIn] = nodeOrder;
        SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_ORDER, cacheCoinInOrder.ToString());

        // 硬件接口：
        RequestAddCoin(orderIdCoinIn);

    }

    void RequestAddCoin(string orderIdCoinIn, Action onFinishCallback = null)
    {


        JSONNode nodeOrder = cacheCoinInOrder[orderIdCoinIn];

        int coinInCredit = (int)nodeOrder["credit"];
        int coinInNum = (int)nodeOrder["count"];

        MachineDataManager02.Instance.RequestCoinIn(coinInNum, (Action<object>)((res) =>
        {

            long creditBefore = SBoxModel.Instance.myCredit;
            long creditAfter = SBoxModel.Instance.myCredit + coinInCredit;
            //Debug.Log($"SBoxModel.Instance.myCredit = {SBoxModel.Instance.myCredit}   creditBefore={creditBefore} creditAfter={creditAfter}");  

            // 清掉缓存订单
            cacheCoinInOrder.Remove(orderIdCoinIn);
            SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_ORDER, cacheCoinInOrder.ToString());


            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
                new TableCoinInOutRecordItem()
                {
                    device_type = nodeOrder["type"],
                    device_number = nodeOrder["device_number"],
                    order_id = nodeOrder["order_id"],
                    count = coinInNum,
                    credit = coinInCredit,
                    credit_after = creditAfter,
                    credit_before = creditBefore,
                    in_out = 1,
                    created_at = nodeOrder["timestamp"],
                });
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

            //每日统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalCoinIn((long)coinInCredit, creditAfter);

            //加钱动画
            SBoxModel.Instance.myCredit = creditAfter;

            // 上面已经加过钱
            MainBlackboardController.Instance.TrySyncMyCreditToReel();


            onFinishCallback?.Invoke();
        }));
    }



    /// <summary>
    /// 订单补发
    /// </summary>
    public IEnumerator ReshipOrde()
    {

        bool isNext = false;

        JSONNode tmpCoinInNum = JSONNode.Parse(cacheCoinInNum.ToString());

        foreach (KeyValuePair<string, JSONNode> item in tmpCoinInNum)
        {

            string deviceNumber = item.Key;
            int count = cacheCoinInNum[deviceNumber]["count"];
            int scale = cacheCoinInNum[deviceNumber]["scale"];
            if (count > 0)
            {
                cacheCoinInNum[deviceNumber]["count"] = 0;
                cacheCoinInNum[deviceNumber]["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_NUM, cacheCoinInNum.ToString());

                //订单：放入缓存:
                orderIdCoinIn = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinIn); //Guid.NewGuid().ToString();
                JSONNode nodeOrder = JSONNode.Parse("{}");
                nodeOrder["type"] = "coin_in";
                nodeOrder["order_id"] = orderIdCoinIn;
                nodeOrder["count"] = count;
                nodeOrder["scale"] = scale;
                nodeOrder["credit"] = count * scale;
                nodeOrder["device_number"] = deviceNumber;
                nodeOrder["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                cacheCoinInOrder[orderIdCoinIn] = nodeOrder;
                SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_IN_ORDER, cacheCoinInOrder.ToString());

            }
        }

        JSONNode tmpCoinInOrder = JSONNode.Parse(cacheCoinInOrder.ToString());

        foreach (KeyValuePair<string, JSONNode> item in tmpCoinInOrder)
        {

            DebugUtils.Log($"【OrderReship - CoinIn】：order id = {item.Key}  credit = {(int)item.Value["credit"]}");
            RequestAddCoin(item.Key, () =>
            {
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
        }
    }

}


public partial class DeviceCoinIn : MonoSingleton<DeviceCoinIn>
{
    /// <summary>
    /// 联网彩金中奖投币处理
    /// </summary>
    /// <param name="count"></param>
    public void OnJackpotOnlineCoinIn(int count)
    {
        string orderId = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinIn); //Guid.NewGuid().ToString();
        int coinInCredit = SBoxModel.Instance.CoinInScale * count;
        int coinInNum = count;

        MachineDataManager02.Instance.RequestCoinIn(coinInNum, (Action<object>)((res) =>
        {
            long creditBefore = SBoxModel.Instance.myCredit;
            long creditAfter = creditBefore + coinInCredit;


            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
                new TableCoinInOutRecordItem()
                {
                    device_type = "coin_in",
                    device_number = 0,
                    order_id = orderId,
                    count = coinInNum,
                    credit = coinInCredit,
                    credit_after = creditAfter,
                    credit_before = creditBefore,
                    in_out = 1,
                    created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                });
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);


            //每日统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalCoinIn((long)coinInCredit, creditAfter);

            //修改真实金额
            SBoxModel.Instance.myCredit += coinInCredit;
        }));

    }
}
public partial class DeviceCoinIn : MonoSingleton<DeviceCoinIn>
{

    /// <summary>
    /// 远程控制投币
    /// </summary>
    /// <param name="coinInCount"></param>
    /// <param name="onCallback"></param>
    public void DoCmdCoinIn(int coinInCount, Action<object> onCallback)
    {
        CoinInData data = new CoinInData();
        data.id = 0;
        data.coinNum = coinInCount;
        OnHardwareCoinIn(data);
        onCallback?.Invoke(coinInCount > 0);
    }
}