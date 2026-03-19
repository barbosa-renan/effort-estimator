using EffortEstimator.Core.Domain.Enums;

namespace EffortEstimator.Core.Application.Dtos;

public record ExternalIntegrationsInput
{
    public int Count { get; init; } = 0;
    public IntegrationComplexityLevel Complexity { get; init; } = IntegrationComplexityLevel.Low;
}
