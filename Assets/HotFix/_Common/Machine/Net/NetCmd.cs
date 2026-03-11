//服务器发送给客户端的消息

public enum S2C_CMD
{
    S2C_JackpotHearHeat = 1000,         //彩金心跳
    S2C_WinJackpot,                     //获得彩金
    S2C_JackpotError,                   //错误
    S2C_GetJackpotData,                 //获取彩金数据
    S2C_JackpotMinBet,                  //彩金最小押分
}

//客户端发送给服务器的消息
public enum C2S_CMD
{

    C2S_JackpotHeartHeat = 2000,        //彩金心跳
    C2S_Login,                          //登录
    C2S_JackBet,                        //下注
    C2S_ReceiveJackpot,                 //领取彩金
    C2S_GetJackpotData,                 //获取彩金数据
}

public enum NET_STATUS
{
    NET_STATUS_DISCONNECTED = 0,
    NET_STATUS_CONNECTED,
}