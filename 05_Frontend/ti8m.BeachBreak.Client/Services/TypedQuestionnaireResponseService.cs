using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto.Commands;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Type-safe service for questionnaire responses.
/// Eliminates Dictionary<string, object> and magic strings from the frontend.
/// </summary>
public class TypedQuestionnaireResponseService : BaseApiService, ITypedQuestionnaireResponseService
{
    private const string QueryEndpoint = "q/api/v1/responses";
    private const string CommandEndpoint = "c/api/v1/responses";

    public TypedQuestionnaireResponseService(IHttpClientFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Gets a complete questionnaire response with strongly-typed data.
    /// </summary>
    public async Task<QuestionnaireResponseDto?> GetResponseAsync(Guid assignmentId)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<QuestionnaireResponseDto>($"{QueryEndpoint}/assignment/{assignmentId}");
        }
        catch (Exception ex)
        {
            LogError($"Error fetching response for assignment {assignmentId}", ex);
            return null;
        }
    }

    /// <summary>
    /// Saves a text response with type safety - no magic strings!
    /// </summary>
    public async Task<Result> SaveTextResponseAsync(Guid assignmentId, Guid questionId, List<string> textSections)
    {
        try
        {
            var dto = new SaveQuestionnaireResponseDto();
            dto.AddTextResponse(questionId, textSections);

            var response = await HttpCommandClient.PostAsJsonAsync($"{CommandEndpoint}/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Result>();
            return result ?? Result.Fail("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            LogError($"Error saving text response for question {questionId}", ex);
            return Result.Fail($"Failed to save text response: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves an assessment response with type safety - no magic strings!
    /// </summary>
    public async Task<Result> SaveAssessmentResponseAsync(Guid assignmentId, Guid questionId, Dictionary<string, CompetencyRatingDto> competencies)
    {
        try
        {
            var dto = new SaveQuestionnaireResponseDto();
            dto.AddAssessmentResponse(questionId, competencies.ToDictionary(
                kvp => kvp.Key,
                kvp => new CompetencyRatingCommandDto
                {
                    Rating = kvp.Value.Rating,
                    Comment = kvp.Value.Comment
                }));

            var response = await HttpCommandClient.PostAsJsonAsync($"{CommandEndpoint}/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Result>();
            return result ?? Result.Fail("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            LogError($"Error saving assessment response for question {questionId}", ex);
            return Result.Fail($"Failed to save assessment response: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves a goal response with type safety - no magic strings!
    /// </summary>
    public async Task<Result> SaveGoalResponseAsync(Guid assignmentId, Guid questionId, GoalResponseDto goalData)
    {
        try
        {
            var commandDto = new GoalResponseCommandDto
            {
                Goals = goalData.Goals.Select(g => new GoalDataCommandDto
                {
                    GoalId = g.GoalId,
                    ObjectiveDescription = g.ObjectiveDescription,
                    TimeframeFrom = g.TimeframeFrom,
                    TimeframeTo = g.TimeframeTo,
                    MeasurementMetric = g.MeasurementMetric,
                    WeightingPercentage = g.WeightingPercentage,
                    AddedByRole = g.AddedByRole
                }).ToList(),
                PredecessorRatings = goalData.PredecessorRatings.Select(r => new PredecessorRatingCommandDto
                {
                    SourceGoalId = r.SourceGoalId,
                    DegreeOfAchievement = r.DegreeOfAchievement,
                    Justification = r.Justification,
                    RatedByRole = r.RatedByRole,
                    OriginalObjective = r.OriginalObjective,
                    OriginalAddedByRole = r.OriginalAddedByRole
                }).ToList(),
                PredecessorAssignmentId = goalData.PredecessorAssignmentId
            };

            var dto = new SaveQuestionnaireResponseDto();
            dto.AddGoalResponse(questionId, commandDto);

            var response = await HttpCommandClient.PostAsJsonAsync($"{CommandEndpoint}/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Result>();
            return result ?? Result.Fail("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            LogError($"Error saving goal response for question {questionId}", ex);
            return Result.Fail($"Failed to save goal response: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a specific question response with type safety.
    /// </summary>
    public async Task<QuestionResponseDto?> GetQuestionResponseAsync(Guid assignmentId, Guid questionId)
    {
        try
        {
            var fullResponse = await GetResponseAsync(assignmentId);
            return fullResponse?.GetResponse(questionId);
        }
        catch (Exception ex)
        {
            LogError($"Error fetching question response {questionId}", ex);
            return null;
        }
    }
}