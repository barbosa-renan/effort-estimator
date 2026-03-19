namespace EffortEstimator.API.Models.Requests;

public record EstimateRequest
{
    public string TaskDescription       { get; init; } = string.Empty;
    public string TechnicalComplexity   { get; init; } = "moderate";
    public string TeamKnowledge         { get; init; } = "intermediate";
    public ExternalIntegrationsRequest ExternalIntegrations { get; init; } = new();
    public ExternalDependenciesRequest ExternalDependencies { get; init; } = new();
}

public record ExternalIntegrationsRequest
{
    public int    Count      { get; init; } = 0;
    public string Complexity { get; init; } = "low";
}

public record ExternalDependenciesRequest
{
    public int    Count           { get; init; } = 0;
    public string TeamReliability { get; init; } = "medium";
}
