using FluentAssertions;
using Xunit;
using EffortEstimator.PertEstimator;

namespace EffortEstimator.PertEstimator.Tests.Services;

public class PertEngineTests
{
    private static TaskInput BuildInput(
        string technicalComplexity   = "moderate",
        string teamKnowledge         = "intermediate",
        int    integrationCount      = 0,
        string integrationComplexity = "low",
        int    dependencyCount       = 0,
        string dependencyReliability = "medium")
        => new()
        {
            TechnicalComplexity  = technicalComplexity,
            TeamKnowledge        = teamKnowledge,
            ExternalIntegrations = new ExternalIntegrations
            {
                Count      = integrationCount,
                Complexity = integrationComplexity,
            },
            ExternalDependencies = new ExternalDependencies
            {
                Count           = dependencyCount,
                TeamReliability = dependencyReliability,
            },
        };

    [Fact]
    public void Estimate_TrivialComplexityWithExpertTeam_ReturnsLowHoursAndLowRisk()
    {
        var input = BuildInput(technicalComplexity: "trivial", teamKnowledge: "expert");

        var result = PertEngine.Estimate(input);

        result.PertHours.Should().Be(1.0);
        result.StoryPoints.Should().Be(1);
        result.RiskLevel.Should().Be("Baixo");
        result.Optimistic.Should().Be(0.4);
        result.MostLikely.Should().Be(0.9);
        result.Pessimistic.Should().Be(2.0);
    }

    [Fact]
    public void Estimate_ModerateComplexityWithIntermediateTeam_ReturnsBaselineEstimate()
    {
        var input = BuildInput(technicalComplexity: "moderate", teamKnowledge: "intermediate");

        var result = PertEngine.Estimate(input);

        result.PertHours.Should().Be(9.0);
        result.StoryPoints.Should().Be(5);
        result.StandardDeviation.Should().Be(2.7);
        result.RiskLevel.Should().Be("Baixo");
    }

    [Fact]
    public void Estimate_VeryComplexWithBeginnerTeam_ReturnsHighHoursAndHighStoryPoints()
    {
        var input = BuildInput(technicalComplexity: "very_complex", teamKnowledge: "beginner");

        var result = PertEngine.Estimate(input);

        result.PertHours.Should().Be(97.2);
        result.StoryPoints.Should().Be(34);
        result.Optimistic.Should().Be(26.0);
        result.MostLikely.Should().Be(76.8);
        result.Pessimistic.Should().Be(250.0);
    }

    [Fact]
    public void Estimate_ExpertTeam_ProducesLowerEstimateThanIntermediateTeam()
    {
        var expertInput       = BuildInput(teamKnowledge: "expert");
        var intermediateInput = BuildInput(teamKnowledge: "intermediate");

        var expertResult       = PertEngine.Estimate(expertInput);
        var intermediateResult = PertEngine.Estimate(intermediateInput);

        expertResult.PertHours.Should().BeLessThan(intermediateResult.PertHours);
    }

    [Fact]
    public void Estimate_UnknownKnowledge_ProducesHigherPessimisticThanBeginner()
    {
        // unknown carries maximum epistemic uncertainty — P multiplier is 2.8 vs 2.5
        var unknownInput  = BuildInput(teamKnowledge: "unknown");
        var beginnerInput = BuildInput(teamKnowledge: "beginner");

        var unknownResult  = PertEngine.Estimate(unknownInput);
        var beginnerResult = PertEngine.Estimate(beginnerInput);

        unknownResult.Pessimistic.Should().BeGreaterThan(beginnerResult.Pessimistic);
    }

    [Fact]
    public void Estimate_WithTwoHighComplexityIntegrations_InflatesPessimisticMoreThanOptimistic()
    {
        var withIntegrations    = BuildInput(integrationCount: 2, integrationComplexity: "high");
        var withoutIntegrations = BuildInput();

        var withResult    = PertEngine.Estimate(withIntegrations);
        var withoutResult = PertEngine.Estimate(withoutIntegrations);

        var optimisticGrowth   = withResult.Optimistic   / withoutResult.Optimistic;
        var pessimisticGrowth  = withResult.Pessimistic  / withoutResult.Pessimistic;

        pessimisticGrowth.Should().BeGreaterThan(optimisticGrowth);
    }

    [Fact]
    public void Estimate_ComplexWithTwoHighIntegrationsAndOneMediumDependency_MatchesExpectedValues()
    {
        var input = BuildInput(
            technicalComplexity:   "complex",
            teamKnowledge:         "intermediate",
            integrationCount:      2,
            integrationComplexity: "high",
            dependencyCount:       1,
            dependencyReliability: "medium");

        var result = PertEngine.Estimate(input);

        result.PertHours.Should().Be(58.4);
        result.StoryPoints.Should().Be(21);
        result.StandardDeviation.Should().Be(22.2);
        result.RiskLevel.Should().Be("Médio");
        result.Optimistic.Should().Be(12.8);
        result.MostLikely.Should().Be(48.0);
        result.Pessimistic.Should().Be(145.7);
    }

    [Fact]
    public void Estimate_WithDependencies_DoesNotAffectOptimistic()
    {
        var withDependencies    = BuildInput(dependencyCount: 3, dependencyReliability: "low");
        var withoutDependencies = BuildInput();

        var withResult    = PertEngine.Estimate(withDependencies);
        var withoutResult = PertEngine.Estimate(withoutDependencies);

        // Optimistic is untouched by dependencies — best case assumes no blocking
        withResult.Optimistic.Should().Be(withoutResult.Optimistic);
    }

    [Fact]
    public void Estimate_ThreeLowReliabilityDependencies_InflatesMostLikelyAndPessimistic()
    {
        var input = BuildInput(
            technicalComplexity:  "moderate",
            teamKnowledge:        "intermediate",
            dependencyCount:      3,
            dependencyReliability: "low");

        var result = PertEngine.Estimate(input);

        result.PertHours.Should().Be(15.8);
        result.MostLikely.Should().Be(13.0);
        result.Pessimistic.Should().Be(39.4);
    }

    [Fact]
    public void Estimate_ConfidenceRange_IsSymmetricAroundPertHours()
    {
        var input = BuildInput(technicalComplexity: "moderate", teamKnowledge: "intermediate");

        var result = PertEngine.Estimate(input);

        result.ConfidenceRange.Low.Should().Be(6.3);
        result.ConfidenceRange.High.Should().Be(11.7);
        result.ConfidenceRange.Low.Should().BeLessThan(result.PertHours);
        result.ConfidenceRange.High.Should().BeGreaterThan(result.PertHours);
    }

    [Fact]
    public void Estimate_UnknownComplexityValue_FallsBackToModerate()
    {
        var unknownComplexity = BuildInput(technicalComplexity: "nonexistent");
        var moderate          = BuildInput(technicalComplexity: "moderate");

        var unknownResult  = PertEngine.Estimate(unknownComplexity);
        var moderateResult = PertEngine.Estimate(moderate);

        // same base hours — fallback to moderate
        unknownResult.Optimistic.Should().Be(moderateResult.Optimistic);
        unknownResult.MostLikely.Should().Be(moderateResult.MostLikely);
    }

    [Fact]
    public void Estimate_UnknownKnowledgeValue_FallsBackToUnknownMultiplier()
    {
        var unknownKnowledge   = BuildInput(teamKnowledge: "nonexistent");
        var explicitUnknown    = BuildInput(teamKnowledge: "unknown");

        var unknownResult  = PertEngine.Estimate(unknownKnowledge);
        var explicitResult = PertEngine.Estimate(explicitUnknown);

        unknownResult.PertHours.Should().Be(explicitResult.PertHours);
        unknownResult.Pessimistic.Should().Be(explicitResult.Pessimistic);
    }

    [Fact]
    public void Estimate_AnyValidInput_PertHoursIsAlwaysPositive()
    {
        var inputs = new[]
        {
            BuildInput("trivial",      "expert"),
            BuildInput("simple",       "intermediate"),
            BuildInput("moderate",     "beginner"),
            BuildInput("complex",      "unknown"),
            BuildInput("very_complex", "beginner"),
        };

        foreach (var input in inputs)
        {
            var result = PertEngine.Estimate(input);
            result.PertHours.Should().BePositive(because: $"complexity={input.TechnicalComplexity}");
        }
    }

    [Fact]
    public void Estimate_AnyValidInput_OptimisticIsAlwaysLessThanPessimistic()
    {
        var inputs = new[]
        {
            BuildInput("trivial",      "expert"),
            BuildInput("moderate",     "intermediate", integrationCount: 2, integrationComplexity: "high"),
            BuildInput("complex",      "beginner",     dependencyCount: 3,  dependencyReliability: "low"),
            BuildInput("very_complex", "unknown"),
        };

        foreach (var input in inputs)
        {
            var result = PertEngine.Estimate(input);
            result.Optimistic.Should().BeLessThan(result.Pessimistic,
                because: $"complexity={input.TechnicalComplexity}, knowledge={input.TeamKnowledge}");
        }
    }

    [Fact]
    public void Estimate_AnyValidInput_StoryPointsIsAlwaysPositiveFibonacciValue()
    {
        int[] validFibonacci = { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };

        var input  = BuildInput("complex", "beginner", integrationCount: 3, integrationComplexity: "high");
        var result = PertEngine.Estimate(input);

        validFibonacci.Should().Contain(result.StoryPoints);
    }

    [Fact]
    public void Estimate_TaskDescription_IsPreservedInResult()
    {
        var input = BuildInput();
        input.TaskDescription = "Implement payment gateway integration";

        var result = PertEngine.Estimate(input);

        result.TaskDescription.Should().Be("Implement payment gateway integration");
    }
}