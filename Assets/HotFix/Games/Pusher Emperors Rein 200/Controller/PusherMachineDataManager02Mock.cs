using SBoxApi;
using SimpleJSON;
using UnityEngine;

/// <summary>
/// </summary>
/// <remarks>
/// * 简化的封装，让代码更加直观。
/// </remarks>
public partial class PusherMachineDataManager02
{



    void OnMockCoinPushConsoleHardwareResult(object req)
    {
        if (Time.unscaledTime > lastHardwareTestTimeS)
        {
            lastHardwareTestTimeS = Time.unscaledTime + UnityEngine.Random.Range(0.5f, 1.2f);

            if (testIsEnableCoinDown)
            {
                int last = (int)curHardwareTestData["coinPushTestCoins"];
                curHardwareTestData["coinPushTestCoins"]
                    = UnityEngine.Random.Range(last + 1, last + 3);
            }

            if (testIsEnableBallDown)
            {
                int last = (int)curHardwareTestData["coinPushTestBalls"];
                curHardwareTestData["coinPushTestBalls"]
                    = UnityEngine.Random.Range(last + 1, last + 3);
            }


            if (testIsTestRegainCoins)
            {
                int last = (int)curHardwareTestData["coinPushTestRegainCoins"];
                curHardwareTestData["coinPushTestRegainCoins"]
                    = UnityEngine.Random.Range(last + 1, last + 3);
            }

            if (testIsTestRegainBalls)
            {
                int last = (int)curHardwareTestData["coinPushTestRegainBalls"];
                curHardwareTestData["coinPushTestRegainBalls"]
                    = UnityEngine.Random.Range(last + 1, last + 3);
            }
        }

        EventCenter.Instance.EventTrigger<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, curHardwareTestData.ToString());

    }

    void OnMockCoinPushConsoleHardwareFlg(object req) // 即将废弃
    {
        // 确保每个位值都是有效的0或1（每个值只能是0或1）
        int bit0 = testIsEnableCoinDown ? 1 : 0;
        int bit1 = testIsEnableBallDown ? 1 : 0;

        int[] bits = { bit0, bit1 };  //{ bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8 };
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i] &= 1; // 只保留最低位，确保值为0或1
        }

        // 合成一个int：将每个位移动到对应位置后进行或运算
        int combined = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            combined |= (bits[i] << i);
        }

        JSONNode res = new JSONObject();
        res["code"] = 0;
        res["flag"] = combined; //1bit 发币，2bit 发球

        // int bit0 = number & 1;  bool isCoinDown = bit0 != 0;
        // int bit1 = number & 2;  bool isBallDown = bit1 != 0;
        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, res.ToString());
    }



    void OnMockCoinPushConsoleHardwareTestStartEnd(object req) 
    {
        int oper = (int)req;

        curHardwareTestData["code"] = 0;


        if (oper == 255)
        {
            testIsEnableCoinDown = false;
            testIsEnableBallDown = false;

            curHardwareTestData["coinPushTestPlate"] = 0;
            curHardwareTestData["coinPushTestWiper"] = 0;
            testIsTestRegainCoins = false;
            testIsTestRegainBalls = false;
        }
        else
        {

            if (oper == 1)
            {
                testIsEnableCoinDown = true;
            }

            if (oper == 2)
            {
                testIsEnableBallDown = true;
            }

            if (oper == 3)
            {
                curHardwareTestData["coinPushTestPlate"] = 1;
            }

            if (oper == 4)
            {
                curHardwareTestData["coinPushTestWiper"] = 1;
            }

            if (oper == 6)
            {
                testIsTestRegainCoins = true;
            }
            if (oper == 7)
            {
                testIsTestRegainBalls = true;
            }
        }

        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, curHardwareTestData.ToString());
    }



    //return new object[] { 408, $"【PusherMachineDataMgr - Mock】没有实现方法：{rpcName}" };

    bool testIsEnableCoinDown = false;
    bool testIsEnableBallDown = false;
    bool testIsTestRegainCoins = false;
    bool testIsTestRegainBalls = false;


    float lastHardwareTestTimeS = 0f;
    //int curHardwareTestOper = 0;
    JSONNode _curHardwareTestData = null;
    JSONNode curHardwareTestData
    {
        get
        {
            if (_curHardwareTestData == null)
            {
                _curHardwareTestData = new JSONObject();
                _curHardwareTestData["code"] = 0;
                _curHardwareTestData["coinPushTestCoins"] = 0;// 后台测试模式，当前累计掉币个数
                _curHardwareTestData["coinPushTestBalls"] = 0;// 后台测试模式，当前累计掉球个数
                _curHardwareTestData["coinPushTestPlate"] = 0;//  测试推盘  1:测试，0:停止
                _curHardwareTestData["coinPushTestWiper"] = 0;//  测试雨刷  1:测试，0:停止
                _curHardwareTestData["coinPushTestRegainCoins"] = 0;// 后台测试模式，当前累计总回币个数
                _curHardwareTestData["coinPushTestRegainBalls"] = 0;// 后台测试模式，当前累计总回球个数
            }
            return _curHardwareTestData;
        }
        set => _curHardwareTestData = value;

    }
}
