using ClosedXML.Excel;
using IAT.Core.ResultData;
using System.IO;

namespace IAT.Core.Services.Excel;

/// <summary>
/// Builds the results workbook. ClosedXML stays in this file.
/// Call <see cref="Write(IReadOnlyList{AdministrationExport},Stream)"/> after decrypt.
/// </summary>
public sealed class IatResultsExcelExporter
{
    public const string SummarySheet = "Summary";
    public const string LatenciesSheet = "Latencies";
    public const string BlocksSheet = "Block means";
    public const string ItemsSheet = "Items";
    public const string NotesSheet = "Notes";

    public void Write(IReadOnlyList<AdministrationExport> administrations, string path)
    {
        using var stream = File.Create(path);
        Write(administrations, stream);
    }

    public void Write(IReadOnlyList<AdministrationExport> administrations, Stream output)
    {
        ArgumentNullException.ThrowIfNull(administrations);
        ArgumentNullException.ThrowIfNull(output);

        var scored = administrations
            .Select(a => (Row: a, Score: IatDScore.Compute(a.Results.IATResult)))
            .ToList();

        using var wb = new XLWorkbook();
        WriteSummary(wb.AddWorksheet(SummarySheet), scored);
        WriteLatencies(wb.AddWorksheet(LatenciesSheet), scored);
        WriteBlockMeans(wb.AddWorksheet(BlocksSheet), scored);
        WriteItems(wb.AddWorksheet(ItemsSheet), scored);
        WriteNotes(wb.AddWorksheet(NotesSheet));
        wb.SaveAs(output);
    }

    private static void WriteSummary(
        IXLWorksheet ws,
        List<(AdministrationExport Row, IatDScoreResult Score)> scored)
    {
        var surveyColumns = DiscoverSurveyColumns(scored.Select(s => s.Row));
        var hasToken = scored.Any(s => !string.IsNullOrWhiteSpace(s.Row.Token));

        var headers = new List<string> { "Result ID", "Admin time" };
        if (hasToken) headers.Add("Token");
        headers.AddRange(surveyColumns.Select(c => c.Header));
        headers.AddRange(
        [
            "IAT D",
            "D practice (B6−B3)",
            "D test (B7−B4)",
            "Included",
            "Exclusion reason",
            "Error count",
            "Trials",
            "Trials used",
            "Dropped >10s",
            "Trials <300ms"
        ]);

        WriteHeaderRow(ws, headers);

        for (var r = 0; r < scored.Count; r++)
        {
            var (row, score) = scored[r];
            var excelRow = r + 2;
            var col = 1;
            ws.Cell(excelRow, col++).Value = row.ResultId;
            var timeCell = ws.Cell(excelRow, col++);
            if (row.AdminTime is { } t)
            {
                timeCell.Value = t.UtcDateTime;
                timeCell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            }

            if (hasToken)
                ws.Cell(excelRow, col++).Value = row.Token ?? "";

            var answers = FlattenAnswers(row.Results);
            foreach (var key in surveyColumns)
            {
                answers.TryGetValue(key, out var value);
                WriteAnswer(ws.Cell(excelRow, col++), value);
            }

            WriteOptionalNumber(ws.Cell(excelRow, col++), score.D, "0.000");
            WriteOptionalNumber(ws.Cell(excelRow, col++), score.DPractice, "0.000");
            WriteOptionalNumber(ws.Cell(excelRow, col++), score.DTest, "0.000");
            ws.Cell(excelRow, col++).Value = score.Included;
            ws.Cell(excelRow, col++).Value = score.ExclusionReason ?? "";
            ws.Cell(excelRow, col++).Value = score.ErrorCount;
            ws.Cell(excelRow, col++).Value = score.TrialCount;
            ws.Cell(excelRow, col++).Value = score.TrialsUsed;
            ws.Cell(excelRow, col++).Value = score.DroppedOver10000;
            ws.Cell(excelRow, col++).Value = score.FastUnder300;
        }

        FinishSheet(ws, freezeHeader: true);
    }

    private static void WriteLatencies(
        IXLWorksheet ws,
        List<(AdministrationExport Row, IatDScoreResult Score)> scored)
    {
        WriteHeaderRow(ws,
        [
            "Result ID", "Presentation #", "Block", "Item #", "Latency (ms)", "Error"
        ]);

        var excelRow = 2;
        foreach (var (row, _) in scored)
        {
            foreach (var frag in row.Results.IATResult.Fragments.OrderBy(f => f.PresentationNum))
            {
                ws.Cell(excelRow, 1).Value = row.ResultId;
                ws.Cell(excelRow, 2).Value = frag.PresentationNum;
                ws.Cell(excelRow, 3).Value = frag.BlockNum;
                ws.Cell(excelRow, 4).Value = frag.ItemNum;
                ws.Cell(excelRow, 5).Value = frag.ResponseTime;
                ws.Cell(excelRow, 6).Value = frag.Error;
                excelRow++;
            }
        }

        FinishSheet(ws, freezeHeader: true);
    }

    private static void WriteBlockMeans(
        IXLWorksheet ws,
        List<(AdministrationExport Row, IatDScoreResult Score)> scored)
    {
        var headers = new List<string> { "Result ID", "Included", "D" };
        for (var b = 1; b <= 7; b++)
        {
            headers.Add($"B{b} N");
            headers.Add($"B{b} mean ms");
            headers.Add($"B{b} errors");
        }
        headers.AddRange(["SD B3+B6", "SD B4+B7"]);
        WriteHeaderRow(ws, headers);

        for (var r = 0; r < scored.Count; r++)
        {
            var (row, score) = scored[r];
            var excelRow = r + 2;
            ws.Cell(excelRow, 1).Value = row.ResultId;
            ws.Cell(excelRow, 2).Value = score.Included;
            WriteOptionalNumber(ws.Cell(excelRow, 3), score.D, "0.000");

            var col = 4;
            for (var b = 1; b <= 7; b++)
            {
                var block = row.Results.IATResult.Fragments.Where(f => f.BlockNum == b).ToList();
                ws.Cell(excelRow, col++).Value = block.Count;
                if (block.Count > 0)
                    WriteOptionalNumber(ws.Cell(excelRow, col++), block.Average(f => f.ResponseTime), "0.0");
                else
                    col++;
                ws.Cell(excelRow, col++).Value = block.Count(f => f.Error);
            }

            WriteOptionalNumber(ws.Cell(excelRow, col++), score.Sd36, "0.0");
            WriteOptionalNumber(ws.Cell(excelRow, col++), score.Sd47, "0.0");
        }

        FinishSheet(ws, freezeHeader: true);
    }

    private static void WriteItems(
        IXLWorksheet ws,
        List<(AdministrationExport Row, IatDScoreResult Score)> scored)
    {
        WriteHeaderRow(ws,
        [
            "Item #", "N", "Mean ms", "Median ms", "Error rate",
            "N in B3", "Mean B3", "N in B4", "Mean B4", "N in B6", "Mean B6", "N in B7", "Mean B7"
        ]);

        var frags = scored.SelectMany(s => s.Row.Results.IATResult.Fragments).ToList();
        var items = frags.Select(f => f.ItemNum).Distinct().OrderBy(n => n).ToList();

        var excelRow = 2;
        foreach (var item in items)
        {
            var rows = frags.Where(f => f.ItemNum == item).ToList();
            var latencies = rows.Select(f => (double)f.ResponseTime).OrderBy(v => v).ToList();
            ws.Cell(excelRow, 1).Value = item;
            ws.Cell(excelRow, 2).Value = rows.Count;
            WriteOptionalNumber(ws.Cell(excelRow, 3), latencies.Average(), "0.0");
            WriteOptionalNumber(ws.Cell(excelRow, 4), Median(latencies), "0.0");
            WriteOptionalNumber(ws.Cell(excelRow, 5), rows.Count == 0 ? null : rows.Count(f => f.Error) / (double)rows.Count, "0.0%");

            var col = 6;
            foreach (var block in new[] { 3, 4, 6, 7 })
            {
                var subset = rows.Where(f => f.BlockNum == block).ToList();
                ws.Cell(excelRow, col++).Value = subset.Count;
                if (subset.Count > 0)
                    WriteOptionalNumber(ws.Cell(excelRow, col++), subset.Average(f => f.ResponseTime), "0.0");
                else
                    col++;
            }
            excelRow++;
        }

        FinishSheet(ws, freezeHeader: true);
    }

    private static void WriteNotes(IXLWorksheet ws)
    {
        ws.Cell(1, 1).Value = "IAT results workbook";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;

        var lines = new[]
        {
            "",
            "Scoring: Greenwald, Nosek & Banaji (2003), improved algorithm (D).",
            "Blocks 3 and 4 are the first combined pairing. Blocks 6 and 7 are the reversed pairing.",
            "D practice = (mean B6 − mean B3) / inclusive SD of B3+B6.",
            "D test     = (mean B7 − mean B4) / inclusive SD of B4+B7.",
            "IAT D is the average of those two quotients.",
            "",
            "Inclusion rules applied to every administration:",
            "  1. Trials slower than 10,000 ms are dropped before anything else.",
            "  2. If more than 10% of remaining trials are faster than 300 ms, the row is excluded (IAT D blank).",
            "  3. Error trials are recoded to that block’s correct-trial mean + 600 ms before the block mean is taken.",
            "  4. Inclusive SDs use correct and error trials after the 10,000 ms cap, before recoding.",
            "",
            "The sign of D is (second pairing − first pairing). Which association that favors depends on the order of keys in the deployed test.",
            "Unscored rows stay on Summary so survey answers are never silently dropped.",
            "",
            "Items sheet is descriptive only: mean/median latency and error rate by stimulus number. It does not drop items or compute loadings.",
            "Survey columns are named from SurveyName + item index. Empty survey names become Survey 1, Survey 2, …",
            "Answers that look numeric are written as numbers; everything else stays text. 'Unanswered' and 'NULL' stay text."
        };

        for (var i = 0; i < lines.Length; i++)
            ws.Cell(i + 2, 1).Value = lines[i];

        ws.Column(1).Width = 140;
        ws.SheetView.FreezeRows(1);
    }

    private static List<SurveyColumn> DiscoverSurveyColumns(IEnumerable<AdministrationExport> rows)
    {
        var seen = new Dictionary<SurveyColumn, int>();
        var order = new List<SurveyColumn>();
        foreach (var row in rows)
        {
            var surveys = row.Results.SurveyResults;
            for (var s = 0; s < surveys.Count; s++)
            {
                var survey = surveys[s];
                var name = string.IsNullOrWhiteSpace(survey.SurveyName)
                    ? $"Survey {s + 1}"
                    : survey.SurveyName.Trim();
                for (var i = 0; i < survey.Answers.Count; i++)
                {
                    var key = new SurveyColumn(name, i);
                    if (seen.ContainsKey(key))
                        continue;
                    seen[key] = order.Count;
                    order.Add(key);
                }
            }
        }
        return order;
    }

    private static Dictionary<SurveyColumn, string> FlattenAnswers(ResultSet results)
    {
        var map = new Dictionary<SurveyColumn, string>();
        for (var s = 0; s < results.SurveyResults.Count; s++)
        {
            var survey = results.SurveyResults[s];
            var name = string.IsNullOrWhiteSpace(survey.SurveyName)
                ? $"Survey {s + 1}"
                : survey.SurveyName.Trim();
            for (var i = 0; i < survey.Answers.Count; i++)
                map[new SurveyColumn(name, i)] = survey.Answers[i];
        }
        return map;
    }

    private static void WriteAnswer(IXLCell cell, string? value)
    {
        if (string.IsNullOrEmpty(value) ||
            value is "Unanswered" or "NULL" or "Unaswered")
        {
            cell.Value = value ?? "";
            return;
        }

        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var n))
        {
            cell.Value = n;
            return;
        }

        cell.Value = value;
    }

    private static void WriteOptionalNumber(IXLCell cell, double? value, string format)
    {
        if (value is null)
            return;
        cell.Value = value.Value;
        cell.Style.NumberFormat.Format = format;
    }

    private static void WriteHeaderRow(IXLWorksheet ws, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            cell.Style.Font.FontColor = XLColor.White;
        }
    }

    private static void FinishSheet(IXLWorksheet ws, bool freezeHeader)
    {
        ws.SheetView.FreezeRows(freezeHeader ? 1 : 0);
        ws.RangeUsed()?.SetAutoFilter();
        ws.Columns().AdjustToContents(1, 40);
        ws.Row(1).Style.Alignment.WrapText = true;
    }

    private static double Median(List<double> sorted)
    {
        if (sorted.Count == 0)
            return double.NaN;
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }

    private readonly record struct SurveyColumn(string SurveyName, int ItemIndex)
    {
        public string Header => $"{SurveyName} Q{ItemIndex + 1}";
    }
}
