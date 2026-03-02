using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ConsoleSlot01
{
    public class PageConsoleGameHistory : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleGameHistory";
        public override PageType pageType => PageType.Overlay;
        TabGameHistoryController taGameHistoryController = new TabGameHistoryController();
        GButton btnClose;
        GButton btnPrev, btnNext;//上一页和下一页按钮
        GComboBox gcbDropdownDates, gcbDropdownGameIds;//日期和游戏id下拉框

        // 存储当前选中的游戏ID和日期时间（精确到秒）
        long currentGameId = 1700; // 默认游戏ID
        string currentDateTime = ""; // 格式：yyyy-MM-dd HH:mm:ss

        // 游戏ID对应的包名和路径映射
        private Dictionary<long, string[]> gamePackageMap = new Dictionary<long, string[]>
        {
            { 1700, new string[] { "SlotZhuZaiJinBi1700", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/FGUIs" } },
            { 200, new string[] { "PusherEmperorsRein200", "Assets/GameRes/Games/Emperors Rein 200/FGUIs" } },
            { 3998, new string[] { "XingYunZhiLun3998", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/FGUIs" } },
            { 3996, new string[] { "CaiFuHuoChe3996", "Assets/GameRes/Games/Cai Fu Huo Che 3996/FGUIs" } },
            { 3997, new string[] { "CaiFuZhiJia3997", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs" } },
            { 3999, new string[] { "CaiFuZhiMen3999", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/FGUIs" } },
        };

        protected override void OnInit()
        {
            base.OnInit();
            int count = 1;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            callback();
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            base.OnClose(data);
        }

        public override void InitParam()
        {
            if (!isInit) return;
            if (!isOpen) return;

            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(null);
            });

            // 初始化Tab控制器，传入两个回调
            taGameHistoryController.InitParam(
                this.contentPane.GetChild("Slot").asCom,
                ConsoleTableName.TABLE_SLOT_GAME_RECORD,
                onDatesChange,
                onGameIdsChange);

            // 初始化日期时间下拉框
            gcbDropdownDates = this.contentPane.GetChild("date").asCom.GetChild("value").asComboBox;
            gcbDropdownDates.onChanged.Clear();
            gcbDropdownDates.onChanged.Add(OnDateTimeComboChanged);

            // 初始化游戏ID下拉框
            gcbDropdownGameIds = this.contentPane.GetChild("gameId").asCom.GetChild("value").asComboBox;
            gcbDropdownGameIds.onChanged.Clear();
            gcbDropdownGameIds.onChanged.Add(OnGameIdComboChanged);

            btnPrev = this.contentPane.GetChild("btnPrev").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickPrev);

            btnNext = this.contentPane.GetChild("btnNext").asButton;
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickNext);

        }

        // 日期时间列表变化回调
        void onDatesChange(List<string> dateTimes)
        {
            // 格式化显示：将"yyyy-MM-dd HH:mm:ss"转换为更友好的显示格式
            List<string> displayItems = new List<string>();
            List<string> displayValues = new List<string>();

            foreach (string dateTime in dateTimes)
            {
                if (!string.IsNullOrEmpty(dateTime))
                {
                    try
                    {
                        // 解析日期时间
                        DateTime dt = DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm:ss", null);
                        // 格式化为显示格式："MM-dd HH:mm:ss" 或自定义格式
                        string display = dt.ToString("MM-dd HH:mm:ss"); // 示例：12-25 14:30:25
                        displayItems.Add(display);
                        displayValues.Add(dateTime); // 实际值保持不变
                    }
                    catch
                    {
                        // 如果解析失败，直接使用原字符串
                        displayItems.Add(dateTime);
                        displayValues.Add(dateTime);
                    }
                }
            }

            gcbDropdownDates.items = displayItems.ToArray();
            gcbDropdownDates.values = displayValues.ToArray();

            if (dateTimes.Count > 0)
            {
                gcbDropdownDates.selectedIndex = 0;
                currentDateTime = dateTimes[0];
                // 自动查询第一条数据
                taGameHistoryController.OnDateTimeChanged(currentGameId, currentDateTime);
            }
            else
            {
                // 没有数据时清空显示
                gcbDropdownDates.items = new string[] { "暂无数据" };
                gcbDropdownDates.values = new string[] { "" };
                gcbDropdownDates.selectedIndex = 0;
                taGameHistoryController.ClearDisplay();
            }
        }

        // 游戏ID列表变化回调
        void onGameIdsChange(List<long> gameIds)
        {
            List<string> gameIdStrings = new List<string>();
            List<string> gameIdValues = new List<string>();

            foreach (long id in gameIds)
            {
                gameIdStrings.Add($"{id}");
                gameIdValues.Add(id.ToString());
            }

            gcbDropdownGameIds.items = gameIdStrings.ToArray();
            gcbDropdownGameIds.values = gameIdValues.ToArray();

            if (gameIds.Count > 0)
            {
                gcbDropdownGameIds.selectedIndex = 0;
                currentGameId = gameIds[0];

                // 加载第一个游戏ID对应的游戏包，包加载完成后再查询
                EnsureGamePackageLoaded(currentGameId, () =>
                {
                    taGameHistoryController.OnGameIdChanged(currentGameId);
                });
            }
        }

        void OnGameIdComboChanged(EventContext context)
        {
            GComboBox sender = context.sender as GComboBox;
            DebugUtils.Log($"选择了游戏ID索引：{gcbDropdownGameIds.selectedIndex}，值：{sender.value}");

            if (long.TryParse(sender.value, out long gameId))
            {
                currentGameId = gameId;

                // 加载对应游戏的包（如果未加载），包加载完成后再查询数据
                EnsureGamePackageLoaded(gameId, () =>
                {
                    taGameHistoryController.OnGameIdChanged(currentGameId);
                });
            }

        }

        void OnDateTimeComboChanged(EventContext context)
        {
            GComboBox sender = context.sender as GComboBox;
            DebugUtils.Log($"选择了日期时间索引：{gcbDropdownDates.selectedIndex}，值：{sender.value}");

            currentDateTime = sender.value;
            // 根据选中的游戏ID和日期时间查询数据
            taGameHistoryController.OnDateTimeChanged(currentGameId, currentDateTime);
        }

        public void OnClickPrev()
        {
            taGameHistoryController.PrevPage();
        }

        public void OnClickNext()
        {
            taGameHistoryController.NextPage();
        }

        // 加载指定游戏的包（如果未加载）
        private void EnsureGamePackageLoaded(long gameId, Action onLoaded = null)
        {
            if (!gamePackageMap.ContainsKey(gameId))
            {
                DebugUtils.LogWarning($"未找到游戏ID {gameId} 对应的包名映射");
                onLoaded?.Invoke();
                return;
            }

            string packageName = gamePackageMap[gameId][0];
            string packagePath = gamePackageMap[gameId][1];

            // 检查包是否已加载
            if (UIPackage.GetByName(packageName) != null)
            {
                // 包已加载，直接执行回调
                DebugUtils.Log($"游戏包已加载: {packageName} (gameID: {gameId})");
                onLoaded?.Invoke();
                return;
            }

            // 通过 ResourceManager02 加载 AssetBundle
            ResourceManager02.Instance.LoadAssetBundleAsync(packagePath, (bundle) =>
            {
                if (bundle != null)
                {
                    UIPackage.AddPackage(bundle);
                    DebugUtils.Log($"已加载游戏包: {packageName} (gameID: {gameId})");
                }
                else
                {
                    DebugUtils.LogWarning($"加载游戏包失败: {packageName} (gameID: {gameId})");
                }
                
                // 包加载完成（或失败）后执行回调
                onLoaded?.Invoke();
            });
        }
    }
}

