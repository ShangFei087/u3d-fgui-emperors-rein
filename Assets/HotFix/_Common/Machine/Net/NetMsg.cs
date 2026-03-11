using System.Net.Sockets;
using System.Collections.Generic;
using SBoxApi;
//服务器信息
public class ServerInfo
{
    public string IP { get; set; }
    public int port { get; set; }
}

public class ClientInfo
{
    public string IP { get; set; }
    public int port { get; set; }

    public int macId { get; set; }
    public string USN { get; set; }
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
    public int cmd { get; set; }        //协议
    public int id { get; set; }         //这里一般都是机台ID
    public string jsonData; //
}

//S2C_Config
public class MsgConfig
{
    public int coinValue;
    public int ticketValue;
    public int scoreTicket;
    public int pulseValue;
    public List<int> switchList;
    public int gameState;
    public int countDown;
    public int curLanguage;
    public int betsMinOfJackpot;
    public int netJackpotState;     //网络彩金状态
    public List<int> betsMinOfNetJackpots;
    public bool clearCoinIn;
    public int minBetExt0;
    public int minBetExt1;
}

public class GameInfo
{
    public int[] odds;
    public int credit;
    public int wins;
    public List<int> bubbleList = new List<int>();
}

//S2C_CreditAction
public class CreditActionInfo
{
    public int type; //0:投币, ps:其他先不管
    public int value;
}

//S2C_State
public class CurrentStateID
{
    public int stateID;
}

//C2S_SBoxPlayerInState
public class PlayerInState
{
    public int[] inState;
    public bool isChange;
}

//C2S_SyncStatus
public class PlayerStatus
{
    public int[] inState;
    public int coinInCount;
    public int coinOutCount;
}

//S2C_NetJackpotInfo
public class NetJackpotInfo
{
    public int win;
    public int jackpotId;
}

//S2C_Error
public class ErrorInfo
{
    public int errCode;
    public string errString;
}

/// <summary>
/// 历史记录数据结构
/// </summary>
public class HistoryRecordData
{
    public int macId;
    public int wins;
    public int credit;
    public int winIndex;
    public int jackpot;
    public int subGameId;
    public int jackpotWin;
    public int[] odds = new int[22];
    public int[] betData = new int[22];
    public List<int> freeRole = new List<int>();
    public List<int> bubbleList = new List<int>();
}

public class RecordStruct
{
    /// <summary>
    /// 开奖结果
    /// </summary>
    public int value;
    /// <summary>
    /// 小游戏id 0:未中, 1:送灯, 3:彩金
    /// </summary>
    public int subGameId;
    /// <summary>
    /// 中奖数据
    /// </summary>
    public int[] data;
    /// <summary>
    /// 是否为赠送
    /// </summary>
    public bool isFree;
}

public class TotalRecord
{
    public List<RecordStruct> recordStructs;
    public List<HistoryRecordData> historyRecordDatas;
}

#region 联网彩金



//登录大厅彩金数据(登录大厅彩金或获取彩金押注赢分相关数据时使用C2S_Login
public class LoginInfo
{
    public int gameType;
    public int macId;
}
//大厅彩金后台相关配置(用于机台管理后台)
public class JackpotConfig
{
    public int jackpotSwitch;  //彩金开关	0:关 1:开
    public int betPercent;     //押分比例 	默认值100(具体数值请询问相关策划, 目前设定只允许查看, 不允许修改)
    public int jpPercent;      //彩金百分比	默认值5  (具体数值请询问相关策划, 目前只允许查看, 不允许修改)
}

//押注上行数据(给大厅彩金主机发送押注信息时使用 C2S_JackBet(C2S_JackBetInfo)
public class JackBetInfo
{
    public int gameType;                       // 游戏类型 
    public int seat;                           // 分机号/座位号                   
    public int bet;                            // 当前的押分,为了避免丢失小数，需要乘以100，硬件读取这个值会除以100后使用
    public int betPercent;                     // 押分比例，目前拉霸默认值传1，同样需要乘以100          
    public int scoreRate;                      // 分值比，1分多少钱，需要乘以1000再往下传
    public int JPPercent;                      // 分机彩金百分比，每次押分贡献给彩金的比例。需要乘以1000再往下传
}

//大厅彩金主机下行赢分数据(赢得彩金时大厅彩金主动下发, ps:句柄cmd:S2C_WinJackpot)
public class WinJackpotInfo
{
    public int macId;           // 机台号
    public int seat;            // 分机号/座位号
    public int win;             // 中奖金额, 传过来的是币数的100倍, 需要转换成分数进行使用
    public int jackpotId;       // 彩金类型Id, 0:大彩金, 1:中彩金, 2:小彩金, 3:迷你彩金
    public long orderId;        // 彩金唯一id, 用于判断是否已处理过本次彩金避免重复处理
    public long time;           // 中奖时间戳
}

//领取主机下发的彩金赢分(接到大厅彩金下发的赢分需要把订单号传给大厅彩金主机, ps:句柄cmd:C2S_ReceiveJackpot)
public class ReceiveJackpotInfo
{
    public int gameType;
    public long orderId;
}

//大厅彩金统计数据(机台管理后台需要查看统计数据时主动向大厅彩金主机请求, ps: 句柄cmd:S2C_GetJackpotData)
public class JackpotDeviceBetData
{
    public int macId;       	// 机台号
    public int seatId;      	// 分机号/座位号
    public long bet;        	// 总押分
    public int betPercent;  	// 押分比例
    public int scoreRate;   	// 分值比
    public int jpPercent;   	// 分机彩金百分比，每次押分贡献给彩金的比例
    public long win;        	// 总赢分
    public long grandWin;   	// 大彩金总赢分
    public int grandTimes;  	// 大彩金总赢次数
    public long majorWin;   	// 中彩金总赢分
    public int majorTimes;  	// 中彩金总赢次数
    public long minorWin;   	// 小彩金总赢分
    public int minorTimes;  	// 小彩金总赢次数
    public long miniWin;    	// 迷你彩金总赢分
    public int miniTimes;   	// 迷你彩金总赢次数
}

#endregion

#region 发送给网页的数据结构
/// <summary>
/// 鳄鱼大亨结构
/// </summary>
public class S2P_CrocodileData
{
    public S2P_CommonData data;
    public S2P_CrocodileRecord record;
}

public class S2P_CommonData
{
    public int gameId;
    public int machineId;
    public int ticketMode;
    public int recharge;
    public int takeOut;
    public int coinIn;
    public int coinOut;
    public int bet;
    public int win;
    public int profit;
    public float profitRate;
    public List<S2P_PlayerData> players;
}

public class S2P_PlayerData
{
    public int macId;
    public int bet;
    public int win;
    public int score;
    public int winScore;
}

public class S2P_CommonRecord
{
    public int roundId;
    public int winIndex;
    public int extraWinIndex;
    public List<S2P_PlayerBetData> playerBetDatas;
}

public class S2P_PlayerBetData
{
    public int macId;
    public int win;
    public int[] betList;
}

public class S2P_CrocodileRecord : S2P_CommonRecord
{
    public int times;
    public int[] oddList;
    public List<int> freeRoleList;
}

public class PostTempData
{
    public SBoxIdeaSummaryData gamePage;
}

public class PostRecord
{
    public int gameId;
    public int machineId;
    public int recharge;
    public int takeOut;
    public int coinIn;
    public int coinOut;
    public int bet;
    public int win;
    public int profit;
    public float profitRate;
    public PostTempData totalExtraRecord;
}

#endregion
