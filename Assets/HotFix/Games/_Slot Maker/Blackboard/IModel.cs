using System.Collections.Generic;
using FairyGUI;
using GameMaker;

namespace SlotMaker
{
    public interface IContentModel
    {
        
        public SlotGameEffect targetSlotGameEffect { get; }

        public PageName pageName { get; }
        
        public bool isRequestToStop { set; }

        public bool isRequestToRealCreditWhenStop { set; }
        public bool isSpin { get; }//{ get; set; }
        public bool isAuto { get; }//{ get; set; }
        public bool isFreeSpin { get; }//{ get; set; }
        public string gameState { get; }//{ get; set; }

        public string btnSpinState { get; }//{ get; set; }


        public long totalBet { get; set; }
        public int betIndex { get; set; }

        public int totalPlaySpins { get; }
        public int remainPlaySpins { get; }


        public GComponent goAnthorPanel { get; set; }

        //public GComponent mainPanel { get; set; }

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; }


        public GComponent[] goPayTableLst { get; }
        public List<List<int>> payLines { get; set; }

        public List<WinMultiple> winLevelMultiple {get; }
        //totalwin  >= totalbet *  100  =>  "ultra"


        public JackpotInfo uiGrandJP { get; }
        public JackpotInfo uiMajorJP { get; }
        public JackpotInfo uiMinorJP { get; }
        public JackpotInfo uiMiniJP { get; }

    }

   
 
    public interface IPanel
    {

        public void OnLongClickHandler(MachineButtonKey machineButtonKey);
        public void OnShortClickHandler(MachineButtonKey machineButtonKey);

        public void OnDownClickHandler(MachineButtonKey machineButtonKey);

        public void OnUpClickHandler(MachineButtonKey machineButtonKey);
    }



    public interface ICustomModel
    {
        /// <summary> 图标宽 </summary>
        public float symbolWidth { get; }

        /// <summary> 图标高 </summary>
        public float symbolHeight { get; }

        /// <summary> 列 </summary>
        public int column { get; }

        /// <summary> 行 </summary>
        public int row { get; }

        public float reelMaxOffsetY { get; }

        /// <summary> 说明页 </summary>
        public string[] payTable { get; }

        /// <summary> 通过图标索引，获取图标真实编号 </summary>
        public List<int> symbolNumber { get; }

        /// <summary> 所有图标个数 </summary>
        public int symbolCount { get; }

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolHitEffect { get; }

        /// <summary> 特殊图标 </summary>
        public List<int> specialHitSymbols { get; }

        /// <summary> 特效图标 - 预制体名称</summary>
        public Dictionary<string, string> symbolAppearEffect { get; }

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolExpectationEffect { get; }

        /// <summary> 边框特效</summary>
        public string borderEffect { get; }

        /// <summary> 图片 - 默认图标</summary>
        public Dictionary<string, string> symbolIcon { get; }

    }

}



/*
public class ContentDefault : IContentModel
{
    public SlotGameEffect targetSlotGameEffect => SlotGameEffect.Default;

    public bool isSpin
    {
        get => false;
        set { }
    }
    public bool isAuto
    {
        get => false;
        set { }
    }

    public bool isFreeSpin
    {
        get => false;
        set { }
    }

    public int totalPlaySpins
    {
        get => 1;
    }
    public int remainPlaySpins
    {
        get => 1;
    }
    public string gameState
    {
        get => GameState.Idle;
        set { }
    }
    public long totalBet
    {
        get => 0;
        set { }
    }
    public int betIndex
    {
        get => 0;
        set { }
    }

    public GComponent goAnthorPanel
    {
        get => null;
        set { }
    }


    public List<PayTableSymbolInfo> payTableSymbolWin
    {
        get => new List<PayTableSymbolInfo>();
        set { }
    }


    public GComponent[] goPayTableLst
    {
        get => new GComponent[0];
    }

    public string btnSpinState { get=> SpinButtonState.Stop; }

    public int reportId
    {
        get => 0;
        set { }
    }

    public int gameNumber
    {
        get => 0;
        set { }
    }

    public List<List<int>> payLines => new List<int[]>();

    public List<WinMultiple> winLevelMultiple => new List<WinMultiple>();
}
*/

