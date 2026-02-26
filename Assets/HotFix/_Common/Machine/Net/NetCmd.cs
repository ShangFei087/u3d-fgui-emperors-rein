
#define NEW_NET_01





#if NEW_NET_01



//服务器发送给客户端的消息
public enum S2C_CMD
{
    // ====待删除的协议
    S2C_JackpotMinBet, // 旧的鳄鱼大亨协议（待删除）
    S2C_ChangeLanguage, // 旧的鳄鱼大亨协议（待删除）
    S2C_WinJackpot, // 旧的鳄鱼大亨协议（待删除）



    // ====游戏自己的协议
    S2C_HeartHeatR = 100,                      //心跳
    //【推币机】新加的协议  （新的协议名）
    S2C_LoginR,   //（新增）
    S2C_ReadConfR,  // 返回配置
    S2C_JackBetR, // 游戏彩金下注返回（新增）
    C2S_ReceiveJackpotR, // 领取彩金
    S2C_GetJackpotDataR, // 获取游戏彩金记录
    S2C_GetJackpotShowValueR,  // 获取游戏彩金显示值


    /// 1000以上不允许
    /// ==== 联网彩金的的协议
    S2C_InitJackpotInfo = 1500,                //初始化彩金信息
    S2C_JackpotBet,                            //彩金下注
    S2C_KickOut,                               //踢出
    S2C_ConnectFail,                           //连接失败

}


//客户端发送给服务器的消息
public enum C2S_CMD
{
    // ====游戏自己的协议
    C2S_HeartHeat = 200,                      //心跳
    C2S_Login,                                 //登录
    C2S_ReadConf,                            //读取配置
    C2S_JackBet,                               //下注
    C2S_ReceiveJackpot,                        //领取彩金
    C2S_GetJackpotData,                        //获取彩金数据-彩金查账
    C2S_GetJackpotShowValue,                  //获取彩金值-UI显示


    /// 1000以上不允许
    /// ==== 联网彩金的的协议
    C2S_InitJackpotInfo = 2500,                //初始化彩金信息
    C2S_JackpotBet,                            //彩金下注



}




#else

//服务器发送给客户端的消息
public enum S2C_CMD
{
    // ====游戏自己的协议
    S2C_HeartHeat = 1000,                      //心跳
    S2C_WinJackpot,                            //获得彩金
    S2C_Error,                                 //错误
    S2C_GetJackpotData,                        //获取彩金数据
    S2C_JackpotMinBet,                         //彩金最小押分
    S2C_ChangeLanguage,                        //多语言切换

    
    /// ==== 联网彩金的的协议
    S2C_InitJackpotInfo = 1500,                //初始化彩金信息
    S2C_JackpotBet,                            //彩金下注
    S2C_KickOut,                               //踢出
    S2C_ConnectFail,                           //连接失败

    //【推币机】新加的协议  （新的协议名）
    S2C_ReadConfR,  // 返回配置

}
//客户端发送给服务器的消息
public enum C2S_CMD
{
    // ====游戏自己的协议
    C2S_HeartHeat = 2000,                      //心跳
    C2S_Login,                                 //登录
    C2S_JackBet,                               //下注
    C2S_ReceiveJackpot,                        //领取彩金
    C2S_GetJackpotData,                        //获取彩金数据


    /// ==== 联网彩金的的协议
    C2S_InitJackpotInfo = 2500,                //初始化彩金信息
    C2S_JackpotBet,                            //彩金下注

    //【推币机】新加的协议
    C2S_ReadConf,                            //读取配置

}

#endif





