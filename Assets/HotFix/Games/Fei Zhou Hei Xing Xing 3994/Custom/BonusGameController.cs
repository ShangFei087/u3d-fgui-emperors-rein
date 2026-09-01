using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom
{
    public class BonusGameController : BaseManager<BonusGameController>
    {
        /// <summary> 获取当前局的游戏结果信息 </summary>
        public List<BonusReelResultInfo> GetCurrentRoundResultInfo(List<int> currentBonusDataList,
            List<GameObject> objs,
            List<string> icons)
        {
            return currentBonusDataList.Select((t, i) => t switch
                {
                    0 => new BonusReelResultInfo()
                    {
                        WinObj = null,
                        HitScore = 0,
                        Index = i,
                        IconPath = "",
                        Type = BonusResultType.None
                    },
                    20001 => new BonusReelResultInfo()
                    {
                        // 这里只保存预制体引用，不在此实例化。
                        // 实例化统一由 BonusGameReelController.SetWinResult() 在揭示结果时执行，避免重复实例化。
                        WinObj = objs[1],
                        HitScore = 0,
                        Index = i,
                        IconPath = icons[1],
                        Type = BonusResultType.Special
                    },
                    _ => new BonusReelResultInfo()
                    {
                        // 这里只保存预制体引用，不在此实例化。
                        // 实例化统一由 BonusGameReelController.SetWinResult() 在揭示结果时执行，避免重复实例化。
                        WinObj = objs[0],
                        HitScore = t % 10000,
                        Index = i,
                        IconPath = icons[0],
                        Type = BonusResultType.Bonus
                    }
                })
                .ToList();
        }

        /// <summary> 初始化彩金游戏滚动卷轴 </summary>
        public List<BonusReelController> InitBonusOnceData(GComponent reelNode,
            Dictionary<string, string> iconPaths,
            List<BonusReelResultInfo> resultInfoList)
        {
            List<BonusReelController> reels = new List<BonusReelController>();
            for (int i = 0; i < reelNode.numChildren; i++)
            {
                BonusReelResultInfo resultInfo = resultInfoList[i];
                GComponent node = reelNode.GetChildAt(i).asCom;
                BonusReelController reel = new BonusReelController(node, i, iconPaths, resultInfo);
                reels.Add(reel);
            }

            return reels;
        }


        /// <summary> 一次BonusGame游戏 </summary>
        public IEnumerator BonusGameOnce(List<BonusReelController> reelControllers, Action onOnceCompleted)
        {
            int completedCount = 0;
            int totalCount = reelControllers.Count;
            bool isCompleted = false;

            Action callBack = () =>
            {
                completedCount++;
                if (completedCount >= totalCount && !isCompleted)
                {
                    isCompleted = true;
                }
            };
            foreach (BonusReelController reelController in reelControllers)
            {
                switch (reelController.ResultInfo.Type)
                {
                    case BonusResultType.None:
                        {
                            reelController.NormalRoll((() =>
                            {
                                callBack.Invoke();
                            }));
                        }
                        break;
                    case BonusResultType.Bonus:
                        {
                            reelController.HitRoll((() =>
                            {
                                callBack.Invoke();
                            }));
                        }
                        break;
                    case BonusResultType.Special:
                        {
                            reelController.HitRoll((() =>
                            {
                                callBack.Invoke();
                            }));
                        }

                        break;
                }
            }

            yield return new WaitUntil((() => isCompleted == true));
            onOnceCompleted?.Invoke();
        }

        /// <summary> 更新UI显示 </summary>
        public void UpdateUIShow(GTextField uiText, int count)
        {
            uiText.text = count.ToString();
        }

        /// <summary> 每局获取彩金数据之前，清空上一次的数据 </summary>
        public void ResetBonusData(List<BonusReelController> onceBonusReels)
        {
            if (onceBonusReels == null || onceBonusReels.Count == 0) return;
            foreach (BonusReelController controller in onceBonusReels)
            {
                controller.Reset();
            }

            onceBonusReels.Clear();
        }
    }
}