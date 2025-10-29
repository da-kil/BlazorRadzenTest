namespace ti8m.BeachBreak.CommandApi.Dto;

public enum QuestionTypeDto
{
    Assessment = 0,      // 1-4 scale with comments - can be used by employee or manager
    TextQuestion = 1,    // Text area questions like CareerPlanningStep.razor
    Goal = 2             // Goal achievement evaluation like GoalReviewStep.razor
}