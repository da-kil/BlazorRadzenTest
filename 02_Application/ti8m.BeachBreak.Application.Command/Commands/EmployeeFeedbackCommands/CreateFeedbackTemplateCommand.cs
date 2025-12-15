using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to create a new custom feedback template.
/// Allows HR users to create reusable feedback templates with specific criteria and text sections.
/// </summary>
public class CreateFeedbackTemplateCommand : ICommand<Result<Guid>>
{
    /// <summary>
    /// Name of the template for identification.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the template's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Source type this template is designed for (0=Customer, 1=Peer, 2=ProjectColleague).
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// Evaluation criteria included in this template.
    /// </summary>
    public List<EvaluationItem> EvaluationCriteria { get; set; } = new();

    /// <summary>
    /// Text sections for comments included in this template.
    /// </summary>
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    /// <summary>
    /// Rating scale for this template (typically 1-10).
    /// </summary>
    public int RatingScale { get; set; } = 10;

    /// <summary>
    /// Label for low end of rating scale.
    /// </summary>
    public string ScaleLowLabel { get; set; } = "Poor";

    /// <summary>
    /// Label for high end of rating scale.
    /// </summary>
    public string ScaleHighLabel { get; set; } = "Excellent";

    /// <summary>
    /// Whether this template is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this template is the default for its source type.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// User ID of the template creator (populated by command handler).
    /// </summary>
    public Guid CreatedBy { get; set; }
}