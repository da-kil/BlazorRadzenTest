using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace ti8m.BeachBreak.Authorization;

/// <summary>
/// Claims transformation that enriches the user principal with ApplicationRole.
/// Runs on every request, allowing dynamic role updates without re-authentication.
/// </summary>
public class ApplicationRoleClaimsTransformation : IClaimsTransformation
{
    private const string TransformationMarker = "__ClaimsTransformed";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApplicationRoleClaimsTransformation> _logger;

    public ApplicationRoleClaimsTransformation(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApplicationRoleClaimsTransformation> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        _logger.LogInformation("TransformAsync called - IsAuthenticated: {IsAuth}", principal.Identity?.IsAuthenticated);

        // Only transform authenticated users
        if (principal.Identity?.IsAuthenticated != true)
        {
            _logger.LogInformation("User not authenticated, skipping transformation");
            return principal;
        }

        // Check if already transformed to prevent infinite recursion
        if (principal.HasClaim(c => c.Type == TransformationMarker))
        {
            _logger.LogInformation("Already has transformation marker, skipping");
            return principal;
        }

        // Also check if already has ApplicationRole (from previous transformation)
        if (principal.HasClaim(c => c.Type == "ApplicationRole"))
        {
            _logger.LogInformation("Already has ApplicationRole, skipping");
            return principal;
        }

        _logger.LogInformation("Starting claims transformation...");

        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext not available for claims transformation");
                return principal;
            }

            // Get user ID from claims
            var userId = principal.FindFirst("oid")?.Value
                        ?? principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                        ?? principal.FindFirst("sub")?.Value;

            _logger.LogInformation("User ID from claims: {UserId}", userId ?? "NULL");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Cannot transform claims: User ID not found");
                // Log all claims to debug
                foreach (var claim in principal.Claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                return principal;
            }

            // Mark as being transformed to prevent recursion during GetTokenAsync
            var identity = principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                identity.AddClaim(new Claim(TransformationMarker, "true"));
            }

            _logger.LogInformation("Getting access token...");
            // Get access token (this can trigger claims transformation recursively)
            var accessToken = await httpContext.GetTokenAsync("access_token");
            _logger.LogInformation("Access token retrieved: {HasToken}", !string.IsNullOrEmpty(accessToken));

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Cannot transform claims: Access token not available");
                return principal;
            }

            // Call backend to get ApplicationRole
            var client = _httpClientFactory.CreateClient("QueryClient");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("q/api/v1/auth/me/role");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch ApplicationRole: {StatusCode}", response.StatusCode);
                return principal;
            }

            var json = await response.Content.ReadAsStringAsync();
            var roleData = JsonDocument.Parse(json);

            if (!roleData.RootElement.TryGetProperty("ApplicationRole", out var roleProperty) ||
                !roleData.RootElement.TryGetProperty("EmployeeId", out var employeeIdProperty))
            {
                _logger.LogWarning("ApplicationRole or EmployeeId not found in response");
                return principal;
            }

            var applicationRole = roleProperty.GetInt32();
            var roleName = ((ApplicationRole)applicationRole).ToString();
            var employeeId = employeeIdProperty.GetGuid();

            // Create new identity with additional claims
            var claims = new List<Claim>
            {
                new Claim("ApplicationRole", roleName),
                new Claim("EmployeeId", employeeId.ToString()),
                new Claim(ClaimTypes.Role, roleName) // Standard role claim for authorization
            };

            var claimsIdentity = new ClaimsIdentity(claims);
            principal.AddIdentity(claimsIdentity);

            _logger.LogInformation("Transformed claims with ApplicationRole: {Role} for user {UserId}",
                roleName, userId);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming user claims");
            return principal;
        }
    }
}
