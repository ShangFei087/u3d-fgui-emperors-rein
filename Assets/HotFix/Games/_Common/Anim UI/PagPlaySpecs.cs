using System;

/// <summary>Play 布局：positionType / layoutExtra / turntable 回退。</summary>
public readonly struct PagPlayLayout
{
    public string PositionType { get; }
    public string LayoutExtra { get; }
    public bool UseTurntableFallback { get; }

    public PagPlayLayout(string positionType, string layoutExtra, bool useTurntableFallback)
    {
        PositionType = positionType ?? "center";
        LayoutExtra = layoutExtra ?? string.Empty;
        UseTurntableFallback = useTurntableFallback;
    }

    public static PagPlayLayout Center => new PagPlayLayout("center", string.Empty, true);
    public static PagPlayLayout Fullscreen => new PagPlayLayout("full", string.Empty, false);
}

/// <summary>同步 Play 后的 Native 事件回调（一次订阅，自动解绑）。</summary>
public readonly struct PagPlayCallbacks
{
    public Action OnStarted { get; }
    public Action OnFinished { get; }
    public Action OnFailed { get; }
    public float StartedTimeoutSec { get; }
    public float FinishedTimeoutSec { get; }
    public bool StopAfterFinished { get; }
    public bool UnsubscribeOnFire { get; }

    public PagPlayCallbacks(
        Action onStarted = null,
        Action onFinished = null,
        Action onFailed = null,
        float startedTimeoutSec = 45f,
        float finishedTimeoutSec = 0f,
        bool stopAfterFinished = false,
        bool unsubscribeOnFire = true)
    {
        OnStarted = onStarted;
        OnFinished = onFinished;
        OnFailed = onFailed;
        StartedTimeoutSec = startedTimeoutSec;
        FinishedTimeoutSec = finishedTimeoutSec;
        StopAfterFinished = stopAfterFinished;
        UnsubscribeOnFire = unsubscribeOnFire;
    }

    public bool HasAnyCallback =>
        OnStarted != null || OnFinished != null || OnFailed != null
        || StartedTimeoutSec > 0f || FinishedTimeoutSec > 0f;
}

/// <summary>Native playlist 序列播放（intro→loop / NPC 共用）。</summary>
public readonly struct PagSequencePlay
{
    public PagSegment[] Segments { get; }
    public PagPlayLayout Layout { get; }
    public float DisplayScale { get; }
    public bool UseGpuSyncGroup { get; }
    public PagPlayCallbacks Callbacks { get; }

    public PagSequencePlay(
        PagSegment[] segments,
        PagPlayLayout layout = default,
        float displayScale = PagPresentationDefaults.DisplayScale,
        bool useGpuSyncGroup = PagPresentationDefaults.UseGpuSyncGroup,
        PagPlayCallbacks callbacks = default)
    {
        Segments = segments;
        Layout = layout.PositionType == null ? PagPlayLayout.Center : layout;
        DisplayScale = displayScale;
        UseGpuSyncGroup = useGpuSyncGroup;
        Callbacks = callbacks;
    }
}

/// <summary>PagSegment 工厂与默认 Play 参数。</summary>
public static class PagPlaySpecs
{
    public static PagSegment[] FromFiles(string[] files, int repeat = 1)
    {
        if (files == null || files.Length == 0)
        {
            return Array.Empty<PagSegment>();
        }

        var segments = new PagSegment[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            segments[i] = new PagSegment(files[i], repeat);
        }

        return segments;
    }

    /// <summary>intro 播一次后 loop 无限循环（loop 段 repeat=-1，不会触发 OnPlaybackFinished）。</summary>
    public static PagSegment[] IntroLoop(string introFile, string loopFile)
    {
        return new[]
        {
            new PagSegment(introFile, 1),
            new PagSegment(loopFile, -1),
        };
    }
}
