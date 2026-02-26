using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SimpleJSON;
using SlotEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ContentModel = SlotEmperorsRein.ContentModel;
namespace slotEmperorsRein
{


    public class PageFreeBonusGame1 : PageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PageFreeBonusGame1";

        bool tipCoinIn = false;
        int credit;
        SlotBonusGamePanelController slotBonusGamePanelController = new SlotBonusGamePanelController();


        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            InitParam();
            ChangAnim(true);
            Times = 3;


        }
        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            base.OnClose(data);
        }


        Animator animator;
        List<GComponent> Bonuslist = new List<GComponent>();
        List<BonusWin> bonusWins = new List<BonusWin>();
        
        protected override void OnInit()
        {
            
            base.OnInit();


            int count = 5;
            Action callBack = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };


            if (UIPackage.GetByName("Panel01") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Panel01/FGUIs", (ab) =>
                {
                    UIPackage.AddPackage(ab);
                    GLoader anchorPanel = this.contentPane.GetChild("anchorDi").asCom.GetChild("anchorDi").asLoader;
                    anchorPanel.url = "ui://Panel01/PanelBonusGame1";
                    callBack();
                });

            }
            else
            {
                GLoader anchorPanel = this.contentPane.GetChild("anchorDi").asCom.GetChild("anchorDi").asLoader;
                anchorPanel.url = "ui://Panel01/PanelBonusGame1";
                callBack();
            }

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Bonus1 Controller.prefab",
                (GameObject clone) =>
                {

                    goBonus1Ctrl = GameObject.Instantiate(clone);
                    goBonus1Ctrl.transform.SetParent(null);

                    slotMachineHelper = goBonus1Ctrl.transform.Find("Slot Machine").GetComponent<SlotMachineHelper>();
                    fguiPoolHelper = goBonus1Ctrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                    mono = goBonus1Ctrl.transform.GetComponent<MonoHelper>();
                    callBack();
                });

            ResourceManager02.Instance.LoadAsset<TextAsset>(
                "Assets/GameRes/Games/Emperors Rein 200/ABs/Datas/slot_machine_config_bouns1.json",
                (TextAsset txt) =>
                {
                    configStr = txt.text;
                    callBack();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PopupZhuanChang/ZhuanChang.prefab",
                (GameObject clone) =>
                {
                    goClone = clone;
                    callBack();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
               "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/GoldBack.prefab",
               (GameObject clone) =>
               {
                    goGoldBack = clone;
                    callBack();
               });






        }

        string configStr;
        
        private int _times; // 私有字段
        public int Times
        {
            get { return _times; }
            set { _times = value; ChangTimes(); } // 给字段赋值，而非属性自身
        }

        GameObject goBonus1Ctrl, go, goClone;
        SlotMachineHelper slotMachineHelper;
        FguiPoolHelper fguiPoolHelper;
        MonoHelper mono;
        GComponent Anchor;
        GComponent anchorExpectation;
        GComponent ComGoldBack;
        GameObject goGoldBack;
        Dictionary<string, object> args;

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            if (inParams != null)
            {
                args = inParams.value as Dictionary<string, object>;
            }

            GComponent lodAnchor = this.contentPane.GetChild("anchorSpine").asCom;
            if (Anchor != lodAnchor)
            {
                go = GameObject.Instantiate(goClone);
                animator = go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                Anchor = lodAnchor;
                GameCommon.FguiUtils.AddWrapper(Anchor, go);
            }

            //Timers.inst.Add(20f, 1, back);
            GComponent goSlotMachine = this.contentPane.GetChild("slotMachine").asCom.GetChild("slotMachine").asCom;
            GComponent goFrame = this.contentPane.GetChild("slotMachine").asCom.GetChild("minGameFrame").asCom;
            slotMachineHelper.Init(configStr, goSlotMachine, goFrame, fguiPoolHelper);

            slotBonusGamePanelController.Init(this.contentPane.GetChild("anchorDi").asCom.GetChild("anchorDi").asLoader.component);
            ComGoldBack = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            anchorExpectation = this.contentPane.GetChild("anchorReel2Effect").asCom;
            ComGoldBack.visible = true;
        }

         void OnPanelInputEvent(EventData res)
         {
            if(res.name== "BonusGame1SpinButtonClick")
            {
                if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
                coGameAuto = mono.StartCoroutine(GameAuto(null, null));
            }

         }


        string gameState = GameState.Idle;
        string strDeckRowCol = "1,1,1,1,1#2,2,2,2,2#3,3,3,3,3";
        List<SymbolWin> winList;

        Coroutine coReelsTurn, coGameOne, coGameAuto;

        void OnGameReset()
        {
            slotMachineHelper.isStopImmediately = false;
            slotMachineHelper.CloseSlotCover();

            slotMachineHelper.SkipWinLine(true);

            slotMachineHelper.SelectWinEffectSetting("default");
            slotMachineHelper.SelectReelSetting("regular");
            //slotMachineHelper.SelectReelSetting("stop_immediately");
        }

        IEnumerator GameAuto(Action successCallback, Action<string> errorCallback)
        {
            //yield return new WaitForSeconds(15);

            bool isErr = false;
            Action<string> errFunc = (err) =>
            {
                isErr = true;
                errorCallback?.Invoke(err);
            };

            while (--Times >= 0)
            {
                yield return GameOnce(null, errFunc);

                if (isErr)
                    yield break;

                yield return new WaitForSeconds(1f);
            }


            if (successCallback != null)
                successCallback.Invoke();
        }

        private bool isChange = false;

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {

            OnGameReset();
            gameState = GameState.Spin;

            slotMachineHelper.BeginTurn();

            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";


            
            
            yield return RequestSlotSpinFromMock(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });

            

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak)
            {
                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            slotMachineHelper.BeginSpin();


            if (slotMachineHelper.isStopImmediately)
            {
                if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                coReelsTurn = mono.StartCoroutine(slotMachineHelper.TurnReelsOnce(strDeckRowCol, () =>
                {
                    isNext = true;
                }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true);
            }
            else
            {
                if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                coReelsTurn = mono.StartCoroutine(slotMachineHelper.TurnReelsNormal(strDeckRowCol, () =>
                {
                    isNext = true;
                }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true || slotMachineHelper.isStopImmediately == true);

                // 等待移动结束
                if (slotMachineHelper.isStopImmediately && isNext == false)
                {
                    if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                    coReelsTurn = mono.StartCoroutine(slotMachineHelper.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    isNext = false;
                    yield return new WaitUntil(() => isNext == true);
                }
            }


            long allWinCredit;
            long totalWinLineCredit = 0;
            if (ContentModel.Instance.bonusResult != null)
            {
                if (ContentModel.Instance.bonusResult.Count > 0)
                {

                    yield return BonusBegin();
                    //totalWinLineCredit = slotMachineHelper.GetTotalBonusCredit(bonusWins);
                    allWinCredit = totalWinLineCredit;
                    yield return ShowSymbolBonusEffect(bonusWins);
                }

                slotMachineHelper.SendTotalWinCreditEvent(totalWinLineCredit);

                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, true);


            }


            // 进入空闲模式
            gameState = GameState.Idle;
                slotMachineHelper.SelectWinEffectSetting("idle");

                // 空闲轮播代码

                if (successCallback != null)
                    successCallback.Invoke();
            
        }


        IEnumerator ShowWinListOnceAtNormalSpin009(List<SymbolWin> winList)
        {

            if (slotMachineHelper.IsWETotalWinLine)
            {
                yield return slotMachineHelper.ShowSymbolWinBySetting(
                    slotMachineHelper.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
            }
            else
            {
                //停止特效显示
                slotMachineHelper.SkipWinLine(false);

                int idx = 0;
                while (idx < winList.Count)
                {
                    SymbolWin curSymbolWin = winList[idx];

                    yield return slotMachineHelper.ShowSymbolWinBySetting(curSymbolWin, true,
                       PusherEmperorsRein.SpinWinEvent.SingleWinLine);
                    ++idx;
                }
            }

            //停止特效显示
            slotMachineHelper.SkipWinLine(false);

            slotMachineHelper.CloseSlotCover();
        }

        IEnumerator BonusBegin()
        {
            if (ContentModel.Instance.bonusResult.ContainsKey(211))
            {
                bonusWins = new List<BonusWin>();

                JSONNode nodBonus = ContentModel.Instance.bonusResult[211];

                foreach (JSONNode item in nodBonus["data"])
                {

                    BonusWin bw = new BonusWin
                    {
                        earnCredit = item["credit"],
                        cell = new Cell((item["pos_xy"] / 10) - 1, (item["pos_xy"] % 10) - 1),
                    };
                    bonusWins.Add(bw);
                    Bonuslist.Add(UIPackage.CreateObject("Common", "AnchorRootDefault").asCom);

                    anchorExpectation.AddChild(Bonuslist[Bonuslist.Count - 1]);
                    Bonuslist[Bonuslist.Count - 1].xy =
                        slotMachineHelper.SymbolCenterToNodeLocalPos((item["pos_xy"] / 10) - 1, (item["pos_xy"] % 10) - 1,
                            anchorExpectation);
                    Bonuslist[Bonuslist.Count - 1].visible = true;
                    GameCommon.FguiUtils.AddWrapper(Bonuslist[Bonuslist.Count - 1], GameObject.Instantiate(goGoldBack));
                }

                yield return null;
               // yield return slotMachineHelper.ShowSymbolBonusBySetting(bonusWins, true);
            }
            //return bounsSroce;
        }

        public IEnumerator ShowSymbolBonusEffect(List<BonusWin> bonusWins)
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < Bonuslist.Count; i++)
            {
                Bonuslist[i]
                    .TweenMove(
                        slotMachineHelper.SymbolCenterToNodeLocalPos(2, 2, anchorExpectation) + new Vector2(0, 212),
                        0.6f);
                yield return new WaitForSeconds(1f);
                Bonuslist[i].visible = false;
                // MainBlackboardController.Instance.AddMyTempCredit(bonusWins[i].earnCredit, true, isAddCreditAnim);
                slotMachineHelper.SendTotalBonusCreditEvent(bonusWins[i].earnCredit);

            }
        }



        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;



            // 有好酷优先用好酷
            if (false && SBoxModel.Instance.isUseIot && tipCoinIn)
            {
                /*
                tipCoinIn = false;

                if (!DeviceIOTPayment.Instance.isIOTConneted)
                {
                    TipPopupHandler.Instance.OpenPopupOnce(string.Format(I18nMgr.T("IOT connection failed [{0}]"), Code.DEVICE_IOT_MQTT_NOT_CONNECT));
                }
                else if (!DeviceIOTPayment.Instance.isIOTSignInGetQRCode)
                {
                    TipPopupHandler.Instance.OpenPopupOnce(string.Format(I18nMgr.T("IOT connection failed [{0}]"), Code.DEVICE_IOT_NOT_SIGN_IN));
                }
                else
                {
                    DeviceIOTPayment.Instance.DoQrCoinIn();
                }
                return;
                */
            }
            else
            {
                string massage = I18nMgr.T(msg);
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(msg));
            }

        }
        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
            coGameAuto = mono.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        public void back(object param)
        {
            // 播放back过渡动画
            this.contentPane.GetTransition("back").Play();

            // 使用FGUI的Timers添加定时任务
            Timers.inst.Add(0.25f, 1, OnTimerComplete);
        }

        private void OnTimerComplete(object param)
        {
            ChangAnim(false);
        }


        public void ChangAnim(bool isgo)
        {
            bool isCommon = args.ContainsKey("isCommon") ? (bool)args["isCommon"] : false;
            if (isgo)
            {
                if (isCommon)
                {
                    animator.Play("animation1");
                }
                else
                {
                    animator.Play("animation2");
                }
            }
            else
            {
                if (isCommon)
                {
                    animator.Play("animation4");
                }
                else
                {
                    animator.Play("animation3");
                }
            }

        }

        public void ChangTimes()
        {
            if (_times == -1)
                return;
            this.contentPane.GetChild("slotMachine").asCom.GetChild("minGameFrame").asCom.GetController("c1").selectedIndex = _times;
                
        }

        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            long totalBet = ContentModel.Instance.totalBet;

            MockDataG300Controller.Instance.RequestBonusSlotSpin(totalBet,
                (JSONNode res) =>
                {

                    //解析数据
                    MockDataG300Controller.Instance.ParseSlotSpin(totalBet, res);


                    // 数据入库


                    // 彩金数据
                    MachineDataManager02.Instance.RequestJackpotGame((res) =>
                    {

                        JackpotRes info = res as JackpotRes;

                        ContentModel.Instance.jpGameRes = info;

                        float showJackpotGrand = info.curJackpotGrand;
                        float showJackpotMega = info.curJackpotMega;
                        float showJackpotMajor = info.curJackpotMajor;
                        float showJackpotMinior = info.curJackpotMinior;
                        float showJackpotMini = info.curJackpotMini;
                        foreach (var item in info.jpWinLst)
                        {
                            if (item.id == 0)
                            {
                                showJackpotGrand = item.whenCredit;
                            }
                            else if (item.id == 1)
                            {
                                showJackpotMega = item.whenCredit;
                            }
                            else if (item.id == 2)
                            {
                                showJackpotMajor = item.whenCredit;
                            }
                            else if (item.id == 3)
                            {
                                showJackpotMinior = item.whenCredit;
                            }
                            else if (item.id == 4)
                            {
                                showJackpotMini = item.whenCredit;
                            }
                        }

                        //ContentModel.Instance.uiGrandJP.nowCredit = uiJPGrangCtrl.nowData;
                        //ContentModel.Instance.uiMegaJP.nowCredit = uiJPMegaCtrl.nowData;
                        //ContentModel.Instance.uiMajorJP.nowCredit = uiJPMajorCtrl.nowData;
                        //ContentModel.Instance.uiMinorJP.nowCredit = uiJPMinorCtrl.nowData;
                        //ContentModel.Instance.uiMiniJP.nowCredit = uiJPMiniCtrl.nowData;

                        //ContentModel.Instance.uiGrandJP.curCredit = info.curJackpotGrand;
                        //ContentModel.Instance.uiMegaJP.curCredit = info.curJackpotMega;
                        //ContentModel.Instance.uiMajorJP.curCredit = info.curJackpotMajor;
                        //ContentModel.Instance.uiMinorJP.curCredit = info.curJackpotMinior;
                        //ContentModel.Instance.uiMiniJP.curCredit = info.curJackpotMini;

                        //uiJPGrangCtrl.SetData(showJackpotGrand);
                        //uiJPMegaCtrl.SetData(showJackpotMega);
                        //uiJPMajorCtrl.SetData(showJackpotMajor);
                        //uiJPMinorCtrl.SetData(showJackpotMinior);
                        //uiJPMiniCtrl.SetData(showJackpotMini);





                        isNext = true;
                        if (successCallback != null)
                            successCallback.Invoke();
                    }, (err) =>
                    {
                        isNext = true;

                        if (errorCallback != null)
                            errorCallback.Invoke(err.msg);
                    });

                },
                (BagelCodeError err) =>
                {

                    isNext = true;

                    if (errorCallback != null)
                        errorCallback.Invoke(err.msg);
                });




            yield return new WaitUntil(() => isNext == true);
            isNext = false;
        }

    }


   

}