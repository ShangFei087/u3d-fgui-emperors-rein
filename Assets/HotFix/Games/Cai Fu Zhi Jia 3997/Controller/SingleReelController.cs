using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaiFuZhiJia_3997
{
    public class SingleReelController
    {
        private readonly int _wheelIndex; // 当前滚轴索引

        private readonly GComponent _wheelRootNode; // 滚轴的根节点 elementBox

        private readonly Transition _rollTransition;
        private readonly Transition _backTransition;
        public readonly Transition ResetTransition;
        // private GComponent _backCom;

        /// <summary>滚轴上的所有图标信息List</summary>
        public readonly List<GComponent> RollElements = new List<GComponent>();

        /// <summary>滚轴上所有的文本组件</summary>
        public readonly List<GTextField> RewardTexts = new List<GTextField>();

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
                for (int i = 0; i < 5; i++)
                {
                    GComponent parentGCom = _wheelRootNode.GetChild("rollElement_" + (i + 1)).asCom;
                    GTextField rewardText = parentGCom.GetChild("rewardText").asTextField;
                    if (i == 1)
                        rewardText.text = Random.Range(100, 900).ToString();
                    GLoader elementLoader = parentGCom.GetChild("element").asLoader;
                    elementLoader.url = CustomModel.Instance.JackpotBgPath[Random.Range(0,1)];
                    RollElements.Add(parentGCom);
                    RewardTexts.Add(rewardText);
                }

                // _backCom = _wheelRootNode.GetChild("result").asCom;
            }
        }

        public void StartRoll(float speed)
        {
            // _rollTransition.timeScale = speed;
            // PlayWithLoops();

            // _backTransition.timeScale = speed;
            _backTransition.Play();
        }


        public void StopRoll()
        {
            _rollTransition.Stop();
            _rollTransition.timeScale = 1f;
            // _rollTransition.Stop(true);
        }

        public void PlayBack(Action callback)
        {
            // Debug.LogError($"[{_wheelIndex}] back 播放前 isPlaying={_backTransition.playing}");

            // GComponent result = _wheelRootNode.GetChild("result").asCom;
            // if (result != null)
            // {
            //     result.visible = true;
            //     result.alpha = 1;
            // }

            _backTransition.Play(() =>
            {
                // Debug.LogError($"[{_wheelIndex}] back 播放完成回调触发");
                callback?.Invoke();
            });

            // Debug.LogError($"[{_wheelIndex}] back 播放后 isPlaying={_backTransition.playing}");
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
    }
}