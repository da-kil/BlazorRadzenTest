using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Core.Infrastructure;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(
    EventId = 4001,
    Level = LogLevel.Information,
    Message = "Loading events for aggregate `{Id}`.")]
    public static partial void LogLoadEventStream(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 4002,
    Level = LogLevel.Information,
    Message = "Saving events (count: {NumberOfEvents}) for aggregate `{Id}`.")]
    public static partial void LogSaveEventStream(this ILogger logger, long numberOfEvents, Guid id);
}