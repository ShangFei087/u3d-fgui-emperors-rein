using FairyGUI;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGameHistoryController : MonoBehaviour
{
    GComponent goOwnerTab;
    GameHistoryDataController ctrl = new GameHistoryDataController();
    List<GComponent> goSymbols = new List<GComponent>();
    public List<GLoader> imgs = new List<GLoader>();

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

    public void InitParam(GComponent go, string tabName,Action<List<string>> onDatesChange,Action<List<long>> onGameIdsChange)
    {
        goOwnerTab = go;
        GComponent reels = go.GetChild("reels").asCom;

        for (int i = 0; i < 15; ++i)
        {
            GLoader img = reels.GetChildAt(i).asCom.GetChild("image").asLoader;
            imgs.Add(img);
        }

        ctrl.InitParam(tabName, onDatesChange, onGameIdsChange, onPageChagne);
    }

    // 游戏ID改变时调用
    public void OnGameIdChanged(long gameId)
    {
        currentGameId = gameId;
        ctrl.LoadDateTimesByGameId(gameId);
    }

    // 日期时间改变时调用
    public void OnDateTimeChanged(long gameId, string dateTime)
    {
        ctrl.QueryByGameIdAndDateTime(gameId, dateTime, 0);
    }

    void onPageChagne(GameHistoryInfo pageInfo)
    {
        currentPageInfo = pageInfo;

        // 如果有数据，显示符号
        if (pageInfo.currentRecord != null && !string.IsNullOrEmpty(pageInfo.currentRecord.strDeckRowCol))
        {
            // 解析符号布局
            List<int> deckColRow = SlotTool.GetDeckRowCol(pageInfo.currentRecord.strDeckRowCol);

            // 更新15个位置的图标
            for (int i = 0; i < 15 && i < imgs.Count; ++i)
            {
                int symbolId = deckColRow[i];
                SetSymbolImage(symbolId, i);
            }

            // 显示时间信息
            DateTime dt = DateTime.ParseExact(pageInfo.currentDateTime, "yyyy-MM-dd HH:mm:ss", null);
            DebugUtils.Log($"显示第{pageInfo.curPageNumber}/{pageInfo.totalPageCount}页，游戏ID：{pageInfo.currentGameId}，时间：{dt.ToString("yyyy-MM-dd HH:mm:ss")}");
        }
        else
        {
            // 没有数据时清空图标
            ClearSymbols();
        }
    }

    public void SetSymbolImage(int symbolNumber, int index)
    {
        // 这里需要根据实际的图标映射来设置
        // 示例：根据symbolNumber映射到对应的图片URL
        string iconUrl = GetIconUrlBySymbolId(symbolNumber);
        imgs[index].url = iconUrl;
    }

    // 根据符号ID获取图标URL
    private string GetIconUrlBySymbolId(int symbolId)
    {
        // 获取当前游戏对应的包名
        string packageName = gamePackageMap.ContainsKey(currentGameId) 
            ? gamePackageMap[currentGameId] 
            : "Console";

        // 返回对应包的图标URL
        // 注意：每个游戏的图标名称可能不同，这里需要根据实际图标名称进行调整
        // 以下是 SlotZhuZaiJinBi1700 的图标映射示例
        switch (symbolId) 
        {
            case 0: return $"ui://{packageName}/ng_sym_9";
            case 1: return $"ui://{packageName}/ng_sym_10";
            case 2: return $"ui://{packageName}/ng_sym_J";
            case 3: return $"ui://{packageName}/ng_sym_Q";
            case 4: return $"ui://{packageName}/ng_sym_K";
            case 5: return $"ui://{packageName}/ng_sym_A";
            case 6: return $"ui://{packageName}/ng_sym_card";
            case 7: return $"ui://{packageName}/ng_sym_wallet";
            case 8: return $"ui://{packageName}/ng_sym_safe";
            case 9: return $"ui://{packageName}/ng_sym_ptycoon";
            case 10: return $"ui://{packageName}/ng_sym_treasury";
            default: return $"ui://{packageName}/ng_sym_pMoneyjar"; // 默认图标
        }
    }

    // 清空所有图标
    private void ClearSymbols()
    {
        foreach (var img in imgs)
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
}