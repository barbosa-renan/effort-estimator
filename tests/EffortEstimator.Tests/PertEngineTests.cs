using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using EffortEstimator.Models;
using EffortEstimator.Models.Enums;
using EffortEstimator.Services;

namespace EffortEstimator.Tests;

public class PertEngineTests
{
    private readonly PertEngine _sut = new();

    private static TaskInput BuildInput(
        string taskDescription       = "",
        string technicalComplexity   = "moderate",
        string teamKnowledge         = "intermediate",
        int    integrationCount      = 0,
        string integrationComplexity = "low",
        int    dependencyCount       = 0,
        string dependencyReliability = "medium")
        => new()
        {
            TaskDescription      = taskDescription,
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
        // Arrange
        var input = BuildInput(technicalComplexity: "trivial", teamKnowledge: "expert");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().Be(1.0);
        result.StoryPoints.Should().Be(1);
        result.RiskLevel.Should().Be(RiskLevel.Low);
        result.Optimistic.Should().Be(0.4);
        result.MostLikely.Should().Be(0.9);
        result.Pessimistic.Should().Be(2.0);
    }

    [Fact]
    public void Estimate_ModerateComplexityWithIntermediateTeam_ReturnsBaselineEstimate()
    {
        // Arrange
        var input = BuildInput(technicalComplexity: "moderate", teamKnowledge: "intermediate");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().Be(9.0);
        result.StoryPoints.Should().Be(5);
        result.StandardDeviation.Should().Be(2.7);
        result.RiskLevel.Should().Be(RiskLevel.Low);
    }

    [Fact]
    public void Estimate_VeryComplexWithBeginnerTeam_ReturnsHighHoursAndHighStoryPoints()
    {
        // Arrange
        var input = BuildInput(technicalComplexity: "very_complex", teamKnowledge: "beginner");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().Be(97.2);
        result.StoryPoints.Should().Be(34);
        result.Optimistic.Should().Be(26.0);
        result.MostLikely.Should().Be(76.8);
        result.Pessimistic.Should().Be(250.0);
    }

    [Fact]
    public void Estimate_ExpertTeam_ProducesLowerEstimateThanIntermediateTeam()
    {
        // Arrange
        var expertInput       = BuildInput(teamKnowledge: "expert");
        var intermediateInput = BuildInput(teamKnowledge: "intermediate");

        // Act
        var expertResult       = _sut.Estimate(expertInput);
        var intermediateResult = _sut.Estimate(intermediateInput);

        // Assert
        expertResult.PertHours.Should().BeLessThan(intermediateResult.PertHours);
    }

    [Fact]
    public void Estimate_UnknownKnowledge_ProducesHigherPessimisticThanBeginner()
    {
        // unknown carries maximum epistemic uncertainty — P multiplier is 2.8 vs 2.5
        // Arrange
        var unknownInput  = BuildInput(teamKnowledge: "unknown");
        var beginnerInput = BuildInput(teamKnowledge: "beginner");

        // Act
        var unknownResult  = _sut.Estimate(unknownInput);
        var beginnerResult = _sut.Estimate(beginnerInput);

        // Assert
        unknownResult.Pessimistic.Should().BeGreaterThan(beginnerResult.Pessimistic);
    }

    [Fact]
    public void Estimate_WithTwoHighComplexityIntegrations_InflatesPessimisticMoreThanOptimistic()
    {
        // Arrange
        var withIntegrations    = BuildInput(integrationCount: 2, integrationComplexity: "high");
        var withoutIntegrations = BuildInput();

        // Act
        var withResult    = _sut.Estimate(withIntegrations);
        var withoutResult = _sut.Estimate(withoutIntegrations);

        // Assert
        var optimisticGrowth  = withResult.Optimistic  / withoutResult.Optimistic;
        var pessimisticGrowth = withResult.Pessimistic / withoutResult.Pessimistic;
        pessimisticGrowth.Should().BeGreaterThan(optimisticGrowth);
    }

    [Fact]
    public void Estimate_ComplexWithTwoHighIntegrationsAndOneMediumDependency_MatchesExpectedValues()
    {
        // Arrange
        var input = BuildInput(
            technicalComplexity:   "complex",
            teamKnowledge:         "intermediate",
            integrationCount:      2,
            integrationComplexity: "high",
            dependencyCount:       1,
            dependencyReliability: "medium");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().Be(58.4);
        result.StoryPoints.Should().Be(21);
        result.StandardDeviation.Should().Be(22.2);
        result.RiskLevel.Should().Be(RiskLevel.Medium);
        result.Optimistic.Should().Be(12.8);
        result.MostLikely.Should().Be(48.0);
        result.Pessimistic.Should().Be(145.7);
    }

    [Fact]
    public void Estimate_WithDependencies_DoesNotAffectOptimistic()
    {
        // Arrange
        var withDependencies    = BuildInput(dependencyCount: 3, dependencyReliability: "low");
        var withoutDependencies = BuildInput();

        // Act
        var withResult    = _sut.Estimate(withDependencies);
        var withoutResult = _sut.Estimate(withoutDependencies);

        // Assert
        // Optimistic is untouched by dependencies — best case assumes no blocking
        withResult.Optimistic.Should().Be(withoutResult.Optimistic);
    }

    [Fact]
    public void Estimate_ThreeLowReliabilityDependencies_InflatesMostLikelyAndPessimistic()
    {
        // Arrange
        var input = BuildInput(
            technicalComplexity:   "moderate",
            teamKnowledge:         "intermediate",
            dependencyCount:       3,
            dependencyReliability: "low");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().Be(15.8);
        result.MostLikely.Should().Be(13.0);
        result.Pessimistic.Should().Be(39.4);
    }

    [Fact]
    public void Estimate_ConfidenceRange_IsSymmetricAroundPertHours()
    {
        // Arrange
        var input = BuildInput(technicalComplexity: "moderate", teamKnowledge: "intermediate");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.ConfidenceRange.Low.Should().Be(6.3);
        result.ConfidenceRange.High.Should().Be(11.7);
        result.ConfidenceRange.Low.Should().BeLessThan(result.PertHours);
        result.ConfidenceRange.High.Should().BeGreaterThan(result.PertHours);
    }

    [Fact]
    public void Estimate_UnknownComplexityValue_FallsBackToModerate()
    {
        // Arrange
        var unknownComplexity = BuildInput(technicalComplexity: "nonexistent");
        var moderate          = BuildInput(technicalComplexity: "moderate");

        // Act
        var unknownResult  = _sut.Estimate(unknownComplexity);
        var moderateResult = _sut.Estimate(moderate);

        // Assert
        // same base hours — fallback to moderate
        unknownResult.Optimistic.Should().Be(moderateResult.Optimistic);
        unknownResult.MostLikely.Should().Be(moderateResult.MostLikely);
    }

    [Fact]
    public void Estimate_UnknownKnowledgeValue_FallsBackToUnknownMultiplier()
    {
        // Arrange
        var unknownKnowledge = BuildInput(teamKnowledge: "nonexistent");
        var explicitUnknown  = BuildInput(teamKnowledge: "unknown");

        // Act
        var unknownResult  = _sut.Estimate(unknownKnowledge);
        var explicitResult = _sut.Estimate(explicitUnknown);

        // Assert
        unknownResult.PertHours.Should().Be(explicitResult.PertHours);
        unknownResult.Pessimistic.Should().Be(explicitResult.Pessimistic);
    }

    public static IEnumerable<object[]> AllComplexityLevelInputs =>
    [
        [BuildInput(technicalComplexity: "trivial",      teamKnowledge: "expert")],
        [BuildInput(technicalComplexity: "simple",       teamKnowledge: "intermediate")],
        [BuildInput(technicalComplexity: "moderate",     teamKnowledge: "beginner")],
        [BuildInput(technicalComplexity: "complex",      teamKnowledge: "unknown")],
        [BuildInput(technicalComplexity: "very_complex", teamKnowledge: "beginner")],
    ];

    [Theory, MemberData(nameof(AllComplexityLevelInputs))]
    public void Estimate_AnyValidInput_PertHoursIsAlwaysPositive(TaskInput input)
    {
        // Arrange done via MemberData

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().BePositive();
    }

    public static IEnumerable<object[]> VariedInputsForInvariants =>
    [
        [BuildInput(technicalComplexity: "trivial",      teamKnowledge: "expert")],
        [BuildInput(technicalComplexity: "moderate",     teamKnowledge: "intermediate", integrationCount: 2, integrationComplexity: "high")],
        [BuildInput(technicalComplexity: "complex",      teamKnowledge: "beginner",     dependencyCount:   3, dependencyReliability: "low")],
        [BuildInput(technicalComplexity: "very_complex", teamKnowledge: "unknown")],
    ];

    [Theory, MemberData(nameof(VariedInputsForInvariants))]
    public void Estimate_AnyValidInput_OptimisticIsAlwaysLessThanPessimistic(TaskInput input)
    {
        // Arrange done via MemberData

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.Optimistic.Should().BeLessThan(result.Pessimistic,
            because: $"complexity={input.TechnicalComplexity}, knowledge={input.TeamKnowledge}");
    }

    [Fact]
    public void Estimate_AnyValidInput_StoryPointsIsAlwaysPositiveFibonacciValue()
    {
        // Arrange
        int[] validFibonacci = { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };
        var input            = BuildInput(technicalComplexity: "complex", teamKnowledge: "beginner", integrationCount: 3, integrationComplexity: "high");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        validFibonacci.Should().Contain(result.StoryPoints);
    }

    [Fact]
    public void Estimate_TaskDescription_IsPreservedInResult()
    {
        // Arrange
        var input = BuildInput(taskDescription: "Implement payment gateway integration");

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.TaskDescription.Should().Be("Implement payment gateway integration");
    }
}
