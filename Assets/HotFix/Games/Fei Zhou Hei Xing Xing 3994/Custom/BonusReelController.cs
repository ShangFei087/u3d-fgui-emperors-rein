using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom
{
    public enum BonusReelState
    {
        /// <summary> 滚轮静止，可以开启滚动 </summary>
        Idle,

        /// <summary> 滚轮正在滚动 </summary>
        Rolling,

        /// <summary> 滚轮滚动结束 </summary>
        Settling,

        /// <summary> 滚轮揭示结果-中奖 </summary>
        Win,

        /// <summary> 滚轮揭示结果-未中奖 </summary>
        Lose,
    }

    public enum BonusResultType
    {
        /// <summary> 没有中奖 </summary>
        None,

        /// <summary> 中特殊图标 </summary>
        Special,

        /// <summary> 中正常数字 </summary>
        Bonus,

        /// <summary> 中Mini彩金 </summary>
        Mini,

        /// <summary> 中Minor彩金 </summary>
        Minor,

        /// <summary> 中Major彩金 </summary>
        Major
    }

    public class BonusReelResultInfo
    {
        /// <summary> 滚轮索引 </summary>
        public int Index;

        /// <summary> 中奖得分 </summary>
        public int HitScore;

        /// <summary> 图标路径 </summary>
        public string IconPath;

        /// <summary> 中奖类型 </summary>
        public BonusResultType Type;

        /// <summary> 中奖预制体 </summary>
        public GameObject WinObj;
    }

    /// <summary> 彩金游戏中滚动图标类 </summary>
    public class BonusGameSymbol
    {
        public readonly GLoader IconLoader;
        public readonly GComponent ObjAnchor;
        public readonly GTextField ScoreText;

        public BonusGameSymbol(GComponent rootNode)
        {
            ObjAnchor = rootNode.GetChildAt(0).asCom;
            IconLoader = rootNode.GetChildAt(1).asLoader;
            ScoreText = rootNode.GetChildAt(2).asTextField;
        }

        public void Dispose()
        {
            IconLoader.url = string.Empty;
            ScoreText.text = string.Empty;
            GameCommon.FguiUtils.DeleteWrapper(ObjAnchor);
        }
    }

    public class BonusReelController
    {
        private BonusReelState _reelState;
        public readonly BonusGameSymbol ResultSymbol;
        private readonly Transition _roll, _rollReset, _result, _resultReset;
        private readonly BonusGameSymbol[] _rollSymbols = new BonusGameSymbol[4];
        public BonusReelResultInfo ResultInfo = new BonusReelResultInfo();
        public GameObject ResultObj;

        /// <summary> 参考非洲黑猩猩的彩金滚轮初始化，卷轴上只显示正常的游戏图标 </summary>
        public BonusReelController(GComponent reelRootNode, int reelIndex, Dictionary<string, string> iconPaths,
            BonusReelResultInfo resultInfo)
        {
            ResultInfo.Index = reelIndex;
            _reelState = BonusReelState.Idle;

            _roll = reelRootNode.GetTransition("roll");
            _rollReset = reelRootNode.GetTransition("rollReset");
            _result = reelRootNode.GetTransition("result");
            _resultReset = reelRootNode.GetTransition("resultReset");

            for (int i = 0; i < _rollSymbols.Length; i++)
            {
                GComponent rollElementNode = reelRootNode.GetChildAt(i).asCom;
                _rollSymbols[i] = new BonusGameSymbol(rollElementNode);
            }

            SetRollIcon(iconPaths);

            GComponent resultCom = reelRootNode.GetChildAt(reelRootNode.numChildren - 1).asCom;
            ResultSymbol = new BonusGameSymbol(resultCom);
            SetResultInfo(resultInfo);
        }

        public void NormalRoll(Action onRollCompleted)
        {
            if (_reelState != BonusReelState.Idle) return;
            _reelState = BonusReelState.Rolling;
            _roll.Stop();
            _roll.Play(() =>
            {
                _roll.Stop();
                _roll.Play(() =>
                {
                    if (_reelState == BonusReelState.Rolling)
                        _reelState = BonusReelState.Idle;
                    onRollCompleted?.Invoke();
                });
            });
        }

        public void HitRoll(Action onRollCompleted)
        {
            if (_reelState != BonusReelState.Idle) return;
            _reelState = BonusReelState.Rolling;
            _roll.Stop();
            _roll.Play(() =>
            {
                SetWinResult();
                _result.Stop();
                _result.Play(() =>
                {
                    ResultSymbol.IconLoader.url = null;
                    ResultSymbol.ObjAnchor.visible = true;
                    onRollCompleted?.Invoke();
                });
            });
        }

        private void SetWinResult()
        {
            if (ResultInfo.WinObj == null) return;
            _reelState = BonusReelState.Win;
            ResultSymbol.IconLoader.url = ResultInfo.IconPath;
            if (ResultInfo.Type != BonusResultType.Special)
                ResultSymbol.ScoreText.text = ResultInfo.HitScore.ToString();

            // 唯一实例化入口：在揭示结果时才克隆预制体，避免与 SetResultInfo 重复实例化
            GameCommon.FguiUtils.DeleteWrapper(ResultSymbol.ObjAnchor);
            ResultObj = Object.Instantiate(ResultInfo.WinObj);
            GameCommon.FguiUtils.AddWrapper(ResultSymbol.ObjAnchor, ResultObj);
            ResultSymbol.ObjAnchor.visible = false;
        }


        private void SetRollIcon(Dictionary<string, string> iconPaths)
        {
            if (_reelState != BonusReelState.Idle) return;
            foreach (BonusGameSymbol symbol in _rollSymbols)
            {
                int iconIndex = Random.Range(0, iconPaths.Count - 3);
                string iconPath = iconPaths[iconIndex.ToString()];
                symbol.IconLoader.url = iconPath;
            }
        }

        private void SetResultInfo(BonusReelResultInfo resultInfo)
        {
            if (_reelState != BonusReelState.Idle) return;
            if (resultInfo.Type == BonusResultType.None) return;

            // 仅保存结果数据，不在构造阶段实例化。
            // 实例化统一由 SetWinResult() 在滚轮揭晓时执行一次，避免 WinObj 被重复实例化。
            ResultInfo.Type = resultInfo.Type;
            ResultInfo.WinObj = resultInfo.WinObj;
            ResultInfo.IconPath = resultInfo.IconPath;
            ResultInfo.HitScore = resultInfo.HitScore;
            ResultSymbol.IconLoader.url = resultInfo.IconPath;
            ResultSymbol.ScoreText.text = resultInfo.Type == BonusResultType.Bonus
                ? resultInfo.HitScore.ToString()
                : string.Empty;
        }

        public void Reset()
        {
            _reelState = BonusReelState.Idle;
            ResultInfo = null;

            _roll.Stop();
            _result.Stop();
            _rollReset.Play();
            _resultReset.Play();

            ResultSymbol.Dispose();
            foreach (BonusGameSymbol rollSymbol in _rollSymbols)
                rollSymbol.Dispose();
        }
    }
}