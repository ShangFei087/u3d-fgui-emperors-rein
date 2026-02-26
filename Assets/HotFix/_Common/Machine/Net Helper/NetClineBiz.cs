using GlobalJackpotConsole;
using UnityEngine;
using SBoxApi;
using Newtonsoft.Json;
using GameMaker;
using FairyGUI;
using System.Collections;
public class NetClineBiz: MonoSingleton<NetClineBiz>
{

    public bool isLoginSuccess = false;


    Coroutine coCheckLoginJpConsole;
    Coroutine coRepeatGetJackpotGameShowWhenIdle;
    IEnumerator CorCheckLoginJpConsole()
    {
        isLoginSuccess = false;
        while (!isLoginSuccess)  // 登录失败，定时重登录
        {

            while (!ClientWS.Instance.IsConnected)  // 等待连接上
            {
                yield return new WaitForSecondsRealtime(0.3f);
            }

            int machineId = int.Parse(SBoxModel.Instance.MachineId);
            LoginInfo loginInfo = new LoginInfo()
            {
                gameType = (int)GameType.CoinPusher,
                macId = machineId,

                groudId = SBoxModel.Instance.groupId,
                seatId = SBoxModel.Instance.seatId
            };
            //登录后台
            NetClientHelper02.Instance.RequestLogin(loginInfo, (res) =>
            {
                DebugUtils.Save("【UDP-WS】彩金后台-登录成功", LogType.Warning);

                isLoginSuccess = true;


                // 登录成功回调
                OnLoginSuccess();

            }, (err) =>
            {
                DebugUtils.Save("【UDP-WS】登录失败!", LogType.Warning);
            });
            yield return new WaitForSecondsRealtime(10f);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// * 首次连接后，进行登录
    /// * 登录失败后，重复检查登录
    /// * 断网重连后，进行登录
    /// </remarks>
    public void CheckLoginJpConsole()
    {
        if (coRepeatGetJackpotGameShowWhenIdle != null)
            StopCoroutine(coRepeatGetJackpotGameShowWhenIdle);
        coRepeatGetJackpotGameShowWhenIdle = null;

        if (coCheckLoginJpConsole != null)
            StopCoroutine(coCheckLoginJpConsole);

        coCheckLoginJpConsole = StartCoroutine(CorCheckLoginJpConsole());


        SetPulseValueDefault();
    }

    public void Clear()
    {
        isLoginSuccess = false;

        if (coRepeatGetJackpotGameShowWhenIdle != null)
            StopCoroutine(coRepeatGetJackpotGameShowWhenIdle);
        coRepeatGetJackpotGameShowWhenIdle = null;


        if (coCheckLoginJpConsole != null)
            StopCoroutine(coCheckLoginJpConsole);
        coCheckLoginJpConsole = null;
    }


    void OnLoginSuccess()
    {


        RequestBase req = new RequestBase()
        {
            gameType = (int)GameType.CoinPusher,
        };
        // 读取后台彩金配置
        NetClientHelper02.Instance.RequestReadConf(req, (System.Action<object>)((res002) =>
        {

            // 设置当前账号
            MachineDataManager02.Instance.RequestCheckPassword((int)SBoxModel.Instance.passwordAdmin, (_res01) =>
            {
                SBoxPermissionsData _data01 = _res01 as SBoxPermissionsData;

                if (_data01.result == 0 && _data01.permissions > 0)
                {

                    ReadConfR _res002 = res002 as ReadConfR;

                    SBoxConfData data002 = _res002.sboxConfData;

                    DebugUtils.Save($"【UDP-WS】彩金后台-获取的配置，{JsonConvert.SerializeObject(data002)}", LogType.Warning);


                    SBoxConfData sBoxConfData = new SBoxConfData();

                    sBoxConfData.MachineId = int.Parse(SBoxModel.Instance.MachineId);
                    sBoxConfData.LineId = int.Parse(SBoxModel.Instance.LineId);

                    sBoxConfData.PlaceType = data002.PlaceType;
                    sBoxConfData.difficulty = data002.difficulty;
                    sBoxConfData.odds = data002.odds;
                    sBoxConfData.WinLock = data002.WinLock;
                    sBoxConfData.TicketMode = data002.TicketMode;
                    sBoxConfData.TicketValue = data002.TicketValue;
                    sBoxConfData.scoreTicket = data002.scoreTicket;
                    sBoxConfData.CoinValue = data002.CoinValue;
                    sBoxConfData.MaxBet = data002.MaxBet;
                    sBoxConfData.MinBet = data002.MinBet;
                    sBoxConfData.CountDown = data002.CountDown;
                    sBoxConfData.BetsMinOfJackpot = data002.BetsMinOfJackpot;
                    sBoxConfData.JackpotStartValue = data002.JackpotStartValue;
                    sBoxConfData.LimitBetsWins = data002.LimitBetsWins;
                    sBoxConfData.ReturnScore = data002.ReturnScore;
                    sBoxConfData.SwitchBetsUnitMin = data002.SwitchBetsUnitMin;
                    sBoxConfData.SwitchBetsUnitMid = data002.SwitchBetsUnitMid;
                    sBoxConfData.SwitchBetsUnitMax = data002.SwitchBetsUnitMax;
                    sBoxConfData.ScoreUpUnit = data002.ScoreUpUnit;
                    sBoxConfData.PrintMode = data002.PrintMode;
                    sBoxConfData.ShowMode = data002.ShowMode;
                    sBoxConfData.CheckTime = data002.CheckTime;
                    sBoxConfData.OpenBoxTime = data002.OpenBoxTime;
                    sBoxConfData.PrintLevel = data002.PrintLevel;
                    sBoxConfData.PlayerWinLock = data002.PlayerWinLock;
                    sBoxConfData.LostLock = data002.LostLock;
                    sBoxConfData.PulseValue = 1; // data.PulseValue;
                    sBoxConfData.NewGameMode = data002.NewGameMode;
                    sBoxConfData.NetJackpot = data002.NetJackpot;
                    sBoxConfData.JackpotLevel = data002.JackpotLevel;
                    //sBoxConfData.BallValue = data002.BallValue;

                    SBoxModel.Instance.SboxConfData = sBoxConfData;

                    /*
                    // 写入数据到算法卡
                    SBoxIdea.WriteConf(sBoxConfData);
                    DebugUtils.Log("【UDP-WS】写入： SBoxIdea.WriteConf");
                    */

                    MachineDataManager02.Instance.RequestWriteConf(sBoxConfData, (res) =>
                    {
                        MachineDataManager02.Instance.RequestReadConf((res003) =>
                        {
                            DebugUtils.Log("【UDP-WS】读取： ReadConf 成功");
                        }, (err) =>
                        {
                            DebugUtils.Log("【UDP-WS】 读取： ReadConf 失败");
                        });

                    },(err)=>{

                    });


                    //DebugUtils.LogError(" GlobalEvent.ON_REMOTE_CONSOL_EVENT 1");
                    EventCenter.Instance.EventTrigger(GlobalEvent.ON_REMOTE_CONSOL_EVENT, new EventData(GlobalEvent.GetRemoteConsoleConfigFinish));

                }
                else
                {
                    DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
                }
            }, (err) =>
            {
                DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
            });

        }), (err) =>
        {
            DebugUtils.Save("【UDP-WS】获取的配置，失败", LogType.Warning);

        });


        if (coRepeatGetJackpotGameShowWhenIdle != null)
            StopCoroutine(coRepeatGetJackpotGameShowWhenIdle);
        coRepeatGetJackpotGameShowWhenIdle = StartCoroutine(CorRepeatGetJackpotGameShowWhenIdle());
    }


    void SetPulseValueDefault()
    {
        //延时设置
        Timers.inst.Add(4f, 1, (TimerCallback)((obj) =>
        {
            MachineDataManager02.Instance.RequestCheckPassword((int)SBoxModel.Instance.passwordAdmin, (_res01) =>
            {
                SBoxPermissionsData _data01 = _res01 as SBoxPermissionsData;
                if (_data01.result == 0 && _data01.permissions > 0)
                {
                    //读取算法卡的数据，把PulseValue默认设置为1
                    MachineDataManager02.Instance.RequestReadConf((res02) =>
                    {

                        SBoxConfData data = (SBoxConfData)res02;
                        data.PulseValue = 1;   // 这个值默认为1
                        SBoxModel.Instance.SboxConfData = data;

                        SBoxIdea.WriteConf(data);

                    }, (err02) =>
                    {

                    });
                }
                else
                {
                    DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
                }
            }, (err) =>
            {
                DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
            });
        }));
    }


    IEnumerator CorRepeatGetJackpotGameShowWhenIdle()
    {

        bool isNext = false;
        while (true)
        {

            while (true)
            {
                if ( MainModel.Instance.contentMD != null 
                    && MainModel.Instance.contentMD.isSpin == false 
                    && MainModel.Instance.contentMD.isAuto == false)
                {
                    break;
                }

                yield return new WaitForSecondsRealtime(1f);
            }

            yield return new WaitForSecondsRealtime(10f);
            RequestBase req = new RequestBase();
            req.gameType = (int)GameType.CoinPusher;

            NetClientHelper02.Instance.RequestGetJackpotShowValue(req, (res) =>
            {

                EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_GAME_COIN_PUSH_EVENT, 
                    new EventData<JackpotGameShowInfoR>(GlobalEvent.GetJackpotGameShow, res as JackpotGameShowInfoR));

                isNext = true;
            },
            (err) =>
            {
                isNext = true;
            });

            yield return new WaitUntil(()=> isNext == true);
            isNext = false;
        }
    }

#if false

    public void LoginJpConsoleBiz()
    {
        int machineId = int.Parse(SBoxModel.Instance.MachineId);
        LoginInfo loginInfo = new LoginInfo()
        {
            gameType = (int)GameType.CoinPusher,
            macId = machineId,

            groudId = SBoxModel.Instance.groupId,
            seatId = SBoxModel.Instance.seatId
        };


        //登录后台
        NetClientHelper02.Instance.RequestLogin(loginInfo, (res) =>
        {
            DebugUtils.Save("【UDP-WS】彩金后台-登录成功", LogType.Warning);

            //NetMgr.Instance.SendHeartHeatToServer(); // 发心跳

            
            RequestBase req = new RequestBase()
            {
                gameType = (int)GameType.CoinPusher,
            };
            NetClientHelper02.Instance.RequestReadConf(req, (res002) =>
            {

                MachineDataManager02.Instance.RequestCheckPassword(SBoxModel.Instance.pwdAdminCache, (_res01) =>
                {
                    SBoxPermissionsData _data01 = _res01 as SBoxPermissionsData;

                    if (_data01.result == 0 && _data01.permissions > 0)
                    {


                        SBoxConfData data002 = res002 as SBoxConfData;

                        DebugUtils.Save($"【UDP-WS】彩金后台-获取的配置，{JsonConvert.SerializeObject(data002)}" , LogType.Warning);


                        SBoxConfData sBoxConfData = new SBoxConfData();

                        sBoxConfData.MachineId = int.Parse(SBoxModel.Instance.MachineId);
                        sBoxConfData.LineId = int.Parse(SBoxModel.Instance.LineId);

                        sBoxConfData.PlaceType = data002.PlaceType;
                        sBoxConfData.difficulty = data002.difficulty;
                        sBoxConfData.odds = data002.odds;
                        sBoxConfData.WinLock = data002.WinLock;
                        sBoxConfData.TicketMode = data002.TicketMode;
                        sBoxConfData.TicketValue = data002.TicketValue;
                        sBoxConfData.scoreTicket = data002.scoreTicket;
                        sBoxConfData.CoinValue = data002.CoinValue;
                        sBoxConfData.MaxBet = data002.MaxBet;
                        sBoxConfData.MinBet = data002.MinBet;
                        sBoxConfData.CountDown = data002.CountDown;
                        sBoxConfData.BetsMinOfJackpot = data002.BetsMinOfJackpot;
                        sBoxConfData.JackpotStartValue = data002.JackpotStartValue;
                        sBoxConfData.LimitBetsWins = data002.LimitBetsWins;
                        sBoxConfData.ReturnScore = data002.ReturnScore;
                        sBoxConfData.SwitchBetsUnitMin = data002.SwitchBetsUnitMin;
                        sBoxConfData.SwitchBetsUnitMid = data002.SwitchBetsUnitMid;
                        sBoxConfData.SwitchBetsUnitMax = data002.SwitchBetsUnitMax;
                        sBoxConfData.ScoreUpUnit = data002.ScoreUpUnit;
                        sBoxConfData.PrintMode = data002.PrintMode;
                        sBoxConfData.ShowMode = data002.ShowMode;
                        sBoxConfData.CheckTime = data002.CheckTime;
                        sBoxConfData.OpenBoxTime = data002.OpenBoxTime;
                        sBoxConfData.PrintLevel = data002.PrintLevel;
                        sBoxConfData.PlayerWinLock = data002.PlayerWinLock;
                        sBoxConfData.LostLock = data002.LostLock;
                        sBoxConfData.PulseValue = 1; // data.PulseValue;
                        sBoxConfData.NewGameMode = data002.NewGameMode;
                        sBoxConfData.NetJackpot = data002.NetJackpot;
                        sBoxConfData.JackpotLevel = data002.JackpotLevel;
                        sBoxConfData.BallValue = data002.BallValue;

                        SBoxModel.Instance.CfgData = sBoxConfData;
                        SBoxIdea.WriteConf(sBoxConfData);
                        DebugUtils.Log("【UDP-WS】写入： SBoxIdea.WriteConf");

                        MachineDataManager02.Instance.RequestReadConf((res003) =>
                        {
                            DebugUtils.Log("【UDP-WS】读取： ReadConf 成功");
                        },(err) =>
                        {
                            DebugUtils.Log("【UDP-WS】 读取： ReadConf 失败");
                        });


                        //DebugUtils.LogError(" GlobalEvent.ON_REMOTE_CONSOL_EVENT 1");
                        EventCenter.Instance.EventTrigger(GlobalEvent.ON_REMOTE_CONSOL_EVENT, new EventData(GlobalEvent.GetRemoteConsoleConfigFinish));
      
                    }
                    else
                    {
                        DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
                    }
                }, (err) =>
                {
                    DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
                });

            }, (err) =>
            {
                DebugUtils.Save("【UDP-WS】获取的配置，失败", LogType.Warning);

            });


        }, (err) =>
        {
            DebugUtils.Save("【UDP-WS】登录失败!", LogType.Warning);
        });

        Timers.inst.Add(4f, 1,(obj) =>
        {
            MachineDataManager02.Instance.RequestCheckPassword(SBoxModel.Instance.pwdAdminCache, (_res01) =>
            {
                SBoxPermissionsData _data01 = _res01 as SBoxPermissionsData;
                if (_data01.result == 0 && _data01.permissions > 0)
                {
                    MachineDataManager02.Instance.RequestReadConf((res02) =>
                    {

                        SBoxConfData data = (SBoxConfData)res02;
                        data.PulseValue = 1;
                        SBoxModel.Instance.sboxConfData = data;

                        SBoxIdea.WriteConf(data);

                    }, (err02) =>
                    {

                    });
                }
                else
                {
                    DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
                }
            }, (err) =>
            {
                DebugUtils.LogError("【UDP-WS】 同步彩金后台配置 - 错误");
            });
        });
    }

#endif


}
