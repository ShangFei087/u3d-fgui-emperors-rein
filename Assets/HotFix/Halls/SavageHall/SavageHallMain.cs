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

namespace SavageHall
{
    public class SavageHallMain : MachinePageBase
    {
        public const string pkgName = "SavageHall";
        public const string resName = "SavageHallMain";

        private GameObject goCard3995, goCard3994, goCard3993;
        private GComponent anchorCard3995, anchorCard3994, anchorCard3993;
        private GameObject ClonegoCard3995, ClonegoCard3994, ClonegoCard3993;
        private Animator animator3995, animator3994, animator3993; //卡牌动画
        private SkeletonMecanim _skeletonMecanim3995, _skeletonMecanim3994, _skeletonMecanim3993;
        private Animator animatorChlick3995, animatorChlick3994, animatorChlick3993;  //卡牌点击特效动画
        private GButton btn3995, btn3994, btn3993;
        private GTextField hallCredit;
        private GButton btnCollect;
        private bool IsClickCard;

        private GameObject goHallLogoTitle, goHallLogoBG;
        private GComponent anchorHallLogoTitle, anchorHallLogoBG;
        private GameObject ClonegoHallLogoTitle, ClonegoHallLogoBG;
        private ParticleSystem goHallLogoTitle_cn, goHallLogoTitle_en;
        private ParticleSystem goHallLogoBG_cn, goHallLogoBG_en;

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

            int count = 5;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/SavageHall/Prefabs/Card/Card3995",
               (GameObject clone) =>
               {
                   goCard3995 = clone;
                   callback();
               });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/SavageHall/Prefabs/Card/Card3994",
            (GameObject clone) =>
            {
                goCard3994 = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/SavageHall/Prefabs/Card/Card3993",
                (GameObject clone) =>
                {
                    goCard3993 = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/SavageHall/Prefabs/HallLogo/HallLogoTitle",
                (GameObject clone) =>
                {
                    goHallLogoTitle = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/SavageHall/Prefabs/HallLogo/HallLogoBG",
                (GameObject clone) =>
                {
                    goHallLogoBG = clone;
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
            //GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            // 添加事件监听 - 彩金贡献值
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            InitParam();
        }

        /// <summary>
        /// 并行预载三张卡牌对应子游戏的 PopupGameLoading，全部就绪后再打开宝库大厅（启动、从子游戏返回等入口应调用本方法而非直接 OpenPage）。
        /// </summary>
        public static void OpenTreasuryHallMainAfterCardGameLoadingPreloads()
        {
            const int total = 3;
            int completed = 0;
            void OnOnePreloadDone()
            {
                completed++;
                if (completed < total)
                {
                    return;
                }
                PageLaunch.Instance.Close(2f);
                PageManager.Instance.OpenPage(PageName.SavageHallMain);
            }
            PageLaunch.Instance.Close(2f);
            PageManager.Instance.OpenPage(PageName.SavageHallMain);

            PageManager.Instance.PreloadPage(PageName.HuoYanGongNiuPopupGameLoading, OnOnePreloadDone);
            PageManager.Instance.PreloadPage(PageName.FeiZhouHeiXingXingPopupGameLoading, OnOnePreloadDone);
            PageManager.Instance.PreloadPage(PageName.MeiZhouHeiBaoPopupGameLoading, OnOnePreloadDone);
        }

        public override void OnClose(EventData data = null)
        {
            // 关闭宝库大厅前先中止「进游戏」过渡，避免卡在不可点状态或残留 Loading
            AbortCardEnterTransitionIfAny();
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            GameSoundHelper.Instance.StopMusic();
            DisposeCardWrappers();
            base.OnClose(data);
        }

        /// <summary>
        /// 关闭时释放卡牌 Spine GoWrapper
        /// </summary>
        private void DisposeCardWrappers()
        {
            GameCommon.FguiUtils.DeleteWrapper(anchorCard3995);
            GameCommon.FguiUtils.DeleteWrapper(anchorCard3993);
            GameCommon.FguiUtils.DeleteWrapper(anchorCard3994);
            GameCommon.FguiUtils.DeleteWrapper(anchorHallLogoTitle);
            GameCommon.FguiUtils.DeleteWrapper(anchorHallLogoBG);

            ClonegoCard3995 = null;
            ClonegoCard3993 = null;
            ClonegoCard3994 = null;
            ClonegoHallLogoTitle = null;
            ClonegoHallLogoBG = null;
            animator3995 = null;
            animator3993 = null;
            animator3994 = null;
            animatorChlick3995 = null;
            animatorChlick3994 = null;
            animatorChlick3993 = null;
            _skeletonMecanim3993 = null;
            _skeletonMecanim3994 = null;
            _skeletonMecanim3995 = null;
            anchorCard3995 = null;
            anchorCard3993 = null;
            anchorCard3994 = null;
            anchorHallLogoTitle = null;
            anchorHallLogoBG = null;
        }

        static void StopAndClearParticle(ParticleSystem particle)
        {
            if (particle == null)
                return;

            particle.Stop();
            particle.Clear();
        }

        public override void InitParam()
        {
            IsClickCard = true;
            if (!isInit) return;
            if (!isOpen) return;

            GComponent LocalLogoTitle= this.contentPane.GetChild("anchorLogoTitle").asCom;
            if (anchorHallLogoTitle != LocalLogoTitle)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorHallLogoTitle);
                ClonegoHallLogoTitle = GameObject.Instantiate(goHallLogoTitle);
                anchorHallLogoTitle = LocalLogoTitle;
                goHallLogoTitle_cn = ClonegoHallLogoTitle.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
                goHallLogoTitle_en = ClonegoHallLogoTitle.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
                GameCommon.FguiUtils.AddWrapper(anchorHallLogoTitle, ClonegoHallLogoTitle);
            }

            GComponent LocalLogoBG = this.contentPane.GetChild("anchorLogoBG").asCom;
            if (anchorHallLogoBG != LocalLogoBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorHallLogoBG);
                ClonegoHallLogoBG = GameObject.Instantiate(goHallLogoBG);
                anchorHallLogoBG = LocalLogoBG;
                goHallLogoBG_cn = ClonegoHallLogoBG.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
                goHallLogoBG_en = ClonegoHallLogoBG.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
                GameCommon.FguiUtils.AddWrapper(anchorHallLogoBG, ClonegoHallLogoBG);
            }


            GComponent LocalCard3995 = this.contentPane.GetChild("card3995").asCom;
            if (anchorCard3995 != LocalCard3995)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3995);
                ClonegoCard3995 = GameObject.Instantiate(goCard3995);
                animator3995 = ClonegoCard3995.transform.GetChild(0).GetChild(1).GetComponent<Animator>();
                _skeletonMecanim3993 = ClonegoCard3995.transform.GetChild(0).GetChild(1).GetComponent<SkeletonMecanim>();
                animatorChlick3995 = ClonegoCard3995.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                HideCardClickEffect(animatorChlick3995);
                anchorCard3995 = LocalCard3995;
                GameCommon.FguiUtils.AddWrapper(anchorCard3995, ClonegoCard3995);
            }

            GComponent LocalCard3994 = this.contentPane.GetChild("card3994").asCom;
            if (anchorCard3994 != LocalCard3994)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3994);
                ClonegoCard3994 = GameObject.Instantiate(goCard3994);
                animator3994 = ClonegoCard3994.transform.GetChild(0).GetChild(1).GetComponent<Animator>();
                _skeletonMecanim3995 = ClonegoCard3994.transform.GetChild(0).GetChild(1).GetComponent<SkeletonMecanim>();
                animatorChlick3994 = ClonegoCard3994.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                HideCardClickEffect(animatorChlick3994);
                anchorCard3994 = LocalCard3994;
                GameCommon.FguiUtils.AddWrapper(anchorCard3994, ClonegoCard3994);

            }

            GComponent LocalCard3993 = this.contentPane.GetChild("card3993").asCom;
            if (anchorCard3993 != LocalCard3993)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3993);
                ClonegoCard3993 = GameObject.Instantiate(goCard3993);
                animator3993 = ClonegoCard3993.transform.GetChild(0).GetChild(1).GetComponent<Animator>();
                _skeletonMecanim3993 = ClonegoCard3993.transform.GetChild(0).GetChild(1).GetComponent<SkeletonMecanim>();
                animatorChlick3993 = ClonegoCard3993.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                HideCardClickEffect(animatorChlick3993);

                anchorCard3993 = LocalCard3993;
                GameCommon.FguiUtils.AddWrapper(anchorCard3993, ClonegoCard3993);
            }


            // 点击卡牌：先播 click；同时 StartCardGameEnter 内会 OpenPage(Loading)，与动画并行
            btn3995 = this.contentPane.GetChild("card3995").asCom.GetChild("btnCard").asButton;
            btn3995.onClick.Clear();
            btn3995.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    //GameSoundHelper.Instance.PlaySoundEff(SoundKey.TLClickGame);
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;

                    PlayCardClickEffect(animatorChlick3995);
                    float clickAnimDuration = PlayCardClickAnimation(animator3995);
                    StartCardGameEnter(animator3995, PageName.HuoYanGongNiuPopupGameLoading, 3996, clickAnimDuration);
                }
            });

            btn3994 = this.contentPane.GetChild("card3994").asCom.GetChild("btnCard").asButton;
            btn3994.onClick.Clear();
            btn3994.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    //GameSoundHelper.Instance.PlaySoundEff(SoundKey.TLClickGame);
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    PlayCardClickEffect(animatorChlick3994);
                    float clickAnimDuration = PlayCardClickAnimation(animator3994);
                    StartCardGameEnter(animator3994, PageName.FeiZhouHeiXingXingPopupGameLoading, 3998, clickAnimDuration);
                }
            });

            btn3993 = this.contentPane.GetChild("card3993").asCom.GetChild("btnCard").asButton;
            btn3993.onClick.Clear();
            btn3993.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    //GameSoundHelper.Instance.PlaySoundEff(SoundKey.TLClickGame);
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    PlayCardClickEffect(animatorChlick3993);
                    float clickAnimDuration = PlayCardClickAnimation(animator3993);
                    StartCardGameEnter(animator3993, PageName.MeiZhouHeiBaoPopupGameLoading, 3997, clickAnimDuration);
                }
            });

            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.SetReelWidth(64);
            uiJPMinorCtrl.SetReelWidth(65);
            uiJPMinorCtrl.SetReelWidth(54);

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
            RefreshHallLogoByLanguage();
        }

        /// <summary>
        /// 根据当前语言刷新大厅logo
        /// </summary>
        private void RefreshHallLogoByLanguage()
        {
            bool isEn = SBoxModel.Instance.language == "en";
            if (goHallLogoTitle_cn != null) goHallLogoTitle_cn.gameObject.SetActive(!isEn);
            if (goHallLogoTitle_en != null) goHallLogoTitle_en.gameObject.SetActive(isEn);
            if (goHallLogoBG_cn != null) goHallLogoBG_cn.gameObject.SetActive(!isEn);
            if (goHallLogoBG_en != null) goHallLogoBG_en.gameObject.SetActive(isEn);
        }

        /// <summary>
        /// 根据当前语言刷新大厅卡牌待机动画（en: idle，cn: idle_cn）。
        /// </summary>
        private void RefreshCardSkinByLanguage()
        {
            PlayCardIdleAnimation(animator3995);
            PlayCardIdleAnimation(animator3994);
            PlayCardIdleAnimation(animator3993);
        }

        private bool IsEnglishLang()
        {
            return SBoxModel.Instance.language == "en";
        }

        private string GetCardIdleStateName()
        {
            return IsEnglishLang() ? "idle" : "idle_cn";
        }

        private string GetCardClickStateName()
        {
            return IsEnglishLang() ? "click" : "click_cn";
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
        /// 进大厅时隐藏点击特效（仅点击卡牌时再激活播放）。
        /// </summary>
        private void HideCardClickEffect(Animator clickAnimator)
        {
            if (clickAnimator == null)
            {
                return;
            }

            clickAnimator.gameObject.SetActive(false);
        }

        /// <summary>
        /// 激活并播放卡牌点击特效 Animator（无中英文分支，固定 click）。
        /// </summary>
        private void PlayCardClickEffect(Animator clickAnimator)
        {
            if (clickAnimator == null)
            {
                return;
            }

            clickAnimator.gameObject.SetActive(true);
            clickAnimator.enabled = true;
            clickAnimator.speed = 1f;
            if (clickAnimator.HasState(0, Animator.StringToHash("click")))
            {
                clickAnimator.Play("click", 0, 0f);
            }
        }

        /// <summary>
        /// 按语言播放卡牌 click / click_cn，并返回动画时长（秒）。
        /// </summary>
        private float PlayCardClickAnimation(Animator animator)
        {
            if (animator == null)
            {
                return 0.15f;
            }

            string stateName = GetCardClickStateName();
            if (animator.HasState(0, Animator.StringToHash(stateName)))
            {
                animator.speed = 1f;
                animator.Play(stateName, 0, 0f);
                float duration = GetAnimationClipLength(animator, stateName);
                return duration > 0f ? duration : 0.15f;
            }

            return 0.15f;
        }

        /// <summary>
        /// 按语言播放卡牌 idle / idle_cn；Loading 等待衔接时也会调用。
        /// </summary>
        private void PlayCardIdleAnimation(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            string stateName = GetCardIdleStateName();
            if (animator.HasState(0, Animator.StringToHash(stateName)))
            {
                animator.speed = 1f;
                animator.Play(stateName, 0, 0f);
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
