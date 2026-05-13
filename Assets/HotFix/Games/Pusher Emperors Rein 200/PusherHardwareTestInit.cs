using SimpleJSON;
using System;

namespace PusherEmperorsRein
{
    /// <summary>
    /// 进游戏前复位推币机硬件自测状态（原 ConsoleCoinPusher01.PageConsoleCheckHardware02.InitHardwaveTest）。
    /// </summary>
    public static class PusherHardwareTestInit
    {
        static bool _isInitGetHardwareTestFlg;

        public static void InitHardwaveTest()
        {
            if (_isInitGetHardwareTestFlg)
                return;

            CloseAllTest(() => { _isInitGetHardwareTestFlg = true; });
        }

        static void CloseAllTest(Action callback = null)
        {
            PusherMachineDataManager02.Instance.RequestCosoleTesetStartEnd(255, (res2) =>
            {
                JSONNode result = JSONNode.Parse((string)res2);
                if ((int)result["code"] == 0)
                {
                    callback?.Invoke();
                }
            });
        }
    }
}
