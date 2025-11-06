using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

/// <summary>
/// Command to change an employee's application role.
/// RequesterRole must be provided by the infrastructure layer after fetching from database.
/// </summary>
public record ChangeEmployeeApplicationRoleCommand(
    Guid EmployeeId,
    ApplicationRole NewRole,
    ApplicationRole RequesterRole) : ICommand<Result>;
