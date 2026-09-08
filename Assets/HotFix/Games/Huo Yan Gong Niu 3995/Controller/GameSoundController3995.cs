using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace HuoYanGongNiu_3995
{
    public static class Game3995AudioEvent
    {
        /// <summary> 主游戏循环 BGM </summary>
        public const string BgmRegularGame = "3995_BgmRegularGame";
        /// <summary> 免费局内循环 BGM </summary>
        public const string BgmFreeSpinGame = "3995_BgmFreeSpinGame";
        /// <summary> 彩金小游戏局内循环 BGM </summary>
        public const string BgmBonusGame = "3995_BgmBonusGame";
        /// <summary> 免费触发和结算弹窗 BGM </summary>
        public const string BgmFreeSpinTrigger = "3995_BgmFreeSpin";
        /// <summary> 彩金触发和结算弹窗 BGM </summary>
        public const string BgmBonusTrigger = "3996_BgmBonus";
    }


    public class GameSoundController3995
    {
        /// <summary> Scatter 图标 ID（与 MachineData 一致，symbolNumber[12]）。 </summary>
        private static int ScatterSymbolId => CustomModel.Instance.symbolNumber[12];

        /// <summary> Bonus 图标 ID（symbolNumber[13]）。 </summary>
        private static int BonusSymbolId => CustomModel.Instance.symbolNumber[13];

        /// <summary> 本局是否已播过 Scatter 线赢音效（防重复）。 </summary>
        private bool _scatterWinPlayedThisSpin;

        /// <summary> 本局是否已播过 Bonus 线赢音效（防重复）。 </summary>
        private bool _bonusWinPlayedThisSpin;

        /// <summary> 注册四类机台相关事件监听。 </summary>
        public GameSoundController3995()
        {
            Init();
        }

        public void Init()
        {
            //EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            //EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnWinEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, OnAudioEvent);
        }

        /// <summary> 注销监听，避免泄漏与重复回调。 </summary>
        public void Dispose()
        {
            //EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            //EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnWinEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, OnAudioEvent);
        }

        /// <summary> 按已算好的标志播 ScatterDown；Bonus 列停统一播 BonusDown1（不按列区分）。 </summary>
        private void PlayScatterBonusColumnStopSoundsFromFlags(int column0Based, bool hasScatter, bool hasBonus)
        {
            if (column0Based < 0 || column0Based >= CustomModel.Instance.column)
                return;

            //if (hasScatter)
            //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.ScatterDown);

            //if (hasBonus)
            //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusDown1);
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
                    //if (ContentModel.Instance.isJackpotSpin)
                    //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusRolling);
                    //else
                    //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.NormalRolling);
                    break;
                case SlotMachineEvent.StoppedSlotMachine:
                    // 触发免费时整列停完后补播 Scatter 中奖提示（与线赢逻辑配合）
                    if (ContentModel.Instance.isFreeSpinTrigger)
                        TryPlayScatterWinSound();
                    // 触发彩金/Bonus 小游戏时 winList 往往不含 icon11 线奖（BonusBet 不进 IDVec），与 Scatter 同理在停轮后补播
                    if (ContentModel.Instance.isJackpotSpinTrigger)
                        TryPlayBonusWinSound();

                    //if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeRollingBox)) GameSoundHelper.Instance.StopSound(SoundKey.FreeRollingBox);
                    //if (GameSoundHelper.Instance.IsPlaySound(SoundKey.BonusRollingBox)) GameSoundHelper.Instance.StopSound(SoundKey.BonusRollingBox);
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
                case Game3995AudioEvent.BgmRegularGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
                    break;
                case Game3995AudioEvent.BgmFreeSpinGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
                    break;
                case Game3995AudioEvent.BgmBonusGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.JackpotBG);
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
                //GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv1);
                return;
            }

            // ≥3 倍押注高档，≥2 倍中档，否则低档
            //if (winCredit >= 3L * totalBet)
            //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv3);
            //else if (winCredit >= 2L * totalBet)
            //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv2);
            //else
            //    GameSoundHelper.Instance.PlaySoundEff(SoundKey.win_lv1);
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
            //GameSoundHelper.Instance.PlaySoundEff(SoundKey.ScatterWin);
        }

        /// <summary> Bonus 线赢音：同一 spin 内只播一次。 </summary>
        private void TryPlayBonusWinSound()
        {
            if (_bonusWinPlayedThisSpin)
                return;
            _bonusWinPlayedThisSpin = true;
            //GameSoundHelper.Instance.PlaySoundEff(SoundKey.BonusWin);
        }
    }
}
