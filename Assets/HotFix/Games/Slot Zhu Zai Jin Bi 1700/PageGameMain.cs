using FairyGUI;
using GameMaker;
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

        [JsonProperty("line_num")] public int LineNum;//线数

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }//赢钱倍数

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }//符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付线
    }

    public class PageGameMain : MachinePageBase
    {
        public const string pkgName = "SlotZhuZaiJinBi1700";
        public const string resName = "PageGameMain";

        private bool isInitPool = false; //资源池是否初始化
        private bool tipCoinIn = false; //提示硬币输入
        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        Coroutine corReelsTurn,corGameIdel, corGameOnce, corEffectSlowMotion, coGameAuto, _corPagTest1, _corPagTest2, _corPagTest3, _corPagTest4;
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
        GComponent gSlotCover, gPlayLines, gFrame;              //滚轴组件
        private GComponent gOwnerPanel;                         //菜单
        private GComponent gNormalGameFrame, gFreeGameFrame;    //外框
        private GComponent gNormalInnerFrame, gFreeInnerFrame;  //内框
        private GComponent gNormalBg, gFreeBg;                  //背景
        //过度动画
        private GComponent anchorNormalFrame, anchorFreeFrame;
        private GameObject goNormalFrame, goFreeFrame;
        private GameObject CLonegoNormalFrame, ClonegoFreeFrame;
        private Animator animatorNormalFrame;
        private SkeletonMecanim SMNormalFrame;
        // PagTest
        private GComponent _anchorPagTest;
        private GameObject _goPagTestPrefab;
        private GameObject _clonePagTest;
        private bool _pagTestButtonsBound;
        private const string PagTestLoader1 = "pagEffect1";
        private const string PagTestLoader2 = "pagEffect2";
        private const string PagTestLoader3 = "pagEffect3";
        private const string PagTestLoader4 = "pagEffect4";
        private const string PagTestLoader5 = "pagEffect5";
        private const string PagTestLoader6 = "pagEffect6";
        private const string PagTestSpine1Node = "Spine Mecanim GameObject (jp_pup_grand)";
        private const string PagTestSpine2Node = "Spine Mecanim GameObject (ng_pop_bigWin)";
        private const string PagTestSpine1PlayAnim = "GRAND_in";
        private const string PagTestSpine2PlayAnim = "bigwin_start";
        private PagSlotBinding _pagTestSlot1;
        private PagSlotBinding _pagTestSlot2;
        private PagSlotBinding _pagTestSlot3;
        private PagSlotBinding _pagTestSlot4;
        private PagSlotBinding _pagTestSlot5;
        private PagSlotBinding _pagTestSlot6;
        private bool _pagTest1Showing;
        private bool _pagTest2Showing;
        private bool _pagTest3Showing;
        private bool _pagTest4Showing;
        private bool _pagTest1CacheWarmed;
        private bool _pagTest2CacheWarmed;
        private bool _pagTest3CacheWarmed;
        private bool _pagTest4CacheWarmed;
        private Animator _pagTestSpine1Animator;
        private SkeletonMecanim _pagTestSpine1Mecanim;
        private Animator _pagTestSpine2Animator;
        private SkeletonMecanim _pagTestSpine2Mecanim;
        private bool _spineTest1Showing;
        private bool _spineTest2Showing;
        private const string PagTestTransitionPag = "BigWin_1024.pag";
        private const string PagTestNezaPag = "XingXing1.pag";
        private const string PagTestFeiZhouPag = "XingXing2.pag";
        private const string PagTestCaiHongFeiDiePag = "BigWin_1024.pag";
        private static readonly string[] PagTestLoopSequence = { PagTestNezaPag, PagTestTransitionPag };
        /// <summary>BigWin_1024 为 1024 合成，2x 显示等同 2048 占位。</summary>
        private const float PagTestBigWin1024DisplayScale = 1f;
        /// <summary>FeiZhou 合成尺寸较大，1x 显示并裁剪到 holder3。</summary>
        private const float PagTestFeiZhouDisplayScale = 1f;
        /// <summary>false：按合成尺寸×displayScale 显示；true：裁剪到 holder（测试槽位较小时会偏小）。</summary>
        private const bool PagTestClampDisplayToHolder = false;
        private const bool PagTestFeiZhouClampDisplayToHolder = false;
        private const float PagTestCaiHongFeiDieDisplayScale = 1f;
        private const bool PagTestCaiHongFeiDieClampDisplayToHolder = false;
        private const float PagTestDuration = 8f;
        private const float PagTestNezaPagDuration = 8f;
        private const float PagTestPlayStartedTimeoutSec = 45f;
        private const string PagLogPrefix = "[1700 PagTest]";
        /// <summary>Phase0 A/B：true 时全屏播 PAG；Phase1 通过后保持 false，走 FGUI extra 对齐。</summary>
        private const bool PagTestDebugFullScreen = false;
        /// <summary>true 时交替循环播 XingYunZhiLun_1080 与 neza；仅按钮触发时保持 false。</summary>
        private const bool PagTestLoop = false;
        /// <summary>true：PAG 在 FGUI pagEffect（层级由 FGUI 配置）；false：Activity WM 浮层。</summary>
        private const bool PagTestUseFguiTexture = true;
        /// <summary>FguiTexture 离屏最大边；0=合成原尺寸，512=降压缩屏（FGUI 仍按合成原尺寸显示）。</summary>
        /// <summary>0 = 不限制，使用 PAG 合成原尺寸渲染。</summary>
        private const int PagTestFguiMaxDisplaySide = 0;
        private const int PagTestFguiFps = 30;
        /// <summary>Overlay 模式：true 时 native 立即 ImageView 软件出帧。</summary>
        private const bool PagTestOverlayFallback = false;
        //免费组件
        private GComponent gFreeTimeBox, gFreeWinBox;
        private GComponent gFreeSlotMachine;
        //彩金
        //MiniReelGroup uiJPGrandCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();
        long TotalBet => (long)MainModel.Instance.contentMD.totalBet;

        /// <summary>1700：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady)
                return;

            int gameId = Convert.ToInt32(res.value);
            if (gameId != 1700)
                return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            preLoadedCallback?.Invoke();
        }

        protected override void OnInit()
        {

            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 5;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };


            //1
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
            //2
            ResourceManager02.Instance.LoadAssetBundleAsync(
                "Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            {
                UIPackage.AddPackage(ab);
                callback();
            });
            //3
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/GameMain/NormalFrame.prefab",
             (GameObject clone) =>
             {
                 goNormalFrame = clone;
                 callback();
             });
            //4
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/GameMain/FreeFrame.prefab",
            (GameObject clone) =>
            {
                goFreeFrame = clone;
                callback();
            });
            //5
            ResourceManager02.Instance.LoadAsset<GameObject>(
          "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/TurnTable/TestBigWin.prefab",
          (GameObject clone) =>
          {
              _goPagTestPrefab = clone;
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
                        CommonPopupHandler.Instance.ClosePopup();
                        OnClickSpinButton(res);

                    },
                },

                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true); // isLongClick
                        CommonPopupHandler.Instance.ClosePopup();
                        OnClickSpinButton(res);
                    }
                }

            };

 
        }
        protected override void OnLanguageChange(I18nLang lang)
        {
            ClearPagTestButtons();
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }
        public override void OnOpen(PageName name, EventData data)
        {
            if (isOpen) return;

            if (goGameCtrl != null && !goGameCtrl.activeSelf)
            {
                goGameCtrl.SetActive(true);
            }
            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);

            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            InitParam(data);
        }
        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            GameSoundHelper.Instance.StopMusic();
            StopAllPagTest();
            ClearPagTestButtons();
            DisposePagTestResources();
            if (goGameCtrl != null && goGameCtrl.activeSelf)
            {
                goGameCtrl.SetActive(false);
            }
            base.OnClose(data);
        }
        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataG1700Controller.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;

            MainModel.Instance.gameID = 1700;
            MainModel.Instance.gameName = "ZhuZaiJinBi1700";
            MainModel.Instance.displayName = "ZhuZaiJinBi1700";
            MainModel.Instance.lineNum = 15;
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;

            List<GComponent> lstPayTable = new List<GComponent>();
            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                lstPayTable.Add(paytable);
            }
            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);

            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            gSlotCover = gSlotMachine.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);

            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;
                fguiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);
                fguiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);
            }

            gOwnerPanel = contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            if (!isOpen) return;

            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {
                SBoxAccount account = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = account.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {
                        MainBlackboardController.Instance.SetMyRealCredit(playerAccountList[i].Credit);
                        break;
                    }
                }
            }, (BagelCodeError err) =>
            {
                DebugUtils.Log(err.msg);
            });
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            gNormalBg = contentPane.GetChild("normalBG").asCom;
            gFreeBg = contentPane.GetChild("freeBG").asCom;
            gNormalGameFrame = contentPane.GetChild("normalGameframe").asCom;
            gFreeGameFrame = contentPane.GetChild("freeGameFrame").asCom;
            gNormalInnerFrame = contentPane.GetChild("normalInnerFrame").asCom;
            gFreeInnerFrame = contentPane.GetChild("freeInnerFrame").asCom;

            gFreeBg.visible = false;
            gFreeGameFrame.visible = false;
            gFreeInnerFrame.visible = false;

            GComponent localNormalFrame = contentPane.GetChild("anchorNormalFrame").asCom;
            if (anchorNormalFrame != localNormalFrame)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorNormalFrame);
                CLonegoNormalFrame = GameObject.Instantiate(goNormalFrame);
                animatorNormalFrame = CLonegoNormalFrame.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                SMNormalFrame = CLonegoNormalFrame.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorNormalFrame = localNormalFrame;
                GameCommon.FguiUtils.AddWrapper(anchorNormalFrame, CLonegoNormalFrame);
            }

            GComponent localFreeFrame = contentPane.GetChild("anchorFreeFrame").asCom;
            if (anchorFreeFrame != localFreeFrame)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorFreeFrame);
                ClonegoFreeFrame = GameObject.Instantiate(goFreeFrame);
                anchorFreeFrame = localFreeFrame;
                GameCommon.FguiUtils.AddWrapper(anchorFreeFrame, ClonegoFreeFrame);
            }
            anchorNormalFrame.visible = false;
            anchorFreeFrame.visible = false;
            SMNormalFrame.Skeleton.SetColor(new Color(1, 1, 1, 0));

            gFreeTimeBox = contentPane.GetChild("freeTimeBox").asCom;
            gFreeWinBox = contentPane.GetChild("freeWinBox").asCom;
            gFreeSlotMachine = contentPane.GetChild("freeSlotMachine").asCom;
            gFreeTimeBox.visible = false;
            gFreeWinBox.visible = false;
            gFreeSlotMachine.visible = false;

            GComponent localPagTestAnchor = contentPane.GetChild("anchorPagTest").asCom;
            if (_anchorPagTest != localPagTestAnchor)
            {
                GameCommon.FguiUtils.DeleteWrapper(_anchorPagTest);
                _clonePagTest = GameObject.Instantiate(_goPagTestPrefab);
                _anchorPagTest = localPagTestAnchor;
                GameCommon.FguiUtils.AddWrapper(_anchorPagTest, _clonePagTest);
                // _pagTestAttachBone = null;
                _pagTestSlot1?.Dispose();
                _pagTestSlot1 = null;
                _pagTestSlot2?.Dispose();
                _pagTestSlot2 = null;
                _pagTestSlot3?.Dispose();
                _pagTestSlot3 = null;
                _pagTestSlot4?.Dispose();
                _pagTestSlot4 = null;
                _pagTestSlot5?.Dispose();
                _pagTestSlot5 = null;
                _pagTestSlot6?.Dispose();
                _pagTestSlot6 = null;
                _pagTestSpine1Animator = null;
                _pagTestSpine1Mecanim = null;
                _pagTestSpine2Animator = null;
                _pagTestSpine2Mecanim = null;
                _spineTest1Showing = false;
                _spineTest2Showing = false;
                _pagTest3Showing = false;
                EnsurePagTestSpines();
            }

            // if (_pagTestAttachBone == null)
            // {
            //     _pagTestAttachBone = FindPagTestAttachBone(_clonePagTest, "c_circle");
            //     AttachJpMajorToPagTestBone();
            // }

            EnsurePagTestSlots();
            EnsurePagTestSpines();
            BindPagTestButtons();

            uiJPMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            uiJPMajorCtrl.SetData(0);
            uiJPMinorCtrl.SetData(0);
            uiJPMiniCtrl.SetData(0);
            ChangeBGPanel(0);
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            TryRestoreFreeSpinSession();
        }

        // private Transform FindPagTestAttachBone(GameObject pagTestObject, string boneName)
        // {
        //     string candidatePaths = $"Anchor/Spine Mecanim GameObject (ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/{boneName}";
        //     Transform pathTransform = pagTestObject.transform.Find(candidatePaths);
        //     return pathTransform;
        // }

        private void EnsurePagTestSlots()
        {
            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlots skipped: anchor null");
                return;
            }

            if (_pagTestSlot1 == null)
            {
                _pagTestSlot1 = new PagSlotBinding("PagTest1");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest1");
            }

            if (_pagTestSlot2 == null)
            {
                _pagTestSlot2 = new PagSlotBinding("PagTest2");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest2");
            }

            if (_pagTestSlot3 == null)
            {
                _pagTestSlot3 = new PagSlotBinding("PagTest3");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest3");
            }

            EnsurePagTestSlot(_pagTestSlot1, PagTestLoader1, "PagTest1");
            EnsurePagTestSlot(_pagTestSlot2, PagTestLoader2, "PagTest2");
            EnsurePagTestSlot(_pagTestSlot3, PagTestLoader3, "PagTest3");

            if (_pagTestSlot4 == null)
            {
                _pagTestSlot4 = new PagSlotBinding("PagTest4");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest4");
            }

            if (_pagTestSlot5 == null)
            {
                _pagTestSlot5 = new PagSlotBinding("PagTest5");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest5");
            }

            if (_pagTestSlot6 == null)
            {
                _pagTestSlot6 = new PagSlotBinding("PagTest6");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest6");
            }

            EnsurePagTestSlot(_pagTestSlot4, PagTestLoader4, "PagTest4");
            EnsurePagTestSlot(_pagTestSlot5, PagTestLoader5, "PagTest5");
            EnsurePagTestSlot(_pagTestSlot6, PagTestLoader6, "PagTest6");
        }

        private void EnsurePagTestSlotForPlay(PagSlotBinding slot)
        {
            if (slot == null)
            {
                return;
            }

            if (slot == _pagTestSlot1 && _pagTestSlot1 == null)
            {
                _pagTestSlot1 = new PagSlotBinding("PagTest1");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest1");
            }

            if (slot == _pagTestSlot2 && _pagTestSlot2 == null)
            {
                _pagTestSlot2 = new PagSlotBinding("PagTest2");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest2");
            }

            if (slot == _pagTestSlot3 && _pagTestSlot3 == null)
            {
                _pagTestSlot3 = new PagSlotBinding("PagTest3");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest3");
            }

            if (slot == _pagTestSlot4 && _pagTestSlot4 == null)
            {
                _pagTestSlot4 = new PagSlotBinding("PagTest4");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest4");
            }

            if (slot == _pagTestSlot5 && _pagTestSlot5 == null)
            {
                _pagTestSlot5 = new PagSlotBinding("PagTest5");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest5");
            }

            if (slot == _pagTestSlot6 && _pagTestSlot6 == null)
            {
                _pagTestSlot6 = new PagSlotBinding("PagTest6");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest6");
            }

            EnsurePagTestSlot(slot, ResolvePagTestLoaderName(slot), slot.InstanceKey);
        }

        private void EnsurePagTestSlot(PagSlotBinding slot, string loaderName, string instanceLabel)
        {
            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlot skipped: anchor null, instance={instanceLabel}");
                return;
            }

            if (slot == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlot skipped: slot null, instance={instanceLabel}");
                return;
            }

            slot.Attach(anchor, loaderName);
        }

        private string ResolvePagTestLoaderName(PagSlotBinding slot)
        {
            if (slot == _pagTestSlot1)
            {
                return PagTestLoader1;
            }

            if (slot == _pagTestSlot2)
            {
                return PagTestLoader2;
            }

            if (slot == _pagTestSlot3)
            {
                return PagTestLoader3;
            }

            if (slot == _pagTestSlot4)
            {
                return PagTestLoader4;
            }

            if (slot == _pagTestSlot5)
            {
                return PagTestLoader5;
            }

            if (slot == _pagTestSlot6)
            {
                return PagTestLoader6;
            }

            return PagFguiGpuPresenter.DefaultLoaderName;
        }

        private void DisposePagTestResources()
        {
            _pagTestSlot1?.Dispose();
            _pagTestSlot1 = null;
            _pagTestSlot2?.Dispose();
            _pagTestSlot2 = null;
            _pagTestSlot3?.Dispose();
            _pagTestSlot3 = null;
            _pagTestSlot4?.Dispose();
            _pagTestSlot4 = null;
            _pagTestSlot5?.Dispose();
            _pagTestSlot5 = null;
            _pagTestSlot6?.Dispose();
            _pagTestSlot6 = null;
            _pagTestSpine1Animator = null;
            _pagTestSpine1Mecanim = null;
            _pagTestSpine2Animator = null;
            _pagTestSpine2Mecanim = null;
            _pagTest1Showing = false;
            _pagTest2Showing = false;
            _pagTest3Showing = false;
            _pagTest4Showing = false;
            _spineTest1Showing = false;
            _spineTest2Showing = false;
            _pagTest1CacheWarmed = false;
            _pagTest2CacheWarmed = false;
            _pagTest3CacheWarmed = false;
            _pagTest4CacheWarmed = false;
        }

        private void StopPagTestGroupPlayback()
        {
            if (_corPagTest4 != null)
            {
                Debug.Log($"{PagLogPrefix} group playback aborted");
                PagCallbackHub.Instance.StopRunCoroutine(_corPagTest4);
                _corPagTest4 = null;
            }

            PagGpuSyncGroup.EndGroup();
            StopPagTestGroupSlots();
            _pagTest4Showing = false;
        }

        private void StopPagTestGroupSlots()
        {
            _pagTestSlot4?.Stop(PagTestUseFguiTexture);
            _pagTestSlot5?.Stop(PagTestUseFguiTexture);
            _pagTestSlot6?.Stop(PagTestUseFguiTexture);
        }

        private void ConfigurePagTestGroupSlot(PagSlotBinding slot, float displayScale, bool clampToHolder)
        {
            if (slot == null)
            {
                return;
            }

            slot.SetFguiDisplayScale(displayScale);
            slot.SetFguiClampDisplayToHolder(clampToHolder);
        }

        private static readonly PagSlotBinding[] PagTestGroupTripleSlots = new PagSlotBinding[3];

        private PagSlotBinding[] GetPagTestGroupSlots()
        {
            PagTestGroupTripleSlots[0] = _pagTestSlot4;
            PagTestGroupTripleSlots[1] = _pagTestSlot5;
            PagTestGroupTripleSlots[2] = _pagTestSlot6;
            return PagTestGroupTripleSlots;
        }

        private bool TryBuildPagTestLayoutExtraForAnchor(GComponent anchor, out string extra, out string debugReason)
        {
            return TryBuildPagTestLayoutExtra(out extra, out debugReason);
        }

        private void StopAllPagTest()
        {
            if (_corPagTest1 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=1");
                mono.StopCoroutine(_corPagTest1);
                _corPagTest1 = null;
            }

            if (_corPagTest2 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=2");
                mono.StopCoroutine(_corPagTest2);
                _corPagTest2 = null;
            }

            if (_corPagTest3 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=3");
                mono.StopCoroutine(_corPagTest3);
                _corPagTest3 = null;
            }

            StopPagTestGroupPlayback();

            StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
            StopPagTest(_pagTestSlot2, ref _pagTest2Showing);
            StopPagTest(_pagTestSlot3, ref _pagTest3Showing);
        }

        private GComponent GetPagTestAnchor()
        {
            if (_anchorPagTest != null)
            {
                return _anchorPagTest;
            }

            return contentPane?.GetChild("anchorPagTest")?.asCom;
        }

        /// <summary>
        /// 将 anchorPagTest 区域换算为 Native overlay 的 extra（x,y,w,h 为相对屏幕 0~1）。
        /// </summary>
        private bool TryBuildPagTestLayoutExtra(out string extra, out string debugReason)
        {
            extra = null;
            debugReason = "unknown";

            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                debugReason = "anchorPagTest is null";
                return false;
            }

            GGraph holder = anchor.GetChild("holder")?.asGraph;
            GLoader example = anchor.GetChild("example")?.asLoader;

            float localW = holder != null && holder.width > 0f ? holder.width : (example != null ? example.width : 200f);
            float localH = holder != null && holder.height > 0f ? holder.height : (example != null ? example.height : 200f);
            if (localW <= 0f || localH <= 0f)
            {
                debugReason = $"invalid size holder={holder?.width}x{holder?.height} example={example?.width}x{example?.height}";
                return false;
            }

            float rootW = GRoot.inst.width;
            float rootH = GRoot.inst.height;
            if (rootW <= 0f || rootH <= 0f)
            {
                debugReason = $"invalid GRoot size {rootW}x{rootH}";
                return false;
            }

            float normW = Screen.width > 0f ? Screen.width : rootW;
            float normH = Screen.height > 0f ? Screen.height : rootH;

            GObject layoutTarget = holder != null && holder.width > 0f ? (GObject)holder : anchor;
            Rect globalRect = layoutTarget.LocalToGlobal(new Rect(0f, 0f, localW, localH));
            float x = Mathf.Clamp01(globalRect.xMin / normW);
            float y = Mathf.Clamp01(globalRect.yMin / normH);
            float w = Mathf.Clamp(globalRect.width / normW, 0.01f, 1f - x);
            float h = Mathf.Clamp(globalRect.height / normH, 0.01f, 1f - y);

            if (w * h < 0.01f)
            {
                debugReason = $"rect too small w={w:F4} h={h:F4}, use turntable fallback";
                return false;
            }

            extra = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0:F4},{1:F4},{2:F4},{3:F4}", x, y, w, h);
            debugReason = $"ok target={layoutTarget.name} global={globalRect} Screen={normW}x{normH} GRoot={rootW}x{rootH}";
            return true;
        }

        private float GetPagTestDurationFallback(string pagFileName)
        {
            return pagFileName == PagTestNezaPag ? PagTestNezaPagDuration : PagTestDuration;
        }

        /// <summary>Play 开始后读取 Native composition frameRate，与 Unity 出帧节流对齐。</summary>
        private IEnumerator TryAlignPagTestFpsAfterPlayStarted(PagSlotBinding slot)
        {
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                yield break;
            }

            yield return controller.WaitForPlayStarted(PagTestPlayStartedTimeoutSec);
            int nativeFps = controller.GetCompositionFrameRate();
            if (nativeFps <= 0)
            {
                yield break;
            }

            if (nativeFps == PagTestFguiFps)
            {
                Debug.Log($"{PagLogPrefix} FGUI fps aligned with composition: {nativeFps}, instance={slot.InstanceKey}");
                yield break;
            }

            slot.ConfigureFgui(PagTestFguiMaxDisplaySide, nativeFps);
            Debug.Log($"{PagLogPrefix} aligned FGUI fps {PagTestFguiFps}->{nativeFps}, instance={slot.InstanceKey}");
        }

        private static bool IsPagCompositionReady(string pagFileName)
        {
            if (!PagPathHelper.IsCached(pagFileName))
            {
                return false;
            }

            string absPath = PagController.ResolvePagPath(pagFileName, PagPathHelper.DefaultGamePagFolder);
            return PagController.IsCompositionCached(absPath);
        }

        /// <summary>Loading 已预热则秒过；否则磁盘 + Java composition 兜底预加载。</summary>
        private IEnumerator EnsurePagTestCompositionReady(string pagFileName, bool alreadyWarmed, Action<bool> onDone)
        {
            if (alreadyWarmed && IsPagCompositionReady(pagFileName))
            {
                onDone?.Invoke(true);
                yield break;
            }

            if (IsPagCompositionReady(pagFileName))
            {
                onDone?.Invoke(true);
                yield break;
            }

            yield return PagController.PreloadCompositionCoroutine(pagFileName);
            onDone?.Invoke(IsPagCompositionReady(pagFileName));
        }

        private void PlayPagTest(PagSlotBinding slot, string pagFileName, int repeatCount = 1, float displayScale = 2f,
            bool? clampDisplayToHolder = null)
        {
            Debug.Log($"{PagLogPrefix} PlayPagTest start: instance={slot?.InstanceKey}, {pagFileName}, repeat={repeatCount}");
            EnsurePagTestSlotForPlay(slot);
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: PagController is null, instance={slot?.InstanceKey}");
                return;
            }

            string resolvedPath = controller.ResolvePagPath(pagFileName);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: resolve path null, file={pagFileName}, instance={slot.InstanceKey}");
                return;
            }

            Debug.Log($"{PagLogPrefix} resolved path: {resolvedPath}, exists={System.IO.File.Exists(resolvedPath)}, instance={slot.InstanceKey}");

            string positionType = "center";
            string layoutExtra = "";
            if (PagTestDebugFullScreen)
            {
                positionType = "full";
                layoutExtra = "";
                Debug.Log($"{PagLogPrefix} debug fullscreen mode, skip layout extra");
            }
            else if (TryBuildPagTestLayoutExtra(out layoutExtra, out string layoutDebug))
            {
                Debug.Log($"{PagLogPrefix} layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} layout extra unavailable ({layoutDebug}), fallback LayoutPagAuto(turntable)");
                controller.LayoutPagAuto("turntable");
            }

            if (PagTestUseFguiTexture)
            {
                slot.SetFguiDisplayScale(displayScale);
                slot.SetFguiClampDisplayToHolder(clampDisplayToHolder ?? PagTestClampDisplayToHolder);

                if (!slot.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
                {
                    Debug.LogError($"{PagLogPrefix} PlayPagTest failed: FGUI slot not ready, pag={pagFileName}, instance={slot.InstanceKey}");
                    return;
                }

                Debug.Log($"{PagLogPrefix} FGUI frame config: maxSide={PagTestFguiMaxDisplaySide} fps={PagTestFguiFps} "
                    + $"displayScale={displayScale} pag={pagFileName} instance={slot.InstanceKey}");
            }
            else
            {
                slot.PreparePlay(false, 0, 0);
                controller.SetForceBitmapOverlayFallback(PagTestOverlayFallback);
            }

            controller.SetRepeatCount(repeatCount);
            bool playOk = controller.PlayPag(pagFileName, positionType, layoutExtra);
            if (playOk)
            {
                Debug.Log($"{PagLogPrefix} PlayPagTest success: {pagFileName}, instance={slot.InstanceKey}");
            }
            else
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: {pagFileName}, instance={slot.InstanceKey}");
            }
        }

        private void StopPagTest(PagSlotBinding slot, ref bool showingFlag)
        {
            if (slot?.Controller == null)
            {
                Debug.LogWarning($"{PagLogPrefix} StopPagTest skipped: PagController is null, instance={slot?.InstanceKey}");
                showingFlag = false;
                return;
            }

            slot.Stop(PagTestUseFguiTexture);
            showingFlag = false;

            Debug.Log($"{PagLogPrefix} StopPagTest instance={slot.InstanceKey}");
        }

        private void TryPlayPagTestOnEnter()
        {
            if (!isInit || mono == null || slotMachineCtrl == null)
            {
                Debug.LogWarning($"{PagLogPrefix} TryPlayPagTestOnEnter skipped: isInit={isInit}, mono={mono != null}, slotMachineCtrl={slotMachineCtrl != null}");
                return;
            }

            Debug.Log($"{PagLogPrefix} TryPlayPagTestOnEnter: loop={PagTestLoop}, sequence=[{string.Join(", ", PagTestLoopSequence)}]");

            if (_corPagTest1 != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=restart");
                mono.StopCoroutine(_corPagTest1);
            }

            _corPagTest1 = mono.StartCoroutine(PlayPagTestEnterSequence());
        }

        private IEnumerator PlayPagTestEnterSequence()
        {
            Debug.Log($"{PagLogPrefix} PlayPagTestEnterSequence start");

            for (int i = 0; i < PagTestLoopSequence.Length; i++)
            {
                yield return PagController.PreloadCompositionCoroutine(PagTestLoopSequence[i]);
            }

            if (PagTestLoop)
            {
                int loopIndex = 0;
                while (true)
                {
                    string pagFileName = PagTestLoopSequence[0];
                    PlayPagTest(_pagTestSlot1, pagFileName, -1, PagTestBigWin1024DisplayScale);
                    yield return WaitPagTestPlayStarted(_pagTestSlot1, PagTestPlayStartedTimeoutSec);
                    PagController controller = _pagTestSlot1?.Controller;
                    if (controller == null || !controller.PlayStarted)
                    {
                        Debug.LogError($"{PagLogPrefix} {pagFileName} play did not start within {PagTestPlayStartedTimeoutSec}s");
                        Debug.Log($"{PagLogPrefix} sequence aborted reason=pag_play_started_timeout pag={pagFileName}");
                        _corPagTest1 = null;
                        yield break;
                    }

                    if (PagTestUseFguiTexture)
                    {
                        float durationFallback = GetPagTestDurationFallback(pagFileName);
                        float pagTimeout = controller.GetCompositionDurationSecWithFallback(durationFallback) + 3f;
                        yield return controller.WaitForPlaybackFinished(pagTimeout);
                    }
                    else
                    {
                        yield return slotMachineCtrl.SlotWaitForSeconds(GetPagTestDurationFallback(pagFileName));
                    }

                    loopIndex = (loopIndex + 1) % PagTestLoopSequence.Length;
                    Debug.Log($"{PagLogPrefix} loop next: {PagTestLoopSequence[loopIndex]}");
                }
            }

            PlayPagTest(_pagTestSlot1, PagTestTransitionPag);
            yield return WaitPagTestPlayStarted(_pagTestSlot1, PagTestPlayStartedTimeoutSec);
            PagController transitionController = _pagTestSlot1?.Controller;
            if (transitionController == null || !transitionController.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} {PagTestTransitionPag} play did not start within {PagTestPlayStartedTimeoutSec}s");
                Debug.Log($"{PagLogPrefix} sequence aborted reason=pag_play_started_timeout");
                _corPagTest1 = null;
                yield break;
            }

            if (PagTestUseFguiTexture)
            {
                float pagTimeout = transitionController.GetCompositionDurationSecWithFallback(PagTestDuration) + 3f;
                yield return transitionController.WaitForPlaybackFinished(pagTimeout);
            }
            else
            {
                yield return slotMachineCtrl.SlotWaitForSeconds(PagTestDuration);
            }

            StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
            yield return PagPathHelper.DeferredUnloadUnusedAssets();
            _corPagTest1 = null;
            Debug.Log($"{PagLogPrefix} PlayPagTestEnterSequence finished");
        }

        private IEnumerator WaitPagTestPlayStarted(PagSlotBinding slot, float timeoutSec)
        {
            EnsurePagTestSlots();
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                yield break;
            }

            float deadline = Time.unscaledTime + timeoutSec;
            while (!controller.PlayStarted && Time.unscaledTime < deadline)
            {
                yield return null;
            }

            if (controller.PlayStarted)
            {
                Debug.Log($"{PagLogPrefix} Pag play started (within {timeoutSec}s), instance={slot.InstanceKey}");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} Pag play started timeout ({timeoutSec}s), instance={slot.InstanceKey}");
            }
        }

        // private void AttachJpMajorToPagTestBone()
        // {
        //     GObject jpMajor = this.contentPane.GetChild("jpMajor");
        //     if (jpMajor?.displayObject?.gameObject != null && _pagTestAttachBone != null)
        //     {
        //         Transform t = jpMajor.displayObject.gameObject.transform;
        //         t.SetParent(_pagTestAttachBone, false);
        //         t.localPosition = Vector3.zero;
        //     }
        // }

        private void BindPagTestButtons()
        {
            if (_pagTestButtonsBound || contentPane == null)
            {
                return;
            }

            GButton btnPag1 = contentPane.GetChild("PAG1")?.asButton;
            if (btnPag1 != null)
            {
                btnPag1.onClick.Clear();
                btnPag1.onClick.Add(OnClickPagTest1Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG1");
            }

            GButton btnPag2 = contentPane.GetChild("PAG2")?.asButton;
            if (btnPag2 != null)
            {
                btnPag2.onClick.Clear();
                btnPag2.onClick.Add(OnClickPagTest2Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG2");
            }

            GButton btnPag3 = contentPane.GetChild("PAG3")?.asButton;
            if (btnPag3 != null)
            {
                btnPag3.onClick.Clear();
                btnPag3.onClick.Add(OnClickPagTest3Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG3");
            }

            GButton btnPag4 = contentPane.GetChild("PAG4")?.asButton;
            if (btnPag4 != null)
            {
                btnPag4.onClick.Clear();
                btnPag4.onClick.Add(OnClickPagTest4Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG4");
            }

            GButton btnSpine1 = contentPane.GetChild("Spine1")?.asButton;
            if (btnSpine1 != null)
            {
                btnSpine1.onClick.Clear();
                btnSpine1.onClick.Add(OnClickSpineTest1Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: Spine1");
            }

            GButton btnSpine2 = contentPane.GetChild("Spine2")?.asButton;
            if (btnSpine2 != null)
            {
                btnSpine2.onClick.Clear();
                btnSpine2.onClick.Add(OnClickSpineTest2Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: Spine2");
            }

            _pagTestButtonsBound = true;
        }

        private void ClearPagTestButtons()
        {
            if (!_pagTestButtonsBound || contentPane == null)
            {
                return;
            }

            contentPane.GetChild("PAG1")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG2")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG3")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG4")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine1")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine2")?.asButton?.onClick.Clear();
            _pagTestButtonsBound = false;
        }

        private void EnsurePagTestSpines()
        {
            if (_clonePagTest == null)
            {
                return;
            }

            EnsurePagTestSpine(ref _pagTestSpine1Animator, ref _pagTestSpine1Mecanim, PagTestSpine1Node, 1);
            EnsurePagTestSpine(ref _pagTestSpine2Animator, ref _pagTestSpine2Mecanim, PagTestSpine2Node, 2);
        }

        private void EnsurePagTestSpine(ref Animator animator, ref SkeletonMecanim mecanim, string nodeName, int spineIndex)
        {
            if (animator != null)
            {
                return;
            }

            Transform spineTransform = _clonePagTest.transform.Find($"Anchor/{nodeName}");
            if (spineTransform == null)
            {
                spineTransform = FindChildRecursiveByName(_clonePagTest.transform, nodeName);
            }

            if (spineTransform == null)
            {
                Debug.LogWarning($"{PagLogPrefix} spine not found on _clonePagTest: {nodeName}");
                return;
            }

            animator = spineTransform.GetComponent<Animator>();
            mecanim = spineTransform.GetComponent<SkeletonMecanim>();
            if (animator == null)
            {
                Debug.LogWarning($"{PagLogPrefix} Animator missing on spine: {nodeName}");
                return;
            }

            HidePagTestSpine(spineIndex);
        }

        private void HidePagTestSpine(int spineIndex)
        {
            if (spineIndex == 1)
            {
                if (_pagTestSpine1Animator == null)
                {
                    return;
                }

                _pagTestSpine1Animator.enabled = false;
                if (_pagTestSpine1Mecanim != null)
                {
                    _pagTestSpine1Mecanim.Skeleton.SetColor(new Color(1f, 1f, 1f, 0f));
                }

                _spineTest1Showing = false;
                return;
            }

            if (_pagTestSpine2Animator == null)
            {
                return;
            }

            _pagTestSpine2Animator.enabled = false;
            if (_pagTestSpine2Mecanim != null)
            {
                _pagTestSpine2Mecanim.Skeleton.SetColor(new Color(1f, 1f, 1f, 0f));
            }

            _spineTest2Showing = false;
        }

        private void ShowPagTestSpine(int spineIndex, string animName)
        {
            EnsurePagTestSpines();

            Animator animator;
            SkeletonMecanim mecanim;

            if (spineIndex == 1)
            {
                animator = _pagTestSpine1Animator;
                mecanim = _pagTestSpine1Mecanim;
            }
            else
            {
                animator = _pagTestSpine2Animator;
                mecanim = _pagTestSpine2Mecanim;
            }

            if (animator == null)
            {
                Debug.LogWarning($"{PagLogPrefix} Spine{spineIndex} show failed: animator is null");
                return;
            }

            if (mecanim != null)
            {
                mecanim.Skeleton.SetColor(new Color(1f, 1f, 1f, 1f));
            }

            animator.enabled = true;
            animator.speed = 1f;
            animator.Rebind();
            animator.Update(0f);
            animator.Play(animName, 0, 0f);

            if (spineIndex == 1)
            {
                _spineTest1Showing = true;
            }
            else
            {
                _spineTest2Showing = true;
            }

            Debug.Log($"{PagLogPrefix} Spine{spineIndex} show {animName}");
        }

        private void OnClickPagTest1Button()
        {
            StopPagTestGroupPlayback();

            if (_pagTest1Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG1 clicked, stop {PagTestNezaPag}");
                _pagTest1Showing = false;
                if (_corPagTest1 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest1);
                    _corPagTest1 = null;
                }

                StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG1 clicked, play {PagTestNezaPag}");
            if (_corPagTest1 != null && mono != null)
            {
                mono.StopCoroutine(_corPagTest1);
                _corPagTest1 = null;
            }

            StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG1 play skipped: mono is null");
                return;
            }

            _pagTest1Showing = true;
            _corPagTest1 = mono.StartCoroutine(StartPagTest1ButtonPlayback());
        }

        private void OnClickPagTest2Button()
        {
            StopPagTestGroupPlayback();

            if (_pagTest2Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG2 clicked, stop {PagTestTransitionPag}");
                _pagTest2Showing = false;
                if (_corPagTest2 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest2);
                    _corPagTest2 = null;
                }

                StopPagTest(_pagTestSlot2, ref _pagTest2Showing);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG2 clicked, play {PagTestTransitionPag}");
            if (_corPagTest2 != null && mono != null)
            {
                mono.StopCoroutine(_corPagTest2);
                _corPagTest2 = null;
            }

            StopPagTest(_pagTestSlot2, ref _pagTest2Showing);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG2 play skipped: mono is null");
                return;
            }

            _pagTest2Showing = true;
            _corPagTest2 = mono.StartCoroutine(StartPagTest2ButtonPlayback());
        }

        private void OnClickPagTest3Button()
        {
            StopPagTestGroupPlayback();

            if (_pagTest3Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG3 clicked, stop {PagTestFeiZhouPag}");
                _pagTest3Showing = false;
                if (_corPagTest3 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest3);
                    _corPagTest3 = null;
                }

                StopPagTest(_pagTestSlot3, ref _pagTest3Showing);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG3 clicked, play {PagTestFeiZhouPag}");
            if (_corPagTest3 != null && mono != null)
            {
                mono.StopCoroutine(_corPagTest3);
                _corPagTest3 = null;
            }

            StopPagTest(_pagTestSlot3, ref _pagTest3Showing);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG3 play skipped: mono is null");
                return;
            }

            _pagTest3Showing = true;
            _corPagTest3 = mono.StartCoroutine(StartPagTest3ButtonPlayback());
        }

        private void OnClickPagTest4Button()
        {
            if (_pagTest4Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG4 clicked, stop triple {PagTestCaiHongFeiDiePag}");
                StopPagTestGroupPlayback();
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG4 clicked, play triple {PagTestCaiHongFeiDiePag}");
            StopPagTestGroupPlayback();
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG4 play skipped: mono is null");
                return;
            }

            _pagTest4Showing = true;
            _corPagTest4 = PagCallbackHub.Instance.RunCoroutine(StartPagTest4ButtonPlayback());
        }

        /// <summary>预热缓存后单次 Play + repeat=-1，由 Native GPU 路径无缝循环，避免圈间重开 Play 空窗。</summary>
        private IEnumerator StartPagTest1ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestNezaPag, _pagTest1CacheWarmed,
                ok => _pagTest1CacheWarmed = ok);

            if (!_pagTest1Showing)
            {
                _corPagTest1 = null;
                yield break;
            }

            PlayPagTest(_pagTestSlot1, PagTestNezaPag, -1, PagTestBigWin1024DisplayScale);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot1);
            _corPagTest1 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest1ButtonPlayback: native loop repeat=-1");
        }

        private IEnumerator StartPagTest2ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestTransitionPag, _pagTest2CacheWarmed,
                ok => _pagTest2CacheWarmed = ok);

            if (!_pagTest2Showing)
            {
                _corPagTest2 = null;
                yield break;
            }

            PlayPagTest(_pagTestSlot2, PagTestTransitionPag, -1, PagTestBigWin1024DisplayScale);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot2);
            _corPagTest2 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest2ButtonPlayback: native loop repeat=-1");
        }

        private IEnumerator StartPagTest3ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestFeiZhouPag, _pagTest3CacheWarmed,
                ok => _pagTest3CacheWarmed = ok);

            if (!_pagTest3Showing)
            {
                _corPagTest3 = null;
                yield break;
            }

            PlayPagTest(_pagTestSlot3, PagTestFeiZhouPag, -1, PagTestFeiZhouDisplayScale,
                PagTestFeiZhouClampDisplayToHolder);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot3);
            _corPagTest3 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest3ButtonPlayback: native loop repeat=-1");
        }

        private IEnumerator StartPagTest4ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestCaiHongFeiDiePag, _pagTest4CacheWarmed,
                ok => _pagTest4CacheWarmed = ok);

            if (!_pagTest4Showing)
            {
                _corPagTest4 = null;
                yield break;
            }

            EnsurePagTestSlots();
            ConfigurePagTestGroupSlot(_pagTestSlot4, PagTestCaiHongFeiDieDisplayScale,
                PagTestCaiHongFeiDieClampDisplayToHolder);
            ConfigurePagTestGroupSlot(_pagTestSlot5, PagTestCaiHongFeiDieDisplayScale,
                PagTestCaiHongFeiDieClampDisplayToHolder);
            ConfigurePagTestGroupSlot(_pagTestSlot6, PagTestCaiHongFeiDieDisplayScale,
                PagTestCaiHongFeiDieClampDisplayToHolder);

            yield return PagGroupPlayer.PlayOnSlots(
                PagTestCaiHongFeiDiePag,
                GetPagTestGroupSlots(),
                TryBuildPagTestLayoutExtraForAnchor,
                PagTestUseFguiTexture,
                PagTestFguiMaxDisplaySide,
                PagTestFguiFps,
                PagLogPrefix,
                repeatCount: -1);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot4);

            _corPagTest4 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest4ButtonPlayback: triple sync repeat=-1");
        }

        private void OnClickSpineTest1Button()
        {
            TogglePagTestSpine(1, PagTestSpine1PlayAnim);
        }

        private void OnClickSpineTest2Button()
        {
            TogglePagTestSpine(2, PagTestSpine2PlayAnim);
        }

        private void TogglePagTestSpine(int spineIndex, string animName)
        {
            bool showing = spineIndex == 1 ? _spineTest1Showing : _spineTest2Showing;
            if (showing)
            {
                Debug.Log($"{PagLogPrefix} Spine{spineIndex} clicked, hide");
                HidePagTestSpine(spineIndex);
                return;
            }

            ShowPagTestSpine(spineIndex, animName);
        }

        private Transform FindChildRecursiveByName(Transform parent, string targetName)
        {
            if (parent == null || string.IsNullOrEmpty(targetName))
            {
                return null;
            }

            if (parent.name == targetName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildRecursiveByName(child, targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary> 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）。 </summary>
        void TryRestoreFreeSpinSession()
        {
            if (ApplicationSettings.Instance.isMock || slotMachineCtrl == null) return;
            if (!SQLitePlayerPrefs03.Instance.isInit) return;
            if (!isOpen) return;

            int pid = SBoxModel.Instance.pid;
            var snap = FreeSpinSessionStoreG1700.TryLoad(pid);
            if (snap == null) return;

            bool sessionStillValid = snap.FreeSpinTotalTimes > 0
                && (snap.FreeSpinPlayTimes < snap.FreeSpinTotalTimes
                    || (snap.FreeSpinPlayTimes == 0 && snap.NextReelStripsIndex == "FS"));
            if (!sessionStillValid)
            {
                FreeSpinSessionStoreG1700.Clear(pid);
                return;
            }

            var cm = ContentModel.Instance;
            cm.freeSpinTotalTimes = snap.FreeSpinTotalTimes;
            cm.freeSpinPlayTimes = snap.FreeSpinPlayTimes;
            cm.freeSpinTotalWinCredit = snap.FreeSpinTotalWinCredit;
            cm.curReelStripsIndex = snap.CurReelStripsIndex;
            cm.nextReelStripsIndex = snap.NextReelStripsIndex;
            cm.gameNumberFreeSpinTrigger = snap.GameNumberFreeSpinTrigger;
            cm.isFreeSpinTrigger = false;
            cm.isFreeSpinResult = false;
            cm.isFreeSpinAdd = false;
            cm.freeSpinAddNum = 0;

            if (snap.BetIndex >= 0 && SBoxModel.Instance.betList != null
                                    && snap.BetIndex < SBoxModel.Instance.betList.Count)
            {
                cm.betIndex = snap.BetIndex;
                cm.totalBet = SBoxModel.Instance.betList[cm.betIndex];
            }
            else
            {
                cm.totalBet = snap.TotalBet;
            }

            cm.betmultiple = snap.BetMultiple;
            cm.showFreeSpinRemainTime = cm.freeSpinTotalTimes - cm.freeSpinPlayTimes;
            cm.gameState = GameState.Idle;
            cm.PendingFreeSpinReconnectValidation = true;

            if (!string.IsNullOrEmpty(snap.StrDeckRowCol))
            {
                cm.strDeckRowCol = snap.StrDeckRowCol;
                slotMachineCtrl.SetReelsDeck(snap.StrDeckRowCol);
            }

            if (cm.curReelStripsIndex == "FS" || cm.nextReelStripsIndex == "FS")
            {
                ChangeBGPanel(1);
                SetUIFreeTimeBox(cm.freeSpinPlayTimes, cm.freeSpinTotalTimes);
            }


            slotMachineCtrl.SendTotalWinCreditEvent(cm.freeSpinTotalWinCredit);
            DebugUtils.Log( $"[G1700] 已恢复免费局快照：剩余 {cm.showFreeSpinRemainTime} / 总 {cm.freeSpinTotalTimes}，待首局 Spin 与算法校验。");
        }

        //普通滚动一次
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

            if (ContentModel.Instance.freeSpinTotalTimes > 0&& ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinFromReconnect(successCallback, errorCallback);
                yield break;
            }

            if (SBoxModel.Instance.myCredit < ContentModel.Instance.totalBet)
            {
                tipCoinIn = true;
                errorCallback?.Invoke("<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            // 检查余额通过后，立即扣除积分（提前扣分）
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(TotalBet, true, false);
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

            //展会模式
            if (ApplicationSettings.Instance.IsExpoMode()&&MainModel.Instance.isExhibitionModeMode)
            {
                string currentDeck = GetCurrentVisibleDeckRowCol();
                if (!string.IsNullOrEmpty(currentDeck))
                {
                    try
                    {
                        int[] deckData = SlotTool.GetDeckRowCol(currentDeck).ToArray();
                        SBoxExhibitionData sBoxExhibitionData = new SBoxExhibitionData
                        {
                            wheelChessNum = deckData.Length,
                            data = deckData
                        };
                        SBoxIdea.SetExhibitionData(sBoxExhibitionData);
                    }
                    catch (Exception e)
                    {
                        DebugUtils.LogError($"[G1700] 设置展会模式结果失败，deck={currentDeck}");
                        DebugUtils.LogException(e);
                    }
                }
            }
          

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
                // 退还之前扣除的积分
                if (ContentModel.Instance.gameState != GameState.FreeSpin)
                {
                    MainBlackboardController.Instance.AddMyTempCredit(TotalBet, true, false);
                }

                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            //检查是否启用在线彩金,请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine && ClientWS.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
            {
                RequestOnlineJackpotBetByCurrentBet();
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

            #region Free
            //免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                //显示中奖动画
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(false);
           
                //切换背景和边框
                ChangeBGPanel(1);
                SetUIFreeTimeBox(ContentModel.Instance.freeSpinPlayTimes, ContentModel.Instance.freeSpinTotalTimes);
                yield return slotMachineCtrl.SlotWaitForSeconds(2.0f);
                yield return FreeSpinTrigger(null, errorCallback);
                ChangeBGPanel(0);
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }
            #endregion

            #region Bonus

            #endregion

            #region JpOnline
            while (ContentModel.Instance.jpOnlineWin.Count > 0)
            {
                WinJackpotInfo data = ContentModel.Instance.jpOnlineWin[0];
                ContentModel.Instance.jpOnlineWin.RemoveAt(0);

                long winCredit = data.win;
                allWinCredit += winCredit;

                // 总线赢分（同步？？）
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                MainBlackboardController.Instance.AddMyTempCredit(winCredit, true, isAddCreditAnim);
            }
            #endregion


            //test核对前后端积分
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
                    if (credit != SBoxModel.Instance.myCredit)
                    {
                        DebugUtils.LogError($" 算法卡 :[0]= {credit}   前端:[0]={SBoxModel.Instance.myCredit}");
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
        //免费游戏滚动一次
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

            //免费次数UI
            SetUIFreeTimeBox(ContentModel.Instance.freeSpinPlayTimes, ContentModel.Instance.freeSpinTotalTimes);
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



                // 免费游戏中赢票栏显示累计值，不即时入余额
                slotMachineCtrl.SendTotalWinCreditEvent(ContentModel.Instance.freeSpinTotalWinCredit);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
                ContentModel.Instance.freeOnceCredit = totalWinLineCredit;


            }

            #endregion


            // 免费游戏中不逐局同步余额，等待免费结束后统一结算
            ContentModel.Instance.gameState = GameState.Idle;

            if (successCallback != null)
                successCallback.Invoke();
        }
        //请求模拟结果
        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            
            //请求结果
            MachineDataG1700Controller.Instance.RequestSlotSpinFromMock(totalBet, (res) =>
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

            ////赠送局不用扣分
            //if (ContentModel.Instance.gameState != GameState.FreeSpin)
            //{
            //    MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            //}

            // 解析数据
            MachineDataG1700Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);
           
            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }
        //请求算法结果
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
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

            //初始化本地彩金数据
            SBoxJackpotData sboxJackpotData =new SBoxJackpotData();
            sboxJackpotData.Lottery = new int[3];
            sboxJackpotData.JackpotOut = new int[3];
            sboxJackpotData.Jackpotlottery = new int[3];
            sboxJackpotData.JackpotOld = new int[3];
            //获取本地彩金贡献值
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

         
            // 解析数据
            MachineDataG1700Controller.Instance.ParseSlotSpin(TotalBet, resNode, sboxJackpotData);
            // 数据入库
            //MachineDataG1700Controller.Instance.Record();
            // ui 彩金
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }

        //下注时向大厅彩金主机发送当前下注
        void RequestOnlineJackpotBetByCurrentBet()
        {
            try
            {
                List<JackBetInfo> jackBetInfoList = new List<JackBetInfo>();

                JackBetInfo betInfo = new JackBetInfo()
                {
                    gameType = 300,
                    seat = 1,
                    bet = (int)TotalBet * 100,
                    betPercent = 100,
                    scoreRate = 1 * 1000,
                    JPPercent = 1 * 1000,
                };
                jackBetInfoList.Add(betInfo);
                NetMessageController.Instance.SendJackBet(jackBetInfoList);
            }
            catch (Exception ex)
            {

                //下注失败需要可以累计压分,最多10次
                DebugUtils.LogError($"请求大厅彩金下注失败: {ex.Message}");
            }
        }

        private readonly HashSet<long> _handledOnlineJackpotOrderIds = new HashSet<long>();
        private static string GetOnlineJackpotName(int jackpotId)
        {
            switch (jackpotId)
            {
                case 0: return "Grand";
                case 1: return "Major";
                case 2: return "Minor"; 
                case 3: return "Mini";
                default: return "Unknown";
            }
        }

        //大厅彩金主机赢分数据
        private void OnJackpotOnLine(WinJackpotInfo winInfo)
        {
            try
            {
                if (winInfo == null)
                    return;

                // 订单去重，避免重复处理
                if (_handledOnlineJackpotOrderIds.Contains(winInfo.orderId))
                    return;

                _handledOnlineJackpotOrderIds.Add(winInfo.orderId);

                // 入队给业务层后续表现/结算使用
                ContentModel.Instance.jpOnlineWin.Add(winInfo);

                // 彩金数据入库
                int jpLevel = winInfo.jackpotId + 1;
                string jpName = GetOnlineJackpotName(winInfo.jackpotId);
                long winCredit = (long)winInfo.win;
                long creditBefore = MainBlackboardController.Instance.myRealCredit;
                long creditAfter = MainBlackboardController.Instance.myRealCredit+ winCredit;
                string gameUID = string.IsNullOrEmpty(ContentModel.Instance.curGameGuid) ? "0" : ContentModel.Instance.curGameGuid;
                long createdAt = winInfo.time;
                TableJackpotRecordAsyncManager.Instance.AddJackpotRecord(jpLevel,jpName,winCredit,creditBefore,creditAfter,gameUID,createdAt);

                //通知算法卡赢得联网彩金
                SBoxWinNetJackpotInfo sBoxWinNetJackpotInfo = new SBoxWinNetJackpotInfo()
                {
                    MachineId = int.Parse(SBoxModel.Instance.MachineId),
                    PlayerId = SBoxModel.Instance.SboxPlayerAccount.PlayerId,
                    JackpotType = jpLevel,
                    JackpotWins = winCredit,
                };
                MachineDataManager02.Instance.RequestJackpotOnline(sBoxWinNetJackpotInfo,(res) =>
                {
                    //算法卡加分后同步分数
                    Debug.Log("通知算法卡赢得联网彩金");
                    JSONNode data = JSONNode.Parse((string)res);
                  
                    long creditBefore = MainBlackboardController.Instance.myRealCredit;
                    long JackpotWins = (long)data["JackpotWins"]; ;
                    creditAfter = creditBefore + JackpotWins;
                    MainBlackboardController.Instance.SetMyRealCredit(creditAfter);

                }, (BagelCodeError err) =>
                {
                    DebugUtils.Log(err.msg);
                });
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"处理大厅彩金中奖下发失败: {ex.Message}");
            }
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

            // 免费游戏结束后统一把累计赢分加到余额
            long freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            if (freeSpinTotalWinCredit > 0)
            {
                MainBlackboardController.Instance.AddMyTempCredit(freeSpinTotalWinCredit, true, isAddCreditAnim);
            }
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

        /// <summary>
        /// 断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。
        /// </summary>
        IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            yield return GameFreeSpin(null, errorCallback);

            long freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            if (freeSpinTotalWinCredit > 0)
            {
                MainBlackboardController.Instance.AddMyTempCredit(freeSpinTotalWinCredit, true, isAddCreditAnim);
            }

            ChangeBGPanel(0);
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            if (successCallback != null)
                successCallback.Invoke();
        }

        //bigwin类型
        WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = CustomModel.Instance.winLevelMultiple;
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
                "Assets/GameRes/_Common/Game Maker/ABs/G1700/Datas/game_info_g1700.json", (txt) =>
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
                    MainModel.Instance.lineNum = config.LineNum;
                });
        }

        private void OnStopSlot(EventData res)
        {

        }

        //机器按钮开始滚动
        private void OnClickSpinButton(EventData res)
        {

            if (res.name == "SpinButtonClick")
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

            if (res.name == "ColUpButtonClick")
            {
                int col = (int)res.value;
                mono.StartCoroutine(slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Up));
            }

            if (res.name == "ColDownButtonClick")
            {
                int col = (int)res.value;
                mono.StartCoroutine(slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Down));
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
                gFreeTimeBox.visible = false;
                gNormalBg.visible = true;
                gNormalGameFrame.visible = true;
                gNormalInnerFrame.visible = true;
     
            }
            else
            {
                gNormalBg.visible = false;
                gNormalGameFrame.visible = false;
                gNormalInnerFrame.visible = false;
                gFreeTimeBox.visible = true;

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

            }
            else
            {
                string massage = I18nMgr.T(msg);
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(msg));
            }
        }

        void GetMyCredit(Action<int> onSuccessCallback, Action<string> onErrorCallback)
        {

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

        protected void SetUIFreeTimeBox(int freeSpinPlayTimes, int freeSpinTotalTimes)
        {
            gFreeTimeBox.visible = true;
            gFreeTimeBox.GetChild("numberGreen").asTextField.text= freeSpinPlayTimes.ToString();
            gFreeTimeBox.GetChild("numberYellow").asTextField.text = freeSpinTotalTimes.ToString();
        }

        //读取当前滚轴显示的图标
        private string GetCurrentVisibleDeckRowCol()
        {
            if (slotMachineCtrl == null)
            {
                return string.Empty;
            }
            List<string> rows = new List<string>(slotMachineCtrl.row);
            for (int row = 0; row < slotMachineCtrl.row; row++)
            {
                List<string> cols = new List<string>(slotMachineCtrl.column);
                for (int col = 0; col < slotMachineCtrl.column; col++)
                {
                    SymbolBase symbol = slotMachineCtrl.GetVisibleSymbolFromDeck(col, row);
                    int symbolNumber = symbol != null ? symbol.GetSymbolNumber() : 0;
                    cols.Add(symbolNumber.ToString());
                }
                rows.Add(string.Join(",", cols));
            }
            return string.Join("#", rows);
        }
    }
}

