using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// DTO for feedback template query responses.
/// Maps from FeedbackTemplateReadModel.
/// </summary>
[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class FeedbackTemplateDto
{
    public Guid Id { get; set; }
    public string NameGerman { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;

    public List<EvaluationItem> Criteria { get; set; } = new();
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    public int RatingScale { get; set; }
    public string ScaleLowLabel { get; set; } = string.Empty;
    public string ScaleHighLabel { get; set; } = string.Empty;

    public List<int> AllowedSourceTypes { get; set; } = new();

    public Guid CreatedByEmployeeId { get; set; }
    public ApplicationRole CreatedByRole { get; set; }
    public string CreatedByEmployeeName { get; set; } = string.Empty;

    public TemplateStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public Guid? PublishedByEmployeeId { get; set; }
    public bool IsDeleted { get; set; }

    public bool CanBeUsedForFeedback { get; set; }
    public bool CanBeEdited { get; set; }
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Creates DTO from read model.
    /// </summary>
    public static FeedbackTemplateDto FromReadModel(FeedbackTemplateReadModel readModel)
    {
        return new FeedbackTemplateDto
        {
            Id = readModel.Id,
            NameGerman = readModel.NameGerman,
            NameEnglish = readModel.NameEnglish,
            DescriptionGerman = readModel.DescriptionGerman,
            DescriptionEnglish = readModel.DescriptionEnglish,
            Criteria = readModel.Criteria,
            TextSections = readModel.TextSections,
            RatingScale = readModel.RatingScale,
            ScaleLowLabel = readModel.ScaleLowLabel,
            ScaleHighLabel = readModel.ScaleHighLabel,
            AllowedSourceTypes = readModel.AllowedSourceTypes,
            CreatedByEmployeeId = readModel.CreatedByEmployeeId,
            CreatedByRole = readModel.CreatedByRole,
            CreatedByEmployeeName = readModel.CreatedByEmployeeName,
            Status = ConvertToDtoStatus(readModel.Status),
            CreatedDate = readModel.CreatedDate,
            PublishedDate = readModel.PublishedDate,
            PublishedByEmployeeId = readModel.PublishedByEmployeeId,
            IsDeleted = readModel.IsDeleted,
            CanBeUsedForFeedback = readModel.CanBeUsedForFeedback,
            CanBeEdited = readModel.CanBeEdited,
            IsAvailable = readModel.IsAvailable
        };
    }

    /// <summary>
    /// Converts domain TemplateStatus to DTO TemplateStatus.
    /// Both enums have identical values (Draft=0, Published=1, Archived=2).
    /// </summary>
    private static TemplateStatus ConvertToDtoStatus(Domain.QuestionnaireTemplateAggregate.TemplateStatus domainStatus)
    {
        return domainStatus switch
        {
            Domain.QuestionnaireTemplateAggregate.TemplateStatus.Draft => TemplateStatus.Draft,
            Domain.QuestionnaireTemplateAggregate.TemplateStatus.Published => TemplateStatus.Published,
            Domain.QuestionnaireTemplateAggregate.TemplateStatus.Archived => TemplateStatus.Archived,
            _ => throw new ArgumentOutOfRangeException(nameof(domainStatus), domainStatus, "Unknown template status")
        };
    }
}
