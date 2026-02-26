#define SQLITE_ASYNC
using System.Collections.Generic;
using UnityEngine;
using System;
using SBoxApi;
using SimpleJSON;
using GameMaker;
using Sirenix.OdinInspector;
using System.Collections;


/// <summary>
/// 打印机
/// </summary>
/// <remarks>
/// * 打印机使用退票比例。
/// * 打印机打印多少数值，等于退多少张票。
/// * 打印的数值必须是整数，整数张票。
/// </remarks>
public partial class DevicePrinterOut : MonoSingleton<DevicePrinterOut>
{

    const string DEVICE_PRINTER_OUT_ORDER = "device_printer_out_order";

    const string MARK_POP_PRINTER_NOT_LINK = "MARK_POP_PRINTER_NOT_LINK";
    //int deviceNumber = 0;




    JSONNode _cachePrinterOutOrder = null;
    JSONNode cachePrinterOutOrder
    {
        get
        {
            if (_cachePrinterOutOrder == null)
                _cachePrinterOutOrder = JSONNode.Parse(SQLitePlayerPrefs03.Instance.GetString(DEVICE_PRINTER_OUT_ORDER, "{}"));
            return _cachePrinterOutOrder;
        }
        //set => _cachePrinterOutOrder = value;
    }

    private void OnEnable()
    {
        // if (!ApplicationSettings.Instance.isMachine) return;

        //if (!ApplicationSettings.Instance.isMock)
        //{
        //    DebugUtils.LogError("非mock模式下，不使用打印机");
        //    return;
        //}

        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_FONTSIZE, OnPrinterFontsize);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_MESSAGE, OnPrinterMessage);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_DATESET, OnPrinterDateSet);
        EventCenter.Instance.AddEventListener<SBoxDate>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_DATEGET, OnPrinterDateGet);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_PAPERCUT, OnPrinterCutPaper);

        RepeatInitPrinter();
    }
    private void OnDisable()
    {
        MachineDataManager02.Instance?.RemoveRequestAt(RPC_MARK_DEVICE_PRINTER_OUT);


        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_FONTSIZE, OnPrinterFontsize);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_MESSAGE, OnPrinterMessage);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_DATESET, OnPrinterDateSet);
        EventCenter.Instance.RemoveEventListener<SBoxDate>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_DATEGET, OnPrinterDateGet);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_PAPERCUT, OnPrinterCutPaper);
    }

    const string RPC_MARK_DEVICE_PRINTER_OUT = "RPC_MARK_DEVICE_PRINTER_OUT";




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


    Coroutine coInitPrinter;
    Coroutine coCheckPrinterConnect;
    Coroutine coIsPrinterOuting;
    /// <summary>
    /// 重复复位打印机，直到复位成功
    /// </summary>
    void RepeatInitPrinter()
    {
        if (coInitPrinter != null)
            StopCoroutine(coInitPrinter);

        coInitPrinter = StartCoroutine(RepeatTask(() => InitPrinter(null, null), 8000));

    }

    void FirstOrRepeatInitPrinter()
    {
        InitPrinter(); //上电里面初始化（失败时，重复请求）
        RepeatInitPrinter();
    }

    public void InitPrinter(Action successCallback = null, Action<string> errorCallback = null)
    {

        SBoxModel.Instance.IsConnectPrinter = false;
        SBoxModel.Instance.isInitPrinter = false;
        ClearCo(coCheckPrinterConnect);

        MachineDataManager02.Instance.RequestGetPrinterList(   // 获取列表
            (res) => {
                List<string> printerList = (List<string>)res;

                SBoxModel.Instance.printerList = printerList;

                if (SBoxModel.Instance.selectPrinterNumber > printerList.Count - 1)
                    SBoxModel.Instance.selectPrinterNumber = 0;


                if (!SBoxModel.Instance.isUsePrinter)
                {
                    ClearCo(coInitPrinter);
                    return;
                }


                MachineDataManager02.Instance.RequestSelectPrinter(SBoxModel.Instance.selectPrinterNumber, (res) => {

                    bool isOk = (int)res == 0;
                    if (isOk)
                    {

                        MachineDataManager02.Instance.RequestResetPrinter((res) => {
                            if ((int)res == 0)
                            {
                                DebugUtils.LogWarning($"【printer】: 打印机初始化成功，选择 idx: {SBoxModel.Instance.selectPrinterNumber} -- {SBoxModel.Instance.printerList[SBoxModel.Instance.selectPrinterNumber]}");
                                ClearCo(coInitPrinter);
                                SBoxModel.Instance.isInitPrinter = true;

                                CheckPrinterConnect();

                                successCallback?.Invoke();
                            }
                            else
                            {
                                DebugUtils.LogWarning($"【printer】: 打印机初始化失败,选择 idx: {SBoxModel.Instance.selectPrinterNumber} -- {SBoxModel.Instance.printerList[SBoxModel.Instance.selectPrinterNumber]}");
                                errorCallback?.Invoke("打印机初始化失败");
                            }
                        }, (err) => {

                            DebugUtils.LogError(err.msg);
                            errorCallback?.Invoke(err.msg);

                        }, RPC_MARK_DEVICE_PRINTER_OUT);
                    }
                    else
                    {
                        DebugUtils.LogError("打印机选择失败");
                        errorCallback?.Invoke("打印机选择失败");
                    }

                }, (err) => {
                    DebugUtils.LogError(err.msg);
                    errorCallback?.Invoke(err.msg);
                }, RPC_MARK_DEVICE_PRINTER_OUT);
            },
            (err) =>
            {
                DebugUtils.LogError(err.msg);
                errorCallback?.Invoke(err.msg);
            }, RPC_MARK_DEVICE_PRINTER_OUT
        );
    }

    void CheckPrinterConnect()
    {
        _CheckPrinterConnect();

        if(coCheckPrinterConnect != null)
            StopCoroutine(coCheckPrinterConnect);

        coCheckPrinterConnect = StartCoroutine(RepeatTask(() => _CheckPrinterConnect(), 8000));
    }
    void _CheckPrinterConnect()
    {
        MachineDataManager02.Instance.RequestIsPrinterConnect((res) =>
        {
            int data = (int)res;
            SBoxModel.Instance.IsConnectPrinter = data == SBoxModel.Instance.selectPrinterNumber;
            //Debug.LogError($"算法卡 打印机编号{data} ， 已选打印机编号： {SBoxModel.Instance.selectPrinterNumber}");

            if (!SBoxModel.Instance.IsConnectPrinter)
            {
                if (PageManager.Instance.IndexOf(PageName.ConsolePageConsoleMain) == -1
                    && SBoxModel.Instance.isMachineActive == true)
                {
                    //TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Printer not linked."));
                    CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                    {
                        text = I18nMgr.T("Printer not linked."),
                        type = CommonPopupType.OK,
                        buttonText1 = I18nMgr.T("OK"),
                        buttonAutoClose1 = true,
                        isUseXButton = false,
                        mark = MARK_POP_PRINTER_NOT_LINK,
                    });
                    //Debug.LogError("打印机没有连接");
                }
            }
            else
            {
                //Debug.LogWarning("打印机已连接");
                CommonPopupHandler.Instance.ClosePopup(MARK_POP_PRINTER_NOT_LINK);
            }

        });
    }



    /// <summary>
    /// 设置字体大小回调
    /// </summary>
    /// <param name="result"></param>
    void OnPrinterFontsize(int result)
    {
        if (result == 0)
        {
            if (printerTaskFunc != null)
                printerTaskFunc();
        }
        else
        {
            DebugUtils.LogWarning("【printer】: 打印机字体设置失败");
            printerTaskFunc = null;
        }
    }


    /// <summary>
    /// 打印内容回调
    /// </summary>
    /// <param name="result"></param>
    void OnPrinterMessage(int result)
    {
        if (result == 0)
        {
            if (printerTaskFunc != null && !string.IsNullOrEmpty(orderIdPrinterOut))
            {
                DebugUtils.LogWarning(" 延时检查打印机是否缺纸？");
                //延时检查是否缺纸
                //检查是否缺纸： 5秒内检查，但是不能立马检查

                if (coCheckPrinterConnect != null)
                    StopCoroutine(coCheckPrinterConnect);

                coCheckPrinterConnect = StartCoroutine(DelayTask(() =>
                {
                    coCheckPrinterConnect = null;


                    int printerState = SBoxSandbox.PrinterState();
                    DebugUtils.LogWarning($"PrinterState = {printerState}");
                    bool isPriniterConnect = printerState >= 0;
                    if (!isPriniterConnect)
                    {
                        //打印机异常
                        cachePrinterOutOrder[orderIdPrinterOut]["code"] = -1; //#seaweed#待完成 Code.DEVICE_PRINTER_OUT_OF_PAGE;
                        cachePrinterOutOrder[orderIdPrinterOut]["msg"] = "printer out of paper";
                        SQLitePlayerPrefs03.Instance.SetString(DEVICE_PRINTER_OUT_ORDER, cachePrinterOutOrder.ToString());

                        DebugUtils.LogError(" 打印机缺纸！");
                        CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                        {
                            text = I18nMgr.T("<size=24>Printer out of paper</size>"),
                            type = CommonPopupType.OK,
                            buttonText1 = I18nMgr.T("OK"),
                            buttonAutoClose1 = true,
                            callback1 = delegate {
                            },
                            isUseXButton = false,
                        });
                        return;
                    }
                    else
                    {
                        DebugUtils.LogWarning(" 打印机有纸");
                        printerTaskFunc();
                    }

                }, 3000));


            }
            else
            {
                DebugUtils.LogWarning("【printer】打印机被误触发了");
            }
        }
        else
        {
            DebugUtils.LogWarning("【printer】: 打印机打印失败");
            printerTaskFunc = null;
        }
    }

    void OnPrinterDateSet(int result)
    {
        if (result == 0)
        {
            DebugUtils.Log("【printer】: set Date succeed");
        }
    }
    void OnPrinterDateGet(SBoxDate sBoxDate)
    {
        if (sBoxDate.result == 0)
        {
        }
    }
    void OnPrinterCutPaper(int result)
    {
        if (result == 0)
        {
            DebugUtils.Log("【printer】: cut paper succeed");
        }
    }


    Action printerTaskFunc = null;
    string orderIdPrinterOut = "";

    [Button]
    public void DoPrinterOut()
    {
        if (!SBoxModel.Instance.isMachineActive)
        {
            DebugUtils.LogWarning("Machine not activated");
            return;
        }


        if (SBoxModel.Instance.isUsePrinter == false)
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Printer function not enabled."));
            return;
        }

        if ( MainModel.Instance.isSpin)
        {
            TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("<size=24>Cannot print out during the game period</size>"));
            return;
        }


        if (coIsPrinterOuting != null)
            return;


        coIsPrinterOuting = StartCoroutine(DelayTask(() =>
        {
            coIsPrinterOuting = null;
        }, 10000)); //延时避免重复触发



        printerTaskFunc = CreatPrinterTask();
        printerTaskFunc();
    }


    Action CreatPrinterTask()
    {
        int _next = -1;
        Action FUNC = null;

        FUNC = () => {

            if (!SBoxModel.Instance.isInitPrinter)
            {
                DebugUtils.LogError(" 打印机初始化失败");

                CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                {
                    text = I18nMgr.T("<size=24>Printer is not init</size>"),
                    type = CommonPopupType.OK,
                    buttonText1 = I18nMgr.T("OK"),
                    buttonAutoClose1 = true,
                    callback1 = delegate {
                    },
                    isUseXButton = false,
                });
                return;
            }

            bool isPriniterConnect = SBoxSandbox.PrinterState() >= 0;
            if (!isPriniterConnect)
            {
                DebugUtils.LogError(" 打印机不在线");

                CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                {
                    text = I18nMgr.T("<size=24>Printer not connected</size>"),
                    type = CommonPopupType.OK,
                    buttonText1 = I18nMgr.T("OK"),
                    buttonAutoClose1 = true,
                    callback1 = delegate {
                    },
                    isUseXButton = false,
                });
                return;
            }

            _next++;

            switch (_next)
            {
                case 0: //判断是否可以出钞
                    {
                        if (false)
                        {

                        }
                        else  // 可以出钞票
                        {
                            FUNC.Invoke();
                        }
                    }
                    break;
                case 1: //设置字体大小
                    {
                        int fontSize = 3;
                        SBoxSandbox.PrinterFontSize(fontSize);
                    }
                    break;
                case 2: //获取console数据并出票
                    {
                        // 获取console数据
                        //long mycredit = 1001;
                        //double money = mycredit / 100f;

                        //double money = ConsoleBlackboard02.Instance.myCredit / (float)ConsoleBlackboard02.Instance.printOutScale;

                        int count = DeviceUtils.GetCoinOutNum();
                        long credit = DeviceUtils.GetCoinOutCredit(count);


                        orderIdPrinterOut = OrderIdCreater.Instance.CreatOrderId(OrderIdCreater.CoinOut);
                        //orderIdPrinterOut =  Guid.NewGuid().ToString();

                        string agentName = "1";
                        string businessName = "2";

                        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        // 使用DateTimeOffset将时间戳转换为DateTimeOffset对象
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                        DateTime localDateTime = dateTimeOffset.LocalDateTime;
                        string time = localDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        // 打印人类可读的格式
                        //DebugUtil.Log("UTC日期和时间: " + utcDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        //DebugUtil.Log("本地日期和时间: " + localDateTime.ToString("yyyy-MM-dd HH:mm:ss"));



                        //long crefitPrint = SBoxModel.Instance.myCredit;
                        //保存订单到缓存
                        JSONNode nodeOrder = JSONNode.Parse("{}");
                        nodeOrder["type"] = "printer_out";
                        nodeOrder["order_id"] = orderIdPrinterOut;
                        nodeOrder["money"] = count;
                        nodeOrder["agent_name"] = agentName;
                        nodeOrder["business_name"] = businessName;
                        nodeOrder["timestamp"] = timestamp;
                        nodeOrder["credit_before"] = SBoxModel.Instance.myCredit;
                        nodeOrder["credit_after"] = SBoxModel.Instance.myCredit - credit;
                        nodeOrder["credit"] = credit;
                        //nodeOrder["count"] = 1;
                        nodeOrder["count"] = count;
                        nodeOrder["device_number"] = SBoxModel.Instance.selectPrinterNumber;
                        nodeOrder["code"] = -1; //#seaweed#待完善 Code.DEVICE_CREAT_ORDER_NUMBER;
                        nodeOrder["msg"] = "";

                        cachePrinterOutOrder[orderIdPrinterOut] = nodeOrder;
                        SQLitePlayerPrefs03.Instance.SetString(DEVICE_PRINTER_OUT_ORDER, cachePrinterOutOrder.ToString());


                        //订单内容
                        string printTitle = $"\t\t{ApplicationSettings.Instance.gameTheme}\r\n";
                        string testMsg = printTitle +   // 平台号
                                                        //$"Credit: {credit}\r\n" +  // 金钱
                            $"Tickets: {count}\r\n" +
                            $"Order number: \r\n" +
                            $"{orderIdPrinterOut}\r\n" +  //订单号
                            $"Distributor: {agentName}\r\n" +  //代理商
                            $"Business: {businessName}\r\n " + // 厂家
                            $"time: {time}\r\n " +  //时间
                            $"";


                        //检查到打印失败？？
                        /*DoCor(COR_REPEAT_CHECK_PRINTER_PAGE, DoTaskRepeat(() =>
                        {
                            DebugUtil.Log($"轮训检查  PrinterState = {SBoxSandbox.PrinterState()}");
                        }, 300));*/

                        // 开始打印
                        SBoxSandbox.PrinterMessage(testMsg);

                        DebugUtils.Log($"@【printer】打印内容 testMsg = {testMsg}");
                    }
                    break;
                case 3: // 打印成功后，记录订单，并删除订单缓冲
                    {
                        DebugUtils.Log($"@【printer】将打印数据给算法卡");
                        //删除订单到缓存
                        JSONNode nodeOrder = cachePrinterOutOrder[orderIdPrinterOut];

                        long creditCoinOut = (long)nodeOrder["credit"];
                        int coinOutNum = (int)nodeOrder["count"];

                        MachineDataManager02.Instance.RequestCoinOut(coinOutNum, (Action<object>)((res) =>
                        {


                            //删除订单到缓存
                            cachePrinterOutOrder.Remove(orderIdPrinterOut);
                            SQLitePlayerPrefs03.Instance.SetString(DEVICE_PRINTER_OUT_ORDER, cachePrinterOutOrder.ToString());

                            //记录订单
                            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableCoinInOutRecordItem>(
                             ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
                             new TableCoinInOutRecordItem()
                             {
                                 device_type = nodeOrder["type"],
                                 device_number = nodeOrder["device_number"],
                                 order_id = nodeOrder["order_id"],
                                 count = nodeOrder["count"],
                                 as_money = nodeOrder["money"],
                                 credit = (long)nodeOrder["credit"],
                                 credit_after = nodeOrder["credit_after"],
                                 credit_before = nodeOrder["credit_before"],
                                 in_out = 0,
                                 created_at = nodeOrder["timestamp"],
                             });
                            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

                            //每日统计
                            TableBusniessDayRecordAsyncManager.Instance.AddTotalCoinOut(nodeOrder["credit"], nodeOrder["credit_after"]);


                            //同步玩家金币
                            SBoxModel.Instance.myCredit -= (long)nodeOrder["credit"];
                            MainBlackboardController.Instance.AutoSyncMyCreditToReel();

                            DebugUtils.Log($"玩家真实金额： {SBoxModel.Instance.myCredit}");
                            orderIdPrinterOut = null;
                            printerTaskFunc = null;

                        }));

                        /*
                        //删除订单到缓存
                        JSONNode nodeOrder = null;
                        if (cachePrinterOutOrder.HasKey(orderIdPrinterOut))
                        {
                            nodeOrder = cachePrinterOutOrder[orderIdPrinterOut];
                            cachePrinterOutOrder.Remove(orderIdPrinterOut);
                            SQLitePlayerPrefs02.Instance.SetString(DEVICE_PRINTER_OUT_ORDER, cachePrinterOutOrder.ToString());
                        }*/

                    }
                    break;
                default:
                    orderIdPrinterOut = null;
                    printerTaskFunc = null;
                    break;
            }

        };

        return FUNC;
    }





    Action TestCreatPrinterTask()
    {
        int _next = -1;
        Action FUNC = null;

        FUNC = () => {


            bool isPriniterConnect = SBoxSandbox.PrinterState() >= 0;
            if (!isPriniterConnect)
            {
                DebugUtils.LogError(" 打印机不在线");

                CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                {
                    text = I18nMgr.T("<size=24>Printer not connected</size>"),
                    type = CommonPopupType.OK,
                    buttonText1 = I18nMgr.T("OK"),
                    buttonAutoClose1 = true,
                    callback1 = delegate {
                    },
                    isUseXButton = false,
                });
                return;
            }

            _next++;

            switch (_next)
            {

                case 0: //设置字体大小
                    {
                        int fontSize = 3;
                        SBoxSandbox.PrinterFontSize(fontSize);
                    }
                    break;
                case 1: //获取console数据并出票
                    {


                        DateTime localDateTime = DateTimeOffset.UtcNow.LocalDateTime;
                        string time = localDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        string testMsg =   // 平台号
                            $"测试打印机\r\n" +  // 金钱
                            $"time: {time}\r\n " +  //时间
                            $"";

                        // 开始打印
                        SBoxSandbox.PrinterMessage(testMsg);

                        DebugUtils.Log($"@【printer】打印内容 testMsg = {testMsg}");
                    }
                    break;
                case 2: // 打印成功后，记录订单，并删除订单缓冲
                    {
                        printerTaskFunc = null;
                    }
                    break;
                default:
                    printerTaskFunc = null;
                    break;
            }

        };

        return FUNC;
    }




}




