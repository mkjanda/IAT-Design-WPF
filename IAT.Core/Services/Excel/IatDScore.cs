using IAT.Core.ResultData;

namespace IAT.Core.Services.Excel;

/// <summary>
/// Greenwald, Nosek &amp; Banaji (2003) improved algorithm, D score.
/// Blocks 3+4 are the first combined pairing, 6+7 the reversed pairing.
/// Sign is (second pairing − first pairing) / inclusive SD.
/// Interpret the sign from the block order of the deployed test, not from this class.
/// </summary>
public static class IatDScore
{
    public const long MaxLatencyMs = 10_000;
    public const long FastTrialMs = 300;
    public const double FastTrialExclusionRate = 0.10;
    public const long ErrorPenaltyMs = 600;

    public static IatDScoreResult Compute(IATResult? iat)
    {
        if (iat is null)
            return IatDScoreResult.Excluded("No IAT result payload.", 0, 0, 0, 0);

        var raw = iat.Fragments ?? [];
        var afterCap = raw.Where(f => f.ResponseTime <= MaxLatencyMs).ToList();
        var droppedSlow = raw.Count - afterCap.Count;

        if (afterCap.Count == 0)
        {
            return IatDScoreResult.Excluded("No trials at or under 10,000 ms.", raw.Count, 0, droppedSlow, 0);
        }

        var fast = afterCap.Count(f => f.ResponseTime < FastTrialMs);
        var fastRate = (double)fast / afterCap.Count;
        if (fastRate > FastTrialExclusionRate)
        {
            return IatDScoreResult.Excluded(
                $">10% of trials were under 300 ms ({fastRate:P1}).",
                raw.Count, afterCap.Count, droppedSlow, fast);
        }

        double? D1 = PairD(afterCap, 3, 6, out var m3, out var m6, out var sd36, out var n36);
        double? D2 = PairD(afterCap, 4, 7, out var m4, out var m7, out var sd47, out var n47);

        if (D1 is null || D2 is null)
        {
            return IatDScoreResult.Excluded(
                "A combined block pair was empty or had no variance after recoding.",
                raw.Count, afterCap.Count, droppedSlow, fast,
                meanBlock3: m3, meanBlock4: m4, meanBlock6: m6, meanBlock7: m7,
                sd36: sd36, sd47: sd47);
        }

        return new IatDScoreResult
        {
            Included = true,
            D = (D1.Value + D2.Value) / 2.0,
            DPractice = D1,
            DTest = D2,
            MeanBlock3 = m3,
            MeanBlock4 = m4,
            MeanBlock6 = m6,
            MeanBlock7 = m7,
            Sd36 = sd36,
            Sd47 = sd47,
            TrialCount = raw.Count,
            TrialsUsed = afterCap.Count,
            DroppedOver10000 = droppedSlow,
            FastUnder300 = fast,
            ErrorCount = raw.Count(f => f.Error),
            ExclusionReason = null
        };
    }

    /// <summary>
    /// Difference of recoded block means (later − earlier) divided by the inclusive
    /// SD of the two raw blocks (correct + error, after the 10,000 ms cap).
    /// </summary>
    private static double? PairD(
        List<IATResultFragment> trials,
        int firstBlock,
        int secondBlock,
        out double? meanFirst,
        out double? meanSecond,
        out double? pooledSd,
        out int n)
    {
        meanFirst = meanSecond = pooledSd = null;
        var a = trials.Where(f => f.BlockNum == firstBlock).ToList();
        var b = trials.Where(f => f.BlockNum == secondBlock).ToList();
        n = a.Count + b.Count;
        if (a.Count == 0 || b.Count == 0)
            return null;

        var rawLatencies = a.Concat(b).Select(f => (double)f.ResponseTime).ToList();
        var sd = SampleSd(rawLatencies);
        pooledSd = sd;
        if (sd is null || sd.Value == 0)
            return null;

        meanFirst = RecodedMean(a);
        meanSecond = RecodedMean(b);
        if (meanFirst is null || meanSecond is null)
            return null;

        return (meanSecond.Value - meanFirst.Value) / sd.Value;
    }

    private static double? RecodedMean(List<IATResultFragment> block)
    {
        var correct = block.Where(f => !f.Error).Select(f => (double)f.ResponseTime).ToList();
        if (correct.Count == 0)
            return null;
        var meanCorrect = correct.Average();
        var recoded = block.Select(f => f.Error ? meanCorrect + ErrorPenaltyMs : f.ResponseTime);
        return recoded.Average();
    }

    private static double? SampleSd(List<double> values)
    {
        if (values.Count < 2)
            return null;
        var mean = values.Average();
        var sumSq = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sumSq / (values.Count - 1));
    }
}


public sealed class IatDScoreResult
{
    public bool Included { get; init; }
    public double? D { get; init; }
    public double? DPractice { get; init; }
    public double? DTest { get; init; }
    public double? MeanBlock3 { get; init; }
    public double? MeanBlock4 { get; init; }
    public double? MeanBlock6 { get; init; }
    public double? MeanBlock7 { get; init; }
    public double? Sd36 { get; init; }
    public double? Sd47 { get; init; }
    public int TrialCount { get; init; }
    public int TrialsUsed { get; init; }
    public int DroppedOver10000 { get; init; }
    public int FastUnder300 { get; init; }
    public int ErrorCount { get; init; }
    public string? ExclusionReason { get; init; }

    public static IatDScoreResult Excluded(
    string reason,
    int trialCount,
    int used,
    int droppedSlow,
    int fast,
    double? meanBlock3 = null,
    double? meanBlock4 = null,
    double? meanBlock6 = null,
    double? meanBlock7 = null,
    double? sd36 = null,
    double? sd47 = null) => new()
    {
        Included = false,
        TrialCount = trialCount,
        TrialsUsed = used,
        DroppedOver10000 = droppedSlow,
        FastUnder300 = fast,
        ExclusionReason = reason,
        MeanBlock3 = meanBlock3,
        MeanBlock4 = meanBlock4,
        MeanBlock6 = meanBlock6,
        MeanBlock7 = meanBlock7,
        Sd36 = sd36,
        Sd47 = sd47
    };
}