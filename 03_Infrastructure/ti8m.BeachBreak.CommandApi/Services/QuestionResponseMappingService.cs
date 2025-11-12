using ti8m.BeachBreak.CommandApi.DTOs;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using DomainApplicationRole = ti8m.BeachBreak.Domain.EmployeeAggregate.ApplicationRole;

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
                QuestionType.TextQuestion => questionResponse.TextResponse != null
                    ? new QuestionResponseValue.TextResponse(questionResponse.TextResponse.TextSections)
                    : new QuestionResponseValue.TextResponse(new List<string>()), // Empty text response

                QuestionType.Assessment => questionResponse.AssessmentResponse != null
                    ? new QuestionResponseValue.AssessmentResponse(
                        questionResponse.AssessmentResponse.Competencies.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new CompetencyRating(kvp.Value.Rating, kvp.Value.Comment)))
                    : new QuestionResponseValue.AssessmentResponse(new Dictionary<string, CompetencyRating>()), // Empty assessment response

                QuestionType.Goal => questionResponse.GoalResponse != null
                    ? new QuestionResponseValue.GoalResponse(
                        MapGoalsToDomain(questionResponse.GoalResponse.Goals),
                        MapPredecessorRatingsToDomain(questionResponse.GoalResponse.PredecessorRatings),
                        questionResponse.GoalResponse.PredecessorAssignmentId)
                    : new QuestionResponseValue.GoalResponse(
                        new List<GoalData>(),
                        new List<PredecessorRating>(),
                        null), // Empty goal response

                _ => throw new ArgumentException($"Invalid question type: {questionResponse.QuestionType}")
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
            (DomainApplicationRole)(int)dto.AddedByRole
        )).ToList();
    }

    private List<PredecessorRating> MapPredecessorRatingsToDomain(IEnumerable<PredecessorRatingDto> dtos)
    {
        return dtos.Select(dto => new PredecessorRating(
            dto.SourceGoalId,
            dto.DegreeOfAchievement,
            dto.Justification,
            (DomainApplicationRole)(int)dto.RatedByRole,
            dto.OriginalObjective,
            (DomainApplicationRole)(int)dto.OriginalAddedByRole
        )).ToList();
    }
}