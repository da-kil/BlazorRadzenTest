using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
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

        await builder.Build().RunAsync();
    }
}
