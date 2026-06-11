using System;
using FairyGUI;
using UnityEngine;

/// <summary>
/// 将 Native PAG GPU 离屏纹理（ExternalTexture）显示在 FGUI GLoader 上；层级由编辑器中 pagEffect 的兄弟顺序决定。
/// </summary>
public sealed class PagFguiGpuPresenter
{
    public const string DefaultLoaderName = "pagEffect";

    private GLoader _loader;
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

    public GLoader Loader => _loader;

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

    public void Bind(GLoader loader)
    {
        ReleaseTexture();
        _loader = loader;
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
            _nTexture = new NTexture(_externalTexture);
            _loader.texture = _nTexture;
            InvalidateLoaderBatching();
        }

        RefreshLoaderLayout();
    }

    public void OnGpuFrameReady()
    {
        if (_loader == null)
        {
            return;
        }

        if (_nTexture != null)
        {
            _nTexture.lastActive = Time.time;
        }
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
        _displayW = 0;
        _displayH = 0;
        _syncedLoaderW = 0;
        _syncedLoaderH = 0;
        _texW = 0;
        _texH = 0;
        _boundNativePtr = IntPtr.Zero;
        _loaderVisible = false;
    }

    private void InvalidateLoaderBatching()
    {
        _loader?.InvalidateBatchingState();
    }

    private void EnsureDisplaySizeFallback()
    {
        if (_displayW > 0 && _displayH > 0)
        {
            return;
        }

        GComponent anchor = _loader?.parent as GComponent;
        if (anchor == null)
        {
            return;
        }

        ResolveAnchorLayoutSize(anchor, out float w, out float h);
        _displayW = Mathf.RoundToInt(w);
        _displayH = Mathf.RoundToInt(h);
    }

    private void SyncLoaderToDisplaySize()
    {
        if (_loader == null || _displayW <= 0 || _displayH <= 0)
        {
            return;
        }

        _loader.fill = FillType.ScaleFree;

        if (_syncedLoaderW == _displayW && _syncedLoaderH == _displayH)
        {
            return;
        }

        _syncedLoaderW = _displayW;
        _syncedLoaderH = _displayH;
        _loader.SetSize(_displayW, _displayH);
        InvalidateLoaderBatching();

        GComponent anchor = _loader.parent as GComponent;
        if (anchor != null)
        {
            ResolveAnchorCenter(anchor, out float cx, out float cy);
            _loader.SetPivot(0.5f, 0.5f, true);
            _loader.SetXY(cx, cy);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"[PAG FGUI GPU] pagEffect display {_displayW}x{_displayH}, texture {_texW}x{_texH}, fill=ScaleFree");
#endif
    }

    private void RefreshLoaderLayout()
    {
        if (_loader == null || _displayW <= 0 || _displayH <= 0)
        {
            return;
        }

        if (_syncedLoaderW == _displayW && _syncedLoaderH == _displayH)
        {
            return;
        }

        _syncedLoaderW = _displayW;
        _syncedLoaderH = _displayH;
        _loader.fill = FillType.ScaleFree;
        _loader.SetSize(_displayW, _displayH);
    }

    private static void ResolveAnchorLayoutSize(GComponent anchor, out float w, out float h)
    {
        w = 200f;
        h = 200f;
        if (anchor == null)
        {
            return;
        }

        GGraph holder = anchor.GetChild("holder")?.asGraph;
        if (holder != null && holder.width > 0f && holder.height > 0f)
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

    private static void ResolveAnchorCenter(GComponent anchor, out float cx, out float cy)
    {
        cx = 0f;
        cy = 0f;
        if (anchor == null)
        {
            return;
        }

        GGraph holder = anchor.GetChild("holder")?.asGraph;
        if (holder != null && holder.width > 0f && holder.height > 0f)
        {
            cx = holder.x + holder.width * holder.pivotX;
            cy = holder.y + holder.height * holder.pivotY;
            return;
        }

        GLoader example = anchor.GetChild("example")?.asLoader;
        if (example != null)
        {
            cx = example.x + example.width * example.pivotX;
            cy = example.y + example.height * example.pivotY;
        }
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
