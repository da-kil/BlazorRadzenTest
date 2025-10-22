using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services;

public interface IProjectionReplayApiService
{
    Task<Guid?> StartReplayAsync(StartProjectionReplayRequestDto request);
    Task<bool> CancelReplayAsync(Guid replayId);
    Task<ProjectionReplayStatus?> GetReplayStatusAsync(Guid replayId);
    Task<List<ProjectionReplayStatus>> GetReplayHistoryAsync(int limit = 50);
    Task<List<ProjectionInfo>> GetAvailableProjectionsAsync(bool rebuildableOnly = true);
}
