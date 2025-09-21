using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Performance optimization utilities for Blazor components
/// </summary>
public static class PerformanceOptimizer
{
    /// <summary>
    /// Debounces function calls to reduce unnecessary operations
    /// Useful for search inputs, auto-save operations, etc.
    /// </summary>
    public static class Debouncer
    {
        private static readonly Dictionary<string, CancellationTokenSource> _tokens = new();
        private static readonly object _lock = new object();

        /// <summary>
        /// Debounces an async operation with a specified delay
        /// </summary>
        public static async Task DebounceAsync(string key, Func<Task> operation, int delayMs = 500)
        {
            CancellationTokenSource newTokenSource;

            lock (_lock)
            {
                // Cancel any existing operation for this key
                if (_tokens.TryGetValue(key, out var existingToken))
                {
                    existingToken.Cancel();
                    existingToken.Dispose();
                }

                // Create new token source
                newTokenSource = new CancellationTokenSource();
                _tokens[key] = newTokenSource;
            }

            try
            {
                // Wait for the delay period
                await Task.Delay(delayMs, newTokenSource.Token);

                // If we haven't been cancelled, execute the operation
                if (!newTokenSource.Token.IsCancellationRequested)
                {
                    await operation();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when debounced operation is cancelled
            }
            finally
            {
                lock (_lock)
                {
                    if (_tokens.TryGetValue(key, out var tokenSource) && tokenSource == newTokenSource)
                    {
                        _tokens.Remove(key);
                    }
                }
                newTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Debounces a synchronous operation
        /// </summary>
        public static async Task DebounceAsync(string key, Action operation, int delayMs = 500)
        {
            await DebounceAsync(key, () =>
            {
                operation();
                return Task.CompletedTask;
            }, delayMs);
        }
    }

    /// <summary>
    /// Throttles function calls to limit execution frequency
    /// Useful for scroll events, resize events, etc.
    /// </summary>
    public static class Throttler
    {
        private static readonly Dictionary<string, DateTime> _lastExecutions = new();
        private static readonly object _lock = new object();

        /// <summary>
        /// Throttles an async operation to execute at most once per interval
        /// </summary>
        public static async Task<bool> TryThrottleAsync(string key, Func<Task> operation, int intervalMs = 100)
        {
            DateTime now = DateTime.UtcNow;
            bool shouldExecute;

            lock (_lock)
            {
                if (_lastExecutions.TryGetValue(key, out var lastExecution))
                {
                    shouldExecute = (now - lastExecution).TotalMilliseconds >= intervalMs;
                }
                else
                {
                    shouldExecute = true;
                }

                if (shouldExecute)
                {
                    _lastExecutions[key] = now;
                }
            }

            if (shouldExecute)
            {
                await operation();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throttles a synchronous operation
        /// </summary>
        public static bool TryThrottle(string key, Action operation, int intervalMs = 100)
        {
            var task = TryThrottleAsync(key, () =>
            {
                operation();
                return Task.CompletedTask;
            }, intervalMs);

            return task.GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Lazy loading utilities for large datasets
    /// </summary>
    public static class LazyLoader
    {
        /// <summary>
        /// Loads data in batches with pagination support
        /// </summary>
        public static async Task<PagedResult<T>> LoadPagedDataAsync<T>(
            Func<int, int, Task<List<T>>> dataLoader,
            int pageNumber,
            int pageSize,
            Func<Task<int>>? totalCountLoader = null)
        {
            try
            {
                var data = await dataLoader(pageNumber, pageSize);
                var totalCount = totalCountLoader != null ? await totalCountLoader() : data.Count;

                return new PagedResult<T>
                {
                    Data = data,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = pageNumber * pageSize < totalCount,
                    HasPreviousPage = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                return new PagedResult<T>
                {
                    Data = new List<T>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Loads data with virtual scrolling support
        /// </summary>
        public static async Task<VirtualScrollResult<T>> LoadVirtualScrollDataAsync<T>(
            Func<int, int, Task<List<T>>> dataLoader,
            int startIndex,
            int itemCount,
            int bufferSize = 10)
        {
            try
            {
                // Load with buffer for smooth scrolling
                var actualStartIndex = Math.Max(0, startIndex - bufferSize);
                var actualItemCount = itemCount + (2 * bufferSize);

                var data = await dataLoader(actualStartIndex, actualItemCount);

                return new VirtualScrollResult<T>
                {
                    Data = data,
                    StartIndex = actualStartIndex,
                    RequestedStartIndex = startIndex,
                    ItemCount = data.Count,
                    BufferSize = bufferSize
                };
            }
            catch (Exception ex)
            {
                return new VirtualScrollResult<T>
                {
                    Data = new List<T>(),
                    StartIndex = startIndex,
                    RequestedStartIndex = startIndex,
                    Error = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// Component performance monitoring utilities
    /// </summary>
    public static class PerformanceMonitor
    {
        private static readonly Dictionary<string, PerformanceMetrics> _metrics = new();
        private static readonly object _lock = new object();

        /// <summary>
        /// Starts performance monitoring for a component operation
        /// </summary>
        public static IDisposable StartOperation(string operationName, string? componentName = null)
        {
            return new PerformanceTracker(operationName, componentName);
        }

        /// <summary>
        /// Gets performance metrics for an operation
        /// </summary>
        public static PerformanceMetrics? GetMetrics(string operationName)
        {
            lock (_lock)
            {
                return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
            }
        }

        /// <summary>
        /// Gets all performance metrics
        /// </summary>
        public static Dictionary<string, PerformanceMetrics> GetAllMetrics()
        {
            lock (_lock)
            {
                return new Dictionary<string, PerformanceMetrics>(_metrics);
            }
        }

        private class PerformanceTracker : IDisposable
        {
            private readonly string _operationName;
            private readonly string? _componentName;
            private readonly DateTime _startTime;
            private bool _disposed;

            public PerformanceTracker(string operationName, string? componentName)
            {
                _operationName = operationName;
                _componentName = componentName;
                _startTime = DateTime.UtcNow;
            }

            public void Dispose()
            {
                if (_disposed) return;

                var duration = DateTime.UtcNow - _startTime;
                RecordMetrics(_operationName, duration, _componentName);
                _disposed = true;
            }

            private static void RecordMetrics(string operationName, TimeSpan duration, string? componentName)
            {
                lock (_lock)
                {
                    if (!_metrics.TryGetValue(operationName, out var metrics))
                    {
                        metrics = new PerformanceMetrics(operationName, componentName);
                        _metrics[operationName] = metrics;
                    }

                    metrics.RecordExecution(duration);
                }
            }
        }
    }

    #region Supporting Types

    /// <summary>
    /// Result of paged data loading
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string? Error { get; set; }
        public bool IsSuccess => string.IsNullOrEmpty(Error);
    }

    /// <summary>
    /// Result of virtual scroll data loading
    /// </summary>
    public class VirtualScrollResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int StartIndex { get; set; }
        public int RequestedStartIndex { get; set; }
        public int ItemCount { get; set; }
        public int BufferSize { get; set; }
        public string? Error { get; set; }
        public bool IsSuccess => string.IsNullOrEmpty(Error);
    }

    /// <summary>
    /// Performance metrics for component operations
    /// </summary>
    public class PerformanceMetrics
    {
        public string OperationName { get; }
        public string? ComponentName { get; }
        public int ExecutionCount { get; private set; }
        public TimeSpan TotalDuration { get; private set; }
        public TimeSpan AverageDuration => ExecutionCount > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / ExecutionCount) : TimeSpan.Zero;
        public TimeSpan MinDuration { get; private set; } = TimeSpan.MaxValue;
        public TimeSpan MaxDuration { get; private set; } = TimeSpan.MinValue;
        public DateTime FirstExecution { get; private set; }
        public DateTime LastExecution { get; private set; }

        public PerformanceMetrics(string operationName, string? componentName)
        {
            OperationName = operationName;
            ComponentName = componentName;
            FirstExecution = DateTime.UtcNow;
        }

        public void RecordExecution(TimeSpan duration)
        {
            ExecutionCount++;
            TotalDuration = TotalDuration.Add(duration);
            LastExecution = DateTime.UtcNow;

            if (duration < MinDuration)
                MinDuration = duration;

            if (duration > MaxDuration)
                MaxDuration = duration;
        }
    }

    #endregion
}