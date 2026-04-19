namespace ti8m.BeachBreak.Core.Application.Contexts;

public class UserContext
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public void Reset(string id, string tenantId, string token, string name)
    {
        Id = id;
        TenantId = tenantId;
        Token = token;
        Name = name;
    }
}