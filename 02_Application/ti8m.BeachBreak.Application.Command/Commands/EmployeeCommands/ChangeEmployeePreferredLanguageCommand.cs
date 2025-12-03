using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public record ChangeEmployeePreferredLanguageCommand(
    Guid EmployeeId,
    Language PreferredLanguage,
    Guid ChangedBy
) : ICommand<Result>;