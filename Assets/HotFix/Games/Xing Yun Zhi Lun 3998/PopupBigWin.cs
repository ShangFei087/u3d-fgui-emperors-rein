using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace XingYunZhiLun_3998
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupBigWin";

        EventData _data = null;

        private GameObject bigWinPref, bigWinObj;
        private GComponent anchorBinWin;
        private Animator bigWinAnim;

        private Transform endEffect;

        private GTextField anchorScore;

        private long score;//分数
        private string WinType;
        private int WinIndex;  //bigwin等级下标

        private bool isok = false;
        private int playCount;
        //定时器
        private List<TimerCallback> _timerCallbacks = new List<TimerCallback>();

        private readonly string[] WinString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] WinOpenString = { "bigwin_start", "superwin_start", "mege_start" };
        private readonly string[] WinCloseString = { "bigwin_end", "superwin_end", "mega_end" };

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
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/GameBgEffect/BigWin.prefab",
                (GameObject clone) =>
                {
                    bigWinPref = clone;
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


        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);

            // 解析数据
            if (data?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    score = longScore;

                WinType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }
            WinIndex = Array.IndexOf(WinString, WinType);

            InitParam(data);

            isok = false;
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;

            GComponent anchorLoad = contentPane.GetChild("anchorBg").asCom;
            if (anchorBinWin != anchorLoad)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBinWin);
                anchorBinWin = anchorLoad;
                bigWinObj = GameObject.Instantiate(bigWinPref);
                bigWinAnim = bigWinObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();

                GameCommon.FguiUtils.AddWrapper(anchorBinWin, bigWinObj);
            }

            anchorScore = contentPane.GetChild("score").asTextField;

            if (!isOpen) return;

            bigWinAnim.Play(WinOpenString[0]);

            ShowAni();
        }


        //播放动画
        public void ShowAni()
        {
            try
            {
                if (WinString.Length < 3)
                {
                    Debug.LogError("WinImageString must have at least 3 elements");
                    return;
                }

                anchorScore.visible = true;
                int showtime = 3 * (WinIndex + 1);
                NumberAnimation.Instance.AnimateNumber(contentPane.GetChild("score").asTextField, 0, score, showtime, EaseType.Linear, () => { });
                // 添加序列动画定时器（存入列表）
                playCount = 0;
                TimerCallback sequenceCallback = obj =>
                {
                    //播放动画
                    playCount++;
                    //animatorBigWin.Rebind();
                    //animatorBigWin.Update(0f);
                    bigWinAnim.Play(WinOpenString[playCount]);
                    TimerCallback innerCallback = innerObj =>
                    {
                        if (playCount == WinIndex)
                        {
                            NumberAnimation.Instance.StopAllAnimations();
                            anchorScore.text = score.ToString();
                            AniEnd();
                        }
                    };
                    Timers.inst.Add(3.5f, 1, innerCallback);
                    _timerCallbacks.Add(innerCallback);
                };
                Timers.inst.Add(3.0f, WinIndex, sequenceCallback);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        public void AniEnd()
        {
            anchorScore.visible = false;
            bigWinAnim.Rebind();
            bigWinAnim.Play(WinCloseString[playCount]);
            bigWinAnim.Update(0f);
            //bigwinPig动画播放到指定时间.
            float closetime = 20f;
            AnimatorStateInfo stateInfo = bigWinAnim.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.length;

            bigWinAnim.Play(stateInfo.fullPathHash, 0, 0);
            ClearAllTimers();
            isok = true;
            Timers.inst.Add(1f, 1, exit);
            _timerCallbacks.Add(exit);
        }

        public void exit(object obj = null)
        {
            ClearAllTimers();
            CloseSelf(null);
        }

        // 清理所有定时器的方法
        private void ClearAllTimers()
        {
            // 遍历列表移除所有定时器
            foreach (var callback in _timerCallbacks)
            {
                if (Timers.inst.Exists(callback)) // 检查定时器是否存在
                    Timers.inst.Remove(callback);
            }

            _timerCallbacks.Clear(); // 清空列表
            Debug.Log("所有定时器已清理");
        }

        public void SpinDown()
        {
            if (!isok)
            {
                AniEnd();
            }
            else
            {
                ClearAllTimers();
                exit();
            }
        }

        //根据传入的父节点依次播放粒子特效
        private void PlayChildEffectAnim(Transform effect)
        {
            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                PlayEffectAnim(child);
            }
        }

        private void PlayEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            if (particle != null) particle.Play();

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                PlayEffectAnim(child);
            }
        }


        //根据传入的父节点依次停止粒子特效
        private void StopChildEffectAnim(Transform effect)
        {
            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }

        //根据传入的节点依次停止粒子特效
        private void StopEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            if (particle != null) particle.Stop(true);

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }

        private void StopAllEffect()
        {
            var allPs = bigWinObj.transform.GetChild(1).GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in allPs)
            {
                ps.Stop(true);
                ps.Clear(true);
            }
        }
    }
}
