using ti8m.BeachBreak.Application.Query.Models;
using DomainApplicationRole = ti8m.BeachBreak.Domain.EmployeeAggregate.ApplicationRole;

namespace ti8m.BeachBreak.Application.Query.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between Domain and Application.Query ApplicationRole enums.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// </summary>
public static class ApplicationRoleMapper
{
    /// <summary>
    /// Maps from Domain ApplicationRole to Application.Query ApplicationRole.
    /// Provides compile-time safety and explicit error handling.
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
    /// Maps from Application.Query ApplicationRole to Domain ApplicationRole.
    /// Used when Query layer needs to interact with Domain operations.
    /// </summary>
    public static DomainApplicationRole MapToDomain(ApplicationRole queryRole)
    {
        return queryRole switch
        {
            ApplicationRole.Employee => DomainApplicationRole.Employee,
            ApplicationRole.TeamLead => DomainApplicationRole.TeamLead,
            ApplicationRole.HR => DomainApplicationRole.HR,
            ApplicationRole.HRLead => DomainApplicationRole.HRLead,
            ApplicationRole.Admin => DomainApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(queryRole), queryRole,
                $"Unknown Query ApplicationRole: {queryRole}")
        };
    }
}