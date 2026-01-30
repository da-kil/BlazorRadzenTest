using System.Text.Json;
using ti8m.BeachBreak.CommandApi.DTOs;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
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
                        questionResponse.AssessmentResponse.Evaluations.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new EvaluationRating(kvp.Value.Rating, kvp.Value.Comment)))
                    : new QuestionResponseValue.AssessmentResponse(new Dictionary<string, EvaluationRating>()), // Empty assessment response

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

    /// <summary>
    /// Converts a single JSON answer from frontend to domain value object.
    /// Used for edit-answer operations during review.
    /// </summary>
    public QuestionResponseValue ConvertSingleAnswerFromJson(string answerJson)
    {
        if (string.IsNullOrWhiteSpace(answerJson) || !answerJson.TrimStart().StartsWith("{"))
        {
            // Fallback to text response for non-JSON answers
            return new QuestionResponseValue.TextResponse(new[] { answerJson ?? "" });
        }

        try
        {
            using var document = JsonDocument.Parse(answerJson);
            var root = document.RootElement;

            if (root.TryGetProperty("TextSections", out _))
            {
                var dto = JsonSerializer.Deserialize<TextResponseDto>(answerJson);
                return new QuestionResponseValue.TextResponse(dto?.TextSections ?? new List<string>());
            }
            else if (root.TryGetProperty("Evaluations", out _))
            {
                var dto = JsonSerializer.Deserialize<AssessmentResponseDto>(answerJson);
                var evaluations = dto?.Evaluations?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new EvaluationRating(kvp.Value.Rating, kvp.Value.Comment ?? string.Empty)
                ) ?? new Dictionary<string, EvaluationRating>();
                return new QuestionResponseValue.AssessmentResponse(evaluations);
            }
            else if (root.TryGetProperty("Goals", out _))
            {
                var dto = JsonSerializer.Deserialize<GoalResponseDto>(answerJson);
                var goals = dto?.Goals?.Select(g => new GoalData(
                    g.GoalId,
                    g.ObjectiveDescription,
                    g.TimeframeFrom,
                    g.TimeframeTo,
                    g.MeasurementMetric,
                    g.WeightingPercentage,
                    (DomainApplicationRole)(int)g.AddedByRole
                )).ToList() ?? new List<GoalData>();

                var predecessorRatings = dto?.PredecessorRatings?.Select(pr => new PredecessorRating(
                    pr.SourceGoalId,
                    pr.DegreeOfAchievement,
                    pr.Justification ?? string.Empty,
                    (DomainApplicationRole)(int)pr.RatedByRole,
                    pr.OriginalObjective,
                    (DomainApplicationRole)(int)pr.OriginalAddedByRole
                )).ToList() ?? new List<PredecessorRating>();

                return new QuestionResponseValue.GoalResponse(goals, predecessorRatings, dto?.PredecessorAssignmentId);
            }
            else
            {
                // Fallback for unrecognized JSON structure
                return new QuestionResponseValue.TextResponse(new[] { answerJson });
            }
        }
        catch (JsonException)
        {
            // Fallback for invalid JSON
            return new QuestionResponseValue.TextResponse(new[] { answerJson });
        }
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