using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GameMaker
{
    /// <summary>
    /// Excel 2003 SpreadsheetML（.xml，Excel 可直接打开）：游戏表 + 着色汇总，
    /// 汇总锚定在「最后一条游戏记录」同一行起、第 K 列（紧挨游戏表右侧）。
    /// </summary>
    public static class SlotGameRecordSpreadsheetXml
    {
        /// <summary>汇总区起始列（1-based）= K，即最后一条游戏记录「右下角」右侧。</summary>
        public const int SummaryStartColumn = 11;

        /// <summary>写入 UTF-8（无 BOM）SpreadsheetML。</summary>
        public static void WriteStyledWorkbook(string path, SlotGameRecordExport.SlotGameRecordExportData data,
            string summaryBlock2Title)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var block2 = string.IsNullOrWhiteSpace(summaryBlock2Title)
                ? "大奖"
                : summaryBlock2Title.Trim();

            var sb = new StringBuilder(64000);
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
            sb.Append("<?mso-application progid=\"Excel.Sheet\"?>\r\n");
            sb.Append("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
            sb.Append("xmlns:o=\"urn:schemas-microsoft-com:office:office\" ");
            sb.Append("xmlns:x=\"urn:schemas-microsoft-com:office:excel\" ");
            sb.Append("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
            sb.Append("xmlns:html=\"http://www.w3.org/TR/REC-html40\">\r\n");
            AppendStyles(sb);
            sb.Append("<Worksheet ss:Name=\"游戏记录\">\r\n");
            sb.Append("<Table>\r\n");

            AppendColumnWidths(sb);

            var inv = CultureInfo.InvariantCulture;
            var excelRow = 1;
            AppendGameHeaderRow(sb, ref excelRow);

            var gr = data.GameRows;
            var m = data.Metrics;
            if (gr.Count == 0)
            {
                sb.Append("</Table>\r\n</Worksheet>\r\n</Workbook>");
                WriteFile(path, sb);
                return;
            }

            for (var i = 0; i < gr.Count - 1; i++)
            {
                AppendGameDataRow(sb, gr[i], zebra: (excelRow & 1) == 0);
                excelRow++;
            }

            var last = gr[gr.Count - 1];
            var firstSummary = BuildFirstSummaryLineCells(m, out var sumBet, out var sumWin, out var n, out var loses,
                out var loseP, out var rtp, out var br, out var nr);
            AppendLastGameRowWithSummaryHead(sb, last, (excelRow & 1) == 0, firstSummary);
            excelRow++;

            AppendSummaryContinuation(sb, ref excelRow, m, block2, inv, n, loses, loseP, sumBet, sumWin, rtp, br, nr);

            sb.Append("</Table>\r\n</Worksheet>\r\n</Workbook>");
            WriteFile(path, sb);
        }

        static void WriteFile(string path, StringBuilder sb)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        static void AppendStyles(StringBuilder sb)
        {
            sb.Append("<Styles>\r\n");
            sb.Append(
                "<Style ss:ID=\"Gh\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#4472C4\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"Gd\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\"/><Interior ss:Color=\"#FFFFFF\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#D0D7E5\"/></Borders></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"GdZ\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\"/><Interior ss:Color=\"#D9E1F2\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#D0D7E5\"/></Borders></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"SxL\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#366092\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"SxV\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\"/><Interior ss:Color=\"#FFFFFF\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#CCCCCC\"/></Borders></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"Dh\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\" ss:Bold=\"1\"/><Interior ss:Color=\"#D6DCE4\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"Sec\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\" ss:Bold=\"1\"/><Interior ss:Color=\"#FFC000\" ss:Pattern=\"Solid\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"Sd\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\"/><Interior ss:Color=\"#FFFFFF\" ss:Pattern=\"Solid\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"SdZ\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\"/><Interior ss:Color=\"#F2F2F2\" ss:Pattern=\"Solid\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"St\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\" ss:Bold=\"1\"/><Interior ss:Color=\"#E2EFDA\" ss:Pattern=\"Solid\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"Tot\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\" ss:Bold=\"1\"/><Interior ss:Color=\"#92D050\" ss:Pattern=\"Solid\"/></Style>\r\n");
            sb.Append(
                "<Style ss:ID=\"Emp\"><Font ss:FontName=\"Calibri\" ss:Size=\"11\"/><Interior ss:Color=\"#FFFFFF\" ss:Pattern=\"Solid\"/></Style>\r\n");
            sb.Append("</Styles>\r\n");
        }

        static void AppendColumnWidths(StringBuilder sb)
        {
            for (var c = 0; c < 10; c++)
                sb.Append("<Column ss:AutoFitWidth=\"0\" ss:Width=\"72\"/>\r\n");
            for (var c = 0; c < 12; c++)
                sb.Append("<Column ss:AutoFitWidth=\"0\" ss:Width=\"68\"/>\r\n");
        }

        static void AppendGameHeaderRow(StringBuilder sb, ref int excelRow)
        {
            sb.Append("<Row ss:AutoFitHeight=\"0\">\r\n");
            foreach (var h in SlotGameRecordExport._Header)
                AppendStringCell(sb, "Gh", h);
            sb.Append("</Row>\r\n");
            excelRow++;
        }

        static void AppendGameDataRow(StringBuilder sb, SlotGameRecordExport.SlotGameRecordCsvRow g, bool zebra)
        {
            var st = zebra ? "Gd" : "GdZ";
            sb.Append("<Row ss:AutoFitHeight=\"0\">\r\n");
            AppendGameCells(sb, g, st);
            sb.Append("</Row>\r\n");
        }

        static void AppendGameCells(StringBuilder sb, SlotGameRecordExport.SlotGameRecordCsvRow g, string st)
        {
            AppendStringCell(sb, st, g.ColA);
            AppendNumberCell(sb, st, g.CreditBefore);
            AppendNumberCell(sb, st, g.TotalBet);
            AppendNumberCell(sb, st, g.BaseGameWin);
            AppendNumberCell(sb, st, g.FreeSpinWin);
            AppendNumberCell(sb, st, g.BonusGameWin);
            if (g.JackpotWin == 0)
                AppendStringCell(sb, st, "");
            else
                AppendNumberCell(sb, st, g.JackpotWin);
            AppendNumberCell(sb, st, g.TotalWin);
            AppendNumberCell(sb, st, g.Delta);
            AppendNumberCell(sb, st, g.CreditAfter);
        }

        static void AppendLastGameRowWithSummaryHead(StringBuilder sb, SlotGameRecordExport.SlotGameRecordCsvRow g,
            bool zebra, string[] firstSummaryLabels)
        {
            var st = zebra ? "Gd" : "GdZ";
            sb.Append("<Row ss:AutoFitHeight=\"0\">\r\n");
            AppendGameCells(sb, g, st);
            for (var i = 0; i < firstSummaryLabels.Length; i++)
            {
                if (i == 0)
                    sb.Append("<Cell ss:Index=\"11\" ss:StyleID=\"SxL\">");
                else
                    sb.Append("<Cell ss:StyleID=\"SxL\">");
                sb.Append("<Data ss:Type=\"String\">");
                sb.Append(XmlEsc(firstSummaryLabels[i]));
                sb.Append("</Data></Cell>\r\n");
            }

            sb.Append("</Row>\r\n");
        }

        static string[] BuildFirstSummaryLineCells(IList<SlotGameRecordExport.SlotGameRecordSummaryMetric> m,
            out double sumBet, out double sumWin, out int n, out int loses, out double loseP, out double rtp,
            out double br, out double nr)
        {
            n = m.Count;
            loses = 0;
            sumBet = 0;
            sumWin = 0;
            var bigWin = 0.0;
            var normalWin = 0.0;
            foreach (var x in m)
            {
                sumBet += x.Bet;
                sumWin += x.WinJp;
                if (string.Equals(x.AwardLabel, "输", StringComparison.Ordinal))
                    loses++;
                if (string.Equals(x.AwardLabel, "大奖", StringComparison.Ordinal))
                    bigWin += x.WinJp;
                if (string.Equals(x.AwardLabel, "赢", StringComparison.Ordinal) ||
                    string.Equals(x.AwardLabel, "输", StringComparison.Ordinal))
                    normalWin += x.WinJp;
            }

            loseP = n > 0 ? (double)loses / n : 0.0;
            rtp = sumBet > 0 ? sumWin / sumBet : 0.0;
            br = sumBet > 0 ? bigWin / sumBet : 0.0;
            nr = sumBet > 0 ? normalWin / sumBet : 0.0;
            return new[]
            {
                "统计项",
                "总局",
                "输局",
                "输局概率",
                "总玩分",
                "总得分",
                "合计RTP",
                "大奖RTP",
                "普通游戏RTP",
            };
        }

        static void AppendSummaryContinuation(StringBuilder sb, ref int excelRow,
            IList<SlotGameRecordExport.SlotGameRecordSummaryMetric> m, string block2, CultureInfo inv, int n, int loses,
            double loseP, double sumBet, double sumWin, double rtp, double br, double nr)
        {
            AppendSummaryRow9FromK(sb, ref excelRow, "SxV", new[]
            {
                "数值",
                n.ToString(inv),
                loses.ToString(inv),
                loseP.ToString("F6", inv),
                sumBet.ToString("F4", inv),
                sumWin.ToString("F4", inv),
                rtp.ToString("F6", inv),
                br.ToString("F6", inv),
                nr.ToString("F6", inv),
            }, numbersFromIndex: 1);

            AppendBlankSummaryRow(sb, ref excelRow);

            AppendSummaryRow6FromK(sb, ref excelRow, "Dh",
                new[] { "类型", "局", "赢分", "平均（倍）", "出现概率", "RTP返还率" });

            var normal = new List<SlotGameRecordExport.SlotGameRecordSummaryMetric>();
            var big = new List<SlotGameRecordExport.SlotGameRecordSummaryMetric>();
            var jpRows = new List<SlotGameRecordExport.SlotGameRecordSummaryMetric>();
            var free = new List<SlotGameRecordExport.SlotGameRecordSummaryMetric>();
            foreach (var x in m)
            {
                if (string.Equals(x.AwardLabel, "赢", StringComparison.Ordinal) ||
                    string.Equals(x.AwardLabel, "输", StringComparison.Ordinal))
                    normal.Add(x);
                else if (string.Equals(x.AwardLabel, "大奖", StringComparison.Ordinal))
                    big.Add(x);
                else if (string.Equals(x.AwardLabel, "彩金", StringComparison.Ordinal))
                    jpRows.Add(x);
                else if (string.Equals(x.AwardLabel, "免费", StringComparison.Ordinal))
                    free.Add(x);
            }

            AppendBucketedSectionXml(sb, ref excelRow, "普通", normal, n, sumBet,
                SlotGameRecordExport.SlotGameRecordSummaryBucketScheme.NormalMainGame);
            AppendBlankSummaryRow(sb, ref excelRow);
            AppendBucketedSectionXml(sb, ref excelRow, block2, big, n, sumBet,
                SlotGameRecordExport.SlotGameRecordSummaryBucketScheme.BigWinFeature);
            AppendBlankSummaryRow(sb, ref excelRow);
            AppendJackpotSectionXml(sb, ref excelRow, jpRows, n, sumBet);
            AppendBlankSummaryRow(sb, ref excelRow);
            AppendBucketedSectionXml(sb, ref excelRow, "免费", free, n, sumBet,
                SlotGameRecordExport.SlotGameRecordSummaryBucketScheme.FreeGameFeature);
            AppendBlankSummaryRow(sb, ref excelRow);

            var avgTot = n > 0 ? sumWin / n : 0.0;
            AppendSummaryRow6FromK(sb, ref excelRow, "Tot",
                new[]
                {
                    "合计",
                    n.ToString(inv),
                    sumWin.ToString("F4", inv),
                    avgTot.ToString("F4", inv),
                    "1",
                    rtp.ToString("F6", inv),
                });
        }

        static void AppendSummaryRow9FromK(StringBuilder sb, ref int excelRow, string styleId, string[] cells,
            int numbersFromIndex)
        {
            sb.Append("<Row ss:AutoFitHeight=\"0\">\r\n");
            for (var i = 0; i < cells.Length; i++)
            {
                if (i == 0)
                    sb.Append("<Cell ss:Index=\"11\" ss:StyleID=\"").Append(styleId).Append("\">");
                else
                    sb.Append("<Cell ss:StyleID=\"").Append(styleId).Append("\">");
                double dv = 0;
                if (i >= numbersFromIndex &&
                    double.TryParse(cells[i], NumberStyles.Float, CultureInfo.InvariantCulture, out dv))
                {
                    sb.Append("<Data ss:Type=\"Number\">");
                    sb.Append(dv.ToString(CultureInfo.InvariantCulture));
                    sb.Append("</Data></Cell>\r\n");
                }
                else
                {
                    sb.Append("<Data ss:Type=\"String\">");
                    sb.Append(XmlEsc(cells[i]));
                    sb.Append("</Data></Cell>\r\n");
                }
            }

            sb.Append("</Row>\r\n");
            excelRow++;
        }

        static void AppendSummaryRow6FromK(StringBuilder sb, ref int excelRow, string styleId, string[] cells)
        {
            sb.Append("<Row ss:AutoFitHeight=\"0\">\r\n");
            for (var i = 0; i < cells.Length; i++)
            {
                if (i == 0)
                    sb.Append("<Cell ss:Index=\"11\" ss:StyleID=\"").Append(styleId).Append("\">");
                else
                    sb.Append("<Cell ss:StyleID=\"").Append(styleId).Append("\">");
                double dv = 0;
                var isNum = i > 0 &&
                            double.TryParse(cells[i], NumberStyles.Float, CultureInfo.InvariantCulture, out dv);
                if (isNum)
                {
                    sb.Append("<Data ss:Type=\"Number\">");
                    sb.Append(dv.ToString(CultureInfo.InvariantCulture));
                    sb.Append("</Data></Cell>\r\n");
                }
                else
                {
                    sb.Append("<Data ss:Type=\"String\">");
                    sb.Append(XmlEsc(cells[i]));
                    sb.Append("</Data></Cell>\r\n");
                }
            }

            sb.Append("</Row>\r\n");
            excelRow++;
        }

        static void AppendBlankSummaryRow(StringBuilder sb, ref int excelRow)
        {
            sb.Append("<Row ss:AutoFitHeight=\"0\"><Cell ss:Index=\"11\" ss:StyleID=\"Emp\"><Data ss:Type=\"String\"></Data></Cell></Row>\r\n");
            excelRow++;
        }

        static void AppendBucketedSectionXml(StringBuilder sb, ref int excelRow, string title,
            List<SlotGameRecordExport.SlotGameRecordSummaryMetric> subset, int totalRounds, double totalBet,
            SlotGameRecordExport.SlotGameRecordSummaryBucketScheme scheme)
        {
            var inv = CultureInfo.InvariantCulture;
            AppendSummaryRow6FromK(sb, ref excelRow, "Sec", new[] { title, "", "", "", "", "" });

            var nBucket = SlotGameRecordExport.SlotGameRecordSummaryBucketCount;
            var buckets = new List<SlotGameRecordExport.SlotGameRecordSummaryMetric>[nBucket];
            for (var i = 0; i < nBucket; i++)
                buckets[i] = new List<SlotGameRecordExport.SlotGameRecordSummaryMetric>();
            foreach (var x in subset)
                buckets[SlotGameRecordExport.GetSummaryBucketIndex(x.Mult, scheme)].Add(x);

            for (var b = 0; b < nBucket; b++)
            {
                var list = buckets[b];
                var nc = list.Count;
                double sw = 0;
                foreach (var x in list)
                    sw += x.WinJp;
                var avg = nc > 0 ? sw / nc : 0.0;
                var prob = totalRounds > 0 ? (double)nc / totalRounds : 0.0;
                var rtp = totalBet > 0 ? sw / totalBet : 0.0;
                var st = (b & 1) == 0 ? "Sd" : "SdZ";
                AppendSummaryRow6FromK(sb, ref excelRow, st,
                    new[]
                    {
                        SlotGameRecordExport.GetSummaryBucketLabel(b, scheme),
                        nc.ToString(inv),
                        sw.ToString("F4", inv),
                        avg.ToString("F4", inv),
                        prob.ToString("F6", inv),
                        rtp.ToString("F6", inv),
                    });
            }

            var nAll = subset.Count;
            double swAll = 0;
            foreach (var x in subset)
                swAll += x.WinJp;
            var avgAll = nAll > 0 ? swAll / nAll : 0.0;
            var probAll = totalRounds > 0 ? (double)nAll / totalRounds : 0.0;
            var rtpAll = totalBet > 0 ? swAll / totalBet : 0.0;
            AppendSummaryRow6FromK(sb, ref excelRow, "St",
                new[]
                {
                    "小计",
                    nAll.ToString(inv),
                    swAll.ToString("F4", inv),
                    avgAll.ToString("F4", inv),
                    probAll.ToString("F6", inv),
                    rtpAll.ToString("F6", inv),
                });
        }

        static void AppendJackpotSectionXml(StringBuilder sb, ref int excelRow,
            List<SlotGameRecordExport.SlotGameRecordSummaryMetric> subset, int totalRounds, double totalBet)
        {
            var inv = CultureInfo.InvariantCulture;
            AppendSummaryRow6FromK(sb, ref excelRow, "Sec", new[] { "彩金", "", "", "", "", "" });
            if (subset.Count == 0)
            {
                AppendSummaryRow6FromK(sb, ref excelRow, "Sd",
                    new[] { "（无数据）", "0", "0", "0", "0", "0" });
                return;
            }

            var n = subset.Count;
            double sw = 0;
            foreach (var x in subset)
                sw += x.WinJp;
            var avg = n > 0 ? sw / n : 0.0;
            var prob = totalRounds > 0 ? (double)n / totalRounds : 0.0;
            var rtp = totalBet > 0 ? sw / totalBet : 0.0;
            AppendSummaryRow6FromK(sb, ref excelRow, "St",
                new[]
                {
                    "小计",
                    n.ToString(inv),
                    sw.ToString("F4", inv),
                    avg.ToString("F4", inv),
                    prob.ToString("F6", inv),
                    rtp.ToString("F6", inv),
                });
        }

        static void AppendStringCell(StringBuilder sb, string styleId, string text)
        {
            sb.Append("<Cell ss:StyleID=\"").Append(styleId).Append("\"><Data ss:Type=\"String\">");
            sb.Append(XmlEsc(text ?? ""));
            sb.Append("</Data></Cell>\r\n");
        }

        static void AppendNumberCell(StringBuilder sb, string styleId, double v)
        {
            sb.Append("<Cell ss:StyleID=\"").Append(styleId).Append("\"><Data ss:Type=\"Number\">");
            sb.Append(v.ToString(CultureInfo.InvariantCulture));
            sb.Append("</Data></Cell>\r\n");
        }

        static string XmlEsc(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
