using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuHuoChe_3996
{
    public class PopupJackpotGameTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuHuoChe_3996";
        public new const string resName = "PopupJackpotGameTrigger";

        private GameObject goSpine, go;
        private GComponent anchorSpine;
        private Animator animator;
        private GButton closeBtn;
        private GTextField spinTime;
        //private Transition idleTransition;

        private bool isClose = false;

        private EventData _data;
        private bool isInit = false;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private TimerCallback _autoModeSimulatedClick;


        //Pag播放
        private const string GamePagFolder = "Games/Cai Fu Huo Che 3996/Pag";
        private PagSlotBinding InJackpot_bmp;

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
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/JackpotGameTrigger.prefab",
                (GameObject clone) =>
                {
                    go = clone;
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                        {
                            return;
                        }

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
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.JackpotPopupAppear));
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmBonusTrigger));
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

            CancelAutoModeSimulatedClick();

            spinTime = contentPane.GetChild("SpinTimes").asTextField;
            closeBtn = contentPane.GetChild("StartBtn").asButton;

            GComponent loadSpine = contentPane.GetChild("anchorSpine").asCom;
            if(anchorSpine != loadSpine)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorSpine);
                anchorSpine = loadSpine;
                goSpine = GameObject.Instantiate(go);
                animator = goSpine.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                ChangeParent(spinTime, goSpine, "Anchor/Spine Mecanim GameObject (sp_pop_frame)/SkeletonUtility-SkeletonRoot/root/all/changkuang/num02", -2.65f, 4.9f);
                ChangeParent(closeBtn, goSpine, "Anchor/Spine Mecanim GameObject (sp_pop_frame)/SkeletonUtility-SkeletonRoot/root/all/btn", -2.56f, 0.75f);
                GameCommon.FguiUtils.AddWrapper(anchorSpine, goSpine);
            }

            EnsureMainPagSlot();

            isClose = false;
            closeBtn.onClick.Clear();
            closeBtn.onClick.Add(OnCloseBtn);

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            if (_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    spinTime.text = args["SpinTimes"].ToString();
                }
            }

            PlayAnim("prompt_start");

            AddTimer(0.3f, (object obj) =>
            {
                spinTime.visible = true;
            });

            AddTimer(0.96f, (object obj) =>
            {
                ScheduleAutoModeSimulatedClick(closeBtn, () => isClose);
            });

        }

        private void EnsureMainPagSlot()
        {
            GComponent anchor = contentPane.GetChild("anchorPag")?.asCom;
            if (anchor == null)
            {
                Debug.LogError("anchor不存在！！！");
                return;
            }

            if(InJackpot_bmp == null) InJackpot_bmp = new PagSlotBinding("InJackpot_bmp", GamePagFolder);
            InJackpot_bmp.EnsureSlot(anchor, "pagEffect");
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

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.JackpotPopupDisappear));

            PlayAnim("prompt_end");

            spinTime.visible = false;

            AddTimer(0.6f, (object obj) =>
            {
                if (InJackpot_bmp != null)
                {
                    InJackpot_bmp.StopWithDefaults();
                    InJackpot_bmp.Play("InJackpot_bmp.pag",
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => InJackpot_bmp?.StopWithDefaults(),
                    stopAfterFinished: true));
                    AddTimer(5.5f, (object obj) =>
                    {
                        CloseSelf(null);
                    });
                }
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
            CancelAutoModeSimulatedClick();
            // 移除所有未执行的定时器
            foreach (var timer in _activeTimers)
            {
                Timers.inst.Remove(timer);
            }

            _activeTimers.Clear();
        }

        private const float AutoModeSimulateClickDelaySeconds = 3f;

        private void CancelAutoModeSimulatedClick()
        {
            if (_autoModeSimulatedClick == null) return;
            Timers.inst.Remove(_autoModeSimulatedClick);
            _activeTimers.Remove(_autoModeSimulatedClick);
            _autoModeSimulatedClick = null;
        }

        private void ScheduleAutoModeSimulatedClick(GButton target, Func<bool> skipWhenTrue)
        {
            CancelAutoModeSimulatedClick();
            if (!TestManager.Instance.IsAutoModeRunning || target == null)
                return;

            _autoModeSimulatedClick = (obj) =>
            {
                try
                {
                    if (skipWhenTrue != null && skipWhenTrue())
                        return;
                    if (target != null && contentPane != null && contentPane.visible)
                        target.onClick.Call();
                }
                finally
                {
                    var cb = _autoModeSimulatedClick;
                    if (cb != null)
                    {
                        Timers.inst.Remove(cb);
                        _activeTimers.Remove(cb);
                        _autoModeSimulatedClick = null;
                    }
                }
            };
            _activeTimers.Add(_autoModeSimulatedClick);
            Timers.inst.Add(AutoModeSimulateClickDelaySeconds, 1, _autoModeSimulatedClick);
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