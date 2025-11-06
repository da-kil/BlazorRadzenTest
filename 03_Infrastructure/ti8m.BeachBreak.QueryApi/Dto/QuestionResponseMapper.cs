using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// Maps domain QuestionResponseValue objects to Query API DTO types.
/// This maintains architectural boundaries by not exposing domain types to the API layer.
/// </summary>
public static class QuestionResponseMapper
{
    /// <summary>
    /// Maps domain QuestionResponseValue to strongly-typed DTO.
    /// </summary>
    public static QuestionResponseDataDto MapToDto(QuestionResponseValue responseValue)
    {
        return responseValue switch
        {
            QuestionResponseValue.TextResponse textResponse => new TextResponseDataDto
            {
                TextSections = textResponse.TextSections?.ToList() ?? new List<string>()
            },
            QuestionResponseValue.AssessmentResponse assessmentResponse => new AssessmentResponseDataDto
            {
                Competencies = (assessmentResponse.Competencies ?? new Dictionary<string, CompetencyRating>())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => new CompetencyRatingDto
                        {
                            Rating = kvp.Value.Rating,
                            Comment = kvp.Value.Comment
                        }
                    )
            },
            QuestionResponseValue.GoalResponse goalResponse => new GoalResponseDataDto
            {
                Goals = (goalResponse.Goals ?? Enumerable.Empty<GoalData>()).Select(g => new GoalDataDto
                {
                    Description = g.ObjectiveDescription,
                    AchievementPercentage = 0, // Not available in domain GoalData
                    Justification = null, // Not available in domain GoalData
                    Weight = (double)g.WeightingPercentage
                }).ToList(),
                PredecessorRatings = (goalResponse.PredecessorRatings ?? Enumerable.Empty<PredecessorRating>()).Select(pr => new PredecessorRatingDto
                {
                    GoalDescription = pr.OriginalObjective,
                    Rating = pr.DegreeOfAchievement,
                    Comment = pr.Justification
                }).ToList(),
                PredecessorAssignmentId = goalResponse.PredecessorAssignmentId
            },
            _ => new TextResponseDataDto { TextSections = [responseValue.ToString() ?? string.Empty] }
        };
    }

    /// <summary>
    /// Infers the QuestionType from a strongly-typed QuestionResponseValue.
    /// </summary>
    public static QuestionType InferQuestionType(QuestionResponseValue responseValue)
    {
        return responseValue switch
        {
            QuestionResponseValue.TextResponse => QuestionType.TextQuestion,
            QuestionResponseValue.AssessmentResponse => QuestionType.Assessment,
            QuestionResponseValue.GoalResponse => QuestionType.Goal,
            _ => QuestionType.TextQuestion // Default fallback
        };
    }
}