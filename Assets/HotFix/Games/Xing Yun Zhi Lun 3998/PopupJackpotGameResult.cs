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
        private Transform caidai, drop, liPao1, liPao2;

        private bool isInit = false;
        private bool isend;
        private EventData _data;
        private MiniReelGroup uiCreditCtrl = new MiniReelGroup();

        //大奖动画预制体
        private GameObject goFgCloneGrand, goFgCloneMajor, goFgCloneMinor, goFgCloneMini, go;
        private Animator animator;
        private bool isClose;

        Action jackpotAction;
        float sorce;
        string jackpotType;
        List<float> jpCredit = new List<float> { };


        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

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

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
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
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinOn);
        }

        public   void InitParam(EventData data)
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

            GComponent lodAnchorBG = this.contentPane.GetChild("effect").asCom;
            if (goEffect != lodAnchorBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(goEffect);
                go = GameObject.Instantiate(goFgCloneMini);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                goEffect = lodAnchorBG;
                GameCommon.FguiUtils.AddWrapper(goEffect, go);
            }

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;

            GButton gbutton = this.contentPane.GetChild("Button").asButton;
            gbutton.onClick.Clear();
            isClose = false;
            gbutton.onClick.Add(SpinDown);

            Dictionary<string, object> argDic = null;
            jpCredit.Clear();
            if (_data != null)
            {
                argDic = (Dictionary<string, object>)_data.value;
                if (argDic.ContainsKey("jackpotType"))
                {
                    jackpotType = (string)argDic["jackpotType"];
                }

                if (argDic.ContainsKey("totalEarnCredit"))
                {
                    sorce = (float)argDic["totalEarnCredit"];
                }

                if (argDic.ContainsKey("onJPPoolSubCredit"))
                {
                    jackpotAction = (Action)argDic["onJPPoolSubCredit"];
                }
            }
            StopAll();
            ExecuteNextStep();
            isend = false;

            uiCreditCtrl.Init("Credit", contentPane.GetChild("reels").asList, "N0");

            uiCreditCtrl.SetData(sorce);

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(1f, (object obj) =>
                {
                    SpinDown();
                });
            }
        }


        private void ExecuteNextStep()
        {
            switch (jackpotType)
            {
                case "mini":
                    AddWrapperEffect(goFgCloneMini);
                    break;
                case "minor":
                    AddWrapperEffect(goFgCloneMinor);
                    break;
                case "major":
                    AddWrapperEffect(goFgCloneMajor);
                    break;
                case "grand":
                    AddWrapperEffect(goFgCloneGrand);
                    break;
            }

            PlayAnim("start");
        }

        private void AddWrapperEffect(GameObject goFgClone)
        {
            GComponent lodAnchorBG = this.contentPane.GetChild("effect").asCom;
            if (true)
            {
                GameCommon.FguiUtils.DeleteWrapper(goEffect);
                go = GameObject.Instantiate(goFgClone);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                caidai = go.transform.GetChild(0).GetChild(1).GetChild(0).transform;
                drop = go.transform.GetChild(0).GetChild(2).GetChild(0).transform;
                liPao1 = go.transform.GetChild(0).GetChild(3).GetChild(0).transform;
                liPao2 = go.transform.GetChild(0).GetChild(4).GetChild(0).transform;
                goEffect = lodAnchorBG;
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
            // 保存定时器回调引用
            _activeTimers.Add(onComplete);
            // 添加定时器，延迟后执行回调，并在执行后从列表中移除
            Timers.inst.Add(delaySeconds, 1, (obj) =>
            {
                onComplete?.Invoke(obj);
                _activeTimers.Remove(onComplete);
            });
        }

        private void PlayAnim(string animName)
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

            PlayAnim("end");
            AddTimer(0.6f, (object obj) =>
            {
                PlayEffectAnim(caidai);
                PlayEffectAnim(drop);
                PlayEffectAnim(liPao1);
                PlayEffectAnim(liPao2);
            });
            isend = true;
            DelayedExit();
        }

        public void DelayedExit()
        {
            StopAll();
            AddTimer(1.5f, (object obj) =>
            {
                Exit();
            });
        }

        private void Exit()
        {
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

            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinOn);
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
    }

}