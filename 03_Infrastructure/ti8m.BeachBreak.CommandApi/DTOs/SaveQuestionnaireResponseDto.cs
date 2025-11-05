namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// Command DTO for saving questionnaire responses with strong typing.
/// Eliminates the need for complex Dictionary<string, object> parsing.
/// </summary>
public class SaveQuestionnaireResponseDto
{
    /// <summary>
    /// Optional template ID to optimize section mapping.
    /// When provided, skips assignment lookup and goes directly to template lookup.
    /// When null, falls back to querying assignment to get template ID.
    /// </summary>
    public Guid? TemplateId { get; set; }

    public Dictionary<Guid, QuestionResponseCommandDto> Responses { get; set; } = new();
}