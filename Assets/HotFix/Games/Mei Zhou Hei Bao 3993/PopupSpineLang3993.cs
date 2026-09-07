using FairyGUI;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>
    /// 3993 弹窗 Spine 中英文：同一预制体 Anchor 下挂 en / cn 两套，按语言 SetActive。
    /// 必须在 Instantiate 之后、new AnimPlayer / Attach 之前调用。
    /// </summary>
    public static class PopupSpineLang3993
    {
        public const string EnName = "en";
        public const string CnName = "cn";

        /// <summary>英文用原节点；cn / tw / hk 等走中文节点。</summary>
        public static bool IsEnglish => I18nMgr.language == I18nLang.en;

        public static I18nLang CurrentLang => I18nMgr.language;

        /// <summary>按当前语言显示 en 或 cn，并把激活节点放到 Anchor 第一位，保证 AnimPlayer 取到当前语言。</summary>
        public static void Apply(GameObject clone)
        {
            if (clone == null || clone.transform.childCount == 0)
                return;

            Transform anchor = clone.transform.GetChild(0);
            Transform en = anchor.Find(EnName);
            Transform cn = anchor.Find(CnName);
            if (en == null || cn == null)
            {
                Debug.LogError("PopupSpineLang3993: prefab missing en/cn spine under Anchor.");
                return;
            }

            bool english = IsEnglish;
            en.gameObject.SetActive(english);
            cn.gameObject.SetActive(!english);
            (english ? en : cn).SetAsFirstSibling();
        }
    }

    /// <summary>弹窗 GoWrapper Spine：只创建一次，关页隐藏，切语言先摘 wrapTarget。</summary>
    public static class PopupSpineWrap3993
    {
        /// <summary>免费 / 大奖 / 彩金弹窗共用金币特效。</summary>
        public const string EffPopPrefabPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Eff_pop.prefab";

        /// <summary>只创建一次；挂点变了只换 wrapper，不 Instantiate。</summary>
        public static void EnsureSpine(ref GComponent bound, GComponent local, GameObject prefab,
            ref GameObject clone, ref AnimPlayer anim)
        {
            if (local == null || prefab == null) return;
            if (clone == null)
                clone = GameObject.Instantiate(prefab);
            if (bound != local)
            {
                GameCommon.FguiUtils.AddWrapper(local, clone);
                bound = local;
            }
            if (anim == null)
                anim = new AnimPlayer(clone);
        }

        /// <summary>开页挂到 childName 并播动画；关页只隐藏。</summary>
        public static void BindAndPlay(GComponent contentPane, string childName, GameObject prefab,
            ref GComponent bound, ref GameObject clone, ref AnimPlayer anim, string animName)
        {
            GComponent local = contentPane?.GetChild(childName) as GComponent;
            EnsureSpine(ref bound, local, prefab, ref clone, ref anim);
            SetVisible(bound, true);
            if (!string.IsNullOrEmpty(animName))
                anim?.Play(animName);
        }

        /// <summary>Dispose contentPane 前摘 wrapTarget，避免 GoWrapper 把实例一起毁掉。</summary>
        public static void PrepareLanguageChange(ref AnimPlayer anim, ref GComponent bound, GameObject clone)
        {
            anim?.DetachAll();
            DetachFromAnchor(bound);
            HideUnwrappedClone(clone);
            bound = null;
        }

        /// <summary>关 GoWrapper 根节点并关 MeshRenderer；只 SetActive clone 关不掉缓存网格。</summary>
        public static void SetVisible(GComponent anchor, bool visible)
        {
            if (anchor == null) return;
            anchor.visible = visible;
            GGraph holder = anchor.GetChild("holder") as GGraph;
            if (holder != null)
                holder.visible = visible;

            GameObject target = GameCommon.FguiUtils.GetWrapperTarget(anchor);
            if (target == null) return;
            target.SetActive(visible);
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }
        }

        /// <summary>从 GoWrapper 摘下实例但不销毁。</summary>
        public static void DetachFromAnchor(GComponent anchor)
        {
            if (anchor == null) return;
            GoWrapper wrapper = GameCommon.FguiUtils.GetWrapper(anchor);
            wrapper?.SetWrapTarget(null, false);
        }

        /// <summary>摘下后的 clone 会回到场景根，先关掉网格避免闪一帧。</summary>
        public static void HideUnwrappedClone(GameObject clone)
        {
            if (clone == null) return;
            clone.SetActive(false);
            Renderer[] renderers = clone.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = false;
            }
        }
    }
}
