using Microsoft.AspNetCore.Authentication;

namespace ti8m.BeachBreak.Authentication;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<BearerTokenHandler> logger;

    public BearerTokenHandler(IHttpContextAccessor httpContextAccessor, ILogger<BearerTokenHandler> logger)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            logger.LogInformation("BearerTokenHandler: Attempting to retrieve access token for request to {RequestUri}", request.RequestUri);

            // Check if user is authenticated
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                logger.LogWarning("BearerTokenHandler: User is not authenticated");
                return await base.SendAsync(request, cancellationToken);
            }

            var accessToken = await httpContext.GetTokenAsync("access_token");
            logger.LogInformation("BearerTokenHandler: Access token retrieved: {HasToken}", !string.IsNullOrEmpty(accessToken));

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                logger.LogInformation("BearerTokenHandler: Added Bearer token to request");
            }
            else
            {
                logger.LogWarning("BearerTokenHandler: No access token found in authentication properties");

                // Log available authentication properties for debugging
                var authResult = await httpContext.AuthenticateAsync();
                if (authResult?.Properties?.Items != null)
                {
                    logger.LogInformation("BearerTokenHandler: Available authentication properties:");
                    foreach (var prop in authResult.Properties.Items)
                    {
                        // Don't log actual token values for security, just keys
                        var value = prop.Key.Contains("token", StringComparison.OrdinalIgnoreCase) ? "[REDACTED]" : prop.Value;
                        logger.LogInformation("  {Key} = {Value}", prop.Key, value);
                    }
                }
                else
                {
                    logger.LogWarning("BearerTokenHandler: No authentication properties found");
                }
            }
        }
        else
        {
            logger.LogWarning("BearerTokenHandler: HttpContext is null");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
