using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
namespace SlotMaker
{


    /// <summary> 单个Bonus的数据 </summary>
    [System.Serializable]
    public class SymbolBonus
    {
        public List<Cell> cells = new List<Cell>();
        public long Credit = 0;
    }

}