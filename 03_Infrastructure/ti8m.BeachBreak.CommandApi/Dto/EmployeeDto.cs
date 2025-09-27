namespace ti8m.BeachBreak.CommandApi.Dto;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public required string EmployeeId { get; set; }
    public required string LoginName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EMail { get; set; }
    public required string Role { get; set; }
    public required string OrganizationNumber { get; set; }
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public required string LastStartDate { get; set; }
    public required string ManagerId { get; set; }
    public required string Manager { get; set; }
}