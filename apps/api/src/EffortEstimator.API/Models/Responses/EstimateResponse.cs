namespace EffortEstimator.API.Models.Responses;

/// <summary>
/// PERT-based effort estimation result for a software task.
/// </summary>
public record EstimateResponse
{
    /// <summary>
    /// The task description provided in the request, echoed back for reference. Null when not provided.
    /// </summary>
    /// <example>Implement OAuth2 login flow with Google provider</example>
    public string? TaskDescription { get; init; }

    /// <summary>
    /// Optimistic hours estimate — the best-case scenario after applying all multipliers.
    /// </summary>
    /// <example>6.4</example>
    public double Optimistic { get; init; }

    /// <summary>
    /// Most-likely hours estimate — the realistic scenario after applying all multipliers.
    /// </summary>
    /// <example>20.0</example>
    public double MostLikely { get; init; }

    /// <summary>
    /// Pessimistic hours estimate — the worst-case scenario after applying all multipliers.
    /// </summary>
    /// <example>48.0</example>
    public double Pessimistic { get; init; }

    /// <summary>
    /// PERT weighted average hours: (O + 4M + P) / 6.
    /// This is the primary effort estimate to use for planning.
    /// </summary>
    /// <example>22.7</example>
    public double PertHours { get; init; }

    /// <summary>
    /// Standard deviation of the estimate: (P − O) / 6.
    /// Indicates how spread out the uncertainty is.
    /// </summary>
    /// <example>6.9</example>
    public double StandardDeviation { get; init; }

    /// <summary>
    /// Variance of the estimate (StandardDeviation²).
    /// </summary>
    /// <example>47.6</example>
    public double Variance { get; init; }

    /// <summary>
    /// Closest Fibonacci Story Point value mapped from PertHours.
    /// Fibonacci scale: 1, 2, 3, 5, 8, 13, 21, 34, 55, 89.
    /// </summary>
    /// <example>21</example>
    public int StoryPoints { get; init; }

    /// <summary>
    /// 68% confidence interval (±1 standard deviation around PertHours).
    /// </summary>
    public ConfidenceRangeResponse ConfidenceRange { get; init; } = new();

    /// <summary>
    /// Categorical risk level derived from the coefficient of variation (σ / μ).
    /// Possible values: <c>Low</c>, <c>Medium</c>, <c>High</c>.
    /// </summary>
    /// <example>Medium</example>
    public string RiskLevel { get; init; } = string.Empty;
}

/// <summary>
/// 68% confidence interval around the PERT estimate (±1 standard deviation).
/// </summary>
public record ConfidenceRangeResponse
{
    /// <summary>
    /// Lower bound of the confidence interval (PertHours − StandardDeviation).
    /// </summary>
    /// <example>15.8</example>
    public double Low { get; init; }

    /// <summary>
    /// Upper bound of the confidence interval (PertHours + StandardDeviation).
    /// </summary>
    /// <example>29.6</example>
    public double High { get; init; }
}
