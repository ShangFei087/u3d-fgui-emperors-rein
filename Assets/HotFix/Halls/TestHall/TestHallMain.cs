using FairyGUI;
using GameMaker;
using SBoxApi;
using System.Collections.Generic;
using UnityEngine;

namespace TestHall
{
    /// <summary>
    /// 测试大厅：按 ThemeProfile.Test 数量动态创建 ButtonCard 入口。
    /// FGUI：TestHallMain.listGames（defaultItem=ButtonCard）；无列表时回退 CreateObject。
    /// </summary>
    public class TestHallMain : MachinePageBase
    {
        public const string pkgName = "TestHall";
        public const string resName = "TestHallMain";
        public const string ButtonCardResName = "ButtonCard";
        public const string ListGamesName = "listGames";

        static readonly Dictionary<int, string> GameDisplayNames = new Dictionary<int, string>
        {
            [3993] = "美洲黑豹",
            [3994] = "非洲黑猩猩",
            [3995] = "火焰公牛",

            [3996] = "财富火车",
            [3997] = "财富之家",
            [3998] = "幸运之轮",
            [3999] = "财富之门",

            [1700] = "猪仔金币",
        };

        GList _listGames;
        GComponent _fallbackRoot;
        readonly List<GButton> _fallbackButtons = new List<GButton>();
        readonly List<int> _boundGameIds = new List<int>();
        readonly List<PageName> _boundLoadingPages = new List<PageName>();

        protected override void OnInit()
        {
            base.OnInit();
            isInit = true;
            InitParam();
        }

        /// <summary>
        /// 打开测试大厅（不预载全部 Loading；点进游戏时再开对应 Loading）。
        /// </summary>
        public static void OpenTestHallMain()
        {
            PageLaunch.Instance.Close(2f);
            PageManager.Instance.OpenPage(PageName.TestHallMain);
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            GameSoundHelper.Instance.StopMusic();
            ClearGameButtons();
            base.OnClose(data);
        }

        public override void InitParam()
        {
            if (!isInit) return;
            if (!isOpen) return;

            BuildGameButtons();
        }

        void BuildGameButtons()
        {
            ClearGameButtons();
            CollectEntries();

            if (_boundGameIds.Count == 0)
            {
                Debug.LogWarning("[TestHall] ThemeProfile.Test 无可用游戏入口");
                return;
            }

            _listGames = contentPane.GetChild(ListGamesName) as GList;
            if (_listGames != null)
                BuildWithList();
            else
                BuildWithCreateObjectFallback();
        }

        void CollectEntries()
        {
            ThemeProfile profile = ThemeProfile.Test;
            for (int i = 0; i < profile.GameIds.Count; i++)
            {
                int gameId = profile.GameIds[i];
                if (!profile.TryGetLoadingPage(gameId, out PageName loadingPage))
                {
                    Debug.LogWarning($"[TestHall] gameId={gameId} 无 Loading 映射，跳过");
                    continue;
                }

                _boundGameIds.Add(gameId);
                _boundLoadingPages.Add(loadingPage);
            }
        }

        void BuildWithList()
        {
            _listGames.RemoveChildrenToPool();
            for (int i = 0; i < _boundGameIds.Count; i++)
            {
                int gameId = _boundGameIds[i];
                PageName loadingPage = _boundLoadingPages[i];

                GButton btn = _listGames.AddItemFromPool().asButton;
                if (btn == null)
                {
                    Debug.LogError("[TestHall] listGames.AddItemFromPool 未得到 GButton，请确认 defaultItem=ButtonCard");
                    continue;
                }

                btn.title = FormatButtonTitle(gameId);
                btn.onClick.Clear();
                btn.onClick.Add(() => EnterGame(gameId, loadingPage));
            }
        }

        void BuildWithCreateObjectFallback()
        {
            Debug.LogWarning($"[TestHall] 未找到 {ListGamesName}，回退 CreateObject({ButtonCardResName})");

            EnsureFallbackRoot();

            const float btnW = 300f;
            const float btnH = 120f;
            const float gapX = 24f;
            const float gapY = 24f;
            const int cols = 3;
            const float startX = 40f;
            const float startY = 40f;

            for (int i = 0; i < _boundGameIds.Count; i++)
            {
                int gameId = _boundGameIds[i];
                PageName loadingPage = _boundLoadingPages[i];

                GButton btn = UIPackage.CreateObject(pkgName, ButtonCardResName) as GButton;
                if (btn == null)
                {
                    Debug.LogError($"[TestHall] CreateObject 失败: {pkgName}/{ButtonCardResName}");
                    continue;
                }

                int col = i % cols;
                int row = i / cols;
                btn.SetSize(btnW, btnH);
                btn.SetXY(startX + col * (btnW + gapX), startY + row * (btnH + gapY));
                btn.title = FormatButtonTitle(gameId);
                btn.onClick.Clear();
                btn.onClick.Add(() => EnterGame(gameId, loadingPage));

                _fallbackRoot.AddChild(btn);
                _fallbackButtons.Add(btn);
            }
        }

        void EnsureFallbackRoot()
        {
            if (_fallbackRoot != null)
                return;

            _fallbackRoot = new GComponent();
            _fallbackRoot.gameObjectName = "TestHallGameListFallback";
            _fallbackRoot.SetXY(0, 120);
            contentPane.AddChild(_fallbackRoot);
        }

        static string FormatButtonTitle(int gameId)
        {
            if (GameDisplayNames.TryGetValue(gameId, out string name))
                return $"{gameId}\n{name}";
            return gameId.ToString();
        }

        void EnterGame(int gameId, PageName loadingPage)
        {
            if (!ApplicationSettings.Instance.isMock)
                SBoxIdea.GameSwitch(gameId);

            PageManager.Instance.OpenPage(loadingPage);
            CloseSelf(null);
        }

        void ClearGameButtons()
        {
            if (_listGames != null)
            {
                _listGames.RemoveChildrenToPool();
                _listGames = null;
            }

            for (int i = 0; i < _fallbackButtons.Count; i++)
            {
                GButton btn = _fallbackButtons[i];
                if (btn == null)
                    continue;
                btn.onClick.Clear();
                btn.Dispose();
            }
            _fallbackButtons.Clear();

            _boundGameIds.Clear();
            _boundLoadingPages.Clear();
        }
    }
}
