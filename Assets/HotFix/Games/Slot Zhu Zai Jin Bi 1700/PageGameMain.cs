using FairyGUI;
using GameMaker;
using GlobalJackpotConsole;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
namespace SlotZhuZaiJinBi1700
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;//游戏 ID

        [JsonProperty("game_name")] public string GameName;//名称

        [JsonProperty("display_name")] public string DisplayName;//显示名称

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }//赢钱倍数

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }//符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付线
    }
    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "SlotZhuZaiJinBi1700";
        public new const string resName = "PageGameMain";

        private bool isInit = false;        //是否初始化
        private bool isInitPool = false; //资源池是否初始化
        private bool tipCoinIn = false; //提示硬币输入
        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        Coroutine corReelsTurn,corGameIdel, corGameOnce, corEffectSlowMotion, coGameAuto;
        //加速框
        bool isEffectSlowMotion2 = false;
        bool isEffectSlowMotion3 = false;
        bool isEffectSlowMotion4 = false;
        EventData _data = null;
        //游戏控制
        private GameObject goGameCtrl;
        private SlotMachineController1700 slotMachineCtrl;
        private MonoHelper mono;
        FguiPoolHelper fguiPoolHelper;
        FguiGObjectPoolHelper gObjectPoolHelper;
        PayTableController payTableController = new PayTableController(); //说明书赔率配置控制
        //组件
        GComponent gSlotCover, gPlayLines, gFrame; //滚轴组件
        private GComponent gOwnerPanel;//菜单
        private GComponent gNormalGameFrame, gFreeGameFrame; //外框
        private GComponent gNormalInnerFrame, gFreeInnerFrame; //内框
        private GComponent gNormalBg, gFreeBg; //背景
        //过度动画
        private GComponent anchorNormalFrame, anchorFreeFrame;
        private GameObject goNormalFrame, goFreeFrame;
        private GameObject CLonegoNormalFrame, ClonegoFreeFrame;
        private Animator animatorNormalFrame;
        private SkeletonMecanim SMNormalFrame;
        //免费组件
        private GComponent gFreeTimeBox, gFreeWinBox;
        private GComponent gFreeSlotMachine;
        //彩金
        //MiniReelGroup uiJPGrandCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();
        long TotalBet => (long)SBoxModel.Instance.CoinInScale;

        protected override void OnInit()
        {

            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 4;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Game Controller/Slot Game Main ControllerClone.prefab",
            (GameObject clone) =>
            {
                if (goGameCtrl != null) //防止重复加载
                {
                    return;
                }
                goGameCtrl=GameObject.Instantiate(clone);
                goGameCtrl.name = "Slot Game Main Controller1700";
                goGameCtrl.transform.SetParent(null);
                //获取组件引用
                slotMachineCtrl=goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController1700>();
                mono=goGameCtrl.transform.GetComponent<MonoHelper>();
                
                Debug.LogWarning("i am Game Controller");

                fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                callback();
            });

            ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            {
                UIPackage.AddPackage(ab);
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/GameMain/NormalFrame.prefab",
             (GameObject clone) =>
             {
                 goNormalFrame = clone;
                 callback();
             });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/GameMain/FreeFrame.prefab",
            (GameObject clone) =>
            {
                goFreeFrame = clone;
                callback();
            });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelController02.isOpenIntroduce == true)
                        {
                            return;
                        }

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnClickSpinButton(res);
                    },
                },

                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true); // isLongClick
                        OnClickSpinButton(res);
                    }
                }

            };

 
        }
        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }
        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            InitParam(data);
        }
        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            GameSoundHelper.Instance.StopMusic();
            base.OnClose(data);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;

            //同步积分和押注
            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {
                SBoxAccount data = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {

                        MainBlackboardController.Instance.SetMyRealCredit(playerAccountList[i].Credit);
                        //DebugUtils.Log("前一局算法卡CoinIn==" + playerAccountList[i].CoinIn);
                       // DebugUtils.Log("前一局算法卡Bet==" + playerAccountList[i].Bets);
                       // DebugUtils.Log("前一局算法卡Credit==" + );
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {

                DebugUtils.Log(err.msg);
            });
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);


            //说明书
            MainModel.Instance.contentMD = ContentModel.Instance;
            List<GComponent> lstPayTable = new List<GComponent>();
            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                lstPayTable.Add(paytable);
            }
            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);
            //读取Json配置
            ReadJsonBet();
            // UI 组件获取和老虎机初始化
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            gSlotCover = gSlotMachine.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);
            //背景
            gNormalBg = contentPane.GetChild("normalBG").asCom;
            gFreeBg = contentPane.GetChild("freeBG").asCom;
            //外框
            gNormalGameFrame = contentPane.GetChild("normalGameframe").asCom;
            gFreeGameFrame = contentPane.GetChild("freeGameFrame").asCom;
            //内框
            gNormalInnerFrame = contentPane.GetChild("normalInnerFrame").asCom;
            gFreeInnerFrame = contentPane.GetChild("freeInnerFrame").asCom;

            gFreeBg.visible = false;
            gFreeGameFrame.visible = false;
            gFreeInnerFrame.visible = false;
            //内外框过渡
            GComponent LocalNormalFrame = this.contentPane.GetChild("anchorNormalFrame").asCom;
            if (anchorNormalFrame != LocalNormalFrame)
            { 
                GameCommon.FguiUtils.DeleteWrapper(anchorNormalFrame);
                CLonegoNormalFrame = GameObject.Instantiate(goNormalFrame);
                animatorNormalFrame= CLonegoNormalFrame.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                SMNormalFrame = CLonegoNormalFrame.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorNormalFrame = LocalNormalFrame;
                GameCommon.FguiUtils.AddWrapper(anchorNormalFrame, CLonegoNormalFrame);
             
            }

            GComponent LocalFreeFrame = this.contentPane.GetChild("anchorFreeFrame").asCom;
            if (anchorFreeFrame != LocalFreeFrame)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorFreeFrame);
                ClonegoFreeFrame = GameObject.Instantiate(goFreeFrame);
                anchorFreeFrame = LocalFreeFrame;
                GameCommon.FguiUtils.AddWrapper(anchorFreeFrame, ClonegoFreeFrame);
               
            }
            anchorNormalFrame.visible = false;
            anchorFreeFrame.visible = false;
            SMNormalFrame.Skeleton.SetColor(new Color(1, 1, 1, 0));

            //免费场景
            gFreeTimeBox = contentPane.GetChild("freeTimeBox").asCom;
            gFreeWinBox = contentPane.GetChild("freeWinBox").asCom;
            gFreeSlotMachine= contentPane.GetChild("freeSlotMachine").asCom;
            gFreeTimeBox.visible = false;
            gFreeWinBox.visible = false;
            gFreeSlotMachine.visible = false;


            //对象池初始化
            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;
                //中奖动画
                fguiPoolHelper.Add(TagPoolObject.SymbolHit,CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);
                //边框
                fguiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect,"border#", 5);
                //落下图标动画
                fguiPoolHelper.Add(TagPoolObject.SymbolAppear,CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);

                //fguiPoolHelper.Init(CustomModel.Instance.symbolHitEffect,CustomModel.Instance.symbolAppearEffect, null,CustomModel.Instance.borderEffect);
            }

            //初始化菜单ui
            gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            // 事件放出
            //goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));


            //彩金
            //uiJPGrangCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            //uiJPGrangCtrl.SetData(50000);
            uiJPMajorCtrl.SetData(30000);
            uiJPMinorCtrl.SetData(1000);
            uiJPMiniCtrl.SetData(500);

            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];
        }

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            /*检查机器是否激活
            检查玩家余额是否足够支付当前投注
            如果条件不满足，调用错误回调并终止协程
            */
            if (!SBoxModel.Instance.isMachineActive)
            {
                errorCallback?.Invoke("<size=24>Machine not activated!</size>");
                yield break;
            }

            if (SBoxModel.Instance.myCredit < ContentModel.Instance.totalBet)
            {
                tipCoinIn = true;
                errorCallback?.Invoke("<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            //test 检查算法积分
            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {

                SBoxAccount data = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {
                        DebugUtils.Log("前一局算法卡CoinIn==" + playerAccountList[i].CoinIn);
                        DebugUtils.Log("前一局算法卡Bet==" + playerAccountList[i].Bets);
                        DebugUtils.Log("前一局算法卡Credit==" + playerAccountList[i].Credit);
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {

                DebugUtils.Log(err.msg);
            });


            // 游戏状态重置和旋转请求
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            slotMachineCtrl.BeginTurn();
            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            //模拟结果
            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }
            else
            {
                yield return RequestSlotSpinFromMachine(() =>
                {
                    isNext = true;
                },(err)=>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }

            yield return new WaitUntil(()=> isNext == true);
            isNext = false;

            //请求结果失败
            if (isBreak)
            {
                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            //检查是否启用在线彩金
            //根据运行环境（模拟或实际）请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine)
            {
                if (ApplicationSettings.Instance.isMock)
                {
                    // 模拟在线彩金中奖数据
                    MachineDataManager.Instance.RequestJackpotOnLine();
                }
                else
                {
                    /*
                    JackpotOnLineManager.Instance.RequestsJackpotOnLineData(
                        new JackBetInfo
                        {
                            seat = 1,  // 固定死
                            bet = (int)_contentBB.Instance.totalBet,  // 总压注
                            betPercent = 100, // 固定死
                            scoreRate =  _consoleBB.Instance.jackpotScoreRate,      //10000,  // 1 除以 币值 乘以 1000 整形   （联网彩金分值比 ：只能该币值）
                            JPPercent =  _consoleBB.Instance.jackpotPercent,    //5  // 千分之几（1 - 100 可调 ；名称： 联网彩金比（千分）  ）
                        },
                        null, null
                    );
                    */
                }
            }

            //开始滚动
            slotMachineCtrl.BeginSpin();
            //是否加速滚动
            if (ContentModel.Instance.isReelsSlowMotion)
            {
                //if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                //corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());
                //slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }

            // 立即停止或正常旋转
            if (slotMachineCtrl.isStopImmediately)
            {
                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    { 
                        isNext = true;
                    }));
                isNext = false;
                yield return new WaitUntil(() => isNext == true);
            }
            else
            {
                // 正常旋转模式
                Debug.Log(ContentModel.Instance.strDeckRowCol);
                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);

                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                    corReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));
                    isNext = false;
                    yield return new WaitUntil(() => isNext == true);
                }
            }

            //线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;

            #region Win
            //普通赢
            if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
            {
                //中奖特效
                if (_spinWEMD.Instance.isSingleWin)
                {
                    //mono.StartCoroutine(PlayKing(1f));
                }
                else
                {
                    //mono.StartCoroutine(PlayKing(2f));
                }

                long totalWinLineCredit = 0;
                totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit += totalWinLineCredit;
                if (winList.Count > 0)
                {
                    //yield return ShowWinListOnceAtNormalSpin(winList);
                }

                //检查bigwin类型
                WinLevelType winLevelType = GetBigWinType();
                //bigwi弹窗
                if (winLevelType != WinLevelType.None)
                {
                    //显示全部中奖图标和中奖线
                   // slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                    //bigwin弹窗
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.CloseSlotCover();
                    slotMachineCtrl.SkipWinLine(false);
                }
                else
                {
                    // 普通赢钱处理
                    bool isAddToCredit = totalWinLineCredit > ContentModel.Instance.totalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }

                //积分同步和退币处理
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);

                // 本剧同步玩家金钱
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }
            #endregion

            //免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                //显示中奖动画
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                //过度动画
                anchorNormalFrame.visible = true;
                //过度淡入，持续1秒
                GTween.To(0, 1, 1)
                .SetEase(EaseType.Linear) // 线性过渡，匀速增长
                .OnUpdate((tween) =>
                {
                    float progress = tween.value.x;
                    SMNormalFrame.Skeleton.SetColor(new Color(1, 1, 1, progress));
                })
                .OnComplete(() =>
                {
                    slotMachineCtrl.SkipWinLine(false);
                    animatorNormalFrame.Play("open");
                    //切换背景和边框
                    ChangeBGPanel(0);
                });
                yield return slotMachineCtrl.SlotWaitForSeconds(2.0f);
                yield return FreeSpinTrigger(null, errorCallback);
            }


            //核对前后端积分
            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {

                JSONNode data = JSONObject.Parse((string)res1);

                int code = (int)data["code"];
                int credit = (int)data["credit"];

                if (code != 0)
                {
                    DebugUtils.LogError($" CoinPushSpinEnd(20102) : [0]= {code}");
                }
                else
                {

                    DebugUtils.Log("算法卡积分==" + credit);
                    DebugUtils.Log("机器积分==" + SBoxModel.Instance.myCredit);
                    if (credit != SBoxModel.Instance.myCredit)
                    {

                    }
                    isNext = true;
                }

            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);


            // 即中即退
            // yield return CoinOutImmediately(allWinCredit);
            // 进入空闲模式
            ContentModel.Instance.gameState = GameState.Idle;
            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
                corGameIdel = mono.StartCoroutine(GameIdle(winList));
            }

            if (successCallback != null)
                successCallback.Invoke();
        }
        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";
        //请求模拟结果
        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            
            //请求结果
            MachineDataG1700Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
            {
                resNode = res;
                isNext = true;
            },(err)=>
            {
                errorCallback?.Invoke(err.msg);
                isNext = true;
                isBreak = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            if (isBreak) yield break;

            SBoxJackpotData sboxJackpotData = null;

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            }

            // 解析数据
            MachineDataG1700Controller.Instance.ParseSlotSpinMock(totalBet, resNode, sboxJackpotData);
           
            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }
        //请求算法结果
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            long totalBet = TotalBet;
            bool isBreak = false;
            bool isNext = false;
            bool isGetMyCredit = false;

            JSONNode resNode = null;
            int myCredit = -1;

            //请求算法结果
            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                Debug.Log("请求算法结果");
                resNode = JSONNode.Parse((string)res);
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //初始化彩金数据
            SBoxJackpotData sboxJackpotData =new SBoxJackpotData();
            sboxJackpotData.Lottery = new int[3];
            sboxJackpotData.JackpotOut = new int[3];
            sboxJackpotData.Jackpotlottery = new int[3];
            sboxJackpotData.JackpotOld = new int[3];
            //获取彩金贡献值
            ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
            {
                Debug.Log("请求本地彩金贡献值");
                JSONNode data = JSONNode.Parse((string)res);
                Debug.Log(data);
                int code = (int)data["code"];

                if (0 != code)
                {
                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                    isNext = true;
                    return;
                }

                int majorBet = (int)data["major"];
                int minorBet = (int)data["minor"];
                int miniBet =   (int)data["mini"];

                Debug.Log("majorBet:" + majorBet);
                Debug.Log("minorBet:" + minorBet);
                Debug.Log("miniBet:" + miniBet);

                sboxJackpotData.Lottery[0] = 0;
                sboxJackpotData.Lottery[1] = 0;
                sboxJackpotData.Lottery[2] = 0;

                sboxJackpotData.JackpotOut[0] = majorBet;
                sboxJackpotData.JackpotOut[1] = minorBet;
                sboxJackpotData.JackpotOut[2] = miniBet;

                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            }
         
            // 解析数据
            MachineDataG1700Controller.Instance.ParseSlotSpinMachine(totalBet, resNode, sboxJackpotData);
            // 数据入库
            //MachineDataG200Controller.Instance.TestRecord();
            // ui 彩金
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }
        //显示线和中奖图标
        IEnumerator ShowWinListOnceAtNormalSpin(List<SymbolWin> winList)
        {
            //总线
            if (_spinWEMD.Instance.isTotalWin)
            {
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, SpinWinEvent.TotalWinLine);
            }
            else
            {
                //单线
                slotMachineCtrl.SkipWinLine(false);
                int idx = 0;
                while (idx<winList.Count)
                {
                    SymbolWin curSymvolWin = winList[idx];
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(curSymvolWin, true, SpinWinEvent.SingleWinLine);
                    ++idx;
                }

                //停止特效显示
                slotMachineCtrl.SkipWinLine(false);
                slotMachineCtrl.CloseSlotCover();
            }
        }
        //游戏状态重置
        private void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            //mono.StopCoroutine(corEffectSlowMotion);
            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            slotMachineCtrl.SkipWinLine(true);
        }
        //游戏状态闲置
        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);

            //yield return new WaitForSeconds(3f);

            yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }
        //bigwin弹窗
        IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit)
        {
            bool isNext = false;
            PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPopupBigWin,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                {
                    ["baseGameWinCredit"] = winCredit, //ContentModel.Instance.baseGameWinCredit,
                    ["WinType"] = winLevelType,
                }),
                (res) =>
                {
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
        }
        //免费弹窗
        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.SlotZhuZaiJinBiPopupFreeSpinTrigger,
              new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        //["autoCloseTimeS"] = 3f,
                        ["freeSpinCount"] = ContentModel.Instance.freeSpinTotalTimes,
                    }),
                (ed) =>
                {
                    Debug.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });
           
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return GameFreeSpin(null, errorCallback);
        }

        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {

                yield return GameFreeSpinOnce(null, errorCallback);
                yield return slotMachineCtrl.SlotWaitForSeconds(1);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }
        //一局免费游戏
        IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
            ContentModel.Instance.gameState = GameState.FreeSpin;

            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            //获取结果
            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }
            else
            {
                yield return RequestSlotSpinFromMachine(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            if (isBreak)
            {

                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            //开始转动
            slotMachineCtrl.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion)
            {
                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());

                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }

            if (slotMachineCtrl.isStopImmediately)
            {
                //reelsTurnType = ReelsTurnType.Once;

                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

            }
            else
            {
                //reelsTurnType = ReelsTurnType.Normal;
                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                    corReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;


            #region Win

            if (winList.Count > 0)
            {
                long totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                if (winList.Count > 0)
                {
                    yield return ShowWinListOnceAtNormalSpin(winList);
                }

                // 播大奖弹窗
                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);

                    // 大奖弹窗
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.CloseSlotCover();

                    slotMachineCtrl.SkipWinLine(false);
                }
                else
                {

                    // 总线赢分（同步？？）
                    bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }



                // 总线赢分事件
                slotMachineCtrl.SendTotalWinCreditEvent(totalWinLineCredit);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
                ContentModel.Instance.freeOnceCredit = totalWinLineCredit;


            }

            #endregion


            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(false);

            // 本局掉币
            //ERPushMachineDataManager.Instance.RequestCoinPushSpinEnd(res1 =>
            //{
            //    isNext = true;
            //});

            //yield return new WaitUntil(() => isNext == true);
            //isNext = false;

            //if (winList.Count > 0 || isHitJackpot)
            //{
            //    //yield return ShowWinListCoinCountDown(winList, allWinCredit, isHitJackpot);
            //}

            // 即中即退
            //yield return CoinOutImmediately(allWinCredit);


            #region 免费游戏中，添加额外免费游戏

            if (ContentModel.Instance.isFreeSpinAdd)
            {

                slotMachineCtrl.BeginBonusFreeSpinAdd();
                PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupFreeSpinTrigger,
                    new EventData<Dictionary<string, object>>("",
                        new Dictionary<string, object>()
                        {
                            ["freeSpinCount"] = ContentModel.Instance.freeSpinAddNum,
                            ["isAddFreeGame"] = true,
                        }),
                    (ed) =>
                    {
                        isNext = true;
                    });


                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 【待修改】重置剩余的局数 
                ContentModel.Instance.showFreeSpinRemainTime =
                    ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes;


                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.EndBonusFreeSpinAdd();

            }

            #endregion


            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }
        //bigwin类型
        WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = ContentModel.Instance.winLevelMultiple;
            long totalBet=ContentModel.Instance.totalBet;
            WinLevelType winLevelType = WinLevelType.None;
            for (int i = 0; i < winMultipleList.Count; i++)
            {
                if (baseGameWinCredit > totalBet * winMultipleList[i].multiple)
                {
                    winLevelType = winMultipleList[i].winLevelType;
                }
            }

            return winLevelType;
        }
        //读取游戏配置
        private void ReadJsonBet()
        {
            //资源加载
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                "Assets/GameRes/_Common/Game Maker/ABs/G1700/Data/game_info_g1700.json", (txt) =>
                {
                    //JSON解析与错误处理
                    GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(txt.text);
                    if (config?.SymbolPaytable == null)
                    {
                        Debug.LogError("解析symbol_paytable失败，数据为空");
                        return;
                    }

                    MainModel.Instance.gameID = config.GameId;
                    MainModel.Instance.gameName = config.GameName;
                    MainModel.Instance.displayName = config.DisplayName;

                    //赢钱倍数处理
                    foreach (var item in config.WinLevelMultiple)
                    {
                        string winKey = item.Key;
                        long winValue = item.Value;
                        MainModel.Instance.contentMD.winLevelMultiple.Add(new WinMultiple(winKey, winValue));
                    }

                    //符号支付表处理
                    foreach (var kvp in config.SymbolPaytable)
                    {
                        string symbolKey = kvp.Key; // 如 "s0"、"s1"、"s2"
                        var jsonData1 = kvp.Value; // 对应x3、x4、x5的数据

                        // 1. 从symbolKey中提取索引（如"s0" → 0，"s1" → 1）
                        if (int.TryParse(symbolKey.Replace("s", ""), out int index))
                        {
                            // 2. 检查索引是否在列表有效范围内
                            if (index >= 0)
                            {
                                // 3. 为列表中对应索引的元素赋值
                                var targetItem = MainModel.Instance.contentMD.payTableSymbolWin[index];
                                targetItem.x3 = jsonData1.x3; // 假设jsonData的属性是X3（根据实际定义调整）
                                targetItem.x4 = jsonData1.x4;
                                targetItem.x5 = jsonData1.x5;
                                // 若需要同步symbol字段（可选，确保一致）
                                targetItem.symbol = index;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"无效的符号键：{symbolKey}，无法解析索引");
                        }
                    }

                    //支付线处理
                    if (ContentModel.Instance.payLines == null)
                    {
                        ContentModel.Instance.payLines = new List<List<int>>() { };
                    }
                    foreach (var item in config.pay_lines)
                    {
                        ContentModel.Instance.payLines.Add(item);
                    }
                });
        }
        private void OnStopSlot(EventData res)
        {

        }
        //机器按钮开始滚动
        private void OnClickSpinButton(EventData res)
        {
          
            bool isLongClick = (bool)res.value;
            switch (ContentModel.Instance.btnSpinState)
            {
                case SpinButtonState.Stop:
                    if (ContentModel.Instance.isSpin) return; //已经开始玩直接退出？
                    ContentModel.Instance.isSpin = true;

                    Action successCallback = () =>
                    {
                        ContentModel.Instance.isSpin = false;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                        ContentModel.Instance.gameState = GameState.Idle;
                        DebugUtils.Log("游戏结束");
                    };

                    if (isLongClick)
                    {
                        Debug.Log("机器按钮开始滚动 :Long");
                        ContentModel.Instance.isAuto = true;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        StartGameAuto(successCallback, StopGameWhenError); //自动玩
                    }
                    else
                    {
                        Debug.Log("机器按钮开始滚动:Short");
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        StartGameOnce(successCallback, StopGameWhenError); //开始玩
                    }
                    break;
                case SpinButtonState.Spin:
                    // 已经在游戏时，去停止游戏
                    if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出
                    slotMachineCtrl.isStopImmediately = true; // 去停止游戏  
                    break;
                case SpinButtonState.Auto:
                    //停止自动玩
                    //停止自动玩
                    ContentModel.Instance.isSpin = true;
                    ContentModel.Instance.isAuto = false;
                    ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                    break;
            }
        }
        //开始游戏
        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }
        //开始自动玩
        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
            coGameAuto = mono.StartCoroutine(GameAuto(successCallback, errorCallback));
        }
        IEnumerator GameAuto(Action successCallback, Action<string> errorCallback)
        {
            bool isErr = false;
            Action<string> errFunc = (err) =>
            {
                isErr = true;
                errorCallback?.Invoke(err);
            };

            while (ContentModel.Instance.isAuto && !ContentModel.Instance.isRequestToStop)
            {
                yield return GameOnce(null, errFunc);

                if (isErr)
                    yield break;

                /*
                float time = Time.time;
                while (Time.time - time < 1f)
                {
                    yield return new WaitForSeconds(0.1f);
                    if (!ContentModel.Instance.isAuto)
                        break;
                }*/

                yield return new WaitForSeconds(0.1f);

                if (!ContentModel.Instance.isAuto)
                    break;
            }

            if (ContentModel.Instance.isRequestToStop)
            {
                ContentModel.Instance.isRequestToStop = false;
                ContentModel.Instance.isAuto = false;
            }

            if (successCallback != null)
                successCallback.Invoke();
        }
        private void ChangeBGPanel(int type )
        {
            if (type == 0)
            {
                gFreeBg.visible = false;
                gFreeGameFrame.visible = false;
                gFreeInnerFrame.visible = false;
                gNormalBg.visible = true;
                gNormalGameFrame.visible = true;
                gNormalInnerFrame.visible = true;
            }
            else
            {
                gNormalBg.visible = false;
                gNormalGameFrame.visible = false;
                gNormalInnerFrame.visible = false;
                gFreeBg.visible = true;
                gFreeGameFrame.visible = true;
                gFreeInnerFrame.visible = true;
            }
        }

        //显示加速框
        public IEnumerator ShowEffectReelsSlowMotion()
        {
            yield return new WaitUntil(() => isEffectSlowMotion2 == true);
        }
        //错误提示
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
                {}
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
        bool isGetCrediting = false;
        void GetMyCredit(Action<int> onSuccessCallback, Action<string> onErrorCallback)
        {
            //GetMyCreditSuccessQueque.Enqueue(onSuccessCallback);
            //GetMyCreditFailQueque.Enqueue(onErrorCallback);
            // onGetMyCreditSuccessCallback = onSuccessCallback;
            // onGetMyCreditErrorCallback = onErrorCallback;
            //if (isGetCrediting == true) return;

            isGetCrediting = true;

            ERPushMachineDataManager02.Instance.RequestGetMyCredit((res) =>
            {
                isGetCrediting = false;
                try
                {
                    int myCredit = (int)res;

                    /*while (GetMyCreditSuccessQueque.Count > 0)
                    {
                        Action<int>  func = GetMyCreditSuccessQueque.Dequeue();
                        func?.Invoke(myCredit);
                    }*/

                    //onGetMyCreditSuccessCallback?.Invoke(myCredit);

                    onSuccessCallback?.Invoke(myCredit);
                }
                catch (Exception ex)
                {
                    DebugUtils.LogError(ex);
                    DebugUtils.LogError(res);

                    /*while (GetMyCreditFailQueque.Count > 0)
                    {
                        Action<string> func = GetMyCreditFailQueque.Dequeue();
                        func?.Invoke(ex.Message);
                    }*/

                    //onGetMyCreditErrorCallback?.Invoke(ex.Message);

                    onErrorCallback?.Invoke(ex.Message);
                }

            });
        }

        public void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;

            //ContentModel.Instance.uiGrandJP.nowCredit = uiJPGrandCtrl.nowData;
            //ContentModel.Instance.uiMegaJP.nowCredit = uiJPMegaCtrl.nowData;
            ContentModel.Instance.uiMajorJP.nowCredit = uiJPMajorCtrl.nowData;
            ContentModel.Instance.uiMinorJP.nowCredit = uiJPMinorCtrl.nowData;
            ContentModel.Instance.uiMiniJP.nowCredit = uiJPMiniCtrl.nowData;

           // ContentModel.Instance.uiGrandJP.curCredit = info.curJackpotGrand;
            //ContentModel.Instance.uiMegaJP.curCredit = info.curJackpotMega;
            ContentModel.Instance.uiMajorJP.curCredit = info.curJackpotMajor;
            ContentModel.Instance.uiMinorJP.curCredit = info.curJackpotMinior;
            ContentModel.Instance.uiMiniJP.curCredit = info.curJackpotMini;

            // 游戏滚轮显示
            //uiJPGrandCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[0]);
            //uiJPMegaCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            //uiJPMajorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            //uiJPMinorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[2]);
            //uiJPMiniCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[3]);

            uiJPMajorCtrl.SetData(info.curJackpotMajor);
            uiJPMinorCtrl.SetData(info.curJackpotMinior);
            uiJPMiniCtrl.SetData(info.curJackpotMini);
        }
    }
}

