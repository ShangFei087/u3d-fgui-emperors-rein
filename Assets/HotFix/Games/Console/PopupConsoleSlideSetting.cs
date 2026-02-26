using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ConsoleSlot01
{
    public class PopupConsoleSlideSetting : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleSlideSetting";
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

        public override void OnClose(EventData data = null)
        {
            base.OnClose(data);
            onChangeUI = null;
            isUseKeyboard = true;
        }

        Button btn01;
        // public override void OnTop() {  DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose, btnKeyboard, btnConfirm;

        GSlider slider;


        GRichTextField rtxtTitle;


        string title;
        int valueCur;
        int valueMax;
        int valueMin;
        bool isUseKeyboard = true;


        Func<int, string> onChangeUI;

        public override void InitParam()
        {
            btnClose = this.contentPane.asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(new EventData("Exit"));
            });

            rtxtTitle = this.contentPane.GetChild("title").asRichTextField;

            slider = this.contentPane.GetChild("slider").asSlider;
            slider.onChanged.Clear();
            slider.onChanged.Add(OnSlide);

            btnKeyboard = this.contentPane.asCom.GetChild("btnKeyboard").asButton;
            btnKeyboard.onClick.Clear();
            btnKeyboard.onClick.Add(OnClickKeyboard);

            btnConfirm = this.contentPane.asCom.GetChild("btnConfirm").asButton;
            btnConfirm.onClick.Clear();
            btnConfirm.onClick.Add(OnClickConfirm);

            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                title = (string)argDic["title"];
                valueMax = (int)argDic["valueMax"];
                valueMin = (int)argDic["valueMin"];

                if (argDic.ContainsKey("isUseKeyboard"))
                    isUseKeyboard = (bool)argDic["isUseKeyboard"];

                if (argDic.ContainsKey("onChangeUI"))
                    onChangeUI = (Func<int, string>)argDic["onChangeUI"];



                DebugUtils.Log($"@@@@ {(int)argDic["valueCur"]}");
                //设置滚轮位置
                SetCurSlider((int)argDic["valueCur"]);
                SetUIContent();
            }

            btnKeyboard.visible = isUseKeyboard;

        }

        bool isScriptSetSlide = false;

        void OnSlide(EventContext context)
        {
            GSlider slider01 = (GSlider)context.sender;

            double value = slider01.value;

            if (isScriptSetSlide)
            {
                isScriptSetSlide = false;
                return;
            }

            valueCur = GetCurValue(value);

            SetUIContent();
        }



        void SetUIContent()
        {
            string value = onChangeUI != null ? onChangeUI(valueCur) : $"1:{valueCur}";
            rtxtTitle.text = $"{title}<br>{value}";
            // ticket out scale(票：钞)<br>200:1
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">0到1</param>
        /// <returns></returns>
        int GetCurValue(double value)
        {
            //value = value / 100; // 0~ 100
            return valueMin + (int)(value * (valueMax - valueMin));
        }

        void SetCurSlider(int value)
        {
            valueCur = value;

            if (valueCur < valueMin)
                valueCur = valueMin;
            else if (valueCur > valueMax)
                valueCur = valueMax;

            //设置滚轮位置
            float tmp = ((float)valueCur - (float)valueMin) / ((float)valueMax - (float)valueMin);

            isScriptSetSlide = true;
            slider.value = tmp;
            isScriptSetSlide = false;
        }

        async void OnClickKeyboard()
        {
            EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["title"] = I18nMgr.T("Input Custom Value"),
                        ["isPlaintext"] = true,
                    }));

            if (res.value != null)
            {
                bool isErr = true;
                try
                {
                    int val = int.Parse((string)res.value);
                    if (val >= valueMin && val <= valueMax)
                    {
                        isErr = false;
                        SetCurSlider(val);
                        SetUIContent();
                    }
                }
                catch { }

                if (isErr)
                    TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"), I18nMgr.T(title), valueMin, valueMax));
            }

        }

        void OnClickConfirm()
        {
            valueCur = GetCurValue(slider.value);
            CloseSelf(new EventData<int>("Result", valueCur));


        }



    }
}