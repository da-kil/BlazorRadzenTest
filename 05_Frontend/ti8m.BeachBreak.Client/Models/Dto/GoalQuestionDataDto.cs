namespace ti8m.BeachBreak.Client.Models.Dto;

public class GoalQuestionDataDto
{
    public Guid QuestionId { get; set; }
    public Guid? PredecessorAssignmentId { get; set; }
    public List<GoalDto> Goals { get; set; } = new();
    public List<PredecessorRatingDto> PredecessorGoalRatings { get; set; } = new();
}
