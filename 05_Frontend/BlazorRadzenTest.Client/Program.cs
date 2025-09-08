using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using BlazorRadzenTest.Client.Services;

namespace BlazorRadzenTest.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddRadzenComponents();
            
            // Add HTTP client and API service
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IQuestionnaireApiService, QuestionnaireApiService>();

            await builder.Build().RunAsync();
        }
    }
}
