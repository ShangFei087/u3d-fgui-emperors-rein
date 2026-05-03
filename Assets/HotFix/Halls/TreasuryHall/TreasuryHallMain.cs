using FairyGUI;
using UnityEngine;
using GameMaker;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using Spine.Unity;
using System;
using System.Collections.Generic;
using PusherEmperorsRein;

namespace TreasuryHall
{
    public class TreasuryHallMain : MachinePageBase
    {
        public const string pkgName = "TreasuryHall";
        public const string resName = "TreasuryHallMain";

        private GameObject goCard3996, goCard3998, goCard3997;
        private GComponent anchorCard3996, anchorCard3998, anchorCard3997;
        private GameObject ClonegoCard3996, ClonegoCard3998, ClonegoCard3997;
        private Animator animator3996, animator3998, animator3997;
        private SkeletonMecanim _skeletonMecanim3998, _skeletonMecanim3997, _skeletonMecanim3996;
        private GButton btn3996, btn3998, btn3997;
        private GTextField hallCredit;
        private GButton btnCollect;

        //特效
        private ParticleSystem goplat_card_cfzj, goplat_card_cfhc1, goplat_card_cfhc2, goplat_card_cfhc3;


        private bool IsClickCard;

        // ---------- 点击卡牌 → 子游戏 Loading 的并行过渡状态 ----------
        /// <summary> 是否处于「点击卡牌进游戏」过渡中；为 false 时异步回调应忽略或只做收尾（如关掉多余的 Loading）。 </summary>
        private bool _cardEnterFlowActive;
        /// <summary> PopupGameLoading 是否已通过 OpenPage 完成 Show + OnOpen（含资源包异步加载完成后的那条路径）。 </summary>
        private bool _cardEnterLoadingReady;
        /// <summary> 卡牌 click 动画是否已按时长播完（Timer 到期）。 </summary>
        private bool _cardEnterClickFinished;
        /// <summary> 当前过渡对应的目标 Loading 页面枚举，用于中止时关闭已弹出的 Loading。 </summary>
        private PageName _cardEnterLoadingPage;
        /// <summary> 目标机台游戏 ID，提交时用于 GameSwitch。 </summary>
        private int _cardEnterGameId;
        /// <summary> 当前点击的那张卡牌上的 Animator，用于 click 结束后在未打开 Loading 时切 idle。 </summary>
        private Animator _cardEnterAnimator;

        //彩金
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();
        protected override void OnInit()
        {

            base.OnInit();

            int count = 3;
           
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/TreasuryHall/Prefabs/card/card_3996",
               (GameObject clone) =>
               {
                   goCard3996 = clone;
                   callback();
               });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/TreasuryHall/Prefabs/card/card_3998",
            (GameObject clone) =>
            {
                goCard3998 = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/TreasuryHall/Prefabs/card/card_3997",
                (GameObject clone) =>
                {
                    goCard3997 = clone;
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：BtnTicketOut");
                        //EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnClickBtnTicketOut();
                    },
                },

            };
        }

        /// <summary>
        /// 语言切换时重建页面，确保多语言文案与皮肤控制器状态同步刷新。
        /// </summary>
        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose();
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            isInit = true;
            InitParam();
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            // 添加事件监听 - 彩金贡献值
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            InitParam();
        }

        /// <summary>
        /// 进入宝库后预载 3996 / 3997 / 3998 的 Loading 页：创建隐藏实例并触发各自资源加载，与后续 OpenPage 共用缓存实例。
        /// </summary>
        private static void PreloadTreasuryCardGameLoadingPages()
        {
            PageManager.Instance.PreloadPage(PageName.CaiFuHuoChePopupGameLoading, null);
            PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupGameLoading, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupGameLoading, null);
        }

        public override void OnClose(EventData data = null)
        {
            // 关闭宝库大厅前先中止「进游戏」过渡，避免卡在不可点状态或残留 Loading
            AbortCardEnterTransitionIfAny();
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            GameSoundHelper.Instance.StopMusic();
            base.OnClose(data);
        }

        public override void InitParam()
        {
            IsClickCard = true;
            if (!isInit) return;

            if (!isOpen) return;

            GComponent LocalCard3998 = this.contentPane.GetChild("card3998").asCom;
            if (anchorCard3998 != LocalCard3998)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3998);
                ClonegoCard3998 = GameObject.Instantiate(goCard3998);
                // 卡牌预制体层级可能调整，使用全子节点查找 Animator 更稳
                animator3998 = ClonegoCard3998.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                _skeletonMecanim3998 = ClonegoCard3998.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorCard3998 = LocalCard3998;
                GameCommon.FguiUtils.AddWrapper(anchorCard3998, ClonegoCard3998);

            }

            GComponent LocalCard3997 = this.contentPane.GetChild("card3997").asCom;
            if (anchorCard3997 != LocalCard3997)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3997);
                ClonegoCard3997 = GameObject.Instantiate(goCard3997);
                animator3997 = ClonegoCard3997.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                _skeletonMecanim3997 = ClonegoCard3997.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorCard3997 = LocalCard3997;
                GameCommon.FguiUtils.AddWrapper(anchorCard3997, ClonegoCard3997);
                //绑定特效
                string Paths = $"Anchor/Spine Mecanim GameObject (plat_card_cfzj)/SkeletonUtility-SkeletonRoot/root/zhijia/plat_card_cfzj/effect";
                //Transform pathTransform = npcObject.transform.Find(candidatePaths);
                goplat_card_cfzj= ClonegoCard3997.transform.Find(Paths).gameObject.GetComponent<ParticleSystem>();
              
            }

            GComponent LocalCard3996 = this.contentPane.GetChild("card3996").asCom;
            if (anchorCard3996 != LocalCard3996)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3996);
                ClonegoCard3996 = GameObject.Instantiate(goCard3996);
                animator3996 = ClonegoCard3996.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                _skeletonMecanim3996 = ClonegoCard3996.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorCard3996 = LocalCard3996;
                GameCommon.FguiUtils.AddWrapper(anchorCard3996, ClonegoCard3996);
                //绑定特效
                string Paths = $"Anchor/Spine Mecanim GameObject (plat_card_cfhc)/SkeletonUtility-SkeletonRoot/root/zong/All/che/BIG_che/plat_card_cfhc1/effect1";
                //Transform pathTransform = npcObject.transform.Find(candidatePaths);
                goplat_card_cfhc1 = ClonegoCard3996.transform.Find(Paths).gameObject.GetComponent<ParticleSystem>();

                Paths = $"Anchor/Spine Mecanim GameObject (plat_card_cfhc)/SkeletonUtility-SkeletonRoot/root/zong/All/che/BIG_che/plat_card_cfhc2/effect2";
                goplat_card_cfhc2 = ClonegoCard3996.transform.Find(Paths).gameObject.GetComponent<ParticleSystem>();

                Paths = $"Anchor/Spine Mecanim GameObject (plat_card_cfhc)/SkeletonUtility-SkeletonRoot/root/zong/All/che/BIG_che/plat_card_cfhc2/effect3";
                goplat_card_cfhc3 = ClonegoCard3996.transform.Find(Paths).gameObject.GetComponent<ParticleSystem>();
              
            }

          
            //清除所有粒子
            if (goplat_card_cfhc3 != null && goplat_card_cfhc2 != null && goplat_card_cfhc1 != null && goplat_card_cfhc3 != null)
            {
                // 停止粒子特效  清除所有粒子
                goplat_card_cfzj.Stop();
                goplat_card_cfzj.Clear();

                goplat_card_cfhc1.Stop();
                goplat_card_cfhc1.Clear();

                goplat_card_cfhc2.Stop();
                goplat_card_cfhc2.Clear();

                goplat_card_cfhc3.Stop();
                goplat_card_cfhc3.Clear();
            }

            // 点击卡牌：先播 click；同时 StartCardGameEnter 内会 OpenPage(Loading)，与动画并行
            btn3998 = this.contentPane.GetChild("card3998").asCom.GetChild("btnCard").asButton;
            btn3998.onClick.Clear();
            btn3998.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.TLClickGame);
                   IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    float clickAnimDuration = PlayCardClickAnimation(animator3998);
                    StartCardGameEnter(animator3998, PageName.XingYunZhiLunPopupGameLoading, 3998, clickAnimDuration);
                }
            });

            btn3997 = this.contentPane.GetChild("card3997").asCom.GetChild("btnCard").asButton;
            btn3997.onClick.Clear();
            btn3997.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.TLClickGame);
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    goplat_card_cfzj.Play();
                    float clickAnimDuration = PlayCardClickAnimation(animator3997);
                    StartCardGameEnter(animator3997, PageName.CaiFuZhiJiaPopupGameLoading, 3997, clickAnimDuration);
                }
            });

            btn3996 = this.contentPane.GetChild("card3996").asCom.GetChild("btnCard").asButton;
            btn3996.onClick.Clear();
            btn3996.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.TLClickGame);
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    // 播放粒子特效
                    goplat_card_cfhc1.Play();
                    goplat_card_cfhc2.Play();
                    goplat_card_cfhc3.Play();
                    float clickAnimDuration = PlayCardClickAnimation(animator3996);
                    StartCardGameEnter(animator3996, PageName.CaiFuHuoChePopupGameLoading, 3996, clickAnimDuration);
                }
            });

            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            btnCollect = this.contentPane.GetChild("btnCollect").asButton;
            btnCollect.onClick.Clear();
            btnCollect.onClick.Add(() =>
            {
                OnClickBtnTicketOut();
            });
            btnCollect.touchable = true;

            hallCredit = this.contentPane.GetChild("Credit").asTextField;

            RefreshCardSkinByLanguage();
            InitJackpot();
            InitHallCredit();
            // 后台预热三张卡牌对应子游戏的 PopupGameLoading（含各自包内预制体异步加载），缩短首次点击后的等待
            PreloadTreasuryCardGameLoadingPages();
        }

        /// <summary>
        /// 根据当前语言刷新大厅卡牌动画的 initial skin。
        /// </summary>
        private void RefreshCardSkinByLanguage()
        {
            ApplySpineInitialSkinByLanguage(_skeletonMecanim3998);
            ApplySpineInitialSkinByLanguage(_skeletonMecanim3997);
            ApplySpineInitialSkinByLanguage(_skeletonMecanim3996);
        }

        /// <summary>
        /// 按语言设置 Spine 动画 initial skin，并立即刷新到当前骨骼实例。
        /// </summary>
        private void ApplySpineInitialSkinByLanguage(SkeletonMecanim skeletonMecanim)
        {
            if (skeletonMecanim == null)
            {
                return;
            }

            string skinName = SBoxModel.Instance.language == "en" ? "en" : "cn";
            skeletonMecanim.initialSkinName = skinName;
            if (skeletonMecanim.Skeleton == null)
            {
                skeletonMecanim.Initialize(true);
                return;
            }

            skeletonMecanim.Skeleton.SetSkin(skinName);
            skeletonMecanim.Skeleton.SetSlotsToSetupPose();
            skeletonMecanim.LateUpdate();
        }

        /// <summary>
        /// 开始「点击卡牌进子游戏」过渡（调用前应先播放 click 动画）。
        /// 关键步骤：① 立刻 OpenPage 打开对应 PopupGameLoading（与卡牌动画并行，首包未加载时会在异步回调里才真正 Show）；② 用 Timer 记录 click 时长；
        /// ③ Loading 打开完成且 click 时长都满足后，由 TryFinishCardEnterTransition → CommitAfterLoadingPageVisible 执行 GameSwitch 并关闭宝库大厅。
        /// </summary>
        private void StartCardGameEnter(Animator cardAnimator, PageName loadingPage, int gameId, float clickAnimDurationSeconds)
        {
            // 重置过渡标志与上下文
            _cardEnterFlowActive = true;
            _cardEnterLoadingReady = false;
            _cardEnterClickFinished = false;
            _cardEnterAnimator = cardAnimator;
            _cardEnterLoadingPage = loadingPage;
            _cardEnterGameId = gameId;

            // 与卡牌 click 同时发起打开 Loading（同步路径会立刻回调；AB 异步路径则等资源就绪后再回调）
            //PageManager.Instance.OpenPage(loadingPage, null, OnCardEnterLoadingOpenFinished);
            
            //按 click 片段时长计时，到期表示「卡牌点击动画阶段」结束
            float wait = clickAnimDurationSeconds > 0f ? clickAnimDurationSeconds : 0.15f;
            Timers.inst.Add(wait, 1, (object _) => { PageManager.Instance.OpenPage(loadingPage, null, OnCardEnterLoadingOpenFinished); });
            Timers.inst.Add(wait, 1, OnCardClickAnimationTimeUp);
        }

        /// <summary>
        /// OpenPage(Loading) 完成后的回调（page 非 null 表示 Show + OnOpen 已走完）。
        /// 若打开失败（page == null）且仍在过渡中：中止并恢复可点。
        /// 若大厅已中止过渡但 Loading 晚到：关掉孤儿 Loading，避免界面叠乱。
        /// </summary>
        private void OnCardEnterLoadingOpenFinished(PageBase page)
        {
            // 打开失败：路径错误、重复打开等 → 结束过渡，恢复大厅交互
            if (page == null)
            {
                if (_cardEnterFlowActive)
                {
                    AbortCardEnterTransitionIfAny();
                }

                return;
            }

            // 过渡已取消（例如大厅提前关闭），但 Loading 这次才真正打开 → 关掉多余 Loading
            if (!_cardEnterFlowActive)
            {
                PageManager.Instance.ClosePage(page, null);
                return;
            }

            // 标记 Loading 已就绪，若 click 计时也已结束则一并提交
            _cardEnterLoadingReady = true;
            TryFinishCardEnterTransition();
        }

        /// <summary>
        /// 卡牌 click 动画计时结束（与 Loading 是否打开无关）。
        /// 若此时 Loading 仍未打开：播放 idle，用户在画面上等待直到 OnCardEnterLoadingOpenFinished 触发后再一并提交。
        /// </summary>
        private void OnCardClickAnimationTimeUp(object _)
        {
            if (!_cardEnterFlowActive)
            {
                return;
            }

            _cardEnterClickFinished = true;
            // Loading 还没到：用 idle 衔接等待，避免停在 click 最后一帧观感生硬
            if (!_cardEnterLoadingReady)
            {
                PlayCardIdleAnimation(_cardEnterAnimator);
            }

            TryFinishCardEnterTransition();
        }

        /// <summary>
        /// 仅当「Loading 已打开」且「click 计时已结束」同时成立时，提交 GameSwitch 并关闭宝库大厅。
        /// </summary>
        private void TryFinishCardEnterTransition()
        {
            if (!_cardEnterFlowActive || !_cardEnterClickFinished || !_cardEnterLoadingReady)
            {
                return;
            }

            _cardEnterFlowActive = false;
            CommitAfterLoadingPageVisible();
        }

        /// <summary>
        /// 提交进入游戏：Loading 界面已在 StartCardGameEnter 里 OpenPage 打开，这里只做切机台协议与关闭当前宝库大厅页面。
        /// </summary>
        private void CommitAfterLoadingPageVisible()
        {
            if (!ApplicationSettings.Instance.isMock)
            {
                SBoxIdea.GameSwitch(_cardEnterGameId);
            }

            CloseSelf(null);
        }

        /// <summary>
        /// 中止进行中的「点击进游戏」过渡：恢复卡牌与收集按钮可点；若 Loading 已经弹出则一并关闭。
        /// </summary>
        private void AbortCardEnterTransitionIfAny()
        {
            if (!_cardEnterFlowActive)
            {
                return;
            }

            bool loadingAlreadyVisible = _cardEnterLoadingReady;
            PageName loadingPageToClose = _cardEnterLoadingPage;

            _cardEnterFlowActive = false;
            _cardEnterLoadingReady = false;
            _cardEnterClickFinished = false;
            IsClickCard = true;
            if (btnCollect != null)
            {
                btnCollect.touchable = true;
            }

            // 若 Loading 已经显示，关掉以免挡住大厅（用户未完成进入游戏）
            if (loadingAlreadyVisible)
            {
                PageManager.Instance.ClosePage(loadingPageToClose, null);
            }
        }

        /// <summary>
        /// 播放卡牌 click 动画并返回动画时长（秒），用于延时执行跳转逻辑。
        /// </summary>
        private float PlayCardClickAnimation(Animator animator)
        {
            if (animator == null)
            {
                return 0.15f;
            }

            if (animator.HasState(0, Animator.StringToHash("click")))
            {
                animator.Play("click", 0, 0f);
                float duration = GetAnimationClipLength(animator, "click");
                return duration > 0f ? duration : 0.15f;
            }

            return 0.15f;
        }

        /// <summary>
        /// 在 click 计时已结束但 PopupGameLoading 尚未打开时调用：切换到 idle 状态，直到 Loading 打开。
        /// Animator 需存在名为 idle 的状态；若无则保持当前姿态不变。
        /// </summary>
        private void PlayCardIdleAnimation(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            if (animator.HasState(0, Animator.StringToHash("idle")))
            {
                animator.Play("idle", 0, 0f);
            }
        }

        /// <summary>
        /// 获取指定动画片段时长；未找到时返回 0。
        /// </summary>
        private float GetAnimationClipLength(Animator animator, string clipName)
        {
            if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(clipName))
            {
                return 0f;
            }

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip clip = clips[i];
                if (clip != null && clip.name == clipName)
                {
                    return clip.length;
                }
            }

            return 0f;
        }

        public void InitJackpot()
        {
            if (ApplicationSettings.Instance.isMock)
            {
                uiJPMajorCtrl.SetData(30000);
                uiJPMinorCtrl.SetData(1000);
                uiJPMiniCtrl.SetData(500);
            }
            else
            {
                //获取彩金贡献值
                ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
                {
                   
                    JSONNode data = JSONNode.Parse((string)res);
                    Debug.Log(data);
                    int code = (int)data["code"];
                    if (0 != code)
                    {
                        DebugUtils.LogError($"大厅请求贡献值报错。 code: {code}");
                        return;
                    }

                    int majorBet = (int)data["major"];
                    int minorBet = (int)data["minor"];
                    int miniBet = (int)data["mini"];

                    uiJPMajorCtrl.SetData(majorBet);
                    uiJPMinorCtrl.SetData(minorBet);
                    uiJPMiniCtrl.SetData(miniBet);

                });
            }
        }

        //积分监听
        protected virtual void OnUpdateNaviCredit(EventData receivedEvent = null)
        {

            bool isAmin = false;
            long fromCredit = 0;
            long toCredit = 0;
            if (receivedEvent == null || receivedEvent.value == null)
            {
                isAmin = false;
                toCredit = MainBlackboardController.Instance.myTempCredit;
            }
            else
            {
                UpdateNaviCredit data = (UpdateNaviCredit)receivedEvent.value;

                isAmin = data.isAnim;
                fromCredit = data.fromCredit;
                toCredit = data.toCredit;
            }


            if (isAmin)
            {
                NumberAnimation.Instance.AnimateNumber(hallCredit, fromCredit, toCredit);
            }
            else
            {
                NumberAnimation.Instance.PauseTextFieldAnimation(hallCredit);
                if (hallCredit != null)
                {
                    hallCredit.text = toCredit.ToString();
                }
            }
        }

        public void InitHallCredit()
        {
            //初始化积分与同步
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
                        MainBlackboardController.Instance.SyncMyTempCreditToReal(false);
                        hallCredit.text = playerAccountList[i].Credit.ToString();
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {

                DebugUtils.Log(err.msg);
            });
            // hallCredit.text = MainBlackboardController.Instance.myRealCredit.ToString();
        }

        public void GameSwitch(int gameid)
        {
            
        }

        private void OnClickBtnTicketOut()
        {
            MachineDeviceCommonBiz.Instance.TestTicketOut();
        }
    }

}
