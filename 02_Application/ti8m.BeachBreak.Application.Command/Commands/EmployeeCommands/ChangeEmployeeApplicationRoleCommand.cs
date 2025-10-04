using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public record ChangeEmployeeApplicationRoleCommand(
    Guid EmployeeId,
    ApplicationRole NewRole) : ICommand<Result>;
