using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class LinkPredecessorQuestionnaireDto
{
    public Guid QuestionId { get; set; }
    public Guid PredecessorAssignmentId { get; set; }
    public string LinkedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
}
