using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Represents a note taken during the InReview phase of a questionnaire assignment
/// </summary>
public class InReviewNote : ValueObject
{
    /// <summary>
    /// Unique identifier for the note
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Content of the note (maximum 2000 characters)
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// When the note was created or last modified
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Optional section ID for context (null = general note)
    /// </summary>
    public Guid? SectionId { get; }

    /// <summary>
    /// ID of the employee who authored the note
    /// </summary>
    public Guid AuthorEmployeeId { get; }

    /// <summary>
    /// Creates a new InReview note
    /// </summary>
    public InReviewNote(
        Guid id,
        string content,
        DateTime timestamp,
        Guid? sectionId,
        Guid authorEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content cannot be empty", nameof(content));

        if (content.Length > 2000)
            throw new ArgumentException("Note content cannot exceed 2000 characters", nameof(content));

        Id = id;
        Content = content.Trim();
        Timestamp = timestamp;
        SectionId = sectionId;
        AuthorEmployeeId = authorEmployeeId;
    }


    /// <summary>
    /// Determines equality for value object
    /// </summary>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Content;
        yield return Timestamp;
        yield return SectionId;
        yield return AuthorEmployeeId;
    }
}