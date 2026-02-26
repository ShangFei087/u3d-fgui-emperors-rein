using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TabSettings03 : ConsoleMenuBase
{

    public override void Init()
    {

    }
    public override void InitParam(GComponent comp, Action onClickPrev, Action onClickNext, Action onClickExitCallback)
    {
        base.InitParam(comp, onClickPrev, onClickNext, onClickExitCallback);

        ResetItem(false);
        AddClickEvent();
    }


    public override void OnClickConfirm()
    {
        if (menuMap.ContainsKey(curIndexMenuItem))
        {

            switch (menuMap[curIndexMenuItem])
            {
                case "prev":
                    {
                        onClickPrev?.Invoke();
                    }
                    return;
                case "next":
                    {
                        onClickNext?.Invoke();
                    }
                    return;
                case "exit":
                    {
                        onClickExitCallback?.Invoke();
                    }
                    return;
            }
        }
    }

}
