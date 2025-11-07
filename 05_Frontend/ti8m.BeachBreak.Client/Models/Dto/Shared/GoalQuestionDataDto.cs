using ti8m.BeachBreak.Client.Models.Dto.Queries;

namespace ti8m.BeachBreak.Client.Models.Dto.Shared;

public class GoalQuestionDataDto
{
    public Guid QuestionId { get; set; }
    public Guid? PredecessorAssignmentId { get; set; }
    public List<GoalDto> Goals { get; set; } = new();
    public List<PredecessorRatingDto> PredecessorGoalRatings { get; set; } = new();
    public WorkflowState WorkflowState { get; set; }
}
