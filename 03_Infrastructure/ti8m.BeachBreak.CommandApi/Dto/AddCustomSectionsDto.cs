using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for adding custom question sections to an assignment during initialization.
/// Custom sections are instance-specific and excluded from aggregate reports.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class AddCustomSectionsDto
{
    /// <summary>
    /// List of custom question sections to add
    /// </summary>
    public List<QuestionSectionDto> Sections { get; set; } = new();
}
