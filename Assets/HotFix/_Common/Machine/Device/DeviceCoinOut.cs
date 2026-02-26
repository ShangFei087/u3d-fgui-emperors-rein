
#define SQLITE_ASYNC
using GameMaker;
using SBoxApi;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 退票
/// </summary>
/// <remarks>
/// * 普通按钮退票 + 即中即退
/// </remarks>
public class DeviceCoinOut : MonoSingleton<DeviceCoinOut> //  CorBehaviour 
{
    const string DEVICE_COIN_OUT_ORDER = "device_coin_out_order";

    string orderIdCoinOut;

    /// <summary> 1币多少分 </summary>
    int coinOutRate;
    int targetCoinOutNum = 0;


    public JSONNode _cacheCoinOutOrder = null;
    public JSONNode cacheCoinOutOrder
    {
        get
        {
            if (_cacheCoinOutOrder == null)
                _cacheCoinOutOrder = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DeviceCoinOut.DEVICE_COIN_OUT_ORDER, "{}"));
            return _cacheCoinOutOrder;
        }
        //set => _cacheCoinOutOrder = value;
    }


    private void OnEnable()
    {
        //if (!ApplicationSettings.Instance.isMachine)return;

        EventCenter.Instance.AddEventListener<int>(SBoxSanboxEventHandle.COIN_OUT, OnHardwareCoinOut);
        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT, OnCLearAllOrderCache);

    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<int>(SBoxSanboxEventHandle.COIN_OUT, OnHardwareCoinOut);
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT, OnCLearAllOrderCache);
    }


    private void OnCLearAllOrderCache(EventData res)
    {
        if (res.name == GlobalEvent.ClearAllOrderCache)
        {
            _cacheCoinOutOrder = null;
            SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_OUT_ORDER, "{}");
        }
    }



    int m_CoinOutDeviceNumber = 0;
    public int coinOutDeviceNumber
    {
        get => m_CoinOutDeviceNumber;
    }


    void InitParam()
    {
        hasCointOutCreditForTicketPerCredit = 0;
        finishCoinOutNum = 0;
        targetCoinOutNum = 0;
        orderIdCoinOut = "";
        coinOutRate = 0;
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

    Coroutine coDelayCoinOut = null;
    Coroutine coIsCoinOutIng = null;
    Coroutine coCoinOutOutTime = null;


    /// <summary>
    /// 退票键退票
    /// </summary>
    public void DoCoinOut()
    {
        ClearCo(coDelayCoinOut);

        // 延时退票，避免退票和游戏开完同时开始
        coDelayCoinOut = StartCoroutine(DelayTask(() =>
        {
            _DoCoinOut();

            coDelayCoinOut = null;
        }, 500));

    }


    public void _DoCoinOut()
    {
        if (!SBoxModel.Instance.isMachineActive)
        {
            DebugUtils.LogWarning("Machine not activated");
            return;
        }


        if (MainModel.Instance.isSpin)
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("<size=24>Cannot coin out during the game period</size>"));
            return;
        }


        if (coIsCoinOutIng != null)
            return;

        //延时避免重复触发
        coIsCoinOutIng = StartCoroutine(DelayTask(() => coIsCoinOutIng = null , 4001));


        DeviceOrderReship.Instance.DelayReshipOrderRepeat();

        /* 机器报错时直接卡死
        if (isCoinOutError)
        {
            ShowCoinOutErrorTip();
            return;
        }*/

        // 打开遮罩
        MaskPopupHandler.Instance.OpenPopup();


        InitParam();

        // 计算能换多少个币
        targetCoinOutNum = DeviceUtils.GetCoinOutNum();

        //创建订单
        orderIdCoinOut = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinOut); // Guid.NewGuid().ToString();

        // 主板 硬件退票
        SBoxSandbox.CoinOutStart(0, this.targetCoinOutNum, 0);


        ClearCo(coCoinOutOutTime);
        coCoinOutOutTime = StartCoroutine(DelayTask(() =>
        {
            StopCoinOut();
            MaskPopupHandler.Instance.ClosePopup();
            CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
            {
                text = I18nMgr.T("<size=24>Ticket out outtime</size>"),
                type = CommonPopupType.OK,
                buttonText1 = I18nMgr.T("OK"),
                buttonAutoClose1 = true,
                callback1 = null,
                isUseXButton = false,
            });
            DeviceOrderReship.Instance.DelayReshipOrderRepeat();


            coCoinOutOutTime = null;
        }, 3001));

    }




    /// <summary>
    /// 即中即退
    /// </summary>
    public void DoCoinOutImmediately(int credit)
    {

        if (coIsCoinOutIng != null)
            return;
        coIsCoinOutIng = StartCoroutine(DelayTask(() => coIsCoinOutIng = null, 4001));//延时避免重复触发



        DeviceOrderReship.Instance.DelayReshipOrderRepeat();

        // 打开遮罩(这里是要个透明遮罩？？)
        MaskPopupHandler.Instance.OpenPopup();

        InitParam();

        // 计算能换多少个币
        targetCoinOutNum = DeviceUtils.GetCoinOutNum(credit);

        //创建订单
        orderIdCoinOut = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinOut); //Guid.NewGuid().ToString();

        // 主板 硬件退票
        SBoxSandbox.CoinOutStart(0, this.targetCoinOutNum, 0);



        ClearCo(coCoinOutOutTime);
        coCoinOutOutTime = StartCoroutine(DelayTask(() =>
        {
            StopCoinOut();
            MaskPopupHandler.Instance.ClosePopup();

            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("<size=24>Ticket out outtime</size>"));

            EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT,
                new EventData<string>(GlobalEvent.CoinOutError, "Ticket out outtime"));

            DeviceOrderReship.Instance.DelayReshipOrderRepeat();


            coCoinOutOutTime = null;
        }, 3001));


    }



    /// <summary> 是否退票中 </summary>
    public bool isRegularCoinOuting
    {
        get => coIsCoinOutIng != null ||coCoinOutOutTime != null; 
    }

    /// <summary> 硬件终止退币 </summary>
    private void StopCoinOut()
    {
        if (Application.isEditor)
        {
            //MatchDebugManager.Instance.SendUdpMessage(SBoxEventHandle.SBOX_SADNBOX_COIN_OUT_STOP, "0");
            //远程调试
            DebugUtils.LogWarning("没有实现远程调试的方法");
        }
        else
        {
            SBoxSandbox.CoinOutStop(0);
        }
        MaskPopupHandler.Instance.ClosePopup();
    }




    bool isCoinOutError;
    int finishCoinOutNum = 0;


    /// <summary> 针对1分多票的ui下分动画 </summary>
    int hasCointOutCreditForTicketPerCredit = 0;
    // int hasCoint

    /// <summary>
    /// 硬件退票响应
    /// </summary>
    /// <param name="coinOutNum01"></param>
    private void OnHardwareCoinOut(int coinOutNum01)
    {
        if (coinOutNum01 <= 0)
            return;

        MachineDataManager02.Instance.RequestCounter(1, coinOutNum01, 2, (res) =>
        {
            int resault = (int)res;
            if (resault < 0)
                DebugUtils.LogError($"退币码表 : 返回状态：{resault}  退币个数：{coinOutNum01}");
            else
                DebugUtils.Log($"退币码表 : 返回状态：{resault}  退币个数：{coinOutNum01}");
        });

        //非正规退票被调用
        if (!isRegularCoinOuting)
        {
            StopCoinOut();
            //机器非正常流程出票！！！
            //isCoinOutError = true;
            //ShowCoinOutErrorTip();
            return;
        }


        //延时避免重复触发
        coIsCoinOutIng = StartCoroutine(DelayTask(() => coIsCoinOutIng = null, 4001));



        DeviceOrderReship.Instance.DelayReshipOrderRepeat();

        // DebugUtil.LogError($"【测试】退币个累积到：{finishCoinOutNum}");

        finishCoinOutNum += coinOutNum01;
        if (finishCoinOutNum > this.targetCoinOutNum)
        {
            finishCoinOutNum = this.targetCoinOutNum;

            DebugUtils.LogError($"[Error] currCointOut = {coinOutNum01} targetCointOut = {this.targetCoinOutNum} finishCointOut = {finishCoinOutNum}");

            StopCoinOut();
            //isCoinOutError = true;
            //ShowCoinOutErrorTip();
        }

        if (!cacheCoinOutOrder.HasKey(orderIdCoinOut))
        {
            SimpleJSON.JSONNode nodeOrder = SimpleJSON.JSONNode.Parse("{}");
            nodeOrder.Add("device_number", m_CoinOutDeviceNumber);
            nodeOrder.Add("type", "coin_out");
            nodeOrder.Add("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            nodeOrder.Add("count", finishCoinOutNum); //多少币
            nodeOrder.Add("order_id", orderIdCoinOut);
            nodeOrder.Add("credit_coin_out", DeviceUtils.GetCoinOutCredit(finishCoinOutNum));
            nodeOrder.Add("credit_before", SBoxModel.Instance.myCredit);
            cacheCoinOutOrder.Add(orderIdCoinOut, nodeOrder);
        }
        else
        {
            cacheCoinOutOrder[orderIdCoinOut]["count"] = finishCoinOutNum;
            cacheCoinOutOrder[orderIdCoinOut]["credit_coin_out"] = DeviceUtils.GetCoinOutCredit(finishCoinOutNum);
        }
        SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_OUT_ORDER, cacheCoinOutOrder.ToString());

        // 前端刷新分数显示
        int outCredit = DeviceUtils.GetCoinOutCredit(coinOutNum01);

        DebugUtils.LogError($"要退积分: {outCredit}  当前临时积分：{MainBlackboardController.Instance.myTempCredit}");
        if (MainBlackboardController.Instance.myTempCredit < outCredit)
        {
            // 刷线ui界面金币显示
            MainBlackboardController.Instance.SetMyTempCredit(0);

            cacheCoinOutOrder[orderIdCoinOut]["credit_after"] = 0;

            ClearCo(coCoinOutOutTime);
            StopCoinOut();
            MaskPopupHandler.Instance.ClosePopup();
            HardwareSubCreditSaveOrder();
            return;
        }
        else
        {

            #region 针对"多票一分"导致 1票对应的小数分，被int整形过滤。
            hasCointOutCreditForTicketPerCredit += outCredit;
            if (outCredit == 0)
            {
                //本次要退的票
                int targetTatalCreditOut = DeviceUtils.GetCoinOutCredit(finishCoinOutNum);
                if (targetTatalCreditOut - hasCointOutCreditForTicketPerCredit > 0)
                {
                    outCredit = targetTatalCreditOut - hasCointOutCreditForTicketPerCredit;
                    hasCointOutCreditForTicketPerCredit = targetTatalCreditOut;
                }
            }
            #endregion


            // 刷线ui界面金币显示
            MainBlackboardController.Instance.MinusMyTempCredit(outCredit, true, false);

            cacheCoinOutOrder[orderIdCoinOut]["credit_after"] =
               (long)cacheCoinOutOrder[orderIdCoinOut]["credit_before"] -
               (long)cacheCoinOutOrder[orderIdCoinOut]["credit_coin_out"];
            //_consoleBB.Instance.myCredit;
        }





        ClearCo(coCoinOutOutTime);
        coCoinOutOutTime = StartCoroutine(DelayTask(() =>
        {
            StopCoinOut();
            MaskPopupHandler.Instance.ClosePopup();
            if (finishCoinOutNum < this.targetCoinOutNum)
            {
                // 机器的票已用光 ，请联系管理员
                // 卡票了！
                //string msg = "The tickets for the machine have been used up. Please contact the administrator.";
                CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                {
                    text = I18nMgr.T("<size=24>The tickets for the machine have been used up,Please contact the administrator</size>"),
                    type = CommonPopupType.OK,
                    buttonText1 = I18nMgr.T("OK"),
                    buttonAutoClose1 = true,
                    callback1 = delegate
                    {
                    },
                    isUseXButton = false,
                });

                EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT,
                    new EventData<string>(GlobalEvent.CoinOutError, "Tickets have been used up"));
            }

            HardwareSubCreditSaveOrder();


            coCoinOutOutTime = null;
        }, 3001));
    }


    void HardwareSubCreditSaveOrder()
    {
        DeviceOrderReship.Instance.DelayReshipOrderRepeat();

        // 订单入库
        JSONNode nodeOrder = cacheCoinOutOrder[orderIdCoinOut];

        //DebugUtils.Log("退票所有订单数据 : " + cacheCoinOutOrder.ToString());

        //DebugUtils.Log("退票订单id : " + orderIdCoinOut);
        //DebugUtil.LogError($"当前退票订单号:{orderIdCoinOut}  退票订单数据 : {nodeOrder.ToString()}");

        long creditCoinOut = (long)nodeOrder["credit_before"] - (long)nodeOrder["credit_after"];

        int coinOutNum = nodeOrder["count"];

        //DebugUtils.Log($"计算出要扣的钱 : {creditCoinOut}  --- 要扣的钱 : {(long)nodeOrder["credit_coin_out"]}");

        // 算法卡退钱扣钱
        MachineDataManager02.Instance.RequestCoinOut(coinOutNum, (Action<object>)((res) =>
        {
            long creditBefore = SBoxModel.Instance.myCredit;
            SBoxModel.Instance.myCredit -= creditCoinOut;
            //DebugUtils.LogError($"减去积分{orderIdCoinOut}: {creditBefore} - {creditCoinOut} = {_consoleBB.Instance.myCredit}");
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            // 删除缓冲订单
            cacheCoinOutOrder.Remove(orderIdCoinOut);
            SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_OUT_ORDER, cacheCoinOutOrder.ToString());


            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
                new TableCoinInOutRecordItem()
                {
                    device_type = nodeOrder["type"],
                    device_number = nodeOrder["device_number"],
                    order_id = nodeOrder["order_id"],
                    count = nodeOrder["count"],
                    credit = creditCoinOut,
                    credit_after = nodeOrder["credit_after"],
                    credit_before = nodeOrder["credit_before"],
                    in_out = 0,
                    created_at = nodeOrder["timestamp"],
                });
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);


            //每日统计
            TableBusniessDayRecordAsyncManager.Instance.AddTotalCoinOut(creditCoinOut, nodeOrder["credit_after"]);

            //复位参数
            InitParam();

            // 退票完成事件
            EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_COIN_IN_OUT_EVENT, new EventData<long>(GlobalEvent.CoinOutSuccess, creditCoinOut));
        }));
    }


    void ShowCoinOutErrorTip() =>
    TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("<size=24>The refund ticket has been damaged. Please contact the administrator.</size>"));



    /// <summary>
    /// 订单补发
    /// </summary>
    public IEnumerator ReshipOrde()
    {

        bool isNext = false;

        JSONNode cacheOrders = JSONNode.Parse(cacheCoinOutOrder.ToString());
        foreach (KeyValuePair<string, JSONNode> item in cacheOrders)
        {
            JSONNode order = item.Value;
            string orderId = item.Key;
            long creditCoinOut = order["credit_coin_out"];
            int coinOutNum = order["count"];

            long creditBefore = SBoxModel.Instance.myCredit;
            long creditAfter = SBoxModel.Instance.myCredit - creditCoinOut;

            if (creditAfter < 0)
            {
                DebugUtils.LogError($"【OrderReship - CoinOut】 点单补发失败； creditAfter = {creditAfter}  order = {order.ToString()}");
                continue;
            }

            //DebugUtil.LogError($"【OrderReship - CoinOut】：orderId = {orderId}  creditCoinOut = {creditCoinOut}  coinOutNum = {coinOutNum}");

            MachineDataManager02.Instance.RequestCoinOut(coinOutNum, (Action<object>)((res) =>
            {
                // 删除缓冲订单
                cacheCoinOutOrder.Remove(orderId);
                SQLitePlayerPrefs03.Instance.SetString(DEVICE_COIN_OUT_ORDER, cacheCoinOutOrder.ToString());


                // 订单入库
                string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                    ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
                    new TableCoinInOutRecordItem()
                    {
                        device_type = order["type"],
                        device_number = order["device_number"],
                        order_id = order["order_id"],
                        count = coinOutNum,
                        credit = creditCoinOut,
                        credit_after = creditAfter,
                        credit_before = creditBefore,
                        in_out = 0,
                        created_at = order["timestamp"],
                    });
                SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);


                SBoxModel.Instance.myCredit = creditAfter;
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

                TableBusniessDayRecordAsyncManager.Instance.AddTotalCoinOut(creditCoinOut, creditAfter);

                isNext = true;
            }));

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
        }
    }


}
