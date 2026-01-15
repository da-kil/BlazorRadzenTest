namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for employee feedback questions.
/// Feedback is linked during questionnaire workflow (not template-based),
/// but this configuration controls whether the feedback section is visible to users.
/// Follows the Goals pattern: minimal template configuration, instance-specific data added during initialization.
/// </summary>
public sealed class EmployeeFeedbackConfiguration : IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type.
    /// </summary>
    public QuestionType QuestionType => QuestionType.EmployeeFeedback;

    /// <summary>
    /// Indicates whether the feedback section should be displayed when filling out the questionnaire.
    /// Allows template creators to hide feedback sections for specific questionnaires.
    /// </summary>
    public bool ShowFeedbackSection { get; set; } = true;

    /// <summary>
    /// Validates that the configuration is valid.
    /// Feedback configuration is always valid (only controls visibility).
    /// </summary>
    public bool IsValid()
    {
        return true;
    }
}