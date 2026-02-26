using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleCoinPusher01
{
    public class PageConsoleTestCoinPush : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleTestCoinPush";
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
            /*
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
            (GameObject clone) =>
            {
                callback();
            });
            */
            callback();



            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                            OnClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                            OnClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                            OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        OnClickNext();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        OnClickPrev();
                    },
                    [MachineButtonKey.BtnConsole] = (info) =>
                    {
                        CloseSelf(null);
                    }
                }
            };

        }


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("退币测试");
            base.OnOpen(name, data);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, changDownCoin);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_RETURN_COIN, changReturnCoin);
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, changDownCoin);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_RETURN_COIN, changReturnCoin);
            base.OnClose(data);
        }



        

        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu;




        Dictionary<int, string> menuMap;



        int curIndexMenuItem = 0;

        public override void InitParam()
        {

 
            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName() );


          

            AddClickEvent();

            Clear(true);
        }


        void Clear(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            curIndexMenuItem = 0;
            goMenu = this.contentPane.GetChild("items").asCom;
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                GComponent goItem = goMenu.GetChildAt(i).asCom;
                if (isClearAllArrow)
                    goItem.GetChild("icon").visible = false;
                else
                    goItem.GetChild("icon").visible = i == curIndexMenuItem;

                menuMap.Add(i, goItem.name);
            }

        }


        void SetAllow()
        {
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                goMenu.GetChildAt(i).asCom.GetChild("icon").visible = i == curIndexMenuItem;
            }
        }
        void OnClickNext()
        {
            if (++curIndexMenuItem >= goMenu.numChildren)
                curIndexMenuItem = 0;
            SetAllow();
        }

        void OnClickPrev()
        {
            if (--curIndexMenuItem < 0)
                curIndexMenuItem = goMenu.numChildren - 1;
            SetAllow();
        }

        void OnClickConfirm()
        {
            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem])
                {
                    case "coinPush":
                        {
                            /*
                            PusherMachineDataManager.Instance.RequestCosoleTopCoinIn((res) =>
                            {
                                int state = (int)res;
                                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "停止测试" : "开始测试";
                            });*/
                        }
                        return;
                    case "reset":
                        {
                            Reset();
                        }
                        return;
                    case "exit":
                        {
                            CloseSelf(null);
                        }
                        return;
                }
            }
        }


        void AddClickEvent()
        {
            for (int i = 0; i < contentPane.GetChild("items").asCom.numChildren; i++)
            {
                int index = i;
                contentPane.GetChild("items").asCom.GetChildAt(index).onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }


        void Reset()
        {
            goMenu.GetChild("reset").asCom.GetChild("value1").asRichTextField.text = 0.ToString();
            goMenu.GetChild("reset").asCom.GetChild("value2").asRichTextField.text = 0.ToString();
            goMenu.GetChild("reset").asCom.GetChild("value3").asRichTextField.text = "0%";
        }

        void BeginTest()
        {
            int.TryParse(goMenu.GetChild("reset").asCom.GetChild("value1").asRichTextField.text, out int result);
            int.TryParse(goMenu.GetChild("reset").asCom.GetChild("value2").asRichTextField.text, out int result2);
            double ratio;
            if (result == 0)
            {
                // 处理除数为 0 的情况，例如显示 0% 或提示信息
                goMenu.GetChild("reset").asCom.GetChild("value3").asRichTextField.text = "0%";
            }
            else
            {
                ratio = (double)result2 / result;
                goMenu.GetChild("reset").asCom.GetChild("value3").asRichTextField.text = ratio.ToString("P2");
            }

        }

        void changDownCoin(int n)
        {
            if (n == 0)
            {
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = "开始测试";
                return;
            }
            int.TryParse(goMenu.GetChild("reset").asCom.GetChild("value1").asRichTextField.text, out int result);
            goMenu.GetChild("reset").asCom.GetChild("value1").asRichTextField.text = (n+result).ToString();
            BeginTest();
        }

        void changReturnCoin(int n)
        {
            if (n == 0)
            {
                return;
            }
            int.TryParse(goMenu.GetChild("reset").asCom.GetChild("value2").asRichTextField.text, out int result);
            goMenu.GetChild("reset").asCom.GetChild("value2").asRichTextField.text = (n + result).ToString();
            BeginTest();
            
        }

    }

}

