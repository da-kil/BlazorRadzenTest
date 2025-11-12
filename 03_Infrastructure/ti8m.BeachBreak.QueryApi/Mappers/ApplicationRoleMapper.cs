using QueryApplicationRole = ti8m.BeachBreak.Application.Query.Models.ApplicationRole;
using DomainApplicationRole = ti8m.BeachBreak.Domain.EmployeeAggregate.ApplicationRole;

namespace ti8m.BeachBreak.QueryApi.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between Domain and Application.Query ApplicationRole enums for QueryApi controllers.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// </summary>
public static class ApplicationRoleMapper
{
    /// <summary>
    /// Maps from Domain ApplicationRole to Application.Query ApplicationRole.
    /// Used in QueryApi controllers when converting from Domain context to Query DTOs.
    /// </summary>
    public static QueryApplicationRole MapFromDomain(DomainApplicationRole domainRole)
    {
        return domainRole switch
        {
            DomainApplicationRole.Employee => QueryApplicationRole.Employee,
            DomainApplicationRole.TeamLead => QueryApplicationRole.TeamLead,
            DomainApplicationRole.HR => QueryApplicationRole.HR,
            DomainApplicationRole.HRLead => QueryApplicationRole.HRLead,
            DomainApplicationRole.Admin => QueryApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(domainRole), domainRole,
                $"Unknown Domain ApplicationRole: {domainRole}")
        };
    }

    /// <summary>
    /// Maps from Application.Query ApplicationRole to Domain ApplicationRole.
    /// Used when QueryApi needs to interact with Domain operations.
    /// </summary>
    public static DomainApplicationRole MapToDomain(QueryApplicationRole queryRole)
    {
        return queryRole switch
        {
            QueryApplicationRole.Employee => DomainApplicationRole.Employee,
            QueryApplicationRole.TeamLead => DomainApplicationRole.TeamLead,
            QueryApplicationRole.HR => DomainApplicationRole.HR,
            QueryApplicationRole.HRLead => DomainApplicationRole.HRLead,
            QueryApplicationRole.Admin => DomainApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(queryRole), queryRole,
                $"Unknown Query ApplicationRole: {queryRole}")
        };
    }
}