namespace ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands
{
    public class SyncOrganization
    {
        public required string Number { get; set; }
        public string? ParentNumber { get; set; }
        public required string Name { get; set; }
        public string? ManagerUserId { get; set; }
    }
}