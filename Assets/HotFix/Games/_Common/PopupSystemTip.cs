using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameMaker;

namespace Common
{
    public class PopupSystemTip : PageBase
    {
        public const string pkgName = "Common";
        public const string resName = "PopupSystemTip";

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


        Color? lastModalLayerColor = null;
        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);

            //GRoot.inst.CloseModalWait();
            //GRoot.inst.modalLayer.visible = false;
            lastModalLayerColor = GRoot.inst.modalLayer.color;
            GRoot.inst.modalLayer.color = new Color(0, 0, 0, 0f); //改成透明


            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            //GRoot.inst.modalLayer.visible = true;
            if (lastModalLayerColor != null)
            {
                GRoot.inst.modalLayer.color = (Color)lastModalLayerColor;
            }
            lastModalLayerColor = null;

            base.OnClose(data); 
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GRichTextField rtxtTip;

        
  
        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            rtxtTip = this.contentPane.GetChild("title").asRichTextField;
            rtxtTip.text = "【温馨提示】";

            //CloseSelf(null);

            /*if (_data != null)
            {
                Dictionary<string, object>  argDic = (Dictionary<string, object>)_data.value;
                rtxtTip.text = (string)argDic["content"];
            }*/

            ShowTip(inParams);
        }


        public void ShowTip(EventData data)
        {
            if (inParams != null)
            {
                Dictionary<string, object> argDic = (Dictionary<string, object>)inParams.value;
                rtxtTip.text = (string)argDic["content"];
            }
        }

    }



}