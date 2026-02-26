using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class SingleReelController
    {
        private int _wheelIndex; // 当前滚轴索引
        private readonly GComponent _wheelRootNode; // 滚轴的根节点 elementBox
        private WheelState _reelState = WheelState.None; // 当前滚轴的状态
        private Coroutine _rollCoroutine;

        /// <summary>滚轴上的所有图标信息List</summary>
        public List<GComponent> RollElements = new List<GComponent>();

        /// <summary>滚轴上所有的文本组件</summary>
        public List<GTextField> RewardTexts = new List<GTextField>();

        /// <summary>临时存储滚轴的位置</summary>
        private List<float> _elementStartPosList = new List<float>();

        public string Wheeleward = "";

        public SingleReelController(GComponent wheelRootNode, int wheelIndex)
        {
            _wheelRootNode = wheelRootNode;
            _wheelIndex = wheelIndex;
            InitReel();
        }

        private void InitReel()
        {
            if (_wheelRootNode != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    GComponent parentGCom = _wheelRootNode.GetChild("rollElement_" + (i + 1)).asCom;
                    GTextField rewardText = parentGCom.GetChild("rewardText").asTextField;
                    RollElements.Add(parentGCom);
                    RewardTexts.Add(rewardText);
                    _elementStartPosList.Add(parentGCom.y); // 记录初始位置
                    Wheeleward = RandomReward();
                }
            }
        }

        string RandomReward()
        {
            return ContentModel.Instance.bonusGameRewardList[
                Random.Range(0, ContentModel.Instance.bonusGameRewardList.Count)];
        }

        public void StartRoll(MonoHelper monoHelper, float speed)
        {
            if (_reelState == WheelState.Roll) return;
            _reelState = WheelState.Roll;
            if (_rollCoroutine != null) monoHelper.StopCoroutine(_rollCoroutine);
            _rollCoroutine = monoHelper.StartCoroutine(RollCoroutine(speed));
        }


        public void StopRoll(MonoHelper monoHelper, List<int> winningList)
        {
            if (_reelState != WheelState.Roll) return;
            _reelState = WheelState.Stop;
            if (_rollCoroutine != null) monoHelper.StopCoroutine(_rollCoroutine);
            ResetReelPos();
            if (!winningList.Contains(_wheelIndex))
            {
                _wheelRootNode.GetChild("rollElement_4").asCom.visible = false;
            }
            else
            {
                // _wheelRootNode.GetChild("rollElement_4").asCom.visible = true;
                // RewardTexts[3].text = RandomReward();
            }
        }

        void ResetReelPos()
        {
            for (int i = 0; i < 4; i++)
            {
                GComponent parentGCom = _wheelRootNode.GetChild("rollElement_" + (i + 1)).asCom;
                parentGCom.y = _elementStartPosList[i];
            }
        }

        private IEnumerator RollCoroutine(float speed)
        {
            while (_reelState == WheelState.Roll)
            {
                for (int i = 0; i < RollElements.Count; i++)
                {
                    GComponent elementCom = RollElements[i];
                    float newY = elementCom.y + speed * Time.deltaTime;

                    if (newY > elementCom.height * 3)
                    {
                        newY -= elementCom.height * 4f;
                    }

                    elementCom.y = newY;
                }

                yield return null;
            }
        }
    }

    public enum WheelState
    {
        None,
        Roll,
        Stop,
    }
}