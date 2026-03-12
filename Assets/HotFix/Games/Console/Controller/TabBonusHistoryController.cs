using FairyGUI;
using System;
using System.Collections.Generic;

namespace HotFix.Games.Console.Controller
{
    public class BonusData
    {
        private GTextField _bonusType, _gameId, _points, _beforeScore, _afterScore, _time;

        public BonusData(GTextField bonusType, GTextField gameId, GTextField points, GTextField beforeScore,
            GTextField afterScore, GTextField time)
        {
            _bonusType = bonusType;
            _gameId = gameId;
            _points = points;
            _beforeScore = beforeScore;
            _afterScore = afterScore;
            _time = time;
        }
    }

    public class TabBonusHistoryController
    {
        private readonly BonusHistoryDataController _bonusHistoryDataController = new BonusHistoryDataController();

        private GTextField _txtGamePageKey,
            _txtGamePageValue,
            _txtBonusType,
            _txtGameId,
            _txtPoints,
            _txtBeforeScore,
            _txtAfterScore,
            _txtTime;

        public void InitParam(GComponent go, string tabName, Action<List<string>> onDatesChange /*,
            Action<List<long>> onGameIdsChange*/)
        {
            GComponent dataCom = go.GetChild("game_data").asCom;
            _txtBonusType = dataCom.GetChild("bonusType").asTextField;
            _txtGameId = dataCom.GetChild("gameId").asTextField;
            _txtPoints = dataCom.GetChild("points").asTextField;
            _txtBeforeScore = dataCom.GetChild("beforeScore").asTextField;
            _txtAfterScore = dataCom.GetChild("afterScore").asTextField;
            _txtTime = dataCom.GetChild("time").asTextField;

            _txtGamePageKey = go.GetChildAt(0).asCom.GetChild("game_page").asCom.GetChild("key")
                .asRichTextField;
            _txtGamePageValue = go.GetChildAt(0).asCom.GetChild("game_page").asCom.GetChild("value")
                .asRichTextField;

            _bonusHistoryDataController.InitParam(tabName, onDatesChange, OnPageChange);
        }

        public void OnDateTimeChanged(long gameId, string dateTime, int selectedIndex)
        {
        }

        private void OnPageChange(BonusHistoryInfo pageInfo)
        {
            if (pageInfo.currentRecord != null)
            {
                // 显示时间信息
                DateTime dt = DateTime.ParseExact(pageInfo.currentDateTime, "yyyy-MM-dd HH:mm:ss", null);

                _txtBonusType.text = $"{pageInfo.currentRecord.jp_name}";
                _txtGameId.text = $"{pageInfo.currentRecord.game_id}";
                _txtPoints.text = $"{pageInfo.currentRecord.win_credit}";
                _txtBeforeScore.text = $"{pageInfo.currentRecord.credit_before}";
                _txtAfterScore.text = $"{pageInfo.currentRecord.credit_after}";
                _txtTime.text = $"{dt:yyyy-MM-dd HH:mm:ss}";

                if (SBoxModel.Instance.language == "cn")
                {
                    _txtGamePageKey.text = $"{pageInfo.currentRecord.game_id}彩金记录";
                    _txtGamePageValue.text = $"第{pageInfo.curPageNumber}/{pageInfo.totalPageCount}页";
                }
                else
                {
                    _txtGamePageKey.text = $"{pageInfo.currentRecord.game_id} BonusHistory";
                    _txtGamePageValue.text = $"Page {pageInfo.curPageNumber} of {pageInfo.totalPageCount}";
                }
            }
        }

        public void PrevPage()
        {
            _bonusHistoryDataController.PrevPage();
        }

        public void NextPage()
        {
            _bonusHistoryDataController.NextPage();
        }
    }
}