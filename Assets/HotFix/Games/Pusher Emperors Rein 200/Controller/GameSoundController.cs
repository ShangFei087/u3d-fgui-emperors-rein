using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMaker;
using SlotMaker;

namespace PusherEmperorsRein
{
    public class GameSoundController
    {

        public GameSoundController()
        {
            Init();
        }

        void Init()
        {
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChangeEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_CONTENT_EVENT, OnContentEvent);
        }

        public void Dispose()
        {

            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnSlotEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChangeEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_CONTENT_EVENT, OnContentEvent);
        }


        void InitParam()
        {
            // OnPropertyChangeGameState();
        }


        private void OnSlotEvent(EventData receivedEvent)
        {
            switch ((string)receivedEvent.name)
            {
                case SlotMachineEvent.SpinSlotMachine:
                    {

                        GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowBG);

                    }
                    break;
                case SlotMachineEvent.StoppedSlotMachine:
                    {

                        GameSoundHelper.Instance.StopSound(SoundKey.SlowBG);

                    }
                    break;
            }
        }


        private void OnSlotDetailEvent(EventData receivedEvent)
        {
            switch ((string)receivedEvent.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        int reelIdx = (int)receivedEvent.value;
                        switch (reelIdx)
                        {
                            case 0:
                                {
                                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.ReelStop1);
                                }
                                break;
                            case 1:
                                {
                                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.ReelStop2);
                                }
                                break;
                            case 2:
                                {
                                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.ReelStop3);
                                }
                                break;
                            case 3:
                                {
                                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.ReelStop4);
                                }
                                break;
                            case 4:
                                {
                                    GameSoundHelper.Instance.PlaySoundEff(SoundKey.ReelStop5);
                                }
                                break;
                        }

                    }
                    break;
            }
        }
        
        

        private void OnPropertyChangeEvent(EventData receivedEvent)
        {
            switch (receivedEvent.name)
            {
                case "ContentModel/gameState":
                    {
                        OnPropertyChangeEventGameState(receivedEvent);
                    }
                    break;
            }
        }
        
        private void OnPropertyChangeEventGameState(EventData receivedEvent)
        {
            string gameState = receivedEvent != null? (string)receivedEvent.value : null;

            //if(gameState = )
        }

        
        
        private void OnContentEvent(EventData receivedEvent)
        {
            switch (receivedEvent.name)
            {
                case SlotMachineEvent.BeginBonus:
                    {
                        OnBeginBonus(receivedEvent);
                    }
                    break;
                case SlotMachineEvent.EndBonus:
                    {
                        OnEndBonus(receivedEvent);
                    }
                    break;
            }
        }
        private void OnBeginBonus(EventData receivedEvent = null)
        {
            string val = (string)receivedEvent.value;
            if (val == "FreeSpin")
            {
                if (GameSoundHelper.Instance.IsPlaySound(SoundKey.RegularBG))
                    GameSoundHelper.Instance.StopSound(SoundKey.RegularBG);
            }
        }

        private void OnEndBonus(EventData receivedEvent = null)
        {
            string val = (string)receivedEvent.value;
            if (val == "FreeSpin")
            {
                // 停止免费游戏音乐
                if (GameSoundHelper.Instance.IsPlaySound(SoundKey.FreeSpinBG))
                    GameSoundHelper.Instance.StopSound(SoundKey.FreeSpinBG);

                // 播放正常音乐
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            }
        }


    }


}
