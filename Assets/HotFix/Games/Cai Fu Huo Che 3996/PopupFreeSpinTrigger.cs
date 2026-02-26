using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CaiFuHuoChe_3996
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuHuoChe_3996";
        public new const string resName = "PopupFreeSpinTrigger";

        private new bool isInit = false;
        private bool isClose = false;

        private GameObject goAnchorSpineFg, go;
        private GComponent lodAnchor;
        private GButton btnStrat;
        private GTextField timesText;
        private Transition endTransition, idleTransition;

        private Animator animator;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        private EventData _data;

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
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameFree/FreeGameTrigger.prefab",
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

            //获取动效贴合动画效果
            endTransition = contentPane.GetTransition("end");
            idleTransition = contentPane.GetTransition("idle");

            GComponent loadSpineBg = contentPane.GetChild("anchorSpine").asCom;
            if (lodAnchor != loadSpineBg)
            {
                GameCommon.FguiUtils.DeleteWrapper(lodAnchor);
                lodAnchor = loadSpineBg;
                goAnchorSpineFg = GameObject.Instantiate(go);
                animator = goAnchorSpineFg.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(lodAnchor, goAnchorSpineFg);
            }

            btnStrat = this.contentPane.GetChild("ButtonStart").asButton;
            btnStrat.onClick.Clear();
            isClose = false;
            btnStrat.onClick.Add(OnBtnStartClick);
            btnStrat.visible = false;
            btnStrat.touchable = false;

            timesText = contentPane.GetChild("times").asTextField;
            timesText.visible = false;
            timesText.scale = new Vector2(1, 1);
            timesText.alpha = 1;

            //打开时设置免费游戏的免费次数
            if (_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    timesText = contentPane.GetChild("times").asTextField;
                    timesText.text = args["freeSpinCount"].ToString();
                }
            }

            PlayAnim("fg_pop_prompt_start");

            AddTimer(0.3f, (object obj) =>
            {
                timesText.visible = true;
            });

            AddTimer(0.5f, (object obj) =>
            {
                btnStrat.visible = true;
                btnStrat.touchable = true;
                idleTransition.Play(-1, 0, null);
            });

            if (ContentModel.Instance.isAuto)
            {
                Timers.inst.Add(1, 1, (object obj) =>
                {
                    OnBtnStartClick();
                });
            }
        }

        private void OnBtnStartClick()
        {
            if (isClose) return;
            isClose = true;

            btnStrat.visible = false;
            btnStrat.touchable = false;

            PlayAnim("fg_pop_prompt_end");

            AddTimer(0.1f, (object obj) =>
            {
                idleTransition.Stop();
                endTransition.Play();
            });

            AddTimer(0.7f, (object obj) =>
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
