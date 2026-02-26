using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;


namespace XingYunZhiLun_3998
{
    public class PopupFreeSpinResult : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupFreeSpinResult";

        private bool isInit = false;
        private EventData _data;

        private GameObject goAnchorSpineFg, go;
        private Animator animator;
        private Transform effectTransform;

        private GComponent lodAnchor;
        private GButton closeBtn;
        private Transition startTransition, idleTransition, endTransition;
        private GTextField totalCredit;

        private bool isClose;

        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
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
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupFreeSpinResult/FreeSpinResult.prefab",
            (GameObject clone) =>
            {
                goAnchorSpineFg = clone;
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


        public   void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            //if (isOpen)
            //{
            //    //初始化菜单ui
            //    GComponent gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            //    ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            //    MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            //    // 事件放出
            //    //goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            //    EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
            //        new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            //    ContentModel.Instance.btnSpinState = SpinButtonState.Hui;
            //}


            GComponent lodAnchortip = contentPane.GetChild("anchorResult").asCom;
            if(lodAnchor != lodAnchortip)
            {
                GameCommon.FguiUtils.DeleteWrapper(lodAnchor);
                lodAnchor = lodAnchortip;
                go = GameObject.Instantiate(goAnchorSpineFg);
                effectTransform = go.transform.GetChild(0).GetChild(0).GetChild(0);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(lodAnchor,go);
            }

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;

            totalCredit = contentPane.GetChild("number").asTextField;
            totalCredit.alpha = 0;

            if(_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    totalCredit.text = args["baseGameWinCredit"].ToString();
                }
            }

            startTransition = contentPane.GetTransition("start");
            idleTransition = contentPane.GetTransition("idle");
            endTransition = contentPane.GetTransition("end");

            closeBtn = contentPane.GetChild("Button").asButton;
            closeBtn.onClick.Clear();
            isClose = false;
            closeBtn.onClick.Add(OnBtnExit);


            effectTransform.gameObject.SetActive(true);
            PlayAnim("start");

            AddTimer(0.6f, (object obj) =>
            {
                startTransition.Play();
            });

            AddTimer(1.33f, (object obj) =>
            {
                idleTransition.Play(-1, 0, null);
            });

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(2, (object obj) =>
                {
                    OnBtnExit();
                });
            }
        }

        private void OnBtnExit()
        {
            if (isClose) return;
            isClose = true;

            CloseEffect(effectTransform);
            effectTransform.gameObject.SetActive(false);
            PlayAnim("end");
            endTransition.Play();

            AddTimer(1.4f, (object obj) =>
            {
                CloseSelf(null);
                StopAll();
            });
        }

        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0);
            animator.Update(0);
        }

        private void CloseEffect(Transform transform)
        {
            ParticleSystem particle = transform.GetComponent<ParticleSystem>();
            particle.Stop();

            foreach(Transform child in transform)
            {
                CloseEffect(child);
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
