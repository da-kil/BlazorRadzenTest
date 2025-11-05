using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// Command DTO for individual question responses with type safety.
/// Only one response type will be populated based on QuestionType.
/// </summary>
public class QuestionResponseCommandDto
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }

    // Only one of these will be populated
    public TextResponseDto? TextResponse { get; set; }
    public AssessmentResponseDto? AssessmentResponse { get; set; }
    public GoalResponseDto? GoalResponse { get; set; }
}