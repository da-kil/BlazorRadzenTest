namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Defines the types of questions that can be included in a questionnaire.
/// Values are explicit to prevent serialization bugs across CQRS layers.
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Assessment question with evaluation items and rating scale.
    /// Users rate multiple evaluation criteria on a scale (e.g., 1-4).
    /// </summary>
    Assessment = 0,

    /// <summary>
    /// Text question with one or more text input sections.
    /// Users provide free-form text responses.
    /// </summary>
    TextQuestion = 1,

    /// <summary>
    /// Goal question for defining and tracking objectives.
    /// Goals are added dynamically during questionnaire workflow.
    /// </summary>
    Goal = 2,

    /// <summary>
    /// Employee feedback from external sources (Customer, Peer, Project Colleague).
    /// Supports configurable evaluation criteria and multiple feedback source types.
    /// </summary>
    EmployeeFeedback = 3,

    /// <summary>
    /// Multiple choice question with configurable options and min/max selection constraints.
    /// Users select between MinSelections and MaxSelections from a list of choices.
    /// </summary>
    MultipleChoice = 4,

    /// <summary>
    /// Binary question with exactly two options (Yes/No or custom labels), rendered as radio buttons.
    /// At most one option can be selected.
    /// </summary>
    Binary = 5
}
