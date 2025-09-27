namespace ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;

public class BulkImportOrganizationCommand(IEnumerable<SyncOrganization> organizations) : ICommand<Result>
{
    public IEnumerable<SyncOrganization> Organizations { get; } = organizations;
}
