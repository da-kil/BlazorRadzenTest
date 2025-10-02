using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace ti8m.BeachBreak.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddRadzenComponents();

        // Add authentication state management for WebAssembly
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();

        await builder.Build().RunAsync();
    }
}
