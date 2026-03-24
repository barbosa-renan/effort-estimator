namespace EffortEstimator.Core.Domain.ValueObjects;

public record ConfidenceRange
{
    public double Low  { get; init; }
    public double High { get; init; }
}
