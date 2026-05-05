
namespace SlotMaker
{

    public static class SlotMachineEvent
    {

        #region 拉霸机-运行
        public const string ON_SLOT_EVENT = "ON_SLOT_EVENT";
        //public const string ON_SLOT_MACHINE_EVENT = "ON_SLOT_MACHINE_EVENT";
        /// <summary> 开始转动滚轮 </summary>
        public const string SpinSlotMachine = "SpinSlotMachine";
        /// <summary> 已经停止所有滚轮的转动 </summary>
        public const string StoppedSlotMachine = "StoppedSlotMachine";
        /// <summary> 命令停止所有滚轮的转动  </summary>
        public const string StopSlotMachine = "StopSlotMachine";
        #endregion


        #region 拉霸机-运行细节
        public const string ON_SLOT_DETAIL_EVENT = "ON_SLOT_DETAIL_EVENT";

        /// <summary> 卷轴转到一步 - value : int </summary>
        public const string ReelOneStep = "ReelOneStep";  //ReelOneStep  value = 0
        /// <summary> 已经停止某个滚轮的转动 - value : int </summary>
        public const string PrepareStoppedReel = "PrepareStoppedReel";  //PrepareStoppedReel  value = 0
        /// <summary> 单列停稳播滚轮停止音（与 PrepareStoppedReel 分离，避免与缓动 UI 耦合）- value : int 0-based 列 </summary>
        public const string ReelColumnStopSound = "ReelColumnStopSound";
        /// <summary> 单列停稳后播 Scatter/Bonus 列停音效，value：<see cref="ScatterBonusColumnStopPayload"/>（仅标量，勿携带 Reel，避免 EventCenter 调试序列化 Unity 对象）。 </summary>
        public const string ScatterBonusColumnStopSound = "ScatterBonusColumnStopSound";
        /// <summary> 开启某列的特效 - value : int </summary>
        public const string BeginExpectation = "BeginExpectation"; // name = BeginExpectation  value = 3
        /// <summary> 关闭某列的特效 - </summary>
        public const string EndExpectation = "EndExpectation";
        /// <summary> 停止特殊symbol的 - value : Symbol </summary>
        public const string PrepareStoppedSpecialSymbol = "PrepareStoppedSpecialSymbol";
        //eventName = OnSlotDetailEvent ， name = PrepareStoppedSpecialSymbol  value = Symbol (SlotMaker.Symbol)
        #endregion

        //public const string ON_BONUS_EVENT = "ON_BONUS_EVENT";

        #region 赢分事件-通知panel面板
        public const string ON_WIN_EVENT = "ON_WIN_EVENT";
        /// <summary>停止显示赢分 </summary>
        public const string SkipWinLine = "SkipWinLine";
        /// <summary> 单线赢分 - value : SymbolWin </summary>
        public const string SingleWinLine = "SingleWinLine";
        /// <summary> 所有线赢分 - value : List<SymbolWin> </summary>
        public const string TotalWinLine = "TotalWinLine";
        //    public const string TotalWinLine = "TotalWinLine";

        /// <summary>单个Bonus赢分  - value : BonnusWin</summary>
        public const string SingleWinBonus = "SingleWinBonus";



        /// <summary> 所有赢分 - value : Credit , isAnim </summary>
        public const string PrepareTotalWinCredit = "PrepareTotalWinCredit";

        /// <summary> 所有赢分 - value : Credit , isAnim </summary>
        public const string PrepareTotalWinCredit02 = "PrepareTotalWinCredit02";

        /// <summary> 显示所有赢分 - value : List<SymbolWin> </summary>
        public const string TotalWinCredit = "TotalWinCredit";



        // PrepareTotalWinCredit  // Credit , isAddTo 
        // TotalWinCredit // Credit, isAddTo 


        //protected const string ON_TOTAL_WIN_EVENT = "TotalWin";
        //protected const string ON_TOTAL_WIN_LINE_EVENT = "TotalWinLine";
        #endregion



        #region symbol图标动画事件
        public const string ON_SYMBOL_EVENT = "ON_SYMBOL_EVENT";
        public const string StopEffect = "StopEffect";
        public const string Skip = "Skip";
        public const string Win = "Win";
        #endregion

        #region 音效总线（ON_AUDIO_EVENT，EventData.name 为下列子常量）

        /// <summary> 仅音效 / 音乐子事件（与 <see cref="ON_SLOT_EVENT"/>、<see cref="ON_WIN_EVENT"/> 等协议总线分离）。 </summary>
        public const string ON_AUDIO_EVENT = "ON_AUDIO_EVENT";

        /// <summary> 进入或退出免费游戏过场 </summary>
        public const string FreeGameFadeTransition = "FreeGameFadeTransition";
        /// <summary> 进入或退出彩金 / Bonus 小游戏过场 </summary>
        public const string BonusGameFadeTransition = "BonusGameFadeTransition";
        /// <summary> 免费触发或结算弹窗出现 </summary>
        public const string FreeSpinPopupAppear = "FreeSpinPopupAppear";
        /// <summary> 免费触发或结算弹窗关闭过程 </summary>
        public const string FreeSpinPopupDisappear = "FreeSpinPopupDisappear";
        /// <summary> 免费触发弹窗 START 按钮就绪 </summary>
        public const string FreeSpinStartButtonShown = "FreeSpinStartButtonShown";

        /// <summary> JACKPOT 提示框出现 </summary>
        public const string JackpotPopupAppear = "JackpotPopupAppear";
        /// <summary> JACKPOT 提示框关闭 </summary>
        public const string JackpotPopupDisappear = "JackpotPopupDisappear";
        /// <summary> Bonus 游戏提示框、结算框出现 </summary>
        public const string BonusPopupAppear = "BonusPopupAppear";
        /// <summary> Bonus 奖出现框 START </summary>
        public const string BonusStartBtn = "BonusStartBtn";
        /// <summary> Bonus 游戏提示框、结算框消失 </summary>
        public const string BonusPopupDisappear = "BonusPopupDisappear";
        /// <summary> Bonus 奖结算框 COLLECT </summary>
        public const string BonusCollectBtn = "BonusCollectBtn";

        /// <summary> Scatter 期望加速框（蓝色） </summary>
        public const string FreeRollingBox = "FreeRollingBox";
        /// <summary> Bonus 期望加速框（金色） </summary>
        public const string BonusRollingBox = "BonusRollingBox";

        #endregion

        #region 拉霸机数据状态

        /// <summary> 内容层事件（FSM、小游戏衔接如 BeginBonus 等）。 </summary>
        public const string ON_CONTENT_EVENT = "ON_CONTENT_EVENT";

        /// <summary> 开始新的一局游戏 </summary>
        public const string BeginTurn = "BeginTurn";
        public const string EndTurn = "EndTurn";

        /// <summary> 开始一个spin游戏 </summary>
        public const string BeginSpin = "BeginSpin";
        public const string EndSpin = "EndSpin";

        /// <summary> 开始一个Bonus(小游戏、彩金、免费游戏) </summary>
        public const string BeginBonus = "BeginBonus";
        public const string EndBonus = "EndBonus";

        #endregion

        #region Game FSM
        //public const string ON_GAME_STATE_EVEN = "ON_CONTENT_EVEN";
        //public const string GameState = "GameState";
        /*
        public const string Idle = "Idle";
        public const string Spin = "Spin";
        public const string FreeSpin = "FreeSpin";
        public const string MiniGame = "MiniGame";
        */
        #endregion

    }

    /// <summary> <see cref="SlotMachineEvent.ScatterBonusColumnStopSound"/> 携带列号与可视区检测结果（勿含 ReelBase 等 Unity 引用，否则 EventCenter 调试序列化会遍历材质）。 </summary>
    public struct ScatterBonusColumnStopPayload
    {
        public int column0Based;
        public bool hasScatter;
        public bool hasBonus;
    }

    public class MetaUIEvent
    {

        #region 界面UI事件
        public const string ON_META_UI_EVENT = "ON_META_UI_EVENT";
        public const string UpdateJackpotInfo = "UpdateJackpotInfo";
        public const string UpdateButtonState = "UpdateButtonState";
        #endregion


        #region 玩家分数事件
        public const string ON_CREDIT_EVENT = "ON_CREDIT_EVENT";
        /// <summary> 刷新turn的赢分 - 不带动画 - value : bool </summary>
        public const string UpdateTurnCredit = "UpdateTurnCredit"; // name = UpdateTurnCredit  value = True
        /// <summary> 刷新玩家分数 - 不带动画 - value : bool </summary>
        public const string UpdateNaviCredit = "UpdateNaviCredit";

        /*
        public const string BetUp = " BetUp";
        public const string BetDown = "BetDown";
        public const string BetMax = "BetMax";
        /// <summary> 刷新压住金额 </summary>
        public const string UpdateBetCredit = "UpdateBetCredit";  //eventName = OnCreditEvent ， name = UpdateBetCredit  value = 60
        /// <summary> 刷新总压住金额 </summary>
        public const string UpdatedTotalBetCredit = "UpdatedTotalBetCredit"; // eventName = OnCreditEvent ， name = UpdatedTotalBetCredit  value = 60
        public const string UpdateBetIndex = "UpdateBetIndex"; //eventName = OnCreditEvent ， name = UpdateBetIndex value = 7
        */
        #endregion

        public const string ON_SPIN_BUTTON_EVENT = "ON_SPIN_BUTTON_EVENT";
        public const string OnSpinButtonEvent = "OnSpinButtonEvent";

    }

    public class UpdateNaviCredit
    {
        /// <summary> 是否要加钱动画 </summary>
        public bool isAnim;
        /// <summary> 加钱前 </summary>
        public long fromCredit;
        /// <summary> 加钱后 </summary>
        public long toCredit;
    }


    public class PrepareTotalWinCredit
    {
        /// <summary> 是否要加钱动画 </summary>
        public bool isAddToCredit;
        /// <summary> 加钱后 </summary>
        public long totalWinCredit;
    }

    public class PrepareTotalWinCredit02
    {
        /// <summary> 是否要加钱动画 </summary>
        public bool isEndToCredit;
        /// <summary> 加钱后 </summary>
        public long credit;
    }
}