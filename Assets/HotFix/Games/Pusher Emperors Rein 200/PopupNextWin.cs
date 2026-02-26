using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PusherEmperorsRein
{


    public class PopupNextWin : PageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupNextWin";

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

        /// <summary> ̧ͷ </summary>
        string title = "";

        /// <summary> �Ƿ����� </summary>
        bool isPlaintext = false;

        string inputText = "";

        public override void InitParam()
        {
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
        }

        void End(int value)
        {
            //PageManager.Instance.ClosePage(this, new EventData<string>("Result", value));
            CloseSelf(new EventData<int>("Result", value));
        }
    }
}