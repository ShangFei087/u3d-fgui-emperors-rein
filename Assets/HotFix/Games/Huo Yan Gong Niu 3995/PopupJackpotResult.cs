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

namespace HuoYanGongNiu_3995
{
    public class PopupJackpotResult : MachinePageBase
    {
        public new const string pkgName = "HuoYanGongNiu_3995";
        public new const string resName = "PopupJackpotResult";
        private GComponent goAnchor;


        private bool isInit = false;
        private bool isend;
        private EventData _data;
        private GTextField credit;
        private GButton gbutton;

        //大奖动画预制体
        private GameObject goFgCloneGrand, goFgCloneMajor, goFgCloneMinor, goFgCloneMini, go;
        private Animator animator;
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

            int count = 3;
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/PushJackpotMajor.prefab",
                (GameObject clone) =>
                {
                    goFgCloneMajor = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/PushJackpotMinor.prefab",
                (GameObject clone) =>
                {
                    goFgCloneMinor = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/PushJackpotMini.prefab",
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
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            GComponent lodAnchorBG = this.contentPane.GetChild("anchor").asCom;
            if (goAnchor != lodAnchorBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(goAnchor);
                go = GameObject.Instantiate(goFgCloneMini);
                animator = go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                goAnchor = lodAnchorBG;
                GameCommon.FguiUtils.AddWrapper(goAnchor, go);
            }

            gbutton = this.contentPane.GetChild("closeBtn").asButton;
            credit = contentPane.GetChild("socre").asTextField;


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
            }

            credit.text = sorce.ToString();

            //EnsureMainPagSlot();

            StopAll();
            isend = false;

            if (!isOpen) return;
            ExecuteNextStep();

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

            if (effectPag == null) effectPag = new PagSlotBinding("effectPag", GamePagFolder);
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

            PlayAnim("in");
        }

        private void AddWrapperEffect(GameObject goFgClone)
        {
            GComponent lodAnchorBG = this.contentPane.GetChild("anchor").asCom;
            if (true)
            {
                //防止内存泄漏
                if(go != null)
                {
                    GameObject.Destroy(go);
                    go = null;  
                }

                GameCommon.FguiUtils.DeleteWrapper(goAnchor);
                go = GameObject.Instantiate(goFgClone);
                animator = go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                goAnchor = lodAnchorBG;
                ChangeParent(credit, go, "Anchor/Spine Mecanim GameObject/SkeletonUtility-SkeletonRoot/root/All/All2/bone6/MAJOR frame", -2.39f, 0.8f);
                GameCommon.FguiUtils.AddWrapper(goAnchor, go);
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
            if (isClose) return;
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

            PlayAnim("out");

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

            //GameSoundHelper.Instance.StopSound(SoundKey.PopupWinOn);
        }


        private void PlayEffectAnim(Transform effect)
        {
            if (effect == null) return;
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