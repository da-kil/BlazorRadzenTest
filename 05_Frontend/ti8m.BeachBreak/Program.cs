using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Radzen;
using ti8m.BeachBreak.Authentication;
using ti8m.BeachBreak.Authorization;
using ti8m.BeachBreak.Client.Services;
using ti8m.BeachBreak.Components;

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

                oidcOptions.Authority = $"{azureEntraSettings.Instance}/{azureEntraSettings.TenantId}/v2.0/";
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
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        // ConfigureCookieOidcRefresh attaches a cookie OnValidatePrincipal callback to get
        // a new access token when the current one expires, and reissue a cookie with the
        // new access token saved inside. If the refresh fails, the user will be signed
        // out. OIDC connect options are set for saving tokens and the offline access
        // scope.
        builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

        // Configure authorization with role-based policies (matching backend hierarchy)
        builder.Services.AddAuthorization(options =>
        {
            // Employee policy: All authenticated employees can access (Employee, TeamLead, HR, HRLead, Admin)
            options.AddPolicy("Employee", policy => policy.RequireRole("Employee", "TeamLead", "HR", "HRLead", "Admin"));

            // TeamLead policy: TeamLead and above can access (TeamLead, HR, HRLead, Admin)
            options.AddPolicy("TeamLead", policy => policy.RequireRole("TeamLead", "HR", "HRLead", "Admin"));

            // HR policy: HR and above can access (HR, HRLead, Admin)
            options.AddPolicy("HR", policy => policy.RequireRole("HR", "HRLead", "Admin"));

            // HRLead policy: HRLead and above can access (HRLead, Admin)
            options.AddPolicy("HRLead", policy => policy.RequireRole("HRLead", "Admin"));

            // Admin policy: Only Admin can access
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        });

        // Register custom AuthenticationStateProvider that enriches claims with ApplicationRole
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddRadzenComponents();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

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
        builder.Services.AddScoped<ICategoryApiService, CategoryApiService>();
        builder.Services.AddScoped<IEmployeeApiService, EmployeeApiService>();
        builder.Services.AddScoped<IOrganizationApiService, OrganizationApiService>();
        builder.Services.AddScoped<IEmployeeQuestionnaireService, EmployeeQuestionnaireService>();
        builder.Services.AddScoped<IManagerQuestionnaireService, ManagerQuestionnaireService>();
        builder.Services.AddScoped<IHRQuestionnaireService, HRQuestionnaireService>();

        // Register refactoring services
        builder.Services.AddScoped<QuestionConfigurationService>();
        builder.Services.AddScoped<QuestionnaireValidationService>();

        // Register question type handlers (Strategy Pattern)
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.SelfAssessmentQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.GoalAchievementQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.TextQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.QuestionHandlerFactory>();

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
