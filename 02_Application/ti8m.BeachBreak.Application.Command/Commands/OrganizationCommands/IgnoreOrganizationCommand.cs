namespace ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;

public class IgnoreOrganizationCommand(Guid organizationId) : ICommand<Result>
{
    public Guid OrganizationId { get; } = organizationId;
}