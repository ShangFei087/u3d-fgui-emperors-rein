using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBoxApi
{
    public partial class SBoxEventHandle
    {

        // 游戏
        public const string SBOX_IDEA_INFO = "SBOX_IDEA_INFO";

        public const string SBOX_SLOT_SPIN = "SBOX_SLOT_SPIN";
        public const string SBOX_JACKPOT_GAME = "SBOX_JACKPOT_GAME";

        // 推币机
        public const string SBOX_COIN_PUSH_BEGIN_TURN = "SBOX_COIN_PUSH_BEGIN_TURN";
        public const string SBOX_COIN_PUSH_SPIN = "SBOX_COIN_PUSH_SPIN";
        public const string SBOX_COIN_PUSH_SPIN_END = "SBOX_COIN_PUSH_SPIN_END";

        public const string SBOX_COIN_PUSH_GET_JP_CONTRIBUTION = "SBOX_COIN_PUSH_GET_JP_CONTRIBUTION";
        public const string SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION = "SBOX_COIN_PUSH_RETURN_CONTRIBUTION";
        public const string SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN = "SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN";
        public const string SBOX_COIN_PUSH_GET_LOCAL_JP_CONTRIBUTION = "SBOX_COIN_PUSH_GET_LOCAL_JP_CONTRIBUTION";


        public const string SBOX_COIN_PUSH_HARDWARE_TEST_START_END = "SBOX_COIN_PUSH_HARDWARE_TEST_START_END"; // 后台开始测试
        public const string SBOX_COIN_PUSH_CONSOLE_TOP_COIN_IN2 = "SBOX_COIN_PUSH_CONSOLE_TOP_COIN_IN2";
        public const string SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN2 = "SBOX_COIN_PUSH_CONSOLE_TOGGLE_BONUS_IN2";  // 1 开始 0 停止
        public const string SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER2 = "SBOX_COIN_PUSH_CONSOLE_TOGGLE_WIPER2";  // 1 开始 0 停止
        public const string SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE2 = "SBOX_COIN_PUSH_CONSOLE_TOGGLE_PUSHPLATE2";  // 1 开始 0 停止

        public const string SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG = "SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG";
        public const string SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT = "SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT";

        // 新增加
        public const string SBOX_DEBUG_CONTROL_MODE = "SBOX_DEBUG_CONTROL_MODE";  //算法卡调试模式
        public const string SBOX_SWITCH_GAME = "SBOX_SWITCH_GAME";//切换游戏


    }
}