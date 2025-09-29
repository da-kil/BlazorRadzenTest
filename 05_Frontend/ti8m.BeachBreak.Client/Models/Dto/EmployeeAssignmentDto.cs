namespace ti8m.BeachBreak.Client.Models.Dto;

public class EmployeeAssignmentDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
}