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

        public override void OnClose(EventData data = null)
        {
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
                // 停止粒子特效
                goplat_card_cfzj.Stop();
                // 清除所有粒子
                goplat_card_cfzj.Clear();
                goplat_card_cfhc1.Stop();
                goplat_card_cfhc1.Clear();
                goplat_card_cfhc2.Stop();
                goplat_card_cfhc2.Clear();
                goplat_card_cfhc3.Stop();
                goplat_card_cfhc3.Clear();
            }

            // 点击卡牌：先播 click 动画，再按动画时长延时进游戏
            btn3998 = this.contentPane.GetChild("card3998").asCom.GetChild("btnCard").asButton;
            btn3998.onClick.Clear();
            btn3998.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    float clickAnimDuration = PlayCardClickAnimation(animator3998);
                    Timers.inst.Add(clickAnimDuration, 1, (obj) => EnterGame3998());
                }
            });

            btn3997 = this.contentPane.GetChild("card3997").asCom.GetChild("btnCard").asButton;
            btn3997.onClick.Clear();
            btn3997.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    goplat_card_cfzj.Play();
                    float clickAnimDuration = PlayCardClickAnimation(animator3997);
                    Timers.inst.Add(clickAnimDuration, 1, (obj) => EnterGame3997());
                }
            });

            btn3996 = this.contentPane.GetChild("card3996").asCom.GetChild("btnCard").asButton;
            btn3996.onClick.Clear();
            btn3996.onClick.Add(() =>
            {
                if (IsClickCard)
                {
                    IsClickCard = !IsClickCard;
                    btnCollect.touchable = false;
                    // 播放粒子特效
                    goplat_card_cfhc1.Play();
                    goplat_card_cfhc2.Play();
                    goplat_card_cfhc3.Play();
                    float clickAnimDuration = PlayCardClickAnimation(animator3996);
                    Timers.inst.Add(clickAnimDuration, 1, (obj) => EnterGame3996());
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
        /// 执行 3998 游戏切换与页面跳转（在点击动画播放完成后调用）。
        /// </summary>
        private void EnterGame3998()
        {
            if (!ApplicationSettings.Instance.isMock)
            {
                SBoxIdea.GameSwitch(3998);
            }
            PageManager.Instance.OpenPage(PageName.XingYunZhiLunPopupGameLoading);

            CloseSelf(null);
        }

        /// <summary>
        /// 执行 3997 游戏切换与页面跳转（在点击动画播放完成后调用）。
        /// </summary>
        private void EnterGame3997()
        {
            if (!ApplicationSettings.Instance.isMock)
            {
                SBoxIdea.GameSwitch(3997);
            }
            PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupGameLoading);

            CloseSelf(null);
        }

        /// <summary>
        /// 执行 3996 游戏切换与页面跳转（在点击动画播放完成后调用）。
        /// </summary>
        private void EnterGame3996()
        {
            if (!ApplicationSettings.Instance.isMock)
            {
                SBoxIdea.GameSwitch(3996);
            }
            PageManager.Instance.OpenPage(PageName.CaiFuHuoChePopupGameLoading);

            CloseSelf(null);
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
