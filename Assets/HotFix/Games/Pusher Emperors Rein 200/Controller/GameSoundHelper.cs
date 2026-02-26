using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMaker;
using System;

namespace PusherEmperorsRein
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
                      (enumObj) => SoundModel.Instance.gsHandlers[(SoundKey)enumObj]);
              }
              return _instance;
          }
      }
      public GameSoundHelper(Func<object, GSHandler> getGSHandler):base(getGSHandler){ }
  }
}
