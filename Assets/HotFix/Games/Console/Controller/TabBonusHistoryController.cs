using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabBonusHistoryController : MonoBehaviour
{
    private GComponent _goOwnerTab;
    private GameHistoryDataController _controllers = new GameHistoryDataController();
    GComponent _bonusPageCom;
    GTextField _rtxtbonusgame_name, _rtxtbonusgame_page;
    
    // 当前显示的数据
    private GameHistoryInfo currentPageInfo;
    
    // 当前游戏ID
    private long currentGameId = 1700;
    
    // 游戏ID对应的包名映射
    private Dictionary<long, string> gamePackageMap = new Dictionary<long, string>
    {
        { 1700, "SlotZhuZaiJinBi1700" },
        { 200, "PusherEmperorsRein200" },
        { 3998, "XingYunZhiLun3998" },
        { 3996, "CaiFuHuoChe3996" },
        { 3997, "CaiFuZhiJia3997" },
        { 3999, "CaiFuZhiMen3999" },
    };
}