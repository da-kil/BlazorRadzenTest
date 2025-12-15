using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for creating a new feedback template.
/// Contains all the configuration needed to create a reusable feedback template.
/// </summary>
public class CreateFeedbackTemplateDto
{
    /// <summary>
    /// Name of the template for identification.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the template's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Source type this template is designed for (0=Customer, 1=Peer, 2=ProjectColleague).
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// Evaluation criteria included in this template.
    /// </summary>
    public List<EvaluationItem>? EvaluationCriteria { get; set; }

    /// <summary>
    /// Text sections for comments included in this template.
    /// </summary>
    public List<TextSectionDefinition>? TextSections { get; set; }

    /// <summary>
    /// Rating scale for this template (typically 1-10).
    /// </summary>
    public int RatingScale { get; set; } = 10;

    /// <summary>
    /// Label for low end of rating scale.
    /// </summary>
    public string? ScaleLowLabel { get; set; }

    /// <summary>
    /// Label for high end of rating scale.
    /// </summary>
    public string? ScaleHighLabel { get; set; }

    /// <summary>
    /// Whether this template is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this template is the default for its source type.
    /// </summary>
    public bool IsDefault { get; set; } = false;
}