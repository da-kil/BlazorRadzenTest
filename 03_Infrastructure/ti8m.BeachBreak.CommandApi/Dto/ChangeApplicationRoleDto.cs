using ti8m.BeachBreak.Application.Command.Models;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class ChangeApplicationRoleDto
{
    public ApplicationRole NewRole { get; set; }
}
