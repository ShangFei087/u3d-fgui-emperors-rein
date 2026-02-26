using CY.GameFramework;
using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleSlot01
{
    public class PopupConsoleSetParameter001 : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleSetParameter001";
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

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose, btnConfirm, btnInput1;

        GRichTextField rtxtTitle, rtxtParam1, rtxtTip1;


        CompInputController compInputCtrl1 = new CompInputController();
        CompKeyboardController compKBCtrl = new CompKeyboardController();


        Func<string, string> checkParam1Func;


        public override void InitParam()
        {

            btnClose = this.contentPane.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                //DebugUtils.Log("i am here 123");
                CloseSelf(null);
            });


            rtxtTitle = this.contentPane.GetChild("title").asRichTextField;
            rtxtParam1 = this.contentPane.GetChild("param1").asRichTextField;
            rtxtTip1 = this.contentPane.GetChild("tip1").asRichTextField;


            btnInput1 = this.contentPane.GetChild("input1").asButton;
            btnInput1.onClick.Clear();


            btnConfirm = this.contentPane.GetChild("btnConfirm").asButton;
            btnConfirm.onClick.Clear();
            btnConfirm.onClick.Add(OnClickButtonConfirm);


            GComponent kb = this.contentPane.GetChild("keyboard").asCom;
            GComponent kbabcd = kb.GetChild("kbabcd").asCom;
            GComponent kbABC = kb.GetChild("kbABC").asCom;
            GComponent kb123 = kb.GetChild("kb123").asCom;
            GComponent kbOperator = kb.GetChild("kbOperator").asCom;
            compKBCtrl.Init(kb, kb123, kbabcd, kbABC, kbOperator);

            compInputCtrl1.Init(btnInput1, btnInput1.GetChild("title").asRichTextField, null, null);
            compInputCtrl1.GetFocus();

            ClearParam();

            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                if (argDic.ContainsKey("title"))
                {
                    rtxtTitle.text = (string)argDic["title"];
                }
                if (argDic.ContainsKey("paramName1"))
                {
                    rtxtParam1.text = (string)argDic["paramName1"];
                }

                if (argDic.ContainsKey("checkParam1Func"))
                {
                    checkParam1Func = (Func<string, string>)argDic["checkParam1Func"];
                }

            }

        }

        public override void OnClose(EventData data = null)
        {
            ClearParam();

            base.OnClose(data);
        }


        void ClearParam()
        {
            compInputCtrl1.value = "";
            rtxtTip1.text = "";

            checkParam1Func = (res) => null;
        }
        void OnClickButtonConfirm()
        {
            rtxtTip1.text = "";

            string msg1 = checkParam1Func(compInputCtrl1.value);
            if (!string.IsNullOrEmpty(msg1))
            {
                TipPopupHandler.Instance.OpenPopup(msg1);
                rtxtTip1.text = msg1;
                return;
            }

            PageManager.Instance.ClosePage(this, new EventData<string>("Result", compInputCtrl1.value));

        }
    }

}
