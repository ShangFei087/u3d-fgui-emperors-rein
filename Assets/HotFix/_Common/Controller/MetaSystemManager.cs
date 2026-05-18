using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using GameMaker;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using FairyGUI;



public class MetaSystemManager : MonoSingleton<MetaSystemManager>
{
    // Start is called before the first frame update
    void OnEnable()
    {

        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_TOOL_EVENT, OnToolEvent);

        //TestManager.Instance.Init();

        ResourceManager02.Instance.LoadAsset<TextAsset>("Assets/GameRes/_Common/Game Maker/ABs/Datas/tmg_page.json", (TextAsset txt) =>
        {
            TestManager.Instance.SetKV(TestManager.DATA_PAGES, txt.text);
        });  

        ResourceManager02.Instance.LoadAsset<TextAsset>("Assets/GameRes/_Common/Game Maker/ABs/Datas/tmg_custom_button.json", (TextAsset txt) =>
        {
            TestManager.Instance.SetKV(TestManager.DATA_CUSTOM_BUTTON, txt.text);
        });

        AnalysisTest(null);
    }

    // Update is called once per frame
    void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_TOOL_EVENT, OnToolEvent);
    }


    void OnToolEvent(EventData res)
    {
        switch (res.name)
        {
            case GlobalEvent.AnalysisTest:
                {
                    AnalysisTest(res);
                }
                break;
            case GlobalEvent.PageButton:
                {
                    OnClickPageBtn(res);
                }
                break;
            case GlobalEvent.ApplicationQuit:
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false; // 编辑器中退出播放模式
#else
                    Application.Quit(); // 构建后退出应用
#endif
                }
                break;

            case GlobalEvent.CustomButtonCoinIn:
                {
                    OnClickCustomButtonCoinIn(res);
                }
                break;
            case GlobalEvent.CustomButtonTicketOut:
                {
                    OnClickCustomButtonTicketOut(res);
                }
                break;
            case GlobalEvent.CustomButtonCreditUp:
                {
                    OnClickCustomButtonCreditUp(res);
                }
                break;
            case GlobalEvent.CustomButtonCreditDown:
                {
                    OnClickCustomButtonCreditDown(res);
                }
                break;
            case GlobalEvent.SlotGameRecord:
                OnSlotGameRecordPrint(res);
                break;
            case GlobalEvent.ShowTableLastData:
                OnShowTableLastData(res);
                break;
            case GlobalEvent.DeviceTestPrintTicket:
                OnClickDeviceTestPrintTicket(res);
                break;

        }
    }

    /// <summary>
    /// 测试菜单「游戏记录打印」
    /// event_data JSON：
    /// last_count：缺省、≤0 表示导出该 game_id 在库中的**全部**记录；指定正数时仅导出最近 N 条（按 id 从新到旧取 N 条后再按时间升序写出）。
    /// game_id：≥0 为指定机台；缺省或 -1 表示用「当前 MainModel.gameID」；若当前未进机台（gameID&lt;0）则自动用库中最新一条记录的 game_id；
    /// credit_divisor（默认 1）、
    /// col_a：label（默认：仅 赢/输/免费/大奖/彩金 五种）/ id / row_index、
    /// merge_free_give（默认 true：免费触发+连续赠送局合并一行，A 列为「免费」）、
    /// big_win_multiple（默认 15，已保留兼容；「大奖」由 result_type=BonusWin 判定）、include_raw_tsv（默认 true）、
    /// summary_block2_title（可选：着色表 .xml 中第二块分档标题，如 FIREBIRD；缺省「大奖」）。
    /// 导出除 _record.csv 外会生成 _record.xml（Excel 打开，含单元格颜色；汇总锚定在最后一条游戏记录右侧）。
    /// </summary>
    void OnSlotGameRecordPrint(EventData res)
    {
        string json = res.value as string ?? "{}";
        JObject jo;
        try
        {
            jo = JObject.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        }
        catch (Exception e)
        {
            DebugUtils.LogError($"[SlotGameRecord] event_data 非合法 JSON: {e.Message}");
            return;
        }

        int? maxExportRows = null;
        if (jo["last_count"] != null && jo["last_count"].Type != JTokenType.Null)
        {
            var v = (int)jo["last_count"];
            if (v > 0)
                maxExportRows = Mathf.Clamp(v, 1, 10_000_000);
        }

        int? explicitGameId = null;
        if (jo["game_id"] != null && jo["game_id"].Type != JTokenType.Null)
        {
            var v = (int)jo["game_id"];
            if (v >= 0)
                explicitGameId = v;
        }

        double creditDivisor = 1.0;
        if (jo["credit_divisor"] != null && jo["credit_divisor"].Type != JTokenType.Null)
        {
            var d = jo["credit_divisor"].Value<double>();
            if (d > 0)
                creditDivisor = d;
        }

        bool includeRawTsv = true;
        if (jo["include_raw_tsv"] != null && jo["include_raw_tsv"].Type != JTokenType.Null)
            includeRawTsv = jo["include_raw_tsv"].Value<bool>();
        var colAMode = SlotGameRecordColAExportMode.AwardLabel;
        if (jo["col_a"] != null && jo["col_a"].Type != JTokenType.Null)
        {
            var ca = jo["col_a"].Value<string>();
            if (string.Equals(ca, "id", StringComparison.OrdinalIgnoreCase))
                colAMode = SlotGameRecordColAExportMode.RowId;
            else if (string.Equals(ca, "row_index", StringComparison.OrdinalIgnoreCase))
                colAMode = SlotGameRecordColAExportMode.RowIndex;
        }

        bool mergeFreeGive = true;
        if (jo["merge_free_give"] != null && jo["merge_free_give"].Type != JTokenType.Null)
            mergeFreeGive = jo["merge_free_give"].Value<bool>();

        double bigWinMultiple = 15.0;
        if (jo["big_win_multiple"] != null && jo["big_win_multiple"].Type != JTokenType.Null)
        {
            var bm = jo["big_win_multiple"].Value<double>();
            if (bm > 0)
                bigWinMultiple = bm;
        }

        string summaryBlock2Title = null;
        if (jo["summary_block2_title"] != null && jo["summary_block2_title"].Type != JTokenType.Null)
        {
            var t = jo["summary_block2_title"].Value<string>();
            if (!string.IsNullOrWhiteSpace(t))
                summaryBlock2Title = t.Trim();
        }

        if (SQLiteAsyncHelper.Instance == null || !SQLiteAsyncHelper.Instance.isConnect)
        {
            DebugUtils.LogWarning("[SlotGameRecord] SQLite 未就绪");
            TipPopupHandler.Instance?.OpenPopupOnce("游戏记录导出失败：数据库未就绪");
            return;
        }

        string table = ConsoleTableName.TABLE_SLOT_GAME_RECORD;

        void DoExport(int gid)
        {
            string sql;
            if (maxExportRows.HasValue)
            {
                var n = maxExportRows.Value;
                sql = $@"
SELECT id, game_id, total_bet, credit_before, credit_after, base_game_win_credit, free_spin_win_credit, bonus_game_win_credit, jackpot_win_credit, total_win_credit, open_type, result_type, free_curtime, free_totaltime, created_at
FROM (
  SELECT id, game_id, total_bet, credit_before, credit_after, base_game_win_credit, free_spin_win_credit, bonus_game_win_credit, jackpot_win_credit, total_win_credit, open_type, result_type, free_curtime, free_totaltime, created_at
  FROM {table}
  WHERE game_id = {gid}
  ORDER BY id DESC
  LIMIT {n}
) t
ORDER BY id ASC";
            }
            else
            {
                sql = $@"
SELECT id, game_id, total_bet, credit_before, credit_after, base_game_win_credit, free_spin_win_credit, bonus_game_win_credit, jackpot_win_credit, total_win_credit, open_type, result_type, free_curtime, free_totaltime, created_at
FROM {table}
WHERE game_id = {gid}
ORDER BY id ASC";
            }

            SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, (DataTable dt) =>
            {
                var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var baseName = $"slot_game_record_g{gid}_{stamp}";
                var dir = Application.persistentDataPath;
                var tip = new StringBuilder();

                try
                {
                    var sorted = SlotGameRecordExport.SortRowsChronological(dt);
                    var exportData = SlotGameRecordExport.BuildExportData(sorted, creditDivisor, colAMode, mergeFreeGive,
                        bigWinMultiple);
                    var recordCsv = SlotGameRecordExport.FormatSlotGameRecordCsv(exportData);
                    var recordCsvPath = Path.Combine(dir, $"{baseName}_record.csv");
                    File.WriteAllText(recordCsvPath, recordCsv, Encoding.UTF8);
                    var recordXmlPath = Path.Combine(dir, $"{baseName}_record.xml");
                    SlotGameRecordSpreadsheetXml.WriteStyledWorkbook(recordXmlPath, exportData,
                        summaryBlock2Title);
                    var csvLines = exportData.GameRows.Count;
                    DebugUtils.Log(
                        $"[SlotGameRecord] game_id={gid} 库记录 {dt.Rows.Count} 条 -> CSV {csvLines} 行 + 着色表 {recordXmlPath}");
                    tip.AppendLine($"game_id={gid}，库 {dt.Rows.Count} 条，导出 {csvLines} 行（含免费合并）");
                    tip.AppendLine($"CSV:\n{recordCsvPath}");
                    tip.AppendLine($"着色汇总（Excel 打开）:\n{recordXmlPath}");

                    if (includeRawTsv)
                    {
                        var rawBody = FormatDataTableAsTsv(dt);
                        var rawPath = Path.Combine(dir, $"{baseName}_raw.tsv");
                        File.WriteAllText(rawPath, rawBody, Encoding.UTF8);
                        DebugUtils.Log($"[SlotGameRecord] 原始 TSV -> {rawPath}");
                        tip.AppendLine($"原始 TSV:\n{rawPath}");
                    }

                    TipPopupHandler.Instance?.OpenPopupOnce(tip.ToString().TrimEnd());
                }
                catch (Exception e)
                {
                    DebugUtils.LogError($"[SlotGameRecord] 写文件失败: {e.Message}");
                    TipPopupHandler.Instance?.OpenPopupOnce($"游戏记录导出失败：{e.Message}");
                }
            });
        }

        if (explicitGameId.HasValue)
        {
            DoExport(explicitGameId.Value);
            return;
        }

        var cur = MainModel.Instance.gameID;
        if (cur >= 0)
        {
            DoExport(cur);
            return;
        }

        var sqlPick = $"SELECT game_id FROM {table} ORDER BY id DESC LIMIT 1";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sqlPick, (DataTable pick) =>
        {
            if (pick == null || pick.Rows.Count == 0)
            {
                DebugUtils.LogWarning("[SlotGameRecord] 库中无记录，无法解析 game_id");
                TipPopupHandler.Instance?.OpenPopupOnce("游戏记录：库中尚无数据，请先玩游戏产生记录");
                return;
            }

            var gid = Convert.ToInt32(pick.Rows[0]["game_id"], CultureInfo.InvariantCulture);
            DebugUtils.Log($"[SlotGameRecord] 当前未进机台，使用库内最新一条记录的 game_id={gid}");
            DoExport(gid);
        });
    }

    /// <summary>
    /// 通用：打印最近 N 条表数据（event_data: table_name, last_count）。表名白名单防注入。
    /// </summary>
    void OnShowTableLastData(EventData res)
    {
        string json = res.value as string ?? "{}";
        JObject jo;
        try
        {
            jo = JObject.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        }
        catch (Exception e)
        {
            DebugUtils.LogError($"[ShowTableLastData] event_data 非合法 JSON: {e.Message}");
            return;
        }

        string tableName = jo["table_name"]?.Value<string>();
        if (string.IsNullOrEmpty(tableName))
        {
            DebugUtils.LogWarning("[ShowTableLastData] 缺少 table_name");
            return;
        }

        if (!_showTableWhitelist.Contains(tableName))
        {
            DebugUtils.LogError($"[ShowTableLastData] 不允许的表名: {tableName}");
            return;
        }

        int lastCount = jo["last_count"] != null ? Mathf.Clamp((int)jo["last_count"], 1, 500) : 30;
        if (SQLiteAsyncHelper.Instance == null || !SQLiteAsyncHelper.Instance.isConnect)
        {
            DebugUtils.LogWarning("[ShowTableLastData] SQLite 未就绪");
            return;
        }

        string sql = $"SELECT * FROM {tableName} ORDER BY id DESC LIMIT {lastCount}";
        SQLiteAsyncHelper.Instance.ExecuteQueryAsync(sql, (DataTable dt) =>
        {
            string body = FormatDataTableAsTsv(dt);
            string safe = tableName.Replace('.', '_');
            string path = Path.Combine(Application.persistentDataPath,
                $"table_{safe}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            try
            {
                File.WriteAllText(path, body, Encoding.UTF8);
                DebugUtils.Log($"[ShowTableLastData] {tableName} {dt.Rows.Count} 行 -> {path}");
            }
            catch (Exception e)
            {
                DebugUtils.LogError($"[ShowTableLastData] 写文件失败: {e.Message}\n{body}");
            }
        });
    }

    static readonly HashSet<string> _showTableWhitelist = new HashSet<string>
    {
        ConsoleTableName.TABLE_BUSINESS_DAY_RECORD,
        ConsoleTableName.TABLE_COIN_IN_OUT_RECORD,
        ConsoleTableName.TABLE_SLOT_GAME_RECORD,
    };

    static string FormatDataTableAsTsv(DataTable dt)
    {
        if (dt == null || dt.Rows.Count == 0)
            return "(无数据)\n";

        var sb = new StringBuilder();
        for (int c = 0; c < dt.Columns.Count; c++)
        {
            if (c > 0) sb.Append('\t');
            sb.Append(dt.Columns[c].ColumnName);
        }

        sb.AppendLine();
        foreach (DataRow row in dt.Rows)
        {
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                if (c > 0) sb.Append('\t');
                var v = row[c];
                sb.Append(v == null || v == DBNull.Value ? "" : v.ToString());
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void OnClickDeviceTestPrintTicket(EventData res)
    {
        DevicePrinterOut.Instance.DoPrinterOut();
    }

    public void OnClickCustomButtonCoinIn(EventData data)
    {
        EventCenter.Instance.EventTrigger<CoinInData>(SBoxSanboxEventHandle.COIN_IN,
            new CoinInData()
            {
                id = 0,
                coinNum = 1,
            });
    }
    public void OnClickCustomButtonTicketOut(EventData data)
    {
        MachineDeviceCommonBiz.Instance.TestTicketOut();
    }

    public void OnClickCustomButtonCreditUp(EventData data)
    {
        DeviceCreditUpDown.Instance.CreditUp();
    }
    public void OnClickCustomButtonCreditDown(EventData data)
    {
        DeviceCreditUpDown.Instance.CreditDown();
    }



    public void AnalysisTest(EventData res = null)
    {
        GCMonitorPro comp = GetComponentInChildren<GCMonitorPro>();
        if(comp != null)
        {
            if(res == null)
                comp.enabled = false;
            else 
                comp.enabled = (bool)res.value;
        }
    }


    public void OnClickPageBtn(EventData data)
    {

        Dictionary<string, object> res = (Dictionary<string, object>)data.value;

        string pgName = (string)res["pageName"];
        string pgData = (string)res["pageData"];

        DebugUtils.Log($" name = {pgName}   value = { JsonConvert.SerializeObject(pgData)} ");

        PageName pageName = (PageName)Enum.Parse(typeof(PageName), pgName);

        /*if (pageName == PageName.ConsolePageConsoleMain)
        {
            MachineDeviceCommonBiz.Instance.OpenConsole();
        }
        else
        {
            if (PageManager.Instance.IndexOf(pageName) != -1)
            {
                PageManager.Instance.ClosePage(pageName);
            }
            else
            {
                PageManager.Instance.OpenPage(pageName);
            }
        }*/

        if (PageManager.Instance.IndexOf(pageName) != -1)
        {
            PageManager.Instance.ClosePage(pageName);
        }
        else
        {
            PageManager.Instance.OpenPage(pageName);
        }

    }


    GTweener tweener;

    [Button]
    void TestTween()
    {
        // 这里可能换成Dotween
        tweener = GTween.To(1, 10, 3)
            .SetEase(EaseType.Linear)  // 设置缓动函数
            .OnUpdate((GTweener tweener) =>
            {
                // 每次更新时调用
                //target.y = tweener.value.x;

                DebugUtils.Log($"[Tween] cur:{tweener.value.x}");
            })
            .OnComplete(() =>
            {
                //action?.Invoke();
               DebugUtils.Log("Tween complete!");
            });
    }

    [Button]
    void TestStopTween()
    {
        // if (tweener != null) 
        tweener.Kill();
        //GTween.Kill(tweener);   
    }

}
