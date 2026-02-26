using SBoxApi;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuHuoChe_3996
{
    public class SlotG3996MachineDataManager : MachineDataManager
    {
        private static object _mutex = new object();
        static SlotG3996MachineDataManager _instance;

        public static SlotG3996MachineDataManager Instance
        {
            get
            {

                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<SlotG3996MachineDataManager>();
                        if (_instance == null)
                        {
                            GameObject obj = new GameObject();
                            _instance = obj.AddComponent<SlotG3996MachineDataManager>();

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

        /// <summary> 掉币倒计时 </summary>
        public const string RpcNameCoinCountDown = "RpcNameCoinCountDown";

        public const string RpcNameGetMyCredit = "RpcNameGetMyCredit";


        ///////////////////////////游戏数据
        ///<summary> 获取游戏彩金 </summary>
        public int RequestCoinPushSpin(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_SPIN, null, finishCallback, null);
        void OnResponseCoinPushSpin(string res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_SPIN, res, false);


        public int RequestCoinPushSpinEnd(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END, null, finishCallback, null);

        void OnResponseCoinPushSpinEnd(string res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END, res, false);



        public int RequestCoinCountDown(int winCredit, Action<object> finishCallback)
            => severHelper.RequestData(RpcNameCoinCountDown, winCredit, finishCallback, null);

        void OnResponseCoinCountDown(int res) => severHelper.OnResponsData(RpcNameCoinCountDown, res, false);



        //void OnResponseCoinPushSpin(string res) =>severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_SPIN, res,false);

        public int RequestGetMyCredit(Action<object> finishCallback)
            => severHelper.RequestData(RpcNameGetMyCredit, null, finishCallback, null);
        void OnResponseGetMyCredit(int res) => severHelper.OnResponsData(RpcNameGetMyCredit, res, false);



        /// <summary> 获取major和grand的贡献值 </summary>
        public int RequestGetJpMajorGrandContribution(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, null, finishCallback, null);
        void OnResponseGetJpMajorGrandContribution(string res) => severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, res, false);


        /// <summary> 通知算方法卡 Major 、Grand 赢的分数 </summary>
        public int RequestSetMajorGrandWin(int winCredit, Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, winCredit, finishCallback, null);
        void OnResponseSetMajorGrandWin(int res) => severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, res, false);


        /// <summary> 返还Major、Grand彩金贡献值 </summary>
        public int RequestReturnMajorGrandContribution(int majorCredit, int grandCredit, Action<object> finishCallback)
        {
            Dictionary<string, int> data = new Dictionary<string, int>()
            {
                ["major"] = majorCredit,
                ["grand"] = grandCredit,
            };
            return severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, data, finishCallback, null);
        }

        void OnResponseReturnMajorGrandContribution(int res) => severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, res, false);



        /// <summary>
        /// 开始一局游戏
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public int RequesBeginTurn(Action<object> finishCallback)
            => severHelper.RequestData(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, null, finishCallback, null);

        void OnResponseBeginTurn(int res) =>
            severHelper.OnResponsData(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, res, false);



        protected override void Start()
        {
            base.Start();


            // ===推币机游戏数据
            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN, OnResponseCoinPushSpin);
            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END, OnResponseCoinPushSpinEnd);
            EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, OnResponseBeginTurn);

            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, OnResponseGetJpMajorGrandContribution);
            EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, OnResponseReturnMajorGrandContribution);
            EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, OnResponseSetMajorGrandWin);
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();

            // ===推币机游戏数据
            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN, OnResponseCoinPushSpin);
            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END, OnResponseCoinPushSpinEnd);
            EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, OnResponseBeginTurn);


            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, OnResponseGetJpMajorGrandContribution);
            EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, OnResponseReturnMajorGrandContribution);
            EventCenter.Instance.RemoveEventListener<int>(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, OnResponseSetMajorGrandWin);
        }


        //public override object[] requestFunc(string rpcName, object req)
        //{
        //    object[] result = new object[] { 0 };

        //    if (rpcName != SBoxEventHandle.SBOX_GAME_JACKPOT) //统一使用假的彩金数据
        //    {
        //        if (!ApplicationSettings.Instance.isMock)
        //        {
        //            result = requestFunc02(rpcName, req);
        //            return result;
        //        }
        //    }



        //    result = MachineDataManager.Instance.requestFunc(rpcName, req);

        //    if ((int)result[0] != 408)
        //        return result;

        //    switch (rpcName)
        //    {

        //        case RpcNameCoinCountDown:
        //            {

        //                if (remainCoinCountDown < 0)
        //                {
        //                    remainCoinCountDown = (int)req;
        //                    coinToCountDown = remainCoinCountDown * 10 / 100;
        //                    if (coinToCountDown <= 0)
        //                        coinToCountDown = 1;
        //                }

        //                OnResponseCoinCountDown(remainCoinCountDown);

        //                if (remainCoinCountDown == 0)
        //                {
        //                    remainCoinCountDown = -1;
        //                }
        //                else if (timeForCoinCountDown < Time.unscaledTime)
        //                {
        //                    timeForCoinCountDown = Time.unscaledTime + UnityEngine.Random.Range(0.02f, 0.5f);
        //                    remainCoinCountDown -= coinToCountDown;

        //                    if (remainCoinCountDown < 0)
        //                        remainCoinCountDown = 0;

        //                }

        //            }
        //            return result;
        //        case RpcNameGetMyCredit:
        //            {

        //                //mycredit += UnityEngine.Random.Range(-4, 5);
        //                //if (mycredit < 20000)
        //                //    mycredit = 20000;
        //                //OnResponseGetMyCredit(mycredit);
        //                myCreditMock += UnityEngine.Random.Range(-4, 5);
        //                if (myCreditMock < 999)
        //                    myCreditMock = 999;
        //                OnResponseGetMyCredit(myCreditMock);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN:
        //            {
        //                // * ret:0表示成功，-1表示传参失败，-2表示分数不足,-3表示发币失败
        //                EventCenter.Instance.EventTrigger<int>(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, 0);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END:
        //            {
        //                // 滚轮停止
        //                JSONNode res = new JSONObject();
        //                res["code"] = 0;
        //                res["credit"] = myCreditMock;
        //                res["playerCredit"] = myCreditMock;
        //                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END, res.ToString());
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION:
        //            {
        //                JSONNode res = new JSONObject();
        //                res["code"] = 0;
        //                res["major"] = 1;
        //                res["grand"] = 2;
        //                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, res.ToString());
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION:
        //            {
        //                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, 0);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN:
        //            {
        //                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, 0);
        //            }
        //            return result;
        //    }

        //    Debug.LogError($"【ERPushMachineDataMgr - Mock】没有实现方法：{rpcName}");
        //    return new object[] { 408, $"【ERPushMachineDataMgr - Mock】没有实现方法：{rpcName}" };
        //}



        //float timeForCoinCountDown = 0;
        //int remainCoinCountDown = -1;
        //int coinToCountDown = 0;
        //public override object[] requestFunc02(string rpcName, object req)
        //{
        //    object[] result = MachineDataManager.Instance.requestFunc02(rpcName, req);

        //    if ((int)result[0] != 408)
        //        return result;

        //    switch (rpcName)
        //    {

        //        case SBoxEventHandle.SBOX_COIN_PUSH_SPIN:
        //            {
        //                SBoxIdea.CoinPushGetSpinResult(0);
        //            }
        //            return result;

        //        case SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END:
        //            {
        //                // 滚轮停止
        //                SBoxIdea.CoinPushGetSpinEnd(0);
        //            }
        //            return result;

        //        case RpcNameCoinCountDown:
        //            {
        //                OnResponseCoinCountDown(SBoxIdea.Jackpot());
        //            }
        //            return result;
        //        case RpcNameGetMyCredit:
        //            {

        //                //foreach (SBoxPlayerScoreInfo item in SBoxIdea.sBoxInfo.PlayerScoreInfoList)
        //                //{
        //                //    if (item.PlayerId == 1)
        //                //    {
        //                //        mycredit = item.Score;
        //                //    }
        //                //}

        //                //OnResponseGetMyCredit(mycredit);

        //                int myCredit = 0;

        //                foreach (SBoxPlayerScoreInfo item in SBoxIdea.sBoxInfo.PlayerScoreInfoList)
        //                {
        //                    if (item.PlayerId == 1)
        //                    {
        //                        myCredit = item.Score;
        //                    }
        //                }

        //                OnResponseGetMyCredit(myCredit);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN:
        //            {
        //                SBoxIdea.CoinPushSpin(0, 1);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION:
        //            {
        //                SBoxIdea.GetJpMajorGrandContribution(SBoxModel.Instance.pid);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION:
        //            {
        //                Dictionary<string, int> data = (Dictionary<string, int>)req;

        //                int majorCredit = data["major"];
        //                int grandCredit = data["grand"];

        //                SBoxIdea.ReturnJpMajorGrandContribution(SBoxModel.Instance.pid, majorCredit, grandCredit);
        //            }
        //            return result;
        //        case SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN:
        //            {
        //                int winCredit = (int)req;
        //                SBoxIdea.SetJpMajorGrandWin(SBoxModel.Instance.pid, winCredit);
        //            }
        //            return result;
        //    }

        //    Debug.LogError($"【ERPushMachineDataMgr - Real】没有实现方法：{rpcName}");
        //    return new object[] { 408, $"【ERPushMachineDataMgr - Real】没有实现方法：{rpcName}" };
        //}


    }
}
