using System.ComponentModel.DataAnnotations;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class SendReminderDto
{
    [Required]
    public Guid AssignmentId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
