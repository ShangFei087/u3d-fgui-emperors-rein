using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
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
        private Action callBack;

        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表


        //Pag播放
        private const string GamePagFolder = "Games/Xing Yun Zhi Lun 3998/Pag";
        private PagSlotBinding effectPag;
        private readonly string effectName = "jp_fade_ng.pag";


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

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput) return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnCloseBtn();
                    },
                }
            };
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


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

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

            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            isClose = false;

            contentPane.GetChild("reels").asList.scale = new Vector2(1, 1);
            uiJPReslutCtrl.Init("JackpotResult", contentPane.GetChild("reels").asList, "N0");
            if(_data != null)
            {
                Dictionary<string, object> argDic = (Dictionary<string, object>)_data.value;
                uiJPReslutCtrl.SetData(Convert.ToInt32(argDic["totalEarnCredit"]));
                callBack = (Action)argDic["callback"];
            }
            else
            {
                uiJPReslutCtrl.SetData(0);
                callBack = null;
            }

            idleTransition = contentPane.GetTransition("idle");
            endTransition = contentPane.GetTransition("end");

            idleTransition.ignoreEngineTimeScale = false;
            endTransition.ignoreEngineTimeScale = false;


            PlayAnim("start");
            idleTransition.Play(-1, 1.3f / Time.timeScale, null);
            
            EnsureMainPagSlot();

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(1.2f, (object obj) =>
                {
                    OnCloseBtn();
                });
            }
        }

        private void EnsureMainPagSlot()
        {
            GComponent anchor = contentPane.GetChild("anchorPag")?.asCom;
            if (anchor == null) return;

            if(effectPag == null)
                effectPag = new PagSlotBinding("JpToNg", GamePagFolder);
            effectPag.EnsureSlot(anchor, "pagEffect");
            GLoader anchorPag = anchor.GetChild("pagEffect").asLoader;

            anchorPag.SetScale(1.5f, 1.5f);
        }


        private void OnCloseBtn()
        {
            if (isClose) return;
            isClose = true;

            idleTransition.Stop();
            endTransition.Play();

            PlayAnim("end");

            if(callBack != null)
            {
                AddTimer(2f / Time.timeScale, (obj) =>
                {
                    callBack.Invoke();
                });
            }

            AddTimer(0.4f / Time.timeScale, (obj) =>
            {
                effectPag.StopWithDefaults();
                effectPag.Play(effectName,
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => effectPag?.StopWithDefaults(),
                    stopAfterFinished: true));
            });

            AddTimer(5.3f / Time.timeScale, (obj) =>
            {
                effectPag.StopWithDefaults();
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
