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
        private readonly int _reelIndex;
        private readonly GComponent _wheelRootNode; // 滚轴的根节点 elementBox
        private readonly Transition _rollTransition;
        private readonly Transition _backTransition;
        public readonly Transition ResetTransition;
        public readonly Transition BackResetTransition;

        /// <summary>滚轴上的所有图标信息List</summary>
        public readonly List<GComponent> RollElements = new List<GComponent>();

        /// <summary>滚轴上所有的文本组件</summary>
        public readonly List<GTextField> RewardTexts = new List<GTextField>();

        public SingleReelController(GComponent wheelRootNode, int reelIndex)
        {
            _wheelRootNode = wheelRootNode;
            _reelIndex = reelIndex;
            _rollTransition = _wheelRootNode.GetTransition("roll");
            _backTransition = _wheelRootNode.GetTransition("back");
            ResetTransition = _wheelRootNode.GetTransition("reset");
            BackResetTransition = _wheelRootNode.GetTransition("backReset");
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

                    GLoader elementLoader = parentGCom.GetChild("element").asLoader;
                    if (i != 4)
                    {
                        elementLoader.url =
                            CustomModel.Instance.JackpotBgPath[
                                Random.Range(0, CustomModel.Instance.JackpotBgPath.Count)];
                        // if (elementLoader.url == "ui://CaiFuZhiJia/ng_sym_diamonds2")
                        //     rewardText.text = Random.Range(100, 900).ToString();
                    }
                    else
                        elementLoader.url = CustomModel.Instance.JackpotTypePath[0]; // 默认是没中奖类型

                    RollElements.Add(parentGCom);
                    RewardTexts.Add(rewardText);
                }
            }
        }

        public void StartRoll(float speed)
        {
            _rollTransition.timeScale = speed;
            PlayWithLoops();
        }


        public void StopRoll(List<int> winList, Action callback)
        {
            _rollTransition.Stop();
            _rollTransition.timeScale = 1f;
            callback?.Invoke();
        }

        public void PlayBack(Action callback, string normalBet)
        {
            _wheelRootNode.GetChild("rollElement_5").asCom.GetChild("element").asLoader.url =
                CustomModel.Instance.JackpotBgPath[2];
            _wheelRootNode.GetChild("rollElement_5").asCom.GetChild("rewardText").asTextField.text = normalBet;

            _backTransition.Play(() =>
            {
                BackResetTransition.timeScale = 2f;
                BackResetTransition.Play(() => callback?.Invoke());
            });
        }

        public void PlayBack(Action callback, int winType)
        {
            _wheelRootNode.GetChild("rollElement_5").asCom.GetChild("element").asLoader.url =
                CustomModel.Instance.JackpotTypePath[winType];
            _wheelRootNode.GetChild("rollElement_5").asCom.GetChild("rewardText").asTextField.text = "";
            _backTransition.Play(() =>
            {
                BackResetTransition.timeScale = 2f;
                BackResetTransition.Play(() => callback?.Invoke());
            });
        }

        void PlayWithLoops(int maxLoops = 3)
        {
            int playCount = 0;
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