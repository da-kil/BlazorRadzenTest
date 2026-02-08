using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

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
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> tokens = new();

        /// <summary>
        /// Debounces an async operation with a specified delay
        /// </summary>
        public static async Task DebounceAsync(string key, Func<Task> operation, int delayMs = 500)
        {
            var newTokenSource = new CancellationTokenSource();
            CancellationTokenSource? canceledToken = null;

            // Atomic swap with cleanup of old token
            tokens.AddOrUpdate(
                key,
                newTokenSource, // Add if not exists
                (k, existingToken) => // Update if exists
                {
                    canceledToken = existingToken; // Capture for cleanup
                    return newTokenSource;
                }
            );

            // Cleanup old token if it existed
            if (canceledToken != null)
            {
                canceledToken.Cancel();
                canceledToken.Dispose();
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
                // Remove our token if it's still the current one (atomic conditional removal)
                tokens.TryRemove(new KeyValuePair<string, CancellationTokenSource>(key, newTokenSource));
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
        private static readonly ConcurrentDictionary<string, DateTime> lastExecutions = new();

        /// <summary>
        /// Throttles an async operation to execute at most once per interval
        /// </summary>
        public static async Task<bool> TryThrottleAsync(string key, Func<Task> operation, int intervalMs = 100)
        {
            DateTime now = DateTime.UtcNow;
            bool shouldExecute = false;

            lastExecutions.AddOrUpdate(
                key,
                addValueFactory: (k) =>
                {
                    shouldExecute = true;
                    return now;
                },
                updateValueFactory: (k, lastExecution) =>
                {
                    shouldExecute = (now - lastExecution).TotalMilliseconds >= intervalMs;
                    return shouldExecute ? now : lastExecution;
                }
            );

            if (shouldExecute)
            {
                await operation();
                return true;
            }

            return false;
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
        private static readonly ConcurrentDictionary<string, PerformanceMetrics> metrics = new();

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
            return metrics.TryGetValue(operationName, out var foundMetrics) ? foundMetrics : null;
        }

        /// <summary>
        /// Gets all performance metrics
        /// </summary>
        public static Dictionary<string, PerformanceMetrics> GetAllMetrics()
        {
            return new Dictionary<string, PerformanceMetrics>(metrics);
        }

        private class PerformanceTracker : IDisposable
        {
            private readonly string operationName;
            private readonly string? componentName;
            private readonly DateTime startTime;
            private bool disposed;

            public PerformanceTracker(string operationName, string? componentName)
            {
                this.operationName = operationName;
                this.componentName = componentName;
                startTime = DateTime.UtcNow;
            }

            public void Dispose()
            {
                if (disposed) return;

                var duration = DateTime.UtcNow - startTime;
                RecordMetrics(operationName, duration, componentName);
                disposed = true;
            }

            private static void RecordMetrics(string operationName, TimeSpan duration, string? componentName)
            {
                metrics.AddOrUpdate(
                    operationName,
                    addValueFactory: (key) =>
                    {
                        var metrics = new PerformanceMetrics(operationName, componentName);
                        metrics.RecordExecution(duration);
                        return metrics;
                    },
                    updateValueFactory: (key, existingMetrics) =>
                    {
                        existingMetrics.RecordExecution(duration);
                        return existingMetrics;
                    }
                );
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
        private int executionCount;
        private long totalDurationTicks;
        private long _minDurationTicks = TimeSpan.MaxValue.Ticks;
        private long _maxDurationTicks = TimeSpan.MinValue.Ticks;
        private DateTime lastExecution;

        public string OperationName { get; }
        public string? ComponentName { get; }
        public int ExecutionCount => executionCount;
        public TimeSpan TotalDuration => TimeSpan.FromTicks(totalDurationTicks);
        public TimeSpan AverageDuration => ExecutionCount > 0 ? TimeSpan.FromTicks(totalDurationTicks / ExecutionCount) : TimeSpan.Zero;
        public TimeSpan MinDuration => TimeSpan.FromTicks(_minDurationTicks);
        public TimeSpan MaxDuration => TimeSpan.FromTicks(_maxDurationTicks);
        public DateTime FirstExecution { get; private set; }
        public DateTime LastExecution => lastExecution;

        public PerformanceMetrics(string operationName, string? componentName)
        {
            OperationName = operationName;
            ComponentName = componentName;
            FirstExecution = DateTime.UtcNow;
            lastExecution = DateTime.UtcNow;
        }

        public void RecordExecution(TimeSpan duration)
        {
            // Atomic increment
            Interlocked.Increment(ref executionCount);

            // Atomic add for total duration
            Interlocked.Add(ref totalDurationTicks, duration.Ticks);

            // Atomic update of last execution
            lastExecution = DateTime.UtcNow;

            // Atomic min/max updates using compare-exchange loops
            UpdateMinDuration(duration.Ticks);
            UpdateMaxDuration(duration.Ticks);
        }

        private void UpdateMinDuration(long durationTicks)
        {
            long currentMin, newMin;
            do
            {
                currentMin = _minDurationTicks;
                newMin = Math.Min(currentMin, durationTicks);
                if (newMin == currentMin) break; // No update needed
            } while (Interlocked.CompareExchange(ref _minDurationTicks, newMin, currentMin) != currentMin);
        }

        private void UpdateMaxDuration(long durationTicks)
        {
            long currentMax, newMax;
            do
            {
                currentMax = _maxDurationTicks;
                newMax = Math.Max(currentMax, durationTicks);
                if (newMax == currentMax) break; // No update needed
            } while (Interlocked.CompareExchange(ref _maxDurationTicks, newMax, currentMax) != currentMax);
        }
    }

    #endregion
}