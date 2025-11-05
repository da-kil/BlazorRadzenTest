namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Clean DTO representing a complete questionnaire response.
/// Replaces complex nested dictionaries with strongly-typed structure.
/// </summary>
public class QuestionnaireResponseDto
{
    public Dictionary<Guid, QuestionResponseDto> Responses { get; set; } = new();

    /// <summary>
    /// Gets response for a specific question, if it exists.
    /// </summary>
    public QuestionResponseDto? GetResponse(Guid questionId) =>
        Responses.TryGetValue(questionId, out var response) ? response : null;
}