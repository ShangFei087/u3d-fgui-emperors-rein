using FairyGUI;
using GameMaker;
using UnityEngine;

namespace ConsoleSlot01
{
    /// <summary>
    /// 断电重连遮罩。FGUI：Console.PopupReconnectMask（未导出时回退 PopupConsoleMask）。
    /// 不与退票 MaskPopupHandler 共用 PageName。
    /// </summary>
    public class PopupReconnectMask : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupReconnectMask";
        const string FallbackResName = "PopupConsoleMask";
        const string TitleChildName = "title";

        public override PageType pageType => PageType.Overlay;

        protected override void OnInit()
        {
            UIPackage pkg = UIPackage.GetByName(pkgName);
            bool published = pkg != null && pkg.GetItemByName(resName) != null;
            if (published)
            {
                base.OnInit();
                return;
            }

            this.contentPane = UIPackage.CreateObject(pkgName, FallbackResName).asCom;
            this.Center();
            this.modal = true;
            this.isReady = false;
            EventCenter.Instance.AddEventListener<I18nLang>(I18nMgr.I18N, OnFallbackLanguageChange);
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<I18nLang>(I18nMgr.I18N, OnFallbackLanguageChange);
            base.OnClose(data);
        }

        public override void InitParam()
        {
            EnsureTitle();
        }

        void OnFallbackLanguageChange(I18nLang lang)
        {
            OnLanguageChange(lang);
        }

        void EnsureTitle()
        {
            if (contentPane == null)
                return;

            GTextField title = contentPane.GetChild(TitleChildName) as GTextField;
            if (title == null)
            {
                title = new GTextField();
                title.name = TitleChildName;
                title.touchable = false;
                title.align = AlignType.Center;
                title.verticalAlign = VertAlignType.Middle;
                title.SetSize(800, 80);
                title.SetPivot(0.5f, 0.5f, true);
                title.SetXY(contentPane.width * 0.5f, contentPane.height * 0.5f + 120f);
                title.AddRelation(contentPane, RelationType.Center_Center);
                title.AddRelation(contentPane, RelationType.Middle_Middle);
                contentPane.AddChild(title);
            }

            TextFormat textFormat = title.textFormat;
            textFormat.size = 36;
            textFormat.color = Color.white;
            title.textFormat = textFormat;
            title.text = I18nMgr.T("断电重连中");
        }
    }
}
