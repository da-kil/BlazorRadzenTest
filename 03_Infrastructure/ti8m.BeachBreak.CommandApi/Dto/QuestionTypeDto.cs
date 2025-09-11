namespace ti8m.BeachBreak.CommandApi.Dto;

public enum QuestionTypeDto
{
    SelfAssessment,      // 1-4 scale with comments like SelfAssessmentStep.razor
    GoalAchievement,     // Goal achievement evaluation like GoalReviewStep.razor  
    TextQuestion         // Text area questions like CareerPlanningStep.razor
}