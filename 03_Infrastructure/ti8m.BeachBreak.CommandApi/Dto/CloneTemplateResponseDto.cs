using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// Response DTO containing the ID of the newly cloned template.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class CloneTemplateResponseDto
{
    /// <summary>
    /// The unique identifier of the newly created cloned template.
    /// </summary>
    public Guid NewTemplateId { get; set; }
}
