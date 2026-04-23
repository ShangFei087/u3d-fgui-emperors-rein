using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public enum WheelState
    {
        None,
        Roll,
        Stop,
    }
    
    public class SingleReelControl
    {
        private readonly int _reelIndex; // 当前滚轴索引
        private readonly GComponent _elementBox; // 滚轴的根节点 elementBox
        private WheelState _reelState = WheelState.None; // 当前滚轴的状态
    }
}

