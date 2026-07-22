using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;


namespace XingYunZhiLun_3998
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupFreeSpinTrigger";

        private bool isInit = false;

        private GameObject goAnchorSpineFg, go;
        private Transform effectFreeGameTriggerBaodian;
        private GComponent lodAnchor;
        GTextField textContent;
        private GButton btnStrat;
        //private Transition endTransition, startTransition, idleTransition;

        private Animator animator;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        private bool isClose;

        private EventData _data;

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupFreeSpinTrigger/FgFadeTips.prefab",
            (GameObject clone) =>
            {
                goAnchorSpineFg = clone;


                isInit = true;
                InitParam(null);
            });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput) return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnBtnStartClick();
                    },
                }
            };
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

            //ContentModel.Instance.btnSpinState = SpinButtonState.Hui;

            //打开时设置免费游戏的免费次数
            textContent = contentPane.GetChild("times").asTextField;
            btnStrat = this.contentPane.GetChild("ButtonStart").asButton;

            GComponent lodAnchortip = this.contentPane.GetChild("anchorSpine").asCom;

            if (lodAnchor != lodAnchortip)
            {
                GameCommon.FguiUtils.DeleteWrapper(lodAnchor);
                lodAnchor = lodAnchortip;
                go = GameObject.Instantiate(goAnchorSpineFg);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                effectFreeGameTriggerBaodian = go.transform.GetChild(0).GetChild(0).GetChild(0).transform;

                GameCommon.FguiUtils.AddWrapper(lodAnchor, go);
                lodAnchor.visible = true;
            }

            ChangeParent(textContent, go, "Anchor/Spine Mecanim GameObject (Lucky_fg_pop_prompt)/SkeletonUtility-SkeletonRoot/root/num01", -2.75f, 1.25f);
            ChangeParent(btnStrat, go, "Anchor/Spine Mecanim GameObject (Lucky_fg_pop_prompt)/SkeletonUtility-SkeletonRoot/root/btn01", -2.45f, 0);

            //ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;

            btnStrat.onClick.Clear();
            isClose = false;
            btnStrat.onClick.Add(OnBtnStartClick);

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            if (_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    textContent.text = args["freeSpinCount"].ToString();
                }
            }

            PlayAnim("start");
            AddTimer(0.15f / Time.timeScale, (object obj) =>
            {
                PlayEffectAnim(effectFreeGameTriggerBaodian);
            });

            if (!isOpen) return;

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

            PlayAnim("end");

            AddTimer(0.7f / Time.timeScale, (object obj) =>
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