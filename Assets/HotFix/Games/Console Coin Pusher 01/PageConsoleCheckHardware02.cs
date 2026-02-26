using FairyGUI;
using GameMaker;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleCoinPusher01
{
    public class PageConsoleCheckHardware02 : MachinePageBase
    {
        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleCheckHardware02";
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
                },
            };

        }


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("硬件功能测试");
            base.OnOpen(name, data);

            InitParam();

            old = null;
            neww = null;

            InitHardwaveTest();

            Timers.inst.Add(0.05f, 0, CycleReadFlag);

        }

        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            Timers.inst.Remove(CycleReadFlag);

            CloseAllTestCheckUI();
            base.OnClose(data);
        }





        static bool isInitGetHardwareTestFlg = false;

        static bool isTestCoinDown = false;
        static bool isTestBallDown = false;
        static bool isTestPlate = false;
        static bool isTestWiper = false;
        static bool isTestRegainCoins = false;
        static bool isTestRegainBalls = false;

        //GButton btnClose;

        


        GComponent glst;

        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu;


        Dictionary<int, string> menuMap;

        JSONNode old, neww;
        GComponent goCoin, goBall,goWiper;
        int new_old1 = 0;
        int new_old2 = 0;
        bool new_old3;
        bool new_old4;
        int new_old6 = 0;
        int new_old7 = 0;

        int curIndexMenuItem = 1;
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


            ChangeMenuItem();
            Clear(false);
            AddClickEvent();
            CheckUI();

            goMenu.GetChild("topCoinIn").asCom.GetChild("value3").asRichTextField.text = "0";
            goMenu.GetChild("ballIn").asCom.GetChild("value3").asRichTextField.text = "0";
            goCoin.GetChild("value3").asRichTextField.text = "0";
            goBall.GetChild("value3").asRichTextField.text = "0";


        }



        void ChangeMenuItem()
        {

            goMenu = this.contentPane.GetChild("items").asCom;
            GComponent goItem = goMenu.GetChild("RegainCoins")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "RegainCoins";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goCoin != null && goCoin.displayObject.gameObject != null)
                    goCoin.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goCoin = goItem;
                goCoin.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }



            goItem = goMenu.GetChild("RegainBalls")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "RegainBalls";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goBall != null && goBall.displayObject.gameObject != null)
                    goBall.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goBall = goItem;
                goBall.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }

            goItem = goMenu.GetChild("wiper")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "wiper";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goWiper != null && goWiper.displayObject.gameObject != null)
                    goWiper.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goWiper = goItem;
                goWiper.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }

            

            if (IsEnableCheckRegainTest())
            {
                goMenu.AddChildAt(goCoin, 3);
                goMenu.AddChildAt(goBall, 4);
            }
        }


        public static void InitHardwaveTest()
        {
            if (isInitGetHardwareTestFlg)
                return;

            /*
            PusherMachineDataManager02.Instance.RequestGetCoinPushHardwareFlag((res) =>
            {
                JSONNode jsonNode = JSONNode.Parse(res.ToString());

                int bit0 = (int)jsonNode["flag"] & 1; bool isCoinDown = bit0 != 0;
                int bit1 = (int)jsonNode["flag"] & 2; bool isBallDown = bit1 != 0;
                isTestBallDown = isBallDown;
                isTestCoinDown = isCoinDown;

                isInitGetHardwareTestFlg = true;


                CloseAllTest();
            });
            */

            CloseAllTest(() =>
            {
                isInitGetHardwareTestFlg = true;
            });
        }


        bool IsEnableCheckRegainTest()
        {
            if (Application.isEditor)
                return true;
            Version currentVersion = new Version(SBoxModel.Instance.AlgorithmVer);
            Version targetVersion = new Version("1.0.6");
            return currentVersion >= targetVersion;   
        }

        void CycleReadFlag(object obj)
        {
            /* PusherMachineDataManager02.Instance.RequestGetCoinPushHardwareFlag((res) =>
             {
                 JSONNode jsonNode = JSONNode.Parse(res.ToString());

                 int bit0 = (int)jsonNode["flag"] & 1; bool isCoinDown = bit0 != 0;
                 int bit1 = (int)jsonNode["flag"] & 2; bool isBallDown = bit1 != 0;
                 goMenu.GetChild("topCoinIn").asCom.GetChild("value1").asRichTextField.text = isCoinDown ? "ON" : "OFF";

                 goMenu.GetChild("ballIn").asCom.GetChild("value1").asRichTextField.text = isBallDown ? "ON" : "OFF";

             });*/


            if (!isTestCoinDown && !isTestBallDown && !isTestPlate && !isTestWiper && !isTestRegainBalls && !isTestRegainCoins)
                return;



            DebugUtils.LogWarning("读取20109数据");

            goMenu.GetChild("topCoinIn").asCom.GetChild("value1").asRichTextField.text = isTestCoinDown ? "ON" : "OFF";

            goMenu.GetChild("ballIn").asCom.GetChild("value1").asRichTextField.text = isTestBallDown ? "ON" : "OFF";

            PusherMachineDataManager02.Instance.RequestGetCoinPushHardwareResult((res) =>
            {

                JSONNode next = JSONNode.Parse((string)res);
                if (old == null)
                {
                    old = next;
                }
                if (neww == null)
                {
                    neww = next;
                }
                int coin = next["coinPushTestCoins"] - neww["coinPushTestCoins"];
                int ball = next["coinPushTestBalls"] - neww["coinPushTestBalls"];
                int regainCoin = next["coinPushTestRegainCoins"] - neww["coinPushTestRegainCoins"];
                int regainball = next["coinPushTestRegainBalls"] - neww["coinPushTestRegainBalls"];
                neww = next;

                new_old1 = neww["coinPushTestCoins"] - old["coinPushTestCoins"];
                new_old2 = neww["coinPushTestBalls"] - old["coinPushTestBalls"];
                new_old3 = neww["coinPushTestPlate"] == 1 ? true : false;
                new_old4 = neww["coinPushTestWiper"] == 1 ? true : false;
                new_old6 = neww["coinPushTestRegainCoins"] - old["coinPushTestRegainCoins"];
                new_old7 = neww["coinPushTestRegainBalls"] - old["coinPushTestRegainBalls"];

                goMenu.GetChild("topCoinIn").asCom.GetChild("value3").asRichTextField.text = new_old1.ToString();
                if (coin > 0)
                {
                    goMenu.GetChild("topCoinIn").asCom.GetChild("value2").asRichTextField.text = "ON";
                    Timers.inst.Add(0.2f, 1, (object obj) =>
                    {
                        goMenu.GetChild("topCoinIn").asCom.GetChild("value2").asRichTextField.text = "OFF";
                    });
                }

                if (ball > 0)
                {
                    goMenu.GetChild("ballIn").asCom.GetChild("value2").asRichTextField.text = "ON";
                    Timers.inst.Add(0.2f, 1, (object obj) =>
                    {
                        goMenu.GetChild("ballIn").asCom.GetChild("value2").asRichTextField.text = "OFF";
                    });
                }
                if (regainCoin > 0)
                {
                    goMenu.GetChild("RegainCoins").asCom.GetChild("value2").asRichTextField.text = "ON";
                    Timers.inst.Add(0.2f, 1, (object obj) =>
                    {
                        goMenu.GetChild("RegainCoins").asCom.GetChild("value2").asRichTextField.text = "OFF";
                    });
                }

                if (regainball > 0)
                {
                    goMenu.GetChild("RegainBalls").asCom.GetChild("value2").asRichTextField.text = "ON";
                    Timers.inst.Add(0.2f, 1, (object obj) =>
                    {
                        goMenu.GetChild("RegainBalls").asCom.GetChild("value2").asRichTextField.text = "OFF";
                    });
                }

                this.contentPane.GetChild("tip").asRichTextField.text = "掉币" + neww[1] + "掉球" + neww[2] + "推盘" + neww[3] + "雨刷" + neww[4]
                + $"<br><br>{next.ToString()}";
                //DebugUtils.LogError($"  {next.ToString()}");

                goMenu.GetChild("ballIn").asCom.GetChild("value3").asRichTextField.text = new_old2.ToString();
                goCoin.asCom.GetChild("value3").asRichTextField.text = new_old6.ToString();
                goBall.asCom.GetChild("value3").asRichTextField.text = new_old7.ToString();
                isTestPlate = new_old3 == true;
                isTestWiper = new_old4 == true;

                goWiper.asCom.GetChild("value1").asRichTextField.text = isTestWiper ? "ON" : "OFF";
                goMenu.GetChild("pushPlate").asCom.GetChild("value1").asRichTextField.text = isTestPlate ? "ON" : "OFF";

            });



            /*
            PusherMachineDataManager02.Instance.RequestGetCoinPushHardwareState((res) =>
            {
                JSONNode next = JSONNode.Parse((string)res);
                if (old == null)
                {
                    old = next;
                }
                if (neww == null)
                {
                    neww = next;
                }
                int coin = next[1] - neww[1];
                int ball = next[2] - neww[2];

                neww = next;
              
                new_old1 = neww[1] - old[1];
                new_old2 = neww[2] - old[2];
                new_old3 = neww[3] == 1 ? true : false;
                new_old4 = neww[4] == 1 ? true : false;

                goMenu.GetChild("topCoinIn").asCom.GetChild("value3").asRichTextField.text = new_old1.ToString();
                this.contentPane.GetChild("tip").asRichTextField.text = "掉币" + neww[1] + "掉球" + neww[2] + "推盘" + neww[3] + "雨刷" + neww[4];
                goMenu.GetChild("ballIn").asCom.GetChild("value3").asRichTextField.text = new_old2.ToString();
                goMenu.GetChild("wiper").asCom.GetChild("value1").asRichTextField.text = new_old4 == true ? "ON" : "OFF";
                goMenu.GetChild("pushPlate").asCom.GetChild("value1").asRichTextField.text = new_old3 == true ? "ON" : "OFF";


            });
            */
        }

        void Clear(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            curIndexMenuItem = 1;
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
            goMenu.GetChild("topCoinIn").asCom.GetChild("value2").asRichTextField.text = "ON";
            Timers.inst.Add(0.2f, 1, (object obj) =>
            {
                goMenu.GetChild("topCoinIn").asCom.GetChild("value2").asRichTextField.text = "OFF";
            });
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
            goMenu.GetChild("ballIn").asCom.GetChild("value2").asRichTextField.text = "ON";
            Timers.inst.Add(0.2f, 1, (object obj) =>
            {
                goMenu.GetChild("ballIn").asCom.GetChild("value2").asRichTextField.text = "OFF";
            });
        }
        #endregion


        /// <summary>
        /// 顶部绿光计数
        /// </summary>
        /// <param name="n"></param>
        #region 


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
            callBackList.Add(timerDict[name]);
            Timers.inst.Add(0.2f, 1, timerDict[name]);
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
            if (curIndexMenuItem == goMenu.GetChildIndex(goMenu.GetChild("_title2")) || curIndexMenuItem == 0)
            {
                ++curIndexMenuItem;
            }
            SetAllow();
        }

        void OnClickPrev()
        {
            if (--curIndexMenuItem < 1)
                curIndexMenuItem = goMenu.numChildren - 1;
            if (curIndexMenuItem == goMenu.GetChildIndex(goMenu.GetChild("_title2")))
            {
                --curIndexMenuItem;
            }
            SetAllow();
        }

        void OnClickConfirm()
        {
            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem])
                {
                    case "topCoinIn":
                        {
                            DoTopCoinInTest();
                        }
                        return;
                    case "ballIn":
                        {
                            DoBallInTest();
                        }
                        return;
                    case "wiper":
                        {
                            DoWiperTest();
                        }
                        return;
                    case "pushPlate":
                        {
                            DoPushPlateTest();
                        }
                        return;
                    case "RegainCoins":
                        {
                            DoRegainCoinsTest();
                        }
                        return;
                    case "RegainBalls":
                        {
                            DoRegainBallsTest();
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
                if (index == goMenu.GetChildIndex(goMenu.GetChild("_title2")) || index == 0)
                {
                    continue;
                }
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("title").onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("title").onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }



        static void CloseAllTest(Action callback = null)
        {
            PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(255, (res2) =>
            {
                JSONNode result = JSONNode.Parse((string)res2);
                if ((int)result["code"] == 0)
                {
                    isTestWiper = (int)result["coinPushTestWiper"] == 1;
                    isTestPlate = (int)result["coinPushTestPlate"] == 1;
                    isTestBallDown = false;
                    isTestCoinDown = false;
                    isTestRegainCoins = false;
                    isTestRegainBalls = false;

                    callback?.Invoke();
                }
            });
        }


        void CloseAllTestCheckUI(Action callback = null)
        {
            CloseAllTest(() =>
            {
                CheckUI();
                callback?.Invoke();
            });
        }


        void CheckUI()
        {
            goMenu.GetChild("topCoinIn").asCom.GetChild("value1").asRichTextField.text = isTestCoinDown ? "ON" : "OFF";
            goMenu.GetChild("ballIn").asCom.GetChild("value1").asRichTextField.text = isTestBallDown ? "ON" : "OFF";
            goWiper.text = isTestWiper ? "ON" : "OFF";
            goMenu.GetChild("pushPlate").asCom.GetChild("value1").asRichTextField.text = isTestPlate ? "ON" : "OFF";
            goCoin.GetChild("value1").text = isTestRegainCoins ? "ON" : "OFF";
            goBall.GetChild("value1").text = isTestRegainBalls ? "ON" : "OFF";
        }



        void DoTopCoinInTest()
        {
            if (isTestCoinDown)
                CloseAllTestCheckUI();
            else
            {
                CloseAllTestCheckUI(() =>
                {
                    PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(1, (res2) =>
                    {
                        JSONNode result = JSONNode.Parse((string)res2);
                        if ((int)result["code"] == 0)
                        {
                            isTestCoinDown = !isTestCoinDown;
                            isTestWiper = (int)result["coinPushTestWiper"] == 1;
                            isTestPlate = (int)result["coinPushTestPlate"] == 1;
                            CheckUI();
                        }
                    });
                });

            }

        }


        void DoBallInTest()
        {

            if (isTestBallDown)
                CloseAllTestCheckUI();
            else
            {
                CloseAllTestCheckUI(() =>
                {
                    PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(2, (res2) =>
                    {
                        JSONNode result = JSONNode.Parse((string)res2);
                        if ((int)result["code"] == 0)
                        {
                            isTestBallDown = !isTestBallDown;
                            isTestWiper = (int)result["coinPushTestWiper"] == 1;
                            isTestPlate = (int)result["coinPushTestPlate"] == 1;
                            CheckUI();
                        }
                    });

                });
            }

        }


        void DoPushPlateTest()
        {
            if (isTestPlate)
            {
                CloseAllTestCheckUI();
            }
            else
            {
                CloseAllTestCheckUI(() =>
                {
                    PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(3, (res2) =>
                    {
                        JSONNode result = JSONNode.Parse((string)res2);
                        if ((int)result["code"] == 0)
                        {
                            isTestWiper = (int)result["coinPushTestWiper"] == 1;
                            isTestPlate = (int)result["coinPushTestPlate"] == 1;
                            CheckUI();
                        }

                    });
                });
            }
        }


        void DoWiperTest()
        {
            if (isTestWiper)
                CloseAllTestCheckUI();
            else
            {
                CloseAllTestCheckUI(() =>
                {
                    PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(4, (res2) =>
                    {
                        JSONNode result = JSONNode.Parse((string)res2);
                        if ((int)result["code"] == 0)
                        {
                            isTestWiper = (int)result["coinPushTestWiper"] == 1;
                            isTestPlate = (int)result["coinPushTestPlate"] == 1;
                            CheckUI();
                        }
                    });
                });
            }


        }



        void DoRegainCoinsTest()
        {
            if (isTestRegainCoins)
            {
                CloseAllTestCheckUI();
            }
            else
            {
                CloseAllTestCheckUI(() =>
                {
                    PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(6, (res2) =>
                    {
                        JSONNode result = JSONNode.Parse((string)res2);
                        if ((int)result["code"] == 0)
                        {
                            isTestRegainCoins = !isTestRegainCoins;
                            isTestWiper = (int)result["coinPushTestWiper"] == 1;
                            isTestPlate = (int)result["coinPushTestPlate"] == 1;
                            CheckUI();
                        }

                    });
                });
            }

        }


        void DoRegainBallsTest()
        {
            if (isTestRegainBalls)
            {
                CloseAllTestCheckUI();
            }
            else
            {
                CloseAllTestCheckUI(() =>
                {
                    PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(7, (res2) =>
                    {
                        JSONNode result = JSONNode.Parse((string)res2);
                        if ((int)result["code"] == 0)
                        {
                            isTestRegainBalls = !isTestRegainBalls;
                            isTestWiper = (int)result["coinPushTestWiper"] == 1;
                            isTestPlate = (int)result["coinPushTestPlate"] == 1;
                            CheckUI();
                        }

                    });
                });
            }

        }



    }

}
