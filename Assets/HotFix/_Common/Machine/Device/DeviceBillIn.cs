using SBoxApi;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceBillIn : MonoSingleton<DeviceBillIn>
{


    const string DEVICE_Bill_IN_ORDER = "device_bill_in_order";
    const string MARK_POP_BILLER_NOT_LINK = "MARK_POP_BILLER_NOT_LINK";

    JSONNode _cacheBillInOrder = null;
    string orderIdBillIn;
    JSONNode cacheBillInOrder
    {
        get
        {
            if (_cacheBillInOrder == null)
                _cacheBillInOrder = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DEVICE_Bill_IN_ORDER, "{}"));
            return _cacheBillInOrder;
        }
        //set => _cacheBillInOrder = value;
    }

    private void OnEnable()
    {
        //if (!ApplicationSettings.Instance.isMachine)  return;
        //if (!ApplicationSettings.Instance.isMock)
        //{
        //    DebugUtils.LogError("非mock模式下，不使用打印机");
        //    return;
        //}

        EventCenter.Instance.AddEventListener<int>(SBoxSanboxEventHandle.BILL_IN, OnHardwareBillIn);
        EventCenter.Instance.AddEventListener(SBoxSanboxEventHandle.BILL_STACKED, OnHardwareBillStacked);

        RepeatInitBiller();
    }
    private void OnDisable()
    {
        MachineDataManager02.Instance?.RemoveRequestAt(RPC_MARK_DEVICE_BILLER_IN);

        EventCenter.Instance.RemoveEventListener<int>(SBoxSanboxEventHandle.BILL_IN, OnHardwareBillIn);
        EventCenter.Instance.RemoveEventListener(SBoxSanboxEventHandle.BILL_STACKED, OnHardwareBillStacked);

    }

    IEnumerator DelayTask(Action task, int timeMS)
    {
        yield return new WaitForSeconds((float)timeMS / 1000f);
        task?.Invoke();
    }

    protected IEnumerator RepeatTask(Action task, int timeMS)
    {
        while (true)
        {
            yield return new WaitForSeconds((float)timeMS / 1000f);
            task?.Invoke();
        }
    }
    void ClearCo(Coroutine co)
    {
        if (co != null)
            StopCoroutine(co);
        co = null;
    }

    Coroutine coInitBill;
    Coroutine coCheckBillerConnect;


    /// <summary>
    /// 重复复位打印机，直到复位成功
    /// </summary>
    void RepeatInitBiller()
    {
        if(coInitBill != null) 
            StopCoroutine(coInitBill);

        coInitBill = StartCoroutine(RepeatTask(() => InitBiller(null, null), 5000));
    }

    void FirstOrRepeatInitBiller()
    {
        InitBiller();
        RepeatInitBiller();
    }
    const string RPC_MARK_DEVICE_BILLER_IN = "RPC_MARK_DEVICE_BILLER_IN";
    public void InitBiller(Action successCallback = null, Action<string> errorCallback = null)
    {
        SBoxModel.Instance.isInitBiller = false;
        SBoxModel.Instance.IsConnectBiller = false;

        ClearCo(coCheckBillerConnect);


        MachineDataManager02.Instance.RequestGetBillerList(
            (res) => {
                List<string> billerList = (List<string>)res;

                SBoxModel.Instance.billerList = billerList;

                if (SBoxModel.Instance.selectBillerNumber > billerList.Count - 1)
                    SBoxModel.Instance.selectBillerNumber = 0;


                if (!SBoxModel.Instance.isUseBiller)
                {
                    ClearCo(coInitBill);
                    return;
                }



                MachineDataManager02.Instance.RequestSelectBiller(SBoxModel.Instance.selectBillerNumber, (res) => {

                    bool isOk = (int)res == 0;
                    if (isOk)
                    {
                        DebugUtils.LogWarning("【biller】: 纸钞机初始化成功");
                        ClearCo(coInitBill);
                        SBoxModel.Instance.isInitBiller = true;

                        CheckIsBillerConnect();

                        successCallback?.Invoke();
                    }
                    else
                    {
                        DebugUtils.LogWarning("【biller】: 纸钞机初始化失败");
                        errorCallback?.Invoke("纸钞机初始化失败");
                    }

                }, (err) => {
                    DebugUtils.LogError(err.msg);
                    errorCallback?.Invoke(err.msg);
                }, RPC_MARK_DEVICE_BILLER_IN);
            },
            (err) =>
            {
                DebugUtils.LogError(err.msg);
                errorCallback?.Invoke(err.msg);
            }, RPC_MARK_DEVICE_BILLER_IN
        );
    }


    void CheckIsBillerConnect()
    {
        _CheckIsBillerConnect();

        if(coCheckBillerConnect != null)
            StopCoroutine(coCheckBillerConnect);
        coCheckBillerConnect = StartCoroutine(RepeatTask(()=> _CheckIsBillerConnect(),8000));
    }


    void _CheckIsBillerConnect()
    {
        MachineDataManager02.Instance.RequestIsBillerConnect((res) =>
        {
            int data = (int)res;
            SBoxModel.Instance.IsConnectBiller = data == SBoxModel.Instance.selectBillerNumber;
            //Debug.LogError($"算法卡 纸钞机编号{data} ， 已选纸钞机编号： {SBoxModel.Instance.selectBillerNumber}");

            if (!SBoxModel.Instance.IsConnectBiller)
            {
                if (
                PageManager.Instance.IndexOf(PageName.ConsolePageConsoleMain) == -1
                && SBoxModel.Instance.isMachineActive == true)
                {
                    //TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Biller not linked."));
                    CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                    {
                        text = I18nMgr.T("Biller not linked."),
                        type = CommonPopupType.OK,
                        buttonText1 = I18nMgr.T("OK"),
                        buttonAutoClose1 = true,
                        isUseXButton = false,
                        mark = MARK_POP_BILLER_NOT_LINK,
                    });
                    //Debug.LogError("纸钞机没有连接");
                }

            }
            else
            {
                //Debug.LogWarning("纸钞机已连接");
                CommonPopupHandler.Instance.ClosePopup(MARK_POP_BILLER_NOT_LINK);
            }
        }, RPC_MARK_DEVICE_BILLER_IN);
    }




    void RejectBill()
    {
        if (Application.isEditor)
        {
            money = 0;
            MatchDebugManager.Instance.SendUdpMessage(SBoxEventHandle.SBOX_SADNBOX_BILL_REJECT);
        }
        else
        {
            SBoxSandbox.BillReject();
        }
    }



    long cashSeq;
    int money;


    /// <summary>
    /// 收到钞票
    /// </summary>
    /// <param name="mny"></param>
    private void OnHardwareBillIn(int mny)
    {
        if (DeviceUtils.IsCurSasBiller())
            return;

        Debug.LogWarning($"OnBillIn  money: {mny}");

        // 普通打印机
        if (mny <= 0)
            return;

        if (!SBoxModel.Instance.isMachineActive)
        {
            RejectBill();
            DebugUtils.LogWarning("Machine not activated");
            return;
        }

        if (MainModel.Instance.isSpin)
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("<size=26>Cannot recharge during the game period</size>"));
            RejectBill();
            return;
        }

#if false
        if (PlayerPrefsUtils.isUseSas)
        {

            MachineDataManager02.Instance.RequestSasCashSeqScoreUp((res) =>
            {
                int[] data = res as int[];

                if (data[0] == 0)
                {
                    cashSeq = ((long)data[1] << 32) | (uint)data[2];

                    SasCommand.Instance.PushGeneralBillInDetails(mny, cashSeq);
                    SasCommand.Instance.SetMeterBillInCash(mny, cashSeq);

                    DoReceiveCash(mny);
                }
                else
                {
                    RejectBill();
                    return;
                }
            });

        }
        else
        {
            DoReceiveCash(mny);
        }
#else
        DoReceiveCash(mny);
#endif

    }


    private void DoReceiveCash(int mny)
    {

        money = mny;

        DebugUtils.LogWarning($"@ 收到金钱 {money}");



        orderIdBillIn = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinIn);

        if (true)  //钞票是否允许收入
        {
            SBoxSandbox.BillApprove();
        }
        else
        {
            RejectBill();
        }
    }






    /// <summary>
    /// 钞票进入钱箱
    /// </summary>
    private void OnHardwareBillStacked()
    {

        if (DeviceUtils.IsCurSasBiller())
            return;


        if (SBoxModel.Instance.isUseBiller == false)
        {
            RejectBill();
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Biller function not enabled."));
            return;
        }


        // 普通进票
        if (string.IsNullOrEmpty(orderIdBillIn))
            return;

        //#seaweed#这个暂时隐藏  if (PlayerPrefsUtils.isUseSas)  SasCommand.Instance.SetMeterBillInCash(money, cashSeq, 100);


        //保存订单到缓存
        JSONNode nodeOrder = JSONNode.Parse("{}");
        nodeOrder["type"] = "bill_in";
        nodeOrder["order_id"] = orderIdBillIn;
        nodeOrder["device_number"] = SBoxModel.Instance.selectBillerNumber;
        nodeOrder["money"] = money;
        nodeOrder["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        nodeOrder["credit_before"] = SBoxModel.Instance.myCredit;


        cacheBillInOrder[orderIdBillIn] = nodeOrder;
        SQLitePlayerPrefs03.Instance.SetString(DEVICE_Bill_IN_ORDER, cacheBillInOrder.ToString());

        RequestBillIn(orderIdBillIn, () =>
        {
            //订单入库

            JSONNode nodeOrder = cacheBillInOrder[orderIdBillIn];

            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
            ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
            new TableCoinInOutRecordItem()
            {
                device_type = nodeOrder["type"],
                device_number = nodeOrder["device_number"],
                order_id = nodeOrder["order_id"],
                count = 1, //nodeOrder["count"],
                credit = (long)nodeOrder["credit"],
                credit_after = nodeOrder["credit_after"],
                credit_before = nodeOrder["credit_before"],
                as_money = nodeOrder["money"],
                in_out = 1,
                created_at = nodeOrder["timestamp"],
            });
            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);


            TableBusniessDayRecordAsyncManager.Instance.AddTotalCoinIn(nodeOrder["credit"], nodeOrder["credit_after"]);

            //删除缓存
            cacheBillInOrder.Remove(orderIdBillIn);

            money = 0;
            orderIdBillIn = null;
        });
    }



    void RequestBillIn(string orderId, Action successCallback)
    {

        long coinInNum = (long)cacheBillInOrder[orderId]["money"];
        long billInCredit = SBoxModel.Instance.BillInScale * coinInNum;

        MachineDataManager02.Instance.RequestCoinIn((int)coinInNum, (res) =>
        {

            SBoxModel.Instance.myCredit += billInCredit;

            MainBlackboardController.Instance.AddOrSyncMyCreditToReal(billInCredit);

            //GlobalSoundHelper.Instance.PlaySound(GameMaker.SoundKey.MachineCoinIn);

            cacheBillInOrder[orderId]["credit"] = billInCredit;
            cacheBillInOrder[orderId]["credit_after"] = SBoxModel.Instance.myCredit;

            successCallback?.Invoke();

        });

    }


}