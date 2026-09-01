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
    public class PopupJackpotGameTrigger : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupJackpotGameTrigger";

        private GameObject go, goAnchorSpineFg;
        private Animator animator;
        //private Transform effectTransform;

        private GComponent loadAnchor;
        private GButton closeBtn;
        private Transition idleTransition, endTransition;

        private bool isClose = false;

        private EventData _data;
        private bool isInit = false;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        private Action callback;

        //Pag播放
        private const string GamePagFolder = "Games/Xing Yun Zhi Lun 3998/Pag";
        private PagSlotBinding effectPag;
        private readonly string effectName = "ng_fade_jp.pag";

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
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.BgBoarderIN));
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

            GComponent loadlodAnchortip = contentPane.GetChild("anchorBg").asCom;
            if (loadAnchor != loadlodAnchortip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchor);
                loadAnchor = loadlodAnchortip;
                go = GameObject.Instantiate(goAnchorSpineFg);
                animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                //effectTransform = go.transform.GetChild(0).GetChild(0).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(loadAnchor, go);
            }

            idleTransition = contentPane.GetTransition("idle");
            endTransition = contentPane.GetTransition("end");

            idleTransition.ignoreEngineTimeScale = false;
            endTransition.ignoreEngineTimeScale = false;

            EnsureMainPagSlot();

            closeBtn = contentPane.GetChild("Button").asButton;
            closeBtn.alpha = 1;
            closeBtn.scale = new Vector2(1, 1);
            closeBtn.onClick.Clear();
            isClose = false;
            closeBtn.onClick.Add(OnCloseBtn);

            preLoadedCallback?.Invoke();

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
            if (_data != null)
            {
                Dictionary<string, object> argDic = (Dictionary<string, object>)_data.value;
                callback = (Action)argDic["callback"];
            }
            else
            {
                callback = null;
            }

            PlayAnim("start");
            idleTransition.Play(-1, 1.3f / Time.timeScale, null);
            AddTimer(1.3f / Time.timeScale, (object obj) =>
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

        private void EnsureMainPagSlot()
        {
            GComponent anchor = contentPane.GetChild("anchorPag")?.asCom;
            if (anchor == null) return;

            if(effectPag == null)
                effectPag = new PagSlotBinding("NgToJp", GamePagFolder);
            effectPag.EnsureSlot(anchor, "pagEffect");
            GLoader anchorPag = anchor.GetChild("pagEffect").asLoader;

            anchorPag.SetScale(1.5f, 1.5f);
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

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.BgBoarderOut));

            idleTransition.Stop();
            endTransition.Play();

            
            PlayAnim("end");

            AddTimer(0.3f / Time.timeScale, (object obj) =>
            {
                closeBtn.alpha = 0;

                effectPag.StopWithDefaults();
                effectPag.Play(effectName,
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => effectPag?.StopWithDefaults(),
                    stopAfterFinished: true));
            });

            if(callback != null)
            {
                AddTimer(0.9f / Time.timeScale, (object obj) =>
                {
                    callback.Invoke();

                    Debug.LogError("彩金进入回调完成");
                });
            }

            AddTimer(3.8f / Time.timeScale, (object obj) =>
            {
                effectPag.StopWithDefaults();
                CloseSelf(null);
            });
        }

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
    }
}
