namespace ti8m.BeachBreak.QueryApi.Dto;

public class AssignmentViewerDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
    public Guid AddedByEmployeeId { get; set; }
    public string? AddedByName { get; set; }
}
