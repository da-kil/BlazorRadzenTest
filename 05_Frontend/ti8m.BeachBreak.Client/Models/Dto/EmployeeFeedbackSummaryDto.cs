namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// Client-side DTO for employee feedback summary display.
/// Matches the QueryApi DTO for type-safe communication.
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
    /// Detailed feedback data including ratings and comments.
    /// Available when retrieving individual feedback records.
    /// </summary>
    public ConfigurableFeedbackDataDto? FeedbackData { get; set; }

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

    // UI helper properties

    /// <summary>
    /// Gets the source type badge CSS class for styling.
    /// </summary>
    public string SourceTypeBadgeClass => SourceType switch
    {
        0 => "badge-customer", // Customer - blue
        1 => "badge-peer", // Peer - green
        2 => "badge-project", // Project Colleague - orange
        _ => "badge-unknown"
    };

    /// <summary>
    /// Gets a formatted rating display (e.g., "4.2/10" or "Not rated").
    /// </summary>
    public string RatingDisplay => AverageRating.HasValue
        ? $"{AverageRating.Value:F1}/10"
        : "Not rated";

    /// <summary>
    /// Gets a short feedback summary for list display.
    /// </summary>
    public string FeedbackSummary
    {
        get
        {
            var parts = new List<string>();

            if (RatedCriteriaCount > 0)
            {
                parts.Add($"{RatedCriteriaCount} ratings");
            }

            if (HasUnstructuredFeedback)
            {
                parts.Add("comments");
            }

            return parts.Any() ? string.Join(", ", parts) : "No content";
        }
    }

    /// <summary>
    /// Gets the project display text (project name if available, otherwise "N/A").
    /// </summary>
    public string ProjectDisplay => !string.IsNullOrWhiteSpace(ProjectName) ? ProjectName : "N/A";

    /// <summary>
    /// Whether this feedback is recent (within last 30 days).
    /// </summary>
    public bool IsRecent => FeedbackDate > DateTime.UtcNow.AddDays(-30);
}