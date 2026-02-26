
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Resources;
using System.Threading.Tasks;

[System.Serializable]
public class BetAllow
{
    public long value;
    public int allowed;
}

/// <summary>
/// 压注信息
/// </summary>
[System.Serializable]
public class TableBetItem
{
    public long id;
    /// <summary> 游戏id </summary>
    public long game_id;
    /// <summary> 最小压住 </summary>
    public long bet_min;
    /// <summary> 最大压注 </summary>
    public long bet_max;
    /// <summary> 压注列表 </summary>
    public string bet_list;  // [{"value":50,"allowed":1},{"value":100,"allowed":1},{"value":150,"allowed":1},{"value":200,"allowed":1},{"value":250,"allowed":1},{"value":300,"allowed":1},{"value":500,"allowed":1},{"value":600,"allowed":1}]
    /// <summary> 已选压注索引 </summary>  
    public int default_bet_index;
    /// <summary> 已选择单线压注 </summary>
    public int default_apostar;
    /// <summary> 已选择线数 </summary>
    public int default_lines;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 修改时间 </summary>
    public long updated_at;

    public static async Task<TableBetItem[]> DefaultTable()
    {
        DebugUtils.LogError($"@@ ConfigUtils.curGameInfoURL = {ConfigUtils.curGameInfoURL}");

        TextAsset jsn = await ResourceManager02.Instance.LoadAssetAsync<TextAsset>(ConfigUtils.curGameInfoURL);
        
        JSONNode gameInfoNode = JSONNode.Parse(jsn.text);
        JSONNode node = JSONNode.Parse("[]");

        foreach (JSONNode item in gameInfoNode["bet_lst"])
        {
            node.Add(JSONNode.Parse( string.Format("{{\"value\":{0},\"allowed\":1}}", (float)item)));
        }
        string betListStr = node.ToString();

        return new TableBetItem[]{
            new TableBetItem()
            {
                game_id = (long)gameInfoNode["game_id"],
                bet_min = 50,
                bet_max = 600,
                bet_list = betListStr,
                default_bet_index = 0,
                default_apostar = -1,
                default_lines = -1,
                updated_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            }
        };
    }
}


/// <summary>
/// 语言支持
/// </summary>
[System.Serializable]
public class TableSupportLanguageItem
{
    public long id;
    /// <summary> 语言号码 </summary>
    public string number;
    /// <summary> 语言名称 </summary>
    public string name;
    /// <summary> 金钱符号 </summary>
    public string money_symbol;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    public static TableSupportLanguageItem[] DefaultTable()
    {
        return new TableSupportLanguageItem[]{
           new TableSupportLanguageItem()
            {
                number = "en",
                name = "English",
                money_symbol = "$",
            },
           new TableSupportLanguageItem()
            {
                number = "cn",
                name = "Chinese(简体)",
                money_symbol = "¥",
            },
           /*new TableSupportLanguageItem()
            {
                number = "tw",
                name = "Chinese(繁体)",
                money_symbol = "¥",
            }
           */
        };
    }
}

/// <summary>
/// 投退币记录
/// </summary>
[System.Serializable]
public class TableCoinInOutRecordItem
{
    public long id;
    /// <summary> 玩家id </summary>
    public string user_id = "";
    /// <summary> 输入输出 </summary>
    public int  in_out;
    /// <summary> 充值前 </summary>
    public long credit_before;
    /// <summary> 充值后 </summary>
    public long credit_after;
    /// <summary> 涉及游戏分 </summary>
    public long credit;
    /// <summary> 或涉及金额 </summary>
    public long as_money = -1;
    /// <summary> 个数 </summary>
    public long count;
    /// <summary> 订单id </summary>
    public string order_id;
    /// <summary> 设备编号 </summary>
    public long device_number;    //device_number  device_model
    /// <summary> 设备类型 </summary>
    public string device_type;
    /// <summary> 设备型号</summary>
    //public string device_model;

    /// <summary> 表格TableDevices的数据id号 </summary>
    //public int table_devices_id;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 日期 </summary>
    public long created_at;
}




/// <summary>
/// 机台设置
/// </summary>
[System.Serializable]
public class TableSysSettingItem
{
    public long id;
    /// <summary> 已使用语言编号 </summary>
    public string language_number = DefaultSettingsUtils.defLanguage;
    /// <summary> 音效量 (sound effect)</summary>
    public float sound = DefaultSettingsUtils.defSound;
    /// <summary> 背景音乐量 </summary>
    public float music = DefaultSettingsUtils.defMusic;
    /// <summary> 声音便捷设置（0-3）：0 静音,3最大音量 </summary>
    //public int sound_level = 3;
    /// <summary> 声音使能 （1开启,0关闭） </summary>
    public int sound_enable = 1;
    /// <summary> ？？ </summary>
    public int is_demo_voice = 1;
    /// <summary> 金钱列表 </summary>
    public string bills_list;
    /// <summary> 游戏允许显示金钱的列表 </summary>
    public string display_credit_list;
    /// <summary> 已选择-显示金钱索引 </summary>
    public int display_credit_as;
    /// <summary> 允许网络 </summary>
    public int is_network = 1;
    /// <summary> 允许调试（查看调试信息、使用调试工具） </summary>
    public int is_debug = ApplicationSettings.Instance.isRelease? 0: 1;
    /// <summary> 允许显示包更新信息 </summary>
    public int is_update_info = 1;
    /// <summary> 使用联网彩金（1使用，0禁用）</summary>
    public int is_jackpot_online;

    /// <summary> 使用日志显示页面</summary>
    public int enable_reporter_page = ApplicationSettings.Instance.isRelease? 0: 1;

    /// <summary> 使用测试工具</summary>
    public int enable_test_tool = ApplicationSettings.Instance.isRelease ? 0 : 1;

    /// <summary> 投币限制 </summary>
    public long coin_in_limit = 1000;
    /// <summary> 上分限制 </summary>
    public long credit_limit = 1000;
    /// <summary> 出票限制 </summary>
    public long payout_limit = 1000;
    /// <summary> 投钞限制 </summary>
    public long bill_limit = 1000;
    /// <summary> 最大投退币记录次数 </summary>
    public long max_coin_in_out_record = DefaultSettingsUtils.defMaxCoinInOutRecord;
    /// <summary> 最大游戏记录局数 </summary>
    public long max_game_record = DefaultSettingsUtils.defMaxGameRecord;
    /// <summary> 最大彩金记录局数 </summary>
    public long max_jackpot_record = DefaultSettingsUtils.defMaxJackpotRecord;
    /// <summary> 最大报错信息记录局数 </summary>
    public long max_error_record = DefaultSettingsUtils.defMaxErrorRecord;
    /// <summary> 最大事件信息记录局数 </summary>
    public long max_event_record = DefaultSettingsUtils.defMaxEventRecord;
    /// <summary> 最大日营收统计记录次数 </summary>
    public long max_businiess_day_record = DefaultSettingsUtils.defMaxBusinessDayRecord;
    /// <summary> 投币倍数（1币多少分） </summary>
    public long coin_in_scale = DefaultSettingsUtils.defCoinInScale;
    /// <summary> 投钞倍数（1钞多少分） </summary>
    public long bills_in_scale = DefaultSettingsUtils.defBillInScale;
    /// <summary> 出票倍数（1票多少分） </summary>
    public long ticket_out_scale = DefaultSettingsUtils.defCoinOutCreditPerTicket;
    /// <summary> 打印倍数（1分多少分） </summary>
    public long printer_out_scale = DefaultSettingsUtils.defPrintOutScale;

    /// <summary> 上下分倍数（1次多少分） </summary>
    public long credit_up_down_scale = DefaultSettingsUtils.defScoreUpDownScale;

    /// <summary> 上分长按倍数（1次多少分） </summary>
    public long credit_up_long_click_scale = DefaultSettingsUtils.defScoreUpLongClickScale;

    /// <summary> 主货币和游戏分兑换比例 </summary>
    public long money_meter_scale = DefaultSettingsUtils.moneyMeterScale;
    /// <summary> 选择的打印机编号 </summary>
    public int select_printer_number = 0;
    /// <summary> 选择的纸钞机编号 </summary>
    public int select_biller_number = 3;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 修改时间 </summary>
    public long updated_at;


    public static TableSysSettingItem[] DefaultTable()
    {
        long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new TableSysSettingItem[]{
            new TableSysSettingItem()
            {
                language_number = DefaultSettingsUtils.defLanguage,

                sound = 1f,
                music = 1f,
                is_demo_voice = 1,

                bills_list = "[{\"value\":1,\"allowed\":1},{\"value\":5,\"allowed\":1},{\"value\":10,\"allowed\":1},{\"value\":20,\"allowed\":1},{\"value\":50,\"allowed\":1},{\"value\":100,\"allowed\":1}]",

                display_credit_list = "[{\"name\":\"Money\",\"number\":1},{\"name\":\"Points\",\"number\":2}]",

                display_credit_as = 2,//"Money Points"

                is_network = 1,

                is_debug = DefaultSettingsUtils.isDebug,

                enable_reporter_page = DefaultSettingsUtils.enableReporterPage,

                enable_test_tool = DefaultSettingsUtils.enableTestTool,

                is_update_info = DefaultSettingsUtils.isUpdateInfo,

                is_jackpot_online = DefaultSettingsUtils.isJackpotOnline,

                max_coin_in_out_record = DefaultSettingsUtils.defMaxCoinInOutRecord,

                max_game_record = DefaultSettingsUtils.defMaxGameRecord,

                max_jackpot_record = DefaultSettingsUtils.defMaxJackpotRecord,

                max_error_record = DefaultSettingsUtils.defMaxErrorRecord,

                max_event_record = DefaultSettingsUtils.defMaxEventRecord,

                coin_in_scale = DefaultSettingsUtils.defCoinInScale,

                bills_in_scale = DefaultSettingsUtils.defBillInScale,

                ticket_out_scale = DefaultSettingsUtils.defCoinOutCreditPerTicket,

                printer_out_scale = DefaultSettingsUtils.defPrintOutScale,

                money_meter_scale = DefaultSettingsUtils.moneyMeterScale,

                select_printer_number = 0,

                select_biller_number = 3,

                updated_at = time,
            }
        };
    }
}




/// <summary>
/// 彩金记录
/// </summary>
[System.Serializable]
public class TableJackpotRecordItem
{
    public long id;
    /// <summary> 玩家id </summary>
    public string user_id = "";
    /// <summary> 游戏id </summary>
    public long game_id;
    /// <summary> 那场游戏 </summary>
    public string game_uid;
    /// <summary> 彩金名称 </summary>
    public string jp_name;
    /// <summary> 彩金等级（1最大） </summary>
    public long jp_level;
    /// <summary> 获得多少积分 </summary>
    public long win_credit;
    /// <summary> 游戏前金额 </summary>
    public long credit_before;
    /// <summary> 游戏后金额 </summary>
    public long credit_after;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 创建时间 </summary>
    public long created_at;
}




/// <summary>
/// 拉霸-游戏记录
/// </summary>
[System.Serializable]

public class TableSlotGameRecordItem
{
    public long id;
    /// <summary> 玩家id </summary>
    public string user_id = "";
    /// <summary> 游戏id </summary>
    public long game_id;

    /// <summary> 游戏类型 </summary>
    public string game_type = "spin";  //【新版本用】 "spin", "free_spin"  "bonus_minigame"

    /// <summary> 游戏奖励类型 </summary>
    public string bonus_type = "";  //【新版本用】 "spin", "free_spin"  "bonus_minigame"

    /// <summary> 本剧游戏guid </summary>
    public string game_uid;
    /// <summary> 自定义场景数据（json） </summary>
    public string scene;
    /// <summary> 总压 </summary>
    public long total_bet;
    /// <summary> 压多少线 </summary>
    public int lines_played = -1;
    /// <summary> 基础游戏前金额 </summary>
    public long credit_before;
    /// <summary> "基础游戏 + 彩金"后金额  </summary>
    public long credit_after;
    /// <summary> 游戏结果 </summary>
    public string game_result = "";
    /// <summary> 总赢 </summary>
    //public long total_win_credit = -1;
    /// <summary> 基础游戏赢分 </summary>
    public long base_game_win_credit = 0;
    /// <summary> 免费游戏赢分 </summary>
    public long free_spin_win_credit = 0;
    /// <summary> 彩金赢分 </summary>
    public long jackpot_win_credit = 0;
    public string jackpot_type = "";

    /// <summary> 小游戏赢分 </summary>
    public long bonus_game_win_credit = 0;
    public string bonus_game_type = "";

    public long link_game_win_credit = 0;
    public long skill_game_win_credit = 0;
    /// <summary> 额外奖励分数 </summary>
    public long reward_credit = 0;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 创建时间 </summary>
    public long created_at;
}





/// <summary>
/// 推币机-游戏记录
/// </summary>
[System.Serializable]

public class TablePusherGameRecordItem
{
    public long id;
    /// <summary> 玩家id </summary>
    public string user_id = "";
    /// <summary> 游戏id </summary>
    public long game_id;

    /// <summary> 本局游戏类型 </summary>
    public string game_type = "spin";  //【新版本用】 "spin", "free_spin" "free_spin_add"  "bonus1" "bonus_minigame" 

    /// <summary> 游戏奖励类型 (暂时不用) </summary>
    public string bonus_type = "";  //

    /// <summary> 本剧游戏guid </summary>
    public string game_uid;

    /// <summary> 自定义场景数据（json） </summary>
    public string scene;

    /// <summary> 总压 </summary>
    public long total_bet;


    /// <summary> 一币多少分 </summary>
    public long credit_per_coin_in;


    /// <summary> 压多少线 </summary>
    public int lines_played = -1;

    /// <summary> 基础游戏前金额（暂时不用） </summary>
    public long credit_before = -1;

    /// <summary> "基础游戏 + 彩金"后金额（暂时不用） </summary>
    public long credit_after = -1; 

    /// <summary> 游戏结果 </summary>
    public string game_result = "";

    /// <summary> 总赢 </summary>
    //public long total_win_coins = -1;

    /// <summary> 基础游戏赢分 (可以是普通局 或 免费局)</summary>
    public long base_game_win_coins = 0;

    /// <summary> 免费游戏赢分 </summary>
    //public long free_spin_win_coins = 0;

    /// <summary> 彩金赢分 </summary>
    public long jackpot_win_coins = 0;
    public string jackpot_type = "";

    /// <summary> 小游戏赢分 </summary>
    public long bonus_game_win_coins = 0;
    public string bonus_game_type = "";

    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 创建时间 </summary>
    public long created_at;
}




/// <summary>
///日志记录
/// </summary>
[System.Serializable]
public class TableLogRecordItem
{
    public long id;
    /// <summary> 日志类型 </summary>
    public string log_type;
    /// <summary> 日志内容 </summary>
    public string log_content;
    /// <summary> 日志堆栈跟踪 </summary>
    public string log_stacktrace;
    /// <summary> 日志标签 </summary>
    public string log_tag;
    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";
    /// <summary> 创建时间 </summary>
    public long created_at;
}


/*
/// <summary>
/// 订单号管理
/// </summary>
[System.Serializable]
public class TableOrdeId
{
    public long id;

    /// <summary> 订单号的hash(MD5码)） </summary>
    public string hash; 

    /// <summary> 订单数据（OrderIdData对象） </summary>
    public string data;

    /// <summary> 创建时间 </summary>
    public long created_at;

    /// <summary> 密文 </summary>
    public string ciphertext;

}


[System.Serializable]
public class OrderIdData
{
    /// <summary> 点单号 </summary>
    public string order_id;

    /// <summary> 类型 </summary>
    public string order_type;

    /// <summary> 状态 0:订单创建  1:订单待完成   2:订单完成 </summary>
    public int state;

    /// <summary> 多少ms后过期  -1:不使用</summary>
    public long overtime_ms = -1;

    /// <summary> 创建时间 ms </summary>
    public long created_at;
}
*/


/// <summary>
/// 每日营收统计
/// </summary>
/// <remarks>
/// * 【这个即将弃用】 每日营销记录 和 总营销记录，将统计到同一张表中！
/// </remarks>
public class  TableBussinessDayRecordItem  
{
    public long id;

    /// <summary> 充值前 </summary>
    public long credit_before;
    /// <summary> 充值后 </summary>
    public long credit_after;
    /// <summary> 每日总押注 </summary>
    public long total_bet_credit;
    /// <summary> 每日总赢分 </summary>
    public long total_win_credit;
    /// <summary> 每日总投币 </summary>
    public long total_coin_in_credit;
    /// <summary> 每日总退币 </summary>
    public long total_coin_out_credit;
    /// <summary> 每日总上分 </summary>
    public long total_score_up_credit;
    /// <summary> 每日总下分 </summary>
    public long total_score_down_credit;

    /// <summary> 每日总入钞 </summary>
    public long total_bill_in_credit;

    /// <summary> 每日总入钞（多少钞） </summary>
    public long total_bill_in_as_money;

    /// <summary> 每日总打印（多少钞） </summary>
    public long total_printer_out_credit;

    /// <summary> 每日总打印</summary>
    public long total_printer_out_as_money;

    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";

    /// <summary> 刷新时间 </summary>
    public long created_at;

    /// <summary> 刷新时间 </summary>
    //public long update_at;
}




public class TableBussinessTotalRecordItem    //public class TableBussinessRecordItem
{
    public long id;

    public string bussiness_type;    // "history_bussiness"  "day_bussiness"

    /// <summary> 充值前 </summary>
    public long credit_before;
    /// <summary> 充值后 </summary>
    public long credit_after;
    /// <summary> 每日总押注 </summary>
    public long total_bet_credit;
    /// <summary> 每日总赢分 </summary>
    public long total_win_credit;
    /// <summary> 每日总投币 </summary>
    public long total_coin_in_credit;
    /// <summary> 每日总退币 </summary>
    public long total_coin_out_credit;
    /// <summary> 每日总上分 </summary>
    public long total_score_up_credit;
    /// <summary> 每日总下分 </summary>
    public long total_score_down_credit;

    /// <summary> 每日总入钞 </summary>
    public long total_bill_in_credit;

    /// <summary> 每日总入钞（多少钞） </summary>
    public long total_bill_in_as_money;

    /// <summary> 每日总打印（多少钞） </summary>
    public long total_printer_out_credit;

    /// <summary> 每日总打印</summary>
    public long total_printer_out_as_money;

    /// <summary> 自定义数据 </summary>
    public string custom_data = "{}";

    /// <summary> 刷新时间 </summary>
    public long created_at;

    /// <summary> 刷新时间 </summary>
    //public long update_at;

    public static TableBussinessTotalRecordItem[] DefaultTable()
    {
        long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new TableBussinessTotalRecordItem[] {
            new TableBussinessTotalRecordItem{
            bussiness_type = "history_bussiness",//  "day_bussiness"
            credit_before = 0,
            credit_after = 0,
            total_bet_credit = 0,
            total_win_credit = 0,
            total_coin_in_credit = 0,
            total_coin_out_credit = 0,
            total_score_up_credit = 0,
            total_score_down_credit = 0,
            total_bill_in_credit = 0,
            total_bill_in_as_money = 0,
            total_printer_out_credit = 0,
            total_printer_out_as_money = 0,
            custom_data = "{}",
            created_at = time,
            }
        };
    }
}




public static class ConsoleTableName
{
    //public const string DB_NAME = "PssOn00152.db";

        public static string DB_NAME => ApplicationSettings.Instance.dbName;


        public const string TABLE_SYS_SETTING = "system_setting";
        public const string TABLE_JACKPOT_RECORD = "jackpot_record";
        public const string TABLE_BET = "bet";
        /// <summary> 投退币记录</summary>
        public const string TABLE_COIN_IN_OUT_RECORD = "coin_in_out_record";
        /// <summary> 游戏游玩记录</summary>
        public const string TABLE_SLOT_GAME_RECORD = "slot_game_record";
        public const string TABLE_PUSHER_GAME_RECORD = "pusher_game_record";

        public const string TABLE_LOG_ERROR_RECORD = "log_error_record";
        public const string TABLE_LOG_EVENT_RECORD = "log_event_record";

        //public const string TABLE_ORDER_ID = "order_id";

        public const string TABLE_BUSINESS_DAY_RECORD = "bussiness_day_record";

        public const string TABLE_BUSINESS_TOTAL_RECORD = "bussiness_total_record";

        public const string TABLE_VER_CACHER = "TABLE_VER_CACHER";

        public static readonly Dictionary<string, string> tableVer = new Dictionary<string, string>()
        {
            [TABLE_SYS_SETTING] = "1.0.3",
            [TABLE_JACKPOT_RECORD] = "1.0.3",
            [TABLE_BET] = "1.0.3",
            [TABLE_COIN_IN_OUT_RECORD] = "1.0.3",
            [TABLE_SLOT_GAME_RECORD] = "1.0.3",
            [TABLE_PUSHER_GAME_RECORD] = "1.0.3",
            [TABLE_LOG_ERROR_RECORD] = "1.0.3",
            [TABLE_LOG_EVENT_RECORD] = "1.0.3",
            [TABLE_BUSINESS_DAY_RECORD] = "1.0.3",
            [TABLE_BUSINESS_TOTAL_RECORD] = "1.0.3",
            //[TABLE_ORDER_ID] = "1.0.3",
        };



}