namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for adding a viewer to a questionnaire assignment
/// </summary>
public class AddViewerDto
{
    /// <summary>
    /// ID of the employee to be added as a viewer
    /// </summary>
    public Guid ViewerEmployeeId { get; set; }
}