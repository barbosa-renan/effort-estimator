using System.Text.Json.Serialization;

namespace EffortEstimator.Models;

public record ExternalIntegrations
{
    [JsonPropertyName("count")]      public int    Count      { get; init; } = 0;
    [JsonPropertyName("complexity")] public string Complexity { get; init; } = "low";
}
