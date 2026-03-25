using EffortEstimator.Core.Domain.Enums;

namespace EffortEstimator.Core.Application.Dtos;

public record EstimationInput
{
    public string? TaskDescription { get; init; }
    public TechnicalComplexityLevel TechnicalComplexity { get; init; } = TechnicalComplexityLevel.Moderate;
    public TeamKnowledgeLevel TeamKnowledge { get; init; } = TeamKnowledgeLevel.Unknown;
    public ExternalIntegrationsInput ExternalIntegrations { get; init; } = new();
    public ExternalDependenciesInput ExternalDependencies { get; init; } = new();
}
