using FairyGUI;
using Spine;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaiFuHuoChe_3996
{
    public enum SmallReelState
    {
        Idle,
        Rolling,

        /// <summary>已揭示中奖结果</summary>
        Revealed,

        /// <summary>已结算（次数用完，未中奖）</summary>
        Settled
    }

    public enum SmallResultType
    {
        None,
        Money,
        Jackpot
    }

    public class SmallReelResultInfo
    {
        public int reelIndex; // 0-14

        /// <summary>行 0-2</summary>
        public int row;

        /// <summary>列 0-4</summary>
        public int col;

        public SmallResultType type;
        public int rewardValue; // 奖励数值
        public int jackpotType; // 彩金类型（如果是彩金）
        public string iconUrl;
        public string rewardText;
        public int anchorChildIndex; // 中奖的预制体索引
    }

    public class SmallGameSymbol
    {
        public readonly GLoader element;
        public readonly GComponent anchor;
        public readonly GTextField rewardText;
        public GGraph mask;

        public SmallGameSymbol(GComponent rollElementNode)
        {
            element = rollElementNode.GetChild("element").asLoader;
            rewardText = rollElementNode.GetChild("rewardText").asTextField;
            anchor = rollElementNode.GetChild("anchorSmallGameDiamond").asCom;
            mask = rollElementNode.GetChild("mask").asGraph;
            SetMask(false);
        }

        public void Clear()
        {
            element.url = string.Empty;
            rewardText.text = string.Empty;
            GameCommon.FguiUtils.DeleteWrapper(anchor);
        }

        public void SetMask(bool setVisial = false)
        {
            mask.visible = setVisial;
        }

        public void RemoveAnchor()
        {
            rewardText.text = string.Empty;
            GameCommon.FguiUtils.DeleteWrapper(anchor);
        }
    }

    public class SmallGameReelController
    {
        public int ReelIndex { get; private set; }
        public SmallReelState State { get; private set; }
        private SmallReelResultInfo ResultInfo { get; set; }

        private GameObject _anchorPrefab;
        private Animator anchorAnim;

        private List<TimerCallback> _activeTimers = new List<TimerCallback>(); // 活跃定时器列表

        public readonly SmallGameSymbol result;
        private readonly SmallGameSymbol[] _rollElements = new SmallGameSymbol[3];
        public GComponent rollElement;
        public GGraph mask;

        private readonly List<string> iconUrlList = new List<string>()
        {
            "ui://CaiFuHuoChe_3996/symbol_1",
            "ui://CaiFuHuoChe_3996/symbol_2",
            "ui://CaiFuHuoChe_3996/symbol_3",
            "ui://CaiFuHuoChe_3996/symbol_4",
            "ui://CaiFuHuoChe_3996/symbol_5",
            "ui://CaiFuHuoChe_3996/symbol_6",
            "ui://CaiFuHuoChe_3996/symbol_7",
            "ui://CaiFuHuoChe_3996/symbol_8",
            "ui://CaiFuHuoChe_3996/symbol_9",
            "ui://CaiFuHuoChe_3996/symbol_13",
            "ui://CaiFuHuoChe_3996/symbol_16",
            "ui://CaiFuHuoChe_3996/symbol_15",
            "ui://CaiFuHuoChe_3996/symbol_14",
        };

        private readonly Transition _rollTrans, _rollResetTrans, _resultTrans, _resultResetTrans;

        public SmallGameReelController(GComponent elementBoxNode, int index)
        {
            ReelIndex = index;
            State = SmallReelState.Idle;

            _rollTrans = elementBoxNode.GetTransition("roll");
            _rollResetTrans = elementBoxNode.GetTransition("rollReset");
            _resultTrans = elementBoxNode.GetTransition("result");
            _resultResetTrans = elementBoxNode.GetTransition("resultReset");

            for (int i = 0; i < 3; i++)
            {
                GComponent rollElementNode = elementBoxNode.GetChild("rollElement_" + (i + 1)).asCom;
                _rollElements[i] = new SmallGameSymbol(rollElementNode);
            }

            rollElement = elementBoxNode.GetChild("rollElement_4").asCom;

            result = new SmallGameSymbol(elementBoxNode.GetChild("result").asCom);
            mask = elementBoxNode.GetChild("mask").asGraph;
            mask.visible = true;
        }

        private string GetUrl(List<string> str)
        {
            if (str == null || str.Count == 0)
                return iconUrlList[Random.Range(0, 10)];
            float randRate = Random.Range(0, 1f);
            string tempStr = randRate < 0.2f
                ? str[Random.Range(10, str.Count)]
                : str[Random.Range(0, 10)];
            return tempStr;
        }

        public void SetRollingVisual()
        {
            if (State != SmallReelState.Idle) return;
            foreach (SmallGameSymbol rollElement in _rollElements)
            {
                string iconUrl = GetUrl(iconUrlList);
                rollElement.element.url = iconUrl;
                rollElement.rewardText.text =
                    iconUrl.Contains("symbol_13") ? Random.Range(0, 1000).ToString() : string.Empty;
            }
        }

        /// <summary>
        /// 播放中奖滚动动画（第一圈roll，第二圈result）
        /// </summary>
        public void PlayHitRoll(float rollSpeed, float resultSpeed, Action onComplete)
        {
            if (State == SmallReelState.Revealed || State == SmallReelState.Settled)
            {
                onComplete?.Invoke();
                return;
            }

            State = SmallReelState.Rolling;

            // 第一圈：roll
            _rollTrans.Stop();
            _rollTrans.timeScale = rollSpeed;
            _rollTrans.Play(() =>
            {
                // 第二圈：result（揭示结果）
                State = SmallReelState.Revealed;

                result.element.url = ResultInfo.iconUrl;
                if (ResultInfo.type == SmallResultType.Money)
                    result.rewardText.text = ResultInfo.rewardText;
                else
                    result.rewardText.text = string.Empty;

                if (_anchorPrefab != null)
                {
                    GameCommon.FguiUtils.DeleteWrapper(result.anchor);
                    SetAnchorChildActive(_anchorPrefab, ResultInfo.anchorChildIndex);
                    _anchorPrefab.SetActive(false);
                    GameCommon.FguiUtils.AddWrapper(result.anchor, _anchorPrefab);
                }

                _resultTrans.Stop();
                _resultTrans.timeScale = resultSpeed /*- 0.9f*/;
                rollElement.GetChild("element").asLoader.url = string.Empty;
                _resultTrans.Play(() =>
                {
                    result.element.url = iconUrlList[9];
                    _anchorPrefab.SetActive(true);
                    onComplete?.Invoke();
                });
            });
        }

        /// <summary>
        /// 播放普通滚动动画（两圈roll）
        /// </summary>
        public void PlayNormalRoll(float speed, Action onComplete)
        {
            if (State == SmallReelState.Revealed || State == SmallReelState.Settled)
            {
                onComplete?.Invoke();
                return;
            }

            State = SmallReelState.Rolling;

            // 第一圈roll
            _rollTrans.Stop();
            _rollTrans.timeScale = speed;
            _rollTrans.Play(() =>
            {
                // 第二圈roll
                _rollTrans.Stop();
                _rollTrans.timeScale = speed;
                _rollTrans.Play(() =>
                {
                    if (State == SmallReelState.Rolling)
                        State = SmallReelState.Idle;
                    onComplete?.Invoke();
                });
            });

            AddTimer(0.3f, (object obj) =>
            {
                rollElement.GetChild("element").asLoader.url = iconUrlList[Random.Range(0, 9)];
            });
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

        public void SetResultData(SmallReelResultInfo resultInfo, GameObject gameObject)
        {
            ResultInfo = resultInfo;
            _anchorPrefab = gameObject;
        }

        public void PlayRollReset()
        {
            if (State == SmallReelState.Revealed) return;

            _rollResetTrans?.Stop();
            _rollResetTrans?.Play();

            foreach (SmallGameSymbol rollElement in _rollElements)
            {
                rollElement.element.url = string.Empty;
                rollElement.rewardText.text = string.Empty;
            }
        }

        private void SetAnchorChildActive(GameObject obj, int targetIndex)
        {
            if (obj == null) return;
            for (int i = 0; i < obj.transform.GetChild(0).childCount; i++)
            {
                GameObject prefab = obj.transform.GetChild(0).GetChild(i).gameObject;
                prefab.SetActive(i == targetIndex);
                if(i == targetIndex)
                {
                    anchorAnim = prefab.GetComponent<Animator>();
                }
            }
        }

        public void Reset()
        {
            State = SmallReelState.Idle;
            ResultInfo = null;
            _anchorPrefab = null; // 清除预制体引用

            _rollTrans.Stop();
            _resultTrans.Stop();
            _rollResetTrans.Play();
            _resultResetTrans.Play();

            result.Clear();
            foreach (SmallGameSymbol rollElement in _rollElements)
            {
                rollElement.element.url = string.Empty;
                rollElement.rewardText.text = string.Empty;
            }
        }

        public void PlayAnim(string animName)
        {
            anchorAnim.Rebind();
            anchorAnim.Play(animName);
            anchorAnim.Update(0f);
        }
    }
}