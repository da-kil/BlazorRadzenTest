using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class SyncOrganizationDto
{
    public required string Number { get; set; }
    public string? ParentNumber { get; set; }
    public required string Name { get; set; }
    public string? ManagerUserId { get; set; }
}
