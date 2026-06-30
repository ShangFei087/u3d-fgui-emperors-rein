/// <summary>
/// 多 PAG 同屏播放统一入口：底层由 PagGpuSyncGroup 动态合组 + 批量 flush。
/// </summary>
public static class PagConcurrentPlayback
{
    /// <summary>开启后 FGUI GPU Play 自动 TryJoin、Stop 自动 TryLeave。</summary>
    public static bool Enabled
    {
        get => PagController.AutoConcurrentGpuSync;
        set => PagController.AutoConcurrentGpuSync = value;
    }

    public static bool IsGroupActive => PagGpuSyncGroup.IsActive;

    public static int ActiveMemberCount => PagGpuSyncGroup.MemberCount;
}
