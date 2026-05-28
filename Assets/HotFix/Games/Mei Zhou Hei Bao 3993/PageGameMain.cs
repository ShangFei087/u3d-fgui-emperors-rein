using FairyGUI;
using Newtonsoft.Json;
using SlotMaker;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId; //游戏 ID

        [JsonProperty("game_name")] public string GameName; //名称

        [JsonProperty("display_name")] public string DisplayName; //显示名称

        [JsonProperty("line_num")] public int LineNum; //线数

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; } //赢钱倍数

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; } //符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付钱
    }
    
    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PageGameMain";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PageGameMain/";
        
        // --------------------------------------------- 游戏中通用变量 -----------------------------------------------
        // 
        private int _totalCount = -1;
        private GComponent _gOwnerPanel;

        // 游戏控制器
        private GameObject _goGameCtrl;
        private MonoHelper _monoHelper;
        private Controller _pageController;
        private FguiPoolHelper _fGuiPoolHelper;
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        private GameSoundController3993 _gameSoundController;
        private SlotMachineController3993 _slotMachineController;
        
        // 彩金
        private readonly MiniReelGroup uiJpMajorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup uiJpMinorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup uiJpMiniCtrl = new MiniReelGroup();
        
        // 玩家押注
        private long TotalBet => MainModel.Instance.contentMD.totalBet;
        private bool IsAddCreditAnim =>
            !(_slotMachineController.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        
        // 说明书
        private List<GComponent> _lstPayTable;
        private readonly PayTableController3993 _payTableController = new PayTableController3993();
        private bool _isStopButtonLocked, _tipCoinIn, _isStoppedSlotMachine;
    }
}