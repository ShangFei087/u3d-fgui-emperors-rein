using CY.GameFramework;
using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleSlot01
{
    public class PopupConsoleCoder : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleCoder";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();
        }


        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }


        GButton btnClose, btnInut;


        GTextField txtA, txtB, txtC, txtD, txtE, txtTime, txtInput;

        GRichTextField txtTitle;

        GComponent cmpInput, cmpKeyboard;

        CompInputController compInputCtrl = new CompInputController();
        CompKeyboardController compKBCtrl = new CompKeyboardController();



        string A, B, C, D, E, day, hour, minute;

        public override void InitParam()
        {
            btnClose = this.contentPane.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(new EventData("Exit"));
            });


            txtTitle = this.contentPane.GetChild("title").asRichTextField;
            txtA = this.contentPane.GetChild("A").asTextField;
            txtB = this.contentPane.GetChild("B").asTextField;
            txtC = this.contentPane.GetChild("C").asTextField;
            txtD = this.contentPane.GetChild("D").asTextField;
            txtE = this.contentPane.GetChild("E").asTextField;

            txtTime = this.contentPane.GetChild("time").asTextField;


            cmpKeyboard = this.contentPane.GetChild("keyboard").asCom;
            compKBCtrl.Init(cmpKeyboard);

            btnInut = this.contentPane.GetChild("input").asButton;
            txtInput = this.contentPane.GetChild("input").asCom.GetChild("title").asRichTextField;
            compInputCtrl.Init(btnInut, txtInput, OnClickButtonOk, null);
            compInputCtrl.GetFocus();


            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                A = (string)argDic["A"];
                B = (string)argDic["B"];
                C = (string)argDic["C"];
                D = (string)argDic["D"];
                E = (string)argDic["E"];
                day = (string)argDic["Day"];
                hour = (string)argDic["Hour"];
                minute = (string)argDic["Minute"];
            }

            txtTitle.text = I18nMgr.T("Activate Code");

            txtA.text = $"A: {A}";
            txtB.text = $"B: {B}";
            txtC.text = $"C: {C}";
            txtD.text = $"D: {D}";
            txtE.text = $"E: {E}";
            txtTime.text = string.Format(I18nMgr.T("remaining time: {0} days; {1} hours; {2} minute;"), day, hour, minute);
        }

        void OnClickButtonOk(string value)
        {
            CloseSelf(new EventData<string>("Result", value));
        }
    }
}