using UnityEngine;

namespace TreasuryHall
{
    /// <summary>
    /// 财富主题入口：预载/打开/返回/关闭 TreasuryHallMain。
    /// </summary>
    public sealed class TreasuryThemeEntry : IThemeEntry
    {
        static readonly TreasuryThemeEntry _instance = new TreasuryThemeEntry();

        /// <summary>单例，便于 Main 显式 Register（HybridCLR 热更程序集可能不触发 RuntimeInitialize）。</summary>
        public static TreasuryThemeEntry Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoRegister()
        {
            Register();
        }

        /// <summary>注册到 ThemeRuntime；可重复调用，后注册覆盖先前实现。</summary>
        public static void Register()
        {
            ThemeRuntime.Register(ThemeKind.Treasury, _instance);
        }

        public void PreloadHall()
        {
            PageManager.Instance.PreloadPage(ThemeProfile.Treasury.HallPageName, null);
        }

        public void OpenHall()
        {
            TreasuryHallMain.OpenTreasuryHallMainAfterCardGameLoadingPreloads();
        }

        public void ReturnToHall()
        {
            TreasuryHallMain.OpenTreasuryHallMainAfterCardGameLoadingPreloads();
        }

        public void CloseHall()
        {
            PageManager.Instance.ClosePage(ThemeProfile.Treasury.CloseHallPageName);
        }
    }
}
