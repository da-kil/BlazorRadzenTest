using Radzen;
using ti8m.BeachBreak.Client.Services;
using ti8m.BeachBreak.Services;
using ti8m.BeachBreak.Components;
using Yarp.ReverseProxy.Transforms;

namespace ti8m.BeachBreak;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddRadzenComponents();

        builder.Services.AddHttpClient("CommandClient", httpClient =>
        {
            var uri = builder.Configuration.GetValue<string>("services:CommandApi:https:0");
            if (uri is null)
            {
                throw new Exception("Command-API URI not found");
            }

            httpClient.BaseAddress = new Uri(uri);
        });

        builder.Services.AddHttpClient("QueryClient", httpClient =>
        {
            var uri = builder.Configuration.GetValue<string>("services:QueryApi:https:0");
            if (uri is null)
            {
                throw new Exception("Query-API URI not found");
            }

            httpClient.BaseAddress = new Uri(uri);
        });

        builder.Services.AddQuestionnaireServices();
        builder.Services.AddScoped<ICategoryApiService, CategoryApiService>();
        builder.Services.AddScoped<IEmployeeApiService, EmployeeApiService>();
        builder.Services.AddScoped<IOrganizationApiService, OrganizationApiService>();
        builder.Services.AddScoped<IEmployeeQuestionnaireService, EmployeeQuestionnaireService>();
        builder.Services.AddScoped<IManagerQuestionnaireService, ManagerQuestionnaireService>();
        builder.Services.AddScoped<IHRQuestionnaireService, HRQuestionnaireService>();
        builder.Services.AddScoped<IAuthenticationService, Services.FakeAuthenticationService>();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddHttpForwarderWithServiceDiscovery();

        var app = builder.Build();

        app.MapDefaultEndpoints();

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

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.MapForwarder("/q/{**catch-all}", "https://QueryApi", transformBuilder =>
        {
            transformBuilder.AddRequestTransform(async transformContext =>
            {
                //var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
                //transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
            });
        }).RequireAuthorization();

        app.MapForwarder("c/{**catch-all}", "https://CommandApi", transformBuilder =>
        {
            transformBuilder.AddRequestTransform(async transformContext =>
            {
                //var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
                //transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
            });
        }).RequireAuthorization();

        app.Run();
    }
}
