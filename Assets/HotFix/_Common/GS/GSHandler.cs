using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 声音配置类
/// </summary>
/// <remarks>
/// * 同个 GSHandler 可以播放多个 GSSource<br/>
/// * 提供声音配置<br/>
/// * 提供声音状态监听<br/>
/// </remarks>
[System.Serializable]
public class GSHandler 
{
    /// <summary> 音乐路劲 </summary>
    public string assetPath;

    /// <summary> 音量 </summary>
    public float volume = 0.7f;

    /// <summary> 输出类型 </summary>
    public GSOutType outputType = GSOutType.SoundEffect;

    /// <summary> 循环播放 </summary>
    // [ShowIf("outputType", GSOutType.Music)]   
    // 由于旧包AOT代码没有包含 “Sirenix.OdinInspector.ShowIfAttribute” ， 所以这里不能使用 ShowIf 除非重新打包AOT代码才会更新！
    public bool loop = false;


    /// <summary> 淡入 </summary>
    public GSFadeInOut fadeIn = null;

    /// <summary> 淡出 </summary>
    public GSFadeInOut fadeOut = null;

    public float delayS = -1f;

    public GSPlayingType playingType = GSPlayingType.Independent;

    //[ShowIf("playingType", GSPlayingType.CountLimit)]
    // 由于旧包AOT代码没有包含 “Sirenix.OdinInspector.ShowIfAttribute” ， 所以这里不能使用 ShowIf 除非重新打包AOT代码才会更新！
    //[ConditionalShow("playingType", GSPlayingType.CountLimit)]
    public int countLimit = 0;

    /// <summary> 自动释放 </summary>
    bool isAutoRelease = true;

    //public string bundleName => "";
    //public string assetName => "";


    // 监听音乐暂停 关闭 静音
    // 播放，停止，暂停，静音


    UnityAction onClear;
    UnityAction onStop;
    UnityAction<bool> onMute;
    UnityAction<bool> onPause;
    //UnityAction onUnPause;

    public int UseCount { get; protected set; }

    /*
    public int UseCount { get; protected set; }
    public GSSource GetSource()
    {
        GSSource source = GSManager.Instance.GetSource();
        source.Initialize(this);
        ++UseCount;
        return source;
    }
    */


}

public enum GSOutType
{
    /// <summary> 背景音乐 </summary>
    Music,
    /// <summary> 音效 </summary>
    SoundEffect,
};



public enum GSPlayingType
{
    /// <summary> 单独播放 </summary>
    Independent,
    /// <summary> 只能是首次播放（单列） </summary>
    FirstOnly,
    /// <summary> 最后一次播放且停止 </summary>
    LastOnly,
    /// <summary> 最多同时播放 countLimit 个声音</summary>
    CountLimit
};

public enum GSEaseType
{
    None,
    /// <summary> 线性变化 </summary>
    Linear,
    /// <summary> 初始变化缓慢，然后逐渐加速 </summary>
    EaseInQuad
};

[System.Serializable]
public class GSFadeInOut
{
    //[HorizontalGroup]
    //[HideLabel]
    public GSEaseType easeType;

    //[HorizontalGroup]
    //[HideLabel]
    //[HideIf("easeType", GSEaseType.None)]
    public float time;
}