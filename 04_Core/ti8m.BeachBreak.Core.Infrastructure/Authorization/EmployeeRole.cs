namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

/// <summary>
/// Employee role information for authorization infrastructure.
/// Simple shared model between Command and Query sides.
/// </summary>
public record EmployeeRole(Guid EmployeeId, int ApplicationRoleValue);