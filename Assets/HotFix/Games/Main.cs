using FairyGUI;
using UnityEngine;
using System.Collections.Generic;
using GameMaker;
using System.Timers;
using System.Collections;
using System;

public class Main
{

    static PreloadAssetBundlesHelper preloadAB = new PreloadAssetBundlesHelper()
    {
        markBundle = "MARK_BUNDLE_MAIN",
        preloadBundleNames = new List<object[]>()
        {
            // 游戏
            //UIConst.Instance.pathDict[PageName.EmperorsReinPageERGameMain],
        },

        preloadAssetAtPath = new List<object[]>()
        {
            // 控台
            //UIConst.Instance.pathDict[PageName.ConsolePageConsoleMain],
            //UIConst.Instance.pathDict[PageName.ConsolePageConsoleMachineSettings],
            //UIConst.Instance.pathDict[PageName.ConsolePopupI18nTest],
            //UIConst.Instance.pathDict[PageName.ConsolePageConsoleBusinessRecord],
            //UIConst.Instance.pathDict[PageName.ConsolePageConsoleGameInformation],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleKeyboard001],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleKeyboard002],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleTip],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleCommon],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleSetParameter002],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleSetParameter001],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleCoder],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleMask],
            //UIConst.Instance.pathDict[PageName.ConsolePopupConsoleSlideSetting],
        },

    };

    static PreloadAssetBundlesHelper preloadABBackground = new PreloadAssetBundlesHelper()
    {
        markBundle = "MARK_BUNDLE_MAIN",
        preloadBundleNames = new List<object[]>()
        {
        },
        preloadAssetAtPath = new List<object[]>()
        {

        },

    };
    public static void MainStart()
    {
        CoroutineAssistant.DoCo("COR_ON_BEFORE_PRELOAD", OnBeforePreLoadBundle());
    }

    static IEnumerator OnBeforePreLoadBundle()
    {
        // 预加载前:
        PageLaunch.Instance.RefreshProgressUIMsg("on before preload bundle");

        while (!SQLitePlayerPrefs03.Instance.isInit)
        {
            yield return null;
        }
        while (!SQLiteAsyncHelper.Instance.isInit)
        {
            yield return null;
        }

        yield return null;

        PreLoadBundle();
    }

    /// <summary>
    /// 预加载
    /// </summary>
    private static void PreLoadBundle()
    {
        int preloadCount = preloadAB.preloadBundleNames.Count;
        PageLaunch.Instance.AddProgressCount(LoadingProgress.PRELOAD_ASSET_BUNDLE, preloadCount);
        DebugUtils.Log($"【内存预加载】ab包个数： {preloadCount}");

        if (!Application.isEditor) // 【？】这里要换成 ApplicationSettings.Instance.IsUseHotfix()
        {
            preloadAB.LoadPreloadBundleAsync((msg) =>
            {
                PageLaunch.Instance.Next(LoadingProgress.PRELOAD_ASSET_BUNDLE, msg);
            },
            () =>
            {
                PreLoadAsset();
            });
        }
        else
        {
            PreLoadAsset();
        }
    }
    /// <summary>
    /// 预加载资源，并等待完成
    /// </summary>
    private static void PreLoadAsset()
    {
        PageLaunch.Instance.RemoveProgress(LoadingProgress.PRELOAD_ASSET_BUNDLE);

        int preloadCount = preloadAB.preloadAssetAtPath.Count;
        PageLaunch.Instance.AddProgressCount(LoadingProgress.PRELOAD_ASSET, preloadCount);
        DebugUtils.Log($"【内存预加载】资源个数： {preloadCount}");

        if (!Application.isEditor) // 【？】这里要换成 ApplicationSettings.Instance.IsUseHotfix()
        {
            preloadAB.LoadPreloadAssetAsync((msg) =>
            {
                PageLaunch.Instance.Next(LoadingProgress.PRELOAD_ASSET, msg);
            },
            () =>
            {
                PreLoadBackboard();
                ConnectHardward();
            });
        }
        else
        {
            ConnectHardward();
        }
    }

    /// <summary>
    /// 后台预加载
    /// </summary>
    private static void PreLoadBackboard()
    {

        if (!Application.isEditor) // 【？】这里要换成 ApplicationSettings.Instance.IsUseHotfix()
        {
            preloadABBackground.LoadPreloadBundleAsync(null,
            () =>
            {
                preloadABBackground.LoadPreloadAssetAsync(null,
                () =>
                {
                    DebugUtils.Log("【PreLoad】： 后台加载ab包、资源，完成");
                });
            });
        }
    }
    private static void ShowPlamtInfo()
    {
        DebugUtils.LogWarning($"平台:{ApplicationSettings.Instance.platformName}; 版本:{ApplicationSettings.Instance.appVersion}; 是否机台包:{ApplicationSettings.Instance.isMachine}; 热更新版本:{"--"}");
    }

    private static void ConnectHardward()
    {

        PageLaunch.Instance.RemoveProgress(LoadingProgress.PRELOAD_ASSET_BUNDLE);
        PageLaunch.Instance.RemoveProgress(LoadingProgress.PRELOAD_ASSET);

        ShowPlamtInfo();
        //DebugUtil.Log( "<color=red>" + "检查是否是机台..." +"</color>");

        if (ApplicationSettings.Instance.isMachine)
        {

            PageLaunch.Instance.AddProgressCount(LoadingProgress.CONNECT_MACHINE, 2);
            PageLaunch.Instance.Next(LoadingProgress.CONNECT_MACHINE, $"connect machine: {ApplicationSettings.Instance.machineDebugUrl} ...");
            DebugUtils.LogWarning($"链接机台({ApplicationSettings.Instance.machineDebugUrl}), 初始化硬件...");
            /*
            if (Application.isEditor) {
                string IP = matchIp;
                if (matchIp.Contains(":"))
                {
                    IP = matchIp.Split(':')[0];
                    string Port = matchIp.Split(':')[1];
                    MatchDebugManager.Instance.port = int.Parse(Port);
                }
                MatchDebugManager.Instance.InitUdpNet(IP);
            }
            */
            //SBoxSandboxInit.Instance.Init(ApplicationSettings.Instance.machineDebugUrl, () =>
            SBoxInit.Instance.Init(ApplicationSettings.Instance.machineDebugUrl, () =>
            {
                DebugUtils.LogWarning("机台 链接成功...");

                InitSettings();//OpenGame();
            });
        }
        else
        {

            InitSettings();//OpenGame();
        }

    }




    #region 初始化参数
    static System.Timers.Timer checkTimer;

    static void ClearTimerInitSettings()
    {
        if (checkTimer != null)
        {
            checkTimer.Stop();
            checkTimer.Dispose();
            checkTimer = null;
        }
    }
    static void DelayCheckSettings()
    {
        ClearTimerInitSettings();

        float ms = 2000f;
        checkTimer = new System.Timers.Timer(ms);
        checkTimer.AutoReset = false; // 是否重复执行
        checkTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
        {
            Loom.QueueOnMainThread((data) =>
            {
                OnInitSettingFinish();
            }, null);
        };
        checkTimer.Start();
    }

    static void InitSettings()
    {
        PageLaunch.Instance.RemoveProgress(LoadingProgress.CONNECT_MACHINE);

        PageLaunch.Instance.AddProgressCount(LoadingProgress.INIT_SETTINGS, 0);
        //PageLaunch.Instance.AddCountToProgress(LoadingProgress.INIT_SETTINGS,1);
        //PageLaunch.Instance.Next(LoadingProgress.INIT_SETTINGS, "msg");

        totalInitCount = 0;
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_INIT_SETTINGS_EVENT, OnInitSettingsEvent);
        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_INIT_SETTINGS_EVENT, OnInitSettingsEvent);
        // 获取设置参数配置
        GameObject pagePrefab =
            ResourceManager.Instance.LoadAssetAtPathOnce<GameObject>("Assets/GameRes/_Common/Game Maker/Prefabs/INSTANCE.prefab");
        pagePrefab.name = "INSTANCE";

        DelayCheckSettings();
    }

    //list - number - msg
    static void OnInitSettingsEvent(EventData res)
    {
        if (res.name == GlobalEvent.AddSettingsCount)
        {
            int count = (int)res.value;
            totalInitCount += count;
            PageLaunch.Instance.AddProgressCount(LoadingProgress.INIT_SETTINGS, count);
        }
        else if (res.name == GlobalEvent.InitSettings)
        {
            totalInitCount--;
            PageLaunch.Instance.Next(LoadingProgress.INIT_SETTINGS, (string)res.value);
        }
        else if (res.name == GlobalEvent.RefreshProgressMsg)
        {
            PageLaunch.Instance.RefreshProgressUIMsg((string)res.value);
        }

        DelayCheckSettings();
    }

    /// <summary> 初始化总个数 </summary>
    static int totalInitCount;

    static void OnInitSettingFinish()
    {

        if (totalInitCount > 0)
        {
            //PageLaunch.Instance.Next(LoadingProgress.INIT_SETTINGS,"init settings error!");
            DebugUtils.LogError("参数初始化失败！！！");
            return;
        }
        DebugUtils.LogWarning("参数初始化成功！！！");
        //DebugUtils.LogError("参数初始化成功！！！");

        #region 参数获取成功后
        TestManager.Instance.Init($"Ver {ApplicationSettings.Instance.appVersion}/{GlobalData.hotfixVersion}");
        TestUtils.CheckTestManager();
        TestUtils.CheckReporter();



        /*##
        MachineDeviceCommonBiz.Instance.CheckSas();

        MachineDeviceCommonBiz.Instance.CheckMoneyBox();

        MachineDeviceCommonBiz.Instance.CheckIOT();

        MachineDeviceCommonBiz.Instance.CheckMqttRemoteButtonController();

        MachineDeviceCommonBiz.Instance.CheckBonusReport();

        MachineDeviceCommonBiz.Instance.SetGameLevel();
        */


        MachineDeviceCommonBiz.Instance.CheckMqttRemoteButtonController();


        //NetMgr.Instance.SetNetAutoConnect(true);


        DebugUtils.SetOpenDebugLog(SBoxModel.Instance.isDebugLog);



        // 打开FGUI鼠标功能
        //if(!ApplicationSettings.Instance.isRelease)  Stage.touchScreen = false;
        Stage.touchScreen = false;

        #endregion
        EventCenter.Instance.EventTrigger(GlobalEvent.ON_INIT_SETTINGS_FINISH_EVENT);

        OpenGame();
    }

    static void OpenGame()
    {

        PageLaunch.Instance.RemoveProgress(LoadingProgress.INIT_SETTINGS);

        //PageLaunch.Instance.AddProgress(LoadingProgress.ENTER_GAME,0);
        //PageLaunch.Instance.Next(LoadingProgress.ENTER_GAME,"enter game");
        PageLaunch.Instance.RemoveProgress(LoadingProgress.ENTER_GAME);
        PageLaunch.Instance.Finish("enter game");
        // 预加载 login 页 ？？
        PageLaunch.Instance.Close(2f);


        // 游戏加载页面：
        //PageManager.Instance.OpenPage(PageName.PusherEmperorsReinPopupERGameLoading);
        //PageManager.Instance.OpenPage(PageName.SlotFanBeiChaoRenPopupLoading);
        //PageManager.Instance.OpenPage(PageName.XingYunZhiLunPopupGameLoading);
        if (!ApplicationSettings.Instance.isMock)
        {
            PageManager.Instance.OpenPage(PageName.Hall01);
        }
        else
        {
            PageManager.Instance.OpenPage(PageName.HallMain);
        }
           
        /*
        System.Action onJPPoolSubCredit = () => {
            DebugUtils.Log("i am here123");
        };
        Dictionary<string, object> args = new Dictionary<string, object>()
        {
            ["jackpotType"] = "grand",
            ["totalEarnCredit"] = 1000,
            ["onJPPoolSubCredit"] = onJPPoolSubCredit,
        };
        PageManager.Instance.OpenPage(PageName.EmperorsReinPopupGameJackpot,new EventData<Dictionary<string, object>>("", args));
        */


        /*
        PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard001,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = "Enter Password",
                    ["isPlaintext"] = false,
                }),
            
            (res) =>
            {
                DebugUtils.Log($"回调执行！res: {res.value}"); // 加日志

            }

            );*/
        /*
        PageManager.Instance.OpenPageAsync(PageName.EmperorsReinPopupFreeSpinTrigger,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["freeSpinCount"] = 77,
                }),
            (res) =>
            {
                DebugUtils.Log($"回调执行！Result: {res.value} "); // 加日志

            });
        */
    }

    #endregion


}
