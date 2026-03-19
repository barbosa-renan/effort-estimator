using EffortEstimator.API.Models.Requests;
using EffortEstimator.API.Models.Responses;
using EffortEstimator.Core.Application.Dtos;
using EffortEstimator.Core.Domain.Enums;

namespace EffortEstimator.API.Mappers;

public static class EstimationMapper
{
    public static EstimationInput ToEstimationInput(EstimateRequest request) =>
        new()
        {
            TaskDescription    = request.TaskDescription,
            TechnicalComplexity = ParseOrDefault(request.TechnicalComplexity, TechnicalComplexityLevel.Moderate),
            TeamKnowledge       = ParseOrDefault(request.TeamKnowledge,        TeamKnowledgeLevel.Unknown),
            ExternalIntegrations = new ExternalIntegrationsInput
            {
                Count      = request.ExternalIntegrations.Count,
                Complexity = ParseOrDefault(request.ExternalIntegrations.Complexity, IntegrationComplexityLevel.Low),
            },
            ExternalDependencies = new ExternalDependenciesInput
            {
                Count           = request.ExternalDependencies.Count,
                TeamReliability = ParseOrDefault(request.ExternalDependencies.TeamReliability, ReliabilityLevel.Medium),
            },
        };

    public static EstimateResponse ToEstimateResponse(EstimationResult result) =>
        new(
            result.TaskDescription,
            result.Optimistic,
            result.MostLikely,
            result.Pessimistic,
            result.PertHours,
            result.StandardDeviation,
            result.Variance,
            result.StoryPoints,
            new ConfidenceRangeResponse(result.ConfidenceRange.Low, result.ConfidenceRange.High),
            result.RiskLevel.ToString());

    private static TEnum ParseOrDefault<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum =>
        !string.IsNullOrEmpty(value) && Enum.TryParse<TEnum>(ToPascalCase(value), ignoreCase: true, out TEnum parsed)
            ? parsed
            : fallback;

    private static string ToPascalCase(string value) =>
        string.Concat(value.Split('_').Select(part => char.ToUpper(part[0]) + part[1..]));
}
