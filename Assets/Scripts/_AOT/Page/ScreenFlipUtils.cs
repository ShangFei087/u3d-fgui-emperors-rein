using FairyGUI;
using UnityEngine;

/// <summary>
/// 机台画面 180 度翻转。绕 GRoot 左上角旋转后再平移，保持 pivot=0 以免破坏 UI 缩放。
/// 放在 SelfAOT，启动页即可生效。
/// </summary>
public static class ScreenFlipUtils
{
    const string PrefKey = "PARAM_IS_FLIP_SCREEN";
    static bool _listeningResize;

#if UNITY_2019_3_OR_NEWER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitializeOnLoad()
    {
        _listeningResize = false;
    }
#endif

    public static bool IsFlipped
    {
        get { return PlayerPrefs.GetInt(PrefKey, 0) != 0; }
        set
        {
            PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static void Toggle()
    {
        IsFlipped = !IsFlipped;
        Apply();
        Debug.Log($"[ScreenFlip] flipped={IsFlipped}");
    }

    public static void Apply()
    {
        ListenResize();

        GRoot root = GRoot.inst;
        if (IsFlipped)
        {
            root.rotation = 180f;
            root.SetXY(Stage.inst.width, Stage.inst.height);
        }
        else
        {
            root.rotation = 0f;
            root.SetXY(0f, 0f);
        }
    }

    static void ListenResize()
    {
        if (_listeningResize)
            return;

        _listeningResize = true;
        Stage.inst.onStageResized.Add(OnStageResized);
    }

    static void OnStageResized()
    {
        // onStageResized 在 ApplyContentScaleFactor 之前派发，下一拍再贴合新尺寸
        Timers.inst.CallLater(OnApplyLater);
    }

    static void OnApplyLater(object param)
    {
        Apply();
    }
}
