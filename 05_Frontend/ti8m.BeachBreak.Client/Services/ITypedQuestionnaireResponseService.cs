using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Interface for type-safe questionnaire response operations.
/// Eliminates Dictionary<string, object> from service contracts.
/// </summary>
public interface ITypedQuestionnaireResponseService
{
    /// <summary>
    /// Gets a complete questionnaire response with strongly-typed data.
    /// </summary>
    Task<QuestionnaireResponseDto?> GetResponseAsync(Guid assignmentId);

    /// <summary>
    /// Saves a text response with compile-time type safety.
    /// </summary>
    Task<Result> SaveTextResponseAsync(Guid assignmentId, Guid questionId, List<string> textSections);

    /// <summary>
    /// Saves an assessment response with compile-time type safety.
    /// </summary>
    Task<Result> SaveAssessmentResponseAsync(Guid assignmentId, Guid questionId, Dictionary<string, CompetencyRatingDto> competencies);

    /// <summary>
    /// Saves a goal response with compile-time type safety.
    /// </summary>
    Task<Result> SaveGoalResponseAsync(Guid assignmentId, Guid questionId, GoalResponseDto goalData);

    /// <summary>
    /// Gets a specific question response with type safety.
    /// </summary>
    Task<QuestionResponseDto?> GetQuestionResponseAsync(Guid assignmentId, Guid questionId);
}