using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using ti8m.BeachBreak.Client.Extensions;

namespace ti8m.BeachBreak.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Add Radzen components
        builder.Services.AddRadzenComponents();

        // Add custom services
        builder.Services.AddQuestionnaireServices();
        builder.Services.AddAuthenticationServices();
        builder.Services.AddLoggingServices();

        await builder.Build().RunAsync();
    }
}
