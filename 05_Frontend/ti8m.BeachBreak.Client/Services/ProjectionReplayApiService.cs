using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services;

public class ProjectionReplayApiService : BaseApiService, IProjectionReplayApiService
{
    private const string QueryEndpoint = "q/api/v1/admin/replay";
    private const string CommandEndpoint = "c/api/v1/admin/replay";

    public ProjectionReplayApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<Guid?> StartReplayAsync(StartProjectionReplayRequestDto request)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync($"{CommandEndpoint}/start", request);

            if (response.IsSuccessStatusCode)
            {
                var replayId = await response.Content.ReadFromJsonAsync<Guid>();
                return replayId;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            LogError($"Failed to start projection replay: {response.StatusCode} - {errorContent}", new Exception(errorContent));
            return null;
        }
        catch (Exception ex)
        {
            LogError("Error starting projection replay", ex);
            return null;
        }
    }

    public async Task<bool> CancelReplayAsync(Guid replayId)
    {
        try
        {
            var response = await HttpCommandClient.PostAsync($"{CommandEndpoint}/{replayId}/cancel", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error cancelling replay {replayId}", ex);
            return false;
        }
    }

    public async Task<ProjectionReplayStatus?> GetReplayStatusAsync(Guid replayId)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<ProjectionReplayStatus>($"{QueryEndpoint}/{replayId}");
        }
        catch (Exception ex)
        {
            LogError($"Error fetching replay status for {replayId}", ex);
            return null;
        }
    }

    public async Task<List<ProjectionReplayStatus>> GetReplayHistoryAsync(int limit = 50)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<ProjectionReplayStatus>>($"{QueryEndpoint}/history?limit={limit}");
            return response ?? new List<ProjectionReplayStatus>();
        }
        catch (Exception ex)
        {
            LogError("Error fetching replay history", ex);
            return new List<ProjectionReplayStatus>();
        }
    }

    public async Task<List<ProjectionInfo>> GetAvailableProjectionsAsync(bool rebuildableOnly = true)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<ProjectionInfo>>($"{QueryEndpoint}/projections?rebuildableOnly={rebuildableOnly}");
            return response ?? new List<ProjectionInfo>();
        }
        catch (Exception ex)
        {
            LogError("Error fetching available projections", ex);
            return new List<ProjectionInfo>();
        }
    }
}
