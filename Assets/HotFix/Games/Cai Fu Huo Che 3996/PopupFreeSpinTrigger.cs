using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CaiFuHuoChe_3996
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "CaiFuHuoChe_3996";
        public new const string resName = "PopupFreeSpinTrigger";

        private new bool isInit = false;
        private bool isClose = false;

        private GameObject goAnchorSpineObj, go;
        private GComponent lodAnchor;
        private GButton btnStrat;
        private GTextField timesText;

        private Animator animator;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private TimerCallback _autoModeSimulatedClick;

        private EventData _data;

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

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameFree/FreeGameTrigger.prefab",
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
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeSpinPopupAppear));
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmFreeSpinTrigger));
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

            btnStrat = this.contentPane.GetChild("ButtonStart").asButton;
            timesText = contentPane.GetChild("times").asTextField;

            GComponent loadSpineBg = contentPane.GetChild("anchorSpine").asCom;
            if (lodAnchor != loadSpineBg)
            {
                GameCommon.FguiUtils.DeleteWrapper(lodAnchor);
                lodAnchor = loadSpineBg;
                goAnchorSpineObj = GameObject.Instantiate(go);
                animator = goAnchorSpineObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();

                ChangeParent(btnStrat, goAnchorSpineObj, "Anchor/Spine Mecanim GameObject (fg_pop_prompt)/SkeletonUtility-SkeletonRoot/root/all/btn", -1.8f, 0.5f);
                ChangeParent(timesText, goAnchorSpineObj, "Anchor/Spine Mecanim GameObject (fg_pop_prompt)/SkeletonUtility-SkeletonRoot/root/all/kuang_6/num01", -3.25f, 2.25f);

                GameCommon.FguiUtils.AddWrapper(lodAnchor, goAnchorSpineObj);
            }


            btnStrat.visible = true;
            btnStrat.touchable = true;
            btnStrat.onClick.Clear();
            isClose = false;
            btnStrat.onClick.Add(OnBtnStartClick);
            btnStrat.touchable = false;


            //打开时设置免费游戏的免费次数
            if (_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    timesText = contentPane.GetChild("times").asTextField;
                    timesText.text = args["freeSpinCount"].ToString();
                }
            }

            PlayAnim("start");


            AddTimer(0.5f, (object obj) =>
            {
                btnStrat.touchable = true;
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData(SlotMachineEvent.FreeSpinStartButtonShown));

                ScheduleAutoModeSimulatedClick(btnStrat, () => isClose);
            });

            preLoadedCallback?.Invoke();
        }

        private void OnBtnStartClick()
        {
            if (isClose) return;
            isClose = true;

            btnStrat.visible = false;
            btnStrat.touchable = false;

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeSpinPopupDisappear));
            PlayAnim("end");


            AddTimer(0.7f, (object obj) =>
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
