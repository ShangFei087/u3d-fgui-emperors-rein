using UnityEngine;

namespace HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom
{
    /// <summary>
    /// 3994 弹窗 Spine 中英文：同一预制体 Anchor 下挂 en / cn 两套，按语言 SetActive。
    /// 必须在 Instantiate 之后、new AnimPlayer / Attach 之前调用。
    /// </summary>
    public static class PopupLang3994
    {
        private const string EnName = "en";
        private const string CnName = "cn";

        /// <summary>英文用原节点；cn / tw / hk 等走中文节点。</summary>
        private static bool IsEnglish => I18nMgr.language == I18nLang.en;

        public static I18nLang CurrentLang => I18nMgr.language;

        /// <summary>按当前语言显示 en 或 cn，并把激活节点放到 Anchor 第一位，保证 AnimPlayer 取到当前语言。</summary>
        public static void Apply(GameObject clone)
        {
            if (clone == null || clone.transform.childCount == 0)
                return;

            Transform anchor = clone.transform;
            Transform en = anchor.Find(EnName);
            Transform cn = anchor.Find(CnName);
            if (en == null || cn == null)
            {
                Debug.LogError("PopupLang3994: prefab missing en/cn spine under Anchor.");
                return;
            }

            bool english = IsEnglish;
            en.gameObject.SetActive(english);
            cn.gameObject.SetActive(!english);
            (english ? en : cn).SetAsFirstSibling();
        }
    }
}