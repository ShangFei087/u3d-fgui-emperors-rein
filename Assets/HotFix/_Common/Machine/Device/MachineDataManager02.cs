using GameMaker;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 硬件交互接口
/// </summary>
/// <remarks>
/// * 简化MachineDataManager 的封装，让代码更加直观。
/// </remarks>
public partial class MachineDataManager02: ProxyHelper<MachineDataManager02>
{
    bool isMock => ApplicationSettings.Instance.isMock;


    protected void Awake()
    {
        receiveOvertimeS = 1.5f;
        isDebugLog = true;
        prefix = "【SBox】";
    }

    protected void Start()
    {


        //== 打码
        //监听下行:打码信息
        EventCenter.Instance.AddEventListener<SBoxCoderData>(SBoxEventHandle.SBOX_REQUEST_CODER, OnResponseMachineCodingInfo);
        //监听下行:打码
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CODER, OnResponseSetCoding);


        //== 硬件配置
        //监听下行:读取投退币配置信息
        EventCenter.Instance.AddEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnResponseReadConf);
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_WRITE_CONF, OnResponseWriteConf);


        //== 版本
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_IDEA_VERSION, OnResponseGetAlgorithmVersion);
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_SANDBOX_VERSION, OnResponseGetHardwareVersion);

        //== 用户密码
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CHECK_PASSWORD, OnResponseCheckPassword);
        EventCenter.Instance.AddEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CHANGE_PASSWORD, OnResponseChangePassword);



        // ==玩家信息
        EventCenter.Instance.AddEventListener<SBoxAccount>(SBoxEventHandle.SBOX_GET_ACCOUNT, OnResponseGetPlayerInfo);

        // ==游戏彩金
        EventCenter.Instance.AddEventListener<JackpotRes>(SBoxEventHandle.SBOX_JACKPOT_GAME, OnResponseJackpotGame);


        // ==码表
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_METER_SET, OnResponseCounter);


        //// 打印机
        EventCenter.Instance.AddEventListener<List<string>>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, OnResponseGetPrinterList);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, OnResponseSelectPrinter);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_RESET, OnResponseResetPrinter);

        // 纸钞机列表
        EventCenter.Instance.AddEventListener<List<string>>(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, OnResponseGetBillerList);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, OnResponseSelectBiller);
    }


    protected override void OnDestroy()
    {

        //== 打码
        //监听下行:打码信息
        EventCenter.Instance.RemoveEventListener<SBoxCoderData>(SBoxEventHandle.SBOX_REQUEST_CODER, OnResponseMachineCodingInfo);
        //监听下行:打码
        EventCenter.Instance.RemoveEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CODER, OnResponseSetCoding);


        //== 硬件配置
        //监听下行:读取投退币配置信息
        EventCenter.Instance.RemoveEventListener<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, OnResponseReadConf);
        EventCenter.Instance.RemoveEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_WRITE_CONF, OnResponseWriteConf);


        // 版本
        EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_IDEA_VERSION, OnResponseGetAlgorithmVersion);
        EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_SANDBOX_VERSION, OnResponseGetHardwareVersion);


        // 用户密码
        EventCenter.Instance.RemoveEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CHECK_PASSWORD, OnResponseCheckPassword);
        EventCenter.Instance.RemoveEventListener<SBoxPermissionsData>(SBoxEventHandle.SBOX_CHANGE_PASSWORD, OnResponseChangePassword);


        // OnResponseChangePassword
        // ==玩家信息
        EventCenter.Instance.RemoveEventListener<SBoxAccount>(SBoxEventHandle.SBOX_GET_ACCOUNT, OnResponseGetPlayerInfo);


        // ==游戏彩金
        EventCenter.Instance.RemoveEventListener<JackpotRes>(SBoxEventHandle.SBOX_JACKPOT_GAME, OnResponseJackpotGame);


        // ==码表
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_METER_SET, OnResponseCounter);

        ////打印机
        EventCenter.Instance.RemoveEventListener<List<string>>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, OnResponseGetPrinterList);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, OnResponseSelectPrinter);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_PRINTER_RESET, OnResponseResetPrinter);


        // 纸钞机列表
        EventCenter.Instance.RemoveEventListener<List<string>>(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, OnResponseGetBillerList);
        EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, OnResponseSelectBiller);

        base.OnDestroy();
    }







    /// <summary> 是否激活 </summary>
    public int RequestIsCodingActive(Action<object> successCallback,string mark = null)
    {
        int seqId = OnRequestBefore(RpcNameIsCodingActive, null, successCallback, null, mark);

        if (isMock)
        {
            OnMockActive(null);
        }
        else
        {
            int code = SBoxIdea.NeedActivated();
            OnResponseIsCodingActive(code);
        }
        return seqId;
    }
    void OnResponseIsCodingActive(int code) => OnResponse(RpcNameIsCodingActive, code);
    const string RpcNameIsCodingActive = "RpcNameIsCodingActive";




    /// <summary> 显示打码数据 </summary>
    public int RequestMachineCodingInfo(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_REQUEST_CODER, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockCoderInfo(null);
        }
        else
        {
            SBoxIdea.RequestCoder(0);
        }
        return seqId;

    }
    void OnResponseMachineCodingInfo(SBoxCoderData res) => OnResponse(SBoxEventHandle.SBOX_REQUEST_CODER, res);



    /// <summary> 打码 </summary>
    public int RequestSetCoding(ulong code, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {

        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_CODER, code, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockCoder(code);
        }
        else
        {
            SBoxIdea.Coder(0, code);
        }
        return seqId;
    }
    void OnResponseSetCoding(SBoxPermissionsData res) => OnResponse(SBoxEventHandle.SBOX_CODER, res);
    
        



    /// <summary> 密码校验 </summary>
    public int RequestCheckPassword(int password, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_CHECK_PASSWORD, password, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockCheckPassword(password);
        }
        else
        {
            SBoxIdea.CheckPassword(password);
        }

        return seqId;
    }
    void OnResponseCheckPassword(SBoxPermissionsData res) => OnResponse(SBoxEventHandle.SBOX_CHECK_PASSWORD, res);





    /// <summary> 修改密码 </summary>
    public int RequestChangePassword(int password, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_CHANGE_PASSWORD, password, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockChangePassword(password);
        }
        else
        {
            SBoxIdea.ChangePassword(password);
        }

        return seqId;
    }
    void OnResponseChangePassword(SBoxPermissionsData res) => OnResponse(SBoxEventHandle.SBOX_CHANGE_PASSWORD, res);







    /// <summary>
    /// 上分
    /// </summary>
    /// <param name="credit">分数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestScoreUp(int credit, Action<object> successCallback, string mark = null)
    {
        int seqId = OnRequestBefore(RpcNameScoreUp, credit, successCallback, null, mark);

        if (isMock)
        {
            OnMockScoreUp(credit);  
        }
        else
        {
            List<SBoxPlayerCoinInfo> sBoxPlayerCoinInfos = new List<SBoxPlayerCoinInfo>();
            sBoxPlayerCoinInfos.Add(new SBoxPlayerCoinInfo()
            {
                PlayerId = SBoxModel.Instance.pid,
                ScoreUp = credit,
            });

            SBoxIdea.SetPlayerCoinInfo(sBoxPlayerCoinInfos);

            // 自己的回调
            OnResponseScoreUp(credit);
        }

        return seqId;
    }
        //=> severHelper.RequestData(RpcNameScoreUp, credit, successCallback, null);
    void OnResponseScoreUp(int credit) => OnResponse(RpcNameScoreUp, credit);
    const string RpcNameScoreUp = "RpcNameScoreUp";








    /// <summary>
    /// 下分
    /// </summary>
    /// <param name="credit">分数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestScoreDown(int credit, Action<object> successCallback, string mark = null)
    {

        int seqId = OnRequestBefore(RpcNameScoreDown, credit, successCallback, null, mark);

        if (isMock)
        {
            OnMockScoreDown(credit);
        }
        else
        {
            List<SBoxPlayerCoinInfo> sBoxPlayerCoinInfos = new List<SBoxPlayerCoinInfo>();
            sBoxPlayerCoinInfos.Add(new SBoxPlayerCoinInfo()
            {
                PlayerId = SBoxModel.Instance.pid,
                ScoreDown = credit,
            });
            SBoxIdea.SetPlayerCoinInfo(sBoxPlayerCoinInfos);

            // 自己的回调
            OnResponseScoreDown(credit);
        }

        return seqId;
    }
    void OnResponseScoreDown(int credit) => OnResponse(RpcNameScoreDown, credit);
    const string RpcNameScoreDown = "RpcNameScoreDown";





    /// <summary>
    /// 投币
    /// </summary>
    /// <param name="num">个数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestCoinIn(int num, Action<object> successCallback, string mark = null)
    {

        int seqId = OnRequestBefore(RpcNameCoinIn, num, successCallback, null, mark);

        if (isMock)
        {
            OnMockCoinIn(num);
        }
        else
        {
            List<SBoxPlayerCoinInfo> sBoxPlayerCoinInfos = new List<SBoxPlayerCoinInfo>()
            {
                new SBoxPlayerCoinInfo()
                {
                    PlayerId = SBoxModel.Instance.pid,
                    CoinIn = num,
                }
            };
            SBoxIdea.SetPlayerCoinInfo(sBoxPlayerCoinInfos);

            // 自己的回调
            OnResponseCoinIn(num);
        }

        return seqId;
    }
    void OnResponseCoinIn(int num) => OnResponse(RpcNameCoinIn, num);
    const string RpcNameCoinIn = "RpcNameCoinIn";





    /// <summary>
    /// 退票
    /// </summary>
    /// <param name="num">个数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestCoinOut(int coinOutNum, Action<object> successCallback, string mark = null)
    {
        int seqId = OnRequestBefore(RpcNameCoinOut, coinOutNum, successCallback, null, mark);

        if (isMock)
        {
            OnMockCoinOut(coinOutNum);
        }
        else
        {
            List<SBoxPlayerCoinInfo> sBoxPlayerCoinInfos = new List<SBoxPlayerCoinInfo>()
            {
                new SBoxPlayerCoinInfo()
                {
                    PlayerId = SBoxModel.Instance.pid,
                    CoinOut = coinOutNum,
                }
            };

            SBoxIdea.SetPlayerCoinInfo(sBoxPlayerCoinInfos);

            // 自己的回调
            OnResponseCoinOut(coinOutNum);
        }
        return seqId;

    }
    void OnResponseCoinOut(int coinOutNum) => OnResponse(RpcNameCoinOut, coinOutNum);
    const string RpcNameCoinOut = "RpcNameCoinOut";







    /// <summary>
    /// 硬件版本
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    public int RequestGetHardwareVersion(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SANDBOX_VERSION, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockSandboxVersion(null);
        }
        else
        {
            DebugUtils.LogError("待完成");
        }
        return seqId;
    }
    void OnResponseGetHardwareVersion(string res) => OnResponse(SBoxEventHandle.SBOX_SANDBOX_VERSION, res);





    /// <summary>
    /// 算法卡版本
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    public int RequestGetAlgorithmVersion(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_IDEA_VERSION, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockIdeaVersion(null);
        }
        else
        {
            SBoxIdea.Version();
        }
        return seqId;
    }
    void OnResponseGetAlgorithmVersion(string res) => OnResponse(SBoxEventHandle.SBOX_IDEA_VERSION, res);





    /// <summary> 请求配置 </summary>
    public int RequestReadConf(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_READ_CONF, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockReadConf(null);   
        }
        else
        {
            SBoxIdea.ReadConf();
        }
        return seqId;
    }
    void OnResponseReadConf(SBoxConfData res) => OnResponse(SBoxEventHandle.SBOX_READ_CONF, res);





    /// <summary> 修改配置 </summary>
    public int RequestWriteConf(SBoxConfData data, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_WRITE_CONF, data, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockWriteConf(data);
        }
        else
        {
            SBoxIdea.WriteConf(data);
        }
        return seqId;
    }
    void OnResponseWriteConf(SBoxPermissionsData res) => OnResponse(SBoxEventHandle.SBOX_WRITE_CONF, res);




    /// <summary> 获取玩家信息20016 </summary>
    public int RequestGetPlayerInfo(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_GET_ACCOUNT, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockGetAccount(null);
        }
        else
        {
            SBoxIdea.GetAccount();
        }
        return seqId;
    }
    void OnResponseGetPlayerInfo(SBoxAccount res) => OnResponse(SBoxEventHandle.SBOX_GET_ACCOUNT, res);




    /// <summary> 修改玩家信息 </summary>
    public int RequestSetPlayerInfo(SBoxPlayerAccount req, Action<object> successCallback, string mark = null)
    {
        int seqId = OnRequestBefore(RpcNameSetPlayerInfo, null, successCallback, null, mark);

        if (isMock)
        {
            OnMockSetAccount(req);
        }
        else
        {
           
            DebugUtils.LogError("待完成");
        }
        return seqId;
    }
    void OnResponseSetPlayerInfo(SBoxPlayerAccount res) => OnResponse(RpcNameSetPlayerInfo, res);
    const string RpcNameSetPlayerInfo = "RpcNameSetPlayerInfo";





    /// <summary> 获取游戏彩金 </summary>
    public int RequestJackpotGame(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_JACKPOT_GAME, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockJackotGame(null);
        }
        else
        {
            SBoxIdea.GetJpContribution(SBoxModel.Instance.pid);
            DebugUtils.LogError("待完成");
        }
        return seqId;
    }
    void OnResponseJackpotGame(JackpotRes res) => OnResponse(SBoxEventHandle.SBOX_JACKPOT_GAME, res);



    /// <summary> 获取游戏彩金 </summary>
    public int RequestJackpotOnLine()
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_JACKPOT_GAME, null, null, null, null);

        if (isMock)
        {
            OnMockJackpotOnLine(null);
        }
        else
        {   
            DebugUtils.LogError("待完成");
        }
        return seqId;
    }
    public const string RpcNameJackpotOnLine = "RpcNameJackpotOnLine";

    /// <summary>
    /// 码表
    /// </summary>
    /// <param name="id">码表编号,0:投币码表,1:退币码表,2:上分码表,3:下分码表</param>
    /// <param name="counts">码表走数</param>
    /// <param name="type">走数类型,0:无䇅,1:counts为绝对值,2:counts为追加值,3:中止走数,</param>
    /// <param name="successCallback"></param>	
    /// <returns>
    /// result = 0：成功
    /// result 《 0：发送参数错误
    /// result 》 0：状态码(保留)
    /// </returns>
    public int RequestCounter(int id, int counts, int type, Action<object> successCallback, string mark = null)
    {
        Dictionary<string, object> req = new Dictionary<string, object>()
        {
            ["id"] = id,
            ["counts"] = counts,
            ["type"] = type,
        };

        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SADNBOX_METER_SET, req, successCallback, null, mark);

        if (isMock)
        {
            OnMockSetMeter(req);
        }
        else { 
            //码表设置
            SBoxSandbox.MeterSet(id, counts, type);
        }

        return seqId;
    }
    void OnResponseCounter(int res) => OnResponse(SBoxEventHandle.SBOX_SADNBOX_METER_SET, res);








    public int RequestGetBillerList(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {

        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockGetBillList(null);
        }
        else
        {
            SBoxSandbox.BillListGet();
        }

        return seqId;
    }
    void OnResponseGetBillerList(List<string> res) => OnResponse(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, res);






    public int RequestSelectBiller(int index, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {

        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, index, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockSelectBill(index);
        }
        else
        {
            SBoxSandbox.BillSelect(index);
        }

        return seqId;
    }
    void OnResponseSelectBiller(int res) => OnResponse(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, res);





    /// <summary>
    /// 纸钞机是否链接
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <returns></returns>
    public int RequestIsBillerConnect(Action<object> successCallback, string mark = null)
    {
        int seqId = OnRequestBefore(RpcNameIsBillerConnect, null, successCallback, null, mark);

        if (isMock)
        {
            OnResponseIsBillerConnect(0);
        }
        else
        {
            OnResponseIsBillerConnect(SBoxSandbox.BillState());
        }

        return seqId;
    }
    void OnResponseIsBillerConnect(int res) => OnResponse(RpcNameIsBillerConnect, res);
    const string RpcNameIsBillerConnect = "RpcNameIsBillerConnect";





    /// <summary>
    /// 获取打印机列表
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <returns></returns>
    public int RequestGetPrinterList(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockGetPrintList(null);
        }
        else
        {
            SBoxSandbox.PrinterListGet();
        }

        return seqId;
    }
    void OnResponseGetPrinterList(List<string> res) => OnResponse(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, res);







    /// <summary>
    /// 选择打印机
    /// </summary>
    /// <param name="index"></param>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <returns></returns>
    public int RequestSelectPrinter(int index, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, index, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockSelectPrinter(null);
        }
        else
        {
            SBoxSandbox.PrinterSelect(index);
        }

        return seqId;
    }
    void OnResponseSelectPrinter(int res) => OnResponse(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, res);




    /// <summary>
    /// 复位打印机
    /// </summary>
    /// <param name="index"></param>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <returns></returns>
    public int RequestResetPrinter(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_SADNBOX_PRINTER_RESET, null, successCallback, errorCallback, mark);

        if (isMock)
        {
            OnMockResetPrinter(null);
        }
        else
        {
            SBoxSandbox.PrinterReset();
        }

        return seqId;
    }

    void OnResponseResetPrinter(int res) => OnResponse(SBoxEventHandle.SBOX_SADNBOX_PRINTER_RESET, res);



    /// <summary>
    /// 打印机是否链接
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <returns></returns>
    public int RequestIsPrinterConnect(Action<object> successCallback, string mark = null)
    {
        int seqId = OnRequestBefore(RpcNameIsPrinterConnect, null, successCallback, null, mark);

        if (isMock)
        {
            OnResponseIsPrinterConnect(0);
        }
        else
        {
            OnResponseIsPrinterConnect(SBoxSandbox.PrinterState());
        }

        return seqId;
    }
    void OnResponseIsPrinterConnect(int res) => OnResponse(RpcNameIsPrinterConnect, res);
    const string RpcNameIsPrinterConnect = nameof(RpcNameIsPrinterConnect);







}
