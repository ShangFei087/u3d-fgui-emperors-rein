using FairyGUI;
using SimpleJSON;
using SlotMaker;
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
            _gameID = value;
        }
    }

    public long myCredit;

    public string gameName;

    public string displayName;




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
