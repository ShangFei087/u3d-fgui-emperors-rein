using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabSettings02: ConsoleMenuBase
{

    public override void  Init()
    {

    }
    public override void InitParam(GComponent comp, Action onClickPrev, Action onClickNext,  Action onClickExitCallback)
    {
        base.InitParam(comp, onClickPrev, onClickNext, onClickExitCallback);


        //DebugUtils.Log($"@@@@@@@@@@@@    {DeviceUtils.GetCoinOutScaleStr()}  {SBoxModel.Instance.CoinInScale}   {SBoxModel.Instance.ScoreUpDownScale}  ");
        goOwnerMenu.GetChild("coinOutScale").asCom.GetChild("value").asRichTextField.text = DeviceUtils.GetCoinOutScaleStr();
        goOwnerMenu.GetChild("coinInScale").asCom.GetChild("value").asRichTextField.text = $"1:{SBoxModel.Instance.CoinInScale}";
        goOwnerMenu.GetChild("scoreScale").asCom.GetChild("value").asRichTextField.text = $"1:{SBoxModel.Instance.ScoreUpDownScale}";
        goOwnerMenu.GetChild("scoreScale").asCom.GetChild("value").asRichTextField.text = $"1:{SBoxModel.Instance.ScoreUpDownScale}";
        goOwnerMenu.GetChild("ballRewardScale").asCom.GetChild("value").asRichTextField.text = $"1:{SBoxModel.Instance.BallRewardScale}"; 
        goOwnerMenu.GetChild("maxCoinInOutRecord").asCom.GetChild("value").asRichTextField.text = $"{SBoxModel.Instance.coinInOutRecordMax}";
        goOwnerMenu.GetChild("maxGameRecord").asCom.GetChild("value").asRichTextField.text = $"{SBoxModel.Instance.gameRecordMax}";
        goOwnerMenu.GetChild("maxEventRecord").asCom.GetChild("value").asRichTextField.text = $"{SBoxModel.Instance.eventRecordMax}";
        goOwnerMenu.GetChild("maxErrorRecord").asCom.GetChild("value").asRichTextField.text = $"{SBoxModel.Instance.errorRecordMax}";
        goOwnerMenu.GetChild("maxBusinessDayRecord").asCom.GetChild("value").asRichTextField.text = $"{SBoxModel.Instance.businiessDayRecordMax}";

        if (!PlayerPrefsUtils.isUseAllConsolePage)
        {
            SetItemTouchable("coinOutScale", false);
            SetItemTouchable("coinInScale", false);
            SetItemTouchable("scoreScale", false);
            SetItemTouchable("ballRewardScale", false);
        }

        SetItemTouchable("maxCoinInOutRecord", false);
        SetItemTouchable("maxGameRecord", false);
        SetItemTouchable("maxEventRecord", false);
        SetItemTouchable("maxErrorRecord", false);
        SetItemTouchable("maxBusinessDayRecord", false);

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
