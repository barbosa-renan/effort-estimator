namespace EffortEstimator.API.Models.Responses;

public record EstimateResponse(
    string  TaskDescription,
    double  Optimistic,
    double  MostLikely,
    double  Pessimistic,
    double  PertHours,
    double  StandardDeviation,
    double  Variance,
    int     StoryPoints,
    ConfidenceRangeResponse ConfidenceRange,
    string  RiskLevel);

public record ConfidenceRangeResponse(double Low, double High);
