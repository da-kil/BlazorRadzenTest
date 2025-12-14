using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.QueryApi.Mappers;

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
                Evaluations = (assessmentResponse.Evaluations ?? new Dictionary<string, EvaluationRating>())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => new EvaluationRatingDto
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
                    GoalId = g.GoalId,
                    ObjectiveDescription = g.ObjectiveDescription,
                    TimeframeFrom = g.TimeframeFrom,
                    TimeframeTo = g.TimeframeTo,
                    MeasurementMetric = g.MeasurementMetric,
                    WeightingPercentage = g.WeightingPercentage,
                    AddedByRole = ApplicationRoleMapper.MapFromDomain(g.AddedByRole)
                }).ToList(),
                PredecessorRatings = (goalResponse.PredecessorRatings ?? Enumerable.Empty<PredecessorRating>()).Select(pr => new PredecessorRatingDto
                {
                    SourceGoalId = pr.SourceGoalId,
                    DegreeOfAchievement = pr.DegreeOfAchievement,
                    Justification = pr.Justification,
                    RatedByRole = ApplicationRoleMapper.MapFromDomain(pr.RatedByRole),
                    OriginalObjective = pr.OriginalObjective,
                    OriginalAddedByRole = ApplicationRoleMapper.MapFromDomain(pr.OriginalAddedByRole)
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