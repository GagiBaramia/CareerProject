using CareerProject.RecommendationService.Services;
using Xunit;

namespace CareerProject.RecommendationService.Tests;

public class HybridMatchingCalculatorTests
{
    [Fact]
    public void CalculateSkillOverlap_NoRequiredSkills_ReturnsFullMatch()
    {
        var personSkills = new HashSet<Guid> { Guid.NewGuid() };
        var requiredSkills = new HashSet<Guid>();

        var overlap = HybridMatchingCalculator.CalculateSkillOverlap(personSkills, requiredSkills);

        Assert.Equal(1.0, overlap);
    }

    [Fact]
    public void CalculateSkillOverlap_AllRequiredSkillsMatched_ReturnsFullMatch()
    {
        var skillA = Guid.NewGuid();
        var skillB = Guid.NewGuid();
        var personSkills = new HashSet<Guid> { skillA, skillB, Guid.NewGuid() };
        var requiredSkills = new HashSet<Guid> { skillA, skillB };

        var overlap = HybridMatchingCalculator.CalculateSkillOverlap(personSkills, requiredSkills);

        Assert.Equal(1.0, overlap);
    }

    [Fact]
    public void CalculateSkillOverlap_PartialMatch_ReturnsFraction()
    {
        var matched = Guid.NewGuid();
        var missing = Guid.NewGuid();
        var personSkills = new HashSet<Guid> { matched };
        var requiredSkills = new HashSet<Guid> { matched, missing };

        var overlap = HybridMatchingCalculator.CalculateSkillOverlap(personSkills, requiredSkills);

        Assert.Equal(0.5, overlap);
    }

    [Fact]
    public void CalculateSkillOverlap_NoMatches_ReturnsZero()
    {
        var personSkills = new HashSet<Guid> { Guid.NewGuid() };
        var requiredSkills = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var overlap = HybridMatchingCalculator.CalculateSkillOverlap(personSkills, requiredSkills);

        Assert.Equal(0.0, overlap);
    }

    [Fact]
    public void CalculateSkillOverlap_EmptyPersonSkills_ReturnsZero()
    {
        var personSkills = new HashSet<Guid>();
        var requiredSkills = new HashSet<Guid> { Guid.NewGuid() };

        var overlap = HybridMatchingCalculator.CalculateSkillOverlap(personSkills, requiredSkills);

        Assert.Equal(0.0, overlap);
    }

    [Theory]
    [InlineData(1.0, 1.0, 0.6, 0.4, 1.0)]
    [InlineData(0.0, 0.0, 0.6, 0.4, 0.0)]
    [InlineData(1.0, 0.0, 0.6, 0.4, 0.6)]
    [InlineData(0.0, 1.0, 0.6, 0.4, 0.4)]
    [InlineData(0.5, 0.9128, 0.6, 0.4, 0.66512)]
    public void CalculateScore_CombinesWeightedComponents(
        double skillOverlap, double semanticSimilarity, double structuredWeight, double semanticWeight, double expected)
    {
        var score = HybridMatchingCalculator.CalculateScore(skillOverlap, semanticSimilarity, structuredWeight, semanticWeight);

        Assert.Equal(expected, score, precision: 5);
    }

    [Fact]
    public void CalculateScore_WeightsAreConfigurable_NotHardcoded()
    {
        // Same inputs, different weights (e.g. from appsettings.json) must change the result -
        // proves the weights aren't baked into the formula.
        var scoreWithDefaultWeights = HybridMatchingCalculator.CalculateScore(1.0, 0.0, 0.6, 0.4);
        var scoreWithEqualWeights = HybridMatchingCalculator.CalculateScore(1.0, 0.0, 0.5, 0.5);

        Assert.NotEqual(scoreWithDefaultWeights, scoreWithEqualWeights);
        Assert.Equal(0.6, scoreWithDefaultWeights, precision: 5);
        Assert.Equal(0.5, scoreWithEqualWeights, precision: 5);
    }
}
