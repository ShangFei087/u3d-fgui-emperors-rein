using GameMaker;
using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MachineDataManager02
{




    private int curPermissions = -1;


    int[] jpDefaults = new int[] { 150000, 15000, 1500, 500, 10 };


    int testActiveMinute = (int)(60f * 24); //1天
    //int testActiveMinute = (int)(60f * 24 * 3f); //3天

    const string MOCK_PASSWORD_SHIFT = "MOCK_PASSWORD_SHIFT";
    const string MOCK_PASSWORD_MANAGER = "MOCK_PASSWORD_MANAGER";
    const string MOCK_PASSWORD_ADMIN = "MOCK_PASSWORD_ADMIN";

    const string MOCK_MACHINE_CODE = "MOCK_MACHINE_CODE";
    const string MOCK_MACHINE_CODE_OUT_TIME = "MOCK_MACHINE_LICENESE_OUT_TIME";
    const string MOCK_MACHINE_CODE_CREAT_TIME = "MOCK_MACHINE_LICENESE_CREAT_TIME";


    /// <summary> 机台设置 </summary>
    const string MOCK_MACHINE_SEVER_CONF = "MOCK_MACHINE_SEVER_CONF";

    /// <summary> 玩家数据 </summary>
    const string MOCK_MACHINE_SEVER_PLAYER = "MOCK_MACHINE_SEVER_PLAYER";

    /// <summary> 游戏彩金 </summary>
    const string MOCK_JP_GRAND = "MOCK_JP_GRAND";
    const string MOCK_JP_MEGA = "MOCK_JP_MEGA";
    const string MOCK_JP_MAJOR = "MOCK_JP_MAJOR";
    const string MOCK_JP_MINOR = "MOCK_JP_MINOR";
    const string MOCK_JP_MINI = "MOCK_JP_MINI";



    #region ==== Mock Data
    SBoxPlayerAccount _RequestGetPlayerAccountWhenMock()
    {
        if (!ApplicationSettings.Instance.isMock)
            return null;
        SBoxPlayerAccount PlayerAccount = new SBoxPlayerAccount()
        {
            PlayerId = SBoxModel.Instance.pid,    // 默认id为0
            CoinIn = 0,    // 总投币分
            CoinOut = 0,     // 总退币分
            ScoreIn = 0,     // 总上分
            ScoreOut = 0,    // 总下分                       
            Credit = 0,      // 余额分
            Bets = 0,        // 历史总押分
            Wins = 0,        // 历史总赢分
        };
        //string cache = SQLitePlayerPrefs03.Instance.GetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject(PlayerAccount));
        string cache = PlayerPrefs.GetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject(PlayerAccount));
        PlayerAccount = JsonConvert.DeserializeObject<SBoxPlayerAccount>(cache);

        return PlayerAccount;

    }


    /// <summary> 类似算法卡设置玩家金额 调试用 </summary>
    /// <remarks>
    /// mock模式下，没有算方法开，需要自己维护玩家金额
    /// </remarks>
    public void RequestSetPlayerCreditWhenMock(long credit, long bet, long win)
        => RequestSetPlayerAccountWhenMock(credit, bet, win, 0, 0, 0, 0);



    public void RequestSetCoinInCoinOutWhenMock(long credit, long coinInNum, long coinOutNum, long scoreInCredit, long scoreOutCredit)
        => RequestSetPlayerAccountWhenMock(credit, 0, 0, coinInNum, coinOutNum, scoreInCredit, scoreOutCredit);

    public void RequestSetPlayerAccountWhenMock(long credit, long bet, long win, long coinInNum, long coinOutNum, long scoreInCredit, long scoreOutCredit)
    {
        if (
            bet < 0
            && win < 0
            && coinInNum < 0
            && coinOutNum < 0
            && scoreInCredit < 0
            && scoreOutCredit < 0
            )
        {
            DebugUtils.Log("SetPlayerAccountWhenMock : data is error");
            return;
        }


        if (!ApplicationSettings.Instance.isMock)
            return;


        if (credit >= 0)
        {
            playerAccountMock.Credit = (int)credit;
            DebugUtils.Log($"设置玩家金额： {playerAccountMock.Credit}");
        }
        else
        {
            int changeCredit = (int)win - (int)bet
                + (int)coinInNum * SBoxModel.Instance.CoinInScale + (int)scoreInCredit
                 - DeviceUtils.GetCoinOutCredit((int)coinOutNum) - (int)scoreOutCredit;
            playerAccountMock.Credit = playerAccountMock.Credit + changeCredit;

            DebugUtils.Log($"玩家金额加减后：-- changeCredit = {changeCredit}  credit_after = {playerAccountMock.Credit} ");
        }

        playerAccountMock.Bets += (int)bet;
        playerAccountMock.Wins += (int)win;
        playerAccountMock.CoinIn += (int)coinInNum;
        playerAccountMock.CoinOut += (int)coinOutNum;
        playerAccountMock.ScoreIn += (int)scoreInCredit;
        playerAccountMock.ScoreOut += (int)scoreOutCredit;
        // SQLitePlayerPrefs03.Instance.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject((SBoxPlayerAccount)playerAccountMock));
        PlayerPrefs.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject((SBoxPlayerAccount)playerAccountMock));

        //return playerAccountMock;
    }





    public SBoxPlayerAccount RequestClearPlayerAccountWhenMock()
    {
        playerAccountMock = new SBoxPlayerAccount()
        {
            PlayerId = 0,    // 默认id为0
            CoinIn = 0,    // 总投币分
            CoinOut = 0,     // 总退币分
            ScoreIn = 0,     // 总上分
            ScoreOut = 0,    // 总下分                       
            Credit = 0,      // 余额分
            Bets = 0,        // 历史总押分
            Wins = 0,        // 历史总赢分
        };
        //SQLitePlayerPrefs03.Instance.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject(playerAccountMock));
        PlayerPrefs.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject(playerAccountMock));


        return GameCommon.Utils.DeepClone<SBoxPlayerAccount>(playerAccountMock);
    }





    SBoxPlayerAccount playerAccountMock
    {
        get
        {
            if (Instance._playerAccountMock == null)
            {
                Instance._playerAccountMock = _RequestGetPlayerAccountWhenMock();
            }

            return Instance._playerAccountMock;
        }
        set => Instance._playerAccountMock = value;
    }
    SBoxPlayerAccount _playerAccountMock = null;

    public int myCreditMock
    {
        get
        {
            return playerAccountMock.Credit;
        }
        set
        {
            playerAccountMock.Credit = value;
            //SQLitePlayerPrefs03.Instance.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject(playerAccountMock));
            PlayerPrefs.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject(playerAccountMock));
        }
    }


    #endregion




    void OnMockScoreUp(object req)
    {
        RequestSetCoinInCoinOutWhenMock(-1, 0, 0, (int)req, 0);
        // 上分逻辑
        OnResponseScoreUp((int)req);
    }

    void OnMockScoreDown(object req)
    {

        RequestSetCoinInCoinOutWhenMock(-1, 0, 0, 0, (int)req);
        // 下分逻辑
        OnResponseScoreDown((int)req);
    }

    void OnMockCoinIn(object req)
    {

        RequestSetCoinInCoinOutWhenMock(-1, (int)req, 0, 0, 0);
        // 上分逻辑
        OnResponseCoinIn((int)req);
    }

    void OnMockCoinOut(object req)
    {
        int coinOutNum = (int)req;

        RequestSetCoinInCoinOutWhenMock(-1, 0, coinOutNum, 0, 0);
        // 自己的回调
        OnResponseCoinOut((int)req);
    }



    void OnMockCheckPassword(object req) {
        SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
        {
            result = 1,
            permissions = -1,
        };

        string passwordStr = $"{req}";

        string passwordShift = PlayerPrefs.GetString(MOCK_PASSWORD_SHIFT, "666666");
        string passwordManager = PlayerPrefs.GetString(MOCK_PASSWORD_MANAGER, "88888888");
        string passwordAdmin = PlayerPrefs.GetString(MOCK_PASSWORD_ADMIN, "187653214");
        if (passwordShift == passwordStr)
        {
            sBoxPermissionsData.result = 0;
            sBoxPermissionsData.permissions = 1;
        }
        else if (passwordManager == passwordStr)
        {
            sBoxPermissionsData.result = 0;
            sBoxPermissionsData.permissions = 2;
        }
        else if (passwordAdmin == passwordStr)
        {
            sBoxPermissionsData.result = 0;
            sBoxPermissionsData.permissions = 3;
        }

        curPermissions = sBoxPermissionsData.permissions;

        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CHECK_PASSWORD, sBoxPermissionsData);
    }


    void OnMockChangePassword(object req)
    {
        string password = $"{(int)req}";

        switch (curPermissions) // -1 ??
        {
            case 1:
                {
                    PlayerPrefs.SetString(MOCK_PASSWORD_SHIFT, password);
                }
                break;
            case 2:
                {
                    PlayerPrefs.SetString(MOCK_PASSWORD_MANAGER, password);
                }
                break;
            case 3:
                {
                    PlayerPrefs.SetString(MOCK_PASSWORD_ADMIN, password);
                }
                break;
        }

        //修改玩家钥匙
        SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
        {
            result = 0,
            permissions = curPermissions,
        };
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CHANGE_PASSWORD, sBoxPermissionsData);
    }

    /// <summary>获取算法版本</summary>
    void OnMockIdeaVersion(object req)
    {
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_IDEA_VERSION, "9.9.9");
    }


    /// <summary>获取硬件版本</summary>
    void OnMockSandboxVersion(object req)
    {
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SANDBOX_VERSION, "8.8.8");
    }

    /// <summary>获取硬件版本</summary>
    void OnMockCoderInfo(object req)
    {
        SBoxCoderData reqDefault = new SBoxCoderData()
        {
            result = 0,
            Bets = SBoxModel.Instance.HistoryTotalBet,
            Wins = SBoxModel.Instance.HistoryTotalWin,
            MachineId = int.Parse(SBoxModel.Instance.MachineId), //HotfixSettings.gameId,
            CoderCount = 0,
            CheckValue = 567,
            RemainMinute = -1, //(60 * 1 + 3),         // 当前剩余时间（分钟）
        };

        SBoxCoderData req01 = JsonConvert.DeserializeObject<SBoxCoderData>(PlayerPrefs.GetString(MOCK_MACHINE_CODE, JsonConvert.SerializeObject(reqDefault)));
        long nowTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (req01.RemainMinute == -1)
        {
            req01.RemainMinute = testActiveMinute;

            long outtime = nowTimeMS + (long)(req01.RemainMinute * 60 * 1000);
            PlayerPrefs.SetString(MOCK_MACHINE_CODE_OUT_TIME, outtime.ToString());
        }

        long difMS = long.Parse(PlayerPrefs.GetString(MOCK_MACHINE_CODE_OUT_TIME, nowTimeMS.ToString()))
            - nowTimeMS;

        int difMinute = (int)(difMS / 1000 / 60);


        if (difMinute > 0)
        {
            req01.RemainMinute = difMinute;
        }
        else
        {
            req01.RemainMinute = 0;
        }

        req01.Bets = SBoxModel.Instance.HistoryTotalBet;
        req01.Wins = SBoxModel.Instance.HistoryTotalWin;
        req01.MachineId = int.Parse(SBoxModel.Instance.MachineId);

        DebugUtils.Log($"RemainMinute = {req01.RemainMinute}");
        PlayerPrefs.SetString(MOCK_MACHINE_CODE, JsonConvert.SerializeObject(req01));

        //下行:
        EventCenter.Instance.EventTrigger<SBoxCoderData>(SBoxEventHandle.SBOX_REQUEST_CODER, req01);
    }






    /// <summary>请求打码</summary>
    void OnMockCoder(object req)
    {

        ulong code = (ulong)req;
        if (code == 666)
        {
            //从PlayerPrefs读取序列化的SBoxCoderData数据
            SBoxCoderData req01 = JsonConvert.DeserializeObject<SBoxCoderData>(PlayerPrefs.GetString(MOCK_MACHINE_CODE, "{}"));
            //设置RemainMinute为testActiveMinute（测试激活分钟数）
            req01.RemainMinute = testActiveMinute;
            //计算过期时间：当前UTC时间 + 剩余分钟数
            long nowTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long outtime = nowTimeMS + (long)(req01.RemainMinute * 60 * 1000);
            //将过期时间保存到PlayerPrefs
            PlayerPrefs.SetString(MOCK_MACHINE_CODE_OUT_TIME, outtime.ToString());
            //更新序列化的激活数据
            PlayerPrefs.SetString(MOCK_MACHINE_CODE, JsonConvert.SerializeObject(req01));

            // 打码成功 触发成功事件（result = 0, permissions = 2001）
            EventCenter.Instance.EventTrigger<SBoxPermissionsData>(SBoxEventHandle.SBOX_CODER, new SBoxPermissionsData()
            {
                result = 0,
                permissions = 2001,
            });
        }
        else
        {
            // 打码失败
            EventCenter.Instance.EventTrigger<SBoxPermissionsData>(SBoxEventHandle.SBOX_CODER, new SBoxPermissionsData()
            {
                result = 1,
                permissions = 2001,
            });
        }
    }



    /// <summary>是否激活</summary>
    void OnMockActive(object req)
    {
        //非0时,需要激活
        long nowTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long lastTimeMS = long.Parse(PlayerPrefs.GetString(MOCK_MACHINE_CODE_OUT_TIME, nowTimeMS.ToString()));
        long difMS = nowTimeMS - lastTimeMS;
        OnResponseIsCodingActive(difMS < 0 ? 0 : 1);
    }




    /// <summary>获取配置</summary>
    void OnMockReadConf(object req)
    {
        SBoxConfData confDefault = new SBoxConfData()
        {
            result = 0,
            PwdType = 0,                         // 0:无任何修改参数的权限,1:普通密码权限,2:管理员密码权限,3:超级管理员密码权限
            PlaceType = 0,                       // 场地类型,0:普通,1:技巧,2:专家
            difficulty = 0,                      // 难度,0~8
            odds = 0,                            // 倍率,0:低倍率,1:高倍率,2:随机

            WinLock = 100,                         // 盈利宕机
            MachineId = 11109001,                       // 机台编号,8位有效十进制数
            LineId = 1110,                          // 线号,4位有效十进制数

            TicketMode = 0,                      // 退票模式,0:即中即退,1:退票
            TicketValue = 1,                   // 1票对应几分（彩票比例）  (投票:1票多少分？)
            scoreTicket = 1,                   // 1分对应几票
            CoinValue = 1000,                    // 投币比例 (投币:1币多少分？)
            MaxBet = 0,                          // 最大押注
            MinBet = 0,                          // 最小押注
            CountDown = 0,                       // 例计时
            MachineIdLock = 0,                   // 1:机台号已锁定,除超级管理员外,无法更改,0:机台号未锁定
            BetsMinOfJackpot = 0,                // 中彩金最小押分值
            JackpotStartValue = 0,               // 彩金初始值

            //LostLockCustom = 0,                  // 当轮爆机分数:默认300000


            LimitBetsWins = 0,// 限红值,默认3000
            ReturnScore = 0,// 返分值,500
            SwitchBetsUnitMin = 0,// 切换单位小,默认10
            SwitchBetsUnitMid = 0, // 切换单位中,默认50
            SwitchBetsUnitMax = 0,// 切换单位大,默认100
            ScoreUpUnit = 0, // 上分单位
            PrintMode = 0, // 打单模式,0:不打印,1:正常打印,2:伸缩打印
            ShowMode = 0,
            CheckTime = 0,// 对单时间
            OpenBoxTime = 0,// 开箱时间
            PrintLevel = 0, // 打印深度,0,1,2三级,0时最
            PlayerWinLock = 0,// 分机爆机分数:默认100000
            LostLock = 0,// 全台爆机分数:默认500000
            PulseValue = 0,// 脉冲比例 
            NewGameMode = 0,// 开始新一轮游戏模式,0:自动开始,1:手动开始
            NetJackpot = 0,
        };
        SBoxConfData conf = JsonUtility.FromJson<SBoxConfData>(
            PlayerPrefs.GetString(MOCK_MACHINE_SEVER_CONF, JsonConvert.SerializeObject(confDefault)));

        EventCenter.Instance.EventTrigger<SBoxConfData>(SBoxEventHandle.SBOX_READ_CONF, conf);
    }





    /// <summary>写配置</summary>
    void OnMockWriteConf(object req)
    {
        PlayerPrefs.SetString(MOCK_MACHINE_SEVER_CONF, JsonConvert.SerializeObject((SBoxConfData)req));

        SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
        {
            result = 0,
            permissions = 1,
        };
        EventCenter.Instance.EventTrigger<SBoxPermissionsData>(SBoxEventHandle.SBOX_WRITE_CONF, sBoxPermissionsData);
    }




    /// <summary>获取玩家信息</summary>
    void OnMockGetAccount(object req)
    {
        List<SBoxPlayerAccount> PlayerAccountList = new List<SBoxPlayerAccount>();

        PlayerAccountList.Add(GameCommon.Utils.DeepClone<SBoxPlayerAccount>(playerAccountMock));
        SBoxAccount res = new SBoxAccount()
        {
            result = 0,
            PlayerAccountList = PlayerAccountList,
        };
        EventCenter.Instance.EventTrigger<SBoxAccount>(SBoxEventHandle.SBOX_GET_ACCOUNT, res);
    }




    /// <summary>设置玩家信息</summary>
    void OnMockSetAccount(object req)
    {
        PlayerPrefs.SetString(MOCK_MACHINE_SEVER_PLAYER, JsonConvert.SerializeObject((SBoxPlayerAccount)req));
        OnResponseSetPlayerInfo((SBoxPlayerAccount)req);
    }





    void OnMockJackotGame(object req)
    {

        int grandJPNow = PlayerPrefs.GetInt(MOCK_JP_GRAND, jpDefaults[0]);
        int megaJPNow = PlayerPrefs.GetInt(MOCK_JP_MEGA, jpDefaults[1]);
        int majorJPNow = PlayerPrefs.GetInt(MOCK_JP_MAJOR, jpDefaults[2]);
        int minorJPNow = PlayerPrefs.GetInt(MOCK_JP_MINOR, jpDefaults[3]);
        int miniJPNow = PlayerPrefs.GetInt(MOCK_JP_MINI, jpDefaults[4]);


        bool isWinGrand = false;
        bool isWinMega = false;
        bool isWinMajor = false;
        bool isWinMinor = false;
        bool isWinMini = false;

        if (MainModel.Instance.contentMD.isSpin)
        {

            grandJPNow = grandJPNow == 0 ? jpDefaults[0] : grandJPNow;
            megaJPNow = megaJPNow == 0 ? jpDefaults[1] : megaJPNow;
            majorJPNow = majorJPNow == 0 ? jpDefaults[2] : majorJPNow;
            minorJPNow = minorJPNow == 0 ? jpDefaults[3] : minorJPNow;
            miniJPNow = miniJPNow == 0 ? jpDefaults[4] : miniJPNow;

            grandJPNow += UnityEngine.Random.Range(10, jpDefaults[0] / 4);
            megaJPNow += UnityEngine.Random.Range(10, jpDefaults[1] / 3);
            majorJPNow += UnityEngine.Random.Range(10, jpDefaults[2] / 3);
            minorJPNow += UnityEngine.Random.Range(10, jpDefaults[3] / 2);
            miniJPNow += UnityEngine.Random.Range(10, jpDefaults[4] / 2);

            grandJPNow = grandJPNow > 2 * jpDefaults[0] ? jpDefaults[0] : grandJPNow;
            megaJPNow = megaJPNow > jpDefaults[0] ? jpDefaults[1] : megaJPNow;
            majorJPNow = majorJPNow > jpDefaults[1] ? jpDefaults[2] : majorJPNow;
            minorJPNow = minorJPNow > jpDefaults[2] ? jpDefaults[3] : minorJPNow;
            miniJPNow = miniJPNow > jpDefaults[3] ? jpDefaults[4] : miniJPNow;


            // int idx = UnityEngine.Random.Range(0, 10);
            int idx = 100; //不中彩金

            isWinGrand = idx == 0;
            isWinMega = idx == 1;
            isWinMajor = idx == 2;
            isWinMinor = idx == 3;
            isWinMini = idx == 4;
        }

        List<JackpotWinInfo> jpWinLst = new List<JackpotWinInfo>();

        if (isWinGrand)
        {
            jpWinLst.Add(new JackpotWinInfo()
            {
                id = 0,
                name = "Grand",
                winCredit = (float)grandJPNow,
                whenCredit = (float)grandJPNow,
                curCredit = jpDefaults[0],
            });
            grandJPNow = jpDefaults[0];
        }

        if (isWinMega)
        {
            jpWinLst.Add(new JackpotWinInfo()
            {
                id = 1,
                name = "Mega",
                winCredit = (float)megaJPNow,
                whenCredit = (float)megaJPNow,
                curCredit = jpDefaults[1],
            });
            megaJPNow = jpDefaults[1];
        }

        if (isWinMajor)
        {
            jpWinLst.Add(new JackpotWinInfo()
            {
                id = 2,
                name = "Major",
                winCredit = (float)majorJPNow,
                whenCredit = (float)majorJPNow,
                curCredit = jpDefaults[2],
            });
            majorJPNow = jpDefaults[2];
        }

        if (isWinMinor)
        {
            jpWinLst.Add(new JackpotWinInfo()
            {
                id = 3,
                name = "Minor",
                winCredit = (float)minorJPNow,
                whenCredit = (float)minorJPNow,
                curCredit = jpDefaults[3],
            });
            minorJPNow = jpDefaults[3];
        }


        if (isWinMini)
        {
            jpWinLst.Add(new JackpotWinInfo()
            {
                id = 4,
                name = "Mini",
                winCredit = (float)miniJPNow,
                whenCredit = (float)miniJPNow,
                curCredit = jpDefaults[4],
            });
            miniJPNow = jpDefaults[4];
        }

        JackpotRes res = new JackpotRes()
        {
            curJackpotGrand = grandJPNow,
            curJackpotMega = megaJPNow,
            curJackpotMajor = majorJPNow,
            curJackpotMinior = minorJPNow,
            curJackpotMini = miniJPNow,
            jpWinLst = jpWinLst,
        };


        PlayerPrefs.SetInt(MOCK_JP_GRAND, grandJPNow);
        PlayerPrefs.SetInt(MOCK_JP_GRAND, megaJPNow);
        PlayerPrefs.SetInt(MOCK_JP_MAJOR, majorJPNow);
        PlayerPrefs.SetInt(MOCK_JP_MINOR, minorJPNow);
        PlayerPrefs.SetInt(MOCK_JP_MINI, miniJPNow);


        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_JACKPOT_GAME, res);
    }




    /// <summary>设置玩家信息</summary>
    void OnMockJackpotOnLine(object req)
    {
        int idx = UnityEngine.Random.Range(0, 300);

        if(testIsHitJackpotOnLine)
            idx = UnityEngine.Random.Range(0, 4);
        testIsHitJackpotOnLine = false;

        //DebugUtils.LogError("Online:" + idx);
        if (idx >= 0 && idx <= 3)
        {
            WinJackpotInfo info = new WinJackpotInfo()
            {
                macId = int.Parse(SBoxModel.Instance.MachineId),
                seat = SBoxModel.Instance.seatId,
                win = UnityEngine.Random.Range((idx + 1) * 10, (idx + 1) * 100),
                jackpotId = idx,
                orderId = 0,
                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            EventCenter.Instance.EventTrigger<string>(RpcNameJackpotOnLine, JsonConvert.SerializeObject(info));
        }
    }
    public bool testIsHitJackpotOnLine = false;


    /// <summary>设置码表</summary>  METER_SET
    void OnMockSetMeter(object req)
    {
        //码表
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_METER_SET, 0);
    }


    void OnMockGetBillList(object req)
    {
        List<string> billerList = new List<string>()
                    {
                        "MEI:AE2831 D5",
                        "Pyramid:APEX 5000 SERIES",
                        "Pyramid:APEX 7000 SERIES",
                        "ICT:PA&/TAO",
                    };
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, billerList);
    }


    void OnMockSelectBill(object req)
    {
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, 0);
    }






    void OnMockGetPrintList(object req)
    {
        List<string> printerList = new List<string>()
                    {
                        "ICT:GP-58CR",
                        "PTI:Phoenix",
                        "ITHACA:Epic950"
                    };
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, printerList);
    }



    void OnMockSelectPrinter(object req)
    {
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, 0);
    }


    void OnMockResetPrinter(object req)
    {
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_RESET, 0);
    }

}
