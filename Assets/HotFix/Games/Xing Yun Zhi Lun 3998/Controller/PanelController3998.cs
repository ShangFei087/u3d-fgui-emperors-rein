using FairyGUI;
using GameMaker;
using slotEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XingYunZhiLun_3998;

public class PanelController3998 : SlotMaker.PanelBaseController
{
    new SpinButtonController spinBtnCtrl = new SpinButtonController();


    //public override void Init(EventData res = null)
    //{
    //    GComponent _goAnchorPanel = null;
    //    if (res != null)
    //        _goAnchorPanel = res.value as GComponent;
    //    else if (MainModel.Instance.contentMD != null)
    //        _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

    //    if (_goAnchorPanel == null)
    //    {

    //        return;
    //    }


    //    if (gOwnerPanel != _goAnchorPanel && _goAnchorPanel != null)
    //    {
    //        if (UIPackage.GetByName("XingYunZhiLun_3998") == null)
    //        {
    //            ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs", (ab) =>
    //            {
    //                UIPackage.AddPackage(ab);
    //                GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
    //                anchorPanel.url = "ui://XingYunZhiLun_3998/Panel";
    //                gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
    //                InitParam();
    //            });

    //        }
    //        else
    //        {
    //            GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
    //            anchorPanel.url = "ui://XingYunZhiLun_3998/Panel";

    //            gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;

    //            InitParam();
    //        }
    //    }

    //}


    protected override void InitParam()
    {
        base.InitParam();
        
        btnSound.GetController("button").selectedIndex = 1;
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
}