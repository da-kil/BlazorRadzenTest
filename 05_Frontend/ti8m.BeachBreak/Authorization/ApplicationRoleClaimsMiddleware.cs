using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace ti8m.BeachBreak.Authorization;

/// <summary>
/// Middleware that enriches user claims with ApplicationRole from the backend QueryAPI.
/// This ensures frontend components can check ApplicationRole for UI visibility.
/// </summary>
public class ApplicationRoleClaimsMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ApplicationRoleClaimsMiddleware> logger;

    public ApplicationRoleClaimsMiddleware(
        RequestDelegate next,
        ILogger<ApplicationRoleClaimsMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        // Only process authenticated users who don't have ApplicationRole claim yet
        if (context.User.Identity?.IsAuthenticated == true &&
            !context.User.HasClaim(c => c.Type == "ApplicationRole"))
        {
            var userId = GetUserId(context.User);
            if (userId.HasValue)
            {
                try
                {
                    var accessToken = await context.GetTokenAsync("access_token");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var client = httpClientFactory.CreateClient("QueryClient");
                        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

                        // Call the dedicated auth endpoint that bypasses role checking
                        var response = await client.GetAsync("api/v1/auth/me/role");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var roleData = JsonDocument.Parse(json);

                            if (roleData.RootElement.TryGetProperty("applicationRole", out var roleProperty))
                            {
                                var applicationRole = roleProperty.GetInt32(); // Enum value
                                var roleName = ((ApplicationRole)applicationRole).ToString();

                                if (roleData.RootElement.TryGetProperty("employeeId", out var employeeIdProperty))
                                {
                                    var employeeId = employeeIdProperty.GetGuid();

                                    // Add ApplicationRole claim to user identity
                                    var claims = new List<Claim>
                                    {
                                        new Claim("ApplicationRole", roleName),
                                        new Claim("EmployeeId", employeeId.ToString())
                                    };

                                    var identity = new ClaimsIdentity(claims);
                                    context.User.AddIdentity(identity);

                                    logger.LogInformation("Added ApplicationRole claim: {Role} for user {UserId}", roleName, userId);
                                }
                            }
                        }
                        else
                        {
                            logger.LogWarning("Failed to fetch user role: {StatusCode}", response.StatusCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error enriching user claims with ApplicationRole for user {UserId}", userId);
                }
            }
        }

        await next(context);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        // Try various claim types for user ID (Entra ID object ID)
        var userIdClaim = user.FindFirst("oid")?.Value
                         ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

/// <summary>
/// ApplicationRole enum matching the domain model
/// </summary>
public enum ApplicationRole
{
    Employee = 1,
    TeamLead = 2,
    HR = 3,
    HRLead = 4,
    Admin = 5
}
