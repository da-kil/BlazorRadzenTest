using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Radzen;
using ti8m.BeachBreak.Authentication;
using ti8m.BeachBreak.Client.Configuration;
using ti8m.BeachBreak.Client.Services;
using ti8m.BeachBreak.Client.Services.Exports;
using ti8m.BeachBreak.Components;
using Blazored.LocalStorage;

namespace ti8m.BeachBreak;

public class Program
{
    public static void Main(string[] args)
    {
        const string MS_OIDC_SCHEME = "MicrosoftOidc";

        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        AzureEntraSettings azureEntraSettings = new();
        builder.Configuration.Bind("AzureAd", azureEntraSettings);

        // Add services to the container.
        builder.Services.AddAuthentication(MS_OIDC_SCHEME)
            .AddOpenIdConnect(MS_OIDC_SCHEME, oidcOptions =>
            {
                // For the following OIDC settings, any line that's commented out
                // represents a DEFAULT setting. If you adopt the default, you can
                // remove the line if you wish.

                // ........................................................................
                // The OIDC handler must use a sign-in scheme capable of persisting 
                // user credentials across requests.

                oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // ........................................................................

                // ........................................................................
                // The "openid" and "profile" scopes are required for the OIDC handler 
                // and included by default. You should enable these scopes here if scopes 
                // are provided by "Authentication:Schemes:MicrosoftOidc:Scope" 
                // configuration because configuration may overwrite the scopes collection.

                //oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                // ........................................................................

                // ........................................................................
                // The following paths must match the redirect and post logout redirect 
                // paths configured when registering the application with the OIDC provider. 
                // The default values are "/signin-oidc" and "/signout-callback-oidc".

                //oidcOptions.CallbackPath = new PathString("/signin-oidc");
                //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                // ........................................................................

                // ........................................................................
                // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
                // sign-out. The default value is "/signout-oidc".

                //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
                // ........................................................................

                // ........................................................................
                // The scope is configured in the Azure or Entra portal under 
                // "Expose an API". This is necessary for backend web API (MinimalApiJwt)
                // to validate the access token with AddBearerJwt. The following code example
                // uses a scope format of the App ID URI for an AAD B2C tenant type. If your
                // tenant is an ME-ID tenant, the App ID URI format is different:
                // The {CLIENT ID} is the application (client) ID of the MinimalApiJwt app 
                // registration.

                oidcOptions.Scope.Add($"api://{azureEntraSettings.ClientId}/{azureEntraSettings.Scope}");
                // ........................................................................

                // ........................................................................
                // The following example Authority is configured for Microsoft Entra ID
                // and a single-tenant application registration. Set the {TENANT ID} 
                // placeholder to the Tenant ID. The "common" Authority 
                // https://login.microsoftonline.com/common/v2.0/ should be used 
                // for multi-tenant apps. You can also use the "common" Authority for 
                // single-tenant apps, but it requires a custom IssuerValidator as shown 
                // in the comments below. 

                oidcOptions.Authority = $"{azureEntraSettings.Instance?.TrimEnd('/')}/{azureEntraSettings.TenantId}/v2.0/";
                // ........................................................................

                // ........................................................................
                // Set the Client ID for the app. Set the {CLIENT ID} placeholder to
                // the Client ID.

                oidcOptions.ClientId = azureEntraSettings.ClientId;
                oidcOptions.ClientSecret = azureEntraSettings.ClientSecret;
                // ........................................................................

                // ........................................................................
                // Setting ResponseType to "code" configures the OIDC handler to use 
                // authorization code flow. Implicit grants and hybrid flows are unnecessary
                // in this mode. In a Microsoft Entra ID app registration, you don't need to 
                // select either box for the authorization endpoint to return access tokens 
                // or ID tokens. The OIDC handler automatically requests the appropriate 
                // tokens using the code returned from the authorization endpoint.

                oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
                // ........................................................................

                // ........................................................................
                // // Set MapInboundClaims to "false" to obtain the original claim types from   
                // the token. Many OIDC servers use "name" and "role"/"roles" rather than 
                // the SOAP/WS-Fed defaults in ClaimTypes. Adjust these values if your 
                // identity provider uses different claim types.

                oidcOptions.MapInboundClaims = false;
                oidcOptions.TokenValidationParameters.NameClaimType = "name";
                oidcOptions.TokenValidationParameters.RoleClaimType = "roles";
                // ........................................................................

                // ........................................................................
                // Many OIDC providers work with the default issuer validator, but the
                // configuration must account for the issuer parameterized with "{TENANT ID}"
                // returned by the "common" endpoint's /.well-known/openid-configuration
                // For more information, see
                // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1731

                //var microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
                //oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;
                // ........................................................................

                // ........................................................................
                // OIDC connect options set later via ConfigureCookieOidcRefresh
                //
                // (1) The "offline_access" scope is required for the refresh token.
                //
                // (2) SaveTokens is set to true, which saves the access and refresh tokens
                // in the cookie, so the app can authenticate requests and
                // cookie, so the app can authenticate requests and
                // use the refresh token to obtain a new access token on access token
                // expiration.
                // ........................................................................

                // Enrich claims with ApplicationRole from backend during authentication
                oidcOptions.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("OnTokenValidated event fired!");

                        var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();

                        try
                        {
                            var userId = context.Principal?.FindFirst("oid")?.Value
                                        ?? context.Principal?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                                        ?? context.Principal?.FindFirst("sub")?.Value;

                            logger.LogInformation("Looking for user ID in claims, found: {UserId}", userId ?? "NULL");

                            if (string.IsNullOrEmpty(userId))
                            {
                                logger.LogWarning("Cannot enrich claims: User ID not found in token");
                                foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
                                {
                                    logger.LogInformation("Available claim: {Type} = {Value}", claim.Type, claim.Value);
                                }
                                return;
                            }

                            var accessToken = context.TokenEndpointResponse?.AccessToken;
                            logger.LogInformation("Access token from TokenEndpointResponse: {HasToken}", !string.IsNullOrEmpty(accessToken));

                            if (string.IsNullOrEmpty(accessToken))
                            {
                                logger.LogWarning("Cannot enrich claims: Access token not available");
                                return;
                            }

                            logger.LogInformation("Calling QueryClient to get role...");
                            // Create a new HttpClient without the BearerTokenHandler since we're setting the token manually
                            // The BearerTokenHandler won't work here because the token hasn't been saved to the auth properties yet
                            var client = new HttpClient();
                            var queryApiUri = builder.Configuration.GetValue<string>("services:QueryApi:https:0");
                            client.BaseAddress = new Uri(queryApiUri!);
                            client.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                            var response = await client.GetAsync("q/api/v1/auth/me/role");
                            logger.LogInformation("API response status: {StatusCode}", response.StatusCode);

                            if (!response.IsSuccessStatusCode)
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                logger.LogWarning("Failed to fetch ApplicationRole: {StatusCode}, Body: {Body}", response.StatusCode, errorContent);
                                return;
                            }

                            var json = await response.Content.ReadAsStringAsync();
                            logger.LogInformation("API response JSON: {Json}", json);

                            var roleData = System.Text.Json.JsonDocument.Parse(json);

                            if (!roleData.RootElement.TryGetProperty("ApplicationRole", out var roleProperty) ||
                                !roleData.RootElement.TryGetProperty("EmployeeId", out var employeeIdProperty))
                            {
                                logger.LogWarning("ApplicationRole or EmployeeId not found in response");
                                return;
                            }

                            var applicationRole = roleProperty.GetInt32();
                            var roleName = ((Authorization.ApplicationRole)applicationRole).ToString();
                            var employeeId = employeeIdProperty.GetGuid();

                            logger.LogInformation("Adding claims: ApplicationRole={Role}, EmployeeId={EmployeeId}", roleName, employeeId);

                            // Create new claims to add
                            var claims = new List<System.Security.Claims.Claim>
                            {
                                new System.Security.Claims.Claim("ApplicationRole", roleName),
                                new System.Security.Claims.Claim("EmployeeId", employeeId.ToString()),
                                // Add role claim with type "roles" to match RoleClaimType configuration
                                new System.Security.Claims.Claim("roles", roleName),
                                // Also add standard ClaimTypes.Role for compatibility
                                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleName)
                            };

                            // Add the claims to the existing identity
                            var identity = (System.Security.Claims.ClaimsIdentity)context.Principal!.Identity!;
                            identity.AddClaims(claims);

                            // Create a new ClaimsPrincipal with the enriched identity to ensure it's used
                            context.Principal = new System.Security.Claims.ClaimsPrincipal(identity);

                            logger.LogInformation("Successfully enriched user claims with ApplicationRole: {Role} for user {UserId}. Claims in identity: {Count}",
                                roleName, userId, identity.Claims.Count());

                            // Log all claims to verify
                            logger.LogInformation("All claims after enrichment:");
                            foreach (var claim in context.Principal.Claims)
                            {
                                logger.LogInformation("  Claim: {Type} = {Value}", claim.Type, claim.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error enriching user claims with ApplicationRole");
                        }
                    }
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        // ConfigureCookieOidcRefresh attaches a cookie OnValidatePrincipal callback to get
        // a new access token when the current one expires, and reissue a cookie with the
        // new access token saved inside. If the refresh fails, the user will be signed
        // out. OIDC connect options are set for saving tokens and the offline access
        // scope.
        builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

        // Note: Claims are enriched during OIDC OnTokenValidated event (see AddOpenIdConnect configuration)
        // Claims transformation doesn't work with WebAssembly rendering mode

        // Configure authorization with role-based policies (shared configuration)
        builder.Services.AddAuthorization(options =>
        {
            options.ConfigureAuthorizationPolicies();
        });

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddRadzenComponents();

        // Add theme services for Radzen theme switching
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddScoped<LocalStorageThemeService>();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization(options =>
            {
                // Include custom claims in serialization
                options.SerializeAllClaims = true;
            });

        // Note: Claims are enriched during OIDC authentication via OnTokenValidated event
        // No need for custom AuthenticationStateProvider

        builder.Services.AddHttpContextAccessor();

        // Register the bearer token handler
        builder.Services.AddScoped<BearerTokenHandler>();

        builder.Services.AddHttpClient("CommandClient", httpClient =>
        {
            var uri = builder.Configuration.GetValue<string>("services:CommandApi:https:0");
            if (uri is null)
            {
                throw new Exception("Command-API URI not found");
            }
            httpClient.BaseAddress = new Uri(uri);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        builder.Services.AddHttpClient("QueryClient", httpClient =>
        {
            var uri = builder.Configuration.GetValue<string>("services:QueryApi:https:0");
            if (uri is null)
            {
                throw new Exception("Query-API URI not found");
            }
            httpClient.BaseAddress = new Uri(uri);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        builder.Services.AddQuestionnaireServices();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ICategoryApiService, CategoryApiService>();
        builder.Services.AddScoped<IEmployeeApiService, EmployeeApiService>();
        builder.Services.AddScoped<IOrganizationApiService, OrganizationApiService>();
        builder.Services.AddScoped<IEmployeeQuestionnaireService, EmployeeQuestionnaireService>();
        builder.Services.AddScoped<IManagerQuestionnaireService, ManagerQuestionnaireService>();
        builder.Services.AddScoped<IHRQuestionnaireService, HRQuestionnaireService>();
        builder.Services.AddScoped<IHRApiService, HRApiService>();
        builder.Services.AddScoped<IProjectionReplayApiService, ProjectionReplayApiService>();
        builder.Services.AddScoped<IGoalApiService, GoalApiService>();

        // Register refactoring services
        builder.Services.AddScoped<QuestionConfigurationService>();
        builder.Services.AddScoped<QuestionnaireValidationService>();
        builder.Services.AddScoped<GoalService>();

        // Register export services
        builder.Services.AddScoped<IQuestionnaireReportExporter, QuestionnaireReportExporter>();

        // Register question type handlers (Strategy Pattern)
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.AssessmentQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.TextQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.GoalQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.QuestionHandlerFactory>();

        // Register state management
        builder.Services.AddScoped<QuestionnaireBuilderState>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        // Map API proxies for WebAssembly client
        // These use the configured HttpClients which include BearerTokenHandler for authentication
        app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/q"), appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("QueryClient");

                var targetUri = new Uri(httpClient.BaseAddress!, context.Request.Path.ToString() + context.Request.QueryString);

                HttpResponseMessage response;
                if (HttpMethods.IsGet(context.Request.Method))
                {
                    response = await httpClient.GetAsync(targetUri);
                }
                else if (HttpMethods.IsPost(context.Request.Method))
                {
                    var content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                    {
                        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    response = await httpClient.PostAsync(targetUri, content);
                }
                else if (HttpMethods.IsPut(context.Request.Method))
                {
                    var content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                    {
                        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    response = await httpClient.PutAsync(targetUri, content);
                }
                else if (HttpMethods.IsDelete(context.Request.Method))
                {
                    response = await httpClient.DeleteAsync(targetUri);
                }
                else if (HttpMethods.IsPatch(context.Request.Method))
                {
                    var content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                    {
                        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    response = await httpClient.PatchAsync(targetUri, content);
                }
                else
                {
                    context.Response.StatusCode = 405;
                    return;
                }

                context.Response.StatusCode = (int)response.StatusCode;
                foreach (var header in response.Headers.Concat(response.Content.Headers))
                {
                    if (!header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                // Only copy response body if status code allows content
                // 204 No Content, 205 Reset Content, 304 Not Modified should not have a body
                if (response.StatusCode != System.Net.HttpStatusCode.NoContent &&
                    response.StatusCode != System.Net.HttpStatusCode.ResetContent &&
                    response.StatusCode != System.Net.HttpStatusCode.NotModified)
                {
                    await response.Content.CopyToAsync(context.Response.Body);
                }
            });
        });

        app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/c"), appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("CommandClient");

                var targetUri = new Uri(httpClient.BaseAddress!, context.Request.Path.ToString() + context.Request.QueryString);

                HttpResponseMessage response;
                if (HttpMethods.IsGet(context.Request.Method))
                {
                    response = await httpClient.GetAsync(targetUri);
                }
                else if (HttpMethods.IsPost(context.Request.Method))
                {
                    var content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                    {
                        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    response = await httpClient.PostAsync(targetUri, content);
                }
                else if (HttpMethods.IsPut(context.Request.Method))
                {
                    var content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                    {
                        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    response = await httpClient.PutAsync(targetUri, content);
                }
                else if (HttpMethods.IsDelete(context.Request.Method))
                {
                    response = await httpClient.DeleteAsync(targetUri);
                }
                else if (HttpMethods.IsPatch(context.Request.Method))
                {
                    var content = new StreamContent(context.Request.Body);
                    if (context.Request.ContentType != null)
                    {
                        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    response = await httpClient.PatchAsync(targetUri, content);
                }
                else
                {
                    context.Response.StatusCode = 405;
                    return;
                }

                context.Response.StatusCode = (int)response.StatusCode;
                foreach (var header in response.Headers.Concat(response.Content.Headers))
                {
                    if (!header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                }

                // Only copy response body if status code allows content
                // 204 No Content, 205 Reset Content, 304 Not Modified should not have a body
                if (response.StatusCode != System.Net.HttpStatusCode.NoContent &&
                    response.StatusCode != System.Net.HttpStatusCode.ResetContent &&
                    response.StatusCode != System.Net.HttpStatusCode.NotModified)
                {
                    await response.Content.CopyToAsync(context.Response.Body);
                }
            });
        });

        // Add middleware to enrich user claims with ApplicationRole from backend
        //app.UseMiddleware<Authorization.ApplicationRoleClaimsMiddleware>();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.UseAntiforgery();

        app.MapDefaultEndpoints();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.MapGroup("/authentication").MapLoginAndLogout();

        app.Run();
    }
}
