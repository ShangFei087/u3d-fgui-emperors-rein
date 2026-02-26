using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMaker;
using FairyGUI;


namespace ConsoleSlot01
{
    public class PopupConsoleCommon : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleCommon";
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

        GButton btnClose, btnButton1, btnButton2;

        GTextField txtTitle01, txtContent01, txtContent02;

        Controller controller;
        public override void InitParam()
        {
            btnClose = this.contentPane.GetChild("btnExit").asButton;
            /*btnClose.onClick.Clear();
            btnClose.onClick.Add(() => {
                //DebugUtils.Log("i am here 123");
                CloseSelf(null);
            });*/

            txtTitle01 = this.contentPane.GetChild("title01").asTextField;
            txtContent01 = this.contentPane.GetChild("content01").asTextField;
            txtContent02 = this.contentPane.GetChild("content02").asTextField;

            btnButton1 = this.contentPane.GetChild("button1").asButton;
            btnButton2 = this.contentPane.GetChild("button2").asButton;

            controller = this.contentPane.GetController("c1");

            SetContent(inParams);
        }

        CommonPopupInfo curInfo;
        public void SetContent(EventData info)
        {
            if (info != null)
                curInfo = info.value as CommonPopupInfo;

            btnClose.visible = false;

            txtTitle01.visible = false;
            txtContent01.visible = false;
            txtContent02.visible = false;

            btnClose.onClick.Clear();
            btnButton1.onClick.Clear();
            btnButton2.onClick.Clear();

            switch (curInfo.type)
            {
                case CommonPopupType.SystemReset:
                case CommonPopupType.OK:
                    {

                        controller.selectedPage = "ok";
                        btnButton1.title = curInfo.buttonText1;
                        btnButton1.onClick.Add(() =>
                        {
                            curInfo.callback1?.Invoke();
                            if (curInfo.buttonAutoClose1)
                                CommonPopupHandler.Instance.ClosePopup();
                        });
                    }
                    break;
                case CommonPopupType.YesNo:
                    {
                        controller.selectedPage = "yesNo";

                        btnButton1.title = curInfo.buttonText1;
                        btnButton1.onClick.Add(() =>
                        {
                            curInfo.callback1?.Invoke();
                            if (curInfo.buttonAutoClose1)
                                CommonPopupHandler.Instance.ClosePopup();
                        });

                        btnButton2.title = curInfo.buttonText2;
                        btnButton2.onClick.Add(() =>
                        {
                            curInfo.callback2?.Invoke();
                            if (curInfo.buttonAutoClose2)
                                CommonPopupHandler.Instance.ClosePopup();
                        });
                    }
                    break;
                case CommonPopupType.SystemTextOnly:
                case CommonPopupType.TextOnly:
                    {
                        controller.selectedPage = "textOnly";
                    }
                    break;
            }


            if (!string.IsNullOrEmpty(curInfo.title))
            {
                txtTitle01.visible = true;
                txtTitle01.text = curInfo.title;

                txtContent01.visible = true;
                txtContent01.text = curInfo.text;
            }
            else
            {
                txtContent02.visible = true;
                txtContent02.text = curInfo.text;
            }


            if (curInfo.isUseXButton)
            {
                btnClose.visible = true;
                btnClose.onClick.Add(() =>
                {
                    curInfo.callbackX?.Invoke();
                    CommonPopupHandler.Instance.ClosePopup();
                });
            }

        }

        public void SetPopupToTop()
        {
            PageManager.Instance.SetPageToTop(this);
        }

    }
}