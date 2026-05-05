using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace CaiFuHuoChe_3996
{
    public static class Game3996AudioEvent
    {
        /// <summary> 免费结算弹窗 COLLECT 按钮就绪 </summary>
        public const string FreeSpinCollectButtonShown = "FreeSpinCollectButtonShown";
        /// <summary> 免费局充能条：value int，0=arrow1 未满增量，1=arrow2 积满，2=gear </summary>
        public const string FreeSpinMeterSound = "FreeSpinMeterSound";
        /// <summary> Wild 倍率飞向充能条（发射光） </summary>
        public const string FreeSpinWildChargeFly = "FreeSpinWildChargeFly";

        /// <summary> Bonus 小游戏结算遍历中，挂上 SymbolHit 特效瞬间触发（见 <see cref="SlotMachineController3996.JackpotWinCredit"/>）；与 <see cref="SoundKey.BonusSymbolAppear"/> 对应。 </summary>
        public const string BonusSymbolAppear = "BonusSymbolAppear";
        /// <summary> Bonus 小游戏内 Bonus 图标收集结算。 </summary>
        public const string BonusSymbolCollect = "BonusSymbolCollect";
        /// <summary> Bonus 小游戏内转轴滚动（亦可由业务手动触发）。 </summary>
        public const string BonusRolling = "BonusRolling";

        /// <summary> 主游戏循环 BGM </summary>
        public const string BgmRegularGame = "3996_BgmRegularGame";
        /// <summary> 免费局内循环 BGM </summary>
        public const string BgmFreeSpinGame = "3996_BgmFreeSpinGame";
        /// <summary> 彩金小游戏局内循环 BGM </summary>
        public const string BgmBonusGame = "3996_BgmBonusGame";
        /// <summary> 免费触发弹窗 BGM </summary>
        public const string BgmFreeSpinTrigger = "3996_BgmFreeSpinTrigger";
        /// <summary> 免费结算弹窗 BGM </summary>
        public const string BgmFreeSpinResult = "3996_BgmFreeSpinResult";
        /// <summary> 彩金触发弹窗 BGM </summary>
        public const string BgmBonusTrigger = "3996_BgmBonusTrigger";
        /// <summary> 彩金结算弹窗 BGM </summary>
        public const string BgmBonusResult = "3996_BgmBonusResult";
    }


    public class GameSoundController3996
    {
        /// <summary> Scatter 图标 ID（与 MachineData 一致，symbolNumber[10]）。 </summary>
        private static int ScatterSymbolId => CustomModel.Instance.symbolNumber[10];

        /// <summary> Bonus 图标 ID（symbolNumber[11]）。 </summary>
        private static int BonusSymbolId => CustomModel.Instance.symbolNumber[11];

        /// <summary> 本局是否已播过 Scatter 线赢音效（防重复）。 </summary>
        private bool _scatterWinPlayedThisSpin;

        /// <summary> 本局是否已播过 Bonus 线赢音效（防重复）。 </summary>
        private bool _bonusWinPlayedThisSpin;

        /// <summary> 注册四类机台相关事件监听。 </summary>
        public  GameSoundController3996()
        {
           Init();
        }

        public void Init()
        {
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnWinEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, OnAudioEvent);
        }

        /// <summary> 注销监听，避免泄漏与重复回调。 </summary>
        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnWinEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, OnAudioEvent);
        }

        /// <summary> 按已算好的标志播 ScatterDown；Bonus 列停统一播 BonusDown1（不按列区分）。 </summary>
        private void PlayScatterBonusColumnStopSoundsFromFlags(int column0Based, bool hasScatter, bool hasBonus)
        {
            if (column0Based < 0 || column0Based >= CustomModel.Instance.column)
                return;

            if (hasScatter)
                GameSoundHelper.Instance.PlaySoundEff(SoundKey.ScatterDown);

            if (hasBonus)
                GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusDown1);
        }

        /// <summary> 滚轮开始/整列停稳：重置本局 Scatter·Bonus 线赢去重标记；开始转时播滚轮循环音（彩金局与普通局区分）。 </summary>
        private void OnSlotEvent(EventData receivedEvent)
        {
            switch ((string)receivedEvent.name)
            {
                case SlotMachineEvent.SpinSlotMachine:
                    _scatterWinPlayedThisSpin = false;
                    _bonusWinPlayedThisSpin = false;
                    // 彩金小游戏（JS）用 BonusRolling，基础局用 NormalRolling
                    if (ContentModel.Instance.isJackpotSpin)
                        GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusRolling);
                    else
                        GameSoundHelper.Instance.PlaySoundEff(SoundKey.NormalRolling);
                    break;
                case SlotMachineEvent.StoppedSlotMachine:
                    // 触发免费时整列停完后补播 Scatter 中奖提示（与线赢逻辑配合）
                    if (ContentModel.Instance.isFreeSpinTrigger)
                        TryPlayScatterWinSound();
                    // 触发彩金/Bonus 小游戏时 winList 往往不含 icon11 线奖（BonusBet 不进 IDVec），与 Scatter 同理在停轮后补播
                    if (ContentModel.Instance.isJackpotSpinTrigger)
                        TryPlayBonusWinSound();

                    if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeRollingBox))GameSoundHelper.Instance.StopSound(SoundKey.FreeRollingBox);
                    if (GameSoundHelper.Instance.IsPlaySound(SoundKey.BonusRollingBox))GameSoundHelper.Instance.StopSound(SoundKey.BonusRollingBox);
                    break;
            }
        }

        /// <summary> 单列停稳细节：<see cref="SlotMachineEvent.ReelColumnStopSound"/> 播 ReelStop；<see cref="SlotMachineEvent.ScatterBonusColumnStopSound"/> 播 Scatter/Bonus 列停音。 </summary>
        private void OnSlotDetailEvent(EventData receivedEvent)
        {
            string name = (string)receivedEvent.name;
            if (name == SlotMachineEvent.ReelColumnStopSound)
            {
                int reelIdx = (int)receivedEvent.value;
                int column = CustomModel.Instance.column;
                if (reelIdx < 0 || reelIdx >= column)
                    return;

                int clamped = reelIdx < 5 ? reelIdx : 4;
                var key = (SoundKey)((int)SoundKey.ReelStop1 + clamped);
                GameSoundHelper.Instance.PlaySoundEff(key);
                return;
            }

            if (name == SlotMachineEvent.ScatterBonusColumnStopSound &&
                receivedEvent is EventData<ScatterBonusColumnStopPayload> payload)
            {
                var p = payload.value;
                PlayScatterBonusColumnStopSoundsFromFlags(p.column0Based, p.hasScatter, p.hasBonus);
            }
        }

        /// <summary> 处理 <see cref="SlotMachineEvent.ON_AUDIO_EVENT"/>：免费/彩金过场、弹窗、充能、Bonus 小游戏图标特效、JACKPOT/Bonus 弹窗、期望加速框等纯音效。 </summary>
        private void OnAudioEvent(EventData receivedEvent)
        {
            switch ((string)receivedEvent.name)
            {
                case SlotMachineEvent.FreeGameFadeTransition:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FadeFree);
                    break;
                case SlotMachineEvent.BonusGameFadeTransition:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FadeBonus);
                    break;
                case SlotMachineEvent.FreeSpinPopupAppear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreePopupAppear);
                    break;
                case SlotMachineEvent.FreeSpinPopupDisappear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreePopupDisappear);
                    break;
                case SlotMachineEvent.FreeSpinStartButtonShown:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreeStartBtn);
                    break;
                case Game3996AudioEvent.FreeSpinCollectButtonShown:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreeCollectBtn);
                    break;
                case Game3996AudioEvent.FreeSpinWildChargeFly:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.WildTail);
                    break;
                case Game3996AudioEvent.FreeSpinMeterSound:
                    {
                        // 0 未满增量 / 1 积满 / 2 齿轮
                        int code = System.Convert.ToInt32(receivedEvent.value);
                        if (code == 0)
                            GameSoundHelper.Instance.PlaySoundEff(SoundKey.arrow1);
                        else if (code == 1)
                            GameSoundHelper.Instance.PlaySoundEff(SoundKey.arrow2);
                        else if (code == 2)
                            GameSoundHelper.Instance.PlaySoundEff(SoundKey.arrow2);
                    }
                    break;
                case Game3996AudioEvent.BonusSymbolAppear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusSymbolAppear);
                    break;
                case Game3996AudioEvent.BonusSymbolCollect:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusSymbolCollect);
                    break;
                case Game3996AudioEvent.BonusRolling:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusRolling);
                    break;
                case SlotMachineEvent.JackpotPopupAppear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.JackpotPopupAppear);
                    break;
                case SlotMachineEvent.JackpotPopupDisappear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.JackpotPopupDisappear);
                    break;
                case SlotMachineEvent.BonusPopupAppear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusPopupAppear);
                    break;
                case SlotMachineEvent.BonusStartBtn:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusStartBtn);
                    break;
                case SlotMachineEvent.BonusPopupDisappear:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusPopupDisappear);
                    break;
                case SlotMachineEvent.BonusCollectBtn:
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusCollectBtn);
                    break;
                case SlotMachineEvent.FreeRollingBox:
                    // 同轨叠播前先停，避免拖音
                    if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeRollingBox))
                        GameSoundHelper.Instance.StopSound(SoundKey.FreeRollingBox);
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreeRollingBox);
                    break;
                case SlotMachineEvent.BonusRollingBox:
                    if (GameSoundHelper.Instance.IsPlaySound(SoundKey.BonusRollingBox))
                        GameSoundHelper.Instance.StopSound(SoundKey.BonusRollingBox);
                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusRollingBox);
                    break;
                case Game3996AudioEvent.BgmRegularGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
                    break;
                case Game3996AudioEvent.BgmFreeSpinGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
                    break;
                case Game3996AudioEvent.BgmBonusGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.BonusBG);
                    break;
                case Game3996AudioEvent.BgmFreeSpinTrigger:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinTriggerBG);
                    break;
                case Game3996AudioEvent.BgmFreeSpinResult:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinResultBG);
                    break;
                case Game3996AudioEvent.BgmBonusTrigger:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.BonusTriggerBG);
                    break;
                case Game3996AudioEvent.BgmBonusResult:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.BonusResultBG);
                    break;
            }
        }

        /// <summary> 总赢分、全线、单线：驱动档位音效与 Scatter/Bonus 线赢音效。 </summary>
        private void OnWinEvent(EventData receivedEvent)
        {
            string name = (string)receivedEvent.name;
            switch (name)
            {
                case SlotMachineEvent.TotalWinCredit:
                    OnTotalWinCreditForWinLevel(receivedEvent);
                    break;
                case SlotMachineEvent.TotalWinLine:
                    PlayScatterBonusWinFromWinList(ContentModel.Instance.winList);
                    break;
                case SlotMachineEvent.SingleWinLine:
                    if (receivedEvent.value is SymbolWin sw)
                        PlayScatterBonusWinFromSingle(sw);
                    break;
            }
        }

        /// <summary>
        /// 总赢分滚分展示时：按相对总押档位播 win_lv1/2/3（押注无效时退化为低档）。
        /// </summary>
        private void OnTotalWinCreditForWinLevel(EventData receivedEvent)
        {
            long winCredit = System.Convert.ToInt64(receivedEvent.value);

            // 免费局里 PageGameMain 发的是 curFreeCredit（整段免费累计
            if (ContentModel.Instance.isFreeSpin) winCredit = ContentModel.Instance.baseGameWinCredit;
            if (ContentModel.Instance.isJackpotSpin) return;
            if (winCredit <= 0) return;

            long totalBet = ContentModel.Instance.totalBet;
            if (totalBet <= 0)
            {
                GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv1);
                return;
            }

            // ≥3 倍押注高档，≥2 倍中档，否则低档
            if (winCredit >= 3L * totalBet)
                GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv3);
            else if (winCredit >= 2L * totalBet)
                GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv2);
            else
                GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv1);
        }

        /// <summary> 遍历本局线赢列表，对含 Scatter/Bonus 的线尝试播线赢音。 </summary>
        private void PlayScatterBonusWinFromWinList(List<SymbolWin> winList)
        {
            if (winList == null)
                return;
            foreach (var sw in winList)
                PlayScatterBonusWinFromSingle(sw);
        }

        /// <summary> 单条 SymbolWin：若为 Scatter/Bonus 图标且赢分&gt;0，则播对应线赢（本局各播一次）。 </summary>
        private void PlayScatterBonusWinFromSingle(SymbolWin sw)
        {
            if (sw == null || sw.earnCredit <= 0)
                return;
            if (sw.symbolNumber == ScatterSymbolId)
                TryPlayScatterWinSound();
            if (sw.symbolNumber == BonusSymbolId)
                TryPlayBonusWinSound();
        }

        /// <summary> Scatter 线赢音：同一 spin 内只播一次。 </summary>
        private void TryPlayScatterWinSound()
        {
            if (_scatterWinPlayedThisSpin)
                return;
            _scatterWinPlayedThisSpin = true;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.ScatterWin);
        }

        /// <summary> Bonus 线赢音：同一 spin 内只播一次。 </summary>
        private void TryPlayBonusWinSound()
        {
            if (_bonusWinPlayedThisSpin)
                return;
            _bonusWinPlayedThisSpin = true;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusWin);
        }
    }
}
