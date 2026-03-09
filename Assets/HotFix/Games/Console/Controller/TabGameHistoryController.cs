using FairyGUI;
using SBoxApi;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGameHistoryController : MonoBehaviour
{
    GComponent goOwnerTab;
    GameHistoryDataController ctrl = new GameHistoryDataController();
    public List<GLoader> Symbols = new List<GLoader>(); //滚轮图标
    GRichTextField _rtxtgame_uid, _rtxtcreated_at, _rtxtcredit_before, _rtxtcredit_after, _rtxttotal_bet, _rtxtbase_game_win_credit, _rtxtjackpot_win_credit, _rtxtopen_type, _rtxtresult_type,
        _rtxtgame_name, _rtxtgame_page;

    // 当前显示的数据
    private GameHistoryInfo currentPageInfo;
    //当前记录的符号映射
    private Dictionary<string, string> currentSymbolIconMap;
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

    public void InitParam(GComponent go, string tabName,Action<List<string>> onDatesChange,Action<List<long>> onGameIdsChange)
    {
        goOwnerTab = go;
        GComponent reels = go.GetChild("reels").asCom;
        Symbols.Clear();
        for (int i = 0; i < 15; ++i)
        {
            GLoader img = reels.GetChildAt(i).asCom.GetChild("image").asLoader;
            Symbols.Add(img);
        }

        _rtxtgame_uid = go.GetChild("game_uid").asCom.GetChild("value").asRichTextField;
        _rtxtcreated_at = go.GetChild("created_at").asCom.GetChild("value").asRichTextField;
        _rtxtcredit_before = go.GetChild("credit_before").asCom.GetChild("value").asRichTextField;
        _rtxtcredit_after = go.GetChild("credit_after").asCom.GetChild("value").asRichTextField;
        _rtxttotal_bet = go.GetChild("total_bet").asCom.GetChild("value").asRichTextField;
        _rtxtbase_game_win_credit = go.GetChild("base_game_win_credit").asCom.GetChild("value").asRichTextField;
        _rtxtjackpot_win_credit = go.GetChild("jackpot_win_credit").asCom.GetChild("value").asRichTextField;
        _rtxtopen_type = go.GetChild("open_type").asCom.GetChild("value").asRichTextField;
        _rtxtresult_type = go.GetChild("result_type").asCom.GetChild("value").asRichTextField;
        _rtxtgame_name = go.GetChild("game_page").asCom.GetChild("key").asRichTextField;
        _rtxtgame_page = go.GetChild("game_page").asCom.GetChild("value").asRichTextField;

        ctrl.InitParam(tabName, onDatesChange, onGameIdsChange, onPageChagne);
    }

    // 游戏ID改变时调用
    public void OnGameIdChanged(long gameId)
    {
        currentGameId = gameId;
        ctrl.LoadDateTimesByGameId(gameId);
    }

    // 日期时间改变时调用
    public void OnDateTimeChanged(long gameId, string dateTime,int selectedIndex)
    {
        ctrl.QueryByGameIdAndDateTime(gameId, dateTime, selectedIndex);
    }

    void onPageChagne(GameHistoryInfo pageInfo)
    {
        currentPageInfo = pageInfo;

        // 如果有数据，显示符号
        if (pageInfo.currentRecord != null && !string.IsNullOrEmpty(pageInfo.currentRecord.strDeckRowCol))
        {
            // 解析符号映射
            ParseSymbolIconMap(pageInfo.currentRecord.symbol_icon_mapping);

            // 显示时间信息
            DateTime dt = DateTime.ParseExact(pageInfo.currentDateTime, "yyyy-MM-dd HH:mm:ss", null);

            // 解析符号布局
            List<int> deckColRow = SlotTool.GetDeckRowCol(pageInfo.currentRecord.strDeckRowCol);

            // 更新15个位置的图标
            for (int i = 0; i < 15 && i < Symbols.Count; ++i)
            {
                int symbolId = deckColRow[i];
                SetSymbolImage(symbolId, i);
            }

            // 更新文本信息
            _rtxtgame_uid.text = $"{pageInfo.currentRecord.game_uid}";
            _rtxtcreated_at.text = $"{dt.ToString("yyyy-MM-dd HH:mm:ss")}";
            _rtxtcredit_before.text = $"{pageInfo.currentRecord.credit_before}";
            _rtxtcredit_after.text = $"{pageInfo.currentRecord.credit_after}";
            _rtxttotal_bet.text = $"{pageInfo.currentRecord.total_bet}";
            _rtxtbase_game_win_credit.text = $"{pageInfo.currentRecord.base_game_win_credit}";
            _rtxtjackpot_win_credit.text = $"{pageInfo.currentRecord.jackpot_win_credit}";
            _rtxtopen_type.text = $"{pageInfo.currentRecord.open_type}";
            _rtxtresult_type.text = $"{pageInfo.currentRecord.result_type}";
            if (SBoxModel.Instance.language == "cn")
            {
                _rtxtgame_name.text = $"{pageInfo.currentRecord.game_id}游戏记录";
                _rtxtgame_page.text = $"第{pageInfo.curPageNumber}/{pageInfo.totalPageCount}页";
            }
            else
            {
                _rtxtgame_name.text = $"{pageInfo.currentRecord.game_id} GameHistory";
                _rtxtgame_page.text = $"Page {pageInfo.curPageNumber} of {pageInfo.totalPageCount}";
            }
           
        }
        else
        {
            // 没有数据时清空图标
            ClearSymbols();
            currentSymbolIconMap = null;
        }
    }

    public void SetSymbolImage(int symbolNumber, int index)
    {
        string iconUrl = GetIconUrlBySymbolId(symbolNumber);

        if (string.IsNullOrEmpty(iconUrl))
        {
            DebugUtils.LogWarning($"符号 {symbolNumber} 没有对应的图标URL");
            Symbols[index].url = "";
        }
        else
        {
            Symbols[index].url = iconUrl;
            //DebugUtils.Log($"设置位置 {index} 的符号 {symbolNumber} 为: {iconUrl}");
        }
    }

    // 根据符号ID获取图标URL
    private string GetIconUrlBySymbolId(int symbolId)
    {
        string symbolKey = symbolId.ToString();

        // 优先使用从数据库读取的符号映射
        if (currentSymbolIconMap != null && currentSymbolIconMap.ContainsKey(symbolKey))
        {
            return currentSymbolIconMap[symbolKey];
        }
        else
        {
            return null;
        }
    }

    // 清空所有图标
    private void ClearSymbols()
    {
        foreach (var img in Symbols)
        {
            img.url = "";
        }
    }

    // 下一页
    public void NextPage()
    {
        ctrl.NextPage();
    }

    // 上一页
    public void PrevPage()
    {
        ctrl.PrevPage();
    }

    // 第一页
    public void FirstPage()
    {
        ctrl.FirstPage();
    }

    // 最后一页
    public void LastPage()
    {
        ctrl.LastPage();
    }

    // 清空显示
    public void ClearDisplay()
    {
        ctrl.ClearDisplay();
    }


    // 解析符号映射的方法
    private void ParseSymbolIconMap(string symbolIconMappingJson)
    {
        if (string.IsNullOrEmpty(symbolIconMappingJson))
        {
            currentSymbolIconMap = null;
            return;
        }

        try
        {
            // 解析JSON为字典
            currentSymbolIconMap = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(symbolIconMappingJson);
            DebugUtils.Log($"成功解析符号映射，共 {currentSymbolIconMap.Count} 个符号");
        }
        catch (Exception e)
        {
            DebugUtils.LogError($"解析符号映射失败: {e.Message}");
            currentSymbolIconMap = null;
        }
    }
}