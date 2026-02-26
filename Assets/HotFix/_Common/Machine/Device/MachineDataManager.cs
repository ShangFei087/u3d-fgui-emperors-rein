using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SBoxApi;
using Newtonsoft.Json;
using GameMaker;
using SimpleJSON;


/// <summary>
/// 【即将废弃使用】
/// </summary>
public partial class MachineDataManager : MonoBehaviour //:MonoSingleton<MachineDataManager>  
{

    private static object _mutex = new object();
    static MachineDataManager _instance;

    public static MachineDataManager Instance
    {
        get
        {

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MachineDataManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        _instance = obj.AddComponent<MachineDataManager>();

                        obj.name = _instance.GetType().Name;
                        if (obj.transform.parent == null)
                        {
                            DontDestroyOnLoad(obj);
                        }
                    }
                }
                return _instance;
            }
        }
    }







    protected SeverHelper severHelper;

    protected virtual void Awake()
    {
        severHelper = new SeverHelper()
        {
            receiveOvertimeMS = 1000,
            requestFunc = requestFunc,
            isDebug = true,
            prefix = "【SBox】",
        };
    }
    protected virtual void Start()
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

    }

    protected virtual void OnDestroy()
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

    }

    protected void Update()
    {
        severHelper.Update();
    }


    /// <summary> 获取打码数据 </summary>
    public const string RpcNameIsCodingActive = "RpcNameIsCodingActive";

    public const string RpcNameSetPlayerInfo = "RpcNameSetPlayerInfo";

    public const string RpcNameClearCodingActive = "RpcNameClearCodingActive";

    public const string RpcNameScoreUp = "RpcNameScoreUp";
    public const string RpcNameScoreDown = "RpcNameScoreDown";

    public const string RpcNameCoinIn = "RpcNameCoinIn";
    public const string RpcNameCoinOut = "RpcNameCoinOut";


    public const string RpcNameIsPrinterConnect = "RpcNameIsPrinterConnect";
    public const string RpcNameIsBillerConnect = "RpcNameIsBillerConnect";


    public const string RpcNameJackpotOnLine = "RpcNameJackpotOnLine";

    /*
    /// <summary> 掉币倒计时 </summary>
    public const string RpcNameCoinCountDown = "RpcNameCoinCountDown";

    public const string RpcNameGetMyCredit = "RpcNameGetMyCredit";
    
    */
    /// <summary> 是否激活 </summary>
    public int RequestIsCodingActive(Action<object> successCallback) =>
        severHelper.RequestData(RpcNameIsCodingActive, null, successCallback, null);
    void OnResponseIsCodingActive(int code) => severHelper.OnSuccessResponseData(RpcNameIsCodingActive, code);

    /// <summary> 显示打码数据 </summary>
    public int RequestMachineCodingInfo(Action<object> successCallback, Action<BagelCodeError> errorCallback) =>
        severHelper.RequestData(SBoxEventHandle.SBOX_REQUEST_CODER, null, successCallback, errorCallback);
    void OnResponseMachineCodingInfo(SBoxCoderData res) => severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_REQUEST_CODER, res);

    /// <summary> 打码 </summary>
    public int RequestSetCoding(ulong code, Action<object> successCallback, Action<BagelCodeError> errorCallback) =>
        severHelper.RequestData(SBoxEventHandle.SBOX_CODER, code, successCallback, errorCallback);
    void OnResponseSetCoding(SBoxPermissionsData res) => severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_CODER, res);



    /// <summary> 密码校验 </summary>
    public int RequestCheckPassword(int password, Action<object> successCallback, Action<BagelCodeError> errorCallback)
        => severHelper.RequestData(SBoxEventHandle.SBOX_CHECK_PASSWORD, password, successCallback, errorCallback);
    void OnResponseCheckPassword(SBoxPermissionsData res) =>
        severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_CHECK_PASSWORD, res);


    /// <summary> 修改密码 </summary>
    public int RequestChangePassword(int password, Action<object> successCallback, Action<BagelCodeError> errorCallback)
        => severHelper.RequestData(SBoxEventHandle.SBOX_CHANGE_PASSWORD, password, successCallback, errorCallback);
    void OnResponseChangePassword(SBoxPermissionsData res) =>
        severHelper.OnResponsData(SBoxEventHandle.SBOX_CHANGE_PASSWORD, res, res.result != 0);


    /// <summary>
    /// 上分
    /// </summary>
    /// <param name="credit">分数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestScoreUp(int credit, Action<object> successCallback)
        => severHelper.RequestData(RpcNameScoreUp, credit, successCallback, null);
    void OnResponseScoreUp(int credit) => severHelper.OnSuccessResponseData(RpcNameScoreUp, credit);


    /// <summary>
    /// 下分
    /// </summary>
    /// <param name="credit">分数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestScoreDown(int credit, Action<object> successCallback)
        => severHelper.RequestData(RpcNameScoreDown, credit, successCallback, null);
    void OnResponseScoreDown(int credit) => severHelper.OnSuccessResponseData(RpcNameScoreDown, credit);



    /// <summary>
    /// 投币
    /// </summary>
    /// <param name="num">个数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestCoinIn(int num, Action<object> successCallback)
        => severHelper.RequestData(RpcNameCoinIn, num, successCallback, null);
    void OnResponseCoinIn(int num) => severHelper.OnSuccessResponseData(RpcNameCoinIn, num);



    /// <summary>
    /// 退票
    /// </summary>
    /// <param name="num">个数</param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public int RequestCoinOut(int num, Action<object> successCallback)
        => severHelper.RequestData(RpcNameCoinOut, num, successCallback, null);
    void OnResponseCoinOut(int num) => severHelper.OnSuccessResponseData(RpcNameCoinOut, num);




    /// <summary>
    /// 硬件版本
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    public int RequestGetHardwareVersion(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        return severHelper.RequestData(SBoxEventHandle.SBOX_SANDBOX_VERSION, null, successCallback, errorCallback, mark);
    }
    void OnResponseGetHardwareVersion(string res) =>
        severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_SANDBOX_VERSION, res);


    /// <summary>
    /// 算法卡版本
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    public int RequestGetAlgorithmVersion(Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
    {
        return severHelper.RequestData(SBoxEventHandle.SBOX_IDEA_VERSION, null, successCallback, errorCallback, mark);
    }
    void OnResponseGetAlgorithmVersion(string res) =>
        severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_IDEA_VERSION, res);



    /// <summary> 请求配置 </summary>
    public int RequestReadConf(Action<object> successCallback, Action<BagelCodeError> errorCallback)
        => severHelper.RequestData(SBoxEventHandle.SBOX_READ_CONF, null, successCallback, errorCallback);
    void OnResponseReadConf(SBoxConfData res) => severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_READ_CONF, res);


    /// <summary> 修改配置 </summary>
    public int RequestWriteConf(SBoxConfData data, Action<object> successCallback, Action<BagelCodeError> errorCallback) =>
                severHelper.RequestData(SBoxEventHandle.SBOX_WRITE_CONF, data, successCallback, errorCallback);
    void OnResponseWriteConf(SBoxPermissionsData res) => severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_WRITE_CONF, res);


    /// <summary> 获取玩家信息20016 </summary>
    public int RequestGetPlayerInfo(Action<object> successCallback, Action<BagelCodeError> errorCallback) =>
        severHelper.RequestData(SBoxEventHandle.SBOX_GET_ACCOUNT, null, successCallback, errorCallback);
    void OnResponseGetPlayerInfo(SBoxAccount res) => severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_GET_ACCOUNT, res);


    /// <summary> 修改玩家信息 </summary>
    public int RequestSetPlayerInfo(SBoxPlayerAccount req, Action<object> successCallback)
         => severHelper.RequestData(RpcNameSetPlayerInfo, req, successCallback, null);
    void OnResponseSetPlayerInfo(SBoxPlayerAccount res) => severHelper.OnSuccessResponseData(RpcNameSetPlayerInfo, res);


    /// <summary> 获取游戏彩金 </summary>
    public int RequestJackpotGame(Action<object> successCallback, Action<BagelCodeError> errorCallback)
        => severHelper.RequestData(SBoxEventHandle.SBOX_JACKPOT_GAME, null, successCallback, errorCallback);
    void OnResponseJackpotGame(JackpotRes res) =>
        severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_JACKPOT_GAME, res);


    /// <summary> 获取游戏彩金 </summary>
    public int RequestJackpotOnLine()
        => severHelper.RequestData(RpcNameJackpotOnLine, null, null, null);



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
    public int RequestCounter(int id, int counts, int type, Action<object> successCallback)
    {
        Dictionary<string, object> req = new Dictionary<string, object>()
        {
            ["id"] = id,
            ["counts"] = counts,
            ["type"] = type,
        };
        return severHelper.RequestData(SBoxEventHandle.SBOX_SADNBOX_METER_SET, req, successCallback, null);
    }
    void OnResponseCounter(int res) =>
        severHelper.OnSuccessResponseData(SBoxEventHandle.SBOX_SADNBOX_METER_SET, res);


    /// <summary>
    /// 真实数据接口
    /// </summary>
    /// <param name="rpcName"></param>
    /// <param name="req"></param>
    /// <returns></returns>
    public virtual object[] requestFunc(string rpcName, object req)
    {
        object[] result = new object[] { 0 };


        if (ApplicationSettings.Instance.isMock
            || rpcName == SBoxEventHandle.SBOX_JACKPOT_GAME)
        {
            result = requestFuncMock(rpcName, req);
            return result;
        }


        switch (rpcName)
        {

            case SBoxEventHandle.SBOX_IDEA_VERSION:  //获取算法版本
                {
                    SBoxIdea.Version();
                }
                return result;

            /* case SBoxEventHandle.SBOX_SET_PLAYER_BETS:
                 {
                     SBoxIdea.SetPlayerBets((SBoxPlayerBetsData)req);
                 }
                 return resault; */
            case SBoxEventHandle.SBOX_READ_CONF:
                {
                    SBoxIdea.ReadConf();
                }
                return result;
            case SBoxEventHandle.SBOX_WRITE_CONF: //写配置
                {
                    SBoxIdea.WriteConf((SBoxConfData)req);
                }
                return result;

            case SBoxEventHandle.SBOX_CHECK_PASSWORD:
                {
                    int password = (int)req;
                    SBoxIdea.CheckPassword(password);
                }
                return result;
            case SBoxEventHandle.SBOX_CHANGE_PASSWORD:
                {
                    int password = (int)req;
                    SBoxIdea.ChangePassword(password);
                }
                return result;


            case SBoxEventHandle.SBOX_REQUEST_CODER:
                {
                    SBoxIdea.RequestCoder(0);
                }
                return result;
            case SBoxEventHandle.SBOX_CODER: //请求打码
                {
                    ulong code = (ulong)req;
                    //SBoxIdea.Coder(0, ulong.Parse((string)res.value));
                    DebugUtils.Log($"@SBoxIdea.Coder({0},  {code})");
                    SBoxIdea.Coder(0, code);
                }
                return result;

            case RpcNameIsCodingActive: //是否激活
                {
                    int code = SBoxIdea.NeedActivated();
                    DebugUtils.Log($"@SBoxIdea.NeedActivated() =  {code}");
                    OnResponseIsCodingActive(code);
                }
                return result;
            case RpcNameCoinIn:
                {
                    int num = (int)req;
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
                    OnResponseCoinIn((int)req);
                }
                return result;
            case RpcNameCoinOut:
                {
                    int coinOutNum = (int)req;
                    List<SBoxPlayerCoinInfo> sBoxPlayerCoinInfos = new List<SBoxPlayerCoinInfo>()
                    {
                        new SBoxPlayerCoinInfo()
                        {
                            PlayerId = SBoxModel.Instance.pid,
                            CoinOut = coinOutNum,
                        }
                    };
                    //DebugUtils.LogWarning($"通知算法卡退票积分： {credit}");
                    SBoxIdea.SetPlayerCoinInfo(sBoxPlayerCoinInfos);


                    // 自己的回调
                    OnResponseCoinOut((int)req);
                }
                return result;
            case RpcNameScoreUp:
                {
                    int credit = (int)req;
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
                return result;
            case RpcNameScoreDown:
                {
                    int credit = (int)req;
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
                return result;


            case SBoxEventHandle.SBOX_GET_ACCOUNT: // 获取玩家信息
                {
                    SBoxIdea.GetAccount();
                }
                return result;

            case SBoxEventHandle.SBOX_SADNBOX_METER_SET:
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)req;
                    //码表设置
                    SBoxSandbox.MeterSet((int)dic["id"], (int)dic["counts"], (int)dic["type"]);
                }
                return result;

                /*##
                case SBoxEventHandle.SBOX_COIN_PUSH_SPIN:
                    {
                        SBoxIdea.CoinPushGetSpinResult(0);
                    }
                    return resault;

                case SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END:
                    {
                        // 滚轮停止
                        SBoxIdea.CoinPushGetSpinEnd(0);
                    }
                    return resault;

                case RpcNameCoinCountDown:
                    {
                        OnResponseCoinCountDown(SBoxIdea.Jackpot());
                    }
                    return resault;
                case RpcNameGetMyCredit:
                    {

                        foreach (SBoxPlayerScoreInfo item in SBoxIdea.sBoxInfo.PlayerScoreInfoList)
                        {
                            if (item.PlayerId == 1)
                            {
                                mycredit = item.Score;
                            }
                        }

                        OnResponseGetMyCredit(mycredit);
                    }
                    return resault;
                */
        }

        //DebugUtils.LogError($"【MachineDataMgr - Real】没有实现方法：{rpcName}");

        return new object[] { 408, $"【MachineDataMgr - Real】没有实现方法：{rpcName}" };
    }









}
