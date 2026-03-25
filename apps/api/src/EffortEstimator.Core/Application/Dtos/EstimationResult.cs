using EffortEstimator.Core.Domain.Enums;
using EffortEstimator.Core.Domain.ValueObjects;

namespace EffortEstimator.Core.Application.Dtos;

public record EstimationResult
{
    public string? TaskDescription  { get; init; }
    public double Optimistic        { get; init; }
    public double MostLikely        { get; init; }
    public double Pessimistic       { get; init; }
    public double PertHours         { get; init; }
    public double StandardDeviation { get; init; }
    public double Variance          { get; init; }
    public int    StoryPoints       { get; init; }
    public ConfidenceRange ConfidenceRange { get; init; } = new();
    public RiskLevel RiskLevel      { get; init; }
}
