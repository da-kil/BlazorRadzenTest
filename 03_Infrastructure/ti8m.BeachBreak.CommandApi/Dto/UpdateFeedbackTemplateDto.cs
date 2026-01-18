using ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for updating an existing feedback template.
/// Only draft templates can be updated.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class UpdateFeedbackTemplateDto
{
    /// <summary>
    /// Template name in German.
    /// </summary>
    public string NameGerman { get; set; } = string.Empty;

    /// <summary>
    /// Template name in English.
    /// </summary>
    public string NameEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Template description in German (optional).
    /// </summary>
    public string DescriptionGerman { get; set; } = string.Empty;

    /// <summary>
    /// Template description in English (optional).
    /// </summary>
    public string DescriptionEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Evaluation criteria (competencies/skills to rate).
    /// </summary>
    public List<EvaluationItem> Criteria { get; set; } = new();

    /// <summary>
    /// Text sections for freeform comments.
    /// </summary>
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    /// <summary>
    /// Rating scale (2-10, default 10).
    /// </summary>
    public int RatingScale { get; set; } = 10;

    /// <summary>
    /// Label for low end of scale (e.g., "Poor").
    /// </summary>
    public string ScaleLowLabel { get; set; } = "Poor";

    /// <summary>
    /// Label for high end of scale (e.g., "Excellent").
    /// </summary>
    public string ScaleHighLabel { get; set; } = "Excellent";

    /// <summary>
    /// Feedback source types this template applies to (0=Customer, 1=Peer, 2=ProjectColleague).
    /// </summary>
    public List<int> AllowedSourceTypes { get; set; } = new();

    /// <summary>
    /// Converts DTO to domain command.
    /// </summary>
    public UpdateFeedbackTemplateCommand ToCommand(Guid templateId)
    {
        return new UpdateFeedbackTemplateCommand(
            templateId,
            new Translation(NameGerman, NameEnglish),
            new Translation(DescriptionGerman, DescriptionEnglish),
            Criteria,
            TextSections,
            RatingScale,
            ScaleLowLabel,
            ScaleHighLabel,
            AllowedSourceTypes.Select(st => (FeedbackSourceType)st).ToList());
    }
}
