using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuoYanGongNiu_3995
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "HuoYanGongNiu_3995";
        public new const string resName = "PopupBigWin";
        EventData _data = null;

        private GameObject bigWinPref, bigWinObj, bigWinEffPref, bigWinEffObj;
        private GComponent anchorBinWin, anchorBigWinEff;
        private Animator bigWinAnim, bigWinEffAnim;

        private GTextField anchorScore;

        private long score;//分数
        private string WinType;
        private int WinIndex;  //bigwin等级下标

        private bool isok = false;
        private int playCount;
        //定时器
        private List<TimerCallback> _timerCallbacks = new List<TimerCallback>();

        private readonly string[] WinString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] WinOpenString = { "Big_in", "BigToSuper", "SuperToMega" };
        private readonly string[] WinCloseString = { "Big_out", "Super_out", "Mega_out" };
        private readonly string[] WinEffectAnimName = { "big_idle", "super_idle", "mega_idle" };

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
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PageGameMain/BigWin.prefab",
                (GameObject clone) =>
                {
                    bigWinPref = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupFreeGame/FreeGameEff.prefab",
                (GameObject clone) =>
                {
                    bigWinEffPref = clone;
                    callback();
                });
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

            GComponent anchorLoad = contentPane.GetChild("anchorSpine").asCom;
            GComponent anchorEffLoad = contentPane.GetChild("anchorEff").asCom;

            anchorScore = contentPane.GetChild("score").asTextField;
            anchorScore.visible = false;

            if (anchorBinWin != anchorLoad)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBinWin);
                anchorBinWin = anchorLoad;
                bigWinObj = GameObject.Instantiate(bigWinPref);
                bigWinAnim = bigWinObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                ChangeParent(anchorScore, bigWinObj, "Anchor/Spine Mecanim GameObject (ng_pup_BigWin)/SkeletonUtility-SkeletonRoot/root/all/frame", -5.4f, 0.71f);

                GameCommon.FguiUtils.AddWrapper(anchorBinWin, bigWinObj);
            }

            if (anchorBigWinEff != anchorEffLoad)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBigWinEff);
                anchorBigWinEff = anchorEffLoad;
                bigWinEffObj = GameObject.Instantiate(bigWinEffPref);
                bigWinEffAnim = bigWinEffObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();

                GameCommon.FguiUtils.AddWrapper(anchorBigWinEff, bigWinEffObj);
            }


            preLoadedCallback?.Invoke();


            if (!isOpen) return;

            bigWinAnim.Play(WinOpenString[0]);
            bigWinEffAnim.Play(WinEffectAnimName[0]);

            TimerCallback showCallback = innerObj =>
            {
                anchorScore.visible = true;
            };

            Timers.inst.Add(1f / Time.timeScale, 1, showCallback);

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
                    //bigWinAnim.Rebind();
                    //bigWinAnim.Update(0f);
                    bigWinAnim.Play(WinOpenString[playCount]);
                    bigWinEffAnim.Play(WinEffectAnimName[playCount]);

                    if (playCount == WinIndex)
                    {
                        TimerCallback innerCallback = innerObj =>
                        {
                            NumberAnimation.Instance.StopAllAnimations();
                            anchorScore.text = score.ToString();
                            AniEnd();
                        };
                        Timers.inst.Add(3.5f / Time.timeScale, 1, innerCallback);
                        _timerCallbacks.Add(innerCallback);
                    }
                };
                if (WinIndex == 0)
                {
                    TimerCallback innerCallback = innerObj =>
                    {
                        if (playCount == WinIndex)
                        {
                            NumberAnimation.Instance.StopAllAnimations();
                            anchorScore.text = score.ToString();
                            AniEnd();
                        }
                    };
                    Timers.inst.Add(3f / Time.timeScale, 1, innerCallback);
                    _timerCallbacks.Add(innerCallback);
                }
                else
                {
                    // 泄漏：sequenceCallback 未加入 _timerCallbacks，ClearAllTimers 清不掉。
                    Timers.inst.Add(3.0f / Time.timeScale, WinIndex, sequenceCallback);

                    _timerCallbacks.Add(sequenceCallback);
                }

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
            bigWinEffAnim.Rebind();

            bigWinAnim.Play(WinCloseString[playCount]);
            bigWinAnim.Update(0f);
            bigWinEffAnim.Play(WinEffectAnimName[playCount]);
            bigWinEffAnim.Update(0f);


            //bigwinPig动画播放到指定时间.
            AnimatorStateInfo stateInfo = bigWinAnim.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo stateEffInfo = bigWinEffAnim.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.length;

            bigWinAnim.Play(stateInfo.fullPathHash, 0, 0);

            ClearAllTimers();
            isok = true;
            Timers.inst.Add(1.0f / Time.timeScale, 1, exit);
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


#region 原本全部使用Pag实现播放，现在改为Spine
//// Pag
//private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag/ng_pup_BigWin_bmp";
//private GComponent _bigWinCom;
//private PagSlotBinding _bigWinPag;

///// <summary> 对应不同级别BigWin的Pag视频数组 </summary>
//private readonly string[] _pagEffString = { "ng_pup_BigWin_small_bmp.pag", "ng_pup_BigWin_zhong_bmp.pag", "ng_pup_BigWin_da_bmp.pag" };

///// <summary> BigWin中奖类型数组 </summary>
//private readonly string[] _winTypeString = { "BIG", "HUGE", "MASSIVE" };

///// <summary> 每个级别Pag视频的时长 </summary>
//private readonly float[] _pagTimes = { 7.53f, 13f, 17.2f };

//private long _score; // BigWin中奖得分
//private int _winIndex; // 当前中大奖索引
//private bool _isExiting; // 当前动画是否已经播放完成
//private GTextField _bigWinText; // 显示BigWin得分的组件
//private const float ExitDelay = 1.0f; // 每一级Pag的结束等待时间
//private TimerCallback _aniEndCallback, _exitCallback; // pag和数字滚动播放结束之后的回调函数 
//private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

//protected override void OnInit()
//{
//    contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
//    base.OnInit();

//    InitParam(null); // 因为BigWin不需要加载预制体，所以需要将InitParam在OnInit里直接调用，否则无法触发Loading中的回调，导致无法正常进入游戏
//    machineBtnClickHelper = new MachineButtonClickHelper()
//    {
//        shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
//        {
//            [MachineButtonKey.BtnSpin] = (info) =>
//            {
//                Debug.LogError("游戏接受到机台短按的数据：Spin");
//                OnAniEnd(null);
//            }
//        },
//    };
//}

//protected override void OnLanguageChange(I18nLang lang)
//{
//    FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
//    contentPane.Dispose(); // 释放当前UI
//    contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
//    InitParam(null);
//}

//private void InitParam(EventData eventData = null)
//{
//    preLoadedCallback?.Invoke();
//    if (!isOpen) return;

//    // 重置状态
//    _isExiting = false;
//    // 获取UI组件
//    _bigWinText = contentPane.GetChild("score").asTextField;
//    // 绑定Pag视频
//    _bigWinCom = contentPane.GetChild("anchorPag").asCom;
//    _bigWinPag = new PagSlotBinding("bigWin", GamePagFolder);
//    _bigWinPag.EnsureSlot(_bigWinCom);

//    PlayNumAniAndPag();
//}

//public override void OnOpen(PageName currentPageName, EventData eventData)
//{
//    base.OnOpen(currentPageName, eventData);

//    if (eventData?.value is Dictionary<string, object> dic)
//    {
//        // 获取BigWin得分
//        if (dic.TryGetValue("baseGameWinCredit", out var scoreVal) && scoreVal is long longScore)
//            _score = longScore;

//        // 获取BigWin中奖类型索引
//        string winType = dic.TryGetValue("WinType", out var wt) ? wt.ToString() : "";
//        if (_winTypeString.Contains(winType))
//            _winIndex = Array.IndexOf(_winTypeString, winType);
//        if (_winIndex < 0) _winIndex = 0;
//        if (_winIndex > _winTypeString.Length) _winIndex = _winTypeString.Length - 1;
//    }

//    InitParam(eventData);
//}

//public override void OnClose(EventData eventData = null)
//{
//    base.OnClose(eventData);

//    ClearPag();
//    ClearTimers();
//}

///// <summary> 播放数字滚动动画以及对应的Pag视频 </summary>
//private void PlayNumAniAndPag()
//{
//    try
//    {
//        if (_winTypeString.Length < 3)
//        {
//            Debug.LogError("最少有三种BigWin类型");
//            return;
//        }

//        // 播放数字滚动动画
//        _bigWinText.visible = false;
//        float showtime = _pagTimes[_winIndex];

//        // 初始化动画结束之后的回调
//        _exitCallback = OnExit;
//        _aniEndCallback = OnAniEnd;

//        AddTimer(0.5f, (object obj) =>
//        {
//            _bigWinText.visible = true;
//            NumberAnimation.Instance.AnimateNumber(_bigWinText, 0, _score, showtime - 1f);
//        });

//        // 播放对应中奖类型的Pag视频
//        if (_bigWinPag == null) return;
//        _bigWinPag.StopWithDefaults();
//        _bigWinPag.Play(new PagSequencePlay(
//            new[] { new PagSegment(_pagEffString[_winIndex], 1) }, PagPlayLayout.Center,
//            useGpuSyncGroup: false));
//        Timers.inst.Add(showtime, 1, _aniEndCallback);
//    }
//    catch (Exception e)
//    {
//        Debug.LogException(e);
//    }
//}

//private void OnAniEnd(object obj)
//{
//    // 停止数字动画，直接显示最终中奖结果
//    NumberAnimation.Instance.StopAllAnimations();
//    _bigWinText.text = _score.ToString();

//    // 延时播放退出动画
//    if (_isExiting) return;

//    _isExiting = true;
//    if (!Timers.inst.Exists(_exitCallback))
//    {
//        Timers.inst.Add(ExitDelay, 1, _exitCallback);
//    }
//}

//private void OnExit(object obj)
//{
//    CloseSelf(null);
//}

///// <summary> 清除Pag对象，避免造成多余的内存占用</summary>
//private void ClearPag()
//{
//    // _bigWinPag?.Dispose();
//    _bigWinPag = null;
//    if (_bigWinPag != null) _bigWinPag.StopWithDefaults();
//}


///// <summary> 清除对Timers的事件监听，避免造成多余的内存占用</summary>
//private void ClearTimers()
//{
//    if (_aniEndCallback != null && Timers.inst.Exists(_aniEndCallback))
//        Timers.inst.Remove(_aniEndCallback);
//    if (_exitCallback != null && Timers.inst.Exists(_exitCallback))
//        Timers.inst.Remove(_exitCallback);
//}
#endregion