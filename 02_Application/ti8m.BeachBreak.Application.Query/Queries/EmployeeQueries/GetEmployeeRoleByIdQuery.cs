namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public record GetEmployeeRoleByIdQuery(Guid UserId) : IQuery<EmployeeRoleResult?>;
