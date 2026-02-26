using GameMaker;
using System.Collections;
using UnityEngine;


/// <summary>
/// 单个声音的控制类
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PooledObject))]
public class GSSource : MonoBehaviour
{
    private AudioSource _source;
    private AudioSource source
    {
        get
        {
            if (_source == null)
                _source = GetComponent<AudioSource>();

            return _source;
        }
    }

    private bool isWaitingForCompletion = false;
    void Update()
    {
        if (isWaitingForCompletion && !source.isPlaying)
        {
            isWaitingForCompletion = false;
            OnAudioFinished();
        }
    }

    void OnAudioFinished()
    {
        DebugUtils.Log("音频播放结束");
        // 执行后续逻辑，如播放下一个音频、触发事件等
    }


    /// <summary>
    /// 控制改对象的GSHandler
    /// </summary>
    public GSHandler Handler { get; set; }

    /// <summary> 资源路劲 </summary>
    public string assetPath;

    public bool IsPlaying =>source.isPlaying;
        

    public bool IsFading => corFadeInOut != null;

    /// <summary> 静音 </summary>
    public bool Mute
    {
        get => source.mute;
        set =>source.mute = value;
    }

    /// <summary> 当前音量 </summary>
    public float curVolume
    {
        get => source.volume;
        set => source.volume = value;
    }

    /// <summary> 目标音量 </summary>
    //float targetVolume;

    float selfVolume = 0.7f;


    public GSOutType outputType = GSOutType.SoundEffect;

    public float GetTargetVolume()
    {
        switch (outputType)
        {
            case GSOutType.SoundEffect:
                return 0.7f * GSManager.Instance.TotalVolumeEff + 0.3f * GSManager.Instance.TotalVolumeEff * selfVolume;  
            case GSOutType.Music:
                return 0.7f * GSManager.Instance.TotalVolumeMusic + 0.3f * GSManager.Instance.TotalVolumeMusic * selfVolume;
            default:
                return 0f;
        }
    }


    bool isAutoRelease = true;

    /*
    public float Volume
    {
        get => source.volume;
        set =>source.volume = value;
    }
    */



    public void Initialize( AudioClip clip,GSHandler gsh)
    {
        this.Handler = gsh;
        Initialize( clip, gsh.assetPath, gsh.volume, gsh.outputType, gsh.loop);
    }

     public void Initialize( AudioClip clip, string assetPath,float volume, GSOutType outputType, bool loop)
    {
        //source.clip = Handler.clip.Load(); //资源包里加载clip

        this.assetPath = assetPath;
        this.selfVolume = volume;
        this.outputType = outputType;


        source.clip = clip;
        source.volume = GetTargetVolume();
        source.loop = loop;

        // 【控制音频播放的音高（音调高低）】
        //0.5：降低 50 % 音高（低沉效果）。
        //1.0：原始音高（默认值）。
        //2.0：提高 100 % 音高（尖锐效果）。
        source.pitch = 1.0f;
        //【位置感和立体声效果】控制音频的空间化程度，决定音频是 2D 还是 3D 播放。
        //0.0：完全 2D 音频（不考虑空间位置，左右声道平衡固定）。
        //1.0：完全 3D 音频（根据 listener 和音频源的位置计算音量和立体声效果）。
        //中间值（如0.5）：混合 2D 和 3D 效果。
        source.spatialBlend = 0.0f;   
    }


    public void ResetVolume()
    {
        if (IsFading && !isFadeIn) //淡出直接返回
        {
            return;
        }
        if (IsFading) // 关闭淡入
        {
            StopCoroutine(corFadeInOut);
            corFadeInOut = null;
        }
        source.volume = GetTargetVolume();
    }


    /// <summary> 暂停 </summary>
    public void Pause()
    {
        source.Pause();
    }


    /// <summary> 取消暂停  </summary>
    public void UnPause()
    {
        source.UnPause();
    }



    public void Play()
    {
        if(Handler != null)
        {
            Play(Handler.delayS,
                Handler.fadeIn != null?  Handler.fadeIn.time : 0,
                Handler.fadeIn != null ? Handler.fadeIn.easeType: GSEaseType.None);
        }
        else
        {
            Play(-1, 0, GSEaseType.None);
        }
    }



    public void Play(float delay, float fadeInTimes, GSEaseType easeType)
    {
        if (delay > 0f)
            source.PlayDelayed(delay);
        else
            source.Play();

        if (corFadeInOut != null)
        {
            StopCoroutine(corFadeInOut);
            corFadeInOut = null;
        }

        if (fadeInTimes > 0f)  // 渐变
        {
            corFadeInOut = FadeInCo(fadeInTimes, easeType);
            StartCoroutine(corFadeInOut);
        }
        else
        {
            curVolume = GetTargetVolume();
        }
    }


    public void Stop()
    {
        if(Handler != null)
        {
            Stop(
            Handler.fadeOut != null ? Handler.fadeOut.time : 0,
            Handler.fadeOut != null ? Handler.fadeOut.easeType : GSEaseType.None);
        }
        else
        {
            Stop(0, GSEaseType.None);
        }
    }

    public void Stop(float fadeOutTimes, GSEaseType easeType)
    {
        if (corFadeInOut != null)
        {
            StopCoroutine(corFadeInOut);
            corFadeInOut = null;
        }

        if (fadeOutTimes > 0f)
        {
            corFadeInOut = FadeOutCo(fadeOutTimes, easeType);
            StartCoroutine(corFadeInOut);
        }
        else
        {
            source.Stop();
        }
    }


    /// <summary>停止播放，解除和GSHandler的绑定，丢回预设池</summary>
    public void Clear()
    {
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (source != null && source.isPlaying == true) { }
        source.Stop();

        if (corFadeInOut != null)
            StopCoroutine(corFadeInOut);
        corFadeInOut = null;

        /*
        Handler.onVolumeChanged -= OnVolumeChanged;
        Handler.onMute -= OnMute;
        Handler.onStop -= Stop;
        Handler.onPause -= Pause;
        Handler.onUnPause -= UnPause;
        Handler.onClear -= Clear;

        Handler.clip.UnLoad();
        Handler.OnDestroySource(this);
        Handler = null;
        */

        Handler = null;

        if (source != null)
            source.clip = null;

    

        GetComponent<PooledObject>().ReturnToPool();
    }









    bool isFadeIn = false;
    private IEnumerator corFadeInOut;

    /// <summary>
    /// 声音变大
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    private IEnumerator FadeInCo(float fadeInTimes, GSEaseType easeType)
    {
        isFadeIn = true;
        curVolume = 0f;
        AnimationCurve curve = null;
        switch (easeType)
        {
            case GSEaseType.Linear:
                {
                    curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                }
                break;
            case GSEaseType.EaseInQuad:
                {
                    curve = EaseInQuad();
                }
                break;
        }
        float startVolume = curVolume;
        float volumeRange = GetTargetVolume() - startVolume;
        var startTime = UnityEngine.Time.time;
        var animationTime = fadeInTimes;

        while (true)
        {
            float deltaTime = UnityEngine.Time.time - startTime;
            curVolume = curve.Evaluate(deltaTime / animationTime) * volumeRange + startVolume;
            
            if (deltaTime >= animationTime)
                break;

            yield return null;
        }
        corFadeInOut = null;
    }


    /// <summary>
    /// 声音变小
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    private IEnumerator FadeOutCo(float fadeOutTimes, GSEaseType easeType)
    {
        isFadeIn = false;

        // AnimationCurve curve = GSManager.Instance.GetEaseCurve(Handler.fadeOut.easeType);
        AnimationCurve curve = null;

        switch (easeType)
        {
            case GSEaseType.Linear:
                {
                    curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                }
                break;
            case GSEaseType.EaseInQuad:
                {
                    curve = EaseInQuad();

                    //[缓入缓出变化]开始和结束时变化缓慢，中间变化最快
                    //curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                }
                break;
        }

        float startVolume = curVolume;
        float volumeRange = startVolume;
        var startTime = UnityEngine.Time.time;
        var animationTime = fadeOutTimes;

        while (true)
        {
            float deltaTime = UnityEngine.Time.time - startTime;
            curVolume = startVolume - (curve.Evaluate(deltaTime / animationTime) * volumeRange);
            if (deltaTime >= animationTime)
                break;

            yield return null;
        }

        source.Stop();
        corFadeInOut = null;
    }


    AnimationCurve EaseInQuad()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f), // 起点：切线为0（水平）
            new Keyframe(1f, 1f, 2f, 0f)  // 终点：切线为2（加速）
        );
    }



    private void LateUpdate()
    {
        if (isAutoRelease && !source.isPlaying)
        {
            ReturnToPool();
        }
    }
}
