using System;

public struct MessageName
{
    // TODO：消息名称在此定义
    //底层事件
    public const string EVENT_HardCheckOK = "EVENT_HardCheckOK";//100,
    public const string EVENT_CoinIn = "EVENT_CoinIn";//投币
    public const string EVENT_CoinOut = "EVENT_CoinOut";//退币
    public const string EVENT_StopCoinOut = "EVENT_StopCoinOut";//停止退币
    public const string EVENT_KeyStatus = "EVENT_KeyStatus";//按键状态
    public const string EVENT_CashIn = "EVENT_CashIn";//进钞
    public const string EVENT_PrintStatus = "EVENT_PrintStatus";//打印机状态
    public const string EVENT_CashStatus = "EVENT_CashStatus";//纸钞机状态
    public const string EVENT_OneLightStatus = "EVENT_OneLightStatus";//灯状态
    public const string EVENT_AllLightStatus = "EVENT_AllLightStatus";//灯状态
    public const string EVENT_CahserList = "EVENT_CahserList";//纸钞机列表
    public const string EVENT_PrinterList = "EVENT_PrinterList";//打印机列表
    public const string EVENT_PrintOK = "EVENT_PrintOK";//打印成功


    //普通事件
    public const string Event_StartCoinOut = "Event_StartCoinOut";//200,//开始退币
    public const string Event_StartPrint = "Event_StartPrint";//开始打印
    public const string Event_RejectCash = "Event_RejectCash";//拒收纸钞
    public const string Event_SaveCredit = "Event_SaveCredit";//存储积分
    public const string Event_ReadCredit = "Event_ReadCredit";//读取积分
    public const string Event_SetCahserID = "Event_SetCahserID";//设置纸钞机
    public const string Event_SetPrinterID = "Event_SetPrinterID";//设置打印机
    public const string Event_StartCoinCoder = "Event_StartCoinCoder";  //操作码表
    public const string Event_JPAdd = "Event_JPAdd";//收到押分累积彩金
    public const string Event_UpdateJP = "Event_UpdateJP";//更新彩金

    //游戏结果
    public const string Event_SlotRetrun = "Event_SlotRetrun";//拉霸结果回传
    public const string Event_SetGameLevel = "Event_SetGameLevel";//设置难度

    //网络
    public const string Event_InitNetwork = "Event_InitNetwork"; //300,
    public const string Event_RecvNetworkBox = "Event_RecvNetworkBox";
    public const string Event_SendNetworkBox = "Event_SendNetworkBox";
    public const string Event_ServerSendNetwork = "Event_ServerSendNetwork";
    public const string Event_NetworkErr = "Event_NetworkErr";
    public const string Event_NetworkJP = "Event_NetworkJP";//同步彩金
    public const string Event_NetworkPlayerWinJP = "Event_NetworkPlayerWinJP";//玩家赢得彩金
    public const string Event_NetworkCClintInfo = "Event_NetworkCClintInfo";//分机发送账目
    public const string Event_NetworkSClintInfo = "Event_NetworkSClintInfo";//主机查询账目

    //TCP
    public const string Event_NetworkClientData = "Event_NetworkClientData";
    public const string Event_NetworkServerData = "Event_NetworkServerData";

    //Websocket
    public const string Event_NetworkWSServerData = "Event_NetworkWSServerData";
   // public const string Event_NetworkWSClientData = "Event_NetworkWSClientData";

    //通用网络传输事件
    public const string Event_ClientNetworkSend = "Event_ClientNetworkSend";
    public const string Event_ClientNetworkRecv = "Event_ClientNetworkRecv";
    public const string Event_ServerNetworkSend = "Event_ServerNetworkSend";
    public const string Event_ServerNetworkRecv = "Event_ServerNetworkRecv";

    //算法卡底板
    public const string Event_ClientSendBaox = "Event_ClientSendBaox"; //500,


    //算码
    public const string Event_GetSminfo = "Event_GetSminfo";//获取打码信息
    public const string Event_SendSminfo = "Event_SendSminfo";//发送打码信息
    public const string Event_CheckSmRetrn = "Event_CheckSmRetrn";//打码回传
    public const string Event_SminfoRetrn = "Event_SminfoRetrn";//打码信息回传

    //账目信息
    public const string Event_ServerCheckinfo = "Event_ServerCheckinfo";//主机查询事件
    public const string Event_ClientReinfo = "Event_ClientReinfo";//分机机回应查询事件


} 