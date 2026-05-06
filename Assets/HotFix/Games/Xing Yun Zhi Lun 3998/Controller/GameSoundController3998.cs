using CaiFuHuoChe_3996;
using GameMaker;
using SlotMaker;
using System.Collections.Generic;


namespace XingYunZhiLun_3998
{
    public static class Game3998AudioEvent
    {
        /// <summary> 主游戏循环 BGM </summary>
        public const string BgmRegularGame = "3998_BgmRegularGame";
        /// <summary> 免费局内循环 BGM </summary>
        public const string BgmFreeSpinGame = "3998_BgmFreeSpinGame";
        /// <summary> 彩金小游戏局内循环 BGM </summary>
        public const string BgmBonusGame = "3998_BgmBonusGame";
        /// <summary> 免费触发弹窗 BGM </summary>
        public const string BgmFreeSpinTrigger = "3998_BgmFreeSpinTrigger";
        /// <summary> 免费结算弹窗 BGM </summary>
        public const string BgmFreeSpinResult = "3998_BgmFreeSpinResult";
        /// <summary> 彩金触发弹窗 BGM </summary>
        public const string BgmBonusTrigger = "3998_BgmBonusTrigger";
        /// <summary> 彩金结算弹窗 BGM </summary>
        public const string BgmBonusResult = "3998_BgmBonusResult";
    }

    public class GameSoundController3998
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
        public GameSoundController3998()
        {
            Init();
        }

        public void Init()
        {
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnWinEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, OnAudioEvent);
        }

        /// <summary> 注销监听，避免泄漏与重复回调。 </summary>
        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnWinEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, OnAudioEvent);
        }

        /// <summary> 滚轮开始/整列停稳：重置本局 Scatter·Bonus 线赢去重标记；开始转时播滚轮循环音（彩金局与普通局区分）。 </summary>
        private void OnSlotEvent(EventData receivedEvent)
        {
            switch ((string)receivedEvent.name)
            {
                
            }
        }

        /// <summary> 单列停稳细节：<see cref="SlotMachineEvent.ReelColumnStopSound"/> 播 ReelStop；<see cref="SlotMachineEvent.ScatterBonusColumnStopSound"/> 播 Scatter/Bonus 列停音。 </summary>
        private void OnSlotDetailEvent(EventData receivedEvent)
        {
            
        }

        /// <summary> 处理 <see cref="SlotMachineEvent.ON_AUDIO_EVENT"/>：免费/彩金过场、弹窗、充能、Bonus 小游戏图标特效、JACKPOT/Bonus 弹窗、期望加速框等纯音效。 </summary>
        private void OnAudioEvent(EventData receivedEvent)
        {
            switch ((string)receivedEvent.name)
            {
                case Game3998AudioEvent.BgmRegularGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
                    break;
                case Game3998AudioEvent.BgmFreeSpinGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
                    break;
                case Game3998AudioEvent.BgmBonusGame:
                    GameSoundHelper.Instance.PlayMusicSingle(SoundKey.JackpotBG);
                    break;
            }
        }
    }
}
