
#define NEW_NET_01

#if NEW_NET_01


using SBoxApi;
using System.Net.Sockets;
//服务器信息
public class ServerInfo
{
    public string IP { get; set; }
    public int port { get; set; }
}

//服务器收到的数据结构(TOOD 此处可能需要优化)
public class SrvMsgData
{
    public Socket mSocket { get; set; }
    public string mData { get; set; }
}

//服务器收到的websocket数据结构
public class WSSrvMsgData
{
    public WebSockets.ClientConnection Client { get; set; }
    public string Data { get; set; }
}

//消息体
public class MsgInfo
{
    /// <summary> 协议名称 </summary>
    public int cmd { get; set; }        //协议

    /// <summary> 机台ID </summary>
    public int id { get; set; }         //这里一般都是机台ID

    /// <summary> 数据 </summary>
    public string jsonData;
}

//C2S_JackBet
public class JackBetInfo
{
    public int gameType;                       // 游戏类型
    public int seat;                           // 分机号/座位号                   
    public int bet;                            // 当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用
    public int betPercent;                     // 100 - 押分比例，目前拉霸(推币机)默认值传1，同样需要乘以100          
    public int scoreRate;                      // 1000 - 分值比，1分多少钱，需要乘以1000再往下传
    public int JPPercent;                      // 1000 - 分机彩金百分比，每次押分贡献给彩金的比例。需要乘以1000再往下传
}


public class JackBetInfoCoinPush : RequestBase
{
    //public int gameType;                       // 游戏类型
    public int seat;                           // 分机号/座位号                   
    public int betPercent;                     // 100 - 押分比例，目前拉霸(推币机)默认值传1，同样需要乘以100          
    public int scoreRate;                      // 1000 - 分值比，1分多少钱，需要乘以1000再往下传
    public int JPPercent;                      // 1000 - 分机彩金百分比，每次押分贡献给彩金的比例。需要乘以1000再往下传

    public int majorBet;  // major贡献值  (当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用)
    public int grandBet;  // grand贡献值  (当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用)
    public int minorBet;  //minor贡献值  (当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用)
    public int miniBet;  // mini贡献值  (当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用)
}
public class JackBetInfoCoinPushR : ResponseBase
{
    public SBoxJackpotData sboxJackpotData;
}




//S2C_WinJackpot
public class WinJackpotInfo
{
    public int macId;
    public int seat;
    public int win;
    public int jackpotId;
    public long orderId;
    public long time;
}




//S2C_Error
public class ErrorInfo
{
    public int errCode;
    public string errString;
}


public enum GameType
{
    None = 0,
    CoinPusher = 1,
    Slot = 2,
}

public class LoginInfo : RequestBase
{
    /// <summary> 游戏类型 </summary>
    //public int gameType;

    /// <summary> 机台id </summary>
    public int macId;
    // 组号
    public int groudId;
    // 座位号
    public int seatId;
}

public class LoginInfoR : ResponseBase { }






/// <summary> 读取彩金后台数据 </summary>
public class ReadConfR : ResponseBase
{
    public SBoxConfData sboxConfData;
}






//C2S_ReceiveJackpot
public class ReceiveJackpotInfo
{
    public int gameType;
    public long orderId;
}

public enum OrderDataMode
{
    Grand = 0,
    Major = 1,
    Minor = 2,
    Mini = 3,
    Fix = 4
}



/// <summary>#seaweed# 游戏彩金显示值 </summary>
public class JackpotGameShowInfoR : ResponseBase
{
    /// <summary> 当前彩金值 </summary>
    public int[] curJackpotOut;
}



/// <summary>#seaweed# 请求基类 </summary>
public class RequestBase
{
    static int _seqID = 0;    // -1 : 没有， 0 : 系统
    static int CreatSeqID()
    {
        if (++_seqID > 10000) _seqID = 1;
        return _seqID;
    }

    public RequestBase()
    {
        this.seqId = CreatSeqID();
    }

    /// <summary> 游戏类型 </summary>
    public int gameType;
    /// <summary> 包id </summary>
    public int seqId = -1;
}


/// <summary>#seaweed# 响应基类 </summary>
public class ResponseBase
{
    /// <summary> 游戏类型 </summary>
    //public int gameType;
    /// <summary> 回传包id </summary>
    public int seqId = -1;
    /// <summary> 包状态标志位 </summary>
    public int code = 0;
    /// <summary> 包信息 </summary>
    public string msg = "";
}



#else

using System.Net.Sockets;
//服务器信息
public class ServerInfo
{
    public string IP { get; set; }
    public int port { get; set; }
}

//服务器收到的数据结构(TOOD 此处可能需要优化)
public class SrvMsgData
{
    public Socket mSocket { get; set; }
    public string mData { get; set; }
}

//服务器收到的websocket数据结构
public class WSSrvMsgData
{
    public WebSockets.ClientConnection Client { get; set; }
    public string Data { get; set; }
}

//消息体
public class MsgInfo
{
    /// <summary> 协议名称 </summary>
    public int cmd { get; set; }        //协议

    /// <summary> 机台ID </summary>
    public int id { get; set; }         //这里一般都是机台ID

    /// <summary> 数据 </summary>
    public string jsonData;
}

//C2S_JackBet
public class JackBetInfo
{
    public int gameType;                       // 游戏类型
    public int seat;                           // 分机号/座位号                   
    public int bet;                            // 当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用
    public int betPercent;                     // 100 - 押分比例，目前拉霸(推币机)默认值传1，同样需要乘以100          
    public int scoreRate;                      // 1000 - 分值比，1分多少钱，需要乘以1000再往下传
    public int JPPercent;                      // 1000 - 分机彩金百分比，每次押分贡献给彩金的比例。需要乘以1000再往下传
}


public class JackBetInfoCoinPush
{
    public int gameType;                       // 游戏类型
    public int seat;                           // 分机号/座位号                   
    public int betPercent;                     // 100 - 押分比例，目前拉霸(推币机)默认值传1，同样需要乘以100          
    public int scoreRate;                      // 1000 - 分值比，1分多少钱，需要乘以1000再往下传
    public int JPPercent;                      // 1000 - 分机彩金百分比，每次押分贡献给彩金的比例。需要乘以1000再往下传

    public int majorBet;  // major贡献值  (当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用)
    public int grandBet;  // grand贡献值  (当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用)
}




//S2C_WinJackpot
public class WinJackpotInfo
{
    public int macId;
    public int seat;
    public int win;
    public int jackpotId;
    public long orderId;
    public long time;
}




//S2C_Error
public class ErrorInfo
{
    public int errCode;
    public string errString;
}


public enum GameType
{
    None = 0,
    CoinPusher = 1,
    Slot = 2,
}
//C2S_login
public class LoginInfo
{
    /// <summary> 游戏类型 </summary>
    public int gameType;
    /// <summary> 机台id </summary>
    public int macId;

    // 组号
    public int groudId;
    // 座位号
    public int seatId;

}




//C2S_ReceiveJackpot
public class ReceiveJackpotInfo
{
    public int gameType;
    public long orderId;
}

public enum OrderDataMode
{
    Grand = 0,
    Major = 1,
    Minor = 2,
    Mini = 3,
    Fix = 4
}



/// <summary>#seaweed# 游戏彩金显示值 </summary>
public class JackpotGameShowInfo
{
    /// <summary> 当前彩金值 </summary>
    public int[] curJackpotOut;
}

/// <summary>#seaweed# 请求基类 </summary>
public class RequestBaseInfo
{
    static int _seqID = 0;    // -1 : 没有， 0 : 系统
    static int CreatSeqID()
    {
        if (++_seqID > 10000)
            _seqID = 1;
        return _seqID;
    }

    public RequestBaseInfo()
    {
        this.seqId = CreatSeqID();
    }

    /// <summary> 游戏类型 </summary>
    public int gameType;
    /// <summary> 包id </summary>
    public int seqId = -1;
    /// <summary> 包数据 </summary>
    public string data = null;
}

/// <summary>#seaweed# 响应基类 </summary>
public class ResponseBaseInfo
{
    /// <summary> 游戏类型 </summary>
    public int gameType;
    /// <summary> 回传包id </summary>
    public int seqId = -1;
    /// <summary> 包状态标志位 </summary>
    public int code = 0;
    /// <summary> 包信息 </summary>
    public string msg = "";
    /// <summary> 包数据 </summary>
    public string data = null;
}




#endif