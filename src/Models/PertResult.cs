using EffortEstimator.Models.Enums;

namespace EffortEstimator.Models;

public record PertResult
{
    public string          TaskDescription   { get; init; } = string.Empty;
    public double          Optimistic        { get; init; }
    public double          MostLikely        { get; init; }
    public double          Pessimistic       { get; init; }
    public double          PertHours         { get; init; }
    public double          StandardDeviation { get; init; }
    public double          Variance          { get; init; }
    public int             StoryPoints       { get; init; }
    public ConfidenceRange ConfidenceRange   { get; init; } = new();
    public RiskLevel       RiskLevel         { get; init; }
}
