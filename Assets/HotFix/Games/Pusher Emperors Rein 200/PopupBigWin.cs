using PusherEmperorsRein;
using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using SoundKey = PusherEmperorsRein.SoundKey;

//PopupBigWin
namespace PusherEmperorsRein
{
    public class PopupBigWin : MachinePageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupBigWin";

        // UI组件
        GComponent anchorNpc, anchorBigWinEffect02, anchorBigWinEffect01;
        GLoader whatWin,Win;
        GTextField number;
        GButton button;
        Animator animatorNpc;

        // 定时器管理 - 仅用列表存储委托
        private List<TimerCallback> _timerCallbacks = new List<TimerCallback>();

        // 业务数据

        private bool isInit;
        private long score;
        private int endIndex;
        private int playCount;
        private string WinType;
        private readonly string[] WinImageString = { "BIG", "HUGE", "MASSIVE", "LEGENDARY" };
        private GameObject go, goBigWinEffect01, goBigWinEffect02;
        private GameObject Clonego, ClonegoBigWinEffect01, ClonegoBigWinEffect02;

        private readonly Dictionary<string, string> WinImage = new Dictionary<string, string>
        {
            { "BIG", "ui://EmperorsRein/ng_txt_BIG" },
            { "HUGE", "ui://EmperorsRein/ng_txt_HUGE" },
            { "MASSIVE", "ui://EmperorsRein/ng_txt_MASSIVE" },
            { "LEGENDARY", "ui://EmperorsRein/ng_txt_LEGENDARY" },
        };

        private readonly Dictionary<string, string> WinImage2 = new Dictionary<string, string>
        {
            { "BIG", "ui://EmperorsRein/ng_txt_WIN" },
            { "HUGE", "ui://EmperorsRein/ng_txt_WIN" },
            { "MASSIVE", "ui://EmperorsRein/ng_txt_WIN_1" },
            { "LEGENDARY", "ui://EmperorsRein/ng_txt_WIN_2" },
        };

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int loadCount = 3;
            Action loadComplete = () =>
            {
                if (--loadCount == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            // 加载NPC预制体
            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PopupBigWin/Begin Queen.prefab",
                clone =>
                {
                    go = clone;
                    //  animatorNpc ??= go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                    loadComplete();
                });

            // 加载特效1
            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PopupBigWin/BIgWinEffect01.prefab",
                clone =>
                {
                    goBigWinEffect01 = clone;
                    loadComplete();
                });

            // 加载特效2
            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PopupBigWin/BIgWinEffect02.prefab",
                clone =>
                {
                    goBigWinEffect02 = clone;
                    loadComplete();
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
            // 解析数据
            if (data?.value is Dictionary<string, object> argDic)
            {
                // 解析分数
                if (argDic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
                    score = longScore;

                // 解析WinType
                WinType = argDic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";

            }

            endIndex = Array.IndexOf(WinImageString, WinType);
            isok = false;
            contentPane.GetTransition("Begin").SetHook("tuichu", () =>
            {

            });
            contentPane.GetTransition("Begin").timeScale = 1f;
            contentPane.GetTransition("End").timeScale = 1f;
            contentPane.GetTransition("Other").timeScale = 1f;
            DongHuang();
        }



        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;


            // 初始化UI锚点
            GComponent LocalAnchorNpc = contentPane.GetChild("anchorNpc").asCom;
            if (anchorNpc != LocalAnchorNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorNpc);
                Clonego = GameObject.Instantiate(go);
                animatorNpc = Clonego.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorNpc = LocalAnchorNpc;
                GameCommon.FguiUtils.AddWrapper(anchorNpc, Clonego);
            }

            GComponent LocalAnchorBigWinEffect01 = contentPane.GetChild("anchorBigWin01Effect").asCom;
            if (anchorBigWinEffect01 != LocalAnchorBigWinEffect01)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBigWinEffect01);
                ClonegoBigWinEffect01 = GameObject.Instantiate(goBigWinEffect01);
                anchorBigWinEffect01 = LocalAnchorBigWinEffect01;
                GameCommon.FguiUtils.AddWrapper(anchorBigWinEffect01, ClonegoBigWinEffect01);
            }


            GComponent LocalAnchorBigWinEffect02 = contentPane.GetChild("anchorBigWin02Effect").asCom;
            if (anchorBigWinEffect02 != LocalAnchorBigWinEffect02)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBigWinEffect02);
                ClonegoBigWinEffect02 = GameObject.Instantiate(goBigWinEffect02);
                anchorBigWinEffect02 = LocalAnchorBigWinEffect02;
                GameCommon.FguiUtils.AddWrapper(anchorBigWinEffect02, ClonegoBigWinEffect02);
            }

            anchorBigWinEffect02.visible = false;

            // 初始化按钮
            button = contentPane.GetChild("Button").asButton;
            button.onClick.Clear();
            button.onClick.Add(SpinDown);

            // 初始化图片和文本
            whatWin = contentPane.GetChild("whatWin").asLoader;
            Win = contentPane.GetChild("Win").asCom.GetChild("n9").asLoader;
            number = contentPane.GetChild("number").asTextField;


        }

        bool isok;

        public void DongHuang()
        {
            try
            {
                if (WinImageString.Length < 3)
                {
                    DebugUtils.LogError("WinImageString must have at least 3 elements");
                    return;
                }
                // 播放NPC动画
                animatorNpc.Play("anim", -1, 0f);

                // 添加特效显示定时器（存入列表）
                TimerCallback effectCallback = obj =>
                {
                    anchorBigWinEffect02.visible = false;
                    anchorBigWinEffect02.visible = true;
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinCoin);
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinEffect);
                };
                Timers.inst.Add(1f, 1, effectCallback);
                _timerCallbacks.Add(effectCallback);

                // 初始显示第一张图
                whatWin.url = WinImage[WinImageString[0]];
                Win.url = WinImage2[WinImageString[0]];
                if (endIndex == 0)
                {
                    NumberAnimation.Instance.AnimateNumber(contentPane.GetChild("number").asTextField, 0, score, 2, EaseType.Linear, () => { GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd); });
                    contentPane.GetTransition("Begin").Play();

                    contentPane.GetTransition("Begin").SetHook("tuichu", () =>
                    {
                        BigEnd();
                    });
                    return;
                }
                else
                {
                    NumberAnimation.Instance.AnimateNumber(contentPane.GetChild("number").asTextField, 0, score, 4, EaseType.Linear, () => { GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd); });
                    contentPane.GetTransition("Begin").Play();
                    contentPane.GetTransition("Begin").timeScale = 2f;
                    contentPane.GetTransition("End").timeScale = 2f;
                    contentPane.GetTransition("Other").timeScale = 2f;
                }


                playCount = 0;

                // 添加序列动画定时器（存入列表）
                TimerCallback sequenceCallback = obj =>
                {
                    playCount++;
                    animatorNpc.Rebind();
                    animatorNpc.speed = 2f;
                    animatorNpc.Play("anim", -1, 0f);
                    animatorNpc.Update(0f);

                    // 序列内定时器（存入列表）
                    TimerCallback innerCallback = innerObj =>
                    {

                        anchorBigWinEffect02.visible = false;
                        anchorBigWinEffect02.visible = true;
                        GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinCoin);
                        GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinEffect);
                        whatWin.url = WinImage[WinImageString[playCount]];
                        Win.url = WinImage2[WinImageString[playCount]];
                        if (playCount == endIndex)
                            contentPane.GetTransition("End").Play(() => { End(); });
                        else if (playCount < endIndex)
                            contentPane.GetTransition("Other").Play();
                    };
                    Timers.inst.Add(0.5f, 1, innerCallback);
                    _timerCallbacks.Add(innerCallback);
                };
                Timers.inst.Add(1.4f, 3, sequenceCallback);
                _timerCallbacks.Add(sequenceCallback);
            }
            catch (Exception e)
            {
                DebugUtils.LogError("DongHuang 执行异常: " + e);
            }
          
        }

        public void SpinDown()
        {
            if (!isok)
            {
                End();
                playCount = endIndex - 1;
            }
            else
            {
                ClearAllTimers();
                exit();
            }
        }

        public void End( object obj=null)
        {

            ClearAllTimers();
            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinBgNumberAdd);
            animatorNpc.Rebind();
            animatorNpc.speed = 1;
            animatorNpc.Play("idle", -1, 0f);
            animatorNpc.Update(0f);
            NumberAnimation.Instance.StopAllAnimations();
            number.text = score.ToString();
            whatWin.url = WinImage[WinType];
            Win.url = WinImage2[WinType];
            contentPane.GetTransition("Begin").Stop();
            contentPane.GetTransition("Other").Stop();
            contentPane.GetTransition("End").Stop();
            contentPane.GetTransition("t3").Play();
            isok = true;
            whatWin.alpha = 1;


            Timers.inst.Add(2f, 1, exit);
            _timerCallbacks.Add(exit);
        }



        public void BigEnd()
        {

            ClearAllTimers();
            animatorNpc.Rebind();
            animatorNpc.speed = 1f;
            animatorNpc.Play("idle", -1, 0f);
            animatorNpc.Update(0f);
            contentPane.GetTransition("Begin").Stop();
            contentPane.GetTransition("Other").Stop();
            contentPane.GetTransition("End").Stop();
            contentPane.GetTransition("t3").Play();
            Timers.inst.Add(2f, 1,End);
            _timerCallbacks.Add(End);
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
            DebugUtils.Log("所有定时器已清理");
        }

        public void exit(object obj=null)
        {
            ClearAllTimers();
            CloseSelf(null);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.PopupWinEnd);
            GameSoundHelper.Instance.StopSound(SoundKey.PopupWinCoin);
            if (MainModel.Instance.contentMD.isFreeSpin)
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
            }
            else
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            }
        }

        /*
        void OnClose(int value)
        {
            CloseSelf(new EventData<int>("Result", value));
        }
        */
    }
}