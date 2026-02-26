using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// * PlayerPrefsUtils定义的，在AOT中可以调用到<br/>
/// * 相比于PlayerPrefsUtils，在Blackboard对象定义的字段，会有值变事件发出。<br/>
/// * 但是使用Blackboard，必须等到Blackboard初始化结束后才能调用。
/// </remarks>
public static class PlayerPrefsUtils
{
    /// <summary> 是否链接Sas </summary>
    public static bool isUseSas
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式版先不放出去
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_USE_SAS, 0);
            return enable != 0;
        }
        set
        {
            PlayerPrefs.DeleteKey("PARAM_IS_CONNECT_SAS");
            PlayerPrefs.SetInt(PARAM_IS_USE_SAS, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    //const string PARAM_IS_CONNECT_SAS = "PARAM_IS_CONNECT_SAS";
    const string PARAM_IS_USE_SAS = "PARAM_IS_USE_SAS";

    /*
    /// <summary> 是否链接钱箱 </summary>
    public static bool isConnectMoneyBox
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式版先不放出去
                return false;


            int enable = PlayerPrefs.GetInt(PARAM_IS_CONNECT_MONEY_BOX, 0);
            return enable != 0;
        }
        set
        {
            PlayerPrefs.SetInt(PARAM_IS_CONNECT_MONEY_BOX, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_CONNECT_MONEY_BOX = "PARAM_IS_CONNECT_MONEY_BOX";

    */

    /// <summary> 是否多次检查热更新 </summary>
    public static bool isCheckHotfixMultipleTimes
    {
        get
        {
            int enable = PlayerPrefs.GetInt(GlobalData.HOTFIX_REQUEST_COUNT_01, 1);
            return enable > 1;
        }
        set
        {
            PlayerPrefs.SetInt(GlobalData.HOTFIX_REQUEST_COUNT_01, value ? 10 : 1);
            PlayerPrefs.Save();
        }
    }


    /// <summary> 是否使用正式版的好酷 </summary>
    /// <remarks>
    /// * 正式包，肯定是用正式服的好酷。 <br>
    /// * 测试包才允许测试正式服好酷的功能。<br>
    /// </remarks>
    public static bool isUseReleaseIot
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包，肯定是用正式服的好酷。 
                return true;

            // 测试包，允许使用正式服的好酷。
            int enable = PlayerPrefs.GetInt(PARAM_IS_TEST_RELEASE_IOT, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_TEST_RELEASE_IOT, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_TEST_RELEASE_IOT = "PARAM_IS_TEST_RELEASE_IOT";






    /// <summary> 暂停免费游戏结束弹窗 </summary>
    public static bool isPauseAtPopupFreeSpinResult
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_RESULT, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_RESULT, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_RESULT = "PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_RESULT";




    /// <summary> 暂停免费游戏触发弹窗 </summary>
    public static bool isPauseAtPopupFreeSpinTrigger
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_TRIGGER, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_TRIGGER, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_TRIGGER = "PARAM_IS_PAUSE_AT_POPUP_FREE_SPIN_TRIGGER";


    /// <summary> 暂停游戏彩金弹窗 </summary>
    public static bool isPauseAtPopupJackpotGame
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_PAUSE_AT_POPUP_JACKPOT_GAME, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_PAUSE_AT_POPUP_JACKPOT_GAME, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_PAUSE_AT_POPUP_JACKPOT_GAME = "PARAM_IS_PAUSE_AT_POPUP_JACKPOT_GAME";



    /// <summary> 暂停联网彩金弹窗 </summary>
    public static bool isPauseAtPopupJackpotOnline
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_PAUSE_AT_POPUP_JACKPOT_ONLINE, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_PAUSE_AT_POPUP_JACKPOT_ONLINE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_PAUSE_AT_POPUP_JACKPOT_ONLINE = "PARAM_IS_PAUSE_AT_POPUP_JACKPOT_ONLINE";




    /*

    /// <summary> 是否打印调试日志 </summary>
    public static bool isDebugLog
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_DEBUG_LOG, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;
           
            PlayerPrefs.SetInt(PARAM_IS_DEBUG_LOG, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_DEBUG_LOG = "PARAM_IS_DEBUG_LOG";



    /// <summary> 是否使用调试工具 </summary>
    public static bool isUseTestTool
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_USE_TEST_TOOL, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_USE_TEST_TOOL, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_USE_TEST_TOOL = "PARAM_IS_USE_TEST_TOOL";



    /// <summary> 是否使用调试页面 </summary>
    public static bool isUseReporterPage
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return false;


            int enable = PlayerPrefs.GetInt(PARAM_IS_USE_REPORTER_PAGE, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_USE_REPORTER_PAGE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_USE_REPORTER_PAGE = "PARAM_IS_USE_REPORTER_PAGE";

    */


    /// <summary> 是否使用调试页面 </summary>
    public static bool isUseAllConsolePage
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return false;


            int enable = PlayerPrefs.GetInt(PARAM_IS_USE_ALL_CONSOLE_PAGE, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_USE_ALL_CONSOLE_PAGE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_USE_ALL_CONSOLE_PAGE = "PARAM_IS_USE_ALL_CONSOLE_PAGE";





    /// <summary> 暂停联网彩金弹窗 </summary>
    public static bool isPauseAtPopupGameLoadingOnce
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_PAUSE_AT_POPUP_GAME_LOADING_ONCE, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_PAUSE_AT_POPUP_GAME_LOADING_ONCE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_PAUSE_AT_POPUP_GAME_LOADING_ONCE = "PARAM_IS_PAUSE_AT_POPUP_GAME_LOADING_ONCE";




    /// <summary> 删除游戏操作界面 </summary>
    public static bool isTestDeletePanel
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_TEST_DELETE_PANEL, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_TEST_DELETE_PANEL, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_TEST_DELETE_PANEL = "PARAM_IS_TEST_DELETE_PANEL";


    /// <summary> 删除游戏管轮组 </summary>
    public static bool isTestDeleteReels
    {
        get
        {
            if (ApplicationSettings.Instance.isRelease)  //正式包
                return false;

            int enable = PlayerPrefs.GetInt(PARAM_IS_TEST_DELETE_REELS, 0);
            return enable != 0;
        }
        set
        {
            if (ApplicationSettings.Instance.isRelease)  //生产环境
                return;

            PlayerPrefs.SetInt(PARAM_IS_TEST_DELETE_REELS, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    const string PARAM_IS_TEST_DELETE_REELS = "PARAM_IS_TEST_DELETE_REELS";




}
