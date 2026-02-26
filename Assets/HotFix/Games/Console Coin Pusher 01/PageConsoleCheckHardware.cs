using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleCoinPusher01 
{ 
    public class PageConsoleCheckHardware : MachinePageBase
    {
        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleCheckHardware";
        public override PageType pageType => PageType.Overlay;

        private Dictionary<string, TimerCallback> timerDict;
        private List<TimerCallback> callBackList = new List<TimerCallback>();
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
                    [MachineButtonKey.BtnBetMax] = (info) =>
                    {
                            //OnClickConfirm();
                    },
                    [MachineButtonKey.BtnNext] = (info) =>
                    {
                            OnClickNext();
                    },
                    [MachineButtonKey.BtnPrev] = (info) =>
                    {
                            OnClickPrev();
                    },
                    [MachineButtonKey.BtnExit] = (info) =>
                    {
                        CloseSelf(null);
                    }
                }
            };

        }


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("硬件功能测试");
            base.OnOpen(name, data);
        
            InitParam();



            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOP_COIN_IN, CoinInNum);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, BonusNum);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN, CollectCoinNum);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER, TicketerNum);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, TriggerCoinIn);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_COIN_BLOCK, TriggerCoinBlock);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_SHAKE, TriggerShake);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_SPIN, TriggerBtnSpin);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER, TriggerBtnWiper);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOUCH_CHANNEL, TriggerTouchChannel);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOUCH_SP, TriggerSP);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_RETURN_COIN, TriggerRetrunCoin);
            
        }

        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();

            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOP_COIN_IN, CoinInNum);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, BonusNum);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN, CollectCoinNum);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER, TicketerNum);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, TriggerCoinIn);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_COIN_BLOCK, TriggerCoinBlock);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_SHAKE, TriggerShake);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_SPIN, TriggerBtnSpin);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER, TriggerBtnWiper);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOUCH_CHANNEL, TriggerTouchChannel);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOUCH_SP, TriggerSP);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_RETURN_COIN, TriggerRetrunCoin);



            base.OnClose(data);
        }




        //GButton btnClose;

        


        GComponent glst;
        
        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu;




        Dictionary<int, string> menuMap;



        int curIndexMenuItem = 0;
        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());
                
            glst = this.contentPane.GetChild("items2").asCom;
            timerDict = new Dictionary<string, TimerCallback>();
            for (int i = 0; i < glst.numChildren; i++)
            {
                string name = glst.GetChildAt(i).name;
                timerDict.Add(name, (object obj) => TimersCallBack(name));
            }
            callBackList.Clear();

            AddClickEvent();

            Clear(false);
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

        /// <summary>
        /// 四个计数
        /// </summary>
        /// <param name="n"></param>
        #region
        void CoinInNum(int n)
        {
            if (n == 0)
            {
                goMenu.GetChild("topCoinIn").asCom.GetChild("value1").asRichTextField.text = "OFF";
                return;
            }
            int.TryParse(goMenu.GetChild("topCoinIn").asCom.GetChild("value3").asRichTextField.text, out int result);
            goMenu.GetChild("topCoinIn").asCom.GetChild("value3").asRichTextField.text = (result + 1).ToString();
        }


        void CollectCoinNum(int n)
        {
            if (n == 0)
            {
                goMenu.GetChild("collectCoin").asCom.GetChild("value1").asRichTextField.text = "OFF";
                return;
            }
            int.TryParse(goMenu.GetChild("collectCoin").asCom.GetChild("value3").asRichTextField.text, out int result);
            goMenu.GetChild("collectCoin").asCom.GetChild("value3").asRichTextField.text = (result + 1).ToString();
        }


        void TicketerNum(int n)
        {
            if (n == 0)
            {
                goMenu.GetChild("ticketer").asCom.GetChild("value1").asRichTextField.text = "OFF";
                return;
            }
            int.TryParse(goMenu.GetChild("ticketer").asCom.GetChild("value3").asRichTextField.text, out int result);
            goMenu.GetChild("ticketer").asCom.GetChild("value3").asRichTextField.text = (result + 1).ToString();
        }

        void BonusNum(int n)
        {
            if (n == 0)
            {
                goMenu.GetChild("ballIn").asCom.GetChild("value1").asRichTextField.text = "OFF";
                return;
            }
            int.TryParse(goMenu.GetChild("ballIn").asCom.GetChild("value3").asRichTextField.text, out int result);
            goMenu.GetChild("ballIn").asCom.GetChild("value3").asRichTextField.text = (result + 1).ToString();
        }
        #endregion


        /// <summary>
        /// 顶部绿光计数
        /// </summary>
        /// <param name="n"></param>
        #region 
        void TriggerCoinIn(int n)
        {
            for (int i = 0; i <= n; i++)
            {
                Trigger("coinIn");
            }
        }

        void TriggerCoinBlock(int n)
        {
            for (int i = 0; i <= n; i++)
            {
                Trigger("coinBlock");
            }
        }

        void TriggerShake(int n)
        {
            for (int i = 0; i <= n; i++)
            {
                Trigger("shake");
            }
        }


        void TriggerBtnSpin(int n)
        {
            for (int i = 0; i <= n; i++)
            {
                Trigger("btnSpin");
            }
        }

        void TriggerBtnWiper(int n)
        {
            for (int i = 0; i <= n; i++)
            {
                Trigger("btnWiper");
            }
        }


        void TriggerRetrunCoin(int n)
        {
            for (int i = 0; i <= n; i++)
            {
                Trigger("coinObtain");
            }
        }
        void TriggerSP(int n)
        {
            if (n == 0)
            {
                Trigger("sp1");
            }
            else if (n == 1)
            {
                Trigger("sp2");
            }

        }
        void TriggerTouchChannel(int n)
        {
            Trigger(("channel" + (n + 1).ToString()));
        }

        void Trigger(string name)
        {
            if (callBackList.Contains(timerDict[name]))
            {
                Timers.inst.Remove(timerDict[name]);
                callBackList.Remove(timerDict[name]);
            }
            glst.GetChild(name).asCom.GetChild("icon").asImage.color = Color.green;
            int.TryParse(glst.GetChild(name).asCom.GetChild("value").asRichTextField.text, out int result);
            glst.GetChild(name).asCom.GetChild("value").asRichTextField.text = (result + 1).ToString();
            Timers.inst.Add(0.5f, 1, timerDict[name]);
            callBackList.Add(timerDict[name]);
        }

        void TimersCallBack(string name)
        {
            glst.GetChild(name).asCom.GetChild("icon").asImage.color = Color.white;
            callBackList.Remove(timerDict[name]);
        }


        #endregion





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
            if(curIndexMenuItem==2|| curIndexMenuItem == 7)
            {
                ++curIndexMenuItem;
            }
            SetAllow();
        }

        void OnClickPrev()
        {
            if (--curIndexMenuItem < 0)
                curIndexMenuItem = goMenu.numChildren - 1;
            if (curIndexMenuItem == 2 || curIndexMenuItem == 7)
            {
                --curIndexMenuItem;
            }
            SetAllow();
        }



        void AddClickEvent()
        {
            for (int i = 0; i < contentPane.GetChild("items").asCom.numChildren; i++)
            {
                int index = i;
                if (index == 2 || index == 7)
                {
                    continue;
                }
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("title").onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("title").onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                   // OnClickConfirm();
                });
            }
        }



        /*
         * 
         * 
         *         void OnClickConfirm()
        {
            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem])
                {
                    case "lightSpin":
                        {
                            LightSpin();
                        }
                        return;
                    case "lightWiperOFF":
                        {
                            LightWiper();
                        }
                        return;
                    case "topCoinIn":
                        {
                            topCoinIn();
                        }
                        return;
                    case "ballIn":
                        {
                            ballIn();
                        }
                        return;
                    case "collectCoin":
                        {
                            CollectCoin();
                        }
                        return;
                    case "ticketer":
                        {
                            ticketer();
                        }
                        return;
                    case "wiper":
                        {
                            wiper();
                        }
                        return;
                    case "pushPlate":
                        {
                            pushPlate();
                        }
                        return;
                    case "bell":
                        {
                            bell();
                        }
                        return;
                    case "channelLight1":
                        {
                            ChanneILight(1);
                        }
                        return;
                    case "channelLight2":
                        {
                            ChanneILight(2);
                        }
                        return;
                    case "channelLight3":
                        {
                            ChanneILight(3);
                        }
                        return;
                    case "channelLight4":
                        {
                            ChanneILight(4);
                        }
                        return;
                    case "channelLight5":
                        {
                            ChanneILight(5);
                        }
                        return;
                    case "channelLight6":
                        {
                            ChanneILight(6);
                        }
                        return;
                    case "channelLight7":
                        {
                            ChanneILight(7);
                        }
                        return;
                    case "channelLight8":
                        {
                            ChanneILight(8);
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






        void LightSpin()
        {
            PusherMachineDataManager.Instance.RequestCosoleLightSpin((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }


        void LightWiper()
        {
            PusherMachineDataManager.Instance.RequestCosoleWiperOff((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }

        void topCoinIn()
        {
            PusherMachineDataManager.Instance.RequestCosoleTopCoinIn((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }


        void ballIn()
        {
            PusherMachineDataManager.Instance.RequestCosoleBonusIn((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }

        void CollectCoin()
        {
            PusherMachineDataManager.Instance.RequestCosoleCollectCoin((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }


        void ticketer()
        {
            PusherMachineDataManager.Instance.RequestCosoleTicketer((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }

        void wiper()
        {
            PusherMachineDataManager.Instance.RequestCosoleWiper((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }

        void pushPlate()
        {
            PusherMachineDataManager.Instance.RequestCosolePushPlate((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }

        void bell()
        {
            PusherMachineDataManager.Instance.RequestCosoleBell((res) =>
            {
                int state = (int)res;
                goMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value1").asRichTextField.text = state == 1 ? "ON" : "OFF";
            });

        }

        void ChanneILight(int n)
        {

            PusherMachineDataManager.Instance.RequestCosoleChanneiLight((res) =>
            {
                int state = (int)res;
                switch (n)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        break;
                }
            });
          
        }
        */
    }

}
