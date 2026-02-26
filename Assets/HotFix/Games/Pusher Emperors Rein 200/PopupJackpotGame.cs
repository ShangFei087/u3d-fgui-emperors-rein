using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace PusherEmperorsRein
{


    public class PopupJackpotGame : MachinePageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupJackpotGame";

        private readonly string[] JPString = { "mini", "minor", "major", "grand" };
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
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PopupGameJackpot/PushJackpot.prefab",
                (GameObject clone) =>
                {
                    goFgClone = clone;
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        //DebugUtils.LogError("游戏接受到机台短按的数据：Spin");
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
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinOn);
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        private GameObject goFgClone;
        GameObject go;
        Animator animator;
        Action jackpotAction;
        
        List<float> jpCredit = new List<float> { };
        float sorce;
        string jackpotType;


        private GComponent goEffect;

        public override void InitParam()
        {
      
            if (!isInit) return;

            if (!isOpen) return;

            GComponent lodAnchorBG = this.contentPane.GetChild("effect").asCom;
            //GameCommon.FguiUtils.DeleteWrapper(lodAnchorBG);
            //GameCommon.FguiUtils.AddWrapper(lodAnchorBG, go);
            if (goEffect != lodAnchorBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(goEffect);
                go = GameObject.Instantiate(goFgClone);
                animator = go.transform.GetChild(1).GetChild(1).GetComponent<Animator>();
                goEffect = lodAnchorBG;
                GameCommon.FguiUtils.AddWrapper(goEffect, go);
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
                    jackpotType = (string)argDic["jackpotType"];
                }

                if (argDic.ContainsKey("totalEarnCredit"))
                {
                    sorce = (float)argDic["totalEarnCredit"];
                }

                if (argDic.ContainsKey("onJPPoolSubCredit"))
                {
                    jackpotAction = (Action)argDic["onJPPoolSubCredit"];
                }
            }


            StopAll();
            ExecuteNextStep();
            isend = false;
            go.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<TextMesh>().text = "";
            
        }

        private int InitialSorce;
        private bool isend;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        private void ExecuteNextStep()
        {

            AddTimer(1.8f, (object obj) =>
            {

                switch (jackpotType)
                {
                    case "mini":
                        PlayAnim("JP4_win");
                        break;
                    case "minor":
                        PlayAnim("JP3_win");
                        break;
                    case "major":
                        PlayAnim("JP2_win");
                        break;
                    case "grand":
                        PlayAnim("JP1_win");
                        break;
                    default:
                        DebugUtils.LogError($"jackpot game type is error: {jackpotType}");
                        break;
                }
                //DebugUtils.LogError(jackpotType);
                AddTimer(0.5f, (object obj) => { DelayedExit(); });

                isend = true;

            });


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
            go.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<TextMesh>().text = sorce.ToString();
            AddTimer(3, (object obj) =>
            {
                Exit();
            });
        }

        private void Exit()
        {
            StopAll();
            jackpotAction?.Invoke();
            CloseSelf(null);
         
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
            switch (jackpotType)
            {
                case "mini":
                    PlayAnim("JP4_idle");
                    break;
                case "minor":
                    PlayAnim("JP3_idle");
                    break;
                case "major":
                    PlayAnim("JP2_idle");
                    break;
                case "grand":
                    PlayAnim("JP1_idle");
                    break;
                default:
                    DebugUtils.LogError($"jackpot game type is error: {jackpotType}");
                    break;
            }
            go.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<TextMesh>().text = sorce.ToString();
            isend =true;
            DelayedExit();
        }

        public void SpinDown()
        {
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

    }

}