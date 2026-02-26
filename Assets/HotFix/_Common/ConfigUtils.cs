using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigUtils
{


    /// <summary> 当前游戏主题 </summary>
    //public string curGameName = "";


    /// <summary> 游戏id </summary>
    public static int curGameId => MainModel.Instance.gameID;
    

    /// <summary> 多语言文件名（不带后缀）</summary>
    public static readonly string[] i18nLoadFile = new string[] { "i18n_po152", "i18n_console001" };

    /// <summary> 游戏配置文件路劲 </summary>
    public static string GetGameInfoURL(int gameId) => $"Assets/GameRes/_Common/Game Maker/ABs/G{gameId}/Datas/game_info_g{gameId}.json";
    //$"Assets/GameRes/_Common/Game Maker/ABs/Datas/{gameId}/game_info_g{gameId}.json";

    /// <summary> 游戏图片路劲 </summary>
    public static string GetGameAvatarURL(int gameId) => $"Assets/GameBackup/Games/G{gameId}/Game Avatar/Game Avatar G{gameId}.png";
       // $"Assets/GameRes/_Common/Game Maker/ABs/{gameId}/Game Icon/Game Avatar {gameId}.png";



    public static string GetErrorCode() => "Assets/GameRes/_Common/Game Maker/ABs/Datas/error_code.json";

    /// <summary> 游戏GM文件路劲 </summary>
    public static string GetGameGMURL(int gameId) =>
        $"Assets/GameRes/_Common/Game Maker/ABs/G{gameId}/GM/tmg_mock_gm_{gameId}.json";
        //$"Assets/GameRes/_Common/Game Maker/ABs/{gameId}/Game Icon/Game Avatar {gameId}.png";
    
    /// <summary> 游戏信息 </summary>      
    public static string curGameGMURL => GetGameGMURL(curGameId);

    /// <summary> 游戏信息 </summary>
    public static string curGameInfoURL => GetGameInfoURL(curGameId);

    /// <summary> 游戏头像 </summary>
    public static string curGameAvatarURL => GetGameAvatarURL(curGameId);

    public static string GetGameAvararWebUrl(int gameId) => PathHelper.GetAssetBackupWEBURL(GetGameAvatarURL(gameId));
        //PathHelper.GetAssetBackupWEBURL($"Assets/GameBackup/Games/G{gameId}/Game Avatar/Game Avatar {gameId}.png");

    public static string curGameAvararWebUrl => GetGameAvararWebUrl(curGameId);
}
