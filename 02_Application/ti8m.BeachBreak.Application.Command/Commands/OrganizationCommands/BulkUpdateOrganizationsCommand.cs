namespace ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;

public class BulkUpdateOrganizationsCommand : ICommand<Result>
{
    public IEnumerable<SyncOrganization> Organizations { get; }

    public BulkUpdateOrganizationsCommand(IEnumerable<SyncOrganization> organizations)
    {
        Organizations = organizations;
    }
}