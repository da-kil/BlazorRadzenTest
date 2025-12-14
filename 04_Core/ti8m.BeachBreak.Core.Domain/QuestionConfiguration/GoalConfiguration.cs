namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for goal questions.
/// Goals are added dynamically during questionnaire workflow (not template-based),
/// but this configuration controls whether the goal section is visible to users.
/// </summary>
public sealed class GoalConfiguration : IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type.
    /// </summary>
    public QuestionType QuestionType => QuestionType.Goal;

    /// <summary>
    /// Indicates whether the goal section should be displayed when filling out the questionnaire.
    /// Allows template creators to hide goal sections for specific questionnaires.
    /// </summary>
    public bool ShowGoalSection { get; set; } = true;

    /// <summary>
    /// Validates that the configuration is valid.
    /// Goal configuration is always valid (only controls visibility).
    /// </summary>
    public bool IsValid()
    {
        return true;
    }
}
