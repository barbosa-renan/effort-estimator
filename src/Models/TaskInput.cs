using System.Text.Json.Serialization;

namespace EffortEstimator.Models;

// Strings kept as JSON deserialization boundary — enums are used internally in PertEngine
public record TaskInput
{
    [JsonPropertyName("task_description")]      public string               TaskDescription      { get; init; } = string.Empty;
    [JsonPropertyName("technical_complexity")]  public string               TechnicalComplexity  { get; init; } = "moderate";
    [JsonPropertyName("team_knowledge")]        public string               TeamKnowledge        { get; init; } = "intermediate";
    [JsonPropertyName("external_integrations")] public ExternalIntegrations ExternalIntegrations { get; init; } = new();
    [JsonPropertyName("external_dependencies")] public ExternalDependencies ExternalDependencies { get; init; } = new();
}
