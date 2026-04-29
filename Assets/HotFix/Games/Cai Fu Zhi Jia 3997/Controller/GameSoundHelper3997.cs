using GameMaker;
using System;

namespace CaiFuZhiJia_3997
{
    public class GameSoundHelper3997 : SoundHelper
    {
        private static GameSoundHelper3997 _instance;

        public static GameSoundHelper3997 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSoundHelper3997(
                        (enumObj) => SoundModel.Instance.gsHandlers[(SoundKey)enumObj]);
                }

                return _instance;
            }
        }

        public GameSoundHelper3997(Func<object, GSHandler> getGSHandler) : base(getGSHandler) { }
    }
}