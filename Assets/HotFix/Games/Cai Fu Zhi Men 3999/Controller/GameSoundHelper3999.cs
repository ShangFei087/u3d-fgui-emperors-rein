using GameMaker;
using System;

namespace CaiFuZhiMen_3999
{
    public class GameSoundHelper3999 : SoundHelper
    {
        private static GameSoundHelper3999 _instance;

        public static GameSoundHelper3999 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSoundHelper3999(
                        (enumObj) => SoundModel.Instance.gsHandlers[(SoundKey)enumObj]);
                }

                return _instance;
            }
        }

        private GameSoundHelper3999(Func<object, GSHandler> getGsHandler) : base(getGsHandler) { }
    }
}

