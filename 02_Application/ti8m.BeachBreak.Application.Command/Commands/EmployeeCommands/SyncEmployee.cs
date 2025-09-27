namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class SyncEmployee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string LastStartDate { get; set; } = string.Empty;
    public string ManagerId { get; set; } = string.Empty;
    public string LoginName { get; set; } = string.Empty;
    public string OrganizationNumber { get; set; } = string.Empty;
}