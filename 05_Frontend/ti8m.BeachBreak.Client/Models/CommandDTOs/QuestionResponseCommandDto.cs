namespace ti8m.BeachBreak.Client.Models.CommandDTOs;

/// <summary>
/// Command DTO for individual question responses with type safety.
/// Only one response type will be populated based on QuestionType.
/// </summary>
public class QuestionResponseCommandDto
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }

    // Only one of these will be populated
    public TextResponseCommandDto? TextResponse { get; set; }
    public AssessmentResponseCommandDto? AssessmentResponse { get; set; }
    public GoalResponseCommandDto? GoalResponse { get; set; }
}