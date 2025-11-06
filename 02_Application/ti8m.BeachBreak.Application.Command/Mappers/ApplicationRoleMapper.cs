using ti8m.BeachBreak.Application.Command.Models;
using DomainApplicationRole = ti8m.BeachBreak.Domain.EmployeeAggregate.ApplicationRole;

namespace ti8m.BeachBreak.Application.Command.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between Domain and Application.Command ApplicationRole enums.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// </summary>
public static class ApplicationRoleMapper
{
    /// <summary>
    /// Maps from Domain ApplicationRole to Application.Command ApplicationRole.
    /// Used when receiving Domain data and converting to Command operations.
    /// </summary>
    public static ApplicationRole MapFromDomain(DomainApplicationRole domainRole)
    {
        return domainRole switch
        {
            DomainApplicationRole.Employee => ApplicationRole.Employee,
            DomainApplicationRole.TeamLead => ApplicationRole.TeamLead,
            DomainApplicationRole.HR => ApplicationRole.HR,
            DomainApplicationRole.HRLead => ApplicationRole.HRLead,
            DomainApplicationRole.Admin => ApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(domainRole), domainRole,
                $"Unknown Domain ApplicationRole: {domainRole}")
        };
    }

    /// <summary>
    /// Maps from Application.Command ApplicationRole to Domain ApplicationRole.
    /// Used when Command handlers need to call Domain aggregate methods.
    /// </summary>
    public static DomainApplicationRole MapToDomain(ApplicationRole commandRole)
    {
        return commandRole switch
        {
            ApplicationRole.Employee => DomainApplicationRole.Employee,
            ApplicationRole.TeamLead => DomainApplicationRole.TeamLead,
            ApplicationRole.HR => DomainApplicationRole.HR,
            ApplicationRole.HRLead => DomainApplicationRole.HRLead,
            ApplicationRole.Admin => DomainApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(commandRole), commandRole,
                $"Unknown Command ApplicationRole: {commandRole}")
        };
    }
}