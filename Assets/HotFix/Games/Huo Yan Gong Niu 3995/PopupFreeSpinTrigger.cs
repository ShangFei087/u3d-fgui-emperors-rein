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
        private GTextField timeImage;

        //Pag播放
        private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag/fg_pup_Collect_bmp";
        private PagSlotBinding effectPag;
        private string[] stageName = { "fg_pup_Collect_start_bmp.pag", "fg_pup_Collect_idle_bmp.pag", "fg_pup_Collect_out_bmp.pag", "fg_Collect_tran.pag" };

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
            effectPag.StopWithDefaults();

            StopAll();
            base.OnClose(data);
        }


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            btnStart = this.contentPane.GetChild("startBtn").asButton;
            timeImage = contentPane.GetChild("times").asTextField;

            GComponent loadAnchor = contentPane.GetChild("anchor").asCom;
            if (anchorBg != loadAnchor)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBg);
                anchorBg = loadAnchor;
                goAnchorSpineFg = GameObject.Instantiate(go);
                animator = goAnchorSpineFg.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                ChangeParent(timeImage, goAnchorSpineFg, "Anchor/Spine Mecanim GameObject (fg_pup_Start)/SkeletonUtility-SkeletonRoot/root/all/number", -1.58f, 1.5f);
                ChangeParent(btnStart, goAnchorSpineFg, "Anchor/Spine Mecanim GameObject (fg_pup_Start)/SkeletonUtility-SkeletonRoot/root/all/START", -1.96f, 0.8f);
                GameCommon.FguiUtils.AddWrapper(anchorBg, goAnchorSpineFg);
            }

            EnsureMainPagSlot();

            preLoadedCallback?.Invoke();

            if (!isOpen) return;
            PlayAnim("in");

            effectPag.StopWithDefaults();
            effectPag.Play(new PagSequencePlay(PagPlaySpecs.IntroLoop(stageName[0], stageName[1]), PagPlayLayout.Center,useGpuSyncGroup: false));

            btnStart.touchable = false;
            btnStart.onClick.Clear();
            isClose = false;
            btnStart.onClick.Add(OnBtnStartClick);

            timeImage.alpha = 1;

            AddTimer(1.2f, (object obj) =>
            {
                btnStart.touchable = true;
            });
        }

        private void EnsureMainPagSlot()
        {
            GComponent anchor = contentPane.GetChild("anchorPag")?.asCom;
            if (anchor == null) return;

            if (effectPag == null)
                effectPag = new PagSlotBinding("NorToFree", GamePagFolder);
            effectPag.EnsureSlot(anchor, "pagEffect");
            GLoader anchorPag = anchor.GetChild("pagEffect").asLoader;
        }


        private void OnBtnStartClick()
        {
            if (isClose) return;
            isClose = true;

            PlayAnim("out");
            timeImage.alpha = 0;

            effectPag.StopWithDefaults();
            effectPag.Play(stageName[2], 1, PagPlayLayout.Center, PagPresentationDefaults.DisplayScale,new PagPlayCallbacks(stopAfterFinished: true));

            AddTimer(1.8f, (object obj) =>
            {
                effectPag.StopWithDefaults();
                effectPag.Play(stageName[3], 1, PagPlayLayout.Center, PagPresentationDefaults.DisplayScale, new PagPlayCallbacks(onFinished: () => effectPag?.StopWithDefaults(), stopAfterFinished: true));
            });

            AddTimer(3.1f, (object obj) =>
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
            TimerCallback wrapper = null;
            wrapper = (obj) =>
            {
                onComplete?.Invoke(obj);
                _activeTimers.Remove(wrapper);
            };
            _activeTimers.Add(wrapper);
            Timers.inst.Add(delaySeconds, 1, wrapper);
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