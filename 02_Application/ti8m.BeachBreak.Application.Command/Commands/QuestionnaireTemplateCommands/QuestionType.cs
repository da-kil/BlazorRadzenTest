namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public enum QuestionType
{
    SelfAssessment,      // 1-4 scale with comments like SelfAssessmentStep.razor
    GoalAchievement,     // Goal achievement evaluation like GoalReviewStep.razor  
    TextQuestion         // Text area questions like CareerPlanningStep.razor
}