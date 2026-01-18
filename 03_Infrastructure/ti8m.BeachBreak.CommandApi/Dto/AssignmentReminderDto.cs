using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class AssignmentReminderDto
{
    public Guid AssignmentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SentBy { get; set; } = string.Empty;
}