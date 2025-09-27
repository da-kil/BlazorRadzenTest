namespace ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;

public class BulkDeleteOrganizationsCommand : ICommand<Result>
{
    public IEnumerable<string> OrgNumbers { get; }

    public BulkDeleteOrganizationsCommand(IEnumerable<string> orgNumbers)
    {
        OrgNumbers = orgNumbers;
    }
}
