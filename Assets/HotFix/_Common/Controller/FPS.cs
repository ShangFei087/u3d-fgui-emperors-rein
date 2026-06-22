using UnityEngine;
using UnityEngine.Events;

public class FPS : MonoSingleton<FPS>
{
    public UnityEvent<string> onFPSChange = new UnityEvent<string>();

    const float _updateInterval = 1.0f;
    const float _smoothFactor = 0.2f;
    readonly FrameTiming[] _frameTimings = new FrameTiming[1];

    int _frames;
    int _timingFrames;
    int _validRenderFrames;
    float _lastSampleTime;
    float _frameStartRealtime;
    float _sumLogicMs;
    float _sumRenderMs;
    float _smoothFps;
    float _smoothLogicMs;
    float _smoothRenderMs;
    int _displayedFps = -1;
    int _displayedLogicMs = -1;
    int _displayedRenderMs = -1;
    bool _displayedRenderAvailable;
    string _fpsFormat;

    public int DisplayFps => _displayedFps;
    public int DisplayLogicMs => _displayedLogicMs;
    public int DisplayRenderMs => _displayedRenderMs;
    public bool DisplayRenderAvailable => _displayedRenderAvailable;
    public string DisplayFormat => _fpsFormat ?? "--";

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        _lastSampleTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
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
        if (Time.unscaledDeltaTime <= 0f)
            return;

        if (Time.timeScale <= 0f)
            return;

        AccumulateFrameTimings();

        float elapsed = Time.realtimeSinceStartup - _lastSampleTime;
        if (elapsed < _updateInterval || _frames <= 0 || _timingFrames <= 0)
            return;

        float measuredFps = _frames / elapsed;
        _smoothFps = _smoothFps <= 0f
            ? measuredFps
            : Mathf.Lerp(_smoothFps, measuredFps, _smoothFactor);

        float measuredLogic = _sumLogicMs / _timingFrames;
        _smoothLogicMs = _smoothLogicMs <= 0f
            ? measuredLogic
            : Mathf.Lerp(_smoothLogicMs, measuredLogic, _smoothFactor);

        bool renderAvailable = _validRenderFrames > 0;
        if (renderAvailable)
        {
            float measuredRender = _sumRenderMs / _validRenderFrames;
            _smoothRenderMs = _smoothRenderMs <= 0f
                ? measuredRender
                : Mathf.Lerp(_smoothRenderMs, measuredRender, _smoothFactor);
        }

        int roundedFps = Mathf.RoundToInt(_smoothFps);
        int roundedLogic = Mathf.RoundToInt(_smoothLogicMs);
        int roundedRender = Mathf.RoundToInt(_smoothRenderMs);
        if (roundedFps != _displayedFps
            || roundedLogic != _displayedLogicMs
            || renderAvailable != _displayedRenderAvailable
            || (renderAvailable && roundedRender != _displayedRenderMs))
        {
            _displayedFps = roundedFps;
            _displayedLogicMs = roundedLogic;
            _displayedRenderAvailable = renderAvailable;
            _displayedRenderMs = roundedRender;
            _fpsFormat = renderAvailable
                ? string.Format("{0:F0}FPS | \u903b\u8f91 {1}ms | \u6e32\u67d3 {2}ms", _smoothFps, roundedLogic, roundedRender)
                : string.Format("{0:F0}FPS | \u903b\u8f91 {1}ms | \u6e32\u67d3 --", _smoothFps, roundedLogic);
            onFPSChange.Invoke(_fpsFormat);
        }

        ResetSampling();
    }

    void AccumulateFrameTimings()
    {
        float logicMs;
        float renderMs;
        bool renderValid;

        uint count = FrameTimingManager.GetLatestTimings(1, _frameTimings);
        if (count > 0 && _frameTimings[0].gpuFrameTime > 0d)
        {
            renderMs = (float)_frameTimings[0].gpuFrameTime;
            logicMs = Mathf.Max(0f, (float)_frameTimings[0].cpuFrameTime - renderMs);
            renderValid = true;
        }
        else
        {
            logicMs = (Time.realtimeSinceStartup - _frameStartRealtime) * 1000f;
            renderMs = Mathf.Max(0f, Time.unscaledDeltaTime * 1000f - logicMs);
            renderValid = renderMs > 0f;
        }

        _sumLogicMs += logicMs;
        if (renderValid)
        {
            _sumRenderMs += renderMs;
            ++_validRenderFrames;
        }

        ++_timingFrames;
    }

    void ResetSampling()
    {
        _frames = 0;
        _timingFrames = 0;
        _validRenderFrames = 0;
        _sumLogicMs = 0f;
        _sumRenderMs = 0f;
        _lastSampleTime = Time.realtimeSinceStartup;
    }
}
