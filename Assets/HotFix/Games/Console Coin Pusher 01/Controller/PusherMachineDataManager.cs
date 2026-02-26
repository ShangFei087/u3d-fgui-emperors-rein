using FairyGUI;
using Hal;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleCoinPusher01
{
    /// <summary>
    /// 【即将废弃使用】
    /// </summary>
    public class PusherMachineDataManager : MachineDataManager
    {
        private static object _mutex = new object();
        static PusherMachineDataManager _instance;

        public new static PusherMachineDataManager Instance
        {
            get
            {

                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<PusherMachineDataManager>();
                        if (_instance == null)
                        {
                            GameObject obj = new GameObject();
                            _instance = obj.AddComponent<PusherMachineDataManager>();

                            obj.name = _instance.GetType().Name;
                            if (obj.transform.parent == null)
                            {
                                DontDestroyOnLoad(obj);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }



        /*

        /// <summary>
        /// 开始按钮灯
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleLightSpin(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_SPIN, null, finishCallback, null);

        void OnResponseCosoleLightSpin(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_SPIN, res, false);



        /// <summary>
        /// 雨刷按钮灯
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleWiperOff(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTWIPEROFF, null, finishCallback, null);

        void OnResponseCosoleWiperOff(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTWIPEROFF, res, false);


        /// <summary>
        /// 顶部入币
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleTopCoinIn(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, null, finishCallback, null);

        void OnResponseCosoleTopCoinIn(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, res, false);


        /// <summary>
        /// 发球
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleBonusIn(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, null, finishCallback, null);

        void OnResponseCosoleBonusIn(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, res, false);




        /// <summary>
        /// 回币器
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleCollectCoin(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN, null, finishCallback, null);

        void OnResponseCosoleCollectCoin(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN, res, false);




        /// <summary>
        /// 彩票机
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleTicketer(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER, null, finishCallback, null);

        void OnResponseCosoleTicketer(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER, res, false);




        /// <summary>
        /// 雨刷
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleWiper(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER, null, finishCallback, null);

        void OnResponseCosoleWiper(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER, res, false);


        /// <summary>
        /// 推盘
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosolePushPlate(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE, null, finishCallback, null);

        void OnResponseCosolePushPlate(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE, res, false);



        /// <summary>
        /// 摇摆通道
        /// </summary>
        /// <param name="finishCallback"> 0 暂停 1 开始</param>
        /// <returns></returns>
        public int RequestCosoleBell(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BELL, null, finishCallback, null);

        void OnResponseCosoleBell(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BELL, res, false);



        /// <summary>
        /// 通道灯
        /// </summary>
        /// <param name="finishCallback"> 0是通道1灯 1 通道2灯
        /// <returns></returns>
        public int RequestCosoleChanneiLight(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_CHANNEILIGHT, null, finishCallback, null);

        void OnResponseCosoleChanneiLight(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_CHANNEILIGHT, res, false);

        */



        /// <summary>
        ///  开始测试"开始"或"停止"
        /// </summary>
        /// <param name="oper">
        /// 1:发币测试
        /// 2:发球测试
        /// 3:推盘测试
        /// 4:雨刷测试
        /// 5:铃铛测试
        /// 6:回币测试
        /// 7:回球测试
        /// 255: 停止所有测试
        /// </param>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        /// <remarks>
        /// * 切换测试模式后，必须手动停止之前的测试项
        /// </remarks>
        public int RequestCosoleTesetStartEnd(int oper, Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, oper, finishCallback, null);

        void OnResponseCosoleTesetStartEnd(string res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, res, false);



        ///// <summary>
        ///// 请求停止硬件测试
        ///// </summary>
        ///// <param name="finishCallback"></param>
        //public void RequestToStopHardwareTest(Action<object> finishCallback)
        //{
        //    if (curHardwareTestOper != 0)
        //    {
        //        int oper = curHardwareTestOper;
        //        curHardwareTestOper = 0;
        //        RequestCosoleTesetStartEnd(oper, finishCallback);
        //    }
        //    else
        //    {
        //        finishCallback?.Invoke(null);
        //    }
        //}




        /*
        const string RpcNameGetCoinPushHardwareState = "RpcNameGetCoinPushHardwareState";

        /// <summary>
        /// 【弃用】获取当前硬件测试状态
        /// </summary>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public int RequestGetCoinPushHardwareState(Action<object> finishCallback)
            => severHelper.RequestData(RpcNameGetCoinPushHardwareState, null, finishCallback, null);

        void OnResponseGetCoinPushHardwareState(string res) =>
            severHelper.OnResponsData(RpcNameGetCoinPushHardwareState, res, false);
        */

        /// <summary>
        /// 获取当前硬件测试状态
        /// </summary>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public int RequestGetCoinPushHardwareFlag(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, null, finishCallback, null);

        void OnResponseGetCoinPushHardwareFlag(string res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, res, false);


        public int RequestGetCoinPushHardwareResult(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, null, finishCallback, null);

        void OnResponseGetCoinPushHardwareResult(string res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, res, false);





        protected override void Start()
        {
            base.Start();
            //===========================

            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTSPIN, OnResponseCosoleLightSpin);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTWIPEROFF, OnResponseCosoleWiperOff);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN, OnResponseCosoleCollectCoin);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER, OnResponseCosoleWiper);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE, OnResponseCosolePushPlate);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BELL, OnResponseCosoleBell);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER, OnResponseCosoleTicketer);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, OnResponseCosoleTopCoinIn);
            //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, OnResponseCosoleBonusIn);

            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, OnResponseCosoleTesetStartEnd);
            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, OnResponseGetCoinPushHardwareFlag);

            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, OnResponseGetCoinPushHardwareResult);



        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            //================
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTSPIN, OnResponseCosoleLightSpin);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTWIPEROFF, OnResponseCosoleWiperOff);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN, OnResponseCosoleCollectCoin);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER, OnResponseCosoleWiper);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE, OnResponseCosolePushPlate);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BELL, OnResponseCosoleBell);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER, OnResponseCosoleTicketer);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, OnResponseCosoleTopCoinIn);
            //EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, OnResponseCosoleBonusIn);


            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, OnResponseCosoleTesetStartEnd);
            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, OnResponseGetCoinPushHardwareFlag);

            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, OnResponseGetCoinPushHardwareResult);


        }


        public override object[] requestFuncMock(string rpcName, object req)
        {
            object[] result = new object[] { 0 };




            result = MachineDataManager.Instance.requestFuncMock(rpcName, req);

            if ((int)result[0] != 408)
                return result;

            switch (rpcName)
            {

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT:
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
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG:  // 即将废弃
                    {

                        // 确保每个位值都是有效的0或1（每个值只能是0或1）
                        int bit0 = testIsEnableCoinDown?1:0;
                        int bit1 = testIsEnableBallDown?1:0;

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
                    return result;
                    /* 弃用
                case RpcNameGetCoinPushHardwareState:
                    {
                        if(Time.unscaledTime > lastHardwareTestTimeS)
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
                        OnResponseGetCoinPushHardwareState(curHardwareTestData.ToString());
                    }
                    return result;
                    */
                case SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END:
                    {
                        int oper = (int)req;

                        curHardwareTestData["code"] = 0;


                        if(oper == 255)
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
                        /*
                        if (oper == 1)
                        {
                            testIsEnableCoinDown = !testIsEnableCoinDown;
                        }


                        if (oper == 2)
                        {
                            testIsEnableBallDown = !testIsEnableBallDown;
                        }


                        if (oper == 3)
                        {
                            int last = (int)curHardwareTestData["coinPushTestPlate"];
                            curHardwareTestData["coinPushTestPlate"] = last > 0 ? 0 : 1;
                        }

                        if (oper == 4)
                        {
                            int last = (int)curHardwareTestData["coinPushTestWiper"];
                            curHardwareTestData["coinPushTestWiper"] = last > 0 ? 0 : 1;
                        }
                        */

                        EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, curHardwareTestData.ToString());

                    }
                    return result;

                    /*
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTSPIN:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTWIPEROFF:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN:
                    {
                        testToggleTopCoinInState = testToggleTopCoinInState == 0 ? 1 : 0;
                       // EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN, testToggleTopCoinInState);

                        if (testToggleTopCoinInState == 1) // 开始
                        {
                            testTotalTopCoinIn = UnityEngine.Random.Range(100, 200);
                            Timers.inst.Add(0.5f, 0, TestDoTopCoinIn);
                        }
                        else
                        {
                            Timers.inst.Remove(TestDoTopCoinIn);
                        }

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN:
                    {

                        testToggleBonusState = testToggleBonusState == 0 ? 1 : 0;
                        //EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, testToggleBonusState);

                        if (testToggleBonusState == 1) // 开始
                        {
                            testTotalBonusIn = UnityEngine.Random.Range(1, 5);
                            DebugUtils.LogError(testTotalBonusIn);
                            Timers.inst.Add(0.5f, 0, TestDoBonusIn);
                        }
                        else
                        {
                            Timers.inst.Remove(TestDoBonusIn);
                        }

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BELL:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_CHANNEILIGHT:
                    {

                    }
                    return result;
                    */
            }

            DebugUtils.LogError($"【PusherMachineDataMgr - Mock】没有实现方法：{rpcName}");
            return new object[] { 408, $"【PusherMachineDataMgr - Mock】没有实现方法：{rpcName}" };
        }


        bool testIsEnableCoinDown = false;
        bool testIsEnableBallDown = false;
        bool testIsTestRegainCoins = false;
        bool testIsTestRegainBalls = false;
        //




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




        /*
        int testToggleTopCoinInState = 0;
        int testTotalTopCoinIn = 0;
        void TestDoTopCoinIn(object parm)
        {

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOP_COIN_IN, 1);
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOUCH_CHANNEL, UnityEngine.Random.Range(0,8));

            int indexSp = UnityEngine.Random.Range(0, 10);
            if (indexSp==0 || indexSp == 1)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOUCH_SP, indexSp);
            }

            int countReturnCoin = UnityEngine.Random.Range(0, 10);
            if (countReturnCoin > 0 && countReturnCoin <= 3)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_RETURN_COIN, countReturnCoin);
            }

            if (--testTotalTopCoinIn ==0)
            {
                Timers.inst.Remove(TestDoTopCoinIn);
                testToggleTopCoinInState = 0;
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOP_COIN_IN, 0);
                testTotalTopCoinIn = 0;
            }
        }
        */








        /*
        int testToggleBonusState = 0;
        int testTotalBonusIn = 0;
        void TestDoBonusIn(object parm)
        {

 

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, 1);


            int countReturnCoin = UnityEngine.Random.Range(0, 7);
            if (countReturnCoin > 0 && countReturnCoin <= 3)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_RETURN_COIN, countReturnCoin);
            }

            if (--testTotalBonusIn == 0)
            {
                Timers.inst.Remove(TestDoBonusIn);
                testToggleBonusState = 0;
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN, 0);
                testTotalBonusIn = 0;
            }
        }

        */


        public override object[] requestFunc(string rpcName, object req)
        {
            object[] result;

            if (ApplicationSettings.Instance.isMock) //统一使用假的彩金数据
            {
                if (!ApplicationSettings.Instance.isMock)
                {
                    result = requestFuncMock(rpcName, req);
                    return result;
                }
            }



            result = MachineDataManager.Instance.requestFunc(rpcName, req);

            if ((int)result[0] != 408)
                return result;

            switch (rpcName)
            {

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT:
                    {

                        SBoxIdea.GetHardwareResult();
                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG:
                    {
                        SBoxIdea.GetHardwareFlag();
                    }
                    return result;
                 /* 弃用
                case RpcNameGetCoinPushHardwareState:
                    {
                        curHardwareTestData["coinPushTestCoins"] = SBoxIdea.sBoxInfo.CoinPushTestCoins;
                        curHardwareTestData["coinPushTestBalls"] = SBoxIdea.sBoxInfo.CoinPushTestBalls;
                        curHardwareTestData["coinPushTestPlate"] = SBoxIdea.sBoxInfo.CoinPushTestPlate;
                        curHardwareTestData["coinPushTestWiper"] = SBoxIdea.sBoxInfo.CoinPushTestWiper;

                        OnResponseGetCoinPushHardwareState(curHardwareTestData.ToString());
                    }
                    return result;
                 */
                case SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END:
                    {
                        int oper = (int)req;
                        //curHardwareTestOper = curHardwareTestOper == oper ? 0 : oper;
                        SBoxIdea.CheckCoinPushHardware(oper);
                    }
                    return result;

                    /*
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTSPIN:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_LIGHTWIPEROFF:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TOP_COIN_IN:
                    {


                    


                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_COLLECT_COIN:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_TICKETER:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER:
                    {

                    }
                    return result;
                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_BELL:
                    {

                    }
                    return result;

                case SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_TOGGLE_CHANNEILIGHT:
                    {

                    }
                    return result;
                    */


            }

            DebugUtils.LogError($"【PusherMachineDataMgr - Real】没有实现方法：{rpcName}");
            return new object[] { 408, $"【PusherMachineDataMgr - Real】没有实现方法：{rpcName}" };
        }


    }


}

