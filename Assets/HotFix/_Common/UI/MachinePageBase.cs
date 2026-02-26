using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMaker;
using System;

public class MachinePageBase : PageBase
{
    protected MachineCustomButton machineCustomButton;
    
    protected MachineButtonClickHelper machineBtnClickHelper = new MachineButtonClickHelper()
    {
        downClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
        {
            [MachineButtonKey.BtnSpin] = (info) =>
            {
                //Debug.LogError("没有实现 BtnSpin Down 的业务逻辑");
            }
        },
        upClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
        {
            [MachineButtonKey.BtnSpin] = (info) =>
            {
                //Debug.LogError("没有实现 BtnSpin Up 的业务逻辑");
            }
        }
    };

    
    protected override void OnInit()
    {
        base.OnInit();
        //Debug.LogError($"111 = {this.GetType().Name}");
        machineCustomButton = new MachineCustomButton()
        {
            btnShowLst = new List<MachineButtonKey>(),
            btnType = MachineButtonType.Regular,
            mark = $"{this.GetType().Name}#{Time.unscaledTime}",
        };

    }

    public override void OnOpen(PageName name, EventData data)
    {
        base.OnOpen(name, data);
        
        EventCenter.Instance.AddEventListener<EventData>(MachineDeviceController.MACHINE_BUTTON_EVENT, OnEventMachineButton);
    }
    
    public override void OnClose(EventData data = null)
    {
        EventCenter.Instance.RemoveEventListener<EventData>(MachineDeviceController.MACHINE_BUTTON_EVENT, OnEventMachineButton);
        base.OnClose(data);
    }
    
    public override void OnTop()
    {
        base.OnTop();
        EventCenter.Instance.EventTrigger<EventData>(MachineCustomButton.MACHINE_CUSTOM_BUTTON_FOCUS_EVENT,
            new EventData<MachineCustomButton>(machineCustomButton.mark, machineCustomButton));
        //Debug.LogError($"mark = {machineCustomButton.mark}");
    }
    protected virtual void OnEventMachineButton(EventData evt)
    {
        if (machineCustomButton == null) 
            return;
        
        if (evt.name != machineCustomButton.mark)
            return;
        MachineButtonInfo info = (MachineButtonInfo)evt.value;
        machineBtnClickHelper?.OnEventMachineButtonInfo(info);
    }
    
}
