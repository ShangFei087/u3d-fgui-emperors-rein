using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameMaker
{
    /*public class GlobalSoundHelper : Singleton<GlobalSoundHelper>
    {
        SoundHelper m_SoundHelper;
        SoundHelper soundHelper
        {
            get
            {
                if (m_SoundHelper == null)
                    m_SoundHelper = new SoundHelper();

                return m_SoundHelper;
            }
        }

        public void PlaySoundEff(object enumObj) => soundHelper?.PlaySoundEff(enumObj);

        public void PlaySoundEffSingle(object enumObj) => soundHelper?.PlaySoundEffSingle(enumObj);
        
        public void PlayMusicSingle(object enumObj) => soundHelper?.PlayMusicSingle(enumObj);

        public bool IsPlaySound(object enumObj) => soundHelper.IsPlaySound(enumObj);
    }*/
    
    public class GlobalSoundHelper : SoundHelper
    {
        private static GlobalSoundHelper _instance;
        public static GlobalSoundHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalSoundHelper();
                }
                return _instance;
            }
        }
        public GlobalSoundHelper():base(){ }
    }
}
