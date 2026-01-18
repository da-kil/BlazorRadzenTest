using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class BulkAssignmentReminderDto
{
    public IEnumerable<Guid> AssignmentIds { get; set; } = new List<Guid>();
    public string Message { get; set; } = string.Empty;
    public string SentBy { get; set; } = string.Empty;
}