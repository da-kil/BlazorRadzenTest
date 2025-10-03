using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public record EmployeeRoleResult(Guid EmployeeId, ApplicationRole ApplicationRole);
