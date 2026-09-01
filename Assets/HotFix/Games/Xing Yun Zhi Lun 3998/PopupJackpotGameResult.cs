using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace XingYunZhiLun_3998
{
    public class PopupJackpotGameResult : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupJackpotGameResult";
        private GComponent goEffect;

        //结束时特效
        //private Transform caidai, drop, liPao1, liPao2;

        private bool isInit = false;
        private bool isend;
        private EventData _data;
        private MiniReelGroup uiCreditCtrl = new MiniReelGroup();
        private GComponent credit, lodEffect;
        private GButton gbutton;
        private GLoader btnLoader;

        private GameObject goEff, goAnchorSpineEff;

        //大奖动画预制体
        private GameObject goFgCloneGrand, goFgCloneMajor, goFgCloneMinor, goFgCloneMini, go;
        private Animator spineAnimator, effAnimator;
        private bool isClose;

        Action jackpotAction;
        float sorce;
        int jackpotType;
        List<float> jpCredit = new List<float> { };


        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        //Pag播放
        private const string GamePagFolder = "Games/Xing Yun Zhi Lun 3998/Pag";
        private PagSlotBinding effectPag;
        private readonly string[] effectName = { "mini_idle.pag", "minor_idle.pag", "major_idle.pag" };

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

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotGrand.prefab",
                (GameObject clone) =>
                {
                    goFgCloneGrand = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotMajor.prefab",
                (GameObject clone) =>
                {
                    goFgCloneMajor = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotMinor.prefab",
                (GameObject clone) =>
                {
                    goFgCloneMinor = clone;
                    callback();
                    });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotMini.prefab",
                (GameObject clone) =>
                {
                    goFgCloneMini = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupFreeSpinResult/FreeSpinResultEff.prefab",
            (GameObject clone) =>
            {
                goAnchorSpineEff = clone;
                callback();
            });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput) return;

                        DebugUtils.LogError("游戏接受到机台短按的数据：Spin");
                        SpinDown();
                    }
                },
            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam(data);
            if (GameSoundHelper.Instance.IsPlaySound(SoundKey.RegularBG))
            {
                GameSoundHelper.Instance.StopSound(SoundKey.RegularBG);
            }
            if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeSpinBG))
            {
                GameSoundHelper.Instance.StopSound(SoundKey.FreeSpinBG);
            }
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.JpBoarder));
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            ////初始化菜单ui
            //GComponent gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            //ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            //MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            //// 事件放出
            ////goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            //EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
            //    new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            //ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

            GComponent lodAnchorBG = this.contentPane.GetChild("spine").asCom;
            if (goEffect != lodAnchorBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(goEffect);
                go = GameObject.Instantiate(goFgCloneMini);
                spineAnimator = go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                goEffect = lodAnchorBG;
                GameCommon.FguiUtils.AddWrapper(goEffect, go);
            }

            GComponent lodAnchorEffect = contentPane.GetChild("effect").asCom;
            if (lodEffect != lodAnchorEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(lodEffect);
                lodEffect = lodAnchorEffect;
                goEff = GameObject.Instantiate(goAnchorSpineEff);
                effAnimator = goEff.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(lodEffect, goEff);
            }

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;

            gbutton = this.contentPane.GetChild("closeBtn").asButton;
            btnLoader = gbutton.GetChild("button").asLoader;
            credit = contentPane.GetChild("reels").asCom;

            gbutton.onClick.Clear();
            isClose = false;
            gbutton.onClick.Add(SpinDown);

            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            gbutton.visible = true;

            Dictionary<string, object> argDic = null;
            jpCredit.Clear();
            if (_data != null)
            {
                argDic = (Dictionary<string, object>)_data.value;
                if (argDic.ContainsKey("jackpotType"))
                {
                    jackpotType = (int)argDic["jackpotType"];
                }

                if (argDic.ContainsKey("totalEarnCredit"))
                {
                    sorce = Convert.ToInt32(argDic["totalEarnCredit"]);
                }

                if (argDic.ContainsKey("onJPPoolSubCredit"))
                {
                    jackpotAction = (Action)argDic["onJPPoolSubCredit"];
                }

                ExecuteNextStep();
            }

            credit.visible = true;

            //EnsureMainPagSlot();

            StopAll();
            isend = false;

            uiCreditCtrl.Init("Credit", contentPane.GetChild("reels").asList, "N0");

            uiCreditCtrl.SetData(sorce);

            if (!isOpen) return;

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(1f, (object obj) =>
                {
                    SpinDown();
                });
            }
        }

        private void EnsureMainPagSlot()
        {
            GComponent anchor = contentPane.GetChild("anchorPag")?.asCom;
            if (anchor == null) return;

            if(effectPag == null) effectPag = new PagSlotBinding("effectPag", GamePagFolder);
            effectPag.EnsureSlot(anchor, "pagEffect");
            GLoader anchorPag = anchor.GetChild("pagEffect").asLoader;

            anchorPag.SetScale(1.5f, 1.5f);
        }


        private void ExecuteNextStep()
        {
            switch (jackpotType)
            {                              
                case 2:
                    AddWrapperEffect(goFgCloneMini);
                    break;
                case 1:
                    AddWrapperEffect(goFgCloneMinor);
                    break;
                case 0:
                    AddWrapperEffect(goFgCloneMajor);
                    break;
            }

            btnLoader.url = CustomModel.Instance.jackpotResultBtnUrl[jackpotType];
            PlayAnim(spineAnimator, "start"); 
            effAnimator.Play("bigwin");
        }

        private void AddWrapperEffect(GameObject goFgClone)
        {
            GComponent lodAnchorBG = this.contentPane.GetChild("spine").asCom;
            if (true)
            {
                GameCommon.FguiUtils.DeleteWrapper(goEffect);
                go = GameObject.Instantiate(goFgClone);
                spineAnimator = go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                goEffect = lodAnchorBG;
                ChangeParent(gbutton, go, "Anchor/Spine Mecanim GameObject (Lucky_jp_pop_Jackpot)/SkeletonUtility-SkeletonRoot/root/btn01", -2.45f, 0);
                ChangeParent(credit, go, "Anchor/Spine Mecanim GameObject (Lucky_jp_pop_Jackpot)/SkeletonUtility-SkeletonRoot/root/num01", -3.88f, 0.5f);
                GameCommon.FguiUtils.AddWrapper(goEffect, go);
            }
        }

        // 终止所有后续步骤（条件不满足时调用）
        private void StopAll()
        {
            // 移除所有未执行的定时器
            foreach (var timer in _activeTimers)
            {
                Timers.inst.Remove(timer);
            }

            _activeTimers.Clear();
        }


        // 添加定时器并记录引用（用于后续清理）
        private void AddTimer(float delaySeconds, TimerCallback onComplete)
        {
            TimerCallback wrapper = null;
            wrapper = (obj) =>
            {
                onComplete?.Invoke(obj);
                _activeTimers.Remove(wrapper);
            };
            _activeTimers.Add(wrapper);
            Timers.inst.Add(delaySeconds, 1, wrapper);
        }

        private void PlayAnim(Animator animator ,string animName)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0f);
            animator.Update(0f);
        }

        public void SpinDown()
        {
            if(isClose) return;
            isClose = true;

            StopAll();
            if (!isend)
            {
                NumberAnimation.Instance.StopAllAnimations();
                End();
            }
            else
            {
                Exit();
            }
        }

        private void End()
        {
            StopAll();

            PlayAnim(spineAnimator, "end");
            effAnimator.Rebind();
            effAnimator.Update(0f);
            //AddTimer(0.6f, (object obj) =>
            //{
            //    PlayEffectAnim(caidai);
            //    PlayEffectAnim(drop);
            //    PlayEffectAnim(liPao1);
            //    PlayEffectAnim(liPao2);
            //});

            credit.visible = false;
            gbutton.visible = false;
            isend = true;
            DelayedExit();
        }

        public void DelayedExit()
        {
            StopAll();
            AddTimer(0.8f / Time.timeScale, (object obj) =>
            {
                Exit();
            });
        }

        private void Exit()
        {
            //effectPag.StopWithDefaults();
            StopAll();
            jackpotAction?.Invoke();
            CloseSelf(null);

            if (MainModel.Instance.contentMD.isFreeSpin)
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
            }
            else
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            }

            //GameSoundHelper.Instance.StopSound(SoundKey.PopupWinOn);
        }


        private void PlayEffectAnim(Transform effect)
        {
            if(effect == null) return;
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Play();

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                PlayEffectAnim(child);
            }
        }

        private void ChangeParent(GObject gComponent, GameObject go, string path, float xDistance, float yDistance)
        {
            Transform num01 = go.transform.Find(path);
            if (gComponent.displayObject?.gameObject != null)
            {
                Transform t = gComponent.displayObject.gameObject.transform;
                t.SetParent(num01, false);
                t.localPosition = new Vector3(xDistance, yDistance, 0);
                t.localScale = new Vector3(0.01f, 0.01f, 1);
            }
        }
    }
}