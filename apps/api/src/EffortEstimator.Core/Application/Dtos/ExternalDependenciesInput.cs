using EffortEstimator.Core.Domain.Enums;

namespace EffortEstimator.Core.Application.Dtos;

public record ExternalDependenciesInput
{
    public int Count { get; init; } = 0;
    public ReliabilityLevel TeamReliability { get; init; } = ReliabilityLevel.Medium;
}
