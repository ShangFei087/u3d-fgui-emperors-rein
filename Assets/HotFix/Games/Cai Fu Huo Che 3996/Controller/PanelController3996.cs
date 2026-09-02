using FairyGUI;
using GameMaker;
using slotEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PanelController3996 : SlotMaker.PanelBaseController
{
    new SpinButtonController spinBtnCtrl = new SpinButtonController();
    protected override string PanelPackagePath => "Assets/GameRes/Panel/Panel3996/FGUIs";

    protected override string ShortSpinPrefabPath => "Assets/GameRes/Panel/Panel3996/Prefabs/Eff_ShortSpin.prefab";
    protected override string LongSpinPrefabPath => "Assets/GameRes/Panel/Panel3996/Prefabs/Eff_LongSpin.prefab";


    public override void Init(EventData res = null)
    {
        base.Init(res);
    }


    protected override void InitParam()
    {
        base.InitParam();
    }

    protected override void OnPropertyChangeBtnSpinState(EventData res = null)
    {
        base.OnPropertyChangeBtnSpinState(res);
        string changeSpinState = (string)res?.value;

        if (changeSpinState == null)
            changeSpinState = "Stop";

        if (gOwnerPanel == null) return;


        switch (changeSpinState)
        {
            case SpinButtonState.Stop:
                {
                    spinBtnCtrl.State = "Stop";
                    ChangButtonNo(false);
                }
                break;
            case SpinButtonState.Spin:
                {
                    spinBtnCtrl.State = "Spin";
                    if (gOwnerPanel != null)
                    {
                        //gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                        //gOwnerPanel.GetChild("win").asCom.visible = true;
                        gOwnerPanel.GetChild("win").asTextField.visible = true;
                    }
                    ChangButtonNo(true);
                }
                break;
            case SpinButtonState.Auto:
                {
                    spinBtnCtrl.State = "Auto";
                    if (gOwnerPanel != null)
                    {
                        //gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                        //gOwnerPanel.GetChild("win").asCom.visible = true;
                        gOwnerPanel.GetChild("win").asTextField.visible = true;
                    }
                    ChangButtonNo(true);
                }
                break;
            case SpinButtonState.Hui:
                {
                    spinBtnCtrl.State = "Hui";
                    if (gOwnerPanel != null)
                    {
                        //gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                        //gOwnerPanel.GetChild("win").asCom.visible = true;
                        gOwnerPanel.GetChild("win").asTextField.visible = true;
                    }
                    ChangButtonNo(true);
                }
                break;
        }
    }

    protected override void OnPropertyGameState(EventData res = null)
    {
        string gameState = (string)res?.value;

        if (gameState == GameState.Spin)
        {
            win.text = 0.ToString();
            ClearSingleLineText();
        }

        else if (gameState == GameState.FreeSpin)
        {
            ClearSingleLineText();
        }
    }
}