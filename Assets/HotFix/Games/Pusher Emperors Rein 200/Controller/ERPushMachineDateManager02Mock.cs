using SBoxApi;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PusherEmperorsRein
{

    public partial class ERPushMachineDataManager02
    {

        float timeForCoinCountDown = 0;
        int remainCoinCountDown = -1;
        int coinToCountDown = 0;


        int myCreditMock
        {
            get => MachineDataManager02.Instance.myCreditMock;
            set => MachineDataManager02.Instance.myCreditMock = value;
        }
         


        void OnMockCoinCountDown(object req)
        {
            if (remainCoinCountDown < 0)
            {
                remainCoinCountDown = (int)req;
                coinToCountDown = remainCoinCountDown * 10 / 100;
                if (coinToCountDown <= 0)
                    coinToCountDown = 1;
            }

            OnResponseCoinCountDown(remainCoinCountDown);

            if (remainCoinCountDown == 0)
            {
                remainCoinCountDown = -1;
            }
            else if (timeForCoinCountDown < Time.unscaledTime)
            {
                timeForCoinCountDown = Time.unscaledTime + UnityEngine.Random.Range(0.02f, 0.5f);
                remainCoinCountDown -= coinToCountDown;

                if (remainCoinCountDown < 0)
                    remainCoinCountDown = 0;

            }
        }


        void OnMockGetMyCredit(object req)
        {
            /*
            if (myCreditMock > 100)
            {
                myCreditMock += UnityEngine.Random.Range(-4, 5);
            }
            */
            OnResponseGetMyCredit(myCreditMock);
        }


        void OnMockBeginTurn(object req)
        {
            // * ret:0表示成功，-1表示传参失败，-2表示分数不足,-3表示发币失败
            EventCenter.Instance.EventTrigger<int>(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, 0);
        }


        void OnMockCoinPushSpinEnd(object req)
        {
            // 滚轮停止
            JSONNode res = new JSONObject();
            res["code"] = 0;
            res["credit"] = myCreditMock;
            res["playerCredit"] = myCreditMock;
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END, res.ToString());
        }



        void OnMockCoinPushGetJPContribution(object req)
        {
            JSONNode res = new JSONObject();
            res["code"] = 0;
            res["major"] = 1;
            res["grand"] = 2;
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, res.ToString());
        }


        void OnMockCoinPushReturnJPContribution(object req)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, 0);
        }


        void OnMockCoinPushSetMajorGrandWin(object req)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, 0);
            //  return new object[] { 408, $"【ERPushMachineDataMgr - Mock】没有实现方法：{rpcName}" };
        }

    }
}