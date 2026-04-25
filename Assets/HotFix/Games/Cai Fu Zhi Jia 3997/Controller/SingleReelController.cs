using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class SingleReelController
    {
        private readonly int _wheelIndex; // 当前滚轴索引

        private readonly GComponent _wheelRootNode; // 滚轴的根节点 elementBox
        // private WheelState _reelState = WheelState.None; // 当前滚轴的状态
        // private Coroutine _rollCoroutine;

        private readonly Transition _rollTransition;
        private Transition _backTransition;
        public Transition ResetTransition;

        /// <summary>滚轴上的所有图标信息List</summary>
        public readonly List<GComponent> RollElements = new List<GComponent>();

        /// <summary>滚轴上所有的文本组件</summary>
        public readonly List<GTextField> RewardTexts = new List<GTextField>();

        // /// <summary>临时存储滚轴的位置</summary>
        private readonly List<float> _elementStartPosList = new List<float>();

        public SingleReelController(GComponent wheelRootNode, int wheelIndex)
        {
            _wheelRootNode = wheelRootNode;
            _wheelIndex = wheelIndex;
            _rollTransition = _wheelRootNode.GetTransition("roll");
            _backTransition = _wheelRootNode.GetTransition("back");
            ResetTransition = _wheelRootNode.GetTransition("reset");
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

        public void StartRoll( /*MonoHelper monoHelper,*/ float speed)
        {
            // if (_reelState == WheelState.Roll) return;
            // _reelState = WheelState.Roll;
            // if (_rollCoroutine != null) monoHelper.StopCoroutine(_rollCoroutine);
            // _rollCoroutine = monoHelper.StartCoroutine(RollCoroutine(speed));
            // _rollTransition.repeatCount = 0;
            _rollTransition.timeScale = speed;
            // _rollTransition.timeScale = 0.5f;
            // _rollTransition.Play();
            PlayWithLoops();
        }


        public void StopRoll(MonoHelper monoHelper, List<int> winedIndexList)
        {
            // if (_reelState != WheelState.Roll) return;
            // _reelState = WheelState.Stop;
            _rollTransition.Stop();
            // ResetTransition.Play();

            // ResetReelPos();
            if (winedIndexList.Contains(_wheelIndex))
            {
                _backTransition.Play();
            }
            // if (_rollCoroutine != null) monoHelper.StopCoroutine(_rollCoroutine);
            // if (!winedIndexList.Contains(_wheelIndex))
            // {
            //     // ResetReelPos();
            //     _rollTransition.Stop();
            //     _wheelRootNode.GetChild("rollElement_4").asCom.visible = false;
            // }
            // else
            // {
            //     _rollTransition.Stop();
            //     _backTransition.Play();
            //     // _rollCoroutine = monoHelper.StartCoroutine(BounceCoroutine());
            // }
        }

        public void StopRoll()
        {
            _rollTransition.Stop();
        }

        public void PlayBack(Action callback)
        {
            _backTransition.Play(() => callback?.Invoke());
        }


        void PlayWithLoops()
        {
            int playCount = 0;
            int maxLoops = 3;
            _rollTransition.Play(() =>
            {
                playCount++;
                if (playCount < maxLoops)
                {
                    _rollTransition.Play(); // 再次播放
                }
                else
                {
                    playCount = 0; // 重置
                }
            });
        }

        void ResetReelPos()
        {
            for (int i = 0; i < 4; i++)
            {
                GComponent parentGCom = _wheelRootNode.GetChild("rollElement_" + (i + 1)).asCom;
                parentGCom.y = _elementStartPosList[i];
            }
        }

        // private IEnumerator RollCoroutine(float speed)
        // {
        //     while (_reelState == WheelState.Roll)
        //     {
        //         for (int i = 0; i < RollElements.Count; i++)
        //         {
        //             GComponent elementCom = RollElements[i];
        //             float newY = elementCom.y + speed * Time.deltaTime;
        //
        //             if (newY > elementCom.height * 3)
        //             {
        //                 newY -= elementCom.height * 4f;
        //             }
        //
        //             elementCom.y = newY;
        //         }
        //
        //         yield return null;
        //     }
        // }
    }

    public enum WheelState
    {
        None,
        Roll,
        Stop,
    }
}