using ti8m.BeachBreak.CommandApi.DTOs;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.CommandApi.Services;

/// <summary>
/// Service for mapping between strongly-typed CommandApi DTOs and Domain value objects.
/// No more Dictionary<string, object> casting - everything is type-safe!
/// </summary>
public class QuestionResponseMappingService
{
    /// <summary>
    /// Converts strongly-typed DTOs directly to domain value objects.
    /// Much simpler than the old object-based approach!
    /// </summary>
    public Dictionary<Guid, QuestionResponseValue> ConvertToTypeSafeFormat(SaveQuestionnaireResponseDto dto)
    {
        var questionResponses = new Dictionary<Guid, QuestionResponseValue>();

        foreach (var response in dto.Responses)
        {
            var questionId = response.Key;
            var questionResponse = response.Value;

            QuestionResponseValue domainResponse = questionResponse.QuestionType switch
            {
                QuestionType.TextQuestion when questionResponse.TextResponse != null =>
                    new QuestionResponseValue.TextResponse(questionResponse.TextResponse.TextSections),

                QuestionType.Assessment when questionResponse.AssessmentResponse != null =>
                    new QuestionResponseValue.AssessmentResponse(
                        questionResponse.AssessmentResponse.Competencies.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new CompetencyRating(kvp.Value.Rating, kvp.Value.Comment))),

                QuestionType.Goal when questionResponse.GoalResponse != null =>
                    new QuestionResponseValue.GoalResponse(
                        MapGoalsToDomain(questionResponse.GoalResponse.Goals),
                        MapPredecessorRatingsToDomain(questionResponse.GoalResponse.PredecessorRatings),
                        questionResponse.GoalResponse.PredecessorAssignmentId),

                _ => throw new ArgumentException($"Invalid question type or missing response data: {questionResponse.QuestionType}")
            };

            questionResponses[questionId] = domainResponse;
        }

        return questionResponses;
    }

    private List<GoalData> MapGoalsToDomain(IEnumerable<GoalDataDto> dtos)
    {
        return dtos.Select(dto => new GoalData(
            dto.GoalId,
            dto.ObjectiveDescription,
            dto.TimeframeFrom,
            dto.TimeframeTo,
            dto.MeasurementMetric,
            dto.WeightingPercentage,
            dto.AddedByRole
        )).ToList();
    }

    private List<PredecessorRating> MapPredecessorRatingsToDomain(IEnumerable<PredecessorRatingDto> dtos)
    {
        return dtos.Select(dto => new PredecessorRating(
            dto.SourceGoalId,
            dto.DegreeOfAchievement,
            dto.Justification,
            dto.RatedByRole,
            dto.OriginalObjective,
            dto.OriginalAddedByRole
        )).ToList();
    }
}