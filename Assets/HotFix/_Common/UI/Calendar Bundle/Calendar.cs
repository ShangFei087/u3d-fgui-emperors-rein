
/*=========================================
* Author: springDong
* Description: SpringGUI.Calendar
* This component you only need to listen onDayClick/onMonthClick/onYearClick three interfaces
* Interface return CurrentDateTime class data.
==========================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using FairyGUI;


namespace SpringGUI
{

    public interface IVCalendar: IViewBase
    {
        event Action onClickHourAdd;
        event Action onClickHourReduce;
        event Action onClickMinuteAdd;
        event Action onClickMinuteReduce;
        event Action onClickTitle;
        event Action onClickNext;
        event Action onClickPrev;

        event Action<DateTime> onClickDay;
        event Action<DateTime> onClickMonth;
        event Action<DateTime> onClickYear;


        void SetHourTxt(string str);
        void SetMinuteTxt(string str);

        void SetSelectDateTxt(string lastSelectDate);

        void SetTitleTxt(string str);

        void RefreshUIDays(List<DateTime> relatedDTs, DateTime selectDT, DateTime confirmDT);

        void RefreshUIMonths(List<DateTime> relatedDTs, DateTime selectDT);

        void RefreshUIYears(List<DateTime> relatedDTs, DateTime selectDT);

        /// <summary> 日历显示内容面板（日期、月、年） </summary>
        void CalendarPanelTypeChange(E_CalendarPanelType panelType);
    }

    public abstract class CalendarViewBase:IVCalendar
    {


        public event Action onClickHourAdd;
        public event Action onClickHourReduce;

        public event Action onClickMinuteAdd;
        public event Action onClickMinuteReduce;
        public event Action onClickTitle;
        public event Action onClickNext;
        public event Action onClickPrev;

        public event Action<DateTime> onClickDay;
        public event Action<DateTime> onClickMonth;
        public event Action<DateTime> onClickYear;
        public abstract void SetHourTxt(string str);
        public abstract void SetMinuteTxt(string str);

        public abstract void SetSelectDateTxt(string lastSelectDate);

        public abstract void SetTitleTxt(string str);

        public abstract void RefreshUIDays(List<DateTime> relatedDTs, DateTime selectDT, DateTime confirmDT);

        public abstract void RefreshUIMonths(List<DateTime> relatedDTs, DateTime selectDT);

        public abstract void RefreshUIYears(List<DateTime> relatedDTs, DateTime selectDT);

        /// <summary> 日历显示内容面板（日期、月、年） </summary>
        public abstract void CalendarPanelTypeChange(E_CalendarPanelType panelType);


        public abstract void InitParam(GComponent uiOwner);

        //========================================


        protected virtual void TriggerOnClickHourAdd() => onClickHourAdd?.Invoke(); // 触发无参事件
        protected virtual void TriggerOnClickHourReduce() => onClickHourReduce?.Invoke();                           
        protected virtual void TriggerOnClickMinuteAdd() => onClickMinuteAdd?.Invoke();
        protected virtual void TriggerOnClickMinuteReduce() => onClickMinuteReduce?.Invoke();
        protected virtual void TriggerOnClickTitle() => onClickTitle?.Invoke();
        protected virtual void TriggerOnClickNext() => onClickNext?.Invoke();
        protected virtual void TriggerOnClickPrev() => onClickPrev?.Invoke();                          
        protected virtual void TriggerOnClickDay(DateTime data) => onClickDay?.Invoke(data);
        protected virtual void TriggerOnClickMonth(DateTime data) => onClickMonth?.Invoke(data);
        protected virtual void TriggerOnClickYear(DateTime data) => onClickYear?.Invoke(data);






        public static Color ParseColor(string hexColor = "#cccccc")
        {
            Color color;
            // 尝试解析十六进制颜色
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                //Debug.Log("转换成功: " + color);
            }
            else
            {
                // 解析失败
                //Debug.LogError("颜色转换失败，请检查十六进制格式");
            }
            return color;
        }


        /// <summary> 前灰色 </summary>
        public static readonly Color ENABLE_BG_COLOR = new Color(128f / 255f, 128f / 255f, 128f / 255f, 255f / 255f);
        public static readonly Color32 ENABLE_BG_COLOR_01 = new Color32(128, 128, 128, 255);

        /// <summary> 深灰色 </summary>
        public static readonly Color DISABLE_BG_COLOR = new Color(51f / 255f, 51f / 255f, 51f / 255f, 255f / 255f);
        public static readonly Color32 DISABLE_BG_COLOR_01 = new Color32(51, 51, 51, 255);

        /// <summary> 高亮-紫色 </summary>
        public static readonly Color HIGHLIGHT_BG_COLOR = new Color(255f / 255f, 0f / 255f, 255f / 255f, 255f / 255f);
        public static readonly Color32 HIGHLIGHT_BG_COLOR_01 = new Color32(255, 0, 255, 255);

        public static readonly Color HIGHLIGHT_BG_COLOR_MONTH_YEAR = ParseColor("#cccccc"); 


        protected virtual Color GetMonthItemBGColor(bool isHighLight) => isHighLight ? HIGHLIGHT_BG_COLOR : ENABLE_BG_COLOR;
        protected virtual Color GetYearItemBGColor(bool isHighLight) => isHighLight ? HIGHLIGHT_BG_COLOR : ENABLE_BG_COLOR;
        protected virtual Color GetDayItemBGHighLightColor() => HIGHLIGHT_BG_COLOR;
        protected virtual Color GetDayItemBGColor(bool isEnableClick) =>  isEnableClick? ENABLE_BG_COLOR : DISABLE_BG_COLOR;

        protected virtual Color GetDayItemTitleColor(bool isEnableClick) => isEnableClick ? Color.white : Color.gray;
        protected string GetMonthString(string month)
        {
            switch (month)
            {
                case "1":
                    return "Jan.";
                case "2":
                    return "Feb.";
                case "3":
                    return "Mar.";
                case "4":
                    return "Apr.";
                case "5":
                    return "May.";
                case "6":
                    return "Jun.";
                case "7":
                    return "Jul.";
                case "8":
                    return "Aug.";
                case "9":
                    return "Sept.";
                case "10":
                    return "Oct.";
                case "11":
                    return "Nov.";
                case "12":
                    return "Dec.";
                default:
                    return $"{month}";
            }

        }


        protected string getWeekName(string weekName)
        {
            switch (weekName)
            {
                case "0":
                    return "Sun";
                case "1":
                    return "Mon";
                case "2":
                    return "Tue";
                case "3":
                    return "Wed";
                case "4":
                    return "Thu";
                case "5":
                    return "Fri";
                case "6":
                    return "Sat";
                default:
                    return $"{weekName}";
            }
        }
    }


public class CalendarController
{
        //private CalendarUIBaseController uiCtrl;

        private IVCalendar calandarView;

        #region 最终确定的日期

        /// <summary> 确定的日期 </summary>
        DateTime ConfirmDT
        {
            get
            {
                return m_confirmDT;
            }
            set
            {
                m_confirmDT = value;
                OnChangeConfirmDateTime();
            }
        }
        private DateTime m_confirmDT = DateTime.Today;
        public int HourValue
        {
            get => _hourValue;
            set
            {
                _hourValue = value;
                OnChangeConfirmDateTime();
            }
        }
        private int _hourValue;

        public int MinuteValue
        {
            get => _minuteValue;
            set
            {
                _minuteValue = value;
                OnChangeConfirmDateTime();
            }
        }
        private int _minuteValue;

        #endregion



        /// <summary> 选择的日期 </summary>
        private DateTime m_selectDT = DateTime.Today;


        /// <summary> 日历显示内容控制器 </summary>
        private readonly CalendarRelatedDataParser m_calendarDataParser = new CalendarRelatedDataParser();

        /// <summary> 当前日历显示内容的类型 </summary>
        public E_CalendarPanelType CalendarType = E_CalendarPanelType.Day;


        public void ClearDelegate()
        {
            if (calandarView != null)
            {
                calandarView.onClickHourAdd -= OnClickHourAddBtn;
                calandarView.onClickHourReduce -= OnClickHourReduceBtn;
                calandarView.onClickMinuteAdd -= OnClickMinuteAddBtn;
                calandarView.onClickMinuteReduce -= OnClickMinuteReduceBtn;
                calandarView.onClickNext -= OnClickTitleNextButton;
                calandarView.onClickPrev -= OnClickTitlePrevButton;
                calandarView.onClickTitle -= OnClickTitle;
                calandarView.onClickDay -= OnCkickDay;
                calandarView.onClickMonth -= OnCkickMonth;
                calandarView.onClickYear -= OnCkickYear;
            }
        }

        //public void InitParam(CalendarUIBaseController ctrl)
        public void InitParam(IVCalendar ctrl)
        {
            ClearDelegate();

            calandarView = ctrl;
            calandarView.onClickHourAdd += OnClickHourAddBtn;
            calandarView.onClickHourReduce += OnClickHourReduceBtn;
            calandarView.onClickMinuteAdd += OnClickMinuteAddBtn;
            calandarView.onClickMinuteReduce += OnClickMinuteReduceBtn;
            calandarView.onClickNext += OnClickTitleNextButton;
            calandarView.onClickPrev += OnClickTitlePrevButton;
            calandarView.onClickTitle += OnClickTitle;
            calandarView.onClickDay += OnCkickDay;
            calandarView.onClickMonth += OnCkickMonth;
            calandarView.onClickYear += OnCkickYear;


            m_selectDT = DateTime.Today;
            ConfirmDT = m_selectDT;   


            var now = DateTime.Now;
            HourValue = now.Hour;
            MinuteValue = now.Minute;

            calandarView.SetHourTxt(GetNumberStr(now.Hour));
            calandarView.SetMinuteTxt(GetNumberStr(now.Minute));
        
            RefreshTitleTimeButtonContent();

            CalendarType = E_CalendarPanelType.Day;
            calandarView.CalendarPanelTypeChange(CalendarType);
            ParseRelatedCalendar();
        }


        /// <summary>
        /// 临近日历解析
        /// </summary>
        private void ParseRelatedCalendar()
        {
            if (CalendarType == E_CalendarPanelType.Day) RefreshUIDays(m_calendarDataParser.Days(m_selectDT), m_selectDT, ConfirmDT);
            else if (CalendarType == E_CalendarPanelType.Month) RefreshUIMonths(m_calendarDataParser.Months(m_selectDT), m_selectDT);
            else RefreshUIYears(m_calendarDataParser.Years(m_selectDT), m_selectDT);
        }


        private void RefreshUIDays(List<DateTime> relatedDTs , DateTime selectDT, DateTime confitmDT) 
                => calandarView.RefreshUIDays(relatedDTs, selectDT, confitmDT);

        private void RefreshUIMonths(List<DateTime> relatedDTs, DateTime selectDT) => calandarView.RefreshUIMonths(relatedDTs , selectDT);

        private void RefreshUIYears(List<DateTime> relatedDTs, DateTime selectDT) => calandarView.RefreshUIYears(relatedDTs, selectDT);




        /// <summary> 设置抬头内容 </summary>
        private void RefreshTitleTimeButtonContent()
        {
            switch (CalendarType)
            {
                case E_CalendarPanelType.Day:
                    {
                        calandarView.SetTitleTxt(m_selectDT.ToString("yyyy-MM-dd"));               
                    }
                    break;
                case E_CalendarPanelType.Month:
                    {
                        calandarView.SetTitleTxt(m_selectDT.Year + "-" + m_selectDT.Month);               
                    }
                    break;
                case E_CalendarPanelType.Year:
                    {
                        calandarView.SetTitleTxt(m_selectDT.Year.ToString());               
                    }
                    break;
            }
        }



        private void OnClickTitleNextButton()
        {
            //SetDayColor();
            if (CalendarType == E_CalendarPanelType.Day)
                m_selectDT = m_selectDT.AddMonths(1);
            else if (CalendarType == E_CalendarPanelType.Month)
                m_selectDT = m_selectDT.AddYears(1);
            else
                m_selectDT = m_selectDT.AddYears(12);
            ParseRelatedCalendar();
            RefreshTitleTimeButtonContent();
        }
        private void OnClickTitlePrevButton()
        {
            //SetDayColor();
            if (CalendarType == E_CalendarPanelType.Day)
                m_selectDT = m_selectDT.AddMonths(-1);
            else if (CalendarType == E_CalendarPanelType.Month)
                m_selectDT = m_selectDT.AddYears(-1);
            else
                m_selectDT = m_selectDT.AddYears(-12);
            ParseRelatedCalendar();
            RefreshTitleTimeButtonContent();
        }


        private void SetHourTxt(int value)
        {
            HourValue += value;
            if (HourValue < 0)
            {
                HourValue = 23;
            }
            if (HourValue > 23)
            {
                HourValue = 0;
            }
            calandarView.SetHourTxt(GetNumberStr(HourValue));
        }

        private void SetMinuteTxt(int value)
        {
            MinuteValue += value;
            if (MinuteValue < 0)
            {
                MinuteValue = 59;
            }
            if (MinuteValue > 59)
            {
                MinuteValue = 0;
            }
            calandarView.SetMinuteTxt(GetNumberStr(MinuteValue));
        }

        private void OnClickHourAddBtn() =>SetHourTxt(1);
        
        private void OnClickMinuteAddBtn() => SetMinuteTxt(1);
        
        private void OnClickHourReduceBtn() => SetHourTxt(-1);
        
        private void OnClickMinuteReduceBtn() =>SetMinuteTxt(-1);


        private void OnClickTitle()
        {
            switch (CalendarType)
            {
                case E_CalendarPanelType.Day:
                    CalendarType = E_CalendarPanelType.Month;
                    break;
                case E_CalendarPanelType.Month:
                    CalendarType = E_CalendarPanelType.Year;
                    break;
                case E_CalendarPanelType.Year:
                    CalendarType = E_CalendarPanelType.Day;
                    break;
            }
            calandarView.CalendarPanelTypeChange(CalendarType);
            ParseRelatedCalendar();
            RefreshTitleTimeButtonContent();
        }

        void OnCkickDay(DateTime data)
        {
            m_selectDT = data;
            ConfirmDT = m_selectDT;
            RefreshUIDays(m_calendarDataParser.Days(m_selectDT), m_selectDT, ConfirmDT);
            RefreshTitleTimeButtonContent();
        }
        void OnCkickMonth(DateTime data)
        {
            m_selectDT = data;
            RefreshTitleTimeButtonContent();
        }
        void OnCkickYear(DateTime data)
        {
            m_selectDT = data;
            RefreshTitleTimeButtonContent();
        }



        private void OnChangeConfirmDateTime()
        {
            calandarView.SetSelectDateTxt(GetLastSelectDate());
        }



        private string GetNumberStr(int numb) => numb < 10 ? "0" + numb : numb.ToString();

        public string GetLastSelectDate()
        {
            string confirmDay = m_confirmDT.ToString("yyyy-MM-dd");

            return string.Format("{0} {1}:{2}:00", confirmDay, GetNumberStr(_hourValue), GetNumberStr(_minuteValue));
        }


        public long GetLastSelectTimestamp()
        {
            string strDate = GetLastSelectDate(); // "2022-03-15 10:30:00";
            DateTimeOffset date = DateTimeOffset.Parse(strDate);
            long timeStamp = date.ToUnixTimeMilliseconds(); // date.ToUnixTimeSeconds();
            return timeStamp;
        }
    }



    public enum E_CalendarPanelType
    {
        Day,
        Month,
        Year
    }
    
  

    /// <summary>
    /// 相关日历解析器
    /// </summary>
    public class CalendarRelatedDataParser
    {
        /// <summary>
        /// 获取临近某日，的日号集合
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public List<DateTime> Days(DateTime month)
        {
            List<DateTime> days = new List<DateTime>();
            DateTime firstDay = new DateTime(month.Year, month.Month, 1);
            DayOfWeek week = firstDay.DayOfWeek;
            int lastMonthDays = (int)week;
            if (lastMonthDays.Equals(0))
                lastMonthDays = 7;
            for (int i = lastMonthDays; i > 0; i--)
                days.Add(firstDay.AddDays(-i));
            for (int i = 0; i < 42 - lastMonthDays; i++)
                days.Add(firstDay.AddDays(i));
            return days;
        }

        /// <summary>
        /// 获取临近某月，的月号集合
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public List<DateTime> Months(DateTime year)
        {
            List<DateTime> months = new List<DateTime>();
            DateTime firstMonth = new DateTime(year.Year, 1, 1);
            months.Add(firstMonth);
            for (int i = 1; i < 12; i++)
                months.Add(firstMonth.AddMonths(i));
            return months;
        }

        /// <summary>
        /// 获取临近某年，的年号集合
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public List<DateTime> Years(DateTime year)
        {
            List<DateTime> years = new List<DateTime>();
            for (int i = 5; i > 0; i--)
                years.Add(year.AddYears(-i));
            for (int i = 0; i < 7; i++)
                years.Add(year.AddYears(i));
            return years;
        }
    }





}
