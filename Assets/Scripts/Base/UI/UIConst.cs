using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIConst
{
    private static UIConst _instance;

    public static UIConst Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIConst();
            }

            return _instance;
        }
    }


    public Dictionary<PageName, object[]> pathDict;

    public UIConst()
    {
        pathDict = new Dictionary<PageName, object[]>()
        {
            // 通用
            [PageName.CommonPopupSystemTip] =
                new object[] { "Assets/GameRes/Games/Common/FGUIs", "Common.PopupSystemTip" },

            // 拉霸机后台
            [PageName.ConsolePageConsoleMain] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleMain" },
            [PageName.ConsolePageConsoleMachineSettings] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleMachineSettings" },
            [PageName.ConsolePopupI18nTest] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupI18nTest" },
            [PageName.ConsolePageConsoleBusinessRecord] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleBusinessRecord" },
            [PageName.ConsolePageConsoleGameInformation] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleGameInformation" },
            [PageName.ConsolePopupConsoleKeyboard001] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleKeyboard001" },
            [PageName.ConsolePopupConsoleKeyboard002] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleKeyboard002" },
            [PageName.ConsolePopupConsoleTip] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleTip" },
            [PageName.ConsolePopupConsoleCommon] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleCommon" },
            [PageName.ConsolePopupConsoleSetParameter002] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleSetParameter002" },
            [PageName.ConsolePopupConsoleSetParameter001] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleSetParameter001" },
            [PageName.ConsolePopupConsoleCoder] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleCoder" },
            [PageName.ConsolePopupConsoleMask] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleMask" },
            [PageName.ConsolePopupConsoleSlideSetting] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleSlideSetting" },
            [PageName.ConsolePageDrawLine] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleDrawLine" },
            [PageName.ConsolePopupConsoleCalendar] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleCalendar" },
            [PageName.ConsolePopupConsoleSound] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleSound" },
            [PageName.ConsolePopupConsoleChoose001] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleChoose001" },
            [PageName.ConsolePageConsoleLogRecord] = new object[]
            {
                "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleLogRecord"
            },

            //平台
            [PageName.HallMain] = new object[] { "Assets/GameRes/Halls/TestHall/FGUIs", "TestHall.TestHallMain" },
            [PageName.Hall01] = new object[] { "Assets/GameRes/Halls/Hall01/FGUIs", "Hall01.Hall01GameMain" },

            // 推币机新后台
            [PageName.ConsolePusher01PageConsoleAdmin] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PageConsoleAdmin"
                },
            [PageName.ConsolePusher01PageConsoleMain] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PageConsoleMain"
                },
            [PageName.ConsolePusher01PageConsoleCheckHardware] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleCheckHardware"
                },
            [PageName.ConsolePusher01PageConsoleCoder] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PageConsoleCoder"
                },
            [PageName.ConsolePusher01PageConsoleSettings] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PageConsoleSettings"
                },
            [PageName.ConsolePusher01PageConsoleTestCoinPush] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleTestCoinPush"
                },
            [PageName.ConsolePusher01PageConsoleSetParameter002] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleSetParameter002"
                },
            [PageName.ConsolePusher01PageConsoleSetParameter001] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleSetParameter001"
                },
            [PageName.ConsolePusher01PageConsoleBusinessRecord] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleBusinessRecord"
                },
            [PageName.ConsolePusher01PageConsoleCheckHardware02] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleCheckHardware02"
                },
            [PageName.ConsolePusher01PageConsoleRecordChoose] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleRecordChoose"
                },
            [PageName.ConsolePusher01PageConsoleEventRecord] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleEventRecord"
                },
            [PageName.ConsolePusher01PageConsoleErrorRecord] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs",
                    "ConsoleCoinPusher01.PageConsoleErrorRecord"
                },
            [PageName.ConsolePusher01PopupConsoleRecord] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PopupConsoleRecord"
                },
            [PageName.ConsolePusher01PageConsoleJpRecord] =
                new object[]
                {
                    "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PageConsoleJpRecord"
                },
            [PageName.ConsolePusher01PageConsoleCoinRecord] = new object[]
            {
                "Assets/GameRes/Games/Console Coin Pusher 01/FGUIs", "ConsoleCoinPusher01.PageConsoleCoinRecord"
            },


            // 推币机帝国之辉
            [PageName.PusherEmperorsReinPopupERGameLoading] =
                new object[] { "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PopupGameLoading" },
            [PageName.PusherEmperorsReinPageERGameMain] =
                new object[] { "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PageGameMain" },
            [PageName.PusherEmperorsReinPopupBigWin] =
                new object[] { "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PopupBigWin" },
            [PageName.PusherEmperorsReinPopupFreeSpinTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PopupFreeSpinTrigger"
                },
            [PageName.PusherEmperorsReinPopupJackpotGame] =
                new object[] { "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PopupJackpotGame" },
            [PageName.PusherEmperorsReinPopupJackpotOnline] =
                new object[]
                {
                    "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PopupJackpotOnline"
                },
            [PageName.PusherEmperorsReinPopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "PusherEmperorsRein.PopupFreeSpinResult"
                },
            [PageName.PusherEmperorsReinPageFreeBonusGame2] = new object[]
            {
                "Assets/GameRes/Games/BonusGame2/FGUIs", "PusherEmperorsRein.PageFreeBonusGame2"
            },

            // 拉霸机帝国之辉
            [PageName.SlotEmperorsReinPageERGameMain] = new object[]
            {
                "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "SlotEmperorsRein.PageGameMainSlot"
            },
            //拉霸ckm测试
            [PageName.SlotCkmTestPageGameMain] =
                new object[]
                {
                    "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PageGameMain"
                }, //fgui的路径，项目命名空间.类名
            [PageName.SlotCkmTestPopupGameLoading] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupGameLoading" },
            [PageName.SlotCkmTestPopupBigWin] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupBigWin" },
            [PageName.SlotCkmTestPageBonusGame1] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PageBonusGame1" },
            [PageName.SlotCkmTestPageBonusGame2] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PageBonusGame2" },
            [PageName.SlotCkmTestPopupEnterBonusGame1] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupEnterBonusGame1" },
            [PageName.SlotCkmTestPopupEnterBonusGame2] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupEnterBonusGame2" },
            [PageName.SlotCkmTestPopupEnterFreeGame] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupEnterFreeGame" },
            [PageName.SlotCkmTestPopupQuitBonusGame1] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupQuitBonusGame1" },
            [PageName.SlotCkmTestPopupQuitBonusGame2] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupQuitBonusGame2" },
            [PageName.SlotCkmTestPopupQuitFreeGame] =
                new object[] { "Assets/GameRes/Games/Ckm Test 4001/FGUIs", "SlotCkmTest.PopupQuitFreeGame" },
            [PageName.SlotEmperorsReinPageFreeBonusGame1] = new object[]
            {
                "Assets/GameRes/Games/Emperors Rein 200/FGUIs", "slotEmperorsRein.PageFreeBonusGame1"
            },


            // 翻倍超人
            [PageName.SlotFanBeiChaoRenPageGameMain] =
                new object[]
                {
                    "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PageGameMain"
                },
            [PageName.SlotFanBeiChaoRenPopupLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PopupGameLoading"
                },
            [PageName.SlotFanBeiChaoRenPopupBigWin] =
                new object[]
                {
                    "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PopupGameBigWin"
                },
            [PageName.SlotFanBeiChaoRenGameXRay] =
                new object[]
                {
                    "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PageGameXRay"
                },
            [PageName.SlotFanBeiChaoRenPopupXRay] =
                new object[] { "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PopupXRay" },
            [PageName.SlotFanBeiChaoRenPopupFreeSpin] =
                new object[]
                {
                    "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PopupFreeSpinTrigger"
                },
            [PageName.SlotFanBeiChaoRenPopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PopupFreeSpinResult"
                },
            [PageName.SlotFanBeiChaoRenPopupXRayResultResult] = new object[]
            {
                "Assets/GameRes/Games/Fan Bei Chao Ren 4000/FGUIs", "SlotFanBeiChaoRen4000.PopupXRayResult"
            },

            //猪仔金币
            [PageName.SlotZhuZaiJinBiPopupGameLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/FGUIs", "SlotZhuZaiJinBi1700.PopupGameLoading"
                },
            [PageName.SlotZhuZaiJinBiPageGameMain] =
                new object[]
                {
                    "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/FGUIs", "SlotZhuZaiJinBi1700.PageGameMain"
                },
            [PageName.SlotZhuZaiJinBiPopupBigWin] =
                new object[]
                {
                    "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/FGUIs", "SlotZhuZaiJinBi1700.PopupBigWin"
                },
            [PageName.SlotZhuZaiJinBiPopupFreeSpinTrigger] = new object[]
            {
                "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/FGUIs", "SlotZhuZaiJinBi1700.PopupFreeSpinTrigger"
            },


            // 财富之门
            [PageName.CaiFuZhiMenPopupGameLoading] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupGameLoading" },
            [PageName.CaiFuZhiMenPageGameMain] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PageGameMain" },
            [PageName.CaiFuZhiMenPopupFreeSpinTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupFreeSpinTrigger"
                },
            [PageName.CaiFuZhiMenPopupBigWin] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupBigWin" },
            [PageName.CaiFuZhiMenPopupJackpotGame] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotGame" },
            [PageName.CaiFuZhiMenPopupJackpotGameResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotGameResult"
                },
            [PageName.CaiFuZhiMenPopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupFreeSpinResult"
                },
            [PageName.CaiFuZhiMenPopupJackpotGameTrigger] = new object[]
            {
                "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotGameTrigger"
            },

            // 财富之家
            [PageName.CaiFuZhiJiaPopupGameLoading] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupGameLoading" },
            [PageName.CaiFuZhiJiaPageGameMain] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PageGameMain" },
            [PageName.CaiFuZhiJiaPopupFreeSpinTrigger] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupFreeSpinTrigger" },
            [PageName.CaiFuZhiJiaPopupFreeSpinResult] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupFreeSpinResult" },
            [PageName.CaiFuZhiJiaPopupJackpotTrigger] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupJackpotTrigger" },
            [PageName.CaiFuZhiJiaPopupJackpotResult] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupJackpotResult" },
            [PageName.CaiFuZhiJiaPopupJackpotGame] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupJackpotGame" },
            
            //幸运之轮
            [PageName.XingYunZhiLunPopupGameLoading] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupGameLoading" },
            [PageName.XingYunZhiLunPageGameMain] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PageGameMain" },
            [PageName.XingYunZhiLunPopupJackpotGameResult] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameResult" },
            [PageName.XingYunZhiLunPopupFreeSpinTrigger] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupFreeSpinTrigger" },
            [PageName.XingYunZhiLunPopupFreeSpinResult] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupFreeSpinResult" },
            [PageName.XingYunZhiLunPopupJackpotGameTrigger] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameTrigger" },
            [PageName.XingYunZhiLunPopupJackpotGameExit] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameExit" },
            [PageName.XingYunZhiLunPopupJackpotGameEnter] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameEnter" },
            [PageName.XingYunZhiLunPopupJackpotGameQuit] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameQuit" },
            [PageName.XingYunZhiLunPopupZhuanPan] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupZhuanPan" },

            //财富火车
            [PageName.CaiFuHuoChePopupGameLoading] = new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupGameLoading" },
            [PageName.CaiFuHuoChePopupFreeSpinTrigger] = new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupFreeSpinTrigger" },
            [PageName.CaiFuHuoChePopupJackpotGameTrigger] = new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupJackpotGameTrigger" },
            [PageName.CaiFuHuoChePopupJackpotGameExit] = new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupJackpotGameExit" },
            [PageName.CaiFuHuoChePopupFreeSpinResult] = new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupFreeSpinResult" },
            [PageName.CaiFuHuoChePageGameMain] = new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PageGameMain" },

            [PageName.XingYunZhiLunPopupGameLoading] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupGameLoading" },
            [PageName.XingYunZhiLunPageGameMain] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PageGameMain" },
            [PageName.XingYunZhiLunPopupJackpotGameResult] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameResult" },
            [PageName.XingYunZhiLunPopupFreeSpinTrigger] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupFreeSpinTrigger" },
            [PageName.XingYunZhiLunPopupFreeSpinResult] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupFreeSpinResult" },
            [PageName.XingYunZhiLunPopupJackpotGameTrigger] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameTrigger" },
            [PageName.XingYunZhiLunPopupJackpotGameExit] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameExit" },
            [PageName.XingYunZhiLunPopupJackpotGameEnter] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameEnter" },
            [PageName.XingYunZhiLunPopupJackpotGameQuit] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameQuit" },
            [PageName.XingYunZhiLunPopupZhuanPan] = new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupZhuanPan" },

            [PageName.XingYunZhiLunPopupGameLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupGameLoading"
                },
            [PageName.XingYunZhiLunPageGameMain] =
                new object[] { "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PageGameMain" },
            [PageName.XingYunZhiLunPopupJackpotGameResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameResult"
                },
            [PageName.XingYunZhiLunPopupFreeSpinTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupFreeSpinTrigger"
                },
            [PageName.XingYunZhiLunPopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupFreeSpinResult"
                },
            [PageName.XingYunZhiLunPopupJackpotGameTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameTrigger"
                },
            [PageName.XingYunZhiLunPopupJackpotGameExit] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameExit"
                },
            [PageName.XingYunZhiLunPopupJackpotGameEnter] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameEnter"
                },
            [PageName.XingYunZhiLunPopupJackpotGameQuit] =
                new object[]
                {
                    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupJackpotGameQuit"
                },
            [PageName.XingYunZhiLunPopupZhuanPan] = new object[]
            {
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupZhuanPan"
            }
        };
    }
}


public enum PageName
{
    // 通用
    CommonPopupSystemTip,


    // 拉霸机-管理后天
    ConsolePageConsoleMain,
    ConsolePageConsoleMachineSettings,
    ConsolePopupI18nTest,
    ConsolePageConsoleBusinessRecord,
    ConsolePageConsoleGameInformation,
    ConsolePopupConsoleKeyboard001,
    ConsolePopupConsoleKeyboard002,
    ConsolePopupConsoleTip,
    ConsolePopupConsoleCommon,
    ConsolePopupConsoleSetParameter002,
    ConsolePopupConsoleSetParameter001,
    ConsolePopupConsoleCoder,
    ConsolePopupConsoleMask,
    ConsolePopupConsoleSlideSetting,
    ConsolePageDrawLine,
    ConsolePopupConsoleCalendar,
    ConsolePopupConsoleSound,
    ConsolePopupConsoleChoose001,
    ConsolePageConsoleLogRecord,


    HallMain,
    Hall01,

    // 推币机新后台
    ConsolePusher01PageConsoleMain,
    ConsolePusher01PageConsoleCheckHardware,
    ConsolePusher01PageConsoleCoder,
    ConsolePusher01PageConsoleSettings,
    ConsolePusher01PageConsoleTestCoinPush,
    ConsolePusher01PageConsoleSetParameter001,
    ConsolePusher01PageConsoleSetParameter002,
    ConsolePusher01PageConsoleBusinessRecord,
    ConsolePusher01PageConsoleAdmin,
    ConsolePusher01PageConsoleCheckHardware02,
    ConsolePusher01PageConsoleRecordChoose,
    ConsolePusher01PageConsoleEventRecord,
    ConsolePusher01PageConsoleErrorRecord,
    ConsolePusher01PopupConsoleRecord,
    ConsolePusher01PageConsoleJpRecord,
    ConsolePusher01PageConsoleCoinRecord,


    // 推币机-帝国之辉
    PusherEmperorsReinPageERGameMain,
    PusherEmperorsReinPopupERGameLoading,
    PusherEmperorsReinPopupBigWin,
    PusherEmperorsReinPopupFreeSpinTrigger,
    PusherEmperorsReinPopupFreeSpinResult,
    PusherEmperorsReinPopupJackpotGame,

    PusherEmperorsReinPopupJackpotOnline,
    PusherEmperorsReinPageFreeBonusGame2,

    // 拉霸机-帝国之辉
    SlotEmperorsReinPageERGameMain,

    //拉霸机-CkmTest
    SlotCkmTestPageGameMain,
    SlotCkmTestPopupGameLoading,
    SlotCkmTestPopupBigWin,
    SlotCkmTestPageBonusGame2,
    SlotCkmTestPageBonusGame1,
    SlotCkmTestPopupEnterBonusGame1,
    SlotCkmTestPopupEnterBonusGame2,
    SlotCkmTestPopupEnterFreeGame,
    SlotCkmTestPopupQuitBonusGame1,
    SlotCkmTestPopupQuitBonusGame2,
    SlotCkmTestPopupQuitFreeGame,

    SlotEmperorsReinPageFreeBonusGame1,


    // 翻倍超人
    SlotFanBeiChaoRenPageGameMain,
    SlotFanBeiChaoRenPopupLoading,
    SlotFanBeiChaoRenPopupBigWin,
    SlotFanBeiChaoRenGameXRay,
    SlotFanBeiChaoRenPopupXRay,
    SlotFanBeiChaoRenPopupFreeSpin,
    SlotFanBeiChaoRenPopupFreeSpinResult,
    SlotFanBeiChaoRenPopupXRayResultResult,

    //猪仔金币
    SlotZhuZaiJinBiPopupGameLoading,
    SlotZhuZaiJinBiPageGameMain,
    SlotZhuZaiJinBiPopupBigWin,
    SlotZhuZaiJinBiPopupFreeSpinTrigger,

    // 财富之门
    CaiFuZhiMenPopupGameLoading,
    CaiFuZhiMenPageGameMain,
    CaiFuZhiMenPopupBigWin,
    CaiFuZhiMenPopupFreeSpinTrigger,
    CaiFuZhiMenPopupJackpotGame,
    CaiFuZhiMenPopupJackpotGameResult,
    CaiFuZhiMenPopupFreeSpinResult,
    CaiFuZhiMenPopupJackpotGameTrigger,

    // 财富之家
    CaiFuZhiJiaPopupGameLoading,
    CaiFuZhiJiaPageGameMain,
    CaiFuZhiJiaPopupFreeSpinTrigger,
    CaiFuZhiJiaPopupFreeSpinResult,
    CaiFuZhiJiaPopupJackpotTrigger,
    CaiFuZhiJiaPopupJackpotResult,
    CaiFuZhiJiaPopupJackpotGame,

    //幸运之轮
    XingYunZhiLunPopupGameLoading,
    XingYunZhiLunPageGameMain,
    XingYunZhiLunPopupJackpotGameResult,
    XingYunZhiLunPopupFreeSpinTrigger,
    XingYunZhiLunPopupFreeSpinResult,
    XingYunZhiLunPopupJackpotGameTrigger,
    XingYunZhiLunPopupJackpotGameExit,
    XingYunZhiLunPopupJackpotGameEnter,
    XingYunZhiLunPopupJackpotGameQuit,
    XingYunZhiLunPopupZhuanPan,

    //财富火车
    CaiFuHuoChePopupGameLoading,
    CaiFuHuoChePopupFreeSpinTrigger,
    CaiFuHuoChePopupJackpotGameTrigger,
    CaiFuHuoChePopupJackpotGameExit,
    CaiFuHuoChePopupFreeSpinResult,
    CaiFuHuoChePageGameMain,
}