namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model projection for tracking changes made during the review meeting.
/// Captures what was changed, who changed it, and when.
/// </summary>
public class ReviewChangeLogReadModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssignmentId { get; set; }
    public Guid SectionId { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Who was originally supposed to complete this question (Employee, Manager, or Both)
    /// </summary>
    public string OriginalCompletionRole { get; set; } = string.Empty;

    /// <summary>
    /// The value before the manager's edit during review
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// The new value after the manager's edit
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }
    public Guid ChangedByEmployeeId { get; set; }
}
