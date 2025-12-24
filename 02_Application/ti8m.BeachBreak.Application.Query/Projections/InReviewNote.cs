namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model entity for in-review notes within questionnaire assignments.
/// This is stored in the database as part of the QuestionnaireAssignmentReadModel.
/// NOT a DTO - this is the actual read model structure for persistence.
/// </summary>
public class InReviewNote
{
    /// <summary>
    /// Unique identifier for the note
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Content of the note
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the note was created or last modified
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Optional section ID for context (null = general note)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// ID of the employee who authored the note
    /// </summary>
    public Guid AuthorEmployeeId { get; set; }
}