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
            [PageName.ConsolePageConsoleLogRecord] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleLogRecord" },
            [PageName.ConsolePageConsoleGameHistory] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleGameHistory" },
            [PageName.ConsolePageConsoleHardware] =
                new object[] { "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PageConsoleHardware" },
            [PageName.ConsolePopupConsoleScreenColor] = new object[]
            {
                "Assets/GameRes/Games/Console/FGUIs", "ConsoleSlot01.PopupConsoleScreenColor"
            },


            //大厅
            [PageName.Hall01] = new object[] { "Assets/GameRes/Halls/Hall01/FGUIs", "Hall01.Hall01GameMain" },
            [PageName.TreasuryHallMain] =
                new object[] { "Assets/GameRes/Halls/TreasuryHall/FGUIs", "TreasuryHall.TreasuryHallMain" },


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
            [PageName.CaiFuZhiMenPopupJackpotGame] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotGame" },
            [PageName.CaiFuZhiMenPopupJackpotResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotResult"
                },
            [PageName.CaiFuZhiMenPopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupFreeSpinResult"
                },
            [PageName.CaiFuZhiMenPopupJackpotTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotTrigger"
                },
            [PageName.CaiFuZhiMenPopupJackpotLoad] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupJackpotLoad" },
            [PageName.CaiFuZhiMenPopupOnlineJackpot] = new object[]
            {
                "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs", "CaiFuZhiMen_3999.PopupOnlineJackpot"
            },
            
            // 财富之家 3997
            [PageName.CaiFuZhiJiaPopupGameLoading] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupGameLoading" },
            [PageName.CaiFuZhiJiaPageGameMain] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PageGameMain" },
            [PageName.CaiFuZhiJiaPopupOverWin] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupOverWin" },
            [PageName.CaiFuZhiJiaPopupFreeSpinTrigger] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupFreeSpinTrigger" },
            [PageName.CaiFuZhiJiaPopupFreeSpinResult] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupFreeSpinResult" },
            [PageName.CaiFuZhiJiaPopupSmallGameTrigger] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupSmallGameTrigger" },
            [PageName.CaiFuZhiJiaPopupSmallGameResult] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupSmallGameResult" },
            [PageName.CaiFuZhiJiaPopupJackpotWin] =
                new object[] { "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", "CaiFuZhiJia_3997.PopupJackpotWin" },

            // 美洲黑豹
            [PageName.MeiZhouHeiBaoPopupGameLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupGameLoading"
                },
            [PageName.MeiZhouHeiBaoPageGameMain] =
                new object[] { "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PageGameMain" },
            [PageName.MeiZhouHeiBaoPopupFreeSpinTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupFreeSpinTrigger"
                },
            [PageName.MeiZhouHeiBaoPopupFreeGameLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupFreeGameLoading"
                },
            [PageName.MeiZhouHeiBaoPopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupFreeSpinResult"
                },
            [PageName.MeiZhouHeiBaoPopupJackpotTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupJackpotTrigger"
                },
            [PageName.MeiZhouHeiBaoPopupJackpotResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupJackpotResult"
                },
            [PageName.MeiZhouHeiBaoPopupJackpotGame] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupJackpotGame"
                },
            [PageName.MeiZhouHeiBaoPopupJackpotLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/FGUIs", "MeiZhouHeiBao_3993.PopupJackpotLoading"
                },

            //幸运之轮
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
            },
            [PageName.XingYunZhiLunPopupBigWin] = new object[]
            {
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", "XingYunZhiLun_3998.PopupBigWin"
            },

            //财富火车
            [PageName.CaiFuHuoChePopupGameLoading] =
                new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupGameLoading" },
            [PageName.CaiFuHuoChePopupFreeSpinTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupFreeSpinTrigger"
                },
            [PageName.CaiFuHuoChePopupJackpotGameTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupJackpotGameTrigger"
                },
            [PageName.CaiFuHuoChePopupJackpotGameExit] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupJackpotGameExit"
                },
            [PageName.CaiFuHuoChePopupFreeSpinResult] =
                new object[]
                {
                    "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupFreeSpinResult"
                },
            [PageName.CaiFuHuoChePageGameMain] =
                new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PageGameMain" },
            
            [PageName.CaiFuHuoChePopupBigWin] = 
                new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupBigWin" },
            [PageName.CaiFuHuoChePopupJackpotResult] = 
                new object[] { "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs", "CaiFuHuoChe_3996.PopupJackpotResult" },


            //火焰公牛
            [PageName.HuoYanGongNiuPopupGameLoading] =
                new object[]
                {
                    "Assets/GameRes/Games/Huo Yan Gong Niu 3995/FGUIs", "HuoYanGongNiu_3995.PopupGameLoading"
                },
            [PageName.HuoYanGongNiuPageGameMain] =
                new object[] { "Assets/GameRes/Games/Huo Yan Gong Niu 3995/FGUIs", "HuoYanGongNiu_3995.PageGameMain" },
            [PageName.HuoYanGongNiuPopupFreeSpinTrigger] =
                new object[]
                {
                    "Assets/GameRes/Games/Huo Yan Gong Niu 3995/FGUIs", "HuoYanGongNiu_3995.PopupFreeSpinTrigger"
                },
            [PageName.HuoYanGongNiuPopupFreeSpinExit] =
                new object[]
                {
                    "Assets/GameRes/Games/Huo Yan Gong Niu 3995/FGUIs", "HuoYanGongNiu_3995.PopupFreeSpinExit"
                },
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
    ConsolePageConsoleGameHistory,
    ConsolePageConsoleHardware,
    ConsolePopupConsoleScreenColor,

    //大厅
    Hall01,

    //财富大厅
    TreasuryHallMain,

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
    CaiFuZhiMenPopupFreeSpinTrigger,
    CaiFuZhiMenPopupJackpotGame,
    CaiFuZhiMenPopupJackpotResult,
    CaiFuZhiMenPopupFreeSpinResult,
    CaiFuZhiMenPopupJackpotTrigger,
    CaiFuZhiMenPopupJackpotLoad,
    CaiFuZhiMenPopupOnlineJackpot,
    
    // 财富之家3997
    CaiFuZhiJiaPopupGameLoading,
    CaiFuZhiJiaPageGameMain,
    CaiFuZhiJiaPopupOverWin,
    CaiFuZhiJiaPopupFreeSpinTrigger,
    CaiFuZhiJiaPopupFreeSpinResult,
    CaiFuZhiJiaPopupSmallGameResult,
    CaiFuZhiJiaPopupSmallGameTrigger,
    CaiFuZhiJiaPopupJackpotWin,
    
    // 美洲黑豹
    MeiZhouHeiBaoPopupGameLoading,
    MeiZhouHeiBaoPageGameMain,
    MeiZhouHeiBaoPopupFreeSpinTrigger,
    MeiZhouHeiBaoPopupFreeSpinResult,
    MeiZhouHeiBaoPopupFreeGameLoading,
    MeiZhouHeiBaoPopupJackpotTrigger,
    MeiZhouHeiBaoPopupJackpotResult,
    MeiZhouHeiBaoPopupJackpotGame,
    MeiZhouHeiBaoPopupJackpotLoading,

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
    XingYunZhiLunPopupBigWin,

    //财富火车
    CaiFuHuoChePopupGameLoading,
    CaiFuHuoChePopupFreeSpinTrigger,
    CaiFuHuoChePopupJackpotGameTrigger,
    CaiFuHuoChePopupJackpotGameExit,
    CaiFuHuoChePopupFreeSpinResult,
    CaiFuHuoChePageGameMain,
    CaiFuHuoChePopupBigWin,
    CaiFuHuoChePopupJackpotResult,

    //火焰公牛
    HuoYanGongNiuPopupFreeSpinExit,
    HuoYanGongNiuPopupFreeSpinTrigger,
    HuoYanGongNiuPopupGameLoading,
    HuoYanGongNiuPageGameMain,
}