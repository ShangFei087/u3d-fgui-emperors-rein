#define SQLITE_ASYNC
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;


/// <summary>
/// 日志系统
/// </summary>
/// <remark>
/// * 脚本用来筛选存储日志到本地
/// * 接入数据上报接口bugly
/// * 
/// </remark>
public class LogManager : MonoSingleton<LogManager>
{
    void Start()
    {
        Application.logMessageReceived += OnLogCallBack;
    }

    protected override void OnDestroy()
    {
        Application.logMessageReceived -= OnLogCallBack;
        base.OnDestroy();
    }

    const string MYSELF_LOG = "【LogMgr】";
    const string SAVE_LOG = "【Log】";
    /// <summary>
    /// 打印日志回调
    /// </summary>
    /// <param name="condition">日志文本</param>
    /// <param name="stackTrace">调用堆栈</param>
    /// <param name="type">日志类型</param>
    private void OnLogCallBack(string condition, string stackTrace, LogType type)
    {
        if (isApplicationQuit ||  condition.StartsWith(MYSELF_LOG))
            return;

        // 这条报错会导致黑屏
        if (condition.Contains("Screen position out of view"))  return;

        switch (type)
        {
            case LogType.Exception:
            case LogType.Error:
                {
                    try
                    {
         
                        if (CheckErrorRepeat(condition))
                            return;


#if !SQLITE_ASYNC

                        string sql = SQLiteHelper01.SQLInsertTableData<TableLogRecordItem>(
                            ConsoleTableName.TABLE_LOG_ERROR_RECORD,
                            new TableLogRecordItem()
                            {
                                log_type = Enum.GetName(typeof(LogType), type),
                                log_content = DoStr2base64str(condition ?? "--"),
                                log_stacktrace = DoStr2base64str(stackTrace ?? "--"),
                                log_tag = "",
                                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            });
                        SQLiteHelper01.Instance.ExecuteNonQueryAfterOpenDB(ConsoleTableName.DB_NAME, sql);
#else
                        string sql = SQLiteAsyncHelper.SQLInsertTableData<TableLogRecordItem>(
                            ConsoleTableName.TABLE_LOG_ERROR_RECORD,
                            new TableLogRecordItem()
                            {
                                log_type = Enum.GetName(typeof(LogType), type),
                                log_content = DoStr2base64str(condition ?? "--"),
                                log_stacktrace = DoStr2base64str(stackTrace ?? "--"),
                                log_tag = "",
                                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            });
                        SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);

#endif


                    }
                    catch (Exception e) {
                        Debug.LogWarning($"{MYSELF_LOG}condition = {condition}");
                        Debug.LogWarning($"{MYSELF_LOG}stackTrace = {stackTrace}");
                        Debug.LogError($"{MYSELF_LOG}{e}");              
                    }
                }
                break;
            case LogType.Warning:
            case LogType.Log:
                {
                    try
                    {
                        if (condition != null && condition.StartsWith(SAVE_LOG))
                        {

#if !SQLITE_ASYNC

                            string sql = SQLiteHelper01.SQLInsertTableData<TableLogRecordItem>(
                                ConsoleTableName.TABLE_LOG_EVENT_RECORD,
                                new TableLogRecordItem()
                                {
                                    log_type = Enum.GetName(typeof(LogType), type),
                                    log_content = DoStr2base64str(condition.Substring(SAVE_LOG.Length)),
                                    log_stacktrace = DoStr2base64str(stackTrace ?? "--"),
                                    log_tag = "",
                                    created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                });
                            SQLiteHelper01.Instance.ExecuteNonQueryAfterOpenDB(ConsoleTableName.DB_NAME, sql);
#else
                            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableLogRecordItem>(
                                ConsoleTableName.TABLE_LOG_EVENT_RECORD,
                                new TableLogRecordItem()
                                {
                                    log_type = Enum.GetName(typeof(LogType), type),
                                    log_content = DoStr2base64str(condition.Substring(SAVE_LOG.Length)),
                                    log_stacktrace = DoStr2base64str(stackTrace ?? "--"),
                                    log_tag = "",
                                    created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                });
                            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
#endif                  


                        }
                    }
                    catch (Exception e){ Debug.LogError($"{MYSELF_LOG}{e}"); }
                }
                break;
        }

    }

    bool isApplicationQuit = false;
    void OnApplicationQuit()
    {
        isApplicationQuit = true;
    }

    public string DoStr2base64str(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    public string Dobase64str2Str(string base64Encoded) => Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));





    #region 防止错误日志,爆内存

    List<int> repeatLogblacklist = new List<int>();
    Dictionary<int,RepeatLogInfo> checkErrorRepeatLogInfos = new Dictionary<int, RepeatLogInfo>();
    List<int> checkErrorRepeatLogs = new List<int>();

    bool CheckErrorRepeat(string content)
    {
        string textTemplate = ReplaceNumbersWithPlaceholder(content);

        int hash = textTemplate.GetHashCode();
        //int hash = content.GetHashCode();
        if (repeatLogblacklist.Contains(hash))
            return true;

        if (!checkErrorRepeatLogs.Contains(hash))
        {
            RepeatLogInfo info = new RepeatLogInfo()
            {
                runTimeS = Time.unscaledTime,
                content = content,
            };
            checkErrorRepeatLogInfos.Add(hash, info);
            checkErrorRepeatLogs.Insert(0, hash);
        }
        checkErrorRepeatLogInfos[hash].count++;

        return false;
    }


    float gapTimeS = 3f;
    float gapCount = 15;
    bool isDirty = true;
    private void Update()
    {
        if (isDirty)
        {
            isDirty = false;

            if (checkErrorRepeatLogs.Count > 0)
            {
                int idx = checkErrorRepeatLogs.Count;
                while (--idx >=0)
                {
                    int hash = checkErrorRepeatLogs[idx];

                    RepeatLogInfo target = checkErrorRepeatLogInfos[hash];
                    if (Time.unscaledTime - target.runTimeS >= gapTimeS)
                    {
                        checkErrorRepeatLogInfos.Remove(hash);
                        checkErrorRepeatLogs.Remove(hash);
                        if (target.count >= gapCount)
                        {
                            repeatLogblacklist.Add(hash);

                            // 存入数据库：
                            string sql = SQLiteAsyncHelper.SQLInsertTableData<TableLogRecordItem>(
                            ConsoleTableName.TABLE_LOG_EVENT_RECORD,
                            new TableLogRecordItem()
                            {
                                log_type = Enum.GetName(typeof(LogType), LogType.Warning),
                                log_content = DoStr2base64str($"{MYSELF_LOG}警告: {gapTimeS}秒内，存{target.count}条记录。 内容: {target.content}"),
                                log_stacktrace = DoStr2base64str("--"),
                                log_tag = "",
                                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            });
                            SQLiteAsyncHelper.Instance.ExecuteNonQueryAsync(sql);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            isDirty = true;
        }
    }





    /// <summary>
    /// 将字符串中的所有数字（整数、小数、科学计数法）替换为 {N}
    /// 适配场景：716.000000、41.267822、123、-456、1.23e+003、-1.23E-004、.567、123.
    /// </summary>
    /// <param name="input">原始字符串</param>
    /// <returns>替换后的字符串</returns>
    /// <remarks>
    /// * 只基于「固定文本模板」判断是否重复。
    /// * 对日志内容进行「模板提取」，将可变参数替换为占位符，再基于模板生成去重。
    /// </remarks>
    public string ReplaceNumbersWithPlaceholder(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 只有数字
        if(FullNumberRegex.IsMatch(input.Trim()))
            return input;

        // 替换所有匹配的数字为 {N}
        return NumberRegex.Replace(input, "{N}");
    }


    // 正则表达式解释：
    // [-+]?                          → 可选的正负号
    // (?:\d{1,3}(?:,\d{3})*|\d+)     → 整数部分（支持千位分隔符）：
    //                                   - \d{1,3}(?:,\d{3})*：1-3位数字 + 可选的（,3位数字）重复（如 1,236 或 1,234,567）
    //                                   - \d+：无千位分隔符的整数（如 123 或 123456）
    // (?:\.\d*)?                     → 可选的小数部分（如 .99、.000000 或 空，支持 123. 格式）
    // (?:[eE][-+]?\d+)?              → 可选的科学计数法部分（e/E + 可选正负号 + 指数数字）
    const string numberPattern = @"[-+]?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d*)?(?:[eE][-+]?\d+)?";
    // 预编译正则（静态类中首次调用时编译，后续复用，提升性能）
    Regex NumberRegex = new Regex(numberPattern, RegexOptions.Compiled);


    // 只有数字
    Regex FullNumberRegex = new Regex(
        @"^[-+]?(?:\d{1,3}(?:,\d{3})*|\d+)(?:\.\d*)?(?:[eE][-+]?\d+)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );
    #endregion





#region 测试
    [Button]
    void TestErrorShowStr()
    {

        //string intStr = "abc 2,555.9977 - +1,236.99 - @".Replace("@", UnityEngine.Random.Range(0,100).ToString());
        string intStr = "Screen position out of view frustum (screen pos 716.000000, 41.267822)(Camera rect 0 0 1280 720)";
        Debug.Log(intStr);
        string str = ReplaceNumbersWithPlaceholder(intStr);
        Debug.Log(str);
    }


    [Button]
    void TestErrorShowStr02()
    {
        const string numberPattern = @"[-+]?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d*)?(?:[eE][-+]?\d+)?";
        Regex NumberRegex = new Regex(numberPattern, RegexOptions.Compiled);

        string intStr = "1280";// "Screen position out of view frustum (screen pos 716.000000, 41.267822)(Camera rect 0 0 1280 720)";
        Debug.Log(intStr);

        string result = NumberRegex.Replace(intStr, "{N}");

        Debug.Log(result);
    }



    [Button]
    void TestErrorShowStr03()
    {
        string intStr = "999";
        Debug.Log(intStr);
        string str = ReplaceNumbersWithPlaceholder(intStr);
        Debug.Log(str);
    }



    [Button]
    void TestErrorRepeatRecord()
    {
        StartCoroutine(CoTestErrorRepeatRecord());
    }
    IEnumerator CoTestErrorRepeatRecord()
    {
        int count = 30;
        while (--count>0)
        {
            Debug.LogError("@@ TestErrorRepeatRecord .....!!!");
            yield return null;
        }
    }

#endregion








}

public class RepeatLogInfo
{
    public float runTimeS;
    public int count = 0;
    public string content;
}