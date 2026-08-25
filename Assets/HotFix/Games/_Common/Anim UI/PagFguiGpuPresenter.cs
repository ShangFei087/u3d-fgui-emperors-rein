using System;
using FairyGUI;
using UnityEngine;

/// <summary>
/// 将 Native PAG 离屏纹理（ExternalTexture）显示在 FGUI GLoader 上；层级由编辑器中 pagEffect 的兄弟顺序决定。
/// </summary>
public sealed class PagFguiGpuPresenter
{
    public const string DefaultLoaderName = "pagEffect";

    private GLoader _loader;
    private GComponent _fguiAnchor;
    private string _loaderName = DefaultLoaderName;
    private Texture2D _externalTexture;
    private NTexture _nTexture;
    private int _displayW;
    private int _displayH;
    private int _syncedLoaderW;
    private int _syncedLoaderH;
    private int _texW;
    private int _texH;
    private IntPtr _boundNativePtr = IntPtr.Zero;
    private bool _loaderVisible;
    private bool _clampDisplayToHolder = false;

    public GLoader Loader => _loader;

    /// <summary>loader 已有纹理且当前可见时为 true，换片时可保留末帧避免空闪。</summary>
    public bool HasVisibleContent =>
        _loader != null && _loader.texture != null && _loader.visible;

    /// <summary>为 true 时 GLoader 显示尺寸不超过 holder，离屏纹理仍用合成原尺寸。</summary>
    public bool ClampDisplayToHolder
    {
        get => _clampDisplayToHolder;
        set => _clampDisplayToHolder = value;
    }

    public bool NeedsDisplayLayoutResync => _displayW <= 0 || _displayH <= 0;

    public static GLoader TryGetPagEffectLoader(GComponent anchor, string loaderName = DefaultLoaderName)
    {
        if (anchor == null)
        {
            return null;
        }

        GObject existing = anchor.GetChild(loaderName);
        if (existing == null)
        {
            Debug.LogError($"[PAG FGUI] 锚点 {anchor.name} 缺少 GLoader「{loaderName}」，请在 FGUI 编辑器中手动添加");
            return null;
        }

        GLoader loader = existing.asLoader;
        if (loader == null)
        {
            Debug.LogError($"[PAG FGUI] 锚点 {anchor.name} 的子节点「{loaderName}」不是 GLoader");
        }

        return loader;
    }

    public void ConfigureAnchor(GComponent anchor, string loaderName)
    {
        if (anchor != null)
        {
            _fguiAnchor = anchor;
        }

        if (!string.IsNullOrEmpty(loaderName))
        {
            _loaderName = loaderName;
        }
    }

    public void Bind(GLoader loader)
    {
        if (loader == null)
        {
            return;
        }

        if (_loader == loader)
        {
            return;
        }

        ReleaseTexture();
        _loader = loader;
        if (_fguiAnchor == null)
        {
            _fguiAnchor = loader.parent as GComponent;
        }

        if (string.IsNullOrEmpty(_loaderName) || _loaderName == DefaultLoaderName)
        {
            _loaderName = string.IsNullOrEmpty(loader.name) ? DefaultLoaderName : loader.name;
        }

        ResetDisplaySizeForNewComposition();
    }

    public void SetDisplaySize(int width, int height)
    {
        if (width > 0 && height > 0)
        {
            _displayW = width;
            _displayH = height;
            _syncedLoaderW = 0;
            _syncedLoaderH = 0;
        }
    }

    public void ResetDisplaySizeForNewComposition()
    {
        _displayW = 0;
        _displayH = 0;
        _syncedLoaderW = 0;
        _syncedLoaderH = 0;
    }

    public void SetVisible(bool visible)
    {
        if (_loader == null)
        {
            return;
        }

        if (_loader.visible != visible)
        {
            _loader.visible = visible;
            _loaderVisible = visible;
            if (visible)
            {
                InvalidateLoaderBatching();
            }
        }
    }

    public void BindExternalTexture(IntPtr nativePtr, int texW, int texH)
    {
        if (_loader == null || nativePtr == IntPtr.Zero || texW <= 0 || texH <= 0)
        {
            return;
        }

        EnsureDisplaySizeFallback();
        SyncLoaderToDisplaySize();

        bool rebuild = _externalTexture == null || _texW != texW || _texH != texH
            || _boundNativePtr != nativePtr;
        if (rebuild)
        {
            ReleaseTexture();
            _texW = texW;
            _texH = texH;
            _boundNativePtr = nativePtr;
            _externalTexture = Texture2D.CreateExternalTexture(
                texW, texH, TextureFormat.RGBA32, false, true, nativePtr);
            _nTexture = new NTexture(_externalTexture)
            {
                destroyMethod = DestroyMethod.None
            };
            _loader.texture = _nTexture;
            InvalidateLoaderBatching();
        }

        SyncLoaderToDisplaySize();
        RefreshLoaderLayout();
    }

    public void OnGpuFrameReady()
    {
        UpdateGpuFrameTexture();
        InvalidateBatchingOnce();
    }

    /// <summary>SyncGroup batch present：仅更新纹理，Invalidate 由组级统一触发。</summary>
    public void UpdateGpuFrameTexture()
    {
        if (_loader == null)
        {
            return;
        }

        if (_nTexture != null)
        {
            _nTexture.lastActive = Time.time;
        }

        if (_externalTexture != null && _boundNativePtr != IntPtr.Zero)
        {
            _externalTexture.UpdateExternalTexture(_boundNativePtr);
        }
    }

    public void InvalidateBatchingOnce()
    {
        InvalidateLoaderBatching();
    }

    /// <summary>holder 布局就绪后强制按合成/holder 尺寸重算 pagEffect（避免初始 100x100）。</summary>
    public void RefreshDisplayLayout()
    {
        if (_loader == null)
        {
            return;
        }

        _syncedLoaderW = 0;
        _syncedLoaderH = 0;
        EnsureDisplaySizeFallback();
        SyncLoaderToDisplaySize();
        RefreshLoaderLayout();
        InvalidateLoaderBatching();
    }

    public void Clear()
    {
        SetVisible(false);
        if (_loader != null)
        {
            _loader.texture = null;
        }

        ReleaseTexture();
        _fguiAnchor = null;
        _loaderName = DefaultLoaderName;
        _displayW = 0;
        _displayH = 0;
        _syncedLoaderW = 0;
        _syncedLoaderH = 0;
        _texW = 0;
        _texH = 0;
        _boundNativePtr = IntPtr.Zero;
        _loaderVisible = false;
    }

    /// <summary>Dispose 时断开 GLoader，避免 contentPane 销毁后仍持有已释放节点。</summary>
    public void DetachLoader()
    {
        Clear();
        _loader = null;
    }

    private void InvalidateLoaderBatching()
    {
        _loader?.InvalidateBatchingState();
    }

    private bool TryResolveEffectiveDisplaySize(out int width, out int height)
    {
        width = _displayW;
        height = _displayH;

        if (width <= 0 || height <= 0)
        {
            if (!_clampDisplayToHolder)
            {
                return false;
            }

            GComponent anchor = ResolveAnchor();
            if (anchor == null)
            {
                return false;
            }

            ResolveAnchorLayoutSize(anchor, _loaderName, out float holderW, out float holderH);
            width = Mathf.Max(1, Mathf.RoundToInt(holderW));
            height = Mathf.Max(1, Mathf.RoundToInt(holderH));
            return true;
        }

        if (!_clampDisplayToHolder)
        {
            return true;
        }

        GComponent layoutAnchor = ResolveAnchor();
        if (layoutAnchor == null)
        {
            return true;
        }

        ResolveAnchorLayoutSize(layoutAnchor, _loaderName, out float maxW, out float maxH);
        if (maxW <= 0f || maxH <= 0f)
        {
            return true;
        }

        if (width <= maxW && height <= maxH)
        {
            return true;
        }

        float scale = Mathf.Min(maxW / width, maxH / height);
        width = Mathf.Max(1, Mathf.RoundToInt(width * scale));
        height = Mathf.Max(1, Mathf.RoundToInt(height * scale));
        return true;
    }

    private GComponent ResolveAnchor()
    {
        if (_fguiAnchor != null)
        {
            return _fguiAnchor;
        }

        return _loader?.parent as GComponent;
    }

    private void EnsureDisplaySizeFallback()
    {
        if (_displayW > 0 && _displayH > 0)
        {
            return;
        }

        if (!_clampDisplayToHolder)
        {
            return;
        }

        GComponent anchor = ResolveAnchor();
        if (anchor == null)
        {
            return;
        }

        ResolveAnchorLayoutSize(anchor, _loaderName, out float w, out float h);
        _displayW = Mathf.RoundToInt(w);
        _displayH = Mathf.RoundToInt(h);
    }

    private void SyncLoaderToDisplaySize()
    {
        if (_loader == null || !TryResolveEffectiveDisplaySize(out int displayW, out int displayH))
        {
            return;
        }

        _loader.fill = FillType.ScaleFree;

        if (_syncedLoaderW == displayW && _syncedLoaderH == displayH)
        {
            return;
        }

        _syncedLoaderW = displayW;
        _syncedLoaderH = displayH;
        _loader.SetSize(displayW, displayH);
        InvalidateLoaderBatching();

        TrySyncLoaderPosition();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"[PAG Texture] pagEffect display {displayW}x{displayH}, composition={_displayW}x{_displayH}, "
            + $"texture {_texW}x{_texH}, holderClamp={_clampDisplayToHolder}, anchor={ResolveAnchor()?.name}, loader={_loaderName}");
#endif
    }

    private bool TrySyncLoaderPosition()
    {
        if (_loader == null)
        {
            return false;
        }

        GComponent anchor = _loader.parent as GComponent;
        if (anchor == null)
        {
            return false;
        }

        string dedicatedHolderName = ResolveHolderName(_loaderName);
        if (dedicatedHolderName != "holder")
        {
            GGraph dedicatedHolder = anchor.GetChild(dedicatedHolderName)?.asGraph;
            if (dedicatedHolder != null && dedicatedHolder.width > 0f && dedicatedHolder.height > 0f)
            {
                return TryCopyLayoutTransformFromReference(dedicatedHolder, _loader);
            }
        }

        if (_loaderName == DefaultLoaderName || string.IsNullOrEmpty(_loaderName))
        {
            GLoader example = anchor.GetChild("example")?.asLoader;
            if (example != null)
            {
                return TryCopyLayoutTransformFromReference(example, _loader);
            }
        }

        return false;
    }

    /// <summary>
    /// 与 holder 对齐 pivot/xy。anchor=true 时 x/y 已是 pivot 点，不可再 + width*pivot。
    /// </summary>
    private static bool TryCopyLayoutTransformFromReference(GObject reference, GLoader loader)
    {
        if (reference == null || loader == null)
        {
            return false;
        }

        loader.SetPivot(reference.pivotX, reference.pivotY, reference.pivotAsAnchor);
        loader.SetXY(reference.x, reference.y);
        return true;
    }

    private static string ResolveHolderName(string loaderName)
    {
        if (loaderName == "pagEffect1")
        {
            return "holder1";
        }

        if (loaderName == "pagEffect2")
        {
            return "holder2";
        }

        if (loaderName == "pagEffect3")
        {
            return "holder3";
        }

        if (loaderName == "pagEffect4")
        {
            return "holder4";
        }

        if (loaderName == "pagEffect5")
        {
            return "holder5";
        }

        if (loaderName == "pagEffect6")
        {
            return "holder6";
        }

        if (loaderName == "pagEffect7")
        {
            return "holder7";
        }

        if (loaderName == "pagEffect8")
        {
            return "holder8";
        }

        if (loaderName == "pagEffect9")
        {
            return "holder9";
        }

        if (loaderName == "pagEffect10")
        {
            return "holder10";
        }

        if (loaderName == "pagEffect11")
        {
            return "holder11";
        }

        if (loaderName == "pagEffect12")
        {
            return "holder12";
        }

        if (loaderName == "pagEffect13")
        {
            return "holder13";
        }

        if (loaderName == "pagEffect14")
        {
            return "holder14";
        }

        return "holder";
    }

    private static GGraph TryGetLayoutHolder(GComponent anchor, string loaderName)
    {
        if (anchor == null)
        {
            return null;
        }

        string holderName = ResolveHolderName(loaderName);
        GGraph holder = anchor.GetChild(holderName)?.asGraph;
        if (holder != null && holder.width > 0f && holder.height > 0f)
        {
            return holder;
        }

        if (holderName != "holder")
        {
            holder = anchor.GetChild("holder")?.asGraph;
            if (holder != null && holder.width > 0f && holder.height > 0f)
            {
                return holder;
            }
        }

        return null;
    }

    private void RefreshLoaderLayout()
    {
        if (_loader == null || !TryResolveEffectiveDisplaySize(out int displayW, out int displayH))
        {
            return;
        }

        if (_syncedLoaderW == displayW && _syncedLoaderH == displayH)
        {
            return;
        }

        _syncedLoaderW = displayW;
        _syncedLoaderH = displayH;
        _loader.fill = FillType.ScaleFree;
        _loader.SetSize(displayW, displayH);
    }

    private static void ResolveAnchorLayoutSize(GComponent anchor, string loaderName, out float w, out float h)
    {
        w = 200f;
        h = 200f;
        if (anchor == null)
        {
            return;
        }

        GGraph holder = TryGetLayoutHolder(anchor, loaderName);
        if (holder != null)
        {
            w = holder.width;
            h = holder.height;
            return;
        }

        GLoader example = anchor.GetChild("example")?.asLoader;
        if (example != null && example.width > 0f && example.height > 0f)
        {
            w = example.width;
            h = example.height;
        }
    }

    private static void ResolveAnchorCenter(GComponent anchor, string loaderName, out float cx, out float cy)
    {
        cx = 0f;
        cy = 0f;
        if (anchor == null)
        {
            return;
        }

        GGraph holder = TryGetLayoutHolder(anchor, loaderName);
        if (holder != null)
        {
            ResolveObjectPivotPoint(holder, out cx, out cy);
            return;
        }

        GLoader example = anchor.GetChild("example")?.asLoader;
        if (example != null)
        {
            ResolveObjectPivotPoint(example, out cx, out cy);
        }
    }

    private static void ResolveObjectPivotPoint(GObject obj, out float cx, out float cy)
    {
        if (obj.pivotAsAnchor)
        {
            cx = obj.x;
            cy = obj.y;
            return;
        }

        cx = obj.x + obj.width * obj.pivotX;
        cy = obj.y + obj.height * obj.pivotY;
    }

    private void ReleaseTexture()
    {
        if (_nTexture != null)
        {
            _nTexture.Dispose();
            _nTexture = null;
        }

        if (_externalTexture != null)
        {
            UnityEngine.Object.Destroy(_externalTexture);
            _externalTexture = null;
        }
    }
}
