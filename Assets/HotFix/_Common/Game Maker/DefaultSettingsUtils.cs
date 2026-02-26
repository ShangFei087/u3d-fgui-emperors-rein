using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class DefaultSettingsUtils
{
    [Tooltip("游戏id")]
    static public int gameId = -1;

    [Title("机台参数")]


    #region 彩金设置
    [Title("彩金设置")]

    /// <summary> 彩金巨奖最大值 </summary>
    [Tooltip("彩金巨奖范围最大值")]
    static public long jpGrandRangeMax = 80000;

    /// <summary> 彩金巨奖最小值 </summary>
    [Tooltip("彩金巨奖范围最小值")]
    static public long jpGrandRangeMin = 50000;

    /// <summary> 彩金巨奖最大值 </summary>
    [Tooltip("彩金巨奖最大值")]
    static public long defJpGrandMax = 80000;

    /// <summary> 彩金巨奖最小值 </summary>
    [Tooltip("彩金巨奖最小值")]
    static public long defJpGrandMin = 50000;

    /// <summary> 彩金头奖最大值 </summary>
    [Tooltip("彩金头奖范围最大值")]
    static public long jpMajorRangeMax = 30000;

    /// <summary> 彩金头奖最小值 </summary>
    [Tooltip("彩金头奖范围最小值")]
    static public long jpMajorRangeMin = 10000;

    /// <summary> 彩金头奖最大值 </summary>
    [Tooltip("彩金头奖最大值")]
    static public long defJpMajorMax = 30000;

    /// <summary> 彩金头奖最小值 </summary>
    [Tooltip("彩金头奖最小值")]
    static public long defJpMajorMin = 10000;


    /// <summary> 彩金大奖最大值 </summary>
    [Tooltip("彩金大奖范围最大值")]
    static public long jpMinorRangeMax = 8000;

    /// <summary> 彩金大奖最小值 </summary>
    [Tooltip("彩金大奖范围最小值")]
    static public long jpMinorRangeMin = 5000;


    /// <summary> 彩金大奖最大值 </summary>
    [Tooltip("彩金大奖最大值")]
    static public long defJpMinorMax = 8000;

    /// <summary> 彩金大奖最小值 </summary>
    [Tooltip("彩金大奖最小值")]
    static public long defJpMinorMin = 5000;





    /// <summary> 彩金小奖最大值 </summary>
    [Tooltip("彩金小奖范围最大值")]
    static public long jpMiniRangeMax = 3000;

    /// <summary> 彩金小奖最小值 </summary>
    [Tooltip("彩金小奖范围最小值")]
    static public long jpMiniRangeMin = 1000;

    /// <summary> 彩金小奖最大值 </summary>
    [Tooltip("彩金小奖最大值")]
    static public long defJpMiniMax = 3000;

    /// <summary> 彩金小奖最小值 </summary>
    [Tooltip("彩金小奖最小值")]
    static public long defJpMiniMin = 1000;


    #endregion

    #region 用户设置
    [Title("用户设置")]

    [Title("用户密码Admin")]
    static public string passwordAdmin = "187653214";
    [Title("用户密码Manager")]
    static public string passwordManager = "88888888";
    [Title("用户密码Shift")]
    static public string passwordShift = "666666";
    #endregion

    #region 语言
    [Title("语言")]

    static public string defLanguage = "cn";
    #endregion

    #region 音乐
    [Title("音效设置")]
    /// <summary> 音效 </summary>
    static public float defSound = 0.5f;

    /// <summary> 背景音乐 </summary>
    static public float defMusic = 0.5f;
    #endregion

    #region 数据记录设置
    [Title("数据记录设置")]

    [Tooltip("最大游戏次数记录")]
    static public int maxMaxGameRecord = 50000;
    [Tooltip("最小游戏次数记录")]
    static public int minMaxGameRecord = 100;
    [Tooltip("默认游戏次数记录")]
    static public int defMaxGameRecord = 1000;

    [Tooltip("最大投退币次数记录")]
    static public int maxMaxCoinInOutRecord = 50000;
    [Tooltip("最少投退币次数记录")]
    static public int minMaxCoinInOutRecord = 100;
    [Tooltip("默认投退币次数记录")]
    static public int defMaxCoinInOutRecord = 1000;

    [Tooltip("最大彩金次数记录")]
    static public int maxMaxJackpotRecord = 50000;
    [Tooltip("最下彩金次数记录")]
    static public int minMaxJackpotRecord = 100;
    [Tooltip("默认彩金次数记录")]
    static public int defMaxJackpotRecord = 1000;

    [Tooltip("最大报错次数记录")]
    static public int maxMaxErrorRecord = 5000;
    [Tooltip("最下报错次数记录")]
    static public int minMaxErrorRecord = 100;
    [Tooltip("默认报错次数记录")]
    static public int defMaxErrorRecord = 500;


    [Tooltip("最大事件次数记录")]
    static public int maxMaxEventRecord = 5000;
    [Tooltip("最下事件次数记录")]
    static public int minMaxEventRecord = 100;
    [Tooltip("默认事件次数记录")]
    static public int defMaxEventRecord = 500;


    [Tooltip("日营收统计记录最大次数")]
    static public int maxMaxBusinessDayRecord = 720;
    [Tooltip("日营收统计记录最小次数")]
    static public int minMaxBusinessDayRecord = 1;
    [Tooltip("默认日营收统计记录次数")]
    static public int defMaxBusinessDayRecord = 7;


    #endregion

    #region 投退币设置
    [Title("投退币设置")]

    [Tooltip("最大上下分倍率(1脉冲多少分)")]
    static public int maxScoreUpDownScale = 10000;
    [Tooltip("最小上下分倍率(1脉冲多少分)")]
    static public int minScoreUpDownScale = 100;
    [Tooltip("默认上下分倍率(1脉冲多少分)")]
    static public int defScoreUpDownScale = 100;


    [Tooltip("最大上分长按倍率(1脉冲多少分)")]
    static public int maxScoreUpLongClickScale = 100000;
    [Tooltip("最小上分长按倍率(1脉冲多少分)")]
    static public int minScoreUpLongClickScale = 1000;
    [Tooltip("默认上分长按倍率(1脉冲多少分)")]
    static public int defScoreUpLongClickScale = 1000;


    /// <summary>1币几分 最大值 </summary>
    [Tooltip("最大投币倍率(1币几分)")]
    static public int maxCoinInScale = 1000;
    /// <summary>1币几分 最小值 </summary>
    [Tooltip("最小投币倍率(1币几分)")]
    static public int minCoinInScale = 100;
    [Tooltip("默认投币倍率(1币几分)")]
    static public int defCoinInScale = 1000;


    /// <summary> “1票几分”最大值 </summary>
    [Tooltip("最大退票倍率(1票几分)")]
    static public readonly int maxCoinOutCreditPerTicket = 50;
    /// <summary> “1票几分”最小值 </summary>
    [Tooltip("最小退票倍率(1票几分)")]
    static public readonly int minCoinOutCreditPerTicket = 1;
    [Tooltip("默认退票倍率(1票几分)")]
    static public readonly int defCoinOutCreditPerTicket = 1;


    /// <summary>  “1分几票”最大值 </summary>
    [Tooltip("最大退票倍率(1分几票)")]
    static public readonly int maxCoinOutTicketPerCredit = 50;
    /// <summary>  “1分几票”最小值 </summary>
    [Tooltip("最小退票倍率(1分几票)")]
    static public readonly int minCoinOutTicketPerCredit = 1;
    [Tooltip("默认退票倍率(1分几票)")]
    static public readonly int defCoinOutTicketPerCredit = 1;


    /// <summary> “1钞几分”最大值  </summary>
    [Tooltip("最大进钞倍率(1钞几分)")]
    static public readonly int maxBillInScale = 100;
    /// <summary> “1钞几分”最小值 </summary>
    [Tooltip("最小进钞倍率(1钞几分)")]
    static public readonly int minBillInScale = 1;
    [Tooltip("默认进钞倍率(1钞几分)")]
    static public readonly int defBillInScale = 1;


    /// <summary> “打印”最大值  </summary>
    [Tooltip("最大打印倍率(1钞多少分)")]
    static public readonly int maxPrintOutScale = 100;
    /// <summary> “打印”最小值 </summary>
    [Tooltip("最小打印倍率(1钞多少分)")]
    static public readonly int minPrintOutScale = 10;
    [Tooltip("默认打印倍率(1钞多少分)")]
    static public readonly int defPrintOutScale = 10;

    [Tooltip("主货币和游戏分兑换比例(1钞多少分)")]
    static public int moneyMeterScale = 100;
    #endregion



    /// <summary> “打印”最大值  </summary>
    [Tooltip("最大打印倍率(1钞多少分)")]
    static public readonly int maxJackpotPercent = 100;
    /// <summary> “打印”最小值 </summary>
    [Tooltip("最小打印倍率(1钞多少分)")]
    static public readonly int minJackpotPercent = 1;
    [Tooltip("默认打印倍率(1钞多少分)")]
    static public readonly int defJackpotPercent = 5;




    /// <summary> 是否显示打印日志 </summary>
    static public int isDebug => ApplicationSettings.Instance.isRelease ? 0 : 1;

    /// <summary> 是否显示包更新信息 </summary>
    static public int isUpdateInfo => ApplicationSettings.Instance.isRelease ? 0 : 1;

    /// <summary> 是否打开联网彩金 </summary>
    static public int isJackpotOnline => 1;

    /// <summary> 使用调试页面 </summary>
    static public int enableReporterPage => ApplicationSettings.Instance.isRelease ? 0 : 1;

    /// <summary> 使用测试工具 </summary>
    static public int enableTestTool => ApplicationSettings.Instance.isRelease ? 0 : 1;
}
