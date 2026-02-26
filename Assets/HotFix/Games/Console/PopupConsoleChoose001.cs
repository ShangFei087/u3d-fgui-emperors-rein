using FairyGUI;
using GameMaker;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleSlot01
{
    public class PopupConsoleChoose001 : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleChoose001";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();

            int count = 1;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };
            callback();
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose;

        

        GRichTextField gtxtTitle, gtxtParam1;
        GList glst;


        Dictionary<string, string> selectLst = new Dictionary<string, string>()
        {
            ["1"] = "--",
        };
        string selectNumber = "1";
        string title = "--";
        Func<string, string> getSelectDes = (number) => { return "--"; };


        //readonly Color HIGHLIGHT_BG_COLOR = GameCommon.Utils.ParseColor("#00FD8D");
        //readonly Color NORMAL_BG_COLOR = GameCommon.Utils.ParseColor("#09221F");
        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            btnClose = this.contentPane.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(new EventData("Exit"));
            });

            gtxtTitle = this.contentPane.GetChild("title").asRichTextField;

            gtxtParam1 = this.contentPane.GetChild("param1").asRichTextField;

            glst = this.contentPane.GetChild("list").asList;


            if (inParams != null)
            {
                Dictionary<string, object> argDic = null;

                argDic = (Dictionary<string, object>)inParams.value;

                if (argDic.ContainsKey("selectLst"))
                {
                    selectLst = (Dictionary<string, string>)argDic["selectLst"];
                }

                if (argDic.ContainsKey("selectNumber"))
                {
                    selectNumber = (string)argDic["selectNumber"];
                }

                if (argDic.ContainsKey("getSelectedDes"))
                {
                    getSelectDes = (Func<string, string>)argDic["getSelectedDes"];
                }

                if (argDic.ContainsKey("title"))
                {
                    title = (string)argDic["title"];
                }
            }

            gtxtTitle.text = title;
            gtxtParam1.text = getSelectDes(selectNumber);

            glst.itemRenderer = (int index, GObject obj) =>
            {
                KeyValuePair<string, string> kv = selectLst.ElementAt(index);
                GButton btn = obj.asButton;
                btn.title = kv.Value;

                obj.asCom.GetChild("bgHighLight").visible = selectNumber == kv.Key;

                btn.onClick.Clear();
                btn.onClick.Add(() =>
                {
                    selectNumber = kv.Key;
                    OnSelectItem(kv.Key);
                });
            };
            glst.numItems = selectLst.Count;

        }


        void OnSelectItem(string number)
        {
            CloseSelf(new EventData<string>("Result", number));
        }
    }
}