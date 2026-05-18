using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace GameMaker
{
    public enum SlotGameRecordColAExportMode
    {
        /// <summary>赢/输/免费/大奖/彩金 五种局类型文案（默认）。</summary>
        AwardLabel,
        /// <summary>数据库 id（合并段取首行 id）。</summary>
        RowId,
        /// <summary>导出表内 1-based 行号。</summary>
        RowIndex,
    }

    public static class SlotGameRecordExport
    {
        public static readonly string[] _Header =
        {
            "大奖统计",
            "下注前",
            "下注",
            "基础游戏得分",
            "免费游戏得分",
            "大奖游戏得分",
            "彩金得分",
            "总得分",
            "输赢",
            "结束",
        };

        /// <summary>与算法 ResultType 枚举顺序一致（见各机台 MachineDataController）。</summary>
        const int ResultTypeFreeWin = 2;
        const int ResultTypeBonusWin = 3;
        const int ResultTypeJackpot = 4;
        const int ResultTypeJackpotOnline = 5;

        const int OpenTypeNormal = 0;
        const int OpenTypeGive = 1;

        /// <param name="rows">已按 id 升序排列。</param>
        /// <param name="creditDivisor">与脚本 --credit-divisor 一致。</param>
        /// <param name="colAMode">A 列：局类型文案 / id / 行号。</param>
        /// <param name="mergeFreeGiveSessions">将「免费触发局 + 其后连续赠送局」合并为一行统计（A 列为「免费」）。</param>
        /// <param name="bigWinBetMultiple">历史参数，当前「大奖」由 result_type==BonusWin 判定，本参数不参与分类。</param>
        /// <param name="outputLineCount">导出合并游戏行数（不含表头）。</param>
        public static string Build_Csv(
            IList<DataRow> rows,
            double creditDivisor,
            SlotGameRecordColAExportMode colAMode,
            bool mergeFreeGiveSessions,
            double bigWinBetMultiple,
            out int outputLineCount)
        {
            var d = BuildExportData(rows, creditDivisor, colAMode, mergeFreeGiveSessions, bigWinBetMultiple);
            outputLineCount = d.GameRows.Count;
            return FormatSlotGameRecordCsv(d);
        }

        /// <summary>一行游戏数据（已缩放）。</summary>
        public readonly struct SlotGameRecordCsvRow
        {
            public readonly string ColA;
            public readonly double CreditBefore;
            public readonly double TotalBet;
            public readonly double BaseGameWin;
            public readonly double FreeSpinWin;
            public readonly double BonusGameWin;
            public readonly double JackpotWin;
            public readonly double TotalWin;
            public readonly double Delta;
            public readonly double CreditAfter;

            public SlotGameRecordCsvRow(
                string colA,
                double creditBefore,
                double totalBet,
                double baseGameWin,
                double freeSpinWin,
                double bonusGameWin,
                double jackpotWin,
                double totalWin,
                double delta,
                double creditAfter)
            {
                ColA = colA ?? "";
                CreditBefore = creditBefore;
                TotalBet = totalBet;
                BaseGameWin = baseGameWin;
                FreeSpinWin = freeSpinWin;
                BonusGameWin = bonusGameWin;
                JackpotWin = jackpotWin;
                TotalWin = totalWin;
                Delta = delta;
                CreditAfter = creditAfter;
            }
        }

        /// <summary>与 slot_game_record_summary_report.py 一致：Bet=下注，Win=基础+免费+大奖游戏得分之和，Jp=彩金得分，倍数用 (Win+Jp)/Bet（应等于总得分/下注）。</summary>
        public readonly struct SlotGameRecordSummaryMetric
        {
            public readonly string AwardLabel;
            public readonly double Bet;
            public readonly double Win;
            public readonly double Jp;

            public SlotGameRecordSummaryMetric(string awardLabel, double bet, double win, double jp)
            {
                AwardLabel = awardLabel ?? "";
                Bet = bet;
                Win = win;
                Jp = jp;
            }

            public double WinJp => Win + Jp;

            public double Mult => Bet > 0 ? WinJp / Bet : 0;
        }

        public sealed class SlotGameRecordExportData
        {
            public readonly List<SlotGameRecordCsvRow> GameRows;
            public readonly List<SlotGameRecordSummaryMetric> Metrics;

            public SlotGameRecordExportData(List<SlotGameRecordCsvRow> gameRows, List<SlotGameRecordSummaryMetric> metrics)
            {
                GameRows = gameRows ?? new List<SlotGameRecordCsvRow>();
                Metrics = metrics ?? new List<SlotGameRecordSummaryMetric>();
            }
        }

        public static SlotGameRecordExportData BuildExportData(
            IList<DataRow> rows,
            double creditDivisor,
            SlotGameRecordColAExportMode colAMode,
            bool mergeFreeGiveSessions,
            double bigWinBetMultiple)
        {
            if (rows == null || rows.Count == 0)
                return new SlotGameRecordExportData(new List<SlotGameRecordCsvRow>(), new List<SlotGameRecordSummaryMetric>());

            var segments = mergeFreeGiveSessions? BuildMergedSegments(rows): WrapSingletonSegments(rows);

            var gameRows = new List<SlotGameRecordCsvRow>(segments.Count);
            var metrics = new List<SlotGameRecordSummaryMetric>(segments.Count);
            var lineNo = 0;
            foreach (var seg in segments)
            {
                lineNo++;
                var awardLabel = seg.Count > 1? ClassifyMergedSegment(seg): ClassifySingleRow(seg[0], creditDivisor, bigWinBetMultiple);

                var colA = FormatColA(colAMode, awardLabel, seg, lineNo);
                TryGetSegmentScaledTotals(seg, awardLabel, creditDivisor, out var cb, out var ca, out var totalBet,
                    out var baseGame, out var freeSpin, out var bonusGame, out var jp, out var totalWin);
                var delta = ca - cb;
                var lineWinNoJp = baseGame + freeSpin + bonusGame;
                var parts = lineWinNoJp + jp - totalBet;
                if (Math.Abs(delta - parts) > 1e-6 && Math.Abs(delta - parts) > 1e-4 * Math.Max(Math.Abs(delta), 1.0))
                {
                    var first = seg[0];
                    DebugUtils.LogWarning(
                        $"[SlotGameRecordExport] 合并段 id 起 {GetLong(first, "id")} delta={delta} != 基础+免费+大奖+彩金-下注={parts}");
                }

                metrics.Add(new SlotGameRecordSummaryMetric(awardLabel, totalBet, lineWinNoJp, jp));
                gameRows.Add(new SlotGameRecordCsvRow(colA, cb, totalBet, baseGame, freeSpin, bonusGame, jp, totalWin,
                    delta, ca));
            }

            return new SlotGameRecordExportData(gameRows, metrics);
        }

        public static string FormatSlotGameRecordCsv(SlotGameRecordExportData d)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", Array.ConvertAll(_Header, EscapeCsv)));
            foreach (var g in d.GameRows)
                AppendSegmentRow(sb, g);
            return sb.ToString();
        }

        /// <summary>游戏记录汇总「普通 / 大奖 / 免费」分档方案（与 slot_game_record_summary_report.py 一致）。</summary>
        public enum SlotGameRecordSummaryBucketScheme
        {
            /// <summary>普通：1 倍以下～20 倍以上。</summary>
            NormalMainGame = 0,
            /// <summary>大奖：200～600 倍档（200/300/400/500/600 分界）+ 上下限档。</summary>
            BigWinFeature = 1,
            /// <summary>免费：60～150 倍档（60/80/100/120/150 分界）+ 上下限档。</summary>
            FreeGameFeature = 2,
        }

        public const int SlotGameRecordSummaryBucketCount = 6;

        /// <summary>兼容旧调用：等同 <see cref="SlotGameRecordSummaryBucketScheme.NormalMainGame"/>。</summary>
        public static int GetSummaryMultBucketIndex(double mult) =>
            GetSummaryBucketIndex(mult, SlotGameRecordSummaryBucketScheme.NormalMainGame);

        public static string GetSummaryMultBucketLabel(int bucketIndex) =>
            GetSummaryBucketLabel(bucketIndex, SlotGameRecordSummaryBucketScheme.NormalMainGame);

        public static int GetSummaryBucketIndex(double mult, SlotGameRecordSummaryBucketScheme scheme)
        {
            switch (scheme)
            {
                case SlotGameRecordSummaryBucketScheme.BigWinFeature:
                    if (mult < 200) return 0;
                    if (mult < 300) return 1;
                    if (mult < 400) return 2;
                    if (mult < 500) return 3;
                    if (mult < 600) return 4;
                    return 5;
                case SlotGameRecordSummaryBucketScheme.FreeGameFeature:
                    if (mult < 60) return 0;
                    if (mult < 80) return 1;
                    if (mult < 100) return 2;
                    if (mult < 120) return 3;
                    if (mult < 150) return 4;
                    return 5;
                default:
                    return MultBucketIndexNormal(mult);
            }
        }

        public static string GetSummaryBucketLabel(int bucketIndex, SlotGameRecordSummaryBucketScheme scheme)
        {
            var labels = scheme == SlotGameRecordSummaryBucketScheme.BigWinFeature
                ? _BigWinBucketLabels
                : scheme == SlotGameRecordSummaryBucketScheme.FreeGameFeature
                    ? _FreeGameBucketLabels
                    : _MultBucketLabelsNormal;
            if (bucketIndex < 0 || bucketIndex >= labels.Length)
                return labels[0];
            return labels[bucketIndex];
        }

        static readonly string[] _MultBucketLabelsNormal =
        {
            "1倍以下",
            "1-2倍",
            "2-5倍",
            "5-10倍",
            "10-20倍",
            "20倍以上",
        };

        static readonly string[] _BigWinBucketLabels =
        {
            "200倍以下",
            "200-300倍",
            "300-400倍",
            "400-500倍",
            "500-600倍",
            "600倍以上",
        };

        static readonly string[] _FreeGameBucketLabels =
        {
            "60倍以下",
            "60-80倍",
            "80-100倍",
            "100-120倍",
            "120-150倍",
            "150倍以上",
        };

        static int MultBucketIndexNormal(double mult)
        {
            if (mult < 1) return 0;
            if (mult < 2) return 1;
            if (mult < 5) return 2;
            if (mult < 10) return 3;
            if (mult < 20) return 4;
            return 5;
        }

        /// <summary>
        /// 合并段内各得分汇总（已缩放）。多行合并为「免费」时，库内 free_spin/total_win 常为整段累计，逐行相加会重复；
        /// 此时用 末 credit_after - 首 credit_before + 本段下注 作为本段总得分，免费得分 = 总得分 - 基础 - 大奖 - 彩金。
        /// </summary>
        /// <summary>
        /// 合并段内各得分汇总（已缩放）。多行合并为「免费」时，库内 free_spin/total_win 常为整段累计，逐行相加会重复；
        /// 此时用 末 credit_after - 首 credit_before + 本段下注 作为本段总得分，免费得分 = 总得分 - 基础 - 大奖 - 彩金。
        /// </summary>
        static void TryGetSegmentScaledTotals(
            List<DataRow> seg,
            string awardLabel,
            double creditDivisor,
            out double cb,
            out double ca,
            out double totalBet,
            out double baseGame,
            out double freeSpin,
            out double bonusGame,
            out double jp,
            out double totalWin)
        {
            long sumBase = 0, sumFreeRaw = 0, sumBonus = 0, sumJp = 0, sumTotal = 0;
            foreach (var r in seg)
            {
                sumBase += GetLong(r, "base_game_win_credit");
                sumFreeRaw += GetLong(r, "free_spin_win_credit");
                sumBonus += GetLong(r, "bonus_game_win_credit");
                sumJp += GetLong(r, "jackpot_win_credit");
                sumTotal += GetLong(r, "total_win_credit");
            }

            if (sumTotal == 0 && sumBase + sumFreeRaw + sumBonus + sumJp != 0)
                sumTotal = sumBase + sumFreeRaw + sumBonus + sumJp;

            var first = seg[0];
            var last = seg[seg.Count - 1];
            long sumBetLong;
            if (seg.Count > 1 && string.Equals(awardLabel, "免费", StringComparison.Ordinal))
                sumBetLong = GetLong(first, "total_bet");
            else
            {
                sumBetLong = 0;
                foreach (var r in seg)
                    sumBetLong += GetLong(r, "total_bet");
            }

            // 合并「免费」段：库内 free_spin_win_credit / total_win_credit 常为「整段累计」，逐行相加会双倍；
            // 用钱包关系 本段总得分 = 末 credit_after - 首 credit_before + 本段下注，再扣基础/大奖/彩金得免费得分。
            long sumFree = sumFreeRaw;
            if (seg.Count > 1 && string.Equals(awardLabel, "免费", StringComparison.Ordinal))
            {
                var canonical = GetLong(last, "credit_after") - GetLong(first, "credit_before") + sumBetLong;
                if (canonical < 0)
                    canonical = 0;
                sumTotal = canonical;
                sumFree = canonical - sumBase - sumBonus - sumJp;
                if (sumFree < 0)
                    sumFree = 0;
            }

            cb = Scale(GetLong(first, "credit_before"), creditDivisor);
            ca = Scale(GetLong(last, "credit_after"), creditDivisor);
            totalBet = Scale(sumBetLong, creditDivisor);
            baseGame = Scale(sumBase, creditDivisor);
            freeSpin = Scale(sumFree, creditDivisor);
            bonusGame = Scale(sumBonus, creditDivisor);
            jp = Scale(sumJp, creditDivisor);
            totalWin = Scale(sumTotal, creditDivisor);
        }

        static void AppendSegmentRow(StringBuilder sb, in SlotGameRecordCsvRow g)
        {
            var fields = new[]
            {
                EscapeCsv(g.ColA),
                EscapeCsv(g.CreditBefore),
                EscapeCsv(g.TotalBet),
                EscapeCsv(g.BaseGameWin),
                EscapeCsv(g.FreeSpinWin),
                EscapeCsv(g.BonusGameWin),
                g.JackpotWin == 0 ? "" : EscapeCsv(g.JackpotWin),
                EscapeCsv(g.TotalWin),
                EscapeCsv(g.Delta),
                EscapeCsv(g.CreditAfter),
            };
            sb.AppendLine(string.Join(",", fields));
        }

        static List<List<DataRow>> WrapSingletonSegments(IList<DataRow> rows)
        {
            var list = new List<List<DataRow>>(rows.Count);
            foreach (var r in rows)
                list.Add(new List<DataRow> { r });
            return list;
        }

        /// <summary>
        /// 合并规则：①「主游戏 + RT_FreeWin + free_totaltime&gt;0」为免费段起点，其后连续 open_type=赠送局 并入同一段；
        /// ②导出窗口开头若仅有连续赠送局（无触发行），也合并为一段「免费」。
        /// </summary>
        public static List<List<DataRow>> BuildMergedSegments(IList<DataRow> ordered)
        {
            var segments = new List<List<DataRow>>();
            var i = 0;
            while (i < ordered.Count)
            {
                var r = ordered[i];
                if (IsFreeTriggerRow(r))
                {
                    var seg = new List<DataRow> { r };
                    var j = i + 1;
                    while (j < ordered.Count && GetInt(ordered[j], "open_type") == OpenTypeGive)
                    {
                        seg.Add(ordered[j]);
                        j++;
                    }

                    segments.Add(seg);
                    i = j;
                }
                else if (GetInt(r, "open_type") == OpenTypeGive)
                {
                    var seg = new List<DataRow>();
                    while (i < ordered.Count && GetInt(ordered[i], "open_type") == OpenTypeGive)
                    {
                        seg.Add(ordered[i]);
                        i++;
                    }

                    segments.Add(seg);
                }
                else
                {
                    segments.Add(new List<DataRow> { r });
                    i++;
                }
            }

            return segments;
        }

        static bool IsFreeTriggerRow(DataRow r)
        {
            return GetInt(r, "open_type") == OpenTypeNormal &&
                   GetInt(r, "result_type") == ResultTypeFreeWin &&
                   GetInt(r, "free_totaltime") > 0;
        }

        static string FormatColA(SlotGameRecordColAExportMode mode, string label, List<DataRow> seg, int lineNo)
        {
            switch (mode)
            {
                case SlotGameRecordColAExportMode.RowId:
                    return GetLong(seg[0], "id").ToString(CultureInfo.InvariantCulture);
                case SlotGameRecordColAExportMode.RowIndex:
                    return lineNo.ToString(CultureInfo.InvariantCulture);
                default:
                    return label;
            }
        }

        static string ClassifyMergedSegment(List<DataRow> seg)
        {
            foreach (var r in seg)
            {
                if (IsJackpotPrizeRow(r))
                    return "彩金";
            }

            return "免费";
        }

        static string ClassifySingleRow(DataRow r, double creditDivisor, double bigWinMul)
        {
            _ = bigWinMul;
            if (IsJackpotPrizeRow(r))
                return "彩金";
            var ot = GetInt(r, "open_type");
            var rt = GetInt(r, "result_type");
            if (rt == ResultTypeBonusWin)
                return "大奖";
            if (ot == OpenTypeGive || rt == ResultTypeFreeWin)
                return "免费";
            return ClassifyWinLose(r, creditDivisor);
        }

        /// <summary>彩金：实发 JP 分或 result_type 为 Jackpot / JackpotOnline（不含 BonusWin，BonusWin 归为「大奖」）。</summary>
        static bool IsJackpotPrizeRow(DataRow r)
        {
            if (GetLong(r, "jackpot_win_credit") > 0)
                return true;
            var rt = GetInt(r, "result_type");
            return rt == ResultTypeJackpot || rt == ResultTypeJackpotOnline;
        }

        /// <summary>主游戏非彩金、非免费、非大奖时仅「赢」「输」（净变化≤0 为输）。</summary>
        static string ClassifyWinLose(DataRow r, double creditDivisor)
        {
            var cb = Scale(GetLong(r, "credit_before"), creditDivisor);
            var ca = Scale(GetLong(r, "credit_after"), creditDivisor);
            var d = ca - cb;
            return d > 0 ? "赢" : "输";
        }

        static int GetInt(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
                return 0;
            var v = row[columnName];
            if (v == null || v == DBNull.Value)
                return 0;
            return Convert.ToInt32(v, CultureInfo.InvariantCulture);
        }

        static long GetLong(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
                return 0;
            var v = row[columnName];
            if (v == null || v == DBNull.Value)
                return 0;
            return Convert.ToInt64(v, CultureInfo.InvariantCulture);
        }

        static double Scale(long v, double divisor)
        {
            if (divisor <= 0)
                divisor = 1.0;
            return v / divisor;
        }

        static string EscapeCsv(object value)
        {
            if (value == null)
                return "";
            var s = value is IFormattable fmt
                ? fmt.ToString(null, CultureInfo.InvariantCulture)
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            if (s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        /// <summary>将查询结果按 id 升序排列（在「先取最近 N 条」子查询之后调用）。</summary>
        public static DataRow[] SortRowsChronological(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return Array.Empty<DataRow>();
            var list = new List<DataRow>(dt.Rows.Count);
            foreach (DataRow r in dt.Rows)
                list.Add(r);
            list.Sort((a, b) => GetLong(a, "id").CompareTo(GetLong(b, "id")));
            return list.ToArray();
        }
    }
}
