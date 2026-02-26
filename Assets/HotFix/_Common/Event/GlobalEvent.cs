using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameMaker
{
    public static partial class GlobalEvent
    {


        #region 工具事件
        ///<summary> 工具事件 </summary>
        public const string ON_TOOL_EVENT = "ON_TOOL_EVENT";

        ///<summary> 点击“性能分析”</summary>
        public const string AnalysisTest = "AnalysisTest";

        ///<summary> 点击“页面按钮”</summary>
        public const string PageButton = "PageButton";

        ///<summary> 点击“打印机按钮”</summary>
        public const string CustomButtonDivicePrenter = "CustomButtonDivicePrenter";
        ///<summary> 点击“清掉激活码”</summary>
        public const string CustomButtonClearCode = "CustomButtonClearCode";
        ///<summary> 点击“上分”</summary>
        public const string CustomButtonCreditUp = "CustomButtonCreditUp";
        ///<summary> 点击“下分”</summary>
        public const string CustomButtonCreditDown = "CustomButtonCreditDown";

        /// <summary> 获取sbox玩家账号信息 </summary>
        public const string CustomButtonSboxGetAccount = "CustomButtonSboxGetAccount";

        ///<summary> 点击“投币”</summary>
        public const string CustomButtonCoinIn = "CustomButtonCoinIn";

        ///<summary> 点击“退票”</summary>
        public const string CustomButtonTicketOut = "CustomButtonTicketOut";

        /// <summary> 提示信息 </summary>
        public const string TipPopupMsg = "TipPopupMsg";

        /// <summary> 好酷二维码投币 </summary>
        public const string IOTCoinIn = "IOTCoinIn";

        /// <summary> 好酷二维码退票 </summary>
        //public const string IOTTicketOut = "IOTTicketOut";

        /// <summary> 显示编码 </summary>
        public const string ShowCode = "ShowCode";

        /// <summary> Aes 测试 </summary>
        public const string AesTest = "AesTest";


        /// <summary> 码表清除 </summary>
        public const string DeviceCounterClear = "DeviceCounterClear";

        /// <summary> 投币码表加1 </summary>
        public const string DeviceCounterAddCoinIn = "DeviceCounterAddCoinIn";

        /// <summary> 退币码表加1 </summary>
        public const string DeviceCounterAddCoinOut = "DeviceCounterAddCoinOut";
        /// <summary> 投币码表加100 </summary>
        public const string DeviceCounterAddCoinIn100 = "DeviceCounterAddCoinIn100";

        /// <summary> 显示表格最近n条数据 </summary>
        public const string ShowTableLastData = "ShowTableLastData";


        /// <summary> 测试打印二维码 </summary>
        public const string DeviceTestPrintQRCode = "DeviceTestPrintQRCode";

        public const string DeviceTestPrintTicket = "DeviceTestPrintTicket";


        public const string DeviceTestPrintJCM950 = "DeviceTestPrintJCM950";
        public const string DeviceTestPrintTRANSACT950 = "DeviceTestPrintTRANSACT950";

        public const string ApplicationQuit = "ApplicationQuit";

        #endregion




        #region GM事件

        ///<summary> 工具事件 </summary>
        public const string ON_GM_EVENT = "ON_GM_EVENT";

        public const string GMSingleWinLine = "GMSingleWinLine";
        public const string GMMultipleWinLine = "GMMultipleWinLine";
        public const string GMFreeSpin = "GMFreeSpin";
        public const string GMBigWin = "GMBigWin";
        public const string GMJp1 = "GMJp1";
        public const string GMJp2 = "GMJp2";
        public const string GMJp3 = "GMJp3";
        public const string GMJp4 = "GMJp4";
        public const string GMJpOnline = "GMJpOnline";    
        public const string GMBonus1 = "GMBonus1";     
        public const string GMBonus2 = "GMBonus2";   
        public const string GMBonus3 = "GMBonus3";
        public const string XRay = "XRay";
        #endregion


        #region 测试事件

        ///<summary> 工具事件 </summary>
        public const string ON_TEST_EVENT = "ON_TEST_EVENT";
        public const string DestroyPanel = "DestroyPanel";
        public const string DestroyReels = "DestroyReels";

        #endregion



        #region 初始化系统参数事件
        /// <summary>  参数初始化事件  </summary>
        public const string ON_INIT_SETTINGS_EVENT = "ON_INIT_SETTINGS_EVENT";
        /// <summary> 添加参数初始化条数-value:int</summary>
        public const string AddSettingsCount = "AddSettingsCount";
        /// <summary> 添加参数初始化条数-value:string</summary>
        public const string InitSettings = "InitSettings";
        /// <summary> 刷新加载页进度条显示的信息-value:string</summary>
        public const string RefreshProgressMsg = "RefreshProgressMsg";


        /// <summary> 参数初始化系统参数结束事件事件</summary>
        public const string ON_INIT_SETTINGS_FINISH_EVENT = "ON_INIT_SETTINGS_FINISH_EVENT";
        #endregion



        #region MOCK事件
        ///<summary> MOCK事件 </summary>
        public const string ON_MOCK_EVENT = "ON_MOCK_EVENT";
        ///<summary> 自定义输入框被点击 </summary>
        public const string PlayerAccountChange = "PlayerAccountChange";
        #endregion


        #region UI事件
        ///<summary> UI事件 </summary>
        public const string ON_UI_INPUT_EVENT = "ON_UI_INPUT_EVENT";
        ///<summary> 自定义输入框被点击 </summary>
        public const string CustomInputClick = "CustomInputClick";
        #endregion


        #region 页面事件
        ///<summary> 游戏事件 </summary>
        public const string ON_PAGE_EVENT = "ON_PAGE_EVENT";

        ///<summary> 置顶页面发生变化 </summary>
        public const string PageOnTopChange = "PageOnTopChange";

        #endregion



        #region 游戏事件
        ///<summary> 游戏事件 </summary>
        public const string ON_GAME_SLOT_EVENT = "ON_GAME_SLOT_EVENT";

        //public const string GameStart = "GameStart";
        //public const string GameIdle = "GameIdle";






        ///<summary> 游戏事件 </summary>
        public const string ON_GAME_COIN_PUSH_EVENT = "ON_GAME_COIN_PUSH_EVENT";

        ///<summary> 获取彩金显示值 </summary>
        public const string GetJackpotGameShow = "GetJackpotGameShow";


        #endregion




        #region 投退币事件
        ///<summary> 投退币事件 </summary>
        public const string ON_COIN_IN_OUT_EVENT = "ON_COIN_IN_OUT_EVENT";

        ///<summary> 退票完成 </summary>
        public const string CoinOutSuccess = "CoinOutSuccess";

        ///<summary> 退票错误 </summary>
        public const string CoinOutError = "CoinOutError";

        ///<summary> 清除所有本地订单缓存 </summary>
        public const string ClearAllOrderCache = "ClearAllOrderCache";






        ///<summary> 投幣完成 </summary>
        public const string CoinInCompleted = "CoinInCompleted";

        ///<summary> 退票完成 </summary>
        public const string CoinOutCompleted = "CoinOutCompleted";


        ///<summary> 二维码投币完成 </summary>
        public const string IOTCoinInCompleted = "IOTCoinInCompleted";
        ///<summary> 二维码退票完成 </summary>
        public const string IOTCoinOutCompleted = "IOTCoinOutCompleted";
        #endregion


        #region 硬件事件
        ///<summary> 驱动事件 </summary>
        public const string ON_DEVICE_EVENT = "ON_DEVICE_EVENT";
        ///<summary> 自定义输入框被点击 </summary>
        public const string ScanQRCode = "ScanQRCode";
        ///<summary> 打码完成 </summary>
        public const string CodeCompleted = "CodeCompleted";


        // 检查几天是否激活
        public const string CheckMachineActiveRepeat = "CheckMachineActiveRepeat";
        #endregion


        #region 后台彩金事件
        public const string ON_REMOTE_CONSOL_EVENT = "ON_REMOTE_CONSOL_EVENT";

        ///<summary> 获取配置完成 </summary>
        public const string GetRemoteConsoleConfigFinish = "GetRemoteConsoleConfigFinish";
        #endregion


        #region 钱箱事件
        ///<summary> 钱箱事件 </summary>
        public const string ON_MONEY_BOX_EVENT = "ON_MONEY_BOX_EVENT";
        ///<summary> 钱箱请求机台上分 </summary>
        public const string MoneyBoxRequestMachineQRCodeUp = "MoneyBoxRequestMachineQRCodeUp";
        #endregion


        #region 网络按钮控制
        public const string ON_MQTT_REMOTE_CONTROL_EVENT = "ON_MQTT_REMOTE_CONTROL_EVENT";
        #endregion





        /*
        #region 多语言
       
        public const string ON_I18N_EVENT = "ON_I18N_EVENT";
        public const string BefaultLanguageChange = "BefaultLanguageChange";
        #endregion
        */
    }

    /*
    public static partial class GlobalEvent
    {
        ///<summary> 游戏音效 </summary>
        public const string ON_GAME_SOUND_EVENT = "ON_GAME_SOUND_EVENT";
        ///<summary> 游戏背景音乐 </summary>
        public const string ON_GAME_MUSIC_EVENT = "ON_GAME_MUSIC_EVENT";
    }
    */


}