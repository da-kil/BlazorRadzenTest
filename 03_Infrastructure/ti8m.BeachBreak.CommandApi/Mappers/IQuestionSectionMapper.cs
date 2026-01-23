using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Mappers;

/// <summary>
/// Service for mapping QuestionSectionDto to CommandQuestionSection.
/// Eliminates duplicate mapping logic across controllers.
/// </summary>
public interface IQuestionSectionMapper
{
    /// <summary>
    /// Maps a single QuestionSectionDto to CommandQuestionSection.
    /// </summary>
    /// <param name="dto">The DTO to map</param>
    /// <returns>Mapped CommandQuestionSection</returns>
    CommandQuestionSection MapToCommand(QuestionSectionDto dto);

    /// <summary>
    /// Maps a collection of QuestionSectionDto to a list of CommandQuestionSection.
    /// </summary>
    /// <param name="dtos">The DTOs to map</param>
    /// <returns>List of mapped CommandQuestionSection</returns>
    List<CommandQuestionSection> MapToCommandList(IEnumerable<QuestionSectionDto> dtos);
}
