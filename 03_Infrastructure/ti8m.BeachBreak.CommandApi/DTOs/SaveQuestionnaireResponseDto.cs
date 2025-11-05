namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// Command DTO for saving questionnaire responses with strong typing.
/// Eliminates the need for complex Dictionary<string, object> parsing.
/// </summary>
public class SaveQuestionnaireResponseDto
{
    public Dictionary<Guid, QuestionResponseCommandDto> Responses { get; set; } = new();
}