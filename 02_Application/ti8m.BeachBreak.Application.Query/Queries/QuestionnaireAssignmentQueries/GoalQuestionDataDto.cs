namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GoalQuestionDataDto
{
    public Guid QuestionId { get; set; }
    public Guid? PredecessorAssignmentId { get; set; }
    public List<GoalDto> Goals { get; set; } = new();
    public List<GoalRatingDto> PredecessorGoalRatings { get; set; } = new();
}
