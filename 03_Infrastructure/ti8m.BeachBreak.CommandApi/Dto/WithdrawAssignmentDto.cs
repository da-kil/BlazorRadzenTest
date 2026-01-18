using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class WithdrawAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public string? WithdrawalReason { get; set; }
}