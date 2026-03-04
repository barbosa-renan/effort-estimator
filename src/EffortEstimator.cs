using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EffortEstimator
{
    public class ExternalIntegrations
    {
        [JsonPropertyName("count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("complexity")]
        public string Complexity { get; set; } = "low";
    }

    public class ExternalDependencies
    {
        [JsonPropertyName("count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("team_reliability")]
        public string TeamReliability { get; set; } = "medium";
    }

    public class TaskInput
    {
        [JsonPropertyName("task_description")]
        public string TaskDescription { get; set; } = string.Empty;

        [JsonPropertyName("technical_complexity")]
        public string TechnicalComplexity { get; set; } = "moderate";

        [JsonPropertyName("team_knowledge")]
        public string TeamKnowledge { get; set; } = "intermediate";

        [JsonPropertyName("external_integrations")]
        public ExternalIntegrations ExternalIntegrations { get; set; } = new();

        [JsonPropertyName("external_dependencies")]
        public ExternalDependencies ExternalDependencies { get; set; } = new();
    }

    public class ConfidenceRange
    {
        public double Low  { get; init; }
        public double High { get; init; }
    }

    public class PertResult
    {
        public string TaskDescription   { get; init; } = string.Empty;
        public double Optimistic        { get; init; }
        public double MostLikely        { get; init; }
        public double Pessimistic       { get; init; }
        public double PertHours         { get; init; }
        public double StandardDeviation { get; init; }
        public double Variance          { get; init; }
        public int    StoryPoints       { get; init; }
        public ConfidenceRange ConfidenceRange { get; init; } = new();
        public string RiskLevel         { get; init; } = string.Empty;
    }

    public static class PertEngine
    {
        private static readonly int[]    Fibonacci     = { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };
        private static readonly double[] FibThresholds = { 2, 4, 8, 16, 28, 48, 80, 130, 200 };

        private static readonly Dictionary<string, (double O, double M, double P)> ComplexityBase = new()
        {
            ["trivial"]      = (0.5, 1,  2  ),
            ["simple"]       = (1,   3,  6  ),
            ["moderate"]     = (3,   8,  16 ),
            ["complex"]      = (8,   20, 40 ),
            ["very_complex"] = (20,  48, 100),
        };

        private static readonly Dictionary<string, (double O, double M, double P)> KnowledgeMultipliers = new()
        {
            ["expert"]       = (0.8, 0.9, 1.0),
            ["intermediate"] = (1.0, 1.0, 1.2),
            ["beginner"]     = (1.3, 1.6, 2.5),
            ["unknown"]      = (1.2, 1.5, 2.8),
        };

        private static readonly Dictionary<string, double> IntegrationComplexityMultiplier = new()
        {
            ["low"]    = 1.1,
            ["medium"] = 1.3,
            ["high"]   = 1.6,
        };

        private static readonly Dictionary<string, double> ReliabilityRisk = new()
        {
            ["high"]   = 0.05,
            ["medium"] = 0.15,
            ["low"]    = 0.35,
        };

        public static PertResult Estimate(TaskInput input)
        {
            var key = input.TechnicalComplexity?.ToLowerInvariant() ?? "moderate";
            if (!ComplexityBase.TryGetValue(key, out var baseHours))
                baseHours = ComplexityBase["moderate"];

            double O = baseHours.O;
            double M = baseHours.M;
            double P = baseHours.P;

            int intCount = input.ExternalIntegrations?.Count ?? 0;
            if (intCount > 0)
            {
                var intKey = input.ExternalIntegrations!.Complexity?.ToLowerInvariant() ?? "low";
                if (!IntegrationComplexityMultiplier.TryGetValue(intKey, out double complexMult))
                    complexMult = 1.1;

                double intMult = 1 + intCount * (complexMult - 1);

                O *= 1 + (intMult - 1) * 0.5;
                M *= intMult;
                P *= intMult * 1.2;
            }

            var knowledgeKey = input.TeamKnowledge?.ToLowerInvariant() ?? "unknown";
            if (!KnowledgeMultipliers.TryGetValue(knowledgeKey, out var km))
                km = KnowledgeMultipliers["unknown"];

            O *= km.O;
            M *= km.M;
            P *= km.P;

            int depCount = input.ExternalDependencies?.Count ?? 0;
            if (depCount > 0)
            {
                var relKey = input.ExternalDependencies!.TeamReliability?.ToLowerInvariant() ?? "medium";
                if (!ReliabilityRisk.TryGetValue(relKey, out double risk))
                    risk = 0.15;

                double depPenalty = 1 + depCount * risk;
                M *= 1 + (depPenalty - 1) * 0.6;
                P *= depPenalty;
            }

            double pertHours = (O + 4 * M + P) / 6.0;
            double stdDev    = (P - O) / 6.0;
            double variance  = stdDev * stdDev;
            double coefVar   = stdDev / pertHours;

            string riskLevel = coefVar < 0.3 ? "Baixo"
                             : coefVar < 0.6 ? "Médio"
                             : "Alto";

            return new PertResult
            {
                TaskDescription   = input.TaskDescription,
                Optimistic        = Round(O),
                MostLikely        = Round(M),
                Pessimistic       = Round(P),
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

        private static double Round(double value) => Math.Round(value, 1);

        private static int MapToFibonacci(double hours)
        {
            for (int i = 0; i < FibThresholds.Length; i++)
                if (hours <= FibThresholds[i])
                    return Fibonacci[i];

            return Fibonacci[^1];
        }
    }
}