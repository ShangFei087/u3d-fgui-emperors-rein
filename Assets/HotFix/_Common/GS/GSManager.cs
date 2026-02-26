using GameMaker;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 声音管理类
/// </summary>
public class GSManager : MonoSingleton<GSManager>
{
    public ObjectPool pool; // 每个pool的子对象有GSSource组件


    const string MARK_BUNDLE_SOUND_MANAGER = "MARK_BUNDLE_SOUND_MANAGER";


    private Dictionary<string, AudioClip> audioClipResDic = new Dictionary<string, AudioClip>();



    private void Start()
    {
        DebugUtils.LogWarning($"【GS】 IsMute:{IsMute}  TotalVolumeEff:{TotalVolumeEff}  TotalVolumeMusic:{TotalVolumeMusic}  pool childCount:{pool.transform.childCount}");
        SetMute(IsMute);
        SetTotalVolumEfft(TotalVolumeEff);
        SetTotalVolumMusic(TotalVolumeMusic);
    }


    public float TotalVolumeMusic
    {
        get => totalVolumeMusic;
        //set => SetTotalVolumMusic(value);
    }

    float totalVolumeMusic
    {
        get
        {
            if(_totalVolumeMusic == null)
            {
                _totalVolumeMusic = PlayerPrefs.GetFloat(TOTAL_VOLUME_MUSIC, 1f);
            }
            return (float)_totalVolumeMusic;
        }
        set
        {
            _totalVolumeMusic = value;
            PlayerPrefs.SetFloat(TOTAL_VOLUME_MUSIC, value);
            PlayerPrefs.Save();
        }
    }
    private float? _totalVolumeMusic = null; //背景音乐声音大小比例
    const string TOTAL_VOLUME_MUSIC = "TOTAL_VOLUME_MUSIC";



    public float TotalVolumeEff
    {
        get => totalVolumeEff;
        //set => SetTotalVolumEfft(value);
    }

    float totalVolumeEff
    {
        get
        {
            if (_totalVolumeEff == null)
            {
                _totalVolumeEff = PlayerPrefs.GetFloat(TOTAL_VOLUME_EFF, 1f);
            }
            return (float)_totalVolumeEff;
        }
        set
        {
            _totalVolumeEff = value;
            PlayerPrefs.SetFloat(TOTAL_VOLUME_EFF, value);
            PlayerPrefs.Save();
        }
    }
    private float? _totalVolumeEff = null; //音效声音大小比例
    const string TOTAL_VOLUME_EFF = "TOTAL_VOLUME_EFF";



    public bool IsMute
    {
        get => isMute;
        //set => SetMute(value);
    }
    bool isMute
    {
        get
        {
            if (_isMute == null)
            {
                _isMute = PlayerPrefs.GetInt(IS_MUTE, 0) == 1;
            }
            return (bool)_isMute;
        }
        set
        {
            _isMute = value;
            PlayerPrefs.SetInt(IS_MUTE, value?1:0);
            PlayerPrefs.Save();
        }
    }
    private bool? _isMute = null; 
    const string IS_MUTE = "IS_MUTE";


    #region 背景音乐、特效音乐、静音
    //设置静音
    public void SetMute(bool isMute)
    {
        //DebugUtils.LogError($"【GS】:  isMute:{isMute}");
        this.isMute = isMute;
        foreach (Transform tfm in pool.transform)
        {
            tfm.GetComponent<GSSource>().Mute = isMute;
        }
    }
    public void SetTotalVolumMusic(float volum)
    {
        //DebugUtils.LogError($"【GS】:  SetTotalVolumMusic:{volum}");
        totalVolumeMusic = volum;
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.outputType == GSOutType.Music)
                    gss.ResetVolume();
            }
        }
    }

    public void SetTotalVolumEfft(float volum)
    {

        //DebugUtils.LogError($"【GS】:  SetTotalVolumEfft:{volum}");
        totalVolumeEff = volum;
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.outputType == GSOutType.SoundEffect)
                    gss.ResetVolume();
            }
        }
    }

    #endregion




    //解暂停
    public void UnPause()
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                tfm.GetComponent<GSSource>().UnPause();
            }
        }
    }
    //暂停所有声音
    public void Pause()
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                tfm.GetComponent<GSSource>().Pause();
            }
        }
    }
    //停止所有声音
    public void Stop()
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                tfm.GetComponent<GSSource>().Stop();
            }
        }
    }



    public void StopMusic()
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.outputType == GSOutType.Music)
                {
                    gss.Stop();
                }
            }
        }
    }
    public void StopSoundEff()
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.outputType == GSOutType.SoundEffect)
                {
                    gss.Stop();
                }
            }
        }
    }
    public void StopSound(string assetPath)
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.assetPath == assetPath)
                {
                    gss.Stop();
                }
            }
        }
    }


    public bool IsPlaySound(string assetPath)
    {
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active )
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.assetPath == assetPath)
                    return true;
            }
        }
        return false;
    }

    public int GetCount(string assetPath)
    {
        int count = 0;
        foreach (Transform tfm in pool.transform)
        {
            if (tfm.gameObject.active)
            {
                GSSource gss = tfm.GetComponent<GSSource>();
                if (gss.assetPath == assetPath)
                {
                    count++;
                }
            }
        }
        return count;   
    }



    public void PlayMusic(string assetPath, float volume, bool loop = true)
    {
        if (!audioClipResDic.ContainsKey(assetPath))
        {
           ResourceManager02.Instance.LoadAsset<AudioClip>(assetPath, 
               (clip)=>
               {
                   audioClipResDic[assetPath] = clip;
                   
                   GSSource gss = GetSource();
                   gss.Initialize( audioClipResDic[assetPath], assetPath, volume, GSOutType.Music, loop);
                   gss.Play();   
               },MARK_BUNDLE_SOUND_MANAGER);     
        }
        else
        {
             GSSource gss = GetSource();
             gss.Initialize( audioClipResDic[assetPath], assetPath, volume, GSOutType.Music, loop);
             gss.Play();   
        }

    }



    
    
    
    public void PlayMusicSingle(string assetPath, float volume, bool loop = false)
    {
        StopMusic();
        PlayMusic(assetPath, volume, loop);
    }


    public void PlaySoundEffSingle(string assetPath, float volume, bool loop = false)
    {
        StopSound(assetPath);
        PlaySoundEff(assetPath, volume, loop);
    }

    public void PlaySoundEff(string assetPath, float volume, bool loop = false)
    {

        if (!audioClipResDic.ContainsKey(assetPath))
        {
            ResourceManager02.Instance.LoadAsset<AudioClip>(assetPath, 
                (clip)=>
                {
                    audioClipResDic[assetPath] = clip;
                    
                    GSSource gss = GetSource();
                    gss.Initialize( audioClipResDic[assetPath], assetPath,volume,GSOutType.SoundEffect, loop);
                    gss.Play();
                },MARK_BUNDLE_SOUND_MANAGER);     
        }
        else
        {
            GSSource gss = GetSource();
            gss.Initialize( audioClipResDic[assetPath], assetPath,volume,GSOutType.SoundEffect, loop);
            gss.Play();
        }
    }
    
    public GSSource GetSource()
    {
        GSSource gss = pool.GetObject().GetComponent<GSSource>();
        gss.transform.SetParent(pool.transform, false);
        //gss.startUseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return gss;
    }


    
    

    
    #region GSHandler
    
    public void PlaySound(GSHandler gsh)
    {
        switch (gsh.playingType)
        {
            case GSPlayingType.Independent:
                break;
            case GSPlayingType.FirstOnly:
                {
                    if(IsPlaySound(gsh.assetPath))
                        return;
                }
                break;
            case GSPlayingType.LastOnly:
                {
                    StopSound(gsh.assetPath);
                }
                break;
            case GSPlayingType.CountLimit:
                {
                    if (GetCount(gsh.assetPath) >= gsh.countLimit)
                        return;
                }
                break;
        }

        if (!audioClipResDic.ContainsKey(gsh.assetPath))
        {
            ResourceManager02.Instance.LoadAsset<AudioClip>(gsh.assetPath, 
                (clip)=>
                {
                    audioClipResDic[gsh.assetPath] = clip;
                    
                    GSSource source = GetSource(); //绑定自己和GSSource
                    source.Initialize(audioClipResDic[gsh.assetPath],gsh);
                    source.Play();
                },MARK_BUNDLE_SOUND_MANAGER);     
        }
        else
        {
            GSSource source = GetSource(); //绑定自己和GSSource
            source.Initialize(audioClipResDic[gsh.assetPath],gsh);
            source.Play();
        }
    }
    
    public void PlaySoundSingle(GSHandler gsh)
    {
        StopSound(gsh.assetPath);
        PlaySound(gsh);
    }



    public void PlaySoundEff(GSHandler gsh)
    {
        if (gsh.outputType != GSOutType.SoundEffect)
        {
             DebugUtils.LogError($"{gsh.assetPath} 的配置不是音效");
             return;
        }
        PlaySound(gsh);
    }

    public void PlaySoundEffSingle(GSHandler gsh)
    {
        if (gsh.outputType != GSOutType.SoundEffect)
        {
            DebugUtils.LogError($"{gsh.assetPath} 的配置不是音效");
            return;
        }
        PlaySoundSingle(gsh);
    }


    public void PlayMusicSingle(GSHandler gsh)
    {
        if (gsh.outputType != GSOutType.Music)
        {
            DebugUtils.LogError($"{gsh.assetPath} 的配置不是音效");
            return;
        }
        PlaySoundSingle(gsh);
    }
    
    public void PlayMusic(GSHandler gsh)
    {
        if (gsh.outputType != GSOutType.Music)
        {
            DebugUtils.LogError($"{gsh.assetPath} 的配置不是音效");
            return;
        }
        PlaySound(gsh);
    }
    
    #endregion
    



    [Button]
    public void TestPlaySound(float volume = 0.7f)
    {
        GSHandler gsh = new GSHandler()
        {
            assetPath = "Assets/GameRes/Games/PssOn00152 (1080x1920)/Sounds/BaseBackground.mp3",
            fadeIn = new GSFadeInOut()
            {
                easeType = GSEaseType.Linear,
                time = 3f,
            },
            fadeOut = new GSFadeInOut()
            {
                easeType = GSEaseType.Linear,
                time = 3f,
            },
            volume = volume,
        };

        Instance.PlaySoundEff(gsh);
    }

    [Button]
    public void TestStopSound()
    {
        GSHandler gsh = new GSHandler()
        {
            assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/BaseBackground.mp3",
            fadeIn = new GSFadeInOut()
            {
                easeType = GSEaseType.Linear,
                time = 3f,

            },
            fadeOut = new GSFadeInOut()
            {
                easeType = GSEaseType.Linear,
                time = 3f,
            },
            //volume =   
        };

        Instance.StopSound(gsh.assetPath);
    }

    [Button]
    public void SetVolume(float volume = 1f)
    {
        SetTotalVolumEfft(volume);
        SetTotalVolumMusic(volume);
    }
}

