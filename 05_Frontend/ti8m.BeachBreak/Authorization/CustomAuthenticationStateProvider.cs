using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;
using System.Text.Json;

namespace ti8m.BeachBreak.Authorization;

/// <summary>
/// ApplicationRole enum matching the domain model
/// </summary>
public enum ApplicationRole
{
    Employee = 0,
    TeamLead = 1,
    HR = 2,
    HRLead = 3,
    Admin = 4
}

/// <summary>
/// Custom authentication state provider that enriches user claims with ApplicationRole from backend.
/// This runs when Blazor requests authentication state, before [Authorize] checks.
/// </summary>
public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<CustomAuthenticationStateProvider> logger;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(loggerFactory)
    {
        this.httpClientFactory = httpClientFactory;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = loggerFactory.CreateLogger<CustomAuthenticationStateProvider>();
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        // Return true to keep the current authentication state valid
        return await Task.FromResult(true);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var authenticationState = await base.GetAuthenticationStateAsync();
        var user = authenticationState.User;

        // Only enrich if user is authenticated and doesn't already have ApplicationRole claim
        if (user.Identity?.IsAuthenticated == true &&
            !user.HasClaim(c => c.Type == "ApplicationRole"))
        {
            try
            {
                var enrichedUser = await EnrichUserWithApplicationRoleAsync(user);
                if (enrichedUser != null)
                {
                    return new AuthenticationState(enrichedUser);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to enrich user with ApplicationRole");
            }
        }

        return authenticationState;
    }

    private async Task<ClaimsPrincipal?> EnrichUserWithApplicationRoleAsync(ClaimsPrincipal user)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            logger.LogWarning("Cannot enrich claims: HttpContext not available");
            return null;
        }

        // Get user ID from claims
        var userIdClaim = user.FindFirst("oid")?.Value
                         ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            logger.LogWarning("Cannot enrich claims: User ID not found");
            return null;
        }

        try
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                logger.LogWarning("Cannot enrich claims: Access token not available");
                return null;
            }

            var client = httpClientFactory.CreateClient("QueryClient");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("q/api/v1/auth/me/role");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to fetch ApplicationRole: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var roleData = JsonDocument.Parse(json);

            if (!roleData.RootElement.TryGetProperty("ApplicationRole", out var roleProperty) ||
                !roleData.RootElement.TryGetProperty("EmployeeId", out var employeeIdProperty))
            {
                logger.LogWarning("ApplicationRole or EmployeeId not found in response. JSON: {Json}", json);
                return null;
            }

            var applicationRole = roleProperty.GetInt32();
            var roleName = ((ApplicationRole)applicationRole).ToString();
            var employeeId = employeeIdProperty.GetGuid();

            // Create new claims identity with ApplicationRole
            var claims = new List<Claim>
            {
                new("ApplicationRole", roleName),
                new("EmployeeId", employeeId.ToString()),
                new(ClaimTypes.Role, roleName) // Add as standard role claim for [Authorize] to work
            };

            var identity = new ClaimsIdentity(claims);
            user.AddIdentity(identity);

            logger.LogInformation("Enriched user claims with ApplicationRole: {Role} for user {UserId}",
                roleName, userIdClaim);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enriching user claims with ApplicationRole for user {UserId}", userIdClaim);
            return null;
        }
    }
}
