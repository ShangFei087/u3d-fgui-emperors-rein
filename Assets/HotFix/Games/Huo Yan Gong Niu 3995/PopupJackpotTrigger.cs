using CaiFuHuoChe_3996;
using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HuoYanGongNiu_3995
{
    public class PopupJackpotTrigger : MachinePageBase
    {
        public new const string pkgName = "HuoYanGongNiu_3995";
        public new const string resName = "PopupJackpotTrigger";

        private GameObject goSpine, go;
        private GComponent anchorSpine;
        private Animator animator;
        private GButton closeBtn;

        private bool isClose = false;

        private EventData _data;
        private bool isInit = false;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private TimerCallback _autoModeSimulatedClick;
        private Action callback = null;
        private const float AutoModeSimulateClickDelaySeconds = 3f;

        //Pag播放
        private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag/jp_tran_huoqiu_bmp";
        private PagSlotBinding JackpotTrigger_bmp;

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
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/JackpotGameTrigger.prefab",
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
            InitParam(data);
        }

        public override void OnClose(EventData data = null)
        {
            JackpotTrigger_bmp.StopWithDefaults();

            StopAll();
            base.OnClose(data);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            closeBtn = contentPane.GetChild("startBtn").asButton;

            GComponent loadSpine = contentPane.GetChild("anchorSpine").asCom;
            if (anchorSpine != loadSpine)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorSpine);
                anchorSpine = loadSpine;
                goSpine = GameObject.Instantiate(go);
                animator = goSpine.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                ChangeParent(closeBtn, goSpine, "Anchor/Spine Mecanim GameObject (jp_pup_Start)/SkeletonUtility-SkeletonRoot/root/all/fg_ic_START", -1.99f, 0.8f);
                GameCommon.FguiUtils.AddWrapper(anchorSpine, goSpine);
            }

            EnsureMainPagSlot();

            isClose = false;
            callback = null;

            closeBtn.onClick.Clear();
            closeBtn.onClick.Add(OnCloseBtn);

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            PlayAnim("in");

            if(_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    callback = args["Callback"] as Action;
                }
            }

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

            if (JackpotTrigger_bmp == null) JackpotTrigger_bmp = new PagSlotBinding("JackpotTrigger_bmp", GamePagFolder);
            JackpotTrigger_bmp.EnsureSlot(anchor, "pagEffect");
        }


        private void OnCloseBtn()
        {
            if (isClose) return;
            isClose = true;

            PlayAnim("out");

            AddTimer(0.56f, (object obj) =>
            {
                if (JackpotTrigger_bmp != null)
                {
                    JackpotTrigger_bmp.StopWithDefaults();
                    JackpotTrigger_bmp.Play("jp_tran_huoqiuxiao.pag",
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => JackpotTrigger_bmp?.StopWithDefaults(),
                    stopAfterFinished: true));

                }
            });

            AddTimer(2.5f, (object obj) =>
            {
                if (JackpotTrigger_bmp != null)
                {
                    JackpotTrigger_bmp.StopWithDefaults();
                    JackpotTrigger_bmp.Play("jp_tran_huoqiuda.pag",
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => JackpotTrigger_bmp?.StopWithDefaults(),
                    stopAfterFinished: true));

                }
            });


            AddTimer(3.5f, (object obj) =>
            {
                callback?.Invoke();
            });

            AddTimer(4.4f, (object obj) =>
            {
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
            CancelAutoModeSimulatedClick();
            // 移除所有未执行的定时器
            foreach (var timer in _activeTimers)
            {
                Timers.inst.Remove(timer);
            }

            _activeTimers.Clear();
        }

        private void PlayAnim(string animName, Action OnComplete = null)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0);
            animator.Update(0);
            OnComplete?.Invoke();
        }

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
