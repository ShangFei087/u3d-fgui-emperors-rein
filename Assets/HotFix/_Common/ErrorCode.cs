using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ErrorCode
{
    #region  服务器（通用成功）
    /// <summary> 响应正常 </summary>
    public static int OK = 0;

    /// <summary> 通用失败 </summary>
    public static int ERROR = 1;
    #endregion


    #region  Net 0 - 999
    #endregion

    #region  Hall 1000 - 1999
    #endregion




    #region Game 2000 - 2999
    /// <summary> 游戏数据请求失败 </summary>
    public static int SLOT_SPIN_REQUEST_ERR = 2000;

    /// <summary> 游戏彩金请求失败 </summary>
    public static int JACKPOT_GAME_REQUEST_ERR = 2001;

    /// <summary> 联网彩金请求失败 </summary>
    public static int JACKPOT_ONLINE_REQUEST_ERR = 2002;

    #endregion



    #region  Machine 3000 - 3999
    /// <summary> 算法卡 加钱响应超时 </summary>
    public static int DEVICE_SBOX_COIN_IN_OVERTIME = 3001;
    #endregion





    #region Device 4000 - 4999

    /* ==== 硬件驱动默认 0-9 */
    /// <summary> 创建投退币订单号 </summary>
    public static int DEVICE_CREAT_ORDER_NUMBER = 4000;

    /* ====上下分 20-29 */

    /*  ==== 打印机缺纸 30-39 */
    /// <summary> 打印机缺纸 </summary>
    public static int DEVICE_PRINTER_OUT_OF_PAGE = 4031;
    /* ==== 投币机 40-49 */
    /* ==== 退票机 50-59 */
    /* ==== 纸钞机 60-69 */


    /* ==== 好酷 退票 70-79 */

    /// <summary> 好酷退票成功 </summary>
    public static int DEVICE_IOT_COIN_OUT_SUCCESS = 4070;


    /// <summary> 数据对不上 </summary>
    public static int DEVICE_IOT_COIN_OUT_DATA_MISMATCH = 4071;

    /// <summary> 找不到缓存数据 </summary>
    public static int DEVICE_IOT_COIN_OUT_CACHE_NOT_FIND = 4072;
    /// <summary> 好酷退票api报错 </summary>
    public static int DEVICE_IOT_COIN_OUT_API_ERR = 4073;

    /// <summary> 好酷退票本地缓存没找到</summary>
    public static int DEVICE_IOT_COIN_OUT_CACHE_ORDER_IS_NOT_FIND = 4074;

    /// <summary> 好酷Mqtt断开连接</summary>
    public static int DEVICE_IOT_MQTT_NOT_CONNECT = 4075;

    /// <summary> 好酷未登录（未获取二维码数据） </summary>
    public static int DEVICE_IOT_NOT_SIGN_IN = 4076;


    /* ==== 好酷 投币 80-89 */
    public static int DEVICE_IOT_COIN_IN_SUCCESS = 4080;

    /// <summary> 没有“投币绑定微信号” </summary>
    public static int DEVICE_IOT_COIN_IN_NOT_BIND_WECHAT_ACCOUNT = 4081;


    /* ==== 钱箱：主动上分 90-100 */
    public static int DEVICE_MB_QRCODE_IN_SUCCESS = 4090;

    /* ==== 钱箱：主动下分 100-109 */
    public static int DEVICE_MB_QRCODE_OUT_SUCCESS = 4100;

    /// <summary> 下分请求点单id失败</summary>
    public static int DEVICE_MB_GET_ORDER_ID_FAIL = 4101;


    /* ==== 钱箱：钱箱要求机台上分被动上分 110-119 */
    public static int DEVICE_MB_REQ_MACH_QRCODE_IN_SUCCESS = 4110;

    #endregion
}
