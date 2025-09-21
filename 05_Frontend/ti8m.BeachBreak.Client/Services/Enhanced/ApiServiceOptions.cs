namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Configuration options for enhanced API service behavior
/// </summary>
public class ApiServiceOptions
{
    /// <summary>
    /// Maximum number of retry attempts for failed requests
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Whether to use exponential backoff for retry delays
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// HTTP status codes that should trigger a retry
    /// </summary>
    public HashSet<System.Net.HttpStatusCode> RetryableStatusCodes { get; set; } = new()
    {
        System.Net.HttpStatusCode.RequestTimeout,
        System.Net.HttpStatusCode.TooManyRequests,
        System.Net.HttpStatusCode.InternalServerError,
        System.Net.HttpStatusCode.BadGateway,
        System.Net.HttpStatusCode.ServiceUnavailable,
        System.Net.HttpStatusCode.GatewayTimeout
    };

    /// <summary>
    /// Request timeout in seconds for individual HTTP requests
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to log detailed error information
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
}