using FairyGUI;
using System;
using System.Collections.Generic;

public class TableJackpotOnlineHistory
{
    private readonly JackpotOnlineHistoryDataController _ctrl = new JackpotOnlineHistoryDataController();
    private readonly List<GComponent> _rows = new List<GComponent>();
    private GTextField _txtGamePageKey;
    private GTextField _txtGamePageValue;
    private const int perPageNumCoinInOut = 20;
    private int fromIdxCoinInOut = 0;
    private List<TableJackpotRecordItem> resJackpotRecord = new List<TableJackpotRecordItem>();

    public void InitParam(GComponent go, string tabName, Action<List<string>> onDatesChange)
    {
        _rows.Clear();
        GList dataPages = go.GetChild("game_data").asList;
        for (int i = 0; i < dataPages.numItems; i++)
        {
            _rows.Add(dataPages.GetChildAt(i).asCom);
            _rows[i].visible = false;
        }

        GComponent gamePage = go.GetChild("game_page").asCom;
        _txtGamePageKey = gamePage.GetChild("key").asTextField;
        _txtGamePageValue = gamePage.GetChild("value").asTextField;

        _ctrl.InitParam(tabName, onDatesChange, OnPageChange);
    }

    // 日期改变时调用
    public void OnDateTimeChanged(string dateTime, int selectedIndex)
    {
        _ctrl.QueryByDate(dateTime, 0);
    }

    private void OnPageChange(JackpotOnlineHistoryPageInfo pageInfo)
    {
        fromIdxCoinInOut = 0;
        resJackpotRecord = pageInfo.pageRecords ?? new List<TableJackpotRecordItem>();
        SetUIJackpotOnline();

        if (SBoxModel.Instance.language == "cn")
        {
            _txtGamePageKey.text = "联网彩金记录";
            _txtGamePageValue.text = $"第{pageInfo.curPageNumber}/{pageInfo.totalPageCount}页";
        }
        else
        {
            _txtGamePageKey.text = "Jackpot Online History";
            _txtGamePageValue.text = $"Page {pageInfo.curPageNumber} of {pageInfo.totalPageCount}";
        }
    }

    /// <summary>
    /// 显示联网彩金内容
    /// </summary>
    void SetUIJackpotOnline()
    {
        if (resJackpotRecord == null || resJackpotRecord.Count <= 0)
        {
            foreach (GComponent item in _rows)
            {
                item.visible = false;
            }
            return;
        }

        int lastIdx = fromIdxCoinInOut + perPageNumCoinInOut - 1;
        if (lastIdx > resJackpotRecord.Count - 1)
        {
            lastIdx = resJackpotRecord.Count - 1;
        }

        foreach (GComponent item in _rows)
        {
            item.visible = false;
        }
        for (int i = 0; i <= lastIdx - fromIdxCoinInOut; i++)
        {
            GComponent item = _rows[i];
            item.visible = true;
            TableJackpotRecordItem res = resJackpotRecord[i + fromIdxCoinInOut];

            item.GetChild("bonusType").asTextField.text = res.jp_name;
            item.GetChild("gameId").asTextField.text = res.game_id.ToString();
            item.GetChild("points").asTextField.text = res.win_credit.ToString();
            item.GetChild("beforeScore").asTextField.text = res.credit_before.ToString();
            item.GetChild("afterScore").asTextField.text = res.credit_after.ToString();
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(res.created_at);
            DateTime localDateTime = dateTimeOffset.LocalDateTime;
            item.GetChild("time").asTextField.text = localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public void PrevPage()
    {
        _ctrl.PrevPage();
    }

    public void NextPage()
    {
        _ctrl.NextPage();
    }
}
