using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using Xunit;

namespace ti8m.BeachBreak.Core.Domain.Tests;

/// <summary>
/// Unit tests for QuestionnaireProcessTypeExtensions.
/// Tests business logic rules for different process types.
/// </summary>
public class QuestionnaireProcessTypeExtensionsTests
{
    #region RequiresManagerReview Tests

    [Theory]
    [InlineData(QuestionnaireProcessType.PerformanceReview, true)]
    [InlineData(QuestionnaireProcessType.Survey, false)]
    public void RequiresManagerReview_ReturnsCorrectValue(
        QuestionnaireProcessType processType,
        bool expectedRequiresReview)
    {
        // Act
        var result = processType.RequiresManagerReview();

        // Assert
        Assert.Equal(expectedRequiresReview, result);
    }

    [Fact]
    public void RequiresManagerReview_InvalidProcessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidProcessType = (QuestionnaireProcessType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => invalidProcessType.RequiresManagerReview());

        Assert.Contains("processType", exception.ParamName);
    }

    #endregion

    #region IsQuestionTypeAllowed Tests

    [Theory]
    // PerformanceReview allows all question types
    [InlineData(QuestionnaireProcessType.PerformanceReview, QuestionType.Assessment, true)]
    [InlineData(QuestionnaireProcessType.PerformanceReview, QuestionType.TextQuestion, true)]
    [InlineData(QuestionnaireProcessType.PerformanceReview, QuestionType.Goal, true)]
    // Survey allows Assessment and TextQuestion but NOT Goal
    [InlineData(QuestionnaireProcessType.Survey, QuestionType.Assessment, true)]
    [InlineData(QuestionnaireProcessType.Survey, QuestionType.TextQuestion, true)]
    [InlineData(QuestionnaireProcessType.Survey, QuestionType.Goal, false)]
    public void IsQuestionTypeAllowed_ReturnsCorrectValue(
        QuestionnaireProcessType processType,
        QuestionType questionType,
        bool expectedAllowed)
    {
        // Act
        var result = processType.IsQuestionTypeAllowed(questionType);

        // Assert
        Assert.Equal(expectedAllowed, result);
    }

    [Fact]
    public void IsQuestionTypeAllowed_InvalidProcessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidProcessType = (QuestionnaireProcessType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => invalidProcessType.IsQuestionTypeAllowed(QuestionType.Assessment));

        Assert.Contains("processType", exception.ParamName);
    }

    [Fact]
    public void IsQuestionTypeAllowed_AllValidQuestionTypes_PerformanceReview_ReturnsTrue()
    {
        // Arrange
        var allQuestionTypes = Enum.GetValues<QuestionType>();

        // Act & Assert
        foreach (var questionType in allQuestionTypes)
        {
            var result = QuestionnaireProcessType.PerformanceReview.IsQuestionTypeAllowed(questionType);
            Assert.True(result, $"PerformanceReview should allow {questionType}");
        }
    }

    [Fact]
    public void IsQuestionTypeAllowed_Survey_OnlyAllowsAssessmentAndTextQuestion()
    {
        // Arrange
        var allQuestionTypes = Enum.GetValues<QuestionType>();

        // Act & Assert
        foreach (var questionType in allQuestionTypes)
        {
            var result = QuestionnaireProcessType.Survey.IsQuestionTypeAllowed(questionType);

            if (questionType == QuestionType.Assessment || questionType == QuestionType.TextQuestion)
            {
                Assert.True(result, $"Survey should allow {questionType}");
            }
            else
            {
                Assert.False(result, $"Survey should NOT allow {questionType}");
            }
        }
    }

    #endregion

    #region IsCompletionRoleAllowed Tests

    [Theory]
    // PerformanceReview allows all completion roles
    [InlineData(QuestionnaireProcessType.PerformanceReview, CompletionRole.Employee, true)]
    [InlineData(QuestionnaireProcessType.PerformanceReview, CompletionRole.Manager, true)]
    [InlineData(QuestionnaireProcessType.PerformanceReview, CompletionRole.Both, true)]
    // Survey allows ONLY Employee role
    [InlineData(QuestionnaireProcessType.Survey, CompletionRole.Employee, true)]
    [InlineData(QuestionnaireProcessType.Survey, CompletionRole.Manager, false)]
    [InlineData(QuestionnaireProcessType.Survey, CompletionRole.Both, false)]
    public void IsCompletionRoleAllowed_ReturnsCorrectValue(
        QuestionnaireProcessType processType,
        CompletionRole completionRole,
        bool expectedAllowed)
    {
        // Act
        var result = processType.IsCompletionRoleAllowed(completionRole);

        // Assert
        Assert.Equal(expectedAllowed, result);
    }

    [Fact]
    public void IsCompletionRoleAllowed_InvalidProcessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidProcessType = (QuestionnaireProcessType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => invalidProcessType.IsCompletionRoleAllowed(CompletionRole.Employee));

        Assert.Contains("processType", exception.ParamName);
    }

    [Fact]
    public void IsCompletionRoleAllowed_AllValidRoles_PerformanceReview_ReturnsTrue()
    {
        // Arrange
        var allCompletionRoles = Enum.GetValues<CompletionRole>();

        // Act & Assert
        foreach (var role in allCompletionRoles)
        {
            var result = QuestionnaireProcessType.PerformanceReview.IsCompletionRoleAllowed(role);
            Assert.True(result, $"PerformanceReview should allow {role}");
        }
    }

    [Fact]
    public void IsCompletionRoleAllowed_Survey_AllowsOnlyEmployeeRole()
    {
        // Arrange
        var allCompletionRoles = Enum.GetValues<CompletionRole>();

        // Act & Assert
        foreach (var role in allCompletionRoles)
        {
            var result = QuestionnaireProcessType.Survey.IsCompletionRoleAllowed(role);

            if (role == CompletionRole.Employee)
            {
                Assert.True(result, "Survey should allow Employee role");
            }
            else
            {
                Assert.False(result, $"Survey should NOT allow {role} role");
            }
        }
    }

    #endregion

    #region Business Logic Integration Tests

    [Fact]
    public void ProcessTypeRules_Survey_ConsistentBehavior()
    {
        // Arrange
        var survey = QuestionnaireProcessType.Survey;

        // Act & Assert - Survey should have consistent restrictions
        Assert.False(survey.RequiresManagerReview(), "Survey should not require manager review");
        Assert.False(survey.IsQuestionTypeAllowed(QuestionType.Goal), "Survey should not allow Goal questions");
        Assert.False(survey.IsCompletionRoleAllowed(CompletionRole.Manager), "Survey should not allow Manager role");
        Assert.False(survey.IsCompletionRoleAllowed(CompletionRole.Both), "Survey should not allow Both role");
        Assert.True(survey.IsCompletionRoleAllowed(CompletionRole.Employee), "Survey should allow Employee role");
    }

    [Fact]
    public void ProcessTypeRules_PerformanceReview_AllowsAllOptions()
    {
        // Arrange
        var performanceReview = QuestionnaireProcessType.PerformanceReview;

        // Act & Assert - PerformanceReview should be unrestricted
        Assert.True(performanceReview.RequiresManagerReview(), "PerformanceReview should require manager review");

        // All question types allowed
        foreach (var questionType in Enum.GetValues<QuestionType>())
        {
            Assert.True(performanceReview.IsQuestionTypeAllowed(questionType),
                $"PerformanceReview should allow {questionType}");
        }

        // All completion roles allowed
        foreach (var role in Enum.GetValues<CompletionRole>())
        {
            Assert.True(performanceReview.IsCompletionRoleAllowed(role),
                $"PerformanceReview should allow {role}");
        }
    }

    [Theory]
    [InlineData(QuestionnaireProcessType.Survey, QuestionType.Goal, CompletionRole.Manager)]
    [InlineData(QuestionnaireProcessType.Survey, QuestionType.Goal, CompletionRole.Both)]
    public void ProcessTypeRules_Survey_MultipleViolations(
        QuestionnaireProcessType processType,
        QuestionType disallowedQuestionType,
        CompletionRole disallowedRole)
    {
        // Act & Assert - Verify multiple restrictions are enforced
        Assert.False(processType.IsQuestionTypeAllowed(disallowedQuestionType),
            $"Survey should not allow {disallowedQuestionType}");
        Assert.False(processType.IsCompletionRoleAllowed(disallowedRole),
            $"Survey should not allow {disallowedRole}");
    }

    #endregion
}
