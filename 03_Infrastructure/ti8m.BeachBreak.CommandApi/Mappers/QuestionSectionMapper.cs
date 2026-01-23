using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Mappers;

/// <summary>
/// Implementation of IQuestionSectionMapper for mapping QuestionSectionDto to CommandQuestionSection.
/// Centralizes mapping logic that was previously duplicated across controllers.
/// </summary>
public class QuestionSectionMapper : IQuestionSectionMapper
{
    /// <summary>
    /// Maps a single QuestionSectionDto to CommandQuestionSection.
    /// </summary>
    /// <param name="dto">The DTO to map</param>
    /// <returns>Mapped CommandQuestionSection</returns>
    public CommandQuestionSection MapToCommand(QuestionSectionDto dto)
    {
        return new CommandQuestionSection
        {
            Id = dto.Id,
            TitleGerman = dto.TitleGerman,
            TitleEnglish = dto.TitleEnglish,
            DescriptionGerman = dto.DescriptionGerman,
            DescriptionEnglish = dto.DescriptionEnglish,
            Order = dto.Order,
            CompletionRole = dto.CompletionRole,
            Type = dto.Type,
            Configuration = dto.Configuration
        };
    }

    /// <summary>
    /// Maps a collection of QuestionSectionDto to a list of CommandQuestionSection.
    /// </summary>
    /// <param name="dtos">The DTOs to map</param>
    /// <returns>List of mapped CommandQuestionSection</returns>
    public List<CommandQuestionSection> MapToCommandList(IEnumerable<QuestionSectionDto> dtos)
    {
        return dtos.Select(MapToCommand).ToList();
    }
}
