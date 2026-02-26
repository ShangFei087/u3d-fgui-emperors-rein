using FairyGUI;
using System.Collections.Generic;
using SpringGUI;
using System;


public class CalendarView : CalendarViewBase
{


    public void Init(){}

    GTextField txtSelectDate;

    GButton btnPrev, btnNext, btnDate,
        btnMinusHour, btnAddHour,
        btnMinusMinute, btnAddMinute;

    GObject goYearMonthBG;

    GTextField gtxtSun, gtxtMon, gtxtTue, gtxtWed, gtxtThu, gtxtFri, gtxtSat,
        gtxtHourValue, gtxtMinuteValue;

    GList glstDays, glstYearMonth;

    public override void InitParam(GComponent uiOwner)
    {

        txtSelectDate = uiOwner.GetChild("selectDate").asRichTextField;

        glstDays = uiOwner.GetChild("days").asList;
        glstYearMonth = uiOwner.GetChild("setYearMonth").asList;
        goYearMonthBG = uiOwner.GetChild("setYearMonthBG");

        btnDate = uiOwner.GetChild("btnDate").asButton;

        gtxtHourValue = uiOwner.GetChild("hourValue").asTextField;
        gtxtMinuteValue = uiOwner.GetChild("minuteValue").asTextField;


        btnPrev = uiOwner.GetChild("buttonPrev").asButton;
        btnPrev.onClick.Clear();
        btnPrev.onClick.Add(() =>
        {
            //onClickPrev?.Invoke();
            TriggerOnClickPrev();
        });

        btnNext = uiOwner.GetChild("buttonNext").asButton;
        btnNext.onClick.Clear();
        btnNext.onClick.Add(() =>
        {
            //onClickNext?.Invoke();
            TriggerOnClickNext();
        });

        btnDate = uiOwner.GetChild("btnDate").asButton;
        btnDate.onClick.Clear();
        btnDate.onClick.Add(() =>
        {
            //onClickTitle?.Invoke();
            TriggerOnClickTitle();
        });

        btnMinusHour = uiOwner.GetChild("buttonMinusHour").asButton;
        btnMinusHour.onClick.Clear();
        btnMinusHour.onClick.Add(() =>
        {
            //onClickHourReduce?.Invoke();
            TriggerOnClickHourReduce();
        });

        btnAddHour = uiOwner.GetChild("buttonAddHour").asButton;
        btnAddHour.onClick.Clear();
        btnAddHour.onClick.Add(() =>
        {
            //onClickHourAdd?.Invoke();
            TriggerOnClickHourAdd();
        });

        btnMinusMinute = uiOwner.GetChild("buttonMinusMinute").asButton;
        btnMinusMinute.onClick.Clear();
        btnMinusMinute.onClick.Add(() =>
        {
            //onClickMinuteReduce?.Invoke();
            TriggerOnClickMinuteReduce();
        });

        btnAddMinute = uiOwner.GetChild("buttonAddMinute").asButton;
        btnAddMinute.onClick.Clear();
        btnAddMinute.onClick.Add(() =>
        {
            //onClickMinuteAdd?.Invoke();
            TriggerOnClickMinuteAdd();
        });


    }


    public override void SetHourTxt(string str)
    {
        gtxtHourValue.text = str;
    }
    public override void SetMinuteTxt(string str) 
    {
        gtxtMinuteValue.text = str;
    }

    public override void SetSelectDateTxt(string lastSelectDate ) {

        string res = I18nMgr.T("Selected Date & Time: {0}");
        res = string.Format(res, lastSelectDate);
        txtSelectDate.text = res;
    }


    public override void SetTitleTxt(string str)
    {
        btnDate.title = str;
    }






    public override void CalendarPanelTypeChange(E_CalendarPanelType panelType)
    {
        if (panelType == E_CalendarPanelType.Day)
        {
            //glstDays.visible = true;
            glstYearMonth.visible = false;
            goYearMonthBG.visible = false;
        }
        else
        {
            glstYearMonth.visible = true;
            goYearMonthBG.visible = true;
        }

    }

    public override void RefreshUIDays(List<DateTime> relatedDTs , DateTime selectDT, DateTime confirmDT)
    {
        for (int i = 0; i < glstDays.numChildren; i++)
        {
            bool isEnableClick = relatedDTs[i].Month == selectDT.Month 
                && relatedDTs[i].Year == selectDT.Year ;
            GButton btn = glstDays.GetChildAt(i).asButton;
            btn.title = relatedDTs[i].Day.ToString();
            btn.GetChild("title").asTextField.color = GetDayItemTitleColor(isEnableClick); 


            if( isEnableClick
                && relatedDTs[i].Year == confirmDT.Year
                && relatedDTs[i].Month == confirmDT.Month
                && relatedDTs[i].Day == confirmDT.Day )
                btn.GetChild("bg").asImage.color = GetDayItemBGHighLightColor();
            else
                btn.GetChild("bg").asImage.color = GetDayItemBGColor(isEnableClick);


            btn.touchable = isEnableClick;

            DateTime date = relatedDTs[i];
            btn.onClick.Clear();
            btn.onClick.Add(() =>
            {
                //onClickDay?.Invoke(date);
                TriggerOnClickDay(date);
            });
        }
    }
    public override void RefreshUIMonths(List<DateTime> relatedDTs, DateTime selectDT)
    {
        for (int i = 0; i < glstYearMonth.numChildren; i++)
        {
            GButton btn = glstYearMonth.GetChildAt(i).asButton;
            btn.title = GetMonthString(relatedDTs[i].Month.ToString());


            bool isHighLight = relatedDTs[i].Year == selectDT.Year
                && relatedDTs[i].Month == selectDT.Month;
            btn.GetChild("bg").asImage.color = GetMonthItemBGColor(isHighLight);

            DateTime date = relatedDTs[i];
            btn.onClick.Clear();
            btn.onClick.Add(() =>
            {

                for(int i=0;i< glstYearMonth.numChildren; i++)
                {
                    glstYearMonth.GetChildAt(i).asCom.GetChild("bg").asImage.color = GetMonthItemBGColor(false);
                }
                btn.GetChild("bg").asImage.color = GetMonthItemBGColor(true);

                //onClickMonth(date);
                TriggerOnClickMonth(date);
            });

        }
    }
    public override void RefreshUIYears(List<DateTime> relatedDTs, DateTime selectDT)
    {
        for (int i = 0; i < glstYearMonth.numChildren; i++)
        {
            GButton btn = glstYearMonth.GetChildAt(i).asButton;
            btn.title = relatedDTs[i].Year.ToString();


            bool isHighLight = relatedDTs[i].Year == selectDT.Year;
            btn.GetChild("bg").asImage.color = GetYearItemBGColor(isHighLight);

            DateTime date = relatedDTs[i];
            btn.onClick.Clear();
            btn.onClick.Add(() =>
            {
                for (int i = 0; i < glstYearMonth.numChildren; i++)
                {
                    glstYearMonth.GetChildAt(i).asCom.GetChild("bg").asImage.color = GetYearItemBGColor(false);
                }
                btn.GetChild("bg").asImage.color = GetYearItemBGColor(true);

                //onClickYear(date);
                TriggerOnClickYear(date);
            });
        }
    }



}
