using GameMaker;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
public class CoinInData
{
    public int id;
    public int coinNum;
}

public class CoinOutData
{
    public int id;
    public int coinNum;
}*/

public partial class SBoxModel: MonoSingleton<SBoxModel>
{



    Observer m_Observable;
    public Observer observable
    {
        get
        {
            if (m_Observable == null)
            {
                string[] classNamePath = this.GetType().ToString().Split('.');
                m_Observable = new Observer(classNamePath[classNamePath.Length - 1]);
            }
            return m_Observable;
        }
    }

    //public bool isSboxSandboxReady;

    //public bool isSboxReady;

    /* #seaweed#
    public int curCoinOutNum;
    public int totalCoinOutNum;
    public bool coinOuting;
    public int macId;
    
    public List<int> switchList = new List<int> { 10, 50, 100 };
    */


    /*
    private SBoxConfData _cfgData;



    public SBoxConfData tempCfgData;

    public SBoxConfData CfgData
    {
        set
        {
            //_cfgData = value;
            observable.SetProperty(ref _cfgData, value);

            SetTempCfgData(value);
            //switchList = new List<int> { sboxConfData.SwitchBetsUnitMin, sboxConfData.SwitchBetsUnitMid, sboxConfData.SwitchBetsUnitMax };
        }
        get {
        
            if(_cfgData == null)
            {
                _cfgData = new SBoxConfData()
                {
                    result = 0,
                    PwdType = 0,                         // 0：无任何修改参数的权限，1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限
                    PlaceType = 0,                       // 场地类型，0：普通，1：技巧，2：专家
                    difficulty = 0,                      // 难度，0~8
                    odds = 0,                            // 倍率，0：低倍率，1：高倍率，2：随机

                    WinLock = 0,                         // 盈利宕机
                    MachineId = -99999999,                       // 机台编号，8位有效十进制数
                    LineId = -9999,                          // 线号，4位有效十进制数

                    TicketMode = 0,                      // 退票模式，0：即中即退，1：退票
                    TicketValue = -999,                     // 1票对应几分（彩票比例）
                    scoreTicket = 1,                     // 1分对应几票
                    CoinValue = -999,                       // 投币比例
                    MaxBet = 0,                          // 最大押注
                    MinBet = 0,                          // 最小押注
                    CountDown = 0,                       // 例计时
                    MachineIdLock = 0,                   // 1：机台号已锁定，除超级管理员外，无法更改，0：机台号未锁定
                    BetsMinOfJackpot = 0,                // 中彩金最小押分值
                    JackpotStartValue = 0,               // 彩金初始值
                    LimitBetsWins = 0,                   // 限红值，默认3000
                    ReturnScore = 0,                     // 返分值，500

                    SwitchBetsUnitMin = 0,               // 切换单位小，默认10
                    SwitchBetsUnitMid = 0,               // 切换单位中，默认50
                    SwitchBetsUnitMax = 0,               // 切换单位大，默认100

                    ScoreUpUnit = -999,                     // 上分单位
                    PrintMode = 0,                       // 打单模式，0：不打印，1：正常打印，2：伸缩打印

                    ShowMode = 0,
                    CheckTime = 0,                       // 对单时间
                    OpenBoxTime = 0,                     // 开箱时间
                    PrintLevel = 0,                      // 打印深度，0，1，2三级，0时最
                    PlayerWinLock = 0,                   // 分机爆机分数：默认100000
                    LostLock = 0,                        // 全台爆机分数：默认500000
                    LostLockCustom = 0,                  // 当轮爆机分数：默认300000
                    PulseValue = 0,                      // 脉冲比例
                    NewGameMode = 0,                     // 开始新一轮游戏模式，0：自动开始，1：手动开始
                    NetJackpot = 0,                      // 是否启用联网彩金 0:关闭 1:开启
                };
            }
            return _cfgData;
        }
    }

    public void SetTempCfgData(SBoxConfData data)
    {
        tempCfgData = new SBoxConfData
        {
            result = data.result,
            PwdType = data.PwdType,
            PlaceType = data.PlaceType,
            difficulty = data.difficulty,
            odds = data.odds,
            WinLock = data.WinLock,
            MachineId = data.MachineId,
            LineId = data.LineId,
            TicketMode = data.TicketMode,
            TicketValue = data.TicketValue,
            scoreTicket = data.scoreTicket,
            CoinValue = data.CoinValue,
            MaxBet = data.MaxBet,
            MinBet = data.MinBet,
            CountDown = data.CountDown,
            MachineIdLock = data.MachineIdLock,
            BetsMinOfJackpot = data.BetsMinOfJackpot,
            JackpotStartValue = data.JackpotStartValue,
            LimitBetsWins = data.LimitBetsWins,
            ReturnScore = data.ReturnScore,
            SwitchBetsUnitMin = data.SwitchBetsUnitMin,
            SwitchBetsUnitMid = data.SwitchBetsUnitMid,
            SwitchBetsUnitMax = data.SwitchBetsUnitMax,
            ScoreUpUnit = data.ScoreUpUnit,
            PrintMode = data.PrintMode,
            ShowMode = data.ShowMode,
            CheckTime = data.CheckTime,
            OpenBoxTime = data.OpenBoxTime,
            PrintLevel = data.PrintLevel,
            PlayerWinLock = data.PlayerWinLock,
            LostLock = data.LostLock,
            LostLockCustom = data.LostLockCustom,
            PulseValue = data.PulseValue,
            NewGameMode = data.NewGameMode,
        };
    }
    */


    public SBoxConfData SboxConfData
    {
        get => m_SboxConfData;
        set => m_SboxConfData = value;
    }
    [SerializeField]
    SBoxConfData m_SboxConfData = new SBoxConfData()
    {
        result = 0,
        PwdType = 0,                         // 0：无任何修改参数的权限，1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限
        PlaceType = 0,                       // 场地类型，0：普通，1：技巧，2：专家
        difficulty = 0,                      // 难度，0~8
        odds = 0,                            // 倍率，0：低倍率，1：高倍率，2：随机

        WinLock = 0,                         // 盈利宕机
        MachineId = -99999999,                       // 机台编号，8位有效十进制数
        LineId = -9999,                          // 线号，4位有效十进制数

        TicketMode = 0,                      // 退票模式，0：即中即退，1：退票
        TicketValue = -999,                     // 1票对应几分（彩票比例）
        scoreTicket = 1,                     // 1分对应几票
        CoinValue = -999,                       // 投币比例
        MaxBet = 0,                          // 最大押注
        MinBet = 0,                          // 最小押注
        CountDown = 0,                       // 例计时
        MachineIdLock = 0,                   // 1：机台号已锁定，除超级管理员外，无法更改，0：机台号未锁定
        BetsMinOfJackpot = 0,                // 中彩金最小押分值
        JackpotStartValue = 0,               // 彩金初始值
        LimitBetsWins = 0,                   // 限红值，默认3000
        ReturnScore = 0,                     // 返分值，500

        SwitchBetsUnitMin = 0,               // 切换单位小，默认10
        SwitchBetsUnitMid = 0,               // 切换单位中，默认50
        SwitchBetsUnitMax = 0,               // 切换单位大，默认100

        ScoreUpUnit = -999,                     // 上分单位
        PrintMode = 0,                       // 打单模式，0：不打印，1：正常打印，2：伸缩打印

        ShowMode = 0,
        CheckTime = 0,                       // 对单时间
        OpenBoxTime = 0,                     // 开箱时间
        PrintLevel = 0,                      // 打印深度，0，1，2三级，0时最
        PlayerWinLock = 0,                   // 分机爆机分数：默认100000
        LostLock = 0,                        // 全台爆机分数：默认500000
        LostLockCustom = 0,                  // 当轮爆机分数：默认300000
        PulseValue = 0,                      // 脉冲比例
        NewGameMode = 0,                     // 开始新一轮游戏模式，0：自动开始，1：手动开始
        NetJackpot = 0,                      // 是否启用联网彩金 0:关闭 1:开启
    };



    /// <summary> 线号 </summary>
    public string LineId
    {
        get => $"{SboxConfData.LineId}";
        set => SboxConfData.LineId = int.Parse(value);
    }

    /// <summary> 机台号 </summary>
    public string MachineId
    {
        get => $"{SboxConfData.MachineId}";
       set => SboxConfData.MachineId = int.Parse(value);
    }


    /// <summary> 组号号 </summary>
    public int groupId
    {
        get
        {
            if (_groupId == null)
            {
                _groupId = SQLitePlayerPrefs03.Instance.GetInt(PARAM_GROUP_ID, 0);
                if (_groupId == 0)
                {
                    _groupId = PlayerPrefs.GetInt(PARAM_GROUP_ID, 0);
                    SQLitePlayerPrefs03.Instance.SetInt(PARAM_GROUP_ID, (int)_groupId);
                }
            }

            return (int)_groupId;
        }
        set
        {
            observable.SetProperty(ref _groupId, value);
            //_groupId = value;
            PlayerPrefs.SetInt(PARAM_GROUP_ID, value);
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_GROUP_ID, value);
        }
    }
    int? _groupId = null;
    const string PARAM_GROUP_ID = "PARAM_GROUP_ID";




    /// <summary> 座位id </summary>
    public int seatId
    {
        get
        {
            if (_seatId == null)
            {

                _seatId = SQLitePlayerPrefs03.Instance.GetInt(PARAM_SEAT_ID, 0);
                if (_seatId == 0)
                {
                    _seatId = PlayerPrefs.GetInt(PARAM_SEAT_ID, 0);
                    SQLitePlayerPrefs03.Instance.SetInt(PARAM_SEAT_ID, (int)_seatId);
                }
            }
            return (int)_seatId;
        }
        set
        {
            observable.SetProperty(ref _seatId, value);
            //_seatId = value;
            PlayerPrefs.SetInt(PARAM_SEAT_ID, value);
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_SEAT_ID, value);
        }
    }
    int? _seatId= null;
    const string PARAM_SEAT_ID = "PARAM_SEAT_ID";






    /// <summary> 分机号 </summary>
    public int pid
    {
        get => 0;
        set { }
        /*
        get
        {
            if (_pid == null)
                _pid = PlayerPrefs.GetInt(PARAM_PID, 0);
            return (int)_pid;
        }
        set
        {
            _pid = value;
            PlayerPrefs.SetInt(PARAM_PID, value);
        }
        */
    }
    int? _pid = null;
    const string PARAM_PID = "PARAM_PID";



    /*
    public int MyCredit
    {
        get => SboxPlayerAccount.Credit;
        set => SboxPlayerAccount.Credit = value;
    }
    */

    public int[] JPBet = new int[] { 1, 2, 5, 10, 20, 50, 100 };

    public int JPBetIndex;
    public int BallPerCoinIndex;

    /// <summary> 1币多少分 </summary>
    public int CoinInScale
    {
        get => SboxConfData.CoinValue;
        set => SboxConfData.CoinValue = value;
    }

    /// <summary> 1次多少分 </summary>
    public int ScoreUpDownScale
    {
        get => SboxConfData.ScoreUpUnit;
        set => SboxConfData.ScoreUpUnit = value;
    }


    /// <summary>
    /// 长按时短按的10倍
    /// </summary>
    public int ScoreUpScaleLongClick
    {
        get =>  SboxConfData.ScoreUpUnit * 10;
    }



    /// <summary> 1分对应几票 </summary>
    public int CoinOutScaleTicketPerCredit
    {
        get => SboxConfData.scoreTicket;
        set => SboxConfData.scoreTicket = value;
    }

    /// <summary> 1票对应几分 </summary>
    public int CoinOutScaleCreditPerTicket
    {
        get => SboxConfData.TicketValue;
        set => SboxConfData.TicketValue = value;
    }


    /// <summary> 回球比例 </summary>
    public int BallRewardScale
    {
        get => 1; // SboxConfData.BallValue;
        set
        {

        }// SboxConfData.BallValue = value;
    }

    /// <summary> 硬盘版本 </summary>
    public string HardwareVer
    {
        //get => tableMachine.hardware_ver;
        //set => tableMachine.hardware_ver = value;
        get => _hardwareVer;
        set => _hardwareVer = value;
    }
    string _hardwareVer = "1.0.0";


    /// <summary> 算法卡版本 </summary>
    public string AlgorithmVer
    {
        get => _algorithmVer;
        set => _algorithmVer = value;
    }
    string _algorithmVer = "1.0.0";


    /// <summary> 当前玩家数据 </summary>
    public SBoxPlayerAccount SboxPlayerAccount
    {
        get{
            if (m_SBoxPlayerAccount == null)
            {
                m_SBoxPlayerAccount = new SBoxPlayerAccount()
                {
                    PlayerId =0,
                    CoinIn = -9999,                    // 历史总-投币分（可能是数量）
                    CoinOut = -9999,                   // 历史总-退币分（可能是数量）
                    ScoreIn = -9999,                    // 历史总-上分
                    ScoreOut = -9999,                    // 历史总-下分

                    Credit = 9999,                      // 余额分

                    Bets = -9999,                        // 历史总-总押分
                    Wins = -9999,                         // 历史总-总赢分
                };
            }
            return observable.GetProperty(() => m_SBoxPlayerAccount);
            // return _sBoxPlayerAccount;
        }
        set {
            //_sBoxPlayerAccount = value;
            //Debug.LogError("SBoxPlayerAccount 赋值 ");
            observable.SetProperty(ref m_SBoxPlayerAccount, value);
        }
    }
    [SerializeField]
    SBoxPlayerAccount m_SBoxPlayerAccount;

    public long myCredit  //   public long myCredit
    {
        get => (long)SboxPlayerAccount.Credit;
        set
        {
            Debug.LogError("myCredit 赋值 =="+ value);
            SboxPlayerAccount.Credit = (int)value; //这个不发事件
        }
    }

    /// <summary> 历史总压注 </summary>
    public long HistoryTotalBet
    {
        get => SboxPlayerAccount.Bets;
    }


    /// <summary> 历史总赢 </summary>
    public long HistoryTotalWin
    {
        get => SboxPlayerAccount.Wins;
    }

    /// <summary> 历史总压分盈利(总压注分 - 总得分) </summary>
    public long HistoryTotalProfitBet
    {
        get => HistoryTotalBet - HistoryTotalWin;
    }



    /// <summary> 历史总投币个数 </summary>
    long HistoryTotalCoinInNums
    {
        get => SboxPlayerAccount.CoinIn;
    }

    /// <summary> 历史总投币 </summary>
    public long HistoryTotalCoinInCredit
    {
        //get => HistoryTotalCoinInNums;
        get => HistoryTotalCoinInNums * CoinInScale;
    }

    /// <summary> 历史总退票个数 （之前是对的，CoinOut返回的是数量。这个版本缺变成了分数！） </summary>
    /// <remarks>
    /// * 存在异议，这个参数改为private，不被外面调用。<br/>
    /// </remarks>
    long HistoryTotalCoinOutNums
    {
        get => SboxPlayerAccount.CoinOut;
    }
    /// <summary> 历史总退票 </summary>
    public long HistoryTotalCoinOutCredit
    {
        // get => HistoryTotalCoinOutNums;
        get => DeviceUtils.GetCoinOutCredit((int)HistoryTotalCoinOutNums);
    }
    /// <summary> 历史总投币盈利(总投币 - 总退票) </summary>
    public long HistoryTotalProfitCoinIn
    {
        get => HistoryTotalCoinInCredit - HistoryTotalCoinOutCredit;
    }

    /// <summary> 历史总上分 </summary>
    public long HistoryTotalScoreUpCredit
    {
        get => SboxPlayerAccount.ScoreIn;
    }

    /// <summary> 历史总下分 </summary>
    public long HistoryTotalScoreDownCredit
    {
        get => SboxPlayerAccount.ScoreOut;
    }

    /// <summary> 历史总上分盈利(总上分 - 总下分) </summary>
    public long HistoryTotalProfitScoreUp
    {
        get => HistoryTotalScoreUpCredit - HistoryTotalScoreDownCredit;
    }


    /*
 账单详细信息：（什么时间？）
年月日，时分
线号/机台号/投币比例
主机版本（软件版本-自定义 ？？）/算法版本/难度/盈利宕机（盈利宕机单位是万，数值需要乘一万）
*/
    public string BillInfoTime
    {
        get
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            DateTime localDateTime = dateTimeOffset.LocalDateTime;
            //return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return localDateTime.ToString("yyyy-MM-dd");
        }
    }

    public string BillInfoLineMachineNumber
    {
        get => $"{LineId}/{MachineId}/1:{CoinInScale}";
    }

    public string BillInfoHardwareAlgorithmVer
    {
        get
        {
#if UNITY_EDITOR
            int winLockBalance = 1000000; // 盈利当机余额
            //int winLockBalance = 100; // 盈利当机余额
#else
            int winLockBalance = SBoxIdea.WinLockBalance(); // 盈利当机余额
#endif

            string tempStr;

            // 【fgui】
            tempStr = winLockBalance / 10000 > 0 ? $"<font color='#F8DF1B'>{winLockBalance / 10000}</font>" : $"<font color='#FF0000'>{winLockBalance / 10000}</font>";

            //Debug.LogError($"【Test】：winLockBalance: {winLockBalance};  winLockBalance 除以 10000: {winLockBalance / 10000}");

            //Debug.LogError($"【Test】：tempStr: {tempStr};");

            return $"{GlobalData.hotfixVersion}/{AlgorithmVer}/{DifficultyName}/{tempStr}";
        }
    }



    public string DifficultyName
    {
        get
        {

            if (SboxConfData.difficulty < difficultyNames.Count)
            {
                return difficultyNames[SboxConfData.difficulty];
            }
            return "--";
        }

    }

    /// <summary> 游戏难度 最小值 </summary>
    public readonly List<string> difficultyNames = new List<string>() { "980", "970", "960", "950", "930" };




    /// <summary>
    /// 是否激活
    /// </summary>
    public bool isMachineActive = true;



    /// <summary>
    /// 0：无任何修改参数的权限，1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限
    /// </summary>
    //public int permissions = 0;
  

    public bool isCurPermissionsAdmin => curPermissions == 3;
    public bool isCurPermissionsManager => curPermissions == 2;
    public bool isCurPermissionsShift => curPermissions == 1;

    /// <summary> 当前权限  1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限</summary>
    public int curPermissions = -1;


    public int passwordAdmin
    {
        get
        {
            if (_pwdAdminCache == null)
                _pwdAdminCache = PlayerPrefs.GetInt(PARAM_ADMIN_PWD_CACHE, 187653214);
            return (int)_pwdAdminCache;
        }
        set
        {
            _pwdAdminCache = value;
            PlayerPrefs.SetInt(PARAM_ADMIN_PWD_CACHE, value);
        }
    }
    int? _pwdAdminCache = null;
    const string PARAM_ADMIN_PWD_CACHE = "PARAM_ADMIN_PWD_CACHE";

    //public int passwordAdmin;

    public int passwordManager;

    public int passwordShift;





    /// <summary> 座位id </summary>


    /// <summary>  支持的语言  </summary>
    public TableSupportLanguageItem[] supportLanguage = TableSupportLanguageItem.DefaultTable();


    [SerializeField]
    private TableBetItem m_TableBet;

    public TableBetItem tableBet
    {
        get => observable.GetProperty(() => m_TableBet);
        set => observable.SetProperty(ref m_TableBet, value);
    }

    [SerializeField]
    private List<BetAllow> m_BetAllowList;
    public List<BetAllow> betAllowList
    {
        get => observable.GetProperty(() => m_BetAllowList);
        set => observable.SetProperty(ref m_BetAllowList, value);
    }


    [SerializeField]
    private List<long> m_BetList;
    public List<long> betList
    {
        get => observable.GetProperty(() => m_BetList);
        set => observable.SetProperty(ref m_BetList, value);
    }









    [SerializeField]
    private TableSysSettingItem m_TableSysSetting;
    public TableSysSettingItem tableSysSetting
    {
        get => observable.GetProperty(() => m_TableSysSetting);
        set => observable.SetProperty(ref m_TableSysSetting, value);
    }



    /// <summary> 多语言编号 </summary>
    public string language
    {
        get => tableSysSetting.language_number;
        set => tableSysSetting.language_number = value;
    }
 

    /// <summary> 当前多语言名称 </summary>
    public string languageName
    {
        get {
            foreach (TableSupportLanguageItem  item in supportLanguage )
            {
                if (item.number == language)
                {
                    return item.name;
                }
            }
            return "";
        }
    }


    #region 音乐

    /// <summary> 音效 </summary>
    public float sound
    {
        //get => tableSysSetting.sound;
        //set => tableSysSetting.sound = value;

        get => GSManager.Instance.TotalVolumeEff;
        set => GSManager.Instance.SetTotalVolumEfft(value);
    }

    /// <summary> 背景音乐 </summary>
    public float music
    {
        //get => tableSysSetting.music;
        //set => tableSysSetting.music = value;
        get => GSManager.Instance.TotalVolumeMusic;
        set => GSManager.Instance.SetTotalVolumMusic(value);
    }

    /// <summary> 是否静音 </summary>
    public bool isMute
    {
        //get => tableSysSetting.sound_enable == 1;
        //set => tableSysSetting.sound_enable = value ? 1 : 0;

        get => GSManager.Instance.IsMute;
        set => GSManager.Instance.SetMute(value);   
    }



    /// <summary> 声音快捷设置 </summary>
    public int soundLevel
    {
        get
        {
            if (isMute)  // if (music <= 0)
                return 0;
            else if (music <= 0.35f)
                return 1;
            else if (music <= 0.65f)
                return 2;
            else
                return 3;
        }
        set
        {
            
            if (value <= 0)
            {
                //music = 0f;
                //sound = 0f;
                isMute = true;
            }
            else if (value == 1)
            {
                isMute = false;
                music = 0.35f;
                sound = 0.35f;
            }
            else if (value == 2)
            {
                isMute = false;
                music = 0.65f;
                sound = 0.65f;
            }
            else if (value >= 3)
            {
                isMute = false;
                music = 1f;
                sound = 1f;
            }
        }
    }



    #endregion












    /// <summary> 最大游戏记录局数 </summary>
    public long gameRecordMax
    {
        get => tableSysSetting.max_game_record;
        set => tableSysSetting.max_game_record = value;
    }


    /// <summary> 最投退币记录次数 </summary>
    public long coinInOutRecordMax
    {
        get => tableSysSetting.max_coin_in_out_record;
        set => tableSysSetting.max_coin_in_out_record = value;
    }

    /// <summary> 最大报警信息记录局数 </summary>
    public long errorRecordMax
    {
        get => tableSysSetting.max_error_record;
        set => tableSysSetting.max_error_record = value;
    }

    /// <summary> 最大事件信息记录局数 </summary>
    public long eventRecordMax
    {
        get => tableSysSetting.max_event_record;
        set => tableSysSetting.max_event_record = value;
    }


    /// <summary> 最大彩金记录局数 </summary>
    public long jackpotRecordMax
    {
        get => tableSysSetting.max_jackpot_record;
        set => tableSysSetting.max_jackpot_record = value;
    }


    /// <summary> 最大盈利天数记录 </summary>
    public long businiessDayRecordMax
    {
        get => tableSysSetting.max_businiess_day_record;
        set => tableSysSetting.max_businiess_day_record = value;
    }



    #region 功能开关

    /// <summary>
    /// 是否使用即中即退
    /// </summary>
    public bool isCoinOutImmediately
    {
        get
        {
            if (_isCoinOutImmediately == null)
                _isCoinOutImmediately = 1 == SQLitePlayerPrefs03.Instance.GetInt(PARAM_IS_COIN_OUT_IMMEDIATELY, 0);
            return (bool)_isCoinOutImmediately;
        }
        set
        {
            _isCoinOutImmediately = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_IS_COIN_OUT_IMMEDIATELY, value ? 1 : 0);
        }
    }
    const string PARAM_IS_COIN_OUT_IMMEDIATELY = "PARAM_IS_COIN_OUT_IMMEDIATELY";
    bool? _isCoinOutImmediately;

    /// <summary>
    /// 是否使用iot
    /// </summary>
    public bool isUseIot
    {
        get
        {
            if (_isUseIot == null)
                _isUseIot = 1 == SQLitePlayerPrefs03.Instance.GetInt(PARAM_IS_USE_IOT, 0);
            return (bool)_isUseIot;
        }
        set
        {
            _isUseIot = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_IS_USE_IOT, value ? 1 : 0);
        }
    }
    const string PARAM_IS_USE_IOT = "PARAM_IS_USE_IOT";
    bool? _isUseIot;


    #endregion






    #region 调试参数
    /// <summary> 是否显示调试信息 </summary>
    public bool isDebugLog
    {
        get
        {
           //#seaweed#
           if (ApplicationSettings.Instance.isRelease)  return false;

            m_IsDebugLog = tableSysSetting.is_debug == 1;
            return m_IsDebugLog;
        }
        set
        {
            //#seaweed#
            if (ApplicationSettings.Instance.isRelease)  return;

            if (m_IsDebugLog != value)
                tableSysSetting.is_debug = value ? 1 : 0;
            observable.SetProperty(ref m_IsDebugLog, value);
        }
    }
    bool m_IsDebugLog = true;


    /// <summary> 是否显示跟新的内容 </summary>
    public bool isUpdateInfo
    {
        get => tableSysSetting.is_update_info == 1;
        set => tableSysSetting.is_update_info = value ? 1 : 0;
    }

    /// <summary> 是否显示日志界面 </summary>
    public bool isUseReporterPage
    {
        get{
            //#seaweed#
            if (ApplicationSettings.Instance.isRelease)   return false;

            return tableSysSetting.enable_reporter_page == 1;
        }
        set
        {
            //#seaweed#
            if (ApplicationSettings.Instance.isRelease)   return;

            tableSysSetting.enable_reporter_page = value ? 1 : 0;
        }
    }

    /// <summary> 是否显示测试工具 </summary>
    public bool isUseTestTool
    {
        get
        {
            //#seaweed#
            if (ApplicationSettings.Instance.isRelease)  return false;

            return tableSysSetting.enable_test_tool == 1;
        }
        set
        {
            //#seaweed#
            if (ApplicationSettings.Instance.isRelease)   return;

            tableSysSetting.enable_test_tool = value ? 1 : 0;
        }
    }


    #endregion




    public bool isJackpotOnLine = false;



    #region 远程控制

    /// <summary>
    /// 使用远端控制默认配置
    /// </summary>
    /// <remarks>
    /// * broker.hivemq.com:1883
    /// </remarks>
    public bool isUseRemoteDefault = false;

    public bool isUseRemoteControl
    {
        get
        {
            if (_isUseRemoteControl == null)
                _isUseRemoteControl = 1 == SQLitePlayerPrefs03.Instance.GetInt(PARAM_IS_USE_REMOTE_CONTROL_01, 1);
            return (bool)_isUseRemoteControl;
        }
        set
        {
            _isUseRemoteControl = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_IS_USE_REMOTE_CONTROL_01, value ? 1 : 0);
        }
    }
    const string PARAM_IS_USE_REMOTE_CONTROL_01 = "PARAM_IS_USE_REMOTE_CONTROL_01";
    bool? _isUseRemoteControl;

    public string remoteControlSetting
    {
        get
        {
            if (isUseRemoteDefault)
                return "broker.hivemq.com:1883";

            if (string.IsNullOrEmpty(_remoteControlSetting))
                _remoteControlSetting = SQLitePlayerPrefs03.Instance.GetString(PARAM_REMOTE_CONTROL_SETTING_01, "1.117.19.172:1883");
            return _remoteControlSetting;
        }
        set
        {
            isUseRemoteDefault = false;

            _remoteControlSetting = value;
            SQLitePlayerPrefs03.Instance.SetString(PARAM_REMOTE_CONTROL_SETTING_01, value);
        }
    }
    const string PARAM_REMOTE_CONTROL_SETTING_01 = "PARAM_REMOTE_CONTROL_SETTING_01";
    string _remoteControlSetting = null;


    public string remoteControlAccount
    {
        get
        {
            if (string.IsNullOrEmpty(_remoteControlAccount))
                _remoteControlAccount = SQLitePlayerPrefs03.Instance.GetString(PARAM_REMOTE_CONTROL_ACCOUNT_01, "fwzz");
            return _remoteControlAccount;
        }
        set
        {
            _remoteControlAccount = value;
            SQLitePlayerPrefs03.Instance.SetString(PARAM_REMOTE_CONTROL_ACCOUNT_01, value);
        }
    }
    const string PARAM_REMOTE_CONTROL_ACCOUNT_01 = "PARAM_REMOTE_CONTROL_ACCOUNT_01";
    string _remoteControlAccount = null;


    public string remoteControlPassword
    {
        get
        {
            if (string.IsNullOrEmpty(_remoteControlPassword))
                _remoteControlPassword = SQLitePlayerPrefs03.Instance.GetString(PARAM_REMOTE_CONTROL_PASSWORD_01, "Fwzz@2024");
            return _remoteControlPassword;
        }
        set
        {
            _remoteControlPassword = value;
            SQLitePlayerPrefs03.Instance.SetString(PARAM_REMOTE_CONTROL_PASSWORD_01, value);
        }
    }
    const string PARAM_REMOTE_CONTROL_PASSWORD_01 = "PARAM_REMOTE_CONTROL_PASSWORD_01";
    string _remoteControlPassword = null;



    public bool isConnectRemoteControl
    {

        get
        {
            if (_isUseRemoteControl == false)
            {
                return false;
            }
            return _isConnectRemoteControl;
        }
        set
        {
            observable.SetProperty(ref _isConnectRemoteControl, value);
        }
    }
    bool _isConnectRemoteControl = true;




    public string remoteSecretkey = "123456";

    #endregion




    /*
    /// <summary> 当前权限  1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限</summary>
    public int curPermissions = -1;

    /// <summary> 当前是超级管理员身份？ </summary>
    public bool isCurAdministrator => curPermissions == 3;
    */




    #region 打印机

    /// <summary> 选择的打印机 </summary>
    public int SelectPrinterIndex
    {
        get
        {
            if (_selectPrinterIndex == null)
                _selectPrinterIndex = PlayerPrefs.GetInt(PARAM_SELECT_PRINTER, 0);
            return (int)_selectPrinterIndex;
        }
        set
        {
            _selectPrinterIndex = value;
            PlayerPrefs.SetInt(PARAM_SELECT_PRINTER, value);
        }
    }
    int? _selectPrinterIndex = null;
    const string PARAM_SELECT_PRINTER = "PARAM_SELECT_PRINTER";





    public bool IsConnectPrinter
    {
        get
        {
            return _isConnectPrinter;
        }
        set => observable.SetProperty(ref _isConnectPrinter, value);
        /*set
        {
            if (_isConnectPrinter != value) { 
                _isConnectPrinter = value;
                Messenger.Broadcast<bool>(Messenger.EventEnum.Logic, "SBoxModel/IsConnectPrinter", value);
            }
        }*/

    }
    bool _isConnectPrinter = false;




    /// <summary> 是否使用打印机 </summary>
    public bool isUsePrinter
    {
        get
        {
            if (m_IsUsePrinter == null)
                m_IsUsePrinter = 1 == SQLitePlayerPrefs03.Instance.GetInt(PARAM_IS_USE_PRINTER, 0);
            return (bool)m_IsUsePrinter;
        }
        set
        {
            m_IsUsePrinter = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_IS_USE_PRINTER, value ? 1 : 0);
        }
    }
    const string PARAM_IS_USE_PRINTER = "PARAM_IS_USE_PRINTER";
    bool? m_IsUsePrinter;


    /// <summary> 打印机初始化完成 </summary>
    public bool isInitPrinter = false;
    /// <summary> 打印机列表 </summary>
    public List<string> printerList = new List<string>();




    public List<DeviceInfo> supportPrinters
    {
        get
        {
            List<DeviceInfo> deviceInfo = new List<DeviceInfo>();
            for (int i = 0; i < printerList.Count; i++)
            {
                string[] str = printerList[i].Split(':');
                deviceInfo.Add(new DeviceInfo()
                {
                    number = i,
                    model = str[1] ?? "--",
                    manufacturer = str[0] ?? "--",
                });
            }
            return deviceInfo;
        }
    }

    /// <summary> 已选打印机型号 </summary>
    public int selectPrinterNumber
    {
        get
        {
            if (_selectPrinterNumber == null)
                _selectPrinterNumber = SQLitePlayerPrefs03.Instance.GetInt(PARAM_SELECT_PRINTER_NUMBER, 0);
            return (int)_selectPrinterNumber;
        }
        set
        {
            _selectPrinterNumber = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_SELECT_PRINTER_NUMBER, value);
        }
    }
    int? _selectPrinterNumber;
    const string PARAM_SELECT_PRINTER_NUMBER = "PARAM_SELECT_PRINTER_NUMBER";


    public string selectPrinterModel
    {
        get
        {
            if (printerList.Count > 0 && printerList.Count > selectPrinterNumber)
            {
                return printerList[selectPrinterNumber].Split(':')[1];
            }
            return "--";
        }
    }


    #endregion






    #region 纸钞机

    /// <summary> 已选纸钞机型号 </summary>
    public int selectBillerNumber
    {
        get
        {
            if (m_selectBillerNumber == null)
                m_selectBillerNumber = SQLitePlayerPrefs03.Instance.GetInt(PARAM_SELECT_BILLER_NUMBER, 0);
            return (int)m_selectBillerNumber;
        }
        set
        {
            m_selectBillerNumber = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_IS_USE_BILLER, value);
        }
    }
    const string PARAM_SELECT_BILLER_NUMBER = "PARAM_SELECT_BILLER_NUMBER";
    [SerializeField]
    int? m_selectBillerNumber;


    /// <summary> 是否使用纸钞机 </summary>
    public bool isUseBiller
    {
        get
        {
            if (m_IsUseBiller == null)
                m_IsUseBiller = 1 == SQLitePlayerPrefs03.Instance.GetInt(PARAM_IS_USE_BILLER, 0);
            return (bool)m_IsUseBiller;
        }
        set
        {
            m_IsUseBiller = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_IS_USE_BILLER, value ? 1 : 0);
        }
    }
    const string PARAM_IS_USE_BILLER = "PARAM_IS_USE_BILLER";
    [SerializeField]
    bool? m_IsUseBiller;



    /// <summary> 纸钞机列表 </summary>
    public List<string> billerList = new List<string>();


    /// <summary> 选择的打印机 </summary>
    public int SelectBillerIndex
    {
        get
        {
            if (_selectBillerIndex == null)
                _selectBillerIndex = PlayerPrefs.GetInt(PARAM_SELECT_BILLER, 0);
            return (int)_selectBillerIndex;
        }
        set
        {
            _selectBillerIndex = value;
            PlayerPrefs.SetInt(PARAM_SELECT_BILLER, value);
        }
    }
    int? _selectBillerIndex = null;
    const string PARAM_SELECT_BILLER = "PARAM_SELECT_BILLER";

    public bool IsConnectBiller
    {
        get
        {
            return _isConnectBiller;
        }
        set => observable.SetProperty(ref _isConnectBiller, value);
        /*set
        {
            if (_isConnectBiller != value)
            {
                _isConnectBiller = value;
                Messenger.Broadcast<bool>(Messenger.EventEnum.Logic, "SBoxModel/IsConnectBiller", value);
            }
        }*/
    }
    bool _isConnectBiller = false;

    public bool isInitBiller = false;


    public string selectBillerModel
    {
        get
        {
            if (billerList.Count > 0 && billerList.Count > selectBillerNumber)
            {
                return billerList[selectBillerNumber].Split(':')[1];
            }
            return "--";
        }
    }


    public List<DeviceInfo> suppoetBillers
    {

        get
        {
            List<DeviceInfo> deviceInfo = new List<DeviceInfo>();
            for (int i = 0; i < billerList.Count; i++)
            {
                string[] str = billerList[i].Split(':');
                deviceInfo.Add(new DeviceInfo()
                {
                    number = i,
                    model = str[1] ?? "--",
                    manufacturer = str[0] ?? "--",
                });
            }
            return deviceInfo;
        }
    }



    #endregion

    public int BillInScale
    {
        get => CoinInScale;
        set => CoinInScale = value;
    }
}

