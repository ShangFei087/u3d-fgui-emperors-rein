using CY.GameFramework;
using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleSlot01
{
    public class PopupConsoleKeyboard002 : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleKeyboard002";
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

        GButton btnInput;

        GTextField txtTitle, txtInput;

        GComponent cmpKeyboardRoot;

        CompInputController compInputCtr = new CompInputController();
        CompKeyboardController compKBCtrl = new CompKeyboardController();


        /// <summary> 抬头 </summary>
        string title = "";
        /// <summary> 是否明文 </summary>
        bool isPlaintext = false;
        string inputText = "";
        public override void InitParam()
        {

            cmpKeyboardRoot = this.contentPane.GetChild("keyboard").asCom;

            txtTitle = this.contentPane.GetChild("title").asTextField;
            txtInput = this.contentPane.GetChild("input").asCom.GetChild("title").asTextField;
            btnInput = this.contentPane.GetChild("input").asButton;

            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                title = (string)argDic["title"];

                if (argDic.ContainsKey("isPlaintext"))
                {
                    isPlaintext = (bool)argDic["isPlaintext"];
                }
                if (argDic.ContainsKey("content"))
                {
                    inputText = (string)argDic["content"];
                }
            }


            txtTitle.text = title;

            compKBCtrl.Init(cmpKeyboardRoot);
            compInputCtr.Init(btnInput, txtInput, OnClickButtonOk, OnClickButtonExit, inputText, isPlaintext);
            compInputCtr.GetFocus();
        }

        void OnClickButtonOk(string value)
        {
            CloseSelf(new EventData<string>("Result", value));
        }

        void OnClickButtonExit()
        {
            CloseSelf(new EventData("Exit"));
        }
    }
}