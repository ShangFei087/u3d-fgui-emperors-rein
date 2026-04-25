using FairyGUI;
using SimpleJSON;
using SlotMaker;
using SBoxApi;
using UnityEngine;

public class MainModel : MonoSingleton<MainModel>
{


    public int _gameID = 200;
    public int gameID
    {
        get
        {
            return _gameID;
        }
        set
        {
            if (_gameID == value)
                return;

            _gameID = value;

            // 当切换游戏时，刷新该游戏对应的 betAllowList/betList。
            // Panel/下注逻辑依赖 betList，因此需要随游戏同步更新。
            TryRefreshBetAllowListByGameId();
        }
    }

    void TryRefreshBetAllowListByGameId()
    {
        try
        {
            // bet 表在启动初始化流程里会先加载一次；
            // 这里用 tableBet.game_id 做二次刷新判断，避免重复查询。
            if (SBoxModel.Instance == null || SBoxModel.Instance.tableBet == null)
                return;

            if (SBoxModel.Instance.tableBet.game_id == (long)_gameID)
                return;
            ConsoleTableUtils.ClearTableData(ConsoleTableName.TABLE_BET);
            ConsoleTableUtils.GetTableBet();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainModel] Refresh betAllowList failed, gameID={_gameID}, err={ex}");
        }
    }

    public long myCredit;

    public string gameName;

    public string displayName;

    public int _lineNum;
    public int lineNum
    {
        get
        {
            return _lineNum;
        }
        set
        {
            _lineNum = value;
        }
    }

    public bool isSpin
    {
        get
        {
            if (MainModel.Instance.contentMD == null )
                return false;
            return MainModel.Instance.contentMD.isSpin;
        }
    }

    public bool isAuto
    {
        get
        {
            if (MainModel.Instance.contentMD == null)
                return false;
            return MainModel.Instance.contentMD.isAuto;
        }
    }

    /// <summary>
    /// 展会模式（可运行时开关；未写入过偏好时默认与 ApplicationSettings 一致，写入后持久化）。
    /// </summary>
    public const string PARAM_EXPO_MODE = "PARAM_EXPO_MODE";
    bool _expoModeLoaded;
    bool _ExhibitionMode;

    void EnsureExpoModeLoaded()
    {
        if (_expoModeLoaded)
            return;
        _expoModeLoaded = true;
        int def = ApplicationSettings.Instance.IsExpoMode() ? 1 : 0;
        _ExhibitionMode = SQLitePlayerPrefs03.Instance.GetInt(PARAM_EXPO_MODE, def) != 0;
    }

    public bool isExhibitionModeMode
    {
        get
        {
            EnsureExpoModeLoaded();
            return _ExhibitionMode;
        }
        set
        {
            EnsureExpoModeLoaded();
            if (_ExhibitionMode == value)
                return;
            _ExhibitionMode = value;
            SQLitePlayerPrefs03.Instance.SetInt(PARAM_EXPO_MODE, value ? 1 : 0);
        }
    }


    public bool isRequestToRealCreditWhenStop
    {
        get => false;
        set {
            if (contentMD != null)
            {
                contentMD.isRequestToRealCreditWhenStop = value;
            }
        }
    }


    public IContentModel _contentMD = null;
    public IContentModel contentMD
    {
        get
        {
            return _contentMD;
        }
        set
        {
            _contentMD = value;
        }
    }


    public ICustomModel _cutomMD = null;
    public ICustomModel cutomMD
    {
        get
        {
            return _cutomMD;
        }
        set
        {
            _cutomMD = value;
        }
    }


    public IPanel _panel = null;
    public IPanel panel
    {
        get
        {
            return _panel;
        }
        set
        {
            _panel = value;
        }
    }

    /// <summary>
    /// 数据上报编号
    /// </summary>
    public int reportId
    {
        get
        {
            if (reportIdNode == null)
            {
                string str = SQLitePlayerPrefs03.Instance.GetString(PARAM_REPORT_ID, "{}");
                reportIdNode = JSONNode.Parse(str);
            }
            string key = $"{gameID}";
            if (!reportIdNode.HasKey(key))
                reportIdNode[key] = 0;

            return (int)reportIdNode[key];
        }
        set
        {
            string key = $"{gameID}";
            if (!reportIdNode.HasKey(key))
                reportIdNode[key] = 0;

            if ((int)reportIdNode[key] != value)
            {
                reportIdNode[key] = value;
                SQLitePlayerPrefs03.Instance.SetString(PARAM_REPORT_ID, reportIdNode.ToString());
            }
        }
    }
    public JSONNode reportIdNode = null;
    public const string PARAM_REPORT_ID = "PARAM_REPORT_ID";



    /// <summary>
    /// 本局游戏编号
    /// </summary>
    public int gameNumber
    {
        get
        {
            if(gameNumberNode == null)
            {
                string str = SQLitePlayerPrefs03.Instance.GetString(PARAM_GAME_NUMBER, "{}");
                gameNumberNode = JSONNode.Parse(str);
            }
            string key = $"{gameID}";
            if (!gameNumberNode.HasKey(key))
                gameNumberNode[key] = 0;

            return (int)gameNumberNode[key];
        }
        set
        {
            string key = $"{gameID}";
            if (!gameNumberNode.HasKey(key))
                gameNumberNode[key] = 0;

            if((int)gameNumberNode[key] != value)
            {
                gameNumberNode[key] = value;
                SQLitePlayerPrefs03.Instance.SetString(PARAM_GAME_NUMBER, gameNumberNode.ToString());
            }
        }
    }
    public JSONNode gameNumberNode = null;
    public const string PARAM_GAME_NUMBER = "PARAM_GAME_NUMBER";



}
