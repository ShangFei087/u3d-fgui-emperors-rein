using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PusherEmperorsRein
{


    public class PopupJackpotOnline : MachinePageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupJackpotOnline";

        protected override void OnInit()
        {
            
            base.OnInit();
            int count = 1;
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Popup Jackpot On Line/Prefabs/Anchor Jackpot Online.prefab",
                (GameObject clone) =>
                {
                    go = clone;
                    //animator = go.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        SpinDown();
                    }
                },
            };
        }



        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
            if (GameSoundHelper.Instance.IsPlaySound(SoundKey.RegularBG))
            {
                GameSoundHelper.Instance.StopSound(SoundKey.RegularBG);
            }
            if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeSpinBG))
            {
                GameSoundHelper.Instance.StopSound(SoundKey.FreeSpinBG);
            }
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinBgNumberAdd);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinOn);
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GameObject go, Clonego;

        Animator animator;
        
        List<float> jpCredit = new List<float> { };
        GComponent ComBg;
        long sorce;
        bool needBack;
        int jackpotType;

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            GComponent lodAnchorBG = this.contentPane.GetChild("Effect").asCom;
            if (ComBg != lodAnchorBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComBg);
                ComBg = lodAnchorBG;
                Clonego = GameObject.Instantiate(go);
                animator = Clonego.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(ComBg, Clonego);
            }






            GButton button = this.contentPane.GetChild("Button").asButton;
            button.onClick.Clear();
            button.onClick.Add(SpinDown);
            Dictionary<string, object> argDic = null;
            jpCredit.Clear();
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                if (argDic.ContainsKey("jackpotType"))
                {
                    jackpotType = (int)argDic["jackpotType"];
                }

                if (argDic.ContainsKey("toCredit"))
                {
                    sorce = (long)argDic["toCredit"];
                }
            }

            _step = -1;
            needBack = false;
            //DebugUtils.LogError(sorce);
            ExecuteNextStep();
            NumberAnimation.Instance.AnimateNumber(
                Clonego.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMesh>(), 0, sorce, 5 ,EaseType.Linear,
                () => {
                    GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
                });
        }

        private int _step = 0; // 步骤计数器（0=未开始，1~13=对应步骤）
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        private void ExecuteNextStep()
        {
//        DebugUtils.LogError(jackpotType);
            _step++; // 进入下一步
            switch (_step)
            {
                case 0:
                    // 步骤1：播放 MINOR_up → 等待0.8秒后执行步骤2
                    PlayAnim("Mini");
                    if (_step == jackpotType)
                    {
                        AddTimer(5, (object obj) => { DelayedExit(); });
                        return;
                    }

                    AddTimer(1.8f, (object obj) => ExecuteNextStep());
                    break;

                case 1:
                    // 步骤1：播放 MINOR_up → 等待0.8秒后执行步骤2
                    PlayAnim("Minor");
                    if (_step == jackpotType)
                    {
                        AddTimer(3.2f, (object obj) => { DelayedExit(); });
                        return;
                    }

                    AddTimer(1.8f, (object obj) => ExecuteNextStep());
                    break;

                case 2:
                    PlayAnim("Major");
                    if (_step == jackpotType)
                    {
                        AddTimer(2.4f, (object obj) => { DelayedExit(); });
                        return;
                    }

                    AddTimer(1.8f, (object obj) => ExecuteNextStep());
                    break;
                case 3:
                    PlayAnim("Grand");
                    if (_step == jackpotType)
                    {
                        AddTimer(0.6f, (object obj) => { DelayedExit(); });
                        return;
                    }

                    break;
                case 4:
                    switch (jackpotType)
                    {
                        case 0:
                            PlayAnim("Minor");
                            break;
                        case 1:
                            PlayAnim("Mini");
                            break;
                        case 2:
                            PlayAnim("Major");
                            break;
                        case 3:
                            PlayAnim("Grand");
                            break;
                    }

                    go.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMesh>().text = sorce.ToString();
                    DelayedExit();
                    break;
            }
        }

        // 播放单个动画（封装重复逻辑）
        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName, -1, 0f);
            animator.Update(0f);
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

        public void DelayedExit()
        {
            StopAll();
            needBack = true;
            NumberAnimation.Instance.StopAllAnimations();
            go.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMesh>().text = sorce.ToString();
            AddTimer(2, (object obj) =>
            {
                Exit();
            });
        }

        private void Exit()
        {
            StopAll();
            CloseSelf(null);
            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
            if (MainModel.Instance.contentMD.isFreeSpin)
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
            }
            else
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            }
            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinOn);
        }

        private void End()
        {
            StopAll();
            _step = 3;
            ExecuteNextStep();
        }


        public void SpinDown()
        {
            StopAll();
            if (!needBack)
            {
                NumberAnimation.Instance.StopAllAnimations();
                go.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMesh>().text = sorce.ToString();
                End();
            }
            else
            {

                Exit();
            }
        }
    }
}