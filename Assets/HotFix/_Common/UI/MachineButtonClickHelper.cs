using GameUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

// Dictionary<MachineButtonKey, UnityEvent<MachineButtonInfo>> 
public class MachineButtonClickHelper
{
    public Dictionary<MachineButtonKey, Action<MachineButtonInfo>> longClickHandler =
        new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>();
    public Dictionary<MachineButtonKey, Action<MachineButtonInfo>> shortClickHandler =
        new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>();
    public Dictionary<MachineButtonKey, Action<MachineButtonInfo>> downClickHandler =
        new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>();  
    public Dictionary<MachineButtonKey, Action<MachineButtonInfo>> upClickHandler =
        new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>();


    private Dictionary<string, DelayTimer> longClickInfo = new Dictionary<string, DelayTimer>();
    /*
    public void OnEventMachineButtonInfo(MachineButtonInfo info)
    {
        if (info != null)
        {
            string keyName = Enum.GetName(typeof(MachineButtonKey), info.btnKey);

            if (!info.isUp)  // 按钮按下
            {
                if (downClickHandler.ContainsKey(info.btnKey))
                    downClickHandler[info.btnKey].Invoke(info);

                if (longClickHandler.ContainsKey(info.btnKey) || shortClickHandler.ContainsKey(info.btnKey)){
                    if (longClickInfo.ContainsKey(keyName))
                    {
                        longClickInfo[keyName].Cancel();
                        longClickInfo.Remove(keyName);
                    }
                    longClickInfo.Add(keyName,Timer.DelayAction(0.8f, () => DoCheckLongClick(info)));
                }

            }
            else  // 按钮抬起
            {
 
                if (upClickHandler.ContainsKey(info.btnKey))
                    upClickHandler[info.btnKey].Invoke(info);


                if (longClickInfo.ContainsKey(keyName))
                {
                    longClickInfo[keyName].Cancel();
                    longClickInfo.Remove(keyName);

                    if (shortClickHandler.ContainsKey(info.btnKey))
                        shortClickHandler[info.btnKey].Invoke(info);
                }
  
            }
        }
    }*/


    List<string> longClickMark = new List<string>();
    public void OnEventMachineButtonInfo(MachineButtonInfo info)
    {
        if (info != null)
        {
            string keyName = Enum.GetName(typeof(MachineButtonKey), info.btnKey);

            if (!info.isUp)  // 按钮按下
            {
                if (downClickHandler.ContainsKey(info.btnKey))
                    downClickHandler[info.btnKey].Invoke(info);

                if (longClickHandler.ContainsKey(info.btnKey) || shortClickHandler.ContainsKey(info.btnKey))
                {
                    if (longClickInfo.ContainsKey(keyName))
                    {
                        longClickInfo[keyName].Cancel();
                        longClickInfo.Remove(keyName);
                    }

                    longClickMark.Remove(keyName);

                    longClickInfo.Add(keyName,
                        Timer.DelayAction(0.8f, () =>{ longClickMark.Add(keyName); })
                        );
                }

            }
            else  // 按钮抬起
            {

                if (upClickHandler.ContainsKey(info.btnKey))
                    upClickHandler[info.btnKey].Invoke(info);

                if (longClickInfo.ContainsKey(keyName))
                {
                    longClickInfo[keyName].Cancel();
                    longClickInfo.Remove(keyName);
                }

                if (longClickMark.Contains(keyName))
                {
                    longClickMark.Remove(keyName);

                    if (longClickHandler.ContainsKey(info.btnKey))
                        longClickHandler[info.btnKey].Invoke(info);
                }
                else
                {
                    if (shortClickHandler.ContainsKey(info.btnKey))
                        shortClickHandler[info.btnKey].Invoke(info);
                }


            }
        }
    }

    /*
    void DoCheckLongClick(MachineButtonInfo info)
    {
        string keyName = Enum.GetName(typeof(MachineButtonKey), info.btnKey);
        longClickInfo.Remove(keyName);
        longClickHandler[info.btnKey].Invoke(info);
    }
    */
}
