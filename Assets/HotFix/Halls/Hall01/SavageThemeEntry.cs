using UnityEngine;

namespace Hall01
{
    /// <summary>
    /// Savage 主题入口：预载/打开/返回/关闭 Hall01。
    /// </summary>
    public sealed class SavageThemeEntry : IThemeEntry
    {
        static readonly SavageThemeEntry _instance = new SavageThemeEntry();

        /// <summary>单例，便于 Main 显式 Register（HybridCLR 热更程序集可能不触发 RuntimeInitialize）。</summary>
        public static SavageThemeEntry Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoRegister()
        {
            Register();
        }

        /// <summary>注册到 ThemeRuntime；可重复调用，后注册覆盖先前实现。</summary>
        public static void Register()
        {
            ThemeRuntime.Register(ThemeKind.Savage, _instance);
        }

        public void PreloadHall()
        {
            PageManager.Instance.PreloadPage(ThemeProfile.Savage.HallPageName, null);
        }

        public void OpenHall()
        {
            Hall01GameMain.OpenHall01AfterCardGameLoadingPreloads();
        }

        public void ReturnToHall()
        {
            Hall01GameMain.OpenHall01AfterCardGameLoadingPreloads();
        }

        public void CloseHall()
        {
            PageManager.Instance.ClosePage(ThemeProfile.Savage.CloseHallPageName);
        }
    }
}
