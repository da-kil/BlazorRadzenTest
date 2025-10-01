using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace ti8m.BeachBreak.Authentication;

/// <summary>
/// Transforms user claims by calling the QueryApi to get Employee data.
/// This runs automatically after OpenIdConnect authentication in the frontend.
/// </summary>
public class FrontendClaimsTransformation : IClaimsTransformation
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FrontendClaimsTransformation> _logger;

    public FrontendClaimsTransformation(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<FrontendClaimsTransformation> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        _logger.LogInformation("[Frontend] FrontendClaimsTransformation.TransformAsync called");

        // Check if we've already transformed (avoid duplicate processing)
        if (principal.HasClaim(c => c.Type == "ApplicationRole"))
        {
            _logger.LogDebug("[Frontend] Claims already transformed, skipping");
            return principal;
        }

        // Get login name from token (try multiple claim types)
        var loginName = principal.FindFirst("preferred_username")?.Value
                       ?? principal.FindFirst(ClaimTypes.Name)?.Value
                       ?? principal.FindFirst(ClaimTypes.Upn)?.Value
                       ?? principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(loginName))
        {
            _logger.LogWarning("[Frontend] No login name found in claims, cannot load employee data");
            return principal;
        }

        _logger.LogInformation("[Frontend] Loading employee data for LoginName: {LoginName}", loginName);

        try
        {
            // Call QueryApi to get employee by login name
            var client = _httpClientFactory.CreateClient("QueryClient");

            // Get access token from current user
            var accessToken = principal.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            // For now, we'll skip the API call since we don't have an endpoint yet
            // Instead, just log that transformation was called
            _logger.LogInformation("[Frontend] Claims transformation called but employee lookup not yet implemented");

            // TODO: Call QueryApi endpoint to get employee by LoginName
            // var response = await client.GetAsync($"q/api/v1/employees/by-login/{loginName}");
            // if (response.IsSuccessStatusCode)
            // {
            //     var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
            //     // Add claims from employee
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Frontend] Error loading employee data for LoginName: {LoginName}", loginName);
        }

        return principal;
    }
}
