using System.Collections.Generic;
using System.Threading;
using FairyGUI;
using Spine;
using Spine.Unity;
using UnityEngine;

public struct LiveSpineStats
{
    public int displayedInstanceCount;
    public int totalVertices;
    public int totalBones;
    public int meshUpdatesInWindow;
}

/// <summary>
/// Spine 实例统计（仅统计 FGUI/场景中正在显示的实例）。
/// </summary>
public static class SpineStatsCounter
{
    const float cacheRefreshInterval = 0.5f;

    static readonly List<SkeletonRenderer> s_renderers = new List<SkeletonRenderer>(64);
    static readonly List<SkeletonGraphic> s_graphics = new List<SkeletonGraphic>(32);
    static readonly HashSet<int> s_scratchIds = new HashSet<int>();
    static readonly HashSet<int> s_hookedRendererIds = new HashSet<int>();
    static readonly HashSet<int> s_hookedGraphicIds = new HashSet<int>();
    static readonly Dictionary<int, SkeletonRenderer> s_hookedRenderers =
        new Dictionary<int, SkeletonRenderer>(64);
    static readonly Dictionary<int, SkeletonGraphic> s_hookedGraphics =
        new Dictionary<int, SkeletonGraphic>(32);
    static readonly Dictionary<int, SkeletonRenderer.SkeletonRendererDelegate> s_rendererCallbacks =
        new Dictionary<int, SkeletonRenderer.SkeletonRendererDelegate>(64);
    static readonly Dictionary<int, SkeletonGraphic.SkeletonRendererDelegate> s_graphicCallbacks =
        new Dictionary<int, SkeletonGraphic.SkeletonRendererDelegate>(32);
    static readonly List<GameObject> s_wrapTargets = new List<GameObject>(64);
    static readonly HashSet<int> s_wrapTargetIds = new HashSet<int>();
    static readonly Dictionary<int, bool> s_wrapTargetDisplayed = new Dictionary<int, bool>(64);

    static FguiPool[] s_cachedPools;
    static int s_meshUpdateCount;
    static bool s_hooksEnabled;
    static float s_lastCacheRefreshTime = -1f;

    public static bool HooksEnabled => s_hooksEnabled;

    public static void EnableHooks()
    {
        s_hooksEnabled = true;
        RefreshCacheIfNeeded(true);
    }

    public static LiveSpineStats GetLiveSpineStats()
    {
        LiveSpineStats stats = default;
        if (!s_hooksEnabled)
            return stats;

        RefreshCacheIfNeeded(false);
        RefreshWrapTargetDisplayMap();

        for (int i = 0; i < s_renderers.Count; ++i)
            AccumulateRenderer(s_renderers[i], ref stats);

        for (int i = 0; i < s_graphics.Count; ++i)
            AccumulateGraphic(s_graphics[i], ref stats);

        stats.meshUpdatesInWindow = Interlocked.Exchange(ref s_meshUpdateCount, 0);
        return stats;
    }

    public static void ClearCache()
    {
        s_hooksEnabled = false;
        UnsubscribeAll();
        s_renderers.Clear();
        s_graphics.Clear();
        s_cachedPools = null;
        s_wrapTargets.Clear();
        s_wrapTargetIds.Clear();
        s_wrapTargetDisplayed.Clear();
        s_lastCacheRefreshTime = -1f;
        Interlocked.Exchange(ref s_meshUpdateCount, 0);
    }

    static void RefreshCacheIfNeeded(bool force)
    {
        if (!force && s_lastCacheRefreshTime >= 0f
            && Time.unscaledTime - s_lastCacheRefreshTime < cacheRefreshInterval)
            return;

        s_lastCacheRefreshTime = Time.unscaledTime;
        RebuildInstanceCache();
        SyncHooks();
    }

    static void RebuildInstanceCache()
    {
        s_renderers.Clear();
        s_graphics.Clear();
        s_scratchIds.Clear();

        s_cachedPools = UnityEngine.Object.FindObjectsOfType<FguiPool>(true);
        CollectWrapTargets();

        for (int i = 0; i < s_wrapTargets.Count; ++i)
            CollectSpineOnGameObject(s_wrapTargets[i]);

        AnimBaseUI[] animBaseUis = UnityEngine.Object.FindObjectsOfType<AnimBaseUI>(true);
        if (animBaseUis != null)
        {
            for (int i = 0; i < animBaseUis.Length; ++i)
                CollectSpineFromAnimBaseUi(animBaseUis[i]);
        }

        SkeletonRenderer[] renderers = UnityEngine.Object.FindObjectsOfType<SkeletonRenderer>(true);
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; ++i)
                TryAddRenderer(renderers[i]);
        }

        SkeletonGraphic[] graphics = UnityEngine.Object.FindObjectsOfType<SkeletonGraphic>(true);
        if (graphics != null)
        {
            for (int i = 0; i < graphics.Length; ++i)
                TryAddGraphic(graphics[i]);
        }
    }

    static void RefreshWrapTargetDisplayMap()
    {
        s_wrapTargetDisplayed.Clear();

        if (GRoot.inst != null && GRoot.inst.displayObject is Container rootContainer)
        {
            IEnumerator<DisplayObject> descendants = rootContainer.GetDescendants(false);
            while (descendants.MoveNext())
            {
                DisplayObject displayObject = descendants.Current;
                if (displayObject is GoWrapper wrapper)
                    RecordWrapTargetDisplay(wrapper.wrapTarget, IsGoWrapperDisplayed(wrapper));
            }
        }

        if (s_cachedPools == null)
            return;

        for (int i = 0; i < s_cachedPools.Length; ++i)
        {
            FguiPool pool = s_cachedPools[i];
            if (pool == null || pool.pool == null)
                continue;

            foreach (GObject poolItem in pool.pool)
            {
                if (poolItem == null)
                    continue;

                GComponent comp = poolItem.asCom;
                if (comp == null)
                    continue;

                RecordWrapTargetDisplay(
                    GameCommon.FguiUtils.GetWrapperTarget(comp),
                    IsGObjectDisplayed(comp));
            }
        }
    }

    static void CollectSpineFromAnimBaseUi(AnimBaseUI anim)
    {
        if (anim == null)
            return;

        GameObject animRoot = anim.goAnim != null ? anim.goAnim : anim.gameObject;
        if (animRoot == null)
            return;

        SkeletonRenderer[] renderers = animRoot.GetComponentsInChildren<SkeletonRenderer>(true);
        for (int i = 0; i < renderers.Length; ++i)
            TryAddRenderer(renderers[i]);

        SkeletonGraphic[] graphics = animRoot.GetComponentsInChildren<SkeletonGraphic>(true);
        for (int i = 0; i < graphics.Length; ++i)
            TryAddGraphic(graphics[i]);
    }

    static void CollectWrapTargets()
    {
        s_wrapTargets.Clear();
        s_wrapTargetIds.Clear();

        if (GRoot.inst != null && GRoot.inst.displayObject is Container rootContainer)
        {
            IEnumerator<DisplayObject> descendants = rootContainer.GetDescendants(false);
            while (descendants.MoveNext())
            {
                DisplayObject displayObject = descendants.Current;
                if (displayObject is GoWrapper wrapper)
                    TryAddWrapTarget(wrapper.wrapTarget);
            }
        }

        if (s_cachedPools == null)
            return;

        for (int i = 0; i < s_cachedPools.Length; ++i)
        {
            FguiPool pool = s_cachedPools[i];
            if (pool == null || pool.pool == null)
                continue;

            foreach (GObject poolItem in pool.pool)
            {
                if (poolItem == null)
                    continue;

                GComponent comp = poolItem.asCom;
                if (comp == null)
                    continue;

                TryAddWrapTarget(GameCommon.FguiUtils.GetWrapperTarget(comp));
            }
        }
    }

    static void RecordWrapTargetDisplay(GameObject target, bool isDisplayed)
    {
        if (target == null)
            return;

        int id = target.GetInstanceID();
        if (s_wrapTargetDisplayed.TryGetValue(id, out bool existing))
            s_wrapTargetDisplayed[id] = existing || isDisplayed;
        else
            s_wrapTargetDisplayed[id] = isDisplayed;
    }

    static void TryAddWrapTarget(GameObject target)
    {
        if (target == null)
            return;

        int id = target.GetInstanceID();
        if (!s_wrapTargetIds.Add(id))
            return;

        s_wrapTargets.Add(target);
    }

    static void CollectSpineOnGameObject(GameObject root)
    {
        if (root == null)
            return;

        SkeletonRenderer[] renderers = root.GetComponentsInChildren<SkeletonRenderer>(true);
        for (int i = 0; i < renderers.Length; ++i)
            TryAddRenderer(renderers[i]);

        SkeletonGraphic[] graphics = root.GetComponentsInChildren<SkeletonGraphic>(true);
        for (int i = 0; i < graphics.Length; ++i)
            TryAddGraphic(graphics[i]);
    }

    static void TryAddRenderer(SkeletonRenderer renderer)
    {
        if (renderer == null)
            return;

        int id = renderer.GetInstanceID();
        if (!s_scratchIds.Add(id))
            return;

        s_renderers.Add(renderer);
    }

    static void TryAddGraphic(SkeletonGraphic graphic)
    {
        if (graphic == null)
            return;

        int id = graphic.GetInstanceID();
        if (!s_scratchIds.Add(id))
            return;

        s_graphics.Add(graphic);
    }

    static void SyncHooks()
    {
        s_scratchIds.Clear();

        for (int i = 0; i < s_renderers.Count; ++i)
        {
            SkeletonRenderer renderer = s_renderers[i];
            if (renderer == null)
                continue;

            int id = renderer.GetInstanceID();
            s_scratchIds.Add(id);
            if (!s_hookedRendererIds.Contains(id))
                SubscribeRenderer(renderer);
        }

        var rendererIdsToRemove = new List<int>(8);
        foreach (int id in s_hookedRendererIds)
        {
            if (!s_scratchIds.Contains(id))
                rendererIdsToRemove.Add(id);
        }

        for (int i = 0; i < rendererIdsToRemove.Count; ++i)
            UnsubscribeRenderer(rendererIdsToRemove[i]);

        s_scratchIds.Clear();
        for (int i = 0; i < s_graphics.Count; ++i)
        {
            SkeletonGraphic graphic = s_graphics[i];
            if (graphic == null)
                continue;

            int id = graphic.GetInstanceID();
            s_scratchIds.Add(id);
            if (!s_hookedGraphicIds.Contains(id))
                SubscribeGraphic(graphic);
        }

        var graphicIdsToRemove = new List<int>(8);
        foreach (int id in s_hookedGraphicIds)
        {
            if (!s_scratchIds.Contains(id))
                graphicIdsToRemove.Add(id);
        }

        for (int i = 0; i < graphicIdsToRemove.Count; ++i)
            UnsubscribeGraphic(graphicIdsToRemove[i]);
    }

    static void SubscribeRenderer(SkeletonRenderer renderer)
    {
        if (renderer == null)
            return;

        int id = renderer.GetInstanceID();
        if (s_hookedRendererIds.Contains(id))
            return;

        SkeletonRenderer.SkeletonRendererDelegate callback = OnRendererMeshUpdated;
        s_rendererCallbacks[id] = callback;
        s_hookedRenderers[id] = renderer;
        renderer.OnMeshAndMaterialsUpdated += callback;
        s_hookedRendererIds.Add(id);
    }

    static void SubscribeGraphic(SkeletonGraphic graphic)
    {
        if (graphic == null)
            return;

        int id = graphic.GetInstanceID();
        if (s_hookedGraphicIds.Contains(id))
            return;

        SkeletonGraphic.SkeletonRendererDelegate callback = OnGraphicMeshUpdated;
        s_graphicCallbacks[id] = callback;
        s_hookedGraphics[id] = graphic;
        graphic.OnMeshAndMaterialsUpdated += callback;
        s_hookedGraphicIds.Add(id);
    }

    static void OnRendererMeshUpdated(SkeletonRenderer renderer)
    {
        if (IsRendererDisplayedInScene(renderer))
            Interlocked.Increment(ref s_meshUpdateCount);
    }

    static void OnGraphicMeshUpdated(SkeletonGraphic graphic)
    {
        if (IsGraphicDisplayedInScene(graphic))
            Interlocked.Increment(ref s_meshUpdateCount);
    }

    static void UnsubscribeRenderer(int id)
    {
        if (s_rendererCallbacks.TryGetValue(id, out SkeletonRenderer.SkeletonRendererDelegate callback)
            && s_hookedRenderers.TryGetValue(id, out SkeletonRenderer renderer)
            && renderer != null)
        {
            renderer.OnMeshAndMaterialsUpdated -= callback;
        }

        s_rendererCallbacks.Remove(id);
        s_hookedRenderers.Remove(id);
        s_hookedRendererIds.Remove(id);
    }

    static void UnsubscribeGraphic(int id)
    {
        if (s_graphicCallbacks.TryGetValue(id, out SkeletonGraphic.SkeletonRendererDelegate callback)
            && s_hookedGraphics.TryGetValue(id, out SkeletonGraphic graphic)
            && graphic != null)
        {
            graphic.OnMeshAndMaterialsUpdated -= callback;
        }

        s_graphicCallbacks.Remove(id);
        s_hookedGraphics.Remove(id);
        s_hookedGraphicIds.Remove(id);
    }

    static void UnsubscribeAll()
    {
        var rendererIds = new List<int>(s_hookedRendererIds);
        for (int i = 0; i < rendererIds.Count; ++i)
            UnsubscribeRenderer(rendererIds[i]);

        var graphicIds = new List<int>(s_hookedGraphicIds);
        for (int i = 0; i < graphicIds.Count; ++i)
            UnsubscribeGraphic(graphicIds[i]);

        s_rendererCallbacks.Clear();
        s_graphicCallbacks.Clear();
        s_hookedRenderers.Clear();
        s_hookedGraphics.Clear();
        s_hookedRendererIds.Clear();
        s_hookedGraphicIds.Clear();
    }

    static void AccumulateRenderer(SkeletonRenderer renderer, ref LiveSpineStats stats)
    {
        if (!IsRendererDisplayedInScene(renderer))
            return;

        stats.displayedInstanceCount++;
        stats.totalVertices += GetRendererVertexCount(renderer);
        stats.totalBones += GetRendererBoneCount(renderer);
    }

    static void AccumulateGraphic(SkeletonGraphic graphic, ref LiveSpineStats stats)
    {
        if (!IsGraphicDisplayedInScene(graphic))
            return;

        stats.displayedInstanceCount++;
        stats.totalVertices += GetGraphicVertexCount(graphic);
        stats.totalBones += GetGraphicBoneCount(graphic);
    }

    static bool IsGoWrapperDisplayed(GoWrapper wrapper)
    {
        if (wrapper == null)
            return false;

        GObject owner = wrapper.gOwner;
        if (owner == null)
            return false;

        GObject anchor = owner.parent ?? owner;
        return IsGObjectDisplayed(anchor);
    }

    static bool IsGObjectDisplayed(GObject obj)
    {
        if (obj == null || !obj.visible || !obj.onStage)
            return false;

        GObject current = obj.parent;
        while (current != null)
        {
            if (!current.visible)
                return false;
            current = current.parent;
        }

        return true;
    }

    static bool IsRendererDisplayedInScene(SkeletonRenderer renderer)
    {
        if (renderer == null || !renderer.enabled)
            return false;

        if (TryGetWrapTargetDisplayed(renderer.gameObject, out bool wrapDisplayed))
            return wrapDisplayed && IsRendererRenderable(renderer);

        if (!renderer.gameObject.activeInHierarchy)
            return false;

        return IsRendererRenderable(renderer);
    }

    static bool IsGraphicDisplayedInScene(SkeletonGraphic graphic)
    {
        if (graphic == null || !graphic.enabled || graphic.freeze)
            return false;

        if (!graphic.gameObject.activeInHierarchy)
            return false;

        CanvasRenderer canvasRenderer = graphic.canvasRenderer;
        if (canvasRenderer != null && canvasRenderer.cull)
            return false;

        if (TryGetWrapTargetDisplayed(graphic.gameObject, out bool wrapDisplayed))
            return wrapDisplayed;

        return graphic.IsValid;
    }

    static bool TryGetWrapTargetDisplayed(GameObject go, out bool isDisplayed)
    {
        Transform current = go.transform;
        while (current != null)
        {
            if (s_wrapTargetDisplayed.TryGetValue(current.gameObject.GetInstanceID(), out isDisplayed))
                return true;
            current = current.parent;
        }

        isDisplayed = false;
        return false;
    }

    static bool IsRendererRenderable(SkeletonRenderer renderer)
    {
        MeshRenderer meshRenderer = renderer.GetComponent<MeshRenderer>();
        if (meshRenderer == null || !meshRenderer.enabled)
            return false;

        return GetRendererVertexCount(renderer) > 0 || meshRenderer.isVisible;
    }

    static int GetRendererVertexCount(SkeletonRenderer renderer)
    {
        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        return mesh != null ? mesh.vertexCount : 0;
    }

    static int GetGraphicVertexCount(SkeletonGraphic graphic)
    {
        if (graphic.allowMultipleCanvasRenderers)
        {
            int total = 0;
            Spine.ExposedList<Mesh> meshes = graphic.MeshesMultipleCanvasRenderers;
            if (meshes != null)
            {
                Mesh[] items = meshes.Items;
                for (int i = 0; i < meshes.Count; ++i)
                {
                    Mesh mesh = items[i];
                    if (mesh != null)
                        total += mesh.vertexCount;
                }
            }

            return total;
        }

        Mesh lastMesh = graphic.GetLastMesh();
        return lastMesh != null ? lastMesh.vertexCount : 0;
    }

    static int GetRendererBoneCount(SkeletonRenderer renderer)
    {
        if (renderer == null)
            return 0;

        try
        {
            Skeleton skeleton = renderer.Skeleton;
            if (skeleton?.Bones != null)
                return skeleton.Bones.Count;
        }
        catch
        {
            // ignored
        }

        return 0;
    }

    static int GetGraphicBoneCount(SkeletonGraphic graphic)
    {
        if (graphic == null)
            return 0;

        try
        {
            Skeleton skeleton = graphic.Skeleton;
            if (skeleton?.Bones != null)
                return skeleton.Bones.Count;
        }
        catch
        {
            // ignored
        }

        return 0;
    }
}
