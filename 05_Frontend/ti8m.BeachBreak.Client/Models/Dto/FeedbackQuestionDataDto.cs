namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for feedback question data including linked feedback records.
/// Used to display feedback sections in questionnaire workflow.
/// </summary>
public record FeedbackQuestionDataDto
{
    public Guid QuestionId { get; init; }
    public WorkflowState WorkflowState { get; init; }
    public List<LinkedEmployeeFeedbackDto> LinkedFeedback { get; init; } = new();
}
