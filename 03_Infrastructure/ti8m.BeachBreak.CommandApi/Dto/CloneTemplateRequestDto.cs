namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// Request DTO for cloning a questionnaire template.
/// </summary>
public class CloneTemplateRequestDto
{
    /// <summary>
    /// Optional prefix to prepend to the cloned template name.
    /// Defaults to "Copy of " if not specified.
    /// </summary>
    public string? NamePrefix { get; set; }
}
