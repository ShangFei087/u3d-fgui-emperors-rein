using FairyGUI;
using GameMaker;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "SlotZhuZaiJinBi1700";
        public new const string resName = "PopupBigWin";
        //UI组件
        private GComponent anchorBigWin,anchorBigWinPig;
        private GTextField number;
        private GButton button;
        private Animator animatorBigWin,animatorBigWinPig;
        public SkeletonMecanim skeletonMecanimBigWinPig;
        //定时器
        private List<TimerCallback> _timerCallbacks = new List<TimerCallback>();
        //数据  
        bool isInit = false;
        EventData _data = null;
        private long score;//分数
        private string WinType;
        private readonly string[] WinString = { "BIG", "HUGE", "MASSIVE", "LEGENDARY" };
        private readonly string[] WinOpenString = { "bigwin_open", "bigwin_super", "super_mega", "super_mega" };
        private readonly string[] WinCloseString = { "bigwin_end", "superwin_end", "mega_end" ,"mega_end" };
        private int WinIndex;  //bigwin等级下标
        private int playCount;
        private bool isok=false;
        //预制体
        private GameObject goBigWin, goBigWinPig;
         //实例化
        private GameObject ClonegoBigWin, ClonegoBigWinPig;

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
            // 加载预制体
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupBigWin/symbol_win",
            (GameObject clone) =>
            {
                goBigWin = clone; 
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
          "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupBigWin/symbol_pig",
          (GameObject clone) =>
          {
              goBigWinPig = clone;
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
            InitParam(data);
            // 解析数据
            if (data?.value is Dictionary<string, object> dic)
            {
                if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    score = longScore;

                WinType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
            }
            WinIndex = Array.IndexOf(WinString, WinType);
            isok = false;
           
        }

        // public override void OnTop() { Debug.Log($"i am top {this.name}"); }
        public void InitParam(EventData data)
        {

            if (data != null) _data = data;
            if (!isInit) return;
            // 初始化UI锚点
            GComponent LocalAnchoBigWin = contentPane.GetChild("anchorBigWin").asCom;
            if (anchorBigWin != LocalAnchoBigWin)
            { 
                GameCommon.FguiUtils.DeleteWrapper(anchorBigWin);
                ClonegoBigWin = GameObject.Instantiate(goBigWin);
                animatorBigWin = ClonegoBigWin.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                if (skeletonMecanimBigWinPig == null) skeletonMecanimBigWinPig = ClonegoBigWin.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorBigWin = LocalAnchoBigWin;
                GameCommon.FguiUtils.AddWrapper(anchorBigWin, ClonegoBigWin);
            }

            GComponent LocalAnchorBigWinPig = contentPane.GetChild("anchorBigWinPig").asCom;
            if (anchorBigWinPig!= LocalAnchorBigWinPig)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBigWinPig);
                ClonegoBigWinPig = GameObject.Instantiate(goBigWinPig);
                animatorBigWinPig= ClonegoBigWinPig.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorBigWinPig = LocalAnchorBigWinPig;
                GameCommon.FguiUtils.AddWrapper(anchorBigWinPig, ClonegoBigWinPig);
            }

            button=contentPane.GetChild("button").asButton;
            number = contentPane.GetChild("number").asTextField;
            ShowAni();
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

                number.visible = true;
                int showtime = 4 * (WinIndex+1);
                NumberAnimation.Instance.AnimateNumber(contentPane.GetChild("number").asTextField, 0, score, showtime, EaseType.Linear, () => { });
                // 添加序列动画定时器（存入列表）
                playCount = 0;
                TimerCallback sequenceCallback = obj =>
                {
                    //播放动画
                    playCount++;
                    //animatorBigWin.Rebind();
                    animatorBigWin.Play(WinOpenString[playCount]);
                    //animatorBigWin.Update(0f);
                    TimerCallback innerCallback = innerObj =>
                    {
                        if (playCount == WinIndex)
                        {
                            NumberAnimation.Instance.StopAllAnimations();
                            number.text = score.ToString();
                            AniEnd();
                        }
                    };
                    Timers.inst.Add(3.0f, 1, innerCallback);
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
            number.visible = false;
            animatorBigWin.Play(WinCloseString[playCount]);
            //bigwinPig动画播放到指定时间.
            float closetime = 14.5f;
            AnimatorStateInfo stateInfo = animatorBigWinPig.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = closetime / stateInfo.length;

            animatorBigWinPig.Play(stateInfo.fullPathHash, 0, normalizedTime);
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
    }
}
