using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace CaiFuHuoChe_3996
{
    public class PopupJackpotResult : MachinePageBase
    {
        public new const string pkgName = "CaiFuHuoChe_3996";
        public new const string resName = "PopupJackpotResult";


        private bool isInit = false;
        private bool isend;
        private EventData _data;

        Action jackpotAction;
        float sorce;
        int jackpotType;

        private GameObject jackpotPref, go;
        private Animator animator;
        private bool isClose;

        private GComponent goEffect;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private TimerCallback _autoModeSimulatedClick;

        private string[] jackpotStartAnimName = { "major_start", "minor_start", "mini_start" };
        private string[] jackpotEndAnimName = { "major_end", "minor_end", "mini_end" };
        private int animIndex = 0;

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
            // 加载预制体
            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/JackpotResult.prefab",
                (GameObject clone) =>
                {
                    jackpotPref = clone;
                    callback();
                });


            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        SpinDown();
                    }
                },
            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam(data);
            isClose = false;
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            GComponent loadEffect = contentPane.GetChild("anchorBg").asCom;
            if (goEffect != loadEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(goEffect);
                goEffect = loadEffect;
                go = GameObject.Instantiate(jackpotPref);
                animator = go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(goEffect, go);
            }

            Dictionary<string, object> argDic = null;
            jackpotType = 1;
            sorce = 0;
            if (_data != null)
            {
                argDic = (Dictionary<string, object>)_data.value;
                if (argDic.ContainsKey("jackpotType"))
                {
                    jackpotType = (int)argDic["jackpotType"];
                }

                if (argDic.ContainsKey("totalEarnCredit"))
                {
                    sorce = (int)argDic["totalEarnCredit"];
                }
            }

            StopAll();
            ExecuteNextStep();

            isend = false;

            preLoadedCallback?.Invoke();
        }

        public void SpinDown()
        {
            if (isClose) return;
            isClose = true;

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.JackpotPopupDisappear));

            StopAll();
            if (!isend)
            {
                NumberAnimation.Instance.StopAllAnimations();
                End();
            }
            else
            {
                Exit();
            }
        }


        private void ExecuteNextStep()
        {
            animIndex = jackpotType;

            ChangeParent();

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.JackpotPopupAppear));

            PlayAnim(jackpotStartAnimName[animIndex]);
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

        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0f);
            animator.Update(0f);
        }

        private void End()
        {
            StopAll();

            PlayAnim(jackpotEndAnimName[animIndex]);
            isend = true;
            DelayedExit();
        }

        public void DelayedExit()
        {
            StopAll();
            AddTimer(1.5f, (object obj) =>
            {
                Exit();
            });
        }

        private void Exit()
        {
            StopAll();
            jackpotAction?.Invoke();
            CloseSelf(null);
        }

        private void ChangeParent()
        {
            CancelAutoModeSimulatedClick();

            string candidatePaths = $"Anchor/Spine Mecanim GameObject (sg_pop_border)/SkeletonUtility-SkeletonRoot/root/all/frame/num01";
            Transform num01 = go.transform.Find(candidatePaths);
            GTextField _gfreetxt = this.contentPane.GetChild("score").asTextField;
            if (_gfreetxt?.displayObject?.gameObject != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;
                t.SetParent(num01, false);
                t.localPosition = new Vector3(-5.35f, 0.75f, 0);
                //t.localRotation = Quaternion.identity;
                t.localScale = new Vector3(0.01f, 0.01f, 1);
            }
            NumberAnimation.Instance.AnimateNumber(_gfreetxt, 0, sorce, 1, EaseType.Linear, () => { });

            string exitBtnPaths = $"Anchor/Spine Mecanim GameObject (sg_pop_border)/SkeletonUtility-SkeletonRoot/root/all/btn";
            Transform btnPos = go.transform.Find(exitBtnPaths);
            GButton exitBtn = this.contentPane.GetChild("exitBtn").asButton;
            if (exitBtn?.displayObject?.gameObject != null)
            {
                Transform b = exitBtn.displayObject.gameObject.transform;
                b.SetParent(btnPos, false);
                b.localPosition = new Vector3(-2.5f, 0.9f, 0);
                //t.localRotation = Quaternion.identity;
                b.localScale = new Vector3(0.01f, 0.01f, 1);
            }

            exitBtn.onClick.Clear();
            exitBtn.onClick.Add(SpinDown);

            ScheduleAutoModeSimulatedClick(exitBtn, () => isClose);
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
    }
}