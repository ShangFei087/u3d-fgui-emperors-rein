using SBoxApi;
using System;

/// <summary>
/// </summary>
/// <remarks>
/// * 简化的封装，让代码更加直观。
/// </remarks>
public partial class PusherMachineDataManager02 : ProxyHelper<PusherMachineDataManager02>
{

    bool isMock => ApplicationSettings.Instance.isMock;

    protected void Awake()
    {
        receiveOvertimeS = 1.5f;
        isDebugLog = true;
        prefix = "【SBox】";
    }

    protected  void Start()
    {
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, OnResponseCosoleTesetStartEnd);
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, OnResponseGetCoinPushHardwareFlag);
        EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, OnResponseGetCoinPushHardwareResult);
    }


    protected override void OnDestroy()
    {

        EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, OnResponseCosoleTesetStartEnd);
        EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, OnResponseGetCoinPushHardwareFlag);
        EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, OnResponseGetCoinPushHardwareResult);

        base.OnDestroy();

    }




    /// <summary>
    ///  开始测试"开始"或"停止"
    /// </summary>
    /// <param name="oper">
    /// 1:发币测试
    /// 2:发球测试
    /// 3:推盘测试
    /// 4:雨刷测试
    /// 5:铃铛测试
    /// 6:回币测试
    /// 7:回球测试
    /// 255: 停止所有测试
    /// </param>
    /// <param name="finishCallback"></param>
    /// <returns></returns>
    /// <remarks>
    /// * 切换测试模式后，必须手动停止之前的测试项
    /// </remarks>
    public int RequestCosoleTesetStartEnd(int oper, Action<object> finishCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, oper, finishCallback, null, mark);

        if (isMock)
        {
            OnMockCoinPushConsoleHardwareTestStartEnd(oper);
        }
        else
        {
            SBoxIdea.CheckCoinPushHardware(oper);
        }
        return seqId;

    }
    void OnResponseCosoleTesetStartEnd(string res) => OnResponse(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, res);




    /// <summary>
    /// 获取当前硬件测试状态
    /// </summary>
    /// <param name="finishCallback"></param>
    /// <returns></returns>
    public int RequestGetCoinPushHardwareFlag(Action<object> finishCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, null, finishCallback, null, mark);

        if (isMock)
        {
            OnMockCoinPushConsoleHardwareFlg(null);
        }
        else
        {
            SBoxIdea.GetHardwareFlag();
        }
        return seqId;
    }

    void OnResponseGetCoinPushHardwareFlag(string res) => OnResponse(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, res);


    public int RequestGetCoinPushHardwareResult(Action<object> finishCallback, string mark = null)
    {
        int seqId = OnRequestBefore(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, null, finishCallback, null, mark);

        if (isMock)
        {
            OnMockCoinPushConsoleHardwareResult(null);
        }
        else
        {
            SBoxIdea.GetHardwareResult();
        }
        return seqId;
    }

    void OnResponseGetCoinPushHardwareResult(string res) => OnResponse(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, res);




}
