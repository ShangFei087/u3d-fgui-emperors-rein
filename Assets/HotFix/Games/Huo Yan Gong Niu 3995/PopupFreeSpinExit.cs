using CaiFuHuoChe_3996;
using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace HuoYanGongNiu_3995
{
    public class PopupFreeSpinExit : MachinePageBase
    {
        public new const string pkgName = "HuoYanGongNiu_3995";
        public new const string resName = "PopupFreeSpinExit";

        private new bool isInit = false;
        private bool isClose = false;

        private EventData _data;

        private Animator spineAnim, effAnim;
        private GameObject goAnchorSpineObj, go, anchorEffPre, anchorEffObj;

        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表
        private GComponent anchorBg, anchorEff;
        private GButton exitBtn;
        private GTextField sorceTxt;
        private Action callBack; 

        private const float AutoModeSimulateClickDelaySeconds = 3f;
        private TimerCallback _autoModeSimulatedClick;

        //Pag播放
        private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag/fg_pup_Collect_bmp";
        private PagSlotBinding effectPag;
        private string[] stageName = { "fg_pup_Collect_start_bmp.pag", "fg_pup_Collect_idle_bmp.pag", "fg_pup_Collect_out_bmp.pag", "fg_Collect_tran.pag" };

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 2;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupFreeGame/FreeGameEnd.prefab",
                (GameObject clone) =>
                {
                    go = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupFreeGame/FreeGameEff.prefab",
                (GameObject clone) =>
                {
                    anchorEffPre = clone;
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
            InitParam(data);

            PlayAnim(spineAnim, "in");
            PlayAnim(effAnim, "all_idle");


            //effectPag.StopWithDefaults();
            //effectPag.Play(new PagSequencePlay(PagPlaySpecs.IntroLoop(stageName[0], stageName[1]), PagPlayLayout.Center, useGpuSyncGroup: false));
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

            CancelAutoModeSimulatedClick();

            exitBtn = this.contentPane.GetChild("exitBtn").asButton;
            sorceTxt = contentPane.GetChild("score").asTextField;

            GComponent loadAnchor = contentPane.GetChild("anchor").asCom;
            if (anchorBg != loadAnchor)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBg);
                anchorBg = loadAnchor;
                goAnchorSpineObj = GameObject.Instantiate(go);
                spineAnim = goAnchorSpineObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                ChangeParent(exitBtn, goAnchorSpineObj, "Anchor/Spine Mecanim GameObject (fg_pup_Collect)/SkeletonUtility-SkeletonRoot/root/all/COLLECT", -1.98f, 0.78f);
                ChangeParent(sorceTxt, goAnchorSpineObj, "Anchor/Spine Mecanim GameObject (fg_pup_Collect)/SkeletonUtility-SkeletonRoot/root/all/FREE GAMNS", -5.56f, 0.9f);
                GameCommon.FguiUtils.AddWrapper(anchorBg, goAnchorSpineObj);
            }


            GComponent loadAnchorEff = contentPane.GetChild("anchorEff").asCom;
            if (anchorEff != loadAnchorEff)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorEff);
                anchorEff = loadAnchorEff;
                anchorEffObj = GameObject.Instantiate(anchorEffPre);
                effAnim = anchorEffObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(anchorEff, anchorEffObj);
            }

            EnsureMainPagSlot();

            preLoadedCallback?.Invoke();

            if (!isOpen) return;


            exitBtn.touchable = false;
            exitBtn.onClick.Clear();
            isClose = false;
            exitBtn.onClick.Add(OnBtnStartClick);


            if (_data != null)
            {
                Dictionary<string, object> argDic = (Dictionary<string, object>)_data.value;
                sorceTxt.text = argDic["baseGameWinCredit"].ToString() ;
                if (argDic.ContainsKey("callback"))
                {
                    callBack = (Action)argDic["callback"];
                }
            }
            else
            {
                sorceTxt.text = "99999";
                callBack = null;
            }

            AddTimer(1.2f, (object obj) =>
            {
                exitBtn.touchable = true;
            });

            AddTimer(1.5f, (object obj) =>
            {
                ScheduleAutoModeSimulatedClick(exitBtn, () => isClose);
            });
        }

        private void EnsureMainPagSlot()
        {
            GComponent anchor = contentPane.GetChild("anchorPag")?.asCom;
            if (anchor == null) return;

            if (effectPag == null)
                effectPag = new PagSlotBinding("FreeToNor", GamePagFolder);
            effectPag.EnsureSlot(anchor, "pagEffect");
            GLoader anchorPag = anchor.GetChild("pagEffect").asLoader;
        }


        private void OnBtnStartClick()
        {
            if (isClose) return;
            isClose = true;

            PlayAnim(spineAnim, "out");

            effAnim.Rebind();
            effAnim.Update(0f);

            //effectPag.StopWithDefaults();
            //effectPag.Play(stageName[2], 1, PagPlayLayout.Center, PagPresentationDefaults.DisplayScale, new PagPlayCallbacks(stopAfterFinished: true));

            AddTimer(1.8f, (object obj) =>
            {
                effectPag.StopWithDefaults();
                effectPag.Play(stageName[3], 1, PagPlayLayout.Center, PagPresentationDefaults.DisplayScale, new PagPlayCallbacks(onFinished: () => effectPag?.StopWithDefaults(), stopAfterFinished: true));
            });


            AddTimer(3.1f, (object obj) =>
            {
                CloseSelf(new EventData<string>("Result", "i am here 1"));
            });
        }


        private void PlayAnim(Animator animator, string animName)
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

        private void StopEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Stop();
            foreach (Transform child in effect)
            {
                // 递归暂停所有子物体的粒子系统
                StopEffectAnim(child);
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

        private void CancelAutoModeSimulatedClick()
        {
            if (_autoModeSimulatedClick == null) return;
            Timers.inst.Remove(_autoModeSimulatedClick);
            _activeTimers.Remove(_autoModeSimulatedClick);
            _autoModeSimulatedClick = null;
        }
    }
}