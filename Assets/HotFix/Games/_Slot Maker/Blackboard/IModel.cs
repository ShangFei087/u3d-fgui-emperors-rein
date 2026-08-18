using System.Collections.Generic;
using FairyGUI;
using GameMaker;

namespace SlotMaker
{
    /// <summary> 局内运行时状态：随每局、每次 Spin 等变化的数据模型。 </summary>
    public interface IContentModel
    {
        /// <summary> 目标机台特效类型（用于选择表现分支等）。 </summary>
        public SlotGameEffect targetSlotGameEffect { get; }

        /// <summary> 当前游戏主界面在框架中的页面枚举。 </summary>
        public PageName pageName { get; }

        /// <summary> 是否请求停止转动（通常由 UI/逻辑写入）。 </summary>
        public bool isRequestToStop { set; }

        /// <summary> 停止时是否请求按真实额度刷新信用显示。 </summary>
        public bool isRequestToRealCreditWhenStop { set; }

        /// <summary> 是否正在 Spin。 </summary>
        public bool isSpin { get; }

        /// <summary> 是否处于自动 Spin 模式。 </summary>
        public bool isAuto { get; }

        /// <summary> 是否处于免费游戏（Free Spin）阶段。 </summary>
        public bool isFreeSpin { get; }

        /// <summary> 当前游戏状态机状态字符串（如 Idle 等）。 </summary>
        public string gameState { get; }

        /// <summary> Spin 按钮当前状态字符串（与 UI/逻辑约定一致）。 </summary>
        public string btnSpinState { get; }

        /// <summary> 本局总下注额。 </summary>
        public long totalBet { get; set; }

        /// <summary> 当前下注档位索引。 </summary>
        public int betIndex { get; set; }

        /// <summary> 下注倍数。 </summary>
        public int betmultiple { get; set; }

        /// <summary> 总可玩次数（如连转总次数）。 </summary>
        public int totalPlaySpins { get; }

        /// <summary> 剩余可玩次数。 </summary>
        public int remainPlaySpins { get; }

        /// <summary> 锚点/附加面板根节点（FGUI 组件）。 </summary>
        public GComponent goAnthorPanel { get; set; }

        /// <summary> 赔付表各页对应的 FGUI 组件数组。 </summary>
        public GComponent[] goPayTableLst { get; set; }

        /// <summary> Grand 彩金在 UI 上的绑定信息。 </summary>
        public JackpotInfo uiGrandJP { get; }

        /// <summary> Major 彩金在 UI 上的绑定信息。 </summary>
        public JackpotInfo uiMajorJP { get; }

        /// <summary> Minor 彩金在 UI 上的绑定信息。 </summary>
        public JackpotInfo uiMinorJP { get; }

        /// <summary> Mini 彩金在 UI 上的绑定信息。 </summary>
        public JackpotInfo uiMiniJP { get; }
    }

    /// <summary> 机台面板按钮交互回调：长按、短按、按下、抬起。 </summary>
    public interface IPanel
    {
        /// <summary> 某机台按键被长按时回调。 </summary>
        /// <param name="machineButtonKey"> 机台按键枚举。 </param>
        public void OnLongClickHandler(MachineButtonKey machineButtonKey);

        /// <summary> 某机台按键被短按（点击）时回调。 </summary>
        /// <param name="machineButtonKey"> 机台按键枚举。 </param>
        public void OnShortClickHandler(MachineButtonKey machineButtonKey);

        /// <summary> 某机台按键按下时回调。 </summary>
        /// <param name="machineButtonKey"> 机台按键枚举。 </param>
        public void OnDownClickHandler(MachineButtonKey machineButtonKey);

        /// <summary> 某机台按键抬起时回调。 </summary>
        /// <param name="machineButtonKey"> 机台按键枚举。 </param>
        public void OnUpClickHandler(MachineButtonKey machineButtonKey);
    }

    /// <summary> 机台配置：行列、符号资源、赔付线与赔付表等（部分列表由启动/配表流程填充）。 </summary>
    public interface ICustomModel
    {
        /// <summary> 单个符号在滚轮上的显示宽度（像素/逻辑单位）。 </summary>
        public float symbolWidth { get; }

        /// <summary> 单个符号在滚轮上的显示高度（像素/逻辑单位）。 </summary>
        public float symbolHeight { get; }

        /// <summary> 滚轮列数。 </summary>
        public int column { get; }

        /// <summary> 每列可见行数。 </summary>
        public int row { get; }

        /// <summary> 滚轮纵向最大偏移量（常用于滚动范围计算）。 </summary>
        public float reelMaxOffsetY { get; }

        /// <summary> 说明页/赔付说明对应的 FGUI 包资源路径数组。 </summary>
        public string[] payTable { get; }

        /// <summary> 逻辑图标索引到算法/真实符号编号的映射列表。 </summary>
        public List<int> symbolNumber { get; }

        /// <summary> 游戏中使用的符号种类总数。 </summary>
        public int symbolCount { get; }

        /// <summary> 符号索引字符串 -> 中奖时播放的特效预制体路径。 </summary>
        public Dictionary<string, string> symbolHitEffect { get; }

        /// <summary> 需特殊中奖表现（与普通线奖不同）的符号索引列表。 </summary>
        public List<int> specialHitSymbols { get; }

        /// <summary> 符号索引字符串 -> 停轮/出现时播放的特效预制体路径。 </summary>
        public Dictionary<string, string> symbolAppearEffect { get; }

        /// <summary> 符号索引字符串 -> 期待线/预演等用的特效预制体路径。 </summary>
        public Dictionary<string, string> symbolExpectationEffect { get; }

        /// <summary> 中奖线框/边框特效预制体路径。 </summary>
        public string borderEffect { get; }

        /// <summary> 符号索引字符串 -> 默认静态图（FGUI 等资源 URL）。 </summary>
        public Dictionary<string, string> symbolIcon { get; }

        /// <summary> 赔付表各符号赔率（由配置或启动流程填充）。 </summary>
        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; }

        /// <summary> 所有赔付线定义（列索引序列）。 </summary>
        public List<List<int>> payLines { get; set; }

        /// <summary> 大赢档位与倍数配置（由配置或启动流程填充）。 </summary>
        public List<WinMultiple> winLevelMultiple { get; set; }
    }
}

