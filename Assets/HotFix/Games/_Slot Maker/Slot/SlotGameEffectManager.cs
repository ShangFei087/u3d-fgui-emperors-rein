using GameMaker;
using _reelSetMD = SlotMaker.ReelSettingModel;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

public enum SlotGameEffect
{
    Default,
    StopImmediately,
    AutoSpin,
    FreeSpin,
    Expectation01,//长滚动
    Expectation02,//Bonus
    GameIdle,
}


/// <summary>
/// 拉霸游戏特效管理
/// </summary>
/// <remarks>
/// * 管理滚轮特效
/// * 管理中奖特效
/// </remarks>
public class SlotGameEffectManager : Singleton<SlotGameEffectManager>
{

    public void SetEffect(SlotGameEffect state)
    {
        SetSpinWinEffect(state);
        SetReelRunEffect(state);

    }


    void SetSpinWinEffect(SlotGameEffect state)
    {
        if (state == SlotGameEffect.GameIdle)
        {
            _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_IDLE);
            return;
        }

        if (MainModel.Instance.contentMD.targetSlotGameEffect == SlotGameEffect.Expectation02)
        {
            _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_BONUS);
            return;
        }        
        
        if (state == SlotGameEffect.StopImmediately)
        {
            _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_STOP_IMMEDIATELY);
            return;
        }

        if (state == SlotGameEffect.FreeSpin)
        {
            _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN);
            return;
        }
        
        if (MainModel.Instance.contentMD.isAuto)
        {
            _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_AUTO);
            return;
        }
        
        _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_DEFAULT);
    }

    void SetReelRunEffect(SlotGameEffect state)
    {
        if (state == SlotGameEffect.StopImmediately)
        {
            _reelSetMD.Instance.SelectData(_reelSetMD.REEL_SETTING_STOP);
            return;
        }

        if (state == SlotGameEffect.Expectation01)
        {
            _reelSetMD.Instance.SelectData(_reelSetMD.REEL_SETTING_EXPECTATION_01);
            return;
        }

        //#seaweed# 新增加
        if (state == SlotGameEffect.AutoSpin)
        {
            _reelSetMD.Instance.SelectData(_reelSetMD.REEL_SETTING_AUTO);
            return;
        }
        _reelSetMD.Instance.SelectData(_reelSetMD.REEL_SETTING_REGULAR);
    }


}
