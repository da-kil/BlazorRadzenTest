namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for assignment viewers in the client model
/// </summary>
public class AssignmentViewerDto
{
    /// <summary>
    /// ID of the employee who is a viewer
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Display name of the viewer employee
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the viewer employee
    /// </summary>
    public string EmployeeEmail { get; set; } = string.Empty;

    /// <summary>
    /// Date when the viewer was added to the assignment
    /// </summary>
    public DateTime AddedDate { get; set; }

    /// <summary>
    /// ID of the employee who added this viewer
    /// </summary>
    public Guid AddedByEmployeeId { get; set; }

    /// <summary>
    /// Display name of the employee who added this viewer
    /// </summary>
    public string? AddedByName { get; set; }
}