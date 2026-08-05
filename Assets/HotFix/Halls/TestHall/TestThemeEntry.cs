using UnityEngine;

namespace TestHall
{
    /// <summary>
    /// 测试主题入口：预载/打开/返回/关闭 TestHallMain（可进全部已配置子游戏）。
    /// </summary>
    public sealed class TestThemeEntry : IThemeEntry
    {
        static readonly TestThemeEntry _instance = new TestThemeEntry();

        public static TestThemeEntry Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoRegister()
        {
            Register();
        }

        public static void Register()
        {
            ThemeRuntime.Register(ThemeKind.Test, _instance);
        }

        public void PreloadHall()
        {
            PageManager.Instance.PreloadPage(ThemeProfile.Test.HallPageName, null);
        }

        public void OpenHall()
        {
            TestHallMain.OpenTestHallMain();
        }

        public void ReturnToHall()
        {
            TestHallMain.OpenTestHallMain();
        }

        public void CloseHall()
        {
            PageManager.Instance.ClosePage(ThemeProfile.Test.CloseHallPageName);
        }
    }
}
