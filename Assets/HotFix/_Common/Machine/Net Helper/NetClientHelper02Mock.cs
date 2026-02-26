using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GlobalJackpotConsole
{

    /// <summary>
    /// </summary>
    /// <remarks>
    /// * 简化的封装，让代码更加直观。
    /// </remarks>
    public partial class NetClientHelper02
    {



        int testGrandCredit = 3000;
        int testMajorCredit = 2000;

        public bool testIsHitJpGrandNext = false;
        public bool testIsHitJpMajorNext = false;




        void OnMockLogin(object req)
        {
            LoginInfoR jsonData = new LoginInfoR();
            jsonData.seqId = (req as RequestBase).seqId;

            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)S2C_CMD.S2C_LoginR,
                id = MachineId,
                jsonData = JsonConvert.SerializeObject(jsonData)
            };
            string res = JsonConvert.SerializeObject(msgInfo);
            byte[] resDtat = Encoding.UTF8.GetBytes(res);
            Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
        }


        void OnMockReadConf(object req)
        {
            SBoxConfData mockData = new SBoxConfData();
            mockData.CoinValue = 99;
            mockData.scoreTicket = 88;
            mockData.TicketValue = 77;
            mockData.MachineId = 22222222;
            mockData.LineId = 2222;


            ReadConfR jsonData = new ReadConfR();
            jsonData.seqId = (req as RequestBase).seqId;
            jsonData.sboxConfData = mockData;

            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)S2C_CMD.S2C_ReadConfR,
                id = MachineId,
                jsonData = JsonConvert.SerializeObject(jsonData)
            };
            string res = JsonConvert.SerializeObject(msgInfo);
            byte[] resDtat = Encoding.UTF8.GetBytes(res);
            Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
        }



        void OnMockJackBet(object req)
        {
            JackBetInfoCoinPush data = req as JackBetInfoCoinPush;
            int index = UnityEngine.Random.Range(0, 200);
            int jackpotId = -1;
            int win = 0;

            int winGrand = 0;
            int winMajor = 0;

            bool isHitGrand = index == 0 || testIsHitJpGrandNext;
            bool isHitMahor = index == 1 || testIsHitJpMajorNext;

            if (isHitGrand)
            {
                jackpotId = 0;
                win = data.grandBet * 1;
                winGrand = win;
            }

            if (isHitMahor)
            {
                jackpotId = 1;
                win = data.majorBet * 1;
                winMajor = win;
            }

            testIsHitJpGrandNext = false;
            testIsHitJpMajorNext = false;

            testGrandCredit += UnityEngine.Random.Range(5, 20);
            testMajorCredit += UnityEngine.Random.Range(5, 20);

            int testGrandCreditOld = isHitGrand ? testGrandCredit : 0;
            int testMajorCreditOld = isHitMahor ? testMajorCredit : 0;

            if (isHitGrand || isHitMahor)
            {
                testGrandCredit -= winGrand;
                testMajorCredit -= winMajor;
            }
            else
            {
                if (testGrandCredit > 4000 || testGrandCredit < 0) testGrandCredit = 3000;
                if (testMajorCredit > 3000 || testMajorCredit < 0) testMajorCredit = 2000;
            }


            SBoxJackpotData sboxJackpotData = new SBoxJackpotData()
            {
                result = 0,
                MachineId = MachineId,
                SeatId = SBoxModel.Instance.seatId,
                ScoreRate = 1 * 1000,
                JpPercent = 1 * 1000,

                // 0:表示没有开出彩金，1:表示已开出彩金
                Lottery = new int[] {
                                isHitGrand? 1:0,
                                isHitMahor? 1:0,
                                0,
                                0 },

                // 开出的彩金注意:此处的单位是钱的单位，而且是乘以了100的，分机收到这个值要根据分机的分值比来转成成对应的分数，而且还要将此值除以100
                Jackpotlottery = new int[]
                {
                                winGrand * 100 ,
                                winMajor * 100 ,
                                0 ,
                                0
                },

                // 彩金显示积累分,用于显示当前的彩金值
                JackpotOut = new int[] {
                                testGrandCredit * 100 ,
                                testMajorCredit * 100 ,
                                0,
                                0
                            },

                // 开出彩金前的显示积累分
                JackpotOld = new int[] {
                                testGrandCreditOld * 100 ,
                                testMajorCreditOld * 100 ,
                                0,
                                0
                            },

            };


            JackBetInfoCoinPushR jsonData = new JackBetInfoCoinPushR();
            jsonData.seqId = data.seqId;
            jsonData.sboxJackpotData = sboxJackpotData;


            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)S2C_CMD.S2C_JackBetR,
                id = MachineId,
                jsonData = JsonConvert.SerializeObject(jsonData)
            };
            string res = JsonConvert.SerializeObject(msgInfo);
            byte[] resDtat = Encoding.UTF8.GetBytes(res);
            Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);

        }


        void OnMockGetJackpotShowValue(object req)
        {
            JackpotGameShowInfoR jsonData = new JackpotGameShowInfoR();
            jsonData.seqId = (req as RequestBase).seqId;
            jsonData.curJackpotOut = new int[] {
                            testGrandCredit * 100 ,
                            testMajorCredit * 100 ,
                            0,
                            0
                        };

            MsgInfo msgInfo = new MsgInfo
            {
                cmd = (int)S2C_CMD.S2C_GetJackpotShowValueR,
                id = MachineId,
                jsonData = JsonConvert.SerializeObject(jsonData)
            };
            string res = JsonConvert.SerializeObject(msgInfo);
            byte[] resDtat = Encoding.UTF8.GetBytes(res);
            Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);

        }


    }
}