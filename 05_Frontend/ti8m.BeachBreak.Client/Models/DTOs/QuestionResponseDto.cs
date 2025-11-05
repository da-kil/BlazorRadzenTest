namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Strongly-typed DTO for individual question responses.
/// Eliminates Dictionary<string, object> with compile-time type safety.
/// </summary>
public class QuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public CompletionRole CompletedBy { get; set; }
    public DateTime LastModified { get; set; }

    // Only one of these will be populated based on QuestionType
    public TextResponseDto? TextResponse { get; set; }
    public AssessmentResponseDto? AssessmentResponse { get; set; }
    public GoalResponseDto? GoalResponse { get; set; }
}