namespace ti8m.BeachBreak.Client.Models;

public enum QuestionType
{
    Assessment,          // 1-4 scale with comments - can be used by employee or manager
    GoalAchievement,     // Goal achievement evaluation like GoalReviewStep.razor
    TextQuestion         // Text area questions like CareerPlanningStep.razor
}