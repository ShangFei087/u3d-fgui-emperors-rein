using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix.Games.Console.Controller
{
    public class BonusHistoryInfo
    {
        public int curPageNumber; // 当前页码
        public int totalPageCount; // 总页数
        public string currentDateTime; // 当前日期时间（精确到秒）
        public TableJackpotRecordItem currentRecord; // 当前显示记录

        public int currentIndex; // 当前记录在列表中的索引
        public int totalRecordCount; // 总记录数
        public List<TableJackpotRecordItem> allRecords; // 所有记录
        public long currentGameId; // 当前游戏ID
    }

    public class BonusHistoryDataController
    {
        private string _tabName = ConsoleTableName.TABLE_JACKPOT_RECORD;
        private List<string> _dropDownDateTimeList;
        private int _curPageIndex = 0, _totalPageCount = 0, _totalRecordCount = 0;// 当前页数（从0开始） 总的页数  总记录数
        private const string DatetimeFormat = "yyyy-MM-dd HH:mm:ss";

        private Action<List<string>> _onDatesChanged;
        private Action<BonusHistoryInfo> _onPageChanged;

        private string _currentSelectDateTime = ""; // 存储当前选中的日期时间
        private List<TableJackpotRecordItem> _allRecords = new List<TableJackpotRecordItem>(); // 当前查询的所有记录

        public void InitParam(string tabName,
            Action<List<string>> onDatesChange,
            /*Action<List<long>> onGameIdsChange,*/
            Action<BonusHistoryInfo> onPageChange)
        {
            this._tabName = tabName;
            this._onDatesChanged = onDatesChange;
            // this.onGameIdsChange = onGameIdsChange;
            this._onPageChanged = onPageChange;
        }

        public void PrevPage()
        {
            Debug.LogError("Bonus PrevPage");
        }
        public void NextPage()
        {
            Debug.LogError("Bonus NextPage");
        }
    }
}