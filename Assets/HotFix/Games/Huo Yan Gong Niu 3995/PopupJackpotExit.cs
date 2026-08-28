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
    public class PopupJackpotExit : MachinePageBase
    {
        public new const string pkgName = "HuoYanGongNiu_3995";
        public new const string resName = "PopupJackpotExit";

        private GameObject goSpine, go;
        private GComponent anchorSpine;
        private Animator animator;
        private GButton closeBtn;
        private GTextField winCredit;

        private bool isClose = false;

        private EventData _data;
        private bool isInit = false;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private TimerCallback _autoModeSimulatedClick;
        private Action callback = null;
        private const float AutoModeSimulateClickDelaySeconds = 3f;

        //Pag播放
        private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag/jp_tran_huoqiu_bmp";
        private PagSlotBinding JackpotExit_bmp, JackpotBg_bmp;
        private readonly string[] effBgName = { "jp_pup_Collect_start_bmp.pag" , "jp_pup_Collect_idle_bmp.pag", "jp_pup_Collect_out_bmp.pag" };

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
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/JackpotGameExit.prefab",
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
            JackpotBg_bmp.StopWithDefaults();
            JackpotExit_bmp.StopWithDefaults();

            StopAll();
            base.OnClose(data);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            closeBtn = contentPane.GetChild("startBtn").asButton;
            winCredit = contentPane.GetChild("win").asTextField;

            GComponent loadSpine = contentPane.GetChild("anchorSpine").asCom;
            if (anchorSpine != loadSpine)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorSpine);
                anchorSpine = loadSpine;
                goSpine = GameObject.Instantiate(go);
                animator = goSpine.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                ChangeParent(closeBtn, goSpine, "Anchor/Spine Mecanim GameObject (jp_pup_Collect)/SkeletonUtility-SkeletonRoot/root/all/COLLECT", -2.05f, 0.78f);
                ChangeParent(winCredit, goSpine, "Anchor/Spine Mecanim GameObject (jp_pup_Collect)/SkeletonUtility-SkeletonRoot/root/all/FREE GAMNS", -3.9f, 0.76f);
                GameCommon.FguiUtils.AddWrapper(anchorSpine, goSpine);
            }

            EnsureMainPagSlot();

            isClose = false;
            closeBtn.onClick.Clear();
            closeBtn.onClick.Add(OnCloseBtn);
            winCredit.visible = true;

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            PlayAnim("in");
            JackpotBg_bmp.StopWithDefaults();
            JackpotBg_bmp.Play(new PagSequencePlay(PagPlaySpecs.IntroLoop(effBgName[0], effBgName[1]), PagPlayLayout.Center, useGpuSyncGroup: false));


            if (_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    winCredit.text = args["winCredit"].ToString();
                    if (args.ContainsKey("Callback"))
                    {
                        callback = args["Callback"] as Action;
                    }
                }
            }
            else
            {
                winCredit.text = "0";
                callback = null;
            }


            AddTimer(0.96f, (object obj) =>
                {
                    ScheduleAutoModeSimulatedClick(closeBtn, () => isClose);
                });
        }


        private void EnsureMainPagSlot()
        {
            GComponent anchorBg = contentPane.GetChild("anchorPag")?.asCom;
            GComponent anchor = contentPane.GetChild("anchorPagExit")?.asCom;

            if (anchor == null)
            {
                Debug.LogError("anchor不存在！！！");
                return;
            }

            if (anchorBg == null)
            {
                Debug.LogError("anchorBg不存在！！！");
                return;
            }

            if (JackpotExit_bmp == null) JackpotExit_bmp = new PagSlotBinding("JackpotExit_bmp", GamePagFolder);
            if (JackpotBg_bmp == null) JackpotBg_bmp = new PagSlotBinding("JackpotBg_bmp", GamePagFolder);

            JackpotExit_bmp.EnsureSlot(anchor, "pagEffect");
            JackpotBg_bmp.EnsureSlot(anchorBg, "pagEffect");
        }


        private void OnCloseBtn()
        {
            if (isClose) return;
            isClose = true;

            PlayAnim("out");
            JackpotBg_bmp.StopWithDefaults();
            JackpotBg_bmp.Play(effBgName[2], 1, PagPlayLayout.Center, PagPresentationDefaults.DisplayScale, new PagPlayCallbacks(onFinished: () => JackpotExit_bmp?.StopWithDefaults(), stopAfterFinished: true));

            AddTimer(0.6f, (object obj) =>
            {
                winCredit.visible = false;
                if (JackpotExit_bmp != null)
                {
                    JackpotBg_bmp.StopWithDefaults();
                    JackpotExit_bmp.StopWithDefaults();
                    JackpotExit_bmp.Play("jp_tran_huoqiuxiao.pag",
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => JackpotExit_bmp?.StopWithDefaults(),
                    stopAfterFinished: true));

                }
            });

            AddTimer(2.4f, (object obj) =>
            {
                if (JackpotExit_bmp != null)
                {
                    JackpotExit_bmp.StopWithDefaults();
                    JackpotExit_bmp.Play("jp_tran_huoqiuda.pag",
                    1,
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    new PagPlayCallbacks(
                    onFinished: () => JackpotExit_bmp?.StopWithDefaults(),
                    stopAfterFinished: true));

                }
            });


            AddTimer(3.4f, (object obj) =>
            {
                callback?.Invoke();
            });

            AddTimer(4.3f, (object obj) =>
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
