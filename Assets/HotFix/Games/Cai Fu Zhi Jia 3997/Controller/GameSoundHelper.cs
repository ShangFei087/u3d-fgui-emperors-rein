using GameMaker;
using System;

namespace CaiFuZhiJia_3997
{
    public class GameSoundHelper : SoundHelper
    {
        private static GameSoundHelper _instance;

        public static GameSoundHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSoundHelper(
                        (enumObj) => SoundModel.Instance.gsHandlers[(SoundModel.SoundKey)enumObj]);
                }

                return _instance;
            }
        }

        public GameSoundHelper(Func<object, GSHandler> getGSHandler) : base(getGSHandler) { }
    }
}