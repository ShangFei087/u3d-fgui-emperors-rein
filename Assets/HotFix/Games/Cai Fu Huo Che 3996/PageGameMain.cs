using FairyGUI;
using GameMaker;
using GlobalJackpotConsole;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using SlotZhuZaiJinBi1700;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace CaiFuHuoChe_3996
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;

        [JsonProperty("game_name")] public string GameName;

        [JsonProperty("display_name")] public string DisplayName;

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; }
    }


    public class PageGameMain : MachinePageBase //: PageBase
    {
        public const string pkgName = "CaiFuHuoChe_3996";
        public const string resName = "PageGameMain";


        private SlotMachineController3996 slotMachineCtrl;
        private GComponent slotCover, gOwnerPanel, gPlayLines, gFrame, gTrain, gFreeCloude;

        private GameObject goGameCtrl;

        PayTableController payTableController = new PayTableController();
        Coroutine corReelsTurn, corGameIdel, corGameOnce, corEffectSlowMotion, corRewardEffect;

        //游戏控制
        private MonoHelper mono;
        private FguiPoolHelper fguiPoolHelper;
        private FguiGObjectPoolHelper gObjectPoolHelper;

        private string JackpotType = "";
        private float winCredit = 0;

        long TotalBet => (long)SBoxModel.Instance.CoinInScale;

        private new bool isInit = false;        //是否初始化
        private bool isInitPool = false;
        private bool tipCoinIn = false; //提示硬币输入
        private bool isStoppedSlotMachine = false;

        //加速框
        private GComponent anchorExpectation, ComReelEffect2, ComReelEffect3;
        private GameObject goFreeReelEffcet, goJackpotReelEffect;

        //免费游戏以及彩金游戏中特殊奖时特效
        private GameObject goRewardEffect;
        private GComponent anchorFreeAdd, anchorFill1, anchorFill2, anchorFill3, anchorFill4, ComRewardEffect1, ComRewardEffect2, ComRewardEffect3;

        //正常游戏和彩金游戏之间转场火车开门时特效
        private GameObject goOpenEffect;
        private GComponent anchorOpenEffect;
        private Transform norToJp, jpToNor;

        //火车预制体、动画
        private GameObject train, goTrain, freeCloude, goFreeCloude;
        private Animator trainAnim;

        //免费游戏和正常游戏直接的动效
        private Transition BsToFsTrans, FsToBsTrans, JsToBsTrans;
        //免费游戏中充能绿条
        private GImage fill1, fill2, fill3, fill4;
        //免费游戏的剩余次数和总次数
        private GTextField freeTimes, freeTotalTimes;

        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        private EventData _data = null;

        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 8;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    count++;
                    callback();
                });
            }

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Game Controller/Slot Game Main ControllerClone.prefab",
            (GameObject clone) =>
            {
                if (goGameCtrl != null) //防止重复加载
                {
                    return;
                }
                goGameCtrl = GameObject.Instantiate(clone);
                goGameCtrl.name = "Slot Game Main Controller3996";
                goGameCtrl.transform.SetParent(null);
                //获取组件引用
                slotMachineCtrl = goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController3996>();
                mono = goGameCtrl.transform.GetComponent<MonoHelper>();

                fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PageGameMain/Train.prefab",
            (GameObject clone) =>
            {
                 goTrain = clone;
                 callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameFree/FreeGameCloude.prefab",
            (GameObject clone) =>
            {
                goFreeCloude = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            {
                UIPackage.AddPackage(ab);
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Effects/FreeReelEffect.prefab",
            (GameObject clone) =>
            {
                goFreeReelEffcet = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Effects/JackpotReelEffect.prefab",
            (GameObject clone) =>
            {
                goJackpotReelEffect = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Effects/RewardEffect.prefab",
            (GameObject clone) =>
            {
                goRewardEffect = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PageGameMain/ngAndsgEffect.prefab",
            (GameObject clone) =>
            {
                goOpenEffect = clone;
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

            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            if (isOpen) return;
            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>("RewardAddEffect", OnRewardEffectEvent);
            EventCenter.Instance.AddEventListener<EventData>("JackpotWinCredit", OnJackpotWinEvent);

            PlayAnim(trainAnim, "fg_ng");
        }

        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>("RewardAddEffect", OnRewardEffectEvent);
            EventCenter.Instance.AddEventListener<EventData>("JackpotWinCredit", OnJackpotWinEvent);

            base.OnClose(data);
        }


        private void OnClickSpinButton(EventData res)
        {
            if (res.name != PanelEvent.SpinButtonClick) return;

            bool isLongClick = (bool)res.value;
            switch (ContentModel.Instance.btnSpinState)
            {
                case SpinButtonState.Stop:
                    {
                        if (ContentModel.Instance.isSpin) return; // 已经开始玩直接退出

                        ContentModel.Instance.isSpin = true;

                        Action successCallback = () =>
                        {
                            DebugUtils.Log("游戏结束");
                            ContentModel.Instance.isSpin = false;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                            ContentModel.Instance.gameState = GameState.Idle;
                        };

                        if (isLongClick)
                        {
                            TestManager.Instance.ShowTip("Spin按钮 - 长按");

                            ContentModel.Instance.isAuto = true;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;

                            StartGameAuto(successCallback, StopGameWhenError); //自动玩
                        }
                        else
                        {
                            TestManager.Instance.ShowTip("Spin按钮 - 短按");

                            //ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            //StartGameOnce(successCallback, StopGameWhenError);//开始玩

                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            StartGameOnce(successCallback, StopGameWhenError); //开始玩
                        }


                    }
                    break;

                case SpinButtonState.Spin:
                    {
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出

                        slotMachineCtrl.isStopImmediately = true; // 去停止游戏  

                        SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.StopImmediately);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        //停止自动玩
                        ContentModel.Instance.isSpin = true;
                        ContentModel.Instance.isAuto = false;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                    }
                    break;
            }

        }


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;

            //对象池初始化
            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;
                //中奖动画
                fguiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);
                //边框
                fguiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolBorder);
                //落下后图标静止动画
                fguiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);

                //fguiPoolHelper.Init(CustomModel.Instance.symbolHitEffect,CustomModel.Instance.symbolAppearEffect, null,CustomModel.Instance.borderEffect);
            }

            //初始化UI组件
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            slotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(slotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);

            if (ComReelEffect2 != null)
            {
                ComReelEffect2.Dispose();
            }

            if(ComReelEffect3 != null)
            {
                ComReelEffect3.Dispose();
            }

            //初始化加速框特效
            ComReelEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComReelEffect2);
            GameCommon.FguiUtils.AddWrapper(ComReelEffect2, GameObject.Instantiate(goFreeReelEffcet));
            ComReelEffect2.visible = false;

            ComReelEffect3 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComReelEffect3);
            GameCommon.FguiUtils.AddWrapper(ComReelEffect3, GameObject.Instantiate(goJackpotReelEffect));
            ComReelEffect3.visible = false;

            anchorExpectation = this.contentPane.GetChild("anchorReelEffect").asCom;
            anchorExpectation.AddChild(ComReelEffect2);
            anchorExpectation.AddChild(ComReelEffect3);
            anchorExpectation.visible = true;


            //初始化特殊中奖特效以及中奖时的锚点
            ComRewardEffect1 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            ComRewardEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            ComRewardEffect3 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect1);
            GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect2);
            GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect3);
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect1, GameObject.Instantiate(goRewardEffect));
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect2, GameObject.Instantiate(goRewardEffect));
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect3, GameObject.Instantiate(goRewardEffect));
            ComRewardEffect1.visible = false;
            anchorFreeAdd = contentPane.GetChild("freeAddPoint").asCom;
            anchorFill1 = contentPane.GetChild("fill1Add").asCom;
            anchorFill2 = contentPane.GetChild("fill2Add").asCom;
            anchorFill3 = contentPane.GetChild("fill3Add").asCom;
            anchorFill4 = contentPane.GetChild("fill4Add").asCom;
            anchorFreeAdd.AddChild(ComRewardEffect1);
            anchorFreeAdd.AddChild(ComRewardEffect2);
            anchorFreeAdd.AddChild(ComRewardEffect3);
            anchorFreeAdd.visible = true;

            GComponent loadTrain = contentPane.GetChild("anchorTrain").asCom;
            if(gTrain != loadTrain)
            {
                GameCommon.FguiUtils.DeleteWrapper(gTrain);
                gTrain = loadTrain;
                train = GameObject.Instantiate(goTrain);
                trainAnim = train.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gTrain, train);
            }

            GComponent loadOpenEffect = contentPane.GetChild("JpEffect").asCom;
            if (anchorOpenEffect != loadOpenEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorOpenEffect);
                anchorOpenEffect = loadOpenEffect;
                GameObject temp = GameObject.Instantiate(goOpenEffect);
                norToJp = temp.transform.GetChild(0).GetChild(0);
                jpToNor = temp.transform.GetChild(1).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(anchorOpenEffect, temp);
            }

            GComponent loadFreeCloude = contentPane.GetChild("anchorFreeCloude").asCom;
            if (gFreeCloude != loadFreeCloude)
            {
                GameCommon.FguiUtils.DeleteWrapper(gFreeCloude);
                gFreeCloude = loadFreeCloude;
                freeCloude = GameObject.Instantiate(goFreeCloude);
                GameCommon.FguiUtils.AddWrapper(gFreeCloude, freeCloude);
            }

            //说明书
            MainModel.Instance.contentMD = ContentModel.Instance;

            List<GComponent> lstPayTable = new List<GComponent>();

            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                paytable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(paytable);

                lstPayTable.Add(paytable);
                paytable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }

            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);
            ContentModel.Instance.payLines = new List<List<int>>();

            //读取json配置
            ReadJsonBet();

            // 玩家积分初始化
            SBoxModel.Instance.myCredit = 9900;
            foreach (SBoxPlayerScoreInfo item in SBoxIdea.SBoxInfo.PlayerScoreInfoList)
            {
                if (item.PlayerId == 1)
                {
                    SBoxModel.Instance.myCredit = item.Score;
                }
            }

            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            //初始化菜单ui
            gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            // 事件放出
            //goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            BsToFsTrans = contentPane.GetTransition("BSToFSTransform");
            FsToBsTrans = contentPane.GetTransition("FSToBSTransform");
            JsToBsTrans = contentPane.GetTransition("JSToBSTransform");
            fill1 = contentPane.GetChild("fill1").asImage;
            fill2 = contentPane.GetChild("fill2").asImage;
            fill3 = contentPane.GetChild("fill3").asImage;
            fill4 = contentPane.GetChild("fill4").asImage;
            freeTimes = contentPane.GetChild("freeRemainTimes").asTextField;
            freeTotalTimes = contentPane.GetChild("freeTotalTimes").asTextField;
        }


        private void ReadJsonBet()
        {
            //资源加载
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                "Assets/GameRes/_Common/Game Maker/ABs/G3996/Data/game_info_g3996.json", (txt) =>
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

        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            if (corGameOnce != null) mono.StopCoroutine(corGameOnce);
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            mono.StartCoroutine(GameAuto(successCallback, errorCallback));
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


        void OnStopSlot(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.StoppedSlotMachine:
                    {
                        isStoppedSlotMachine = true;
                    }
                    break;
            }
        }

        void OnSlotDetailEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (ContentModel.Instance.isReelsSlowMotion && !slotMachineCtrl.isStopImmediately)
                        {
                            int colIndex = (int)res.value;
                            if (colIndex == 1)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(1));
                            }
                            else if (colIndex == 2)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(2));
                            }
                            else if (colIndex == 3)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(3));
                            }
                            else if (colIndex == 4)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(4));
                            }
                        }
                    }
                    break;
            }

        }

        Dictionary<int, List<int>> tempResult;
        void OnRewardEffectEvent(EventData res)
        {
            tempResult = (Dictionary<int, List<int>>)res.value;
            int col = tempResult.Keys.First();

            //如果有需要跳过不播放中奖的线条特效可启用
            //if (slotMachineCtrl.isStopImmediately && res.name == "MultRewardEffect")
            //{
            //    SkipAddMult(tempResult[col].Count * 0.25f);
            //    return;
            //}
            
            switch (res.name)
            {
                case "FreeRewardEffect":
                    foreach(int row in tempResult[col])
                    {
                        mono.StartCoroutine(ShowRewardEffect(col, row, anchorFreeAdd));
                    }
                    break;
                case "MultRewardEffect":
                    foreach (int row in tempResult[col])
                    {
                        if(fill1.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill1));
                            fill1.fillAmount += 0.25f;
                        }
                        else if(fill2.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill2));
                            fill2.fillAmount += 0.25f;
                        }
                        else if(fill3.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill3));
                            fill3.fillAmount += 0.25f;
                        }
                        else if(fill4.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill4));
                            fill4.fillAmount += 0.25f;
                        }
                    }
                    break;
            }
        }


        void OnJackpotWinEvent(EventData res)
        {
            Dictionary<int, int> tempPos = (Dictionary<int, int>)res.value;
            mono.StartCoroutine(ShowRewardEffect(tempPos.Keys.First(), tempPos.Values.First(), anchorFreeAdd));
        }


        void SkipAddMult(float value)
        {
            float temp = 0;
            if (fill1.fillAmount != 1)
            {
                if(fill1.fillAmount + value <= 1) fill1.fillAmount += value;
                else
                {
                    temp = fill1.fillAmount;
                    fill1.fillAmount = 1;
                    SkipAddMult(value + temp - 1);
                }
            }
            else if (fill2.fillAmount != 1)
            {
                if (fill2.fillAmount + value <= 1)  fill2.fillAmount += value;
                else
                {
                    temp = fill2.fillAmount;
                    fill2.fillAmount = 1;
                    SkipAddMult(value + temp - 1);
                }
            }
            else if (fill3.fillAmount != 1)
            {
                if (fill3.fillAmount + value <= 1)  fill3.fillAmount += value;
                else
                {
                    temp = fill3.fillAmount;
                    fill3.fillAmount = 1;
                    SkipAddMult(value + temp - 1);
                }
            }
            else if (fill4.fillAmount != 1)
            {
                if (fill3.fillAmount + value <= 1) fill4.fillAmount += value;
            }
        }

        private IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
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
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }

            yield return new WaitUntil(() => isNext == true);
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
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
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

                ////检查bigwin类型
                WinLevelType winLevelType = GetBigWinType();
                ////bigwin弹窗
                if (winLevelType != WinLevelType.None)
                {
                    //显示全部中奖图标和中奖线
                     slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                    //bigwin弹窗
                    //yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

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
            }
            #endregion

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(false);
            // 即中即退
            // yield return CoinOutImmediately(allWinCredit);

            //免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                PageManager.Instance.PreloadPage(PageName.CaiFuHuoChePopupFreeSpinTrigger, null);
                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1f);
                isNext = false;

                slotMachineCtrl.SkipWinLine(true);
                yield return FreeSpinTrigger(() => isNext = true, errorCallback);

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            //彩金奖
            if (ContentModel.Instance.isJackpotSpinTrigger)
            {
                isNext = false;
                //预加载触发动画防止字体闪烁
                PageManager.Instance.PreloadPage(PageName.CaiFuHuoChePopupJackpotGameTrigger, null);

                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 11 }, true, 11, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                yield return jackpotSpinTrigger(() => isNext = true, errorCallback);

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

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

        //彩金游戏进入和退出
        IEnumerator jackpotSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            PageManager.Instance.PreloadPage(PageName.CaiFuHuoChePopupJackpotGameExit, null);
            ContentModel.Instance.jackpotSpinWinCredit = 0;

            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotGameTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["SpinTimes"] = 3,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            PlayAnim(trainAnim, "ng_sg");
            yield return new WaitForSeconds(0.7f);
            PlayEffectAnim(norToJp);
            yield return new WaitForSeconds(1.2f);

            train.SetActive(false);

            ChangeBGPanel(2);
            freeTotalTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
            freeTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();

            yield return GameJackpotSpin(null, errorCallback);

            yield return slotMachineCtrl.JackpotWinCredit(() => isNext = true);

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipIdle(true);
            slotMachineCtrl.SkipWinLine(true);

            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotGameExit,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["winCredit"] = allWinCredit,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //加钱动画
            MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true, isAddCreditAnim);

            PlayEffectAnim(jpToNor);
            yield return new WaitForSeconds(0.5f);

            ChangeBGPanel(0);
            train.SetActive(true);
            JsToBsTrans.Play();
            PlayAnim(trainAnim, "sg_ng");
            yield return new WaitForSeconds(2.6f);

            PlayAnim(trainAnim, "idle");

            successCallback?.Invoke();
        }

        //开始彩金游戏
        IEnumerator GameJackpotSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "JS")
            {
                yield return slotMachineCtrl.SlotWaitForSeconds(1);
                yield return GameJackpotSpinOnce(null, errorCallback);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        //一局彩金游戏
        IEnumerator GameJackpotSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();

            ContentModel.Instance.haveJackpotCredit = false;
            ContentModel.Instance.gameState = GameState.Spin;

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

            freeTimes.text = (ContentModel.Instance.jackpotSpinTotalTimes - ContentModel.Instance.jackpotSpinPlayTimes).ToString();

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

            if (ContentModel.Instance.haveJackpotCredit)
            {
                freeTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
            }
        }


        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupFreeSpinTrigger,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["freeSpinCount"] = ContentModel.Instance.freeSpinTotalTimes,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipWinLine(false);
            slotMachineCtrl.CloseSlotCover();

            FreeGameReset();
            PlayAnim(trainAnim, "ng_fg");
            BsToFsTrans.Play();

            InputStackContextFreeSpin((context) =>
            {
            });

            yield return new WaitForSeconds(1.5f);
            ChangeBGPanel(1);
            gTrain.visible = false;

            slotMachineCtrl.BeginBonusFreeSpin();

            PageManager.Instance.PreloadPage(PageName.CaiFuHuoChePopupFreeSpinResult, null);

            yield return GameFreeSpin(null, errorCallback);


            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCredit,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            gTrain.visible = true;
            PlayAnim(trainAnim, "fg_ng");
            FsToBsTrans.Play();

            yield return new WaitForSeconds(0.75f);

            OutputStackContextFreeSpin(
                (context) =>
                {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);

                slotMachineCtrl.SetReelsDeck((string)context["./strDeckRowCol"]);

                _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);


                SymbolWin sw = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
                if (sw != null && sw.cells.Count > 0)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(sw, true);
                }
            });

            slotMachineCtrl.EndBonusFreeSpin();

            yield return new WaitForSeconds(0.75f);
            ChangeBGPanel(0);

            slotMachineCtrl.SkipWinLine(true);
            successCallback?.Invoke();
        }


        //开始免费游戏
        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return slotMachineCtrl.SlotWaitForSeconds(1f);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }


        //一局免费游戏
        IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
            ContentModel.Instance.haveFreeSpecialIcon = false;
            freeTimes.text = (ContentModel.Instance.freeSpinPlayTimes + 1).ToString();
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
                },(err) =>
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

            //停止特效显示
            slotMachineCtrl.SkipWinLine(true);
            slotMachineCtrl.CloseSlotCover();

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

            if (ContentModel.Instance.haveFreeSpecialIcon)
            {
                yield return new WaitForSeconds(1f);
            }

            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;


            #region Win

            if (winList.Count > 0 )
            {
                long totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                if(ContentModel.Instance.newFreeOnceCredit.Count > ContentModel.Instance.freeSpinPlayTimes - 1)
                {
                    totalWinLineCredit = ContentModel.Instance.newFreeOnceCredit[ContentModel.Instance.freeSpinPlayTimes - 1];
                }

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

            /* 先结算“免费游戏”或“小游戏”再回主游戏结算主游戏，则每局不能同步玩家真实金钱金额
           MainBlackboardController.Instance.SyncMyCreditToReal(false);*/

            /*
             if (ContentModel.Instance.isFreeSpinTrigger"))
             {
                 //增加免费游戏？？
             }*/


             #region 中游戏彩金

            //bool isHitJackpot = ContentModel.Instance.jpGameWinLst.Count > 0;
            //List<JackpotWinInfo> jpRes = ContentModel.Instance.jpGameWinLst;
            //while (jpRes.Count > 0)
            //{
            //    JackpotWinInfo jpWin = jpRes[0];
            //    jpRes.RemoveAt(0);

            //    Action onJPPoolSubCredit = () =>
            //    {
            //        SetJPCurCredit(jpWin);
            //    };

            //    allWinCredit += (long)jpWin.winCredit;

            //    PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupJackpotGame,
            //        new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
            //        {
            //            ["jackpotType"] = jpWin.name,
            //            ["totalEarnCredit"] = jpWin.winCredit,
            //            ["onJPPoolSubCredit"] = onJPPoolSubCredit,
            //            ["jpCredit"] = ContentModel.Instance.jpGameWhenCreditLst,
            //        }),
            //        (res) =>
            //        {
            //            isNext = true;
            //        });

            //    yield return new WaitUntil(() => isNext == true);
            //    isNext = false;

            //    // 总线赢分事件
            //    slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

            //    //MainBlackboardController.Instance.AddMyTempCredit((long)jpWin.winCredit, true, isAddCreditAnim);
            //}

            #endregion




            // 【取消】大厅彩金

            // 小游戏


            // 本剧同步玩家金钱
            //MainBlackboardController.Instance.SyncMyTempCreditToReal(false);

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

            if (ContentModel.Instance.freeSpinAddNum > 0)
            {
                slotMachineCtrl.BeginBonusFreeSpinAdd();

                // 【待修改】重置剩余的局数 
                ContentModel.Instance.showFreeSpinRemainTime =
                    ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes;

                if (!ApplicationSettings.Instance.isMock)
                {
                    ContentModel.Instance.freeSpinTotalTimes += ContentModel.Instance.freeSpinAddNum;
                }

                freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();

                slotMachineCtrl.EndBonusFreeSpinAdd();
            }

            #endregion


            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }


        void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            if(corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);

            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            isStoppedSlotMachine = false;

            ComReelEffect2.visible = false;
            ComReelEffect3.visible = false;
            if (!ContentModel.Instance.isJackpotSpin)
            {
                slotMachineCtrl.SkipWinLine(true);
            }
        }

        //游戏状态闲置
        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);

            int i = 0;
            while (i < 3 && !slotMachineCtrl.isStopImmediately)
            {
                i++;
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
            }
            yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }


        //免费游戏开始时重置
        private void FreeGameReset()
        {
            fill1.fillAmount = 0;
            fill2.fillAmount = 0;
            fill3.fillAmount = 0;
            fill4.fillAmount = 0;

            freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
        }

        //检查bigwin类型
        WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = ContentModel.Instance.winLevelMultiple;
            long totalBet = ContentModel.Instance.totalBet;
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

        #region bigWin相关
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

        #endregion


        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            //请求结果
            MachineDataG3996Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
            {
                resNode = res;
                isNext = true;
            }, (err) =>
            {
                errorCallback?.Invoke(err.msg);
                isNext = true;
                isBreak = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            if (isBreak) yield break;

            SBoxJackpotData sboxJackpotData = null;
            // 获取彩金贡献值
            int cacheTotalJpMajor = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            int cacheTotalJpGrand = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);

            SlotG3996MachineDataManager.Instance.RequestGetJpMajorGrandContribution((res) =>
            {
                JSONNode data = JSONNode.Parse((string)res);
                if (0 != (int)data["code"])
                {
                    errorCallback?.Invoke("请求贡献值报错");
                    isNext = true;
                    isBreak = true;
                    return;
                }

                int majorBet = (int)data["major"];
                int grandBet = (int)data["grand"];

                // 【保存数据，等下行时，删除数据】。
                cacheTotalJpMajor += majorBet;
                cacheTotalJpGrand += grandBet;
                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                JackBetInfoCoinPush info = new JackBetInfoCoinPush()
                {
                    gameType = 1,
                    seat = SBoxModel.Instance.pid,
                    betPercent = 1 * 100,
                    scoreRate = 1 * 1000,
                    JPPercent = 1 * 1000,
                    majorBet = majorBet * 100,
                    grandBet = grandBet * 100,
                };

                // 没有联网彩金
                //if (!ClientWS.Instance.IsConnected && !ApplicationSettings.Instance.isMock)
                //{
                //    isNext = true;
                //    return;
                //}

                //NetClientManager.Instance.RequestJackMajorGrand(info, (res) =>
                //{
                //    // 【联网彩金，请求成功 ，删除数据】
                //    cacheTotalJpMajor -= majorBet;
                //    cacheTotalJpGrand -= grandBet;
                //    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                //    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                //    sboxJackpotData = res as SBoxJackpotData;

                //    for (int i = 0; i < sboxJackpotData.Jackpotlottery.Length; i++)
                //        sboxJackpotData.Jackpotlottery[i] = sboxJackpotData.Jackpotlottery[i] / 100;

                //    for (int i = 0; i < sboxJackpotData.JackpotOut.Length; i++)
                //        sboxJackpotData.JackpotOut[i] = sboxJackpotData.JackpotOut[i] / 100;

                //    for (int i = 0; i < sboxJackpotData.JackpotOld.Length; i++)
                //        sboxJackpotData.JackpotOld[i] = sboxJackpotData.JackpotOld[i] / 100;

                //    // 【如果获取到联网彩金-通知算法卡】
                //    if (sboxJackpotData.Lottery[0] == 1)
                //    {
                //        ERPushMachineDataManager.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[0], (res) =>
                //        {

                //        });
                //    }
                //    if (sboxJackpotData.Lottery[1] == 1)
                //    {
                //        ERPushMachineDataManager.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[1], (res) =>
                //        {

                //        });
                //    }
                //    isNext = true;

                //}, (err) => // 联网彩金，请求失败
                //{
                //    errorCallback?.Invoke(err.msg);
                //    isNext = true;
                //    isBreak = true;
                //});

                isNext = true;
            });

            isNext = true;
            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            if (isBreak) yield break;

            // 【贡献返回给算法卡】
            if (cacheTotalJpMajor > 10 || cacheTotalJpGrand > 10)
            {
                ERPushMachineDataManager.Instance.RequestReturnMajorGrandContribution(
                    cacheTotalJpMajor > 10 ? cacheTotalJpMajor : 0,
                    cacheTotalJpGrand > 10 ? cacheTotalJpGrand : 0,
                    (res) =>
                    {

                        if ((int)res == 0)
                        {
                            if (cacheTotalJpMajor > 10)
                            {
                                cacheTotalJpMajor = 0;
                                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
                            }

                            if (cacheTotalJpGrand > 10)
                            {
                                cacheTotalJpGrand = 0;
                                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);
                            }
                        }

                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            // 解析数据
            MachineDataG3996Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }


        //请求算法结果
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            Debug.Log("请求算法结果");
            long totalBet = TotalBet;
            bool isBreak = false;
            bool isNext = false;
            bool isGetMyCredit = false;

            JSONNode resNode = null;
            int myCredit = -1;

            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                resNode = JSONNode.Parse((string)res);
                isNext = true;
                Debug.Log("算法结果");
                Debug.Log((string)res);
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //获取玩家金额
            //Debug.Log("获取玩家金额");
            //while (!isGetMyCredit)
            //{
            //    GetMyCredit((credit) =>
            //    {
            //        myCredit = credit;
            //        isGetMyCredit = true;
            //        isNext = true;
            //    }, (errMsg) =>
            //    {
            //        isNext = true;
            //    });

            //    yield return new WaitUntil(() => isNext == true);
            //    isNext = false;
            //}

            SBoxJackpotData sboxJackpotData = null;

            // 获取彩金贡献值
            int cacheTotalJpMajor = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            int cacheTotalJpGrand = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);


            //Debug.Log("获取彩金贡献值");
            //ERPushMachineDataManager02.Instance.RequestGetJpMajorGrandContribution((res) =>
            //{
            //    JSONNode data = JSONNode.Parse((string)res);

            //    if (0 != (int)data["code"])
            //    {
            //        errorCallback?.Invoke("请求贡献值报错");
            //        isNext = true;
            //        isBreak = true;
            //        return;
            //    }

            //    int majorBet = (int)data["major"];
            //    int grandBet = (int)data["grand"];

            //    // 【保存数据，等下行时，删除数据】。
            //    cacheTotalJpMajor += majorBet;
            //    cacheTotalJpGrand += grandBet;
            //    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
            //    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

            //    JackBetInfoCoinPush info = new JackBetInfoCoinPush()
            //    {
            //        gameType = 1,
            //        seat = SBoxModel.Instance.seatId,
            //        betPercent = 1 * 100,
            //        scoreRate = 1 * 1000,
            //        JPPercent = 1 * 1000,
            //        majorBet = majorBet * 100,
            //        grandBet = grandBet * 100,
            //    };

            //    // 没有联网彩金
            //    if (!ClientWS.Instance.IsConnected && !ApplicationSettings.Instance.isMock)
            //    {
            //        isNext = true;
            //        return;
            //    }

            //    NetClientHelper02.Instance.RequestJackBetMajorGrand(info, (res) =>
            //    {
            //        // 【联网彩金，请求成功 ，删除数据】
            //        cacheTotalJpMajor -= majorBet;
            //        cacheTotalJpGrand -= grandBet;
            //        SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
            //        SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

            //        sboxJackpotData = res as SBoxJackpotData;

            //        for (int i = 0; i < sboxJackpotData.Jackpotlottery.Length; i++)
            //            sboxJackpotData.Jackpotlottery[i] = sboxJackpotData.Jackpotlottery[i] / 100;

            //        for (int i = 0; i < sboxJackpotData.JackpotOut.Length; i++)
            //            sboxJackpotData.JackpotOut[i] = sboxJackpotData.JackpotOut[i] / 100;

            //        for (int i = 0; i < sboxJackpotData.JackpotOld.Length; i++)
            //            sboxJackpotData.JackpotOld[i] = sboxJackpotData.JackpotOld[i] / 100;

            //        // 【如果获取到联网彩金-通知算法卡】
            //        if (sboxJackpotData.Lottery[0] == 1)
            //        {
            //            ERPushMachineDataManager02.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[0], (res) =>
            //            {

            //            });
            //        }
            //        if (sboxJackpotData.Lottery[1] == 1)
            //        {
            //            ERPushMachineDataManager02.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[1], (res) =>
            //            {

            //            });
            //        }
            //        isNext = true;

            //    }, (err) => // 联网彩金，请求失败
            //    {
            //        errorCallback?.Invoke(err.msg);
            //        isNext = true;
            //        isBreak = true;
            //    });

            //});

            //isNext = true;
            //yield return new WaitUntil(() => isNext == true);
            //isNext = false;

            //if (isBreak) yield break;

            // 【贡献返回给算法卡】
            //Debug.Log("贡献返回给算法卡");
            //if (cacheTotalJpMajor > 10 || cacheTotalJpGrand > 10)
            //{
            //    ERPushMachineDataManager02.Instance.RequestReturnMajorGrandContribution(
            //        cacheTotalJpMajor > 10 ? cacheTotalJpMajor : 0,
            //        cacheTotalJpGrand > 10 ? cacheTotalJpGrand : 0,
            //        (res) =>
            //        {

            //            if ((int)res == 0)
            //            {
            //                if (cacheTotalJpMajor > 10)
            //                {
            //                    cacheTotalJpMajor = 0;
            //                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            //                }

            //                if (cacheTotalJpGrand > 10)
            //                {
            //                    cacheTotalJpGrand = 0;
            //                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);
            //                }
            //            }

            //            isNext = true;
            //        });

            //    yield return new WaitUntil(() => isNext == true);
            //    isNext = false;

            //}

            //int code = (int)resNode["code"]; //:0表示成功，-1表示传参失败
            //int code = 0;
            //if (code != 0)
            //{
            //    errorCallback?.Invoke($"Spin数据有误");
            //    DebugUtils.LogError($"Spin数据有误： {resNode.ToString()}");
            //    yield break;
            //}

            resNode["creditAfter"] = myCredit;
            //Debug.Log("解析数据");
            // 解析数据
            MachineDataG3996Controller.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // ui 彩金
            //SetUIJackpotGameReel();
            // Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }

        //显示线和中奖图标
        IEnumerator ShowWinListOnceAtNormalSpin(List<SymbolWin> winList)
        {
            //停止特效显示
            slotMachineCtrl.SkipWinLine(true);
            slotMachineCtrl.CloseSlotCover();

            //总线
            if (_spinWEMD.Instance.isTotalWin)
            {
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
            }
            else
            {
                //单线
                slotMachineCtrl.SkipWinLine(false);
                int idx = 0;
                while (idx < winList.Count)
                {
                    SymbolWin curSymvolWin = winList[idx];
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(curSymvolWin, true, PusherEmperorsRein.SpinWinEvent.SingleWinLine);
                    ++idx;
                }
            }
        }

        private bool isGetCrediting = false;
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

        List<Dictionary<string, object>> stackContext = new List<Dictionary<string, object>>();
        void InputStackContextFreeSpin(Action<Dictionary<string, object>> inputStackCallBack)
        {
            Dictionary<string, object> context = new Dictionary<string, object>()
            {
                ["name"] = "FreeSpinTrigger",
                ["modifyTime"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["./gameState"] = ContentModel.Instance.gameState,
                ["./winList"] = ContentModel.Instance.winList,
                ["./response"] = ContentModel.Instance.response,
                ["./winFreeSpinTriggerOrAddCopy"] = ContentModel.Instance.winFreeSpinTriggerOrAddCopy,
                //["./win5Kind"] = ContentModel.Instance.win5Kind,
                //["./isWin5Kind"] = ContentModel.Instance.isWin5Kind,
                ["./strDeckRowCol"] = ContentModel.Instance.strDeckRowCol,
                //["./middleIndexList"] = ContentModel.Instance.middleIndexList,
                ["./curReelStripsIndex"] = ContentModel.Instance.curReelStripsIndex,
                ["./nextReelStripsIndex"] = ContentModel.Instance.nextReelStripsIndex,
                ["./totalEarnCredit"] = ContentModel.Instance.totalEarnCredit,
                ["./isReelsSlowMotion"] = ContentModel.Instance.isReelsSlowMotion,
                ["./isFreeSpinTrigger"] = ContentModel.Instance.isFreeSpinTrigger,
                //["./customDataName"] = ContentModel.Instance.customDataName,
                //["./shufflingList"] = ContentModel.Instance.shufflingList,

                ["./curGameNumber"] = ContentModel.Instance.curGameNumber,
                ["./curGameCreatTimeMS"] = ContentModel.Instance.curGameCreatTimeMS,
                ["./curGameGuid"] = ContentModel.Instance.curGameGuid,
            };
            stackContext.Insert(0, context);

            //=====================
            inputStackCallBack?.Invoke(context);
        }


        void OutputStackContextFreeSpin(Action<Dictionary<string, object>> outputStackCallBack)
        {
            Dictionary<string, object> context = stackContext[0];
            stackContext.RemoveAt(0);

            ContentModel.Instance.gameState = (string)context["./gameState"];


            ContentModel.Instance.winList = (List<SymbolWin>)context["./winList"];
            ContentModel.Instance.response = (string)context["./response"];
            ContentModel.Instance.winFreeSpinTriggerOrAddCopy = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
            // ContentModel.Instance.win5Kind = (SymbolWin)context["./win5Kind"];
            ContentModel.Instance.strDeckRowCol = (string)context["./strDeckRowCol"];
            //ContentModel.Instance.middleIndexList = (List<int>)context["./middleIndexList"];
            ContentModel.Instance.curReelStripsIndex = (string)context["./curReelStripsIndex"];
            ContentModel.Instance.nextReelStripsIndex = (string)context["./nextReelStripsIndex"];
            ContentModel.Instance.totalEarnCredit = (long)context["./totalEarnCredit"];
            //ContentModel.Instance.isWin5Kind = (bool)context["./isWin5Kind"];
            ContentModel.Instance.isReelsSlowMotion = (bool)context["./isReelsSlowMotion"];
            ContentModel.Instance.isFreeSpinTrigger = (bool)context["./isFreeSpinTrigger"];
            //ContentModel.Instance.customDataName = (string)context["./customDataName"];
            //ContentModel.Instance.shufflingList = (List<List<int>>)context["./shufflingList"];


            ContentModel.Instance.curGameNumber = (long)context["./curGameNumber"];
            ContentModel.Instance.curGameCreatTimeMS = (long)context["./curGameCreatTimeMS"];
            ContentModel.Instance.curGameGuid = (string)context["./curGameGuid"];


            //=====================
            outputStackCallBack?.Invoke(context);
        }


        //通过控制器切换场景
        private void ChangeBGPanel(int type)
        {
            switch (type)
            {
                case 0:
                    this.contentPane.GetController("c1").selectedPage = "BS";
                    break;
                case 1:
                    this.contentPane.GetController("c1").selectedPage = "FS";
                    break;
                case 2:
                    this.contentPane.GetController("c1").selectedPage = "JS";
                    break;
            }
        }


        //显示加速框
        public IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            GComponent ComReelEffect = ComReelEffect3;
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                ComReelEffect = ComReelEffect2;
            }

            ComReelEffect.visible = false;
            ComReelEffect.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, anchorExpectation);
            ComReelEffect.visible = true;
           // GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);

            yield return new WaitUntil(() => isStoppedSlotMachine == true);
            // 关闭Expectation
            ComReelEffect.visible = false;
        }


        int rewardEffectIndex = 0;
        long allWinCredit = 0;
        //显示中奖后飞行粒子特效
        public IEnumerator ShowRewardEffect(int colIdx, int rowIdx, GComponent toNode)
        {
            GComponent rewardEffect = null;
            rewardEffectIndex = (rewardEffectIndex + 1) % 3;
            switch (rewardEffectIndex)
            {
                case 0:
                    rewardEffect = ComRewardEffect1;
                    break;
                case 1:
                    rewardEffect = ComRewardEffect2;
                    break;
                case 2:
                    rewardEffect = ComRewardEffect3;
                    break;
            }

            if (rewardEffect != null)
            {
                rewardEffect.parent.RemoveChild(rewardEffect);
                toNode.AddChild(rewardEffect);
                rewardEffect.visible = false;
                rewardEffect.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                rewardEffect.visible = true;

                yield return MoveToZeroOverTime(rewardEffect, slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode));
            }

            //记录并显示累计分数
            allWinCredit += ContentModel.Instance.jackpotSpinWinCredit;
            slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
        }

        IEnumerator MoveToZeroOverTime(GComponent effect ,Vector2 startPosition, float duration = 1f, Action successCallback = null)
        {
            Vector2 endPos = Vector2.zero; // (0,0)
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // 应用OutQuad缓动（更自然）
                float easedT = t * (2 - t);

                effect.xy = Vector2.Lerp(startPosition, endPos, easedT);
                yield return null;
            }

            // 确保最终位置准确
            effect.xy = Vector2.zero;
        }

        //播放指定动画
        private void PlayAnim(Animator animator, string animName)
        {
            animator.Rebind();
            animator.Play(animName);
            animator.Update(0f);
        }

        //根据传入的节点依次播放粒子特效
        private void PlayEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Play();

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                PlayEffectAnim(child);
            }
        }

        //根据传入的节点依次停止粒子特效
        private void StopEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Stop(true);

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }
    }
}
