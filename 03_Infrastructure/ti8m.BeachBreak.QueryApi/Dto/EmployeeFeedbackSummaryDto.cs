using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// DTO for employee feedback summary display.
/// Contains key information for listing and review purposes.
/// </summary>
public class EmployeeFeedbackSummaryDto
{
    /// <summary>
    /// Unique identifier for the feedback record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the employee the feedback is about.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Employee name for display.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Source type as integer (0=Customer, 1=Peer, 2=ProjectColleague).
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// Display name for the source type.
    /// </summary>
    public string SourceTypeDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the feedback provider.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Role of the feedback provider.
    /// </summary>
    public string ProviderRole { get; set; } = string.Empty;

    /// <summary>
    /// Project name (for project colleague feedback).
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Date when the feedback was provided.
    /// </summary>
    public DateTime FeedbackDate { get; set; }

    /// <summary>
    /// Date when the feedback was recorded in the system.
    /// </summary>
    public DateTime RecordedDate { get; set; }

    /// <summary>
    /// Name of the person who recorded the feedback.
    /// </summary>
    public string RecordedByEmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Average rating across all criteria (if any ratings provided).
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of criteria that were rated.
    /// </summary>
    public int RatedCriteriaCount { get; set; }

    /// <summary>
    /// Whether the feedback contains unstructured comments.
    /// </summary>
    public bool HasUnstructuredFeedback { get; set; }

    /// <summary>
    /// Whether this is project-related feedback.
    /// </summary>
    public bool IsProjectFeedback { get; set; }

    /// <summary>
    /// Completion percentage of rated criteria.
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Creates DTO from read model.
    /// </summary>
    public static EmployeeFeedbackSummaryDto FromReadModel(EmployeeFeedbackReadModel readModel)
    {
        return new EmployeeFeedbackSummaryDto
        {
            Id = readModel.Id,
            EmployeeId = readModel.EmployeeId,
            EmployeeName = readModel.EmployeeName,
            SourceType = (int)readModel.SourceType,
            SourceTypeDisplayName = readModel.SourceTypeDisplayName,
            ProviderName = readModel.ProviderName,
            ProviderRole = readModel.ProviderRole,
            ProjectName = readModel.ProjectName,
            FeedbackDate = readModel.FeedbackDate,
            RecordedDate = readModel.RecordedDate,
            RecordedByEmployeeName = readModel.RecordedByEmployeeName,
            AverageRating = readModel.AverageRating,
            RatedCriteriaCount = readModel.RatedCriteriaCount,
            HasUnstructuredFeedback = readModel.HasUnstructuredFeedback,
            IsProjectFeedback = readModel.IsProjectFeedback,
            CompletionPercentage = readModel.CompletionPercentage
        };
    }
}