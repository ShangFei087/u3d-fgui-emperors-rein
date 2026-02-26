using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XingYunZhiLun_3998
{
    public class PopupJackpotGameExit : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupJackpotGameExit";

        private GameObject go, goSpineBg;
        private Animator animator;

        private Transition idleTransition, endTransition;

        private GComponent loadAnchor;
        private GButton closeBtn;

        private MiniReelGroup uiJPReslutCtrl = new MiniReelGroup();

        private EventData _data;
        private bool isInit = false, isClose = false;

        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/JackpotGameExit.prefab",
                (GameObject clone) =>
                {
                    goSpineBg = clone;
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

            ////初始化菜单ui
            //GComponent gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            //ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            //MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            //// 事件放出
            ////goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            //EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
            //    new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));
            //ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

            GComponent loadAnchorTip = contentPane.GetChild("anchorBg").asCom;
            if(loadAnchor != loadAnchorTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchor);
                loadAnchor = loadAnchorTip;
                go = GameObject.Instantiate(goSpineBg);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(loadAnchor, go);
            }

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;

            closeBtn = contentPane.GetChild("ButtonClose").asButton;
            closeBtn.scale = new Vector2(1, 1);
            closeBtn.onClick.Clear();
            closeBtn.onClick.Add(OnCloseBtn);
            isClose = false;

            contentPane.GetChild("reels").asList.scale = new Vector2(1, 1);
            uiJPReslutCtrl.Init("JackpotResult", contentPane.GetChild("reels").asList, "N0");
            if(_data != null)
            {
                Dictionary<string, object> argDic = (Dictionary<string, object>)_data.value;
                uiJPReslutCtrl.SetData((float)argDic["totalEarnCredit"]);
            }
            else
            {
                uiJPReslutCtrl.SetData(0);
            }

            idleTransition = contentPane.GetTransition("idle");
            endTransition = contentPane.GetTransition("end");

            PlayAnim("start");
            idleTransition.Play(-1, 1.3f, null);

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(1.2f, (object obj) =>
                {
                    OnCloseBtn();
                });
            }
        }

        private void OnCloseBtn()
        {
            if (isClose) return;
            isClose = true;

            idleTransition.Stop();
            endTransition.Play();

            PlayAnim("end");

            AddTimer(1f, (obj) =>
            {
                CloseSelf(null);
            });
        }

        public void PlayAnim(string aniName)
        {
            animator.Rebind();
            animator.Play(aniName, -1, 0);
            animator.Update(0);
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
