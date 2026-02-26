using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

public class TabLogRecordController : MonoBehaviour
{
    void Init() { }

    GComponent goOwnerTab;

    LogRecordDataController ctrl = new LogRecordDataController();



    List<GComponent> goItems = new List<GComponent>();
    GRichTextField rtxtDetail;

    LogPageInfo pageInfo;

    GComboBox gcbDropdownDates;


    public void InitParam(GComponent go, string tabName)
    {
        goOwnerTab = go;

        for (int i=1; i<=11; i++)
        {
            GComponent gItem = goOwnerTab.GetChild($"item{i}").asCom;
            goItems.Add(gItem) ;

            gItem.visible = false;
            //gItem.GetChild("value1").asRichTextField.text = "";
            //gItem.GetChild("value2").asRichTextField.text = "";
        }

        rtxtDetail = goOwnerTab.GetChild("detail").asCom.GetChild("title").asRichTextField;
        rtxtDetail.text = "";

        gcbDropdownDates = goOwnerTab.GetChild("date").asCom.GetChild("value").asComboBox;
        gcbDropdownDates.onChanged.Clear();
        gcbDropdownDates.onChanged.Add(OnComboChanged);

        ctrl.InitParam(tabName, 11 , onDatesChange, onPageChagne);
    }
    //void OnComboChanged(EventContext context)
    void OnComboChanged(EventContext context)
    {
        GComboBox sender = context.sender as GComboBox;
        DebugUtils.Log($"选择了：（索引：{gcbDropdownDates.selectedIndex}） -{sender.value}-  {sender.selectedIndex} ");
        OnDatasChanged(sender.selectedIndex);
    }

    void OnDatasChanged(int selectedIndex)
    {
        //DebugUtils.LogError("++++  999 ");
        ctrl.ChangeDate(selectedIndex);
    }

    void onDatesChange(List<string> dates) {

        gcbDropdownDates.items = dates.ToArray();
        gcbDropdownDates.values = dates.ToArray();
        if (dates.Count>0)
        {
            //DebugUtils.LogError("-+-+-+_+");
            gcbDropdownDates.selectedIndex = 0;
            OnDatasChanged(gcbDropdownDates.selectedIndex);
        }
    }
    void onPageChagne(LogPageInfo pageInfo)
    {
        this.pageInfo = pageInfo;
        rtxtDetail.text = "";
        foreach (GComponent item in goItems)
        {
            item.visible = false;
            item.onClick.Clear();
        }

        List<LogRecord> logRecords = pageInfo.logRecords;
        for (int i = 0; i < logRecords.Count; i++)
        {
            goItems[i].visible = true;
            goItems[i].GetChild("value1").asRichTextField.text = logRecords[i].timeStr;
            goItems[i].GetChild("value2").asRichTextField.text = logRecords[i].content;

            string detail = logRecords[i].detail;
            goItems[i].onClick.Add(() =>
            {
                rtxtDetail.text = detail;
            });
        }
    }
}
