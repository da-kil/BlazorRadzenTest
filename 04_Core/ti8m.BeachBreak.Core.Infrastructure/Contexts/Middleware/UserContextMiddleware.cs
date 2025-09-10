using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace ti8m.BeachBreak.Core.Infrastructure.Contexts.Middleware;

internal class UserContextMiddleware : IMiddleware
{
    private const string AuthorizationKey = "Authorization";

    private readonly ILogger<UserContextMiddleware> logger;
    private readonly UserContext userContext;

    public UserContextMiddleware(
        ILogger<UserContextMiddleware> logger,
        UserContext userContext)
    {
        this.logger = logger;
        this.userContext = userContext;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        DoInvoke(context);
        await next(context);
    }

    /// <summary>
    /// Extract information about the calling user (or service) from the authorization token. And store the information on the IUserContext object. 
    /// </summary>
    /// <remarks>
    /// A description of claims can be found here: https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens
    /// </remarks>
    protected virtual void ReadUserInformationFromToken(JwtSecurityToken authorizationToken)
    {
        userContext.Id = GetId(authorizationToken)!;
        userContext.TenantId = GetTenantId(authorizationToken)!;
        userContext.Name = GetName(authorizationToken)!;
    }

    private string? GetId(JwtSecurityToken authorizationToken)
    {
        var claim = authorizationToken.Claims.FirstOrDefault(claim => claim.Type == "oid");
        return claim?.Value;
    }

    private string? GetTenantId(JwtSecurityToken authorizationToken)
    {
        var claim = authorizationToken.Claims.FirstOrDefault(claim => claim.Type == "tid" || claim.Type == "http://schemas.microsoft.com/identity/claims/tenantid");
        return claim?.Value;
    }

    private string? GetName(JwtSecurityToken authorizationToken)
    {
        var claim = authorizationToken.Claims.FirstOrDefault(claim => claim.Type == "name");
        return claim?.Value;
    }

    private void DoInvoke(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(AuthorizationKey))
        {
            logger.LogDebug("No '{AuthorizationKey}' header found", AuthorizationKey);
            return;
        }

        string authorizationHeader = context.Request.Headers[AuthorizationKey].ToString();
        if (string.IsNullOrEmpty(authorizationHeader))
        {
            logger.LogDebug("'{AuthorizationKey}' header was empty", AuthorizationKey);
            return;
        }

        try
        {
            var authorizationToken = DecodeToken(authorizationHeader);
            ReadUserInformationFromToken(authorizationToken);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Unable to extract claims from '{AuthorizationKey}' header.", AuthorizationKey);
            throw;
        }
    }

    private JwtSecurityToken DecodeToken(string authorizationHeader)
    {
        var schemaLength = $"{JwtBearerDefaults.AuthenticationScheme} ".Length;
        userContext.Token = authorizationHeader.Substring(schemaLength);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ReadJwtToken(userContext.Token);
    }
}