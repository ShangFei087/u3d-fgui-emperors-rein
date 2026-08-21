/// <summary>
/// 多 PAG 同屏播放统一入口：底层由 PagGpuSyncGroup 动态合组 + 批量 flush。
/// </summary>
public static class PagConcurrentPlayback
{
    /// <summary>开启后纹理模式 Play 自动 TryJoin、Stop 自动 TryLeave。</summary>
    public static bool Enabled
    {
        get => PagController.AutoConcurrentGpuSync;
        set => PagController.AutoConcurrentGpuSync = value;
    }

    public static bool IsGroupActive => PagGpuSyncGroup.IsActive;

    public static int ActiveMemberCount => PagGpuSyncGroup.MemberCount;
}

/// <summary>全游戏 PAG 渲染/播放管线默认参数（与 PagController / PagSlotBinding 配套）。</summary>
public static class PagPresentationDefaults
{
    public const float DisplayScale = 1f;//FGUI 显示缩放；1=按合成尺寸×1 显示
    public const bool ClampDisplayToHolder = false;//按合成尺寸×displayScale；true：裁剪到 holder。
    public const bool UseGpuSyncGroup = false; // 单路独立出帧；多实例同屏时由调用方显式合组
    public const bool UseFguiTexture = true;//Fgui纹理模式
    public const int FguiMaxDisplaySide = 0;//FguiTexture 离屏最大边；0=合成原尺寸不限制，512=降压缩屏（FGUI 仍按合成原尺寸显示）。
    public const int FguiFps = 30;//纹理模式出帧目标帧率
    public const bool OverlayFallback = false;//Overlay 模式：true 时 native 立即 ImageView 软件出帧。

    /// <summary>初始化全局 PAG 管线开关（EnsurePagTestSlots / Page OnOpen 时调用一次即可）。</summary>
    public static void ApplyPipelineGlobals()
    {
        PagController.AutoConcurrentGpuSync = UseGpuSyncGroup;
    }
}
