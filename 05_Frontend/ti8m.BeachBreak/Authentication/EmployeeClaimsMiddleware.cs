using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ti8m.BeachBreak.Authentication;

/// <summary>
/// Middleware that enriches user claims with employee data from QueryApi.
/// Runs after authentication and before authorization.
/// </summary>
public class EmployeeClaimsMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<EmployeeClaimsMiddleware> logger;

    public EmployeeClaimsMiddleware(
        RequestDelegate next,
        ILogger<EmployeeClaimsMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        logger.LogInformation("[Frontend] EmployeeClaimsMiddleware invoked");

        // Only process authenticated users who don't have employee claims yet
        if (context.User.Identity?.IsAuthenticated == true &&
            !context.User.HasClaim(c => c.Type == "ApplicationRole"))
        {
            var loginName = context.User.FindFirst("preferred_username")?.Value
                           ?? context.User.FindFirst(ClaimTypes.Name)?.Value
                           ?? context.User.FindFirst(ClaimTypes.Upn)?.Value
                           ?? context.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(loginName))
            {
                logger.LogInformation("[Frontend] Loading employee data for LoginName: {LoginName}", loginName);

                try
                {
                    var accessToken = await context.GetTokenAsync("access_token");

                    if (string.IsNullOrEmpty(accessToken))
                    {
                        logger.LogWarning("[Frontend] No access token found, cannot call QueryApi");
                    }
                    else
                    {
                        var client = httpClientFactory.CreateClient();
                        client.BaseAddress = new Uri($"{context.Request.Scheme}://{context.Request.Host}");
                        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

                        // TODO: Uncomment when endpoint is implemented
                        // var response = await client.GetAsync($"/q/api/v1/employees/by-login/{Uri.EscapeDataString(loginName)}");
                        // if (response.IsSuccessStatusCode)
                        // {
                        //     var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
                        //     if (employee != null)
                        //     {
                        //         // Create new claims identity with employee data
                        //         var claims = new List<Claim>
                        //         {
                        //             new("EmployeeId", employee.Id.ToString()),
                        //             new("ApplicationRole", employee.Role)
                        //         };
                        //
                        //         var identity = new ClaimsIdentity(claims);
                        //         context.User.AddIdentity(identity);
                        //
                        //         logger.LogInformation("[Frontend] Added employee claims for {LoginName}", loginName);
                        //     }
                        // }
                        // else
                        // {
                        //     logger.LogWarning("[Frontend] Failed to load employee data: {StatusCode}", response.StatusCode);
                        // }

                        logger.LogInformation("[Frontend] Employee claims middleware executed (endpoint not yet implemented)");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Frontend] Error loading employee data for LoginName: {LoginName}", loginName);
                }
            }
        }

        await next(context);
    }
}
