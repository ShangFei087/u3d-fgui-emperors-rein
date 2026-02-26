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
    public class PopupJackpotGameTrigger : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupJackpotGameTrigger";

        private GameObject go, goAnchorSpineFg;
        private Animator animator;
        private Transform effectTransform;

        private GComponent loadAnchor;
        private GButton closeBtn;
        private Transition idleTransition, endTransition;

        private bool isClose = false;

        private EventData _data;
        private bool isInit = false;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/JackpotGameTrigger.prefab",
            (GameObject clone) =>
            {
                goAnchorSpineFg = clone;
                isInit = true;
                InitParam(null);
            });
        }

        public override void OnOpen(PageName name, EventData data)
        {
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

            GComponent loadlodAnchortip = contentPane.GetChild("anchorBg").asCom;
            if (loadAnchor != loadlodAnchortip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchor);
                loadAnchor = loadlodAnchortip;
                go = GameObject.Instantiate(goAnchorSpineFg);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                effectTransform = go.transform.GetChild(0).GetChild(0).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(loadAnchor, go);
            }

            idleTransition = contentPane.GetTransition("idle");
            endTransition = contentPane.GetTransition("end");

            closeBtn = contentPane.GetChild("Button").asButton;
            closeBtn.alpha = 1;
            closeBtn.scale = new Vector2(1, 1);
            closeBtn.onClick.Clear();
            isClose = false;
            closeBtn.onClick.Add(OnCloseBtn);

            if (!isOpen) return;

            /*
            //初始化菜单ui
            GComponent gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            // 事件放出
            //goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));
            
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;
        */
            PlayAnim("start");
            idleTransition.Play(-1, 1.3f, null);
            AddTimer(1.3f, (object obj) =>
            {
                closeBtn.alpha = 1;
                closeBtn.scale = new Vector2(1, 1);
            });

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(2f, (object obj) =>
                {
                    OnCloseBtn();
                });
            }
        }


        private void PlayAnim(string animName, Action OnComplete = null)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0);
            animator.Update(0);
            OnComplete?.Invoke();
        }

        private void OnCloseBtn()
        {
            if (isClose) return;
            isClose = true;

            idleTransition.Stop();
            endTransition.Play();

            
            PlayAnim("end");

            AddTimer(0.74f, (object obj) =>
            {
                closeBtn.alpha = 0;
            });

            AddTimer(0.84f, (object obj) =>
            {
                CloseSelf(null);
            });
        }

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
