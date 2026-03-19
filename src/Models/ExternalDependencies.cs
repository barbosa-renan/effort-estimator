using System.Text.Json.Serialization;

namespace EffortEstimator.Models;

public record ExternalDependencies
{
    [JsonPropertyName("count")]            public int    Count           { get; init; } = 0;
    [JsonPropertyName("team_reliability")] public string TeamReliability { get; init; } = "medium";
}
