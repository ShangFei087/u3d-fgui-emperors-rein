using GameMaker;
using System;

namespace MeiZhouHeiBao_3993
{
    public class GameSoundHelper3993 : SoundHelper
    {
        private static GameSoundHelper3993 _instance;

        public static GameSoundHelper3993 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSoundHelper3993(
                        (enumObj) => SoundModel.Instance.gsHandlers[(SoundKey)enumObj]);
                }

                return _instance;
            }
        }

        private GameSoundHelper3993(Func<object, GSHandler> getGsHandler) : base(getGsHandler) { }
    }
}