namespace MeiZhouHeiBao_3993
{
    public class PopupBigWin : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupBigWin";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupBigWin/";

        private int _totalCount = -1;

        private readonly string[] winString = { "BIG", "HUGE", "MASSIVE" };
        private readonly string[] winOpenString = { "bigwin_start", "bigwin_superwin", "superwin_megawin" };
        private readonly string[] winCloseString = { "bigwin_end", "superwin_end", "megawin_end" };

        private readonly string[] npcStartString = { "ng_pop_border_bigwin", "ng_pop_border_supwin", "ng_pop_border_megawin" };
    }
}