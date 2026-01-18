using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class ExtendAssignmentDueDateDto
{
    public Guid AssignmentId { get; set; }
    public DateTime NewDueDate { get; set; }
    public string? ExtensionReason { get; set; }
}