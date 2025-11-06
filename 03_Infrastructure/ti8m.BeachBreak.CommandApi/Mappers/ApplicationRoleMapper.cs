using CommandApplicationRole = ti8m.BeachBreak.Application.Command.Models.ApplicationRole;
using QueryApplicationRole = ti8m.BeachBreak.Application.Query.Models.ApplicationRole;
using DomainApplicationRole = ti8m.BeachBreak.Domain.EmployeeAggregate.ApplicationRole;

namespace ti8m.BeachBreak.CommandApi.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between different ApplicationRole enums for CommandApi controllers.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// </summary>
public static class ApplicationRoleMapper
{
    /// <summary>
    /// Maps from Domain ApplicationRole to Application.Command ApplicationRole.
    /// Used when CommandApi receives Domain roles (from authorization) and needs to create Commands.
    /// </summary>
    public static CommandApplicationRole MapFromDomain(DomainApplicationRole domainRole)
    {
        return domainRole switch
        {
            DomainApplicationRole.Employee => CommandApplicationRole.Employee,
            DomainApplicationRole.TeamLead => CommandApplicationRole.TeamLead,
            DomainApplicationRole.HR => CommandApplicationRole.HR,
            DomainApplicationRole.HRLead => CommandApplicationRole.HRLead,
            DomainApplicationRole.Admin => CommandApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(domainRole), domainRole,
                $"Unknown Domain ApplicationRole: {domainRole}")
        };
    }

    /// <summary>
    /// Maps from Application.Query ApplicationRole to Application.Command ApplicationRole.
    /// Used when CommandApi receives Query roles (from user context) and needs to create Commands.
    /// </summary>
    public static CommandApplicationRole MapFromQuery(QueryApplicationRole queryRole)
    {
        return queryRole switch
        {
            QueryApplicationRole.Employee => CommandApplicationRole.Employee,
            QueryApplicationRole.TeamLead => CommandApplicationRole.TeamLead,
            QueryApplicationRole.HR => CommandApplicationRole.HR,
            QueryApplicationRole.HRLead => CommandApplicationRole.HRLead,
            QueryApplicationRole.Admin => CommandApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(queryRole), queryRole,
                $"Unknown Query ApplicationRole: {queryRole}")
        };
    }

    /// <summary>
    /// Maps from Application.Command ApplicationRole to Domain ApplicationRole.
    /// Used when CommandApi needs to convert Command roles back to Domain for comparisons/authorization.
    /// </summary>
    public static DomainApplicationRole MapToDomain(CommandApplicationRole commandRole)
    {
        return commandRole switch
        {
            CommandApplicationRole.Employee => DomainApplicationRole.Employee,
            CommandApplicationRole.TeamLead => DomainApplicationRole.TeamLead,
            CommandApplicationRole.HR => DomainApplicationRole.HR,
            CommandApplicationRole.HRLead => DomainApplicationRole.HRLead,
            CommandApplicationRole.Admin => DomainApplicationRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(commandRole), commandRole,
                $"Unknown Command ApplicationRole: {commandRole}")
        };
    }
}