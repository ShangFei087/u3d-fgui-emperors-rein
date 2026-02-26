using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuHuoChe_3996
{
    public class PopupJackpotGameExit : MachinePageBase
    {
        public new const string pkgName = "CaiFuHuoChe_3996";
        public new const string resName = "PopupJackpotGameExit";

        private GameObject goSpine, go;
        private GComponent anchorSpine;
        private Animator animator;
        private GButton closeBtn;
        private GTextField winCredit;
        private Transition idleTransition;

        private bool isClose = false;

        private EventData _data;
        private bool isInit = false;
        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/JackpotGameExit.prefab",
                (GameObject clone) =>
                {
                    go = clone;
                    isInit = true;
                    InitParam();
                });
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
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

            GComponent loadSpine = contentPane.GetChild("anchorSpine").asCom;
            if (anchorSpine != loadSpine)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorSpine);
                anchorSpine = loadSpine;
                goSpine = GameObject.Instantiate(go);
                animator = goSpine.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(anchorSpine, goSpine);
            }

            isClose = false;
            closeBtn = contentPane.GetChild("closeBtn").asButton;
            closeBtn.onClick.Clear();
            closeBtn.onClick.Add(OnCloseBtn);
            winCredit = contentPane.GetChild("winCredit").asTextField;
            winCredit.visible = false;
            idleTransition = contentPane.GetTransition("idle");

            if (_data != null)
            {
                Dictionary<string, object> args = _data.value as Dictionary<string, object>;
                if (args != null)
                {
                    winCredit.text = args["winCredit"].ToString();
                }
            }

            PlayAnim("start");

            AddTimer(0.3f, (object obj) =>
            {
                winCredit.visible = true;
            });

            AddTimer(0.96f, (object obj) =>
            {
                idleTransition.Play(-1, 0, null);
            });

            if (ContentModel.Instance.isAuto)
            {
                AddTimer(2f, (object obj) =>
                {
                    OnCloseBtn();
                });
            }
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


            PlayAnim("end");

            winCredit.visible = false;

            AddTimer(1f, (object obj) =>
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
            // 移除所有未执行的定时器
            foreach (var timer in _activeTimers)
            {
                Timers.inst.Remove(timer);
            }

            _activeTimers.Clear();
        }
    }
}