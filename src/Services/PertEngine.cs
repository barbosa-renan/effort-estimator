using System;
using System.Collections.Generic;
using System.Linq;
using EffortEstimator.Models;
using EffortEstimator.Models.Enums;

namespace EffortEstimator.Services;

public class PertEngine
{
    private const double PertFormulaDenominator      = 6.0;
    private const double PertMostLikelyWeight        = 4.0;
    private const double OptimisticIntegrationScale  = 0.5;
    private const double PessimisticIntegrationScale = 1.2;
    private const double DependencyPenaltyScale      = 0.6;
    private const double LowRiskThreshold            = 0.3;
    private const double HighRiskThreshold           = 0.6;
    private const int    RoundingDecimalPlaces        = 1;

    private static readonly int[]    Fibonacci     = { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };
    private static readonly double[] FibThresholds = { 2, 4, 8, 16, 28, 48, 80, 130, 200 };

    private readonly Dictionary<TechnicalComplexityLevel, (double O, double M, double P)> _complexityBase = new()
    {
        [TechnicalComplexityLevel.Trivial]     = (0.5, 1,   2  ),
        [TechnicalComplexityLevel.Simple]      = (1,   3,   6  ),
        [TechnicalComplexityLevel.Moderate]    = (3,   8,   16 ),
        [TechnicalComplexityLevel.Complex]     = (8,   20,  40 ),
        [TechnicalComplexityLevel.VeryComplex] = (20,  48,  100),
    };

    private readonly Dictionary<TeamKnowledgeLevel, (double O, double M, double P)> _knowledgeMultipliers = new()
    {
        [TeamKnowledgeLevel.Expert]       = (0.8, 0.9, 1.0),
        [TeamKnowledgeLevel.Intermediate] = (1.0, 1.0, 1.2),
        [TeamKnowledgeLevel.Beginner]     = (1.3, 1.6, 2.5),
        [TeamKnowledgeLevel.Unknown]      = (1.2, 1.5, 2.8),
    };

    private readonly Dictionary<IntegrationComplexityLevel, double> _integrationComplexityMultiplier = new()
    {
        [IntegrationComplexityLevel.Low]    = 1.1,
        [IntegrationComplexityLevel.Medium] = 1.3,
        [IntegrationComplexityLevel.High]   = 1.6,
    };

    private readonly Dictionary<ReliabilityLevel, double> _reliabilityRisk = new()
    {
        [ReliabilityLevel.High]   = 0.05,
        [ReliabilityLevel.Medium] = 0.15,
        [ReliabilityLevel.Low]    = 0.35,
    };

    public PertResult Estimate(TaskInput input)
    {
        var complexity = ParseOrDefault(input.TechnicalComplexity, TechnicalComplexityLevel.Moderate);
        var baseHours  = _complexityBase[complexity];

        double optimistic  = baseHours.O;
        double mostLikely  = baseHours.M;
        double pessimistic = baseHours.P;

        int integrationCount = input.ExternalIntegrations.Count;
        if (integrationCount > 0)
        {
            var integrationComplexity = ParseOrDefault(input.ExternalIntegrations.Complexity, IntegrationComplexityLevel.Low);
            double complexMult        = _integrationComplexityMultiplier[integrationComplexity];
            double integrationMult    = 1 + integrationCount * (complexMult - 1);

            optimistic  *= 1 + (integrationMult - 1) * OptimisticIntegrationScale;
            mostLikely  *= integrationMult;
            pessimistic *= integrationMult * PessimisticIntegrationScale;
        }

        var knowledge          = ParseOrDefault(input.TeamKnowledge, TeamKnowledgeLevel.Unknown);
        var knowledgeMult      = _knowledgeMultipliers[knowledge];
        optimistic  *= knowledgeMult.O;
        mostLikely  *= knowledgeMult.M;
        pessimistic *= knowledgeMult.P;

        int dependencyCount = input.ExternalDependencies.Count;
        if (dependencyCount > 0)
        {
            var reliability   = ParseOrDefault(input.ExternalDependencies.TeamReliability, ReliabilityLevel.Medium);
            double risk        = _reliabilityRisk[reliability];
            double depPenalty  = 1 + dependencyCount * risk;
            mostLikely  *= 1 + (depPenalty - 1) * DependencyPenaltyScale;
            pessimistic *= depPenalty;
        }

        double pertHours      = (optimistic + PertMostLikelyWeight * mostLikely + pessimistic) / PertFormulaDenominator;
        double stdDev         = (pessimistic - optimistic) / PertFormulaDenominator;
        double variance       = stdDev * stdDev;
        double coefVar        = stdDev / pertHours;

        var riskLevel = coefVar < LowRiskThreshold ? RiskLevel.Low
                      : coefVar < HighRiskThreshold ? RiskLevel.Medium
                      : RiskLevel.High;

        return new PertResult
        {
            TaskDescription   = input.TaskDescription,
            Optimistic        = Round(optimistic),
            MostLikely        = Round(mostLikely),
            Pessimistic       = Round(pessimistic),
            PertHours         = Round(pertHours),
            StandardDeviation = Round(stdDev),
            Variance          = Round(variance),
            StoryPoints       = MapToFibonacci(pertHours),
            ConfidenceRange   = new ConfidenceRange
            {
                Low  = Round(pertHours - stdDev),
                High = Round(pertHours + stdDev),
            },
            RiskLevel = riskLevel,
        };
    }

    private static double Round(double value) => Math.Round(value, RoundingDecimalPlaces);

    private static int MapToFibonacci(double hours)
    {
        for (int i = 0; i < FibThresholds.Length; i++)
            if (hours <= FibThresholds[i])
                return Fibonacci[i];

        return Fibonacci[^1];
    }

    private static string ToPascalCase(string value) =>
        string.Concat(value.Split('_').Select(part => char.ToUpper(part[0]) + part[1..]));

    private static TEnum ParseOrDefault<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum =>
        !string.IsNullOrEmpty(value) && Enum.TryParse<TEnum>(ToPascalCase(value), ignoreCase: true, out var result)
            ? result
            : fallback;
}
