using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotBonusGamePanelController : IContorller
{
    GComponent Spin,Panel;
    GTextField win, Er;
    public void Dispose()
    {
        
    }

    public void Init(GComponent panel1)
    {
        Panel = panel1;
        Spin = Panel.GetChild("ButtonSpin").asCom;
        win = Panel.GetChild("win").asCom.GetChild("winSound").asTextField;
        Er = Panel.GetChild("ERCredits").asTextField;
        Er.text = SBoxModel.Instance.myCredit.ToString();
        Dispose();
        InitParam();
    }

    public void Init()
    {
        //throw new System.NotImplementedException();
    }

    public void InitParam(params object[] parameters)
    {

        Spin.onClick.Clear();
        Spin.onClick.Add(() =>
        {
            OnClickSpinButton();
        });
        Spin.GetController("c1").selectedPage = "start";

    }

    public void OnClickSpinButton()
    {
        if (Spin.GetController("c1").selectedPage == "stop")
        {
            DebugUtils.LogError(1111);
            return;
        }

        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,
           new EventData(PanelEvent.BonusGame1SpinButtonClick));
        Spin.GetController("c1").selectedPage = "stop";
    }
}
