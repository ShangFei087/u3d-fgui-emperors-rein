using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace HuoYanGongNiu_3995
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "HuoYanGongNiu_3995";
        public new const string resName = "PopupFreeSpinTrigger";

        private new bool isInit = false;
        private bool isClose = false;

        private EventData _data;

        private Animator animator;
        private GameObject goAnchorSpineFg, go;

        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private GComponent anchorBg;
        private GButton btnStart;
        private Transform bgEffect, exitEffect;
        private GLoader timeImage;

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 1;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupFreeGame/FreeGameTrigger.prefab",
                (GameObject clone) =>
                {
                    go = clone;
                    callback();
                });
        }


        public override void OnOpen(PageName name, EventData data)
        {
            //if (GameSoundHelper.Instance.IsPlaySound(SoundKey.RegularBG))
            //{
            //    GameSoundHelper.Instance.StopSound(SoundKey.RegularBG);
            //}
            //GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinTriggerBG);

            base.OnOpen(name, data);
            InitParam(data);
        }


        public override void OnClose(EventData data = null)
        {
            StopAll();
            base.OnClose(data);
        }


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            GComponent loadAnchor = contentPane.GetChild("anchor").asCom;
            if (anchorBg != loadAnchor)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBg);
                anchorBg = loadAnchor;
                goAnchorSpineFg = GameObject.Instantiate(go);
                animator = goAnchorSpineFg.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                bgEffect = goAnchorSpineFg.transform.GetChild(0).transform;
                exitEffect = goAnchorSpineFg.transform.GetChild(2).transform;
                GameCommon.FguiUtils.AddWrapper(anchorBg, goAnchorSpineFg);
            }

            btnStart = this.contentPane.GetChild("startBtn").asButton;
            btnStart.touchable = false;
            btnStart.onClick.Clear();
            isClose = false;
            btnStart.onClick.Add(OnBtnStartClick);

            timeImage = contentPane.GetChild("times").asLoader;
            timeImage.alpha = 1;

            AddTimer(1.2f, (object obj) =>
            {
                btnStart.touchable = true;
            });
        }


        private void OnBtnStartClick()
        {
            if (isClose) return;
            isClose = true;
            StopEffectAnim(bgEffect);

            PlayAnim("start_out");
            timeImage.alpha = 0;

            AddTimer(1.2f, (object obj) =>
            {
                PlayEffectAnim(exitEffect);
            });

            AddTimer(3.5f, (object obj) =>
            {
                CloseSelf(new EventData<string>("Result", "i am here 1"));
            });
        }


        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0);
            animator.Update(0);
        }


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

        private void StopEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Stop();
            foreach(Transform child in effect)
            {
                // 递归暂停所有子物体的粒子系统
                StopEffectAnim(child);
            }
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
    }
}