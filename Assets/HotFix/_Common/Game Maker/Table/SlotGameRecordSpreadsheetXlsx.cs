using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GameMaker
{
    /// <summary>
    /// 将游戏记录导出为 Office Open XML（.xlsx），供 WPS / Excel 打开。
    /// 左侧 A～L 为局数据（部分列为公式）；右侧 M 列起为着色汇总区，数值全部由表格公式计算。
    /// </summary>
    public static class SlotGameRecordSpreadsheetXlsx
    {
        // —— 游戏数据列（1-based，与表头一致）——
        const int ColAward = 1;          // A 大奖统计：赢/输/免费/大奖/彩金
        const int ColCreditBefore = 2;   // B 下注前
        const int ColBet = 3;            // C 下注
        const int ColBaseWin = 4;        // D 基础游戏得分
        const int ColFreeWin = 5;        // E 免费游戏得分
        const int ColBonusWin = 6;       // F 大奖游戏得分（Bonus 玩法）
        const int ColJackpot = 7;        // G 彩金得分
        const int ColTotalWin = 8;       // H 总得分（公式：SUM(D:G)）
        const int ColDelta = 9;          // I 输赢（公式：结束-下注前）
        const int ColCreditAfter = 10;   // J 结束
        const int ColMult = 11;          // K 倍数（公式：总得分/下注，供分档汇总引用）
        const int ColJackpotType = 12;   // L 彩金类型（0=Major，1=Minor，2=Mini；非彩金为空）

        // —— 顶部汇总（M～U：横排指标）——
        const int SummaryColStart = 13;      // M 统计项 / 类型
        const int SummaryColRounds = 14;     // N 总局及各类汇总数值列
        const int SummaryColSumBet = 15;     // O 总玩分
        const int SummaryColSumWin = 16;     // P 总得分
        const int SummaryColRtpTotal = 17;   // Q 合计 RTP
        const int SummaryColRtpBig = 18;     // R 大奖 RTP
        const int SummaryColRtpNormal = 19;  // S 普通游戏 RTP
        const int SummaryColRtpFree = 20;    // T 免费 RTP
        const int SummaryColRtpJackpot = 21; // U 彩金游戏 RTP

        // —— 分档明细列（M～S，与 SummaryColStart 同列区不同行）——
        const int DetailColType = 13;   // M 类型 / 分档名
        const int DetailColCount = 14;  // N 局
        const int DetailColBet = 15;    // O 总玩（本段 C 列押注之和）
        const int DetailColWin = 16;    // P 赢分
        const int DetailColAvg = 17;    // Q 平均（倍）= 赢分 ÷ 总玩
        const int DetailColProb = 18;   // R 出现概率
        const int DetailColRtp = 19;    // S RTP 返还率（赢分 ÷ 全局总玩分）

        /// <summary>汇总区起始列号（M），供外部引用。</summary>
        public const int SummaryStartColumn = SummaryColStart;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static readonly string[] _GameHeader =
        {
            "大奖统计", "下注前", "下注", "基础游戏得分", "免费游戏得分", "大奖游戏得分",
            "彩金得分", "总得分", "输赢", "结束", "倍数", "彩金类型",
        };

        /// <summary>jackpot_type 0/1/2 → Major / Minor / Mini。</summary>
        static readonly (string Code, string Label)[] _JackpotTypeRows =
        {
            ("0", "Major"),
            ("1", "Minor"),
            ("2", "Mini"),
        };

        /// <summary>普通（仅「赢」局）倍数分档：左闭右开，最后一档无上界。</summary>
        static readonly (double Lo, double? Hi)[] _BucketNormal =
        {
            (0, 1), (1, 2), (2, 5), (5, 10), (10, 20), (20, null),
        };

        /// <summary>大奖玩法倍数分档（200～600 倍）。</summary>
        static readonly (double Lo, double? Hi)[] _BucketBigWin =
        {
            (0, 200), (200, 300), (300, 400), (400, 500), (500, 600), (600, null),
        };

        /// <summary>免费玩法倍数分档（60～150 倍）。</summary>
        static readonly (double Lo, double? Hi)[] _BucketFree =
        {
            (0, 60), (60, 80), (80, 100), (100, 120), (120, 150), (150, null),
        };

        const string NsMain =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        const string NsR =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        const string NsPkg =
            "http://schemas.openxmlformats.org/package/2006/relationships";

        /// <summary>
        /// 写出带样式与公式的 xlsx。仅写入原始分项得分；总得分/输赢/倍数及全部汇总由公式生成。
        /// </summary>
        /// <param name="path">目标路径，非 .xlsx 后缀会自动补上。</param>
        /// <param name="data">由 <see cref="SlotGameRecordExport.BuildExportData"/> 构建的导出行。</param>
        /// <param name="summaryBlock2Title">汇总区「大奖」分档块标题（如 FIREBIRD），用于 SUMIF 匹配 A 列。</param>
        public static void WriteStyledWorkbook(string path, SlotGameRecordExport.SlotGameRecordExportData data,
            string summaryBlock2Title)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (!path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                path += ".xlsx";

            var block2 = string.IsNullOrWhiteSpace(summaryBlock2Title) ? "大奖" : summaryBlock2Title.Trim();
            var gr = data.GameRows;

            // xlsx 本质是 ZIP 包，内嵌 workbook / sheet / styles / sharedStrings
            using var fs = File.Create(path);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);

            var sst = new SharedStringTable();
            var sheet = new SheetBuilder(sst);

            // 样式索引（对应 styles.xml 中 cellXfs 下标）
            var stGh = 1;   // 表头蓝底白字
            var stGd = 2;   // 数据行白底
            var stGdZ = 3;  // 数据行斑马纹
            var stSxL = 4;  // 汇总标题深蓝
            var stSxV = 5;  // 汇总数值行
            var stDh = 6;   // 分档表头灰底
            var stSec = 7;  // 分块标题橙底
            var stSd = 8;   // 分档明细
            var stSdZ = 9;  // 分档明细斑马纹
            var stSt = 10;  // 小计绿底
            var stTot = 11; // 合计深绿

            var row = 1;
            sheet.BeginRow(row);
            for (var c = 0; c < _GameHeader.Length; c++)
                sheet.Str(c + 1, stGh, _GameHeader[c]);
            sheet.EndRow();
            row++;

            if (gr.Count == 0)
            {
                WritePackage(zip, sheet, sst);
                return;
            }

            // 第 1 行为表头；游戏数据从第 2 行起
            var firstDataRow = 2;
            var lastDataRow = 1 + gr.Count;
            // 汇总「数值」行紧挨最后一条游戏记录下一行
            var summaryValuesRow = lastDataRow + 1;

            for (var i = 0; i < gr.Count - 1; i++)
            {
                AppendGameRow(sheet, gr[i], row, (row & 1) == 0 ? stGd : stGdZ);
                row++;
            }

            AppendGameRowWithSummaryHead(sheet, gr[gr.Count - 1], row, (row & 1) == 0 ? stGd : stGdZ, stSxL);
            row++;

            AppendFormulaSummary(sheet, ref row, block2, firstDataRow, lastDataRow, summaryValuesRow,
                stSxV, stDh, stSec, stSd, stSdZ, stSt, stTot);

            WritePackage(zip, sheet, sst);
        }

        /// <summary>将 OOXML 各部件写入 ZIP 包。</summary>
        static void WritePackage(ZipArchive zip, SheetBuilder sheet, SharedStringTable sst)
        {
            WriteEntry(zip, "[Content_Types].xml", ContentTypes());
            WriteEntry(zip, "_rels/.rels", RootRels());
            WriteEntry(zip, "xl/workbook.xml", WorkbookXml());
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRels());
            WriteEntry(zip, "xl/styles.xml", StylesXml());
            WriteEntry(zip, "xl/sharedStrings.xml", sst.ToXml());
            WriteEntry(zip, "xl/worksheets/sheet1.xml", sheet.ToXml());
        }

        static void WriteEntry(ZipArchive zip, string name, string xml)
        {
            var e = zip.CreateEntry(name, CompressionLevel.Optimal);
            using var sw = new StreamWriter(e.Open(), new UTF8Encoding(false));
            sw.Write(xml);
        }

        /// <summary>最后一条游戏行：左侧为局数据，右侧 L 列起写汇总表头（统计项/总局/…）。</summary>
        static void AppendGameRowWithSummaryHead(SheetBuilder sheet, SlotGameRecordExport.SlotGameRecordCsvRow g,
            int row, int stData, int stSxL)
        {
            sheet.BeginRow(row);
            AppendGameRowCells(sheet, g, stData, row);
            var labels = GetTopSummaryLabels();
            for (var i = 0; i < labels.Length; i++)
                sheet.Str(SummaryColStart + i, stSxL, labels[i]);
            sheet.EndRow();
        }

        static void AppendGameRow(SheetBuilder sheet, SlotGameRecordExport.SlotGameRecordCsvRow g, int row,
            int stData)
        {
            sheet.BeginRow(row);
            AppendGameRowCells(sheet, g, stData, row);
            sheet.EndRow();
        }

        /// <summary>
        /// 写入一行局数据。D～G 为库表原始分项；H/I/K 为 Excel 公式，便于在 WPS 中改分项后自动重算。
        /// </summary>
        static void AppendGameRowCells(SheetBuilder sheet, SlotGameRecordExport.SlotGameRecordCsvRow g, int stData,
            int row)
        {
            sheet.Str(ColAward, stData, g.ColA);
            sheet.Num(ColCreditBefore, stData, g.CreditBefore);
            sheet.Num(ColBet, stData, g.TotalBet);
            sheet.Num(ColBaseWin, stData, g.BaseGameWin);
            sheet.Num(ColFreeWin, stData, g.FreeSpinWin);
            sheet.Num(ColBonusWin, stData, g.BonusGameWin);
            if (g.JackpotWin == 0)
                sheet.Str(ColJackpot, stData, "");
            else
                sheet.Num(ColJackpot, stData, g.JackpotWin);

            // 下列变量为当前行 A1 单元格引用，供下方公式字符串拼接（row 为 1-based 行号，与 Excel 一致）。
            var c = Cell(ColBet, row);              // C 列：本局下注
            var h = Cell(ColTotalWin, row);         // H 列：总得分（由公式算出，供 K 列倍数引用）
            var b = Cell(ColCreditBefore, row);     // B 列：下注前余额
            var j = Cell(ColCreditAfter, row);      // J 列：结束余额（先写公式再写数值，公式引用同列 J）
            var winFrom = Cell(ColBaseWin, row);    // D 列：基础游戏得分（SUM 区间起点）
            var winTo = Cell(ColJackpot, row);      // G 列：彩金得分（SUM 区间终点；G 为空时 SUM 忽略空单元格）

            // H 总得分 = 四项得分之和（D 基础 + E 免费 + F 大奖 + G 彩金），与表头「总得分」一致。
            // 示例第 2 行：SUM(D2:G2)。不在 C# 里写死合计，便于 WPS 中改 D～G 后自动重算。
            sheet.Fml(ColTotalWin, stData, $"SUM({winFrom}:{winTo})");

            // I 输赢 = 结束余额 − 下注前余额（J − B），表示本局对钱包的净变动（含下注与派彩）。
            // 与 A 列「赢/输」文案不同：A 列按 base_game_win_credit 判主游戏赢输；I 列为数值差。
            sheet.Fml(ColDelta, stData, $"{j}-{b}");

            // J 结束：写入库表 credit_after（缩放后），供 I 列公式与人工核对。
            sheet.Num(ColCreditAfter, stData, g.CreditAfter);

            // K 倍数 = 总得分 ÷ 下注；下注为 0 时置 0，避免除零。汇总区分档用 COUNTIFS 引用 K 列。
            sheet.Fml(ColMult, stData, $"IF({c}=0,0,{h}/{c})");

            // L 彩金类型：仅彩金局写入（BuildExportData 已过滤），其余留空。
            if (string.IsNullOrEmpty(g.JackpotType))
                sheet.Str(ColJackpotType, stData, "");
            else
                sheet.Str(ColJackpotType, stData, g.JackpotType);
        }

        /// <summary>顶部横排表头（M～U）；「输」与彩金类型明细见 <see cref="AppendFormulaSummary"/>。</summary>
        static string[] GetTopSummaryLabels() => new[]
        {
            "统计项", "总局", "总玩分", "总得分",
            "合计RTP", "大奖RTP", "普通游戏RTP", "免费RTP", "彩金游戏RTP",
        };

        /// <summary>
        /// 在汇总区写入全部由 Excel/WPS 公式计算的统计表（不写死数值）。
        /// </summary>
        /// <remarks>
        /// <para><b>布局顺序</b>（从 <paramref name="row"/> 起向下，列 L～T 与游戏区 A～K 并列）：</para>
        /// <list type="number">
        /// <item>顶部「数值」行（<paramref name="summaryValuesRow"/>）：总局、总玩分、RTP 等（横排，与最后一条游戏记录行表头对齐）。</item>
        /// <item>空行分隔。</item>
        /// <item>分档明细表头：类型 / 局 / 赢分 / 平均（倍）/ 出现概率 / RTP返还率。</item>
        /// <item>明细块：普通（赢）→ 输 → <paramref name="block2Title"/>（大奖）→ 彩金 → 免费，每块后空一行。</item>
        /// <item>「合计」行：总玩/局/赢分/出现概率/RTP = 各块小计同列之和；平均倍 = 合计赢分÷合计总玩。</item>
        /// </list>
        /// <para><b>数据引用</b>：游戏区第 <paramref name="firstDataRow"/>～<paramref name="lastDataRow"/> 行；
        /// A=局类型，C=下注，H=总得分（公式），K=倍数（公式）。改左侧数据后表格自动重算。</para>
        /// <para><b>RTP 口径</b>：各类 RTP = 该类型 H 列之和 ÷ 顶部「总玩分」；分母统一为全局 SUM(C)，非该类型下注之和。</para>
        /// </remarks>
        /// <param name="sheet">工作表构建器。</param>
        /// <param name="row">当前写入行号（入参为汇总区起始行，出参为合计行下一行）。</param>
        /// <param name="block2Title">第二块分档标题，须与 A 列大奖文案一致（如 FIREBIRD、大奖），用于 SUMIF/COUNTIFS。</param>
        /// <param name="firstDataRow">游戏数据首行（通常为 2）。</param>
        /// <param name="lastDataRow">游戏数据末行（= 1 + 记录条数）。</param>
        /// <param name="summaryValuesRow">顶部汇总「数值」行号（= lastDataRow + 1），各块出现概率/RTP 分母引用此行。</param>
        /// <param name="stSxV">顶部数值行样式。</param>
        /// <param name="stDh">分档表头样式。</param>
        /// <param name="stSec">分块标题（普通/大奖/彩金/免费）样式。</param>
        /// <param name="stSd">分档明细行样式。</param>
        /// <param name="stSdZ">分档明细斑马纹样式。</param>
        /// <param name="stSt">各块「小计」行样式。</param>
        /// <param name="stTot">最底「合计」行样式。</param>
        static void AppendFormulaSummary(SheetBuilder sheet, ref int row, string block2Title, int firstDataRow,
            int lastDataRow, int summaryValuesRow, int stSxV, int stDh, int stSec, int stSd, int stSdZ, int stSt,
            int stTot)
        {
            // —— 游戏区绝对区域（如 A2:K500），供 COUNT/COUNTIF/SUM/SUMIF/COUNTIFS/SUMIFS 引用 ——
            var rngA = AbsRange(ColAward, firstDataRow, lastDataRow);       // A：赢/输/免费/大奖/彩金
            var rngC = AbsRange(ColBet, firstDataRow, lastDataRow);         // C：下注
            var rngH = AbsRange(ColTotalWin, firstDataRow, lastDataRow);    // H：总得分（每行 SUM(D:G)）
            var rngMult = AbsRange(ColMult, firstDataRow, lastDataRow);     // K：倍数 H/C，分档用
            var rngJpType = AbsRange(ColJackpotType, firstDataRow, lastDataRow); // L：彩金类型 0/1/2

            // 顶部横排「数值」行单元格（总局、总玩分等；分档表出现概率/RTP 仍引用本行）
            var cellRounds = Cell(SummaryColRounds, summaryValuesRow);      // M 总局
            var cellSumBet = Cell(SummaryColSumBet, summaryValuesRow);      // N 总玩分
            var cellSumWin = Cell(SummaryColSumWin, summaryValuesRow);      // O 总得分

            // —— 第 1 段：顶部横排「数值」行（L=数值，M～T 与上一行表头 L～T 对应）——
            sheet.BeginRow(row);
            sheet.Str(SummaryColStart, stSxV, "数值");

            // M 总局：A 列非空单元格数（每条导出记录一行）
            sheet.Fml(SummaryColRounds, stSxV, $"COUNTA({rngA})");
            // N 总玩分 = 全部下注之和
            sheet.Fml(SummaryColSumBet, stSxV, $"SUM({rngC})");
            // O 总得分 = 全部 H 列之和
            sheet.Fml(SummaryColSumWin, stSxV, $"SUM({rngH})");
            // P 合计 RTP = 总得分 / 总玩分
            sheet.Fml(SummaryColRtpTotal, PctStyle(stSxV), $"IF({cellSumBet}=0,0,{cellSumWin}/{cellSumBet})");
            // Q 大奖 RTP：A 列等于 block2Title（BonusWin 局，标题可配置为 FIREBIRD 等）
            sheet.Fml(SummaryColRtpBig, PctStyle(stSxV),
                $"IF({cellSumBet}=0,0,SUMIF({rngA},\"{FmlEsc(block2Title)}\",{rngH})/{cellSumBet})");
            // R 普通游戏 RTP：仅 A 列为「赢」的 H 之和 / 总玩分（不含「输」局）
            sheet.Fml(SummaryColRtpNormal, PctStyle(stSxV),
                $"IF({cellSumBet}=0,0,SUMIF({rngA},\"赢\",{rngH})/{cellSumBet})");
            // S 免费 RTP：A 列为「免费」的 H 之和 / 总玩分
            sheet.Fml(SummaryColRtpFree, PctStyle(stSxV),
                $"IF({cellSumBet}=0,0,SUMIF({rngA},\"免费\",{rngH})/{cellSumBet})");
            // T 彩金游戏 RTP：A 列为「彩金」的 H 之和 / 总玩分
            sheet.Fml(SummaryColRtpJackpot, PctStyle(stSxV),
                $"IF({cellSumBet}=0,0,SUMIF({rngA},\"彩金\",{rngH})/{cellSumBet})");
            sheet.EndRow();
            row++;

            // —— 第 2 段：空行（视觉分隔顶部总览与下方分档表）——
            sheet.BeginRow(row);
            sheet.Str(SummaryColStart, 0, "");
            sheet.EndRow();
            row++;

            // —— 第 3 段：分档明细表头（L～R）——
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stDh, "类型");
            sheet.Str(DetailColCount, stDh, "局");
            sheet.Str(DetailColBet, stDh, "总玩");
            sheet.Str(DetailColWin, stDh, "赢分");
            sheet.Str(DetailColAvg, stDh, "平均（倍）");
            sheet.Str(DetailColProb, stDh, "出现概率");
            sheet.Str(DetailColRtp, stDh, "RTP返还率");
            sheet.EndRow();
            row++;

          
            var subtotalRows = new List<int>();

            // —— 第 4 段：输（A=「输」，单行小计：局/赢分/平均倍/出现概率/RTP，与彩金块同结构）——
            AppendTypeSubtotalSection(sheet, ref row, "输", rngA, rngC, rngH, summaryValuesRow, stSec, stSt, subtotalRows);
            sheet.BeginRow(row);
            sheet.EndRow();
            row++;

            // —— 第 5 段
            AppendBucketSection(sheet, ref row, "普通", rngA, "赢", rngC, rngH, rngMult, _BucketNormal,
                SlotGameRecordExport.SlotGameRecordSummaryBucketScheme.NormalMainGame, summaryValuesRow,
                stSec, stSd, stSdZ, stSt, subtotalRows);
            sheet.BeginRow(row);
            sheet.EndRow();
            row++;



            // —— 第 6 段：大奖玩法（A=block2Title，K 列 200～600 倍分档）——
            AppendBucketSection(sheet, ref row, block2Title, rngA, block2Title, rngC, rngH, rngMult,
                _BucketBigWin, SlotGameRecordExport.SlotGameRecordSummaryBucketScheme.BigWinFeature,
                summaryValuesRow, stSec, stSd, stSdZ, stSt, subtotalRows);
            sheet.BeginRow(row);
            sheet.EndRow();
            row++;

            // —— 第 8 段：彩金（Major/Minor/Mini + 小计）——
            AppendJackpotSection(sheet, ref row, rngA, rngC, rngH, rngJpType, summaryValuesRow, stSec, stSd, stSdZ,
                stSt, subtotalRows);
            sheet.BeginRow(row);
            sheet.EndRow();
            row++;

            // —— 第 9 段：免费（A=「免费」，K 列 60～150 倍分档）——
            AppendBucketSection(sheet, ref row, "免费", rngA, "免费", rngC, rngH, rngMult, _BucketFree,
                SlotGameRecordExport.SlotGameRecordSummaryBucketScheme.FreeGameFeature, summaryValuesRow,
                stSec, stSd, stSdZ, stSt, subtotalRows);
            sheet.BeginRow(row);
            sheet.EndRow();
            row++;

            // —— 合计行：总玩/局/赢分/出现概率/RTP = 各块「小计」同列之和；平均倍 = 合计赢分÷合计总玩 ——
            var totRow = row;
            var sumCountF = SumSubtotalCells(subtotalRows, DetailColCount);
            var sumWinF = SumSubtotalCells(subtotalRows, DetailColWin);
            var sumBetF = SumSubtotalCells(subtotalRows, DetailColBet);
            var sumProbF = SumSubtotalCells(subtotalRows, DetailColProb);
            var sumRtpF = SumSubtotalCells(subtotalRows, DetailColRtp);
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stTot, "合计");
            sheet.Fml(DetailColBet, stTot, sumBetF);
            sheet.Fml(DetailColCount, stTot, sumCountF);
            sheet.Fml(DetailColWin, stTot, sumWinF);
            sheet.Fml(DetailColAvg, stTot, AvgMultFormula(Cell(DetailColWin, totRow), Cell(DetailColBet, totRow)));
            sheet.Fml(DetailColProb, PctStyle(stTot), sumProbF);
            sheet.Fml(DetailColRtp, PctStyle(stTot), sumRtpF);
            sheet.EndRow();
            row++;
        }

        /// <summary>对多块「小计」行的同一列求和，如 SUM(M10,M25,M40,…)。</summary>
        static string SumSubtotalCells(List<int> subtotalRows, int col)
        {
            if (subtotalRows == null || subtotalRows.Count == 0)
                return "0";
            if (subtotalRows.Count == 1)
                return Cell(col, subtotalRows[0]);
            var sb = new StringBuilder("SUM(");
            for (var i = 0; i < subtotalRows.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(Cell(col, subtotalRows[i]));
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>按倍数分档的一块（普通/大奖/免费）：每档 6 行公式 + 1 行小计公式。</summary>
        /// <param name="typeLabel">A 列匹配文案（如 赢、免费、FIREBIRD），与 <paramref name="title"/> 块标题一致时相同。</param>
        static void AppendBucketSection(SheetBuilder sheet, ref int row, string title, string rngA, string typeLabel,
            string rngC, string rngH, string rngMult, (double Lo, double? Hi)[] buckets,
            SlotGameRecordExport.SlotGameRecordSummaryBucketScheme scheme, int summaryValuesRow,
            int stSec, int stSd, int stSdZ, int stSt, List<int> subtotalRows)
        {
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stSec, title);
            sheet.EndRow();
            row++;

            var typeBetF = TypeBetSumFormula(rngA, typeLabel, rngC);
            var firstBucketRow = row;
            for (var b = 0; b < buckets.Length; b++)
            {
                var (lo, hi) = buckets[b];
                var label = SlotGameRecordExport.GetSummaryBucketLabel(b, scheme);
                var st = (b & 1) == 0 ? stSd : stSdZ;
                var countF = BucketCountFormula(rngA, typeLabel, rngMult, lo, hi);
                var winF = BucketWinFormula(rngA, typeLabel, rngH, rngMult, lo, hi);
                var betF = BucketBetFormula(rngA, typeLabel, rngC, rngMult, lo, hi);
                var br = row;
                sheet.BeginRow(row);
                sheet.Str(DetailColType, st, label);
                sheet.Fml(DetailColBet, st, betF);
                sheet.Fml(DetailColCount, st, countF);
                sheet.Fml(DetailColWin, st, winF);
                sheet.Fml(DetailColAvg, st, AvgMultFormula(Cell(DetailColWin, br), Cell(DetailColBet, br)));
                sheet.Fml(DetailColProb, PctStyle(st),
                    $"IF({Cell(SummaryColRounds, summaryValuesRow)}=0,0,{Cell(DetailColCount, br)}/{Cell(SummaryColRounds, summaryValuesRow)})");
                sheet.Fml(DetailColRtp, PctStyle(st),
                    $"IF({Cell(SummaryColSumBet, summaryValuesRow)}=0,0,{Cell(DetailColWin, br)}/{Cell(SummaryColSumBet, summaryValuesRow)})");
                sheet.EndRow();
                row++;
            }

            var lastBucketRow = row - 1;
            var subRow = row;
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stSt, "小计");
            sheet.Fml(DetailColBet, stSt,
                $"SUM({Cell(DetailColBet, firstBucketRow)}:{Cell(DetailColBet, lastBucketRow)})");
            sheet.Fml(DetailColCount, stSt,
                $"SUM({Cell(DetailColCount, firstBucketRow)}:{Cell(DetailColCount, lastBucketRow)})");
            sheet.Fml(DetailColWin, stSt,
                $"SUM({Cell(DetailColWin, firstBucketRow)}:{Cell(DetailColWin, lastBucketRow)})");
            sheet.Fml(DetailColAvg, stSt, AvgMultFormula(Cell(DetailColWin, subRow), Cell(DetailColBet, subRow)));
            sheet.Fml(DetailColProb, PctStyle(stSt),
                $"SUM({Cell(DetailColProb, firstBucketRow)}:{Cell(DetailColProb, lastBucketRow)})");
            sheet.Fml(DetailColRtp, PctStyle(stSt),
                $"SUM({Cell(DetailColRtp, firstBucketRow)}:{Cell(DetailColRtp, lastBucketRow)})");
            sheet.EndRow();
            subtotalRows.Add(subRow);
            row++;
        }

        /// <summary>
        /// 彩金块：按 L 列 jackpot_type（0=Major，1=Minor，2=Mini）分行统计，末行「小计」统计全部 A=彩金。
        /// </summary>
        static void AppendJackpotSection(SheetBuilder sheet, ref int row, string rngA, string rngC, string rngH,
            string rngJpType, int summaryValuesRow, int stSec, int stSd, int stSdZ, int stSt, List<int> subtotalRows)
        {
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stSec, "彩金");
            sheet.EndRow();
            row++;

            var firstTypeRow = row;
            for (var i = 0; i < _JackpotTypeRows.Length; i++)
            {
                var (code, label) = _JackpotTypeRows[i];
                var st = (i & 1) == 0 ? stSd : stSdZ;
                AppendJackpotTypeDetailRow(sheet, ref row, label, rngA, rngC, rngH, rngJpType, code, summaryValuesRow,
                    st);
            }

            var lastTypeRow = row - 1;
            var dataRow = row;
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stSt, "小计");
            sheet.Fml(DetailColBet, stSt,
                $"SUM({Cell(DetailColBet, firstTypeRow)}:{Cell(DetailColBet, lastTypeRow)})");
            sheet.Fml(DetailColCount, stSt,
                $"SUM({Cell(DetailColCount, firstTypeRow)}:{Cell(DetailColCount, lastTypeRow)})");
            sheet.Fml(DetailColWin, stSt,
                $"SUM({Cell(DetailColWin, firstTypeRow)}:{Cell(DetailColWin, lastTypeRow)})");
            sheet.Fml(DetailColAvg, stSt, AvgMultFormula(Cell(DetailColWin, dataRow), Cell(DetailColBet, dataRow)));
            sheet.Fml(DetailColProb, PctStyle(stSt),
                $"SUM({Cell(DetailColProb, firstTypeRow)}:{Cell(DetailColProb, lastTypeRow)})");
            sheet.Fml(DetailColRtp, PctStyle(stSt),
                $"SUM({Cell(DetailColRtp, firstTypeRow)}:{Cell(DetailColRtp, lastTypeRow)})");
            sheet.EndRow();
            subtotalRows.Add(dataRow);
            row++;
        }

        /// <summary>彩金块内单行：A=彩金 且 L=typeCode（0/1/2）。</summary>
        static void AppendJackpotTypeDetailRow(SheetBuilder sheet, ref int row, string label, string rngA, string rngC,
            string rngH, string rngJpType, string typeCode, int summaryValuesRow, int st)
        {
            var dataRow = row;
            var roundsRef = Cell(SummaryColRounds, summaryValuesRow);
            var betF = JackpotTypeBetFormula(rngA, rngC, rngJpType, typeCode);
            sheet.BeginRow(row);
            sheet.Str(DetailColType, st, label);
            sheet.Fml(DetailColBet, st, betF);
            sheet.Fml(DetailColCount, st, JackpotTypeCountFormula(rngA, rngJpType, typeCode));
            sheet.Fml(DetailColWin, st, JackpotTypeWinFormula(rngA, rngH, rngJpType, typeCode));
            sheet.Fml(DetailColAvg, st, AvgMultFormula(Cell(DetailColWin, dataRow), Cell(DetailColBet, dataRow)));
            sheet.Fml(DetailColProb, PctStyle(st),
                $"IF({roundsRef}=0,0,{Cell(DetailColCount, dataRow)}/{roundsRef})");
            sheet.Fml(DetailColRtp, PctStyle(st),
                $"IF({Cell(SummaryColSumBet, summaryValuesRow)}=0,0,{Cell(DetailColWin, dataRow)}/{Cell(SummaryColSumBet, summaryValuesRow)})");
            sheet.EndRow();
            row++;
        }

        static string JackpotTypeCountFormula(string rngA, string rngJpType, string typeCode) =>
            $"COUNTIFS({rngA},\"彩金\",{rngJpType},\"{FmlEsc(typeCode)}\")";

        static string JackpotTypeWinFormula(string rngA, string rngH, string rngJpType, string typeCode) =>
            $"SUMIFS({rngH},{rngA},\"彩金\",{rngJpType},\"{FmlEsc(typeCode)}\")";

        static string JackpotTypeBetFormula(string rngA, string rngC, string rngJpType, string typeCode) =>
            $"SUMIFS({rngC},{rngA},\"彩金\",{rngJpType},\"{FmlEsc(typeCode)}\")";

        /// <summary>
        /// 单类型汇总块（输等）：块标题行 + 一行「小计」，列与分档表一致（局、赢分、平均倍、出现概率、RTP）。
        /// </summary>
        /// <param name="title">块标题，须与 A 列局类型文案一致（如 输、彩金）。</param>
        static void AppendTypeSubtotalSection(SheetBuilder sheet, ref int row, string title, string rngA, string rngC,
            string rngH, int summaryValuesRow, int stSec, int stSt, List<int> subtotalRows)
        {
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stSec, title);
            sheet.EndRow();
            row++;

            var dataRow = row;
            var roundsRef = Cell(SummaryColRounds, summaryValuesRow);
            var typeEsc = FmlEsc(title);
            var typeBetF = TypeBetSumFormula(rngA, title, rngC);
            sheet.BeginRow(row);
            sheet.Str(DetailColType, stSt, "小计");
            sheet.Fml(DetailColBet, stSt, typeBetF);
            sheet.Fml(DetailColCount, stSt, $"COUNTIF({rngA},\"{typeEsc}\")");
            sheet.Fml(DetailColWin, stSt, $"SUMIF({rngA},\"{typeEsc}\",{rngH})");
            sheet.Fml(DetailColAvg, stSt, AvgMultFormula(Cell(DetailColWin, dataRow), Cell(DetailColBet, dataRow)));
            sheet.Fml(DetailColProb, PctStyle(stSt),
                $"IF({roundsRef}=0,0,{Cell(DetailColCount, dataRow)}/{roundsRef})");
            sheet.Fml(DetailColRtp, PctStyle(stSt),
                $"IF({Cell(SummaryColSumBet, summaryValuesRow)}=0,0,{Cell(DetailColWin, dataRow)}/{Cell(SummaryColSumBet, summaryValuesRow)})");
            sheet.EndRow();
            subtotalRows.Add(dataRow);
            row++;
        }

        /// <summary>COUNTIFS：A 列=类型 且 K 列落在倍数区间内的局数。</summary>
        static string BucketCountFormula(string rngA, string typeLabel, string rngMult, double lo, double? hi)
        {
            var typeEsc = FmlEsc(typeLabel);
            var sb = new StringBuilder();
            sb.Append("COUNTIFS(").Append(rngA).Append(",\"").Append(typeEsc).Append("\",")
                .Append(rngMult).Append(",\">=").Append(Fmt(lo)).Append("\"");
            if (hi.HasValue)
                sb.Append(',').Append(rngMult).Append(",\"<").Append(Fmt(hi.Value)).Append('"');
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>平均（倍）= 赢分 ÷ 押注；押注为 0 时置 0（与 K 列 H/C 口径一致）。</summary>
        static string AvgMultFormula(string winRef, string betFormula) =>
            $"IF({betFormula}=0,0,{winRef}/{betFormula})";

        /// <summary>某类型在 C 列的下注总和。</summary>
        static string TypeBetSumFormula(string rngA, string typeLabel, string rngC) =>
            $"SUMIF({rngA},\"{FmlEsc(typeLabel)}\",{rngC})";

        /// <summary>SUMIFS：A 列=类型 且 K 列落在倍数区间内的 C 列押注之和。</summary>
        static string BucketBetFormula(string rngA, string typeLabel, string rngC, string rngMult, double lo,
            double? hi)
        {
            var typeEsc = FmlEsc(typeLabel);
            var sb = new StringBuilder();
            sb.Append("SUMIFS(").Append(rngC).Append(',').Append(rngA).Append(",\"").Append(typeEsc).Append("\",")
                .Append(rngMult).Append(",\">=").Append(Fmt(lo)).Append("\"");
            if (hi.HasValue)
                sb.Append(',').Append(rngMult).Append(",\"<").Append(Fmt(hi.Value)).Append('"');
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>SUMIFS：A 列=类型 且 K 列落在倍数区间内的 H 列总得分。</summary>
        static string BucketWinFormula(string rngA, string typeLabel, string rngH, string rngMult, double lo,
            double? hi)
        {
            var typeEsc = FmlEsc(typeLabel);
            var sb = new StringBuilder();
            sb.Append("SUMIFS(").Append(rngH).Append(',').Append(rngA).Append(",\"").Append(typeEsc).Append("\",")
                .Append(rngMult).Append(",\">=").Append(Fmt(lo)).Append("\"");
            if (hi.HasValue)
                sb.Append(',').Append(rngMult).Append(",\"<").Append(Fmt(hi.Value)).Append('"');
            sb.Append(')');
            return sb.ToString();
        }

        static string Cell(int col, int row) => ColLetter(col) + row.ToString(Inv);

        static string AbsRange(int col, int row1, int row2) =>
            "$" + ColLetter(col) + "$" + row1 + ":$" + ColLetter(col) + "$" + row2;

        static string ColLetter(int col1Based)
        {
            var n = col1Based;
            var s = "";
            while (n > 0)
            {
                n--;
                s = (char)('A' + n % 26) + s;
                n /= 26;
            }

            return s;
        }

        static string Fmt(double v) => v.ToString(Inv);

        static string FmlEsc(string s) => (s ?? "").Replace("\"", "\"\"");

        static string ContentTypes() =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
            "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
            "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
            "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
            "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
            "<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>" +
            "</Types>";

        static string RootRels() =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"" + NsPkg + "\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
            "</Relationships>";

        static string WorkbookXml() =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<workbook xmlns=\"" + NsMain + "\" xmlns:r=\"" + NsR + "\">" +
            "<sheets><sheet name=\"游戏记录\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>";

        static string WorkbookRels() =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"" + NsPkg + "\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
            "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
            "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>" +
            "</Relationships>";

        const int StSxV = 5;
        const int StSd = 8;
        const int StSdZ = 9;
        const int StSt = 10;
        const int StTot = 11;
        // 12～16：与 5/8/9/10/11 同配色，numFmtId=10（0.00%）
        const int StPctSxV = 12;
        const int StPctSd = 13;
        const int StPctSdZ = 14;
        const int StPctSt = 15;
        const int StPctTot = 16;

        /// <summary>出现概率 / RTP 列使用百分比样式（公式值仍为 0～1 小数）。</summary>
        static int PctStyle(int style) => style switch
        {
            StSxV => StPctSxV,
            StSd => StPctSd,
            StSdZ => StPctSdZ,
            StSt => StPctSt,
            StTot => StPctTot,
            _ => style
        };

        static string StylesXml() =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<styleSheet xmlns=\"" + NsMain + "\">" +
            "<fonts count=\"3\">" +
            "<font><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
            "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font>" +
            "<font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
            "</fonts>" +
            "<fills count=\"12\">" +
            "<fill><patternFill patternType=\"none\"/></fill>" +
            "<fill><patternFill patternType=\"gray125\"/></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF4472C4\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFFFFFF\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFD9E1F2\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF366092\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFD6DCE4\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFFC000\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFF2F2F2\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFE2EFDA\"/></patternFill></fill>" +
            "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF92D050\"/></patternFill></fill>" +
            "</fills>" +
            "<borders count=\"2\">" +
            "<border><left/><right/><top/><bottom/><diagonal/></border>" +
            "<border><bottom style=\"thin\"><color rgb=\"FFD0D7E5\"/></bottom></border>" +
            "</borders>" +
            "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
            "<cellXfs count=\"17\">" +
            "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
            "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\"/></xf>" +
            "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"3\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"5\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\"/></xf>" +
            "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"3\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"6\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\"/></xf>" +
            "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"7\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"3\" borderId=\"0\" xfId=\"0\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"8\" borderId=\"0\" xfId=\"0\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"9\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"10\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"10\" fontId=\"0\" fillId=\"3\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyBorder=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"10\" fontId=\"2\" fillId=\"7\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"10\" fontId=\"0\" fillId=\"3\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"10\" fontId=\"2\" fillId=\"9\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\"/>" +
            "<xf numFmtId=\"10\" fontId=\"2\" fillId=\"10\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\"/>" +
            "</cellXfs>" +
            "</styleSheet>";

        /// <summary>中文等文本写入 sharedStrings.xml，单元格通过索引引用。</summary>
        sealed class SharedStringTable
        {
            readonly List<string> _list = new List<string>();
            readonly Dictionary<string, int> _map = new Dictionary<string, int>();

            public int Index(string s)
            {
                s ??= "";
                if (_map.TryGetValue(s, out var i))
                    return i;
                i = _list.Count;
                _list.Add(s);
                _map[s] = i;
                return i;
            }

            public string ToXml()
            {
                var sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                sb.Append("<sst xmlns=\"").Append(NsMain).Append("\" count=\"").Append(_list.Count)
                    .Append("\" uniqueCount=\"").Append(_list.Count).Append("\">");
                foreach (var t in _list)
                {
                    sb.Append("<si><t");
                    if (NeedsPreserve(t))
                        sb.Append(" xml:space=\"preserve\"");
                    sb.Append(">").Append(XmlEsc(t)).Append("</t></si>");
                }

                sb.Append("</sst>");
                return sb.ToString();
            }

            static bool NeedsPreserve(string s) =>
                s.StartsWith(" ", StringComparison.Ordinal) || s.EndsWith(" ", StringComparison.Ordinal);

            static string XmlEsc(string s) =>
                s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        /// <summary>按行累积单元格 XML，最终输出 sheet1.xml 的 sheetData 段。</summary>
        sealed class SheetBuilder
        {
            readonly SharedStringTable _sst;
            readonly StringBuilder _sb = new StringBuilder();
            int _row;
            readonly Dictionary<int, string> _pending = new Dictionary<int, string>();

            public SheetBuilder(SharedStringTable sst) => _sst = sst;

            public void BeginRow(int row)
            {
                FlushRow();
                _row = row;
            }

            public void EndRow() => FlushRow();

            void FlushRow()
            {
                if (_row <= 0 || _pending.Count == 0)
                    return;
                _sb.Append("<row r=\"").Append(_row).Append("\">");
                var keys = new List<int>(_pending.Keys);
                keys.Sort();
                foreach (var col in keys)
                    _sb.Append(_pending[col]);
                _sb.Append("</row>");
                _pending.Clear();
                _row = 0;
            }

            public void Str(int col, int style, string text)
            {
                var idx = _sst.Index(text);
                _pending[col] = CellXml(col, style, "s", idx.ToString(Inv), null);
            }

            public void Num(int col, int style, double v)
            {
                _pending[col] = CellXml(col, style, null, v.ToString(Inv), null);
            }

            public void Fml(int col, int style, string formulaBody)
            {
                _pending[col] = CellXml(col, style, null, "0", formulaBody);
            }

            public void FmlRef(int col, int style, string a1Ref)
            {
                _pending[col] = CellXml(col, style, null, "0", a1Ref);
            }

            string CellXml(int col, int style, string typeAttr, string v, string formula)
            {
                var sb = new StringBuilder();
                sb.Append("<c r=\"").Append(Cell(col, _row)).Append("\"");
                if (style > 0)
                    sb.Append(" s=\"").Append(style).Append("\"");
                if (!string.IsNullOrEmpty(typeAttr))
                    sb.Append(" t=\"").Append(typeAttr).Append("\"");
                sb.Append(">");
                if (!string.IsNullOrEmpty(formula))
                    sb.Append("<f>").Append(XmlEscF(formula)).Append("</f>");
                sb.Append("<v>").Append(v).Append("</v></c>");
                return sb.ToString();
            }

            public string ToXml()
            {
                FlushRow();
                return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                       "<worksheet xmlns=\"" + NsMain + "\"><sheetData>" + _sb + "</sheetData></worksheet>";
            }

            static string XmlEscF(string s) =>
                s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }

    /// <summary>
    /// 兼容旧类名：实际写入 .xlsx。若传入 .xml 路径会自动改为同名的 .xlsx。
    /// </summary>
    public static class SlotGameRecordSpreadsheetXml
    {
        public const int SummaryStartColumn = SlotGameRecordSpreadsheetXlsx.SummaryStartColumn;

        public static void WriteStyledWorkbook(string path, SlotGameRecordExport.SlotGameRecordExportData data,
            string summaryBlock2Title)
        {
            if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                path = path.Substring(0, path.Length - 4) + ".xlsx";
            SlotGameRecordSpreadsheetXlsx.WriteStyledWorkbook(path, data, summaryBlock2Title);
        }
    }
}
