using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaiFuZhiJia_3997
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
        RedDiamond,
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

        public SmallGameSymbol(GComponent rollElementNode)
        {
            element = rollElementNode.GetChild("element").asLoader;
            rewardText = rollElementNode.GetChild("rewardText").asTextField;
            anchor = rollElementNode.GetChild("anchorSmallGameDiamond").asCom;
        }

        public void Clear()
        {
            element.url = string.Empty;
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

        public readonly SmallGameSymbol result;
        private readonly SmallGameSymbol[] _rollElements = new SmallGameSymbol[3];

        private readonly List<string> iconUrlList = new List<string>()
        {
            "ui://CaiFuZhiJia/ng_sym_null",
            "ui://CaiFuZhiJia/ng_sym13_jiaj",
            "ui://CaiFuZhiJia/ng_sym_14_minor",
            "ui://CaiFuZhiJia/ng_sym_14_major",
            "ui://CaiFuZhiJia/ng_sym14_mini",
        };

        private const string DefaultUrl = "ui://CaiFuZhiJia/ng_sym_null";
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

            result = new SmallGameSymbol(elementBoxNode.GetChild("result").asCom);
        }

        private string GetUrl(List<string> str)
        {
            if (str == null || str.Count == 0)
                return DefaultUrl;
            float randRate = Random.Range(0, 1f);
            string tempStr = randRate < 0.2f
                ? str[Random.Range(3, str.Count)]
                : str[Random.Range(0, 3)];
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
                    iconUrl.Contains("diamonds2") ? Random.Range(0, 1000).ToString() : string.Empty;
            }
        }

        /// <summary>
        /// 播放中奖滚动动画（第一圈roll，第二圈result）
        /// </summary>
        public void PlayHitRoll(float rollSpeed, float resultSpeed, Action onComplete, Action hitJackpotComplete)
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
                result.rewardText.text =
                    ResultInfo.type == SmallResultType.RedDiamond ? ResultInfo.rewardText : string.Empty;

                if (_anchorPrefab != null)
                {
                    GameCommon.FguiUtils.DeleteWrapper(result.anchor);
                    SetAnchorChildActive(_anchorPrefab, ResultInfo.anchorChildIndex);
                    _anchorPrefab.SetActive(false);
                    GameCommon.FguiUtils.AddWrapper(result.anchor, _anchorPrefab);
                }

                _resultTrans.Stop();
                _resultTrans.timeScale = resultSpeed;
                _resultTrans.Play(() =>
                {
                    if (result.rewardText.text == string.Empty) hitJackpotComplete?.Invoke();
                    result.element.url = DefaultUrl;
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
                obj.transform.GetChild(0).GetChild(i).gameObject.SetActive(i == targetIndex);
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
    }
}