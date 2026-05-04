using FairyGUI;
using GameMaker;
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

        private GameObject miniPref, minorPref, majorPref, go;
        private Animator animator;
        private bool isClose;

        private GComponent goEffect;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 3;

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
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/JackpotMini.prefab",
                (GameObject clone) =>
                {
                    miniPref = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/JackpotMinor.prefab",
                (GameObject clone) =>
                {
                    minorPref = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/JackpotMajor.prefab",
                (GameObject clone) =>
                {
                    majorPref = clone;
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
                go = GameObject.Instantiate(miniPref);
                animator = go.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Animator>();
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

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(1f, (object obj) =>
                {
                    SpinDown();
                });
            }
        }

        public void SpinDown()
        {
            if (isClose) return;
            isClose = true;

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
            switch (jackpotType)
            {
                case 1:
                    AddWrapperEffect(majorPref);
                    break;
                case 2:
                    AddWrapperEffect(minorPref);
                    break;
                case 3:
                    AddWrapperEffect(miniPref);
                    break;
            }


            PlayAnim("sg_pop_border_start");
        }

        private void AddWrapperEffect(GameObject goFgClone)
        {
            GameCommon.FguiUtils.DeleteWrapper(goEffect);
            go = GameObject.Instantiate(goFgClone);
            animator = go.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Animator>();
            GameCommon.FguiUtils.AddWrapper(goEffect, go);

            ChangeParent();
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

            PlayAnim("sg_pop_border_end");
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
            string candidatePaths = $"Anchor/sg_pop_border/Animation/tiao/num";
            Transform num01 = go.transform.Find(candidatePaths);
            GTextField _gfreetxt = this.contentPane.GetChild("score").asTextField;
            if (_gfreetxt?.displayObject?.gameObject != null)
            {
                Transform t = _gfreetxt.displayObject.gameObject.transform;
                t.SetParent(num01, false);
                t.localPosition = new Vector3(-368, 69, -10);
                //t.localRotation = Quaternion.identity;
                t.localScale = new Vector3(0.7f, 0.7f, 1);
            }
            NumberAnimation.Instance.AnimateNumber(_gfreetxt, 0, sorce, 1, EaseType.Linear, () => { });

            string exitBtnPaths = $"Anchor/sg_pop_border/Animation/btn/num";
            Transform btnPos = go.transform.Find(candidatePaths);
            GButton exitBtn = this.contentPane.GetChild("exitBtn").asButton;
            if (exitBtn?.displayObject?.gameObject != null)
            {
                Transform b = exitBtn.displayObject.gameObject.transform;
                b.SetParent(btnPos, false);
                b.localPosition = new Vector3(-255, -267, 0);
                //t.localRotation = Quaternion.identity;
                b.localScale = new Vector3(1, 1, 1);
            }

            exitBtn.onClick.Clear();
            exitBtn.onClick.Add(SpinDown);
        }
    }
}