using GameMaker;
using System;

namespace FeiZhouHeiXingXing_3994
{
    public class GameSoundHelper3994 : SoundHelper
    {
        private static GameSoundHelper3994 _instance;

        public static GameSoundHelper3994 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSoundHelper3994(
                        (enumObj) => SoundModel.Instance.gsHandlers[(SoundKey)enumObj]);
                }

                return _instance;
            }
        }

        public GameSoundHelper3994(Func<object, GSHandler> getGSHandler) : base(getGSHandler) { }
    }
}