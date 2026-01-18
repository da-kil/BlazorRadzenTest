using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for adding a note during the InReview phase of a questionnaire assignment
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class AddInReviewNoteDto
{
    /// <summary>
    /// Note content (maximum 2000 characters)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional section ID for context (null = general note)
    /// </summary>
    public Guid? SectionId { get; set; }
}