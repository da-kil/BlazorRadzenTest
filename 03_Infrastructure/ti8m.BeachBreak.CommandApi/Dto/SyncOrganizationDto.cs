namespace ti8m.BeachBreak.CommandApi.Dto;

public class SyncOrganizationDto
{
    public required string Number { get; set; }
    public string? ParentNumber { get; set; }
    public required string Name { get; set; }
    public string? ManagerUserId { get; set; }
}
