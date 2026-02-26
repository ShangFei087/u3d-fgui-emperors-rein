using FairyGUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SlotMaker
{
    public enum ReelState
    {
        /// <summary> 空闲中 </summary>
        Idle,
        /// <summary> 开始滚动 </summary>
        StartTurn,
        /// <summary> 开始停止滚动 </summary>
        StartStop,
        /// <summary> 滚动结束 </summary>
        EndStop,
    }
    public class ReelBase : MonoBehaviour
    {

        protected ICustomModel customModel;

        /// <summary> 滚轮索引号 </summary>
        public int reelIndex = 0;

        /// <summary> 滚轮状态 </summary>
        public ReelState state = ReelState.Idle;

        public List<SymbolBase> symbolList;

        public GComponent goOwnerReel;
        public GComponent goSymbols;
        
        /// <summary> the anchor for "symbol hit" or "symbol appear"</summary>
        public GComponent goExpectation;
        
        public virtual void Init(ICustomModel customModel, GComponent gOwnerReel,GComponent gExpectation)
        {
            this.customModel = customModel;
            goOwnerReel = gOwnerReel;
            goExpectation = gExpectation;
        }

        public virtual void SetReelDeck(string reelValue = null)
        {
            DebugUtils.LogWarning("==@ BaseReel - SetDeckReel");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rollTime"> 滚动圈数 </param>
        /// <param name="action"> 完成回调 </param>
        public virtual void StartTurn(int rollTime, UnityAction action)
        {
            DebugUtils.LogWarning("==@ BaseReel - StartRoll");
        }


        /// <summary>
        /// 滚动则停止，没有滚动则滚动一次
        /// </summary>
        /// <param name="action"></param>
        public virtual void ReelToStopOrTurnOnce(UnityAction action)
        {
            DebugUtils.LogWarning("==@ BaseReel - ReelToStopOrTurnOnce");
        }

        /// <summary>
        /// 滚动则停止，显示最后的结果
        /// </summary>
        /// <param name="action"></param>
        public virtual void ReelSetEndImmediately(UnityAction action = null)
        {
            DebugUtils.LogWarning("==@ BaseReel - ReelSetEndImmediately");
        }


        public virtual void SetResult(List<int> result)
        {
            DebugUtils.LogWarning("==@ BaseReel - SetResult");
        }




        /////////////////////
        public virtual void SymbolAppearEffect()
        {
            DebugUtils.LogWarning("==@ BaseReel - SpecialSymbolEffect");
        }

        public virtual void SetReelState(ReelState state = ReelState.Idle)
        {
            DebugUtils.LogWarning("==@ BaseReel - SetReelState");
        }
    }
}

