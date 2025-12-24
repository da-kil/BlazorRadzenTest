using System.Text.Json;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model for employee feedback with denormalized data for efficient querying.
/// Supports filtering by source type, employee, date ranges, and user access rights.
/// </summary>
public class EmployeeFeedbackReadModel
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
    /// Denormalized employee name for performance.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's first name for filtering and display.
    /// </summary>
    public string EmployeeFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's last name for filtering and display.
    /// </summary>
    public string EmployeeLastName { get; set; } = string.Empty;

    /// <summary>
    /// Type of feedback source.
    /// </summary>
    public FeedbackSourceType SourceType { get; set; }

    /// <summary>
    /// Display name for the source type.
    /// </summary>
    public string SourceTypeDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person providing the feedback.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Role of the person providing the feedback.
    /// </summary>
    public string ProviderRole { get; set; } = string.Empty;

    /// <summary>
    /// Project name (for Project Colleague feedback).
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Project context information.
    /// </summary>
    public string? ProjectContext { get; set; }

    /// <summary>
    /// Date when the feedback was originally provided.
    /// </summary>
    public DateTime FeedbackDate { get; set; }

    /// <summary>
    /// Date when the feedback was recorded in the system.
    /// </summary>
    public DateTime RecordedDate { get; set; }

    /// <summary>
    /// ID of the employee who recorded this feedback.
    /// </summary>
    public Guid RecordedByEmployeeId { get; set; }

    /// <summary>
    /// Denormalized name of the person who recorded the feedback.
    /// </summary>
    public string RecordedByEmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Date when the feedback was last modified (if any).
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// ID of the employee who last modified the feedback.
    /// </summary>
    public Guid? LastModifiedByEmployeeId { get; set; }

    /// <summary>
    /// Name of the person who last modified the feedback.
    /// </summary>
    public string? LastModifiedByEmployeeName { get; set; }

    /// <summary>
    /// Whether the feedback has been soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Date when the feedback was deleted.
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// Reason for deletion (if provided).
    /// </summary>
    public string? DeleteReason { get; set; }

    // Aggregated metrics for performance

    /// <summary>
    /// Average rating across all rated criteria.
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of criteria that were rated (not null).
    /// </summary>
    public int RatedCriteriaCount { get; set; }

    /// <summary>
    /// Total number of criteria in the feedback.
    /// </summary>
    public int TotalCriteriaCount { get; set; }

    /// <summary>
    /// Whether the feedback contains any unstructured comments.
    /// </summary>
    public bool HasUnstructuredFeedback { get; set; }

    /// <summary>
    /// Number of characters in all comments combined.
    /// </summary>
    public int CommentCharacterCount { get; set; }

    /// <summary>
    /// JSON serialized feedback data for detailed views.
    /// Stored as JSON for flexibility while maintaining queryable fields above.
    /// </summary>
    public string FeedbackDataJson { get; set; } = string.Empty;

    // Helper properties for business logic

    /// <summary>
    /// Whether this feedback is for a project-based context.
    /// </summary>
    public bool IsProjectFeedback => SourceType == FeedbackSourceType.ProjectColleague;

    /// <summary>
    /// Whether the feedback has any ratings.
    /// </summary>
    public bool HasRatings => RatedCriteriaCount > 0;

    /// <summary>
    /// Percentage of criteria that were rated.
    /// </summary>
    public decimal CompletionPercentage => TotalCriteriaCount > 0
        ? (decimal)RatedCriteriaCount / TotalCriteriaCount * 100
        : 0;

    /// <summary>
    /// Gets the fiscal year of the feedback date.
    /// Useful for filtering current year feedback during questionnaire reviews.
    /// </summary>
    public int FiscalYear => FeedbackDate.Month >= 4 ? FeedbackDate.Year : FeedbackDate.Year - 1;

    /// <summary>
    /// Whether this feedback is from the current fiscal year.
    /// </summary>
    public bool IsCurrentFiscalYear
    {
        get
        {
            var now = DateTime.UtcNow;
            var currentFiscalYear = now.Month >= 4 ? now.Year : now.Year - 1;
            return FiscalYear == currentFiscalYear;
        }
    }

    // Apply methods for Marten event sourcing projection

    /// <summary>
    /// Projects the EmployeeFeedbackRecorded event to create the initial read model.
    /// </summary>
    public void Apply(EmployeeFeedbackRecorded @event)
    {
        Id = @event.FeedbackId;
        EmployeeId = @event.EmployeeId;

        // Note: Employee names need to be populated separately via denormalization
        // The events only contain IDs to keep them pure domain events
        EmployeeName = string.Empty;
        EmployeeFirstName = string.Empty;
        EmployeeLastName = string.Empty;

        SourceType = @event.SourceType;
        SourceTypeDisplayName = GetSourceTypeDisplayName(@event.SourceType);

        // Provider information
        ProviderName = @event.ProviderInfo.ProviderName;
        ProviderRole = @event.ProviderInfo.ProviderRole;
        ProjectName = @event.ProviderInfo.ProjectName;
        ProjectContext = @event.ProviderInfo.ProjectContext;

        FeedbackDate = @event.FeedbackDate;
        RecordedDate = @event.RecordedDate;
        RecordedByEmployeeId = @event.RecordedByEmployeeId;

        // Note: RecordedByEmployeeName needs to be populated separately via denormalization
        RecordedByEmployeeName = string.Empty;

        // Calculate metrics from feedback data
        UpdateMetricsFromFeedbackData(@event.FeedbackData);

        // Serialize feedback data to JSON for storage
        FeedbackDataJson = JsonSerializer.Serialize(@event.FeedbackData);

        IsDeleted = false;
    }

    /// <summary>
    /// Projects the EmployeeFeedbackUpdated event to update the read model.
    /// </summary>
    public void Apply(EmployeeFeedbackUpdated @event)
    {
        // Provider information (may have changed)
        ProviderName = @event.ProviderInfo.ProviderName;
        ProviderRole = @event.ProviderInfo.ProviderRole;
        ProjectName = @event.ProviderInfo.ProjectName;
        ProjectContext = @event.ProviderInfo.ProjectContext;

        FeedbackDate = @event.FeedbackDate;
        LastModifiedDate = @event.UpdatedDate;
        LastModifiedByEmployeeId = @event.UpdatedByEmployeeId;

        // Note: LastModifiedByEmployeeName needs to be populated separately via denormalization
        LastModifiedByEmployeeName = string.Empty;

        // Recalculate metrics from updated feedback data
        UpdateMetricsFromFeedbackData(@event.FeedbackData);

        // Update serialized feedback data
        FeedbackDataJson = JsonSerializer.Serialize(@event.FeedbackData);
    }

    /// <summary>
    /// Projects the EmployeeFeedbackDeleted event to mark the read model as deleted.
    /// </summary>
    public void Apply(EmployeeFeedbackDeleted @event)
    {
        IsDeleted = true;
        DeletedDate = @event.DeletedDate;
        DeleteReason = @event.DeleteReason;
    }

    // Helper methods

    private void UpdateMetricsFromFeedbackData(Domain.EmployeeFeedbackAggregate.ValueObjects.ConfigurableFeedbackData feedbackData)
    {
        // Calculate rating metrics
        RatedCriteriaCount = feedbackData.RatedItemsCount;
        TotalCriteriaCount = feedbackData.Ratings.Count;
        AverageRating = feedbackData.AverageRating;

        // Calculate comment metrics
        HasUnstructuredFeedback = feedbackData.HasAnyComment;

        var totalCommentLength = 0;
        foreach (var rating in feedbackData.Ratings.Values)
        {
            totalCommentLength += rating.Comment?.Length ?? 0;
        }
        foreach (var comment in feedbackData.Comments.Values)
        {
            totalCommentLength += comment?.Length ?? 0;
        }
        CommentCharacterCount = totalCommentLength;
    }

    private static string GetSourceTypeDisplayName(FeedbackSourceType sourceType)
    {
        return sourceType switch
        {
            FeedbackSourceType.Customer => "Customer",
            FeedbackSourceType.Peer => "Peer",
            FeedbackSourceType.ProjectColleague => "Project Colleague",
            _ => sourceType.ToString()
        };
    }
}