using FairyGUI;
using UnityEngine;
using GameMaker;
using System.Timers;
using System.Collections;
using System;

public class Main
{
    public static void MainStart()
    {
        PagBootstrap.EnsureReady();
        ThemeRuntime.SelectFromSettings();
        CoroutineAssistant.DoCo("COR_ON_BEFORE_PRELOAD", OnBeforeConnectHardware());
    }

    /// <summary>
    /// 预载主题大厅页，等待 SQLite 就绪后连接机台/初始化。
    /// AB 预载已由主题 Entry.OpenHall（Loading 预载）承担，不再走 preloadAB 空列表。
    /// </summary>
    static IEnumerator OnBeforeConnectHardware()
    {
        PageLaunch.Instance.RefreshProgressUIMsg("on before connect hardware");
        if (ThemeRuntime.HasCurrent)
            ThemeRuntime.Current.PreloadHall();
        else
            PageManager.Instance.PreloadPage(ThemeRuntime.Profile.HallPageName, null);

        while (!SQLitePlayerPrefs03.Instance.isInit)
        {
            yield return null;
        }

        while (!SQLiteAsyncHelper.Instance.isInit)
        {
            yield return null;
        }

        yield return null;

        ConnectHardward();
    }

    private static void ShowPlamtInfo()
    {
        DebugUtils.LogWarning(
            $"平台:{ApplicationSettings.Instance.platformName}; 版本:{ApplicationSettings.Instance.appVersion}; 是否机台包:{ApplicationSettings.Instance.isMachine}; 热更新版本:{"--"}");
    }

    private static void ConnectHardward()
    {
        ShowPlamtInfo();

        if (ApplicationSettings.Instance.isMachine)
        {
            PageLaunch.Instance.AddProgressCount(LoadingProgress.CONNECT_MACHINE, 2);
            PageLaunch.Instance.Next(LoadingProgress.CONNECT_MACHINE,
                $"connect machine: {ApplicationSettings.Instance.machineDebugUrl} ...");
            DebugUtils.LogWarning($"链接机台({ApplicationSettings.Instance.machineDebugUrl}), 初始化硬件...");
            SBoxInit.Instance.Init(ApplicationSettings.Instance.machineDebugUrl, () =>
            {
                DebugUtils.LogWarning("机台 链接成功...");

                InitSettings();
            });
        }
        else
        {
            InitSettings();
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

        totalInitCount = 0;
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_INIT_SETTINGS_EVENT, OnInitSettingsEvent);
        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_INIT_SETTINGS_EVENT, OnInitSettingsEvent);
        // 获取设置参数配置
        GameObject pagePrefab =
            ResourceManager.Instance.LoadAssetAtPathOnce<GameObject>(
                "Assets/GameRes/_Common/Game Maker/Prefabs/INSTANCE.prefab");
        pagePrefab.name = "INSTANCE";

        DelayCheckSettings();
    }

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
            DebugUtils.LogError("参数初始化失败！！！");
            return;
        }

        DebugUtils.LogWarning("参数初始化成功！！！");

        #region 参数获取成功后

        // 机台语言就绪后刷新 Savage 启动页 Logo 中/英粒子
        //PageLaunch.Instance.RefreshSavageLogoByLanguage(SBoxModel.Instance.language);

        TestManager.Instance.Init($"Ver {ApplicationSettings.Instance.appVersion}/{GlobalData.hotfixVersion}");
        TestUtils.CheckTestManager();
        TestUtils.CheckGCMonitorPro();
        TestUtils.CheckReporter();

        // 所有包关闭 MQTT 远程控制，避免外网 broker TCP 超时导致周期卡死
        MachineDeviceCommonBiz.Instance.CheckMqttRemoteButtonController();

        NetMessageController.Instance.Init();

        if (SBoxModel.Instance.isJackpotOnLine)
            NetMgr.Instance.SetNetAutoConnect(false);

        DebugUtils.SetOpenDebugLog(SBoxModel.Instance.isDebugLog);

        Stage.touchScreen = false;

        #endregion

        EventCenter.Instance.EventTrigger(GlobalEvent.ON_INIT_SETTINGS_FINISH_EVENT);

        OpenGame();
    }

    static void OpenGame()
    {
        PageLaunch.Instance.RemoveProgress(LoadingProgress.INIT_SETTINGS);
        PageLaunch.Instance.RemoveProgress(LoadingProgress.ENTER_GAME);
        PageLaunch.Instance.Finish("enter game");

        if (!ThemeRuntime.HasCurrent)
        {
            DebugUtils.LogError($"[Main] ThemeRuntime 无当前 IThemeEntry: {ThemeRuntime.SelectedKind}，无法打开大厅");
            return;
        }

        ThemeRuntime.Current.OpenHall();
    }

    #endregion
}
