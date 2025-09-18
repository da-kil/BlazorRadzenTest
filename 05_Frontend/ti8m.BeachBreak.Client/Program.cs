using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddRadzenComponents();
        builder.Services.AddSingleton<ILanguageService, LanguageService>();

        await builder.Build().RunAsync();
    }
}
