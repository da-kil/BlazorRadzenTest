using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services;

public interface IHRApiService
{
    Task<HRDashboardDto?> GetHRDashboardAsync();
}

public class HRApiService : BaseApiService, IHRApiService
{
    private const string HREndpoint = "q/api/v1/hr";

    public HRApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<HRDashboardDto?> GetHRDashboardAsync()
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<HRDashboardDto>($"{HREndpoint}/dashboard");
        }
        catch (Exception ex)
        {
            LogError("Error retrieving HR dashboard", ex);
            return null;
        }
    }
}
