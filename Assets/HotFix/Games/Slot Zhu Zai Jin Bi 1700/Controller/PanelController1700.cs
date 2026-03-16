using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController1700 : SlotMaker.PanelBaseController
{
    public override void Init(EventData res = null)
    {
        GComponent _goAnchorPanel = null;
        if (res != null)
            _goAnchorPanel = res.value as GComponent;
        else if (MainModel.Instance.contentMD != null)
            _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

        if (_goAnchorPanel == null)
        {

            return;
        }


        if (gOwnerPanel != _goAnchorPanel && _goAnchorPanel != null)
        {
            if (UIPackage.GetByName("Panel01") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Panel01/FGUIs", (ab) =>
                {
                    UIPackage.AddPackage(ab);
                    GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    anchorPanel.url = "ui://Panel01/Panel";
                    gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                    InitParam();
                });

            }
            else
            {
                GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                anchorPanel.url = "ui://Panel01/Panel";

                gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;

                InitParam();
            }
        }

    }


    protected override void InitParam()
    {
        print(MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component.name);
        gOwnerPanel = MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component;
        //gSetPanel = gOwnerPanel.GetChild("setBtns").asCom;
        gOwnerPanel.GetChild("ERCredits").asTextField.text = SBoxModel.Instance.myCredit.ToString();
        gOwnerPanel.GetChild("win").asTextField.text = 0.ToString();
        btnBetUp = gOwnerPanel.GetChild("ButtonBetUp").asButton;
        btnBetUp.onClick.Clear();
        btnBetUp.onClick.Add(OnClickButtonBetUp);
        btnBetDown = gOwnerPanel.GetChild("ButtonBetDown").asButton;
        btnBetDown.touchable = false;
        btnBetDown.GetChild("n1").visible = true;
        btnBetDown.onClick.Clear();
        btnBetDown.onClick.Add(OnClickButtonBetDown);
        bet = gOwnerPanel.GetChild("BetTable").asTextField;
        bet.text = SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex].ToString();
        spinBtnCtrl.InitParam(gOwnerPanel.GetChild("ButtonSpin").asCom, "Spin", OnClickSpinButton);
        btnHelp = gOwnerPanel.GetChild("ButtonHelp").asCom;
        //gIntroducePanel = gOwnerPanel.GetChild("payTable").asCom;
        //Set.onClick.Clear();
        //Set.onClick.Add(() =>
        //{
        //    isSet = !isSet;
        //    if (isSet)
        //    {
        //        gSetPanel.visible = true;
        //        Set.GetController("button").selectedPage = "Back";
        //        gOwnerPanel.GetChild("mash").asGraph.visible = true;
        //        spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "hui";
        //        spinBtnCtrl.goOwnerSpin.touchable = false;
        //    }
        //    else
        //    {

        //        gSetPanel.visible = false;
        //        gIntroducePanel.visible = false;
        //        gOwnerPanel.GetChild("mash").asGraph.visible = false;
        //        Set.GetController("button").selectedPage = "Introduce";
        //        spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
        //        spinBtnCtrl.goOwnerSpin.touchable = true;
        //    }

        //});
        //btnIntroduce = gSetPanel.GetChild("btnIntroduce").asButton;
        //btnIntroduce.onClick.Clear();
        //btnIntroduce.onClick.Add(() =>
        //{
        //    if (Introduce == null)
        //    {

        //        Introduce = gIntroducePanel.GetChild("Page").asCom;
        //        Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
        //        btnIntroduceL = gIntroducePanel.GetChild("introduceBtn").asCom.GetChild("btnL").asButton;
        //        btnIntroduceR = gIntroducePanel.GetChild("introduceBtn").asCom.GetChild("btnR").asButton;
        //        btnIntroduceL.onClick.Clear();
        //        btnIntroduceL.onClick.Add(OnClickIntroduceL);
        //        btnIntroduceR.onClick.Clear();
        //        btnIntroduceR.onClick.Add(OnClickIntroduceR);

        //    }
        //    IntroduceInit();
        //    gIntroducePanel.visible = true;
        //    gSetPanel.visible = false;
        //});
        //Sound = gSetPanel.GetChild("btnSound").asCom;
        //Sound.onClick.Clear();
        //Sound.onClick.Add(() =>
        //{
        //    VolumeLevel += 1;
        //    if (VolumeLevel == 4)
        //    {
        //        VolumeLevel = 0;
        //    }
        //    switch (VolumeLevel)
        //    {

        //        case 0:
        //            //闊抽噺涓夌骇
        //            break;
        //        case 1:
        //            //闊抽噺浜岀骇
        //            break;
        //        case 2:
        //            //闊抽噺涓€绾ustomModel.Instance.VolumeLevel
        //            break;
        //        case 3:
        //            //鏃犻煶閲?
        //            break;
        //    }

        //    Sound.GetController("button").selectedIndex = VolumeLevel;

        //});

        base.InitParam();
    }

    public override void ChangButtonNo(bool can)
    {
        if (can)
        {
            //gOwnerPanel.GetChild("ButtonHelp").asButton.GetChild("n3").visible = true;
            //gOwnerPanel.GetChild("ButtonHelp").asButton.touchable = false;
            btnBetUp.GetChild("n1").visible = true;
            btnBetUp.touchable = false;
            btnBetDown.GetChild("n1").visible = true;
            btnBetDown.touchable = false;

            // ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
        }
        else
        {
            //gOwnerPanel.GetChild("ButtonHelp").asButton.GetChild("n3").visible = false;
            //gOwnerPanel.GetChild("ButtonHelp").asButton.touchable = true;
            btnBetUp.GetChild("n1").visible = false;
            btnBetUp.touchable = true;
            btnBetDown.GetChild("n1").visible = false;
            btnBetDown.touchable = true;

            if (MainModel.Instance.contentMD.betIndex == 0)
            {
                btnBetDown.GetChild("n1").visible = true;
                btnBetDown.touchable = false;
            }

            if (MainModel.Instance.contentMD.betIndex == 7)
            {
                btnBetUp.GetChild("n1").visible = true;
                btnBetUp.touchable = false;
            }
        }
    }
}
