namespace ti8m.BeachBreak.Application.Command.Models;

/// <summary>
/// Application roles for Command side of CQRS architecture.
/// This maintains architectural independence from domain enums while providing type safety.
/// Values must match domain ApplicationRole enum for proper mapping.
/// </summary>
public enum ApplicationRole
{
    Employee = 0,
    TeamLead = 1,
    HR = 2,
    HRLead = 3,
    Admin = 4
}