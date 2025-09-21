using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionType
{
    SelfAssessment,      // 1-4 scale with comments like SelfAssessmentStep.razor
    GoalAchievement,     // Goal achievement evaluation like GoalReviewStep.razor
    TextQuestion,        // Text area questions like CareerPlanningStep.razor

    // Additional fallback values that might exist in database
    Rating,              // Alternative name for SelfAssessment
    Text,                // Alternative name for TextQuestion
    SingleChoice,        // Single choice questions
    MultipleChoice,      // Multiple choice questions
    Scale,               // Scale-based questions
    YesNo,               // Yes/No questions
    Date,                // Date input questions
    Number,              // Numeric input questions
    Unknown              // Fallback for unknown types
}