using System.ComponentModel.DataAnnotations;

namespace EffortEstimator.API.Models.Requests;

/// <summary>
/// Payload for a PERT-based effort estimation request.
/// </summary>
public record EstimateRequest
{
    /// <summary>
    /// Short description of the task to estimate (used as a label in the response).
    /// </summary>
    /// <example>Implement OAuth2 login flow with Google provider</example>
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string TaskDescription { get; init; } = string.Empty;

    /// <summary>
    /// Technical complexity of the task.
    /// Accepted values: <c>trivial</c>, <c>simple</c>, <c>moderate</c>, <c>complex</c>, <c>very_complex</c>.
    /// Defaults to <c>moderate</c> when omitted or unrecognised.
    /// </summary>
    /// <example>complex</example>
    [StringLength(20)]
    public string TechnicalComplexity { get; init; } = "moderate";

    /// <summary>
    /// Knowledge level of the team working on the task.
    /// Accepted values: <c>expert</c>, <c>intermediate</c>, <c>beginner</c>, <c>unknown</c>.
    /// Defaults to <c>intermediate</c> when omitted or unrecognised.
    /// </summary>
    /// <example>intermediate</example>
    [StringLength(20)]
    public string TeamKnowledge { get; init; } = "intermediate";

    /// <summary>
    /// External system integrations required by the task (e.g. third-party APIs, message brokers).
    /// </summary>
    public ExternalIntegrationsRequest ExternalIntegrations { get; init; } = new();

    /// <summary>
    /// External team dependencies the task relies on (e.g. another squad, external vendor).
    /// </summary>
    public ExternalDependenciesRequest ExternalDependencies { get; init; } = new();
}

/// <summary>
/// Describes the external system integrations required by the task.
/// </summary>
public record ExternalIntegrationsRequest
{
    /// <summary>
    /// Number of distinct external systems the task must integrate with.
    /// </summary>
    /// <example>2</example>
    [Range(0, 50)]
    public int Count { get; init; } = 0;

    /// <summary>
    /// Complexity of each integration.
    /// Accepted values: <c>low</c>, <c>medium</c>, <c>high</c>.
    /// Defaults to <c>low</c> when omitted or unrecognised.
    /// </summary>
    /// <example>medium</example>
    [StringLength(20)]
    public string Complexity { get; init; } = "low";
}

/// <summary>
/// Describes external team dependencies the task relies on.
/// </summary>
public record ExternalDependenciesRequest
{
    /// <summary>
    /// Number of external teams or vendors the task depends on.
    /// </summary>
    /// <example>1</example>
    [Range(0, 50)]
    public int Count { get; init; } = 0;

    /// <summary>
    /// Historical reliability of the external teams.
    /// Accepted values: <c>high</c>, <c>medium</c>, <c>low</c>.
    /// Defaults to <c>medium</c> when omitted or unrecognised.
    /// </summary>
    /// <example>medium</example>
    [StringLength(20)]
    public string TeamReliability { get; init; } = "medium";
}
