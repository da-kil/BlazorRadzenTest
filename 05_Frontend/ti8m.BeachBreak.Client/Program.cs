using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ti8m.BeachBreak.Client.Configuration;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddRadzenComponents();

        // Configure authorization with role-based policies (shared configuration)
        builder.Services.AddAuthorizationCore(options =>
        {
            options.ConfigureAuthorizationPolicies();
        });

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();

        // Configure JSON serialization to use PascalCase (consistent with backend)
        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = null; // PascalCase for both sending and receiving
            options.PropertyNameCaseInsensitive = false; // Enforce strict PascalCase consistency
            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver(); // Enable polymorphic type discrimination
        });

        // Configure HttpClients for API communication
        // In WebAssembly, these will call back to the host server which proxies to the actual APIs
        builder.Services.AddHttpClient("CommandClient", client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        });

        builder.Services.AddHttpClient("QueryClient", client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        });

        // Register questionnaire services
        builder.Services.AddQuestionnaireServices();

        // Register role-specific questionnaire services
        builder.Services.AddScoped<IEmployeeQuestionnaireService, EmployeeQuestionnaireService>();
        builder.Services.AddScoped<IManagerQuestionnaireService, ManagerQuestionnaireService>();
        builder.Services.AddScoped<IHRQuestionnaireService, HRQuestionnaireService>();

        // Register other API services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ICategoryApiService, CategoryApiService>();
        builder.Services.AddScoped<IEmployeeApiService, EmployeeApiService>();
        builder.Services.AddScoped<IOrganizationApiService, OrganizationApiService>();
        builder.Services.AddScoped<IHRApiService, HRApiService>();
        builder.Services.AddScoped<IProjectionReplayApiService, ProjectionReplayApiService>();
        builder.Services.AddScoped<IGoalApiService, GoalApiService>();

        // Register refactoring services
        builder.Services.AddScoped<QuestionConfigurationService>();
        builder.Services.AddScoped<QuestionnaireValidationService>();
        builder.Services.AddScoped<GoalService>();

        // Register question type handlers (Strategy Pattern)
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.AssessmentQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.TextQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.GoalQuestionHandler>();
        builder.Services.AddScoped<ti8m.BeachBreak.Client.Services.QuestionHandlers.QuestionHandlerFactory>();

        // Register state management
        builder.Services.AddScoped<QuestionnaireBuilderState>();

        await builder.Build().RunAsync();
    }
}
