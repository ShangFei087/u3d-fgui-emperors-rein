using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class SingleReelController
    {
        private readonly int _wheelIndex; // 当前滚轴索引
        private readonly GComponent _wheelRootNode; // 滚轴的根节点 elementBox
        private WheelState _reelState = WheelState.None; // 当前滚轴的状态
        private Coroutine _rollCoroutine;

        /// <summary>滚轴上的所有图标信息List</summary>
        public readonly List<GComponent> RollElements = new List<GComponent>();

        /// <summary>滚轴上所有的文本组件</summary>
        public readonly List<GTextField> RewardTexts = new List<GTextField>();

        /// <summary>临时存储滚轴的位置</summary>
        private readonly List<float> _elementStartPosList = new List<float>();

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
                    GLoader elementLoader = parentGCom.GetChild("element").asLoader;
                    elementLoader.url = CustomModel.Instance.JackpotBgPath[i];
                    RollElements.Add(parentGCom);
                    RewardTexts.Add(rewardText);
                    _elementStartPosList.Add(parentGCom.y); // 记录初始位置
                }
            }
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
            // ResetReelPos();  原来的
            if (!winningList.Contains(_wheelIndex))
            {
                ResetReelPos();
                _wheelRootNode.GetChild("rollElement_4").asCom.visible = false;
            }
            else
            {
                // _wheelRootNode.GetChild("rollElement_4").asCom.visible = true;
                // RewardTexts[3].text = RandomReward();
                _rollCoroutine = monoHelper.StartCoroutine(BounceCoroutine(monoHelper));
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
        
        /// <summary>
        /// 新增回弹功能
        /// </summary>
        /// <param name="monoHelper"></param>
        /// <returns></returns>
        private IEnumerator BounceCoroutine(MonoHelper monoHelper)
        {
            // 记录当前位置
            List<float> currentPositions = new List<float>();
            for (int i = 0; i < RollElements.Count; i++)
            {
                currentPositions.Add(RollElements[i].y);
            }
    
            // 计算回弹距离（一个元素的高度）
            float bounceDistance = RollElements[0].height;
    
            // 回弹动画参数
            float bounceDuration = 0.3f; // 回弹持续时间
            float elapsedTime = 0f;
    
            // 执行回弹动画
            while (elapsedTime < bounceDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / bounceDuration;
        
                // 使用缓动函数使回弹更自然
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
        
                // 更新所有元素位置
                for (int i = 0; i < RollElements.Count; i++)
                {
                    RollElements[i].y = currentPositions[i] + bounceDistance * easedProgress;
                }
        
                yield return null;
            }
    
            // 确保最终位置准确
            for (int i = 0; i < RollElements.Count; i++)
            {
                RollElements[i].y = currentPositions[i] + bounceDistance;
            }
    
            // 显示中奖元素
            _wheelRootNode.GetChild("rollElement_4").asCom.visible = true;
            // RewardTexts[3].text = RandomReward();
    
            // 短暂延迟后恢复初始位置
            yield return new WaitForSeconds(0.5f);
    
            // 恢复到初始位置
            ResetReelPos();
        }
    }

    // public enum WheelState
    // {
    //     None,
    //     Roll,
    //     Stop,
    // }
}