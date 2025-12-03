using ti8m.BeachBreak.Application.Query.Models;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? LastStartDate { get; set; }
    public Guid? ManagerId { get; set; }
    public string Manager { get; set; } = string.Empty;
    public string LoginName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public int OrganizationNumber { get; set; }
    public string Organization { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
    public Language PreferredLanguage { get; set; }
}