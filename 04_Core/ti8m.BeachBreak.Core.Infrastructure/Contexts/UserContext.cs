namespace ti8m.BeachBreak.Core.Infrastructure.Contexts;

public class UserContext
{
    public string Id { get; internal set; } = string.Empty;
    public string TenantId { get; internal set; } = string.Empty;
    public string Token { get; internal set; } = string.Empty;
    public string Name { get; internal set; } = string.Empty;

    public void Reset(string id, string tenantId, string token, string name)
    {
        Id = id;
        TenantId = tenantId;
        Token = token;
        Name = name;
    }
}