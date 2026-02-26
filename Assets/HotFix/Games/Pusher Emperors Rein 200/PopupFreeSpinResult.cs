using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;


namespace PusherEmperorsRein
{

    public class PopupFreeSpinResult : MachinePageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupFreeSpinResult";



        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            isInit = true;

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        OnClickSpinButton();
                    }
                },
            };
        }



        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinBgNumberAdd);
            InitParam();
        }


        public override void OnClose(EventData data = null)
        {
            CloseAllTask();

            base.OnClose(data);
        }


        // 业务数据

        bool isInit;
        GTextField gtxtNumber;

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            gtxtNumber = this.contentPane.GetChild("number").asTextField;

            GButton btnBG = this.contentPane.GetChild("Button").asButton;
            btnBG.onClick.Clear();
            btnBG.onClick.Add(OnClickBG);


            // 解析数据
            if (inParams != null && inParams?.value is Dictionary<string, object> argDic)
            {
                // 解析分数
                if (argDic.TryGetValue("freeSpinTotalWin", out var scoreVal) && scoreVal is long longScore)
                {
                    DebugUtils.LogError($"【免费游戏结算弹窗】 总币数 longScore = {longScore}");
                    CloseAllTask();

                    DoTaskToNumber(longScore);
                    DoTaskStopAddNumber();
                }
                else
                {
                    DebugUtils.LogError($"【免费游戏结算弹窗】 没拿到总币数");
                }

            }
        }

        void DoTaskToNumber(long longScore)
        {
            toNumber = longScore;
            curNumber = 0;
            step = Step.AddToNumber;
            TaskToNumber(null);
        }

        void DoTaskStopAddNumber()
        {
            Timers.inst.Add(3, 1, TaskStopAddNumber);
        }


        void TaskStopAddNumber(object param)
        {
            if (step == Step.AddToNumber)
            {
                Timers.inst.Remove(TaskToNumber);

                step = Step.ShowBigger;
                TaskToNumber(null);
            }
        }


        enum Step
        {
            /// <summary> 循环添加到目标值 </summary>
            AddToNumber = 0,

            /// <summary> 直接设置到目标值-停留一会 （变大？）</summary>
            ShowBigger = 1,

            /// <summary> 直接设置到目标值-停留一会 （变大？）</summary>
            ShowBiggerEnd = 2,

            /// <summary> 延时3秒自动结束 </summary>
            DelayAutoClose = 3,
        }


        long toNumber = 100;
        long curNumber = 0;
        Step step = Step.AddToNumber;

        void TaskToNumber(object param)
        {
            switch (step)
            {
                case Step.AddToNumber: 

                    curNumber += 1;
                    if (curNumber <= toNumber)
                    {
                        gtxtNumber.text = curNumber.ToString();

                        Timers.inst.Add(0.05f, 1, TaskToNumber);
                    }
                    else
                    {
                        gtxtNumber.text = toNumber.ToString();

                        step = Step.ShowBigger;
                        TaskToNumber(null);
                    }
                    break;
                case Step.ShowBigger: 
                    {
                        gtxtNumber.text = toNumber.ToString();
                        GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
                        // 变大动画

                        contentPane.GetTransition("Bigger").Play(() => {

                            step = Step.ShowBiggerEnd;
                            TaskToNumber(null);
                        });
                    }
                    break;
                case Step.ShowBiggerEnd:
                    {
                        // 是否延时3秒
                        if (!PlayerPrefsUtils.isPauseAtPopupFreeSpinResult)
                        {
                            step = Step.DelayAutoClose;
                            Timers.inst.Add(3f, 1, TaskToNumber);
                        }
                    }
                    break;
                case Step.DelayAutoClose: 
                    {
                        ClosePopup();
                    }
                    break;
            }
        }

   
        void OnClickBG() => OnClickSpinButton();

        void OnClickSpinButton()
        {
            if (step == Step.AddToNumber)
            {
                CloseAllTask();

                step = Step.ShowBigger;
                TaskToNumber(null);
            }else 
            {
                ClosePopup();
            }
        }



        void CloseAllTask()
        {
            contentPane.GetTransition("Bigger").Stop();

            Timers.inst.Remove(TaskToNumber);
            Timers.inst.Remove(TaskStopAddNumber);
        }



        void ClosePopup(object onj =null)
        {
            CloseAllTask();

            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
            CloseSelf(null);
        }
    }

}


#if false




namespace PusherEmperorsRein
{

    public class PopupFreeSpinResult : MachinePageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupFreeSpinResult";


        // 业务数据

        bool isInit, isEnd;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            isInit = true;
            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        close();
                        Spin();
                    }
                },
            };
        }



        public override void OnOpen(PageName name, EventData data)
        {
            //if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeSpinBG))
            //{
            //    GameSoundHelper.Instance.StopSound(SoundKey.FreeSpinBG);
            //}
            //GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinResultBG);
            base.OnOpen(name, data);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinBgNumberAdd);
            InitParam();
        }

        GTextField number;

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            isEnd = false;
            number = contentPane.GetChild("number").asTextField;
            contentPane.GetChild("Button").asButton.onClick.Add(() =>
            {
                CloseSelf(null);
            });
            // 解析数据
            if (inParams?.value is Dictionary<string, object> argDic)
            {
                // 解析分数
                if (argDic.TryGetValue("freeSpinTotalWin", out var scoreVal) && scoreVal is long longScore)
                {
                    NumberAnimation.Instance.AnimateNumber(
                    number,
                    0, longScore, 5, EaseType.Linear,
                     () => {
                         GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
                         isEnd = true;

                         if (!PlayerPrefsUtils.isPauseAtPopupFreeSpinResult)
                         {
                             Timers.inst.Remove(close);
                             Timers.inst.Add(3, 1, close);
                         }
                     });
                }
            }
        }



        void Spin()
        {
            if (isEnd)
            {
                NumberAnimation.Instance.ForceSetAllTargetValues();
            }
            else
            {
                Timers.inst.Remove(close);
                close();
            }
        }

        void close(object onj = null)
        {
            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            CloseSelf(null);
        }
    }

}

#endif