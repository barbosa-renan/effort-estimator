using FluentAssertions;
using Xunit;
using EffortEstimator.Core.Application.Dtos;
using EffortEstimator.Core.Application.Services;
using EffortEstimator.Core.Domain.Enums;

namespace EffortEstimator.Core.Tests;

public class PertEngineTests
{
    private readonly PertEngine _sut = new();

    private static EstimationInput BuildInput(
        string                    taskDescription      = "",
        TechnicalComplexityLevel  technicalComplexity  = TechnicalComplexityLevel.Moderate,
        TeamKnowledgeLevel        teamKnowledge        = TeamKnowledgeLevel.Intermediate,
        int                       integrationCount     = 0,
        IntegrationComplexityLevel integrationComplexity = IntegrationComplexityLevel.Low,
        int                       dependencyCount      = 0,
        ReliabilityLevel          dependencyReliability = ReliabilityLevel.Medium)
        => new()
        {
            TaskDescription      = taskDescription,
            TechnicalComplexity  = technicalComplexity,
            TeamKnowledge        = teamKnowledge,
            ExternalIntegrations = new ExternalIntegrationsInput
            {
                Count      = integrationCount,
                Complexity = integrationComplexity,
            },
            ExternalDependencies = new ExternalDependenciesInput
            {
                Count           = dependencyCount,
                TeamReliability = dependencyReliability,
            },
        };

    [Fact]
    public void Estimate_TrivialComplexityWithExpertTeam_ReturnsLowHoursAndLowRisk()
    {
        // Arrange
        var input = BuildInput(
            technicalComplexity: TechnicalComplexityLevel.Trivial,
            teamKnowledge:       TeamKnowledgeLevel.Expert);

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
        var input = BuildInput(
            technicalComplexity: TechnicalComplexityLevel.Moderate,
            teamKnowledge:       TeamKnowledgeLevel.Intermediate);

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
        var input = BuildInput(
            technicalComplexity: TechnicalComplexityLevel.VeryComplex,
            teamKnowledge:       TeamKnowledgeLevel.Beginner);

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
        var expertInput       = BuildInput(teamKnowledge: TeamKnowledgeLevel.Expert);
        var intermediateInput = BuildInput(teamKnowledge: TeamKnowledgeLevel.Intermediate);

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
        var unknownInput  = BuildInput(teamKnowledge: TeamKnowledgeLevel.Unknown);
        var beginnerInput = BuildInput(teamKnowledge: TeamKnowledgeLevel.Beginner);

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
        var withIntegrations    = BuildInput(integrationCount: 2, integrationComplexity: IntegrationComplexityLevel.High);
        var withoutIntegrations = BuildInput();

        // Act
        var withResult    = _sut.Estimate(withIntegrations);
        var withoutResult = _sut.Estimate(withoutIntegrations);

        // Assert
        double optimisticGrowth  = withResult.Optimistic  / withoutResult.Optimistic;
        double pessimisticGrowth = withResult.Pessimistic / withoutResult.Pessimistic;
        pessimisticGrowth.Should().BeGreaterThan(optimisticGrowth);
    }

    [Fact]
    public void Estimate_ComplexWithTwoHighIntegrationsAndOneMediumDependency_MatchesExpectedValues()
    {
        // Arrange
        var input = BuildInput(
            technicalComplexity:   TechnicalComplexityLevel.Complex,
            teamKnowledge:         TeamKnowledgeLevel.Intermediate,
            integrationCount:      2,
            integrationComplexity: IntegrationComplexityLevel.High,
            dependencyCount:       1,
            dependencyReliability: ReliabilityLevel.Medium);

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
        var withDependencies    = BuildInput(dependencyCount: 3, dependencyReliability: ReliabilityLevel.Low);
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
            technicalComplexity:   TechnicalComplexityLevel.Moderate,
            teamKnowledge:         TeamKnowledgeLevel.Intermediate,
            dependencyCount:       3,
            dependencyReliability: ReliabilityLevel.Low);

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
        var input = BuildInput(
            technicalComplexity: TechnicalComplexityLevel.Moderate,
            teamKnowledge:       TeamKnowledgeLevel.Intermediate);

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.ConfidenceRange.Low.Should().Be(6.3);
        result.ConfidenceRange.High.Should().Be(11.7);
        result.ConfidenceRange.Low.Should().BeLessThan(result.PertHours);
        result.ConfidenceRange.High.Should().BeGreaterThan(result.PertHours);
    }

    public static IEnumerable<object[]> AllComplexityLevelInputs =>
    [
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Trivial,     teamKnowledge: TeamKnowledgeLevel.Expert)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Simple,      teamKnowledge: TeamKnowledgeLevel.Intermediate)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Moderate,    teamKnowledge: TeamKnowledgeLevel.Beginner)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Complex,     teamKnowledge: TeamKnowledgeLevel.Unknown)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.VeryComplex, teamKnowledge: TeamKnowledgeLevel.Beginner)],
    ];

    [Theory, MemberData(nameof(AllComplexityLevelInputs))]
    public void Estimate_AnyValidInput_PertHoursIsAlwaysPositive(EstimationInput input)
    {
        // Arrange done via MemberData

        // Act
        var result = _sut.Estimate(input);

        // Assert
        result.PertHours.Should().BePositive();
    }

    public static IEnumerable<object[]> VariedInputsForInvariants =>
    [
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Trivial,     teamKnowledge: TeamKnowledgeLevel.Expert)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Moderate,    teamKnowledge: TeamKnowledgeLevel.Intermediate, integrationCount: 2, integrationComplexity: IntegrationComplexityLevel.High)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.Complex,     teamKnowledge: TeamKnowledgeLevel.Beginner,     dependencyCount:   3, dependencyReliability: ReliabilityLevel.Low)],
        [BuildInput(technicalComplexity: TechnicalComplexityLevel.VeryComplex, teamKnowledge: TeamKnowledgeLevel.Unknown)],
    ];

    [Theory, MemberData(nameof(VariedInputsForInvariants))]
    public void Estimate_AnyValidInput_OptimisticIsAlwaysLessThanPessimistic(EstimationInput input)
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
        var input = BuildInput(
            technicalComplexity:   TechnicalComplexityLevel.Complex,
            teamKnowledge:         TeamKnowledgeLevel.Beginner,
            integrationCount:      3,
            integrationComplexity: IntegrationComplexityLevel.High);

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
