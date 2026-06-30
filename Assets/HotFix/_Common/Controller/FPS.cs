using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class FPS : MonoSingleton<FPS>
{
    [Header("性能检测总开关")]
    public bool enableProfiler = true;
    [Header("采样间隔(秒)")]
    public float updateInterval = 1.0f;
    [Header("平滑系数 0~1，越大变化越快")]
    [Range(0.01f, 1f)] public float smoothFactor = 0.2f;

    public UnityEvent<string> onFPSChange = new UnityEvent<string>();

    readonly FrameTiming[] _frameTimings = new FrameTiming[1];

    int _frames;
    int _timingFrames;
    int _validGpuFrames;
    float _lastSampleTime;
    float _frameStartRealtime;
    float _sumFrameMs;
    float _sumCpuMs;
    float _sumGpuMs;

    float _smoothFps;
    float _smoothFrameMs;
    float _smoothCpuMs;
    float _smoothGpuMs;

    int _displayedFps = -1;
    float _displayedFrameMs = -1f;
    int _displayedCpuMs = -1;
    int _displayedGpuMs = -1;
    bool _displayedGpuReliable;
    readonly StringBuilder _sb = new StringBuilder(128);
    string _fpsFormat;

    #region 对外只读属性

    public int DisplayFps => _displayedFps;
    public float DisplayFrameMs => _displayedFrameMs;
    public int DisplayCpuMs => _displayedCpuMs;
    public int DisplayGpuMs => _displayedGpuMs;
    public bool DisplayGpuTimingReliable => _displayedGpuReliable;

    public int DisplayLogicMs => _displayedCpuMs;
    public int DisplayRenderMs => _displayedGpuReliable ? _displayedGpuMs : -1;
    public bool DisplayRenderAvailable => _displayedGpuReliable;
    public string DisplayFormat => _fpsFormat ?? "--";

    #endregion

    void Awake()
    {
        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _lastSampleTime = Time.realtimeSinceStartup;
        ResetSampling();
    }

    void Update()
    {
        if (!enableProfiler)
            return;
        if (Time.unscaledDeltaTime <= 0f)
            return;

        if (Time.timeScale <= 0f)
        {
            ResetSampling();
            return;
        }

        _frameStartRealtime = Time.realtimeSinceStartup;
        FrameTimingManager.CaptureFrameTimings();
        ++_frames;
    }

    void LateUpdate()
    {
        if (!enableProfiler)
            return;
        if (Time.unscaledDeltaTime <= 0f || Time.timeScale <= 0f)
            return;

        AccumulateFrameTimings();

        float elapsed = Time.realtimeSinceStartup - _lastSampleTime;
        if (elapsed < updateInterval || _frames <= 0 || _timingFrames <= 0)
            return;

        float measuredFps = _frames / elapsed;
        _smoothFps = _smoothFps > float.Epsilon
            ? Mathf.Lerp(_smoothFps, measuredFps, smoothFactor)
            : measuredFps;

        float measuredFrame = _sumFrameMs / _timingFrames;
        _smoothFrameMs = _smoothFrameMs > float.Epsilon
            ? Mathf.Lerp(_smoothFrameMs, measuredFrame, smoothFactor)
            : measuredFrame;

        float measuredCpu = _sumCpuMs / _timingFrames;
        _smoothCpuMs = _smoothCpuMs > float.Epsilon
            ? Mathf.Lerp(_smoothCpuMs, measuredCpu, smoothFactor)
            : measuredCpu;

        bool gpuMeasured = _validGpuFrames > 0;
        if (gpuMeasured)
        {
            float measuredGpu = _sumGpuMs / _validGpuFrames;
            _smoothGpuMs = _smoothGpuMs > float.Epsilon
                ? Mathf.Lerp(_smoothGpuMs, measuredGpu, smoothFactor)
                : measuredGpu;
        }

        bool gpuReliable = gpuMeasured
            && _smoothGpuMs > float.Epsilon
            && _smoothGpuMs <= _smoothCpuMs * 1.2f;

        int roundedFps = Mathf.RoundToInt(_smoothFps);
        float roundedFrame = Mathf.Round(_smoothFrameMs * 10f) / 10f;
        int roundedCpu = Mathf.RoundToInt(_smoothCpuMs);
        int roundedGpu = gpuMeasured ? Mathf.RoundToInt(_smoothGpuMs) : -1;

        if (roundedFps != _displayedFps
            || !Mathf.Approximately(roundedFrame, _displayedFrameMs)
            || roundedCpu != _displayedCpuMs
            || roundedGpu != _displayedGpuMs
            || gpuReliable != _displayedGpuReliable)
        {
            _displayedFps = roundedFps;
            _displayedFrameMs = roundedFrame;
            _displayedCpuMs = roundedCpu;
            _displayedGpuMs = roundedGpu;
            _displayedGpuReliable = gpuReliable;

            _fpsFormat = BuildDisplayFormat(gpuMeasured, gpuReliable, roundedCpu, roundedGpu);
            onFPSChange.Invoke(_fpsFormat);
        }

        ResetSampling();
    }

    string BuildDisplayFormat(bool gpuMeasured, bool gpuReliable, int roundedCpu, int roundedGpu)
    {
        _sb.Clear();
        if (gpuReliable)
        {
            _sb.AppendFormat("{0:F0}FPS | \u5e27 {1:F1}ms | CPU {2}ms | GPU {3}ms",
                _smoothFps, _displayedFrameMs, roundedCpu, roundedGpu);
        }
        else if (gpuMeasured)
        {
            _sb.AppendFormat("{0:F0}FPS | \u5e27 {1:F1}ms | CPU {2}ms | GPU* --",
                _smoothFps, _displayedFrameMs, roundedCpu);
        }
        else
        {
            _sb.AppendFormat("{0:F0}FPS | \u5e27 {1:F1}ms | CPU {2}ms | GPU --",
                _smoothFps, _displayedFrameMs, roundedCpu);
        }

        return _sb.ToString();
    }

    void AccumulateFrameTimings()
    {
        float frameMs = Time.unscaledDeltaTime * 1000f;
        float cpuMs;
        float gpuMs = -1f;

        uint count = FrameTimingManager.GetLatestTimings(1, _frameTimings);
        if (count > 0)
        {
            cpuMs = (float)_frameTimings[0].cpuFrameTime;
            if (cpuMs <= float.Epsilon)
                cpuMs = (Time.realtimeSinceStartup - _frameStartRealtime) * 1000f;

            if (_frameTimings[0].gpuFrameTime > double.Epsilon)
                gpuMs = (float)_frameTimings[0].gpuFrameTime;
        }
        else
        {
            cpuMs = (Time.realtimeSinceStartup - _frameStartRealtime) * 1000f;
        }

        _sumFrameMs += frameMs;
        _sumCpuMs += cpuMs;
        if (gpuMs > float.Epsilon)
        {
            _sumGpuMs += gpuMs;
            ++_validGpuFrames;
        }

        ++_timingFrames;
    }

    void ResetSampling()
    {
        _frames = 0;
        _timingFrames = 0;
        _validGpuFrames = 0;
        _sumFrameMs = 0f;
        _sumCpuMs = 0f;
        _sumGpuMs = 0f;
        _lastSampleTime = Time.realtimeSinceStartup;
    }

    void ResetDisplayState()
    {
        _displayedFps = -1;
        _displayedFrameMs = -1f;
        _displayedCpuMs = -1;
        _displayedGpuMs = -1;
        _displayedGpuReliable = false;
        _smoothFps = 0f;
        _smoothFrameMs = 0f;
        _smoothCpuMs = 0f;
        _smoothGpuMs = 0f;
    }

    /// <summary>
    /// 手动开关性能监控
    /// </summary>
    public void SetEnable(bool enable)
    {
        enableProfiler = enable;
        ResetSampling();
        if (!enable)
        {
            ResetDisplayState();
            _fpsFormat = "\u6027\u80fd\u68c0\u6d4b\u5df2\u5173\u95ed";
            onFPSChange.Invoke(_fpsFormat);
        }
    }
}
