namespace ti8m.BeachBreak.Client.Services;

public interface IAuthenticationService
{
    Task<CurrentUser> GetCurrentUserAsync();
    string GetCurrentEmployeeId();
    string GetCurrentUserRole();
    bool IsInRole(string role);
}

// Note: CurrentUser is now defined in IUserContextService.cs to avoid duplication