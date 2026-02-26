/**
 * @file    
 * @author  Huang Wen <Email:ww1383@163.com, QQ:214890094, WeChat:w18926268887>
 * @version 1.0
 *
 * @section LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * @section DESCRIPTION
 *
 * This file is ...
 */
using Hal;
using UnityEngine;

namespace SBoxApi
{
    public partial class SBoxEventHandle
    {
        // IDEA
        public const string SBOX_IDEA_VERSION = "SBOX_IDEA_VERSION";
        public const string SBOX_IDEA_USN = "SBOX_IDEA_USN";
        public const string SBOX_RESET = "SBOX_RESET";
        public const string SBOX_READ_CONF = "SBOX_READ_CONF";
        public const string SBOX_WRITE_CONF = "SBOX_WRITE_CONF";
        public const string SBOX_CHECK_PASSWORD = "SBOX_CHECK_PASSWORD";
        public const string SBOX_CHANGE_PASSWORD = "SBOX_CHANGE_PASSWORD";
        public const string SBOX_REQUEST_CODER = "SBOX_REQUEST_CODER";
        public const string SBOX_CODER = "SBOX_CODER";
        public const string SBOX_GET_ODDS = "SBOX_GET_ODDS";
        public const string SBOX_GET_PRIZE_PREPARE = "SBOX_GET_PRIZE_PREPARE";
        public const string SBOX_GET_PRIZE = "SBOX_GET_PRIZE";
        public const string SBOX_GET_PRIZE_PLAYER = "SBOX_GET_PRIZE_PLAYER";
        public const string SBOX_SET_PLAYER_BETS = "SBOX_SET_PLAYER_BETS";
        public const string SBOX_SET_COIN_TO_HOLE_COUNT = "SBOX_SET_COIN_TO_HOLE_COUNT";
        public const string SBOX_GET_SUMMARY = "SBOX_GET_SUMMARY";
        public const string SBOX_GET_ACCOUNT = "SBOX_GET_ACCOUNT";
        public const string SBOX_MOVE_PLAYER_SCORE = "SBOX_MOVE_PLAYER_SCORE";
        public const string SBOX_REQUEST_START = "SBOX_REQUEST_START";
        public const string SBOX_BETS_START = "SBOX_BETS_START";
        public const string SBOX_BETS_STOP = "SBOX_BETS_STOP";
        public const string SBOX_BATS_COUNT_DOWN = "SBOX_BATS_COUNT_DOWN";
        public const string SBOX_IS_MACHINE_ID_READY = "SBOX_IS_MACHINE_ID_READY";
        public const string SBOX_WAVE_GAME_COUNT = "SBOX_WAVE_GAME_COUNT";

        public const string SBOX_BATTLE_GET_STATE = "SBOX_BATTLE_GET_STATE";
        public const string SBOX_BATTLE_GAME_NUMBER = "SBOX_BATTLE_GAME_NUMBER";
        public const string SBOX_BATTLE_GET_COMPLETED_GAME = "SBOX_BATTLE_GET_COMPLETED_GAME";
        public const string SBOX_BATTLE_RESET_GAME = "SBOX_BATTLE_RESET_GAME";
        public const string SBOX_BATTLE_NEW_ROUND = "SBOX_BATTLE_NEW_ROUND";
        public const string SBOX_BATTLE_END_ROUND = "SBOX_BATTLE_END_ROUND";
        public const string SBOX_BATTLE_REQUEST_RESULT = "SBOX_BATTLE_REQUEST_RESULT";
        public const string SBOX_BATTLE_LEAD_START = "SBOX_BATTLE_LEAD_START";
        public const string SBOX_BATTLE_LEAD_STOP = "SBOX_BATTLE_LEAD_STOP";
        public const string SBOX_BATTLE_LUCK_SHOW = "SBOX_BATTLE_LUCK_SHOW";
        public const string SBOX_BATTLE_LUCK_PRIZE = "SBOX_BATTLE_LUCK_PRIZE";
        public const string SBOX_BATTLE_PRINTER_OPEN_BOX = "SBOX_BATTLE_PRINTER_OPEN_BOX";
        public const string SBOX_BATTLE_PLAYER_OUT_STATE = "SBOX_BATTLE_PLAYER_OUT_STATE";

        public const string SBOX_SICBO_RESET_DATA = "SBOX_SICBO_RESET_DATA";
        public const string SBOX_SICBO_REQUEST_GOODLUCK = "SBOX_SICBO_REQUEST_GOODLUCK";
        public const string SBOX_SICBO_CALCULATE = "SBOX_SICBO_CALCULATE";
        public const string SBOX_SICBO_SET_DIFFICULTY = "SBOX_SICBO_SET_DIFFICULTY";
        public const string SBOX_GET_SUMMARY_SICBO = "SBOX_GET_SUMMARY_SICBO";
        public const string SBOX_SICBO_READ_USR_DATA = "SBOX_SICBO_READ_USR_DATA";
        public const string SBOX_SICBO_WRITE_USR_DATA = "SBOX_SICBO_WRITE_USR_DATA";

        public const string SBOX_CROCODILE_OPEN_INFO = "SBOX_CROCODILE_OPEN_INFO";
        public const string SBOX_CROCODILE_GET_REWARD = "SBOX_CROCODILE_GET_REWARD";

        // SANDBOX
        public const string SBOX_SANDBOX_VERSION = "SBOX_SANDBOX_VERSION";
        public const string SBOX_SANDBOX_USN = "SBOX_SANDBOX_USN";
        public const string SBOX_SANDBOX_GET_DATETIME = "SBOX_SANDBOX_GET_DATETIME";
        public const string SBOX_SANDBOX_SET_DATETIME = "SBOX_SANDBOX_SET_DATETIME";
        public const string SBOX_SADNBOX_RESET = "SBOX_SADNBOX_RESET";
        public const string SBOX_SADNBOX_COIN_OUT_START = "SBOX_SADNBOX_COIN_OUT_START";
        public const string SBOX_SADNBOX_COIN_OUT_STOP = "SBOX_SADNBOX_COIN_OUT_STOP";
        public const string SBOX_SADNBOX_METER_SET = "SBOX_SADNBOX_METER_SET";
        public const string SBOX_SADNBOX_MOTOR_TOUCH = "SBOX_SADNBOX_MOTOR_TOUCH";

        // bill
        public const string SBOX_SADNBOX_BILL_LIST_GET = "SBOX_SADNBOX_BILL_LIST_GET";
        public const string SBOX_SADNBOX_BILL_SELECT = "SBOX_SADNBOX_BILL_SELECT";
        public const string SBOX_SADNBOX_BILL_APPROVE = "SBOX_SADNBOX_BILL_APPROVE";
        public const string SBOX_SADNBOX_BILL_REJECT = "SBOX_SADNBOX_BILL_REJECT";

        // printer
        public const string SBOX_SADNBOX_PRINTER_LIST_GET = "SBOX_SADNBOX_PRINTER_LIST_GET";
        public const string SBOX_SADNBOX_PRINTER_SELECT = "SBOX_SADNBOX_PRINTER_SELECT";
        public const string SBOX_SADNBOX_PRINTER_RESET = "SBOX_SADNBOX_PRINTER_RESET";
        public const string SBOX_SADNBOX_PRINTER_FONTSIZE = "SBOX_SADNBOX_PRINTER_FONTSIZE";
        public const string SBOX_SADNBOX_PRINTER_PAPERCUT = "SBOX_SADNBOX_PRINTER_PAPERCUT";
        public const string SBOX_SADNBOX_PRINTER_MESSAGE = "SBOX_SADNBOX_PRINTER_MESSAGE";
        public const string SBOX_SADNBOX_PRINTER_DATESET = "SBOX_SADNBOX_PRINTER_DATESET";
        public const string SBOX_SADNBOX_PRINTER_DATEGET = "SBOX_SADNBOX_PRINTER_DATEGET";

        // roulette
        public const string SBOX_SADNBOX_ROULETTE_CTRL = "SBOX_SADNBOX_ROULETTE_CTRL";
        public const string SBOX_SADNBOX_ROULETTE_MOTOR_MODE = "SBOX_SADNBOX_ROULETTE_MOTOR_MODE";
        public const string SBOX_SADNBOX_ROULETTE_RUN = "SBOX_SADNBOX_ROULETTE_RUN";
        public const string SBOX_SADNBOX_ROULETTE_LED_DEMO = "SBOX_SADNBOX_ROULETTE_LED_DEMO";
        public const string SBOX_SADNBOX_ROULETTE_LED_MODE = "SBOX_SADNBOX_ROULETTE_LED_MODE";
        public const string SBOX_SADNBOX_ROULETTE_TOUCH = "SBOX_SADNBOX_ROULETTE_TOUCH";
        public const string SBOX_SADNBOX_ROULETTE_RESULT = "SBOX_SADNBOX_ROULETTE_RESULT";
        public const string SBOX_SADNBOX_ROULETTE_LED_STATE = "SBOX_SADNBOX_ROULETTE_LED_STATE";
        public const string SBOX_SADNBOX_ROULETTE_STATE = "SBOX_SADNBOX_ROULETTE_STATE";
        public const string SBOX_SADNBOX_ROULETTE_RESULT_COLOR = "SBOX_SADNBOX_ROULETTE_RESULT_COLOR";

        // eject
        public const string SBOX_SADNBOX_EJECT_STATE = "SBOX_SADNBOX_EJECT_STATE";
        public const string SBOX_SADNBOX_EJECT_RESULT_NUMBER = "SBOX_SADNBOX_EJECT_RESULT_NUMBER";
        public const string SBOX_SADNBOX_EJECT_OPEN = "SBOX_SADNBOX_EJECT_OPEN";
        public const string SBOX_SADNBOX_EJECT_CLOSE = "SBOX_SADNBOX_EJECT_CLOSE";
        public const string SBOX_SADNBOX_EJECT_RESET = "SBOX_SADNBOX_EJECT_RESET";
        public const string SBOX_SADNBOX_EJECT_SET = "SBOX_SADNBOX_EJECT_SET";

        //jackpot
        public const string SBOX_JACKPOT_HOST_INIT = "SBOX_JACKPOT_HOST_INIT";
        public const string SBOX_JACKPOT_BET_HOST = "SBOX_JACKPOT_BET_HOST";
        public const string SBOX_JACKPOT_WRITE_CONFIG = "SBOX_JACKPOT_WRITE_CONFIG";
        public const string SBOX_JACKPOT_READ_CONFIG = "SBOX_JACKPOT_READ_CONFIG";

        public const string SBOX_PLAYER_OUT_STATE = "SBOX_PLAYER_OUT_STATE";
    }


    public class SBoxBaseData
    {
        public int[] value;
        public SBoxBaseData(int[] value)
        {
            this.value = value;
        }
    }

    public class SBox : MonoBehaviour
    {
        private static bool bInit = false;

        public static void Init()
        {
            bInit = SBoxIOStream.Init();
            if (bInit)
            {
                SBoxIdea.Init();

                SBoxSandbox.Init();
            }
        }

        public static void Exit()
        {
            SBoxSandbox.Exit();

            SBoxIdea.Exit();

            SBoxIOStream.Exit();

            bInit = false;
        }


        /**
          *  @brief          
          *  @param          无
          *  @return         无
          *  @details        
          */
        private void Update()
        {
            if (bInit == true && !Application.isEditor)
            {
                int counter = 0;
                int millisecond = (int)(Time.deltaTime * 1000);

                SBoxIOStream.Exec();

                SBoxSandbox.Exec(millisecond);

                SBoxIdea.Exec(millisecond);

                counter = 0;

                //while (bExecRun == true)
                while (counter++ < 50)
                {
                    SBoxPacket packet = SBoxIOStream.Read();
                    if (packet != null)
                    {
                        SBoxIOEvent.SendEvent(packet.cmd, packet);
                    }
                    else
                    {
                        break;
                        //Thread.Sleep(5);
                    }
                }
            }
        }
    }
}