using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddRadzenComponents();
            
            builder.Services.AddHttpClient("ApiClient", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });

            await builder.Build().RunAsync();
        }
    }
}
