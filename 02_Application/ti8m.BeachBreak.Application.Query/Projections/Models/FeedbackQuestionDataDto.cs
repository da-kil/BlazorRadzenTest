using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections.Models;

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
