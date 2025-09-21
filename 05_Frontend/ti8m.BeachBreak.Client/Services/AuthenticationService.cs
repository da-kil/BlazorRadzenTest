using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserContextService _userContext;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserContextService userContext,
        ILogger<AuthenticationService> logger)
    {
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        try
        {
            return await _userContext.GetCurrentUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            throw;
        }
    }

    public string GetCurrentEmployeeId()
    {
        try
        {
            return _userContext.CurrentEmployeeId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current employee ID");
            return string.Empty;
        }
    }

    public string GetCurrentUserRole()
    {
        try
        {
            return _userContext.CurrentRole.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user role");
            return string.Empty;
        }
    }

    public bool IsInRole(string role)
    {
        try
        {
            return _userContext.CurrentRole.ToString().Equals(role, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user role {Role}", role);
            return false;
        }
    }
}