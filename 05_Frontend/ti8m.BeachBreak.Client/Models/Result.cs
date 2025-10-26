namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Represents the result of an operation without a payload.
/// </summary>
public class Result
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public int StatusCode { get; init; }

    // Compatibility property for existing code using ErrorMessage
    public string? ErrorMessage => Message;

    protected Result(bool succeeded, string? message = null, int statusCode = 200)
    {
        Succeeded = succeeded;
        Message = message;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true);

    public static Result Fail(string message, int statusCode = 400) => new(false, message, statusCode);
}

/// <summary>
/// Represents the result of an operation with a payload.
/// </summary>
public class Result<T> : Result
{
    public T? Payload { get; init; }

    private Result(bool succeeded, T? payload = default, string? message = null, int statusCode = 200)
        : base(succeeded, message, statusCode)
    {
        Payload = payload;
    }

    public static Result<T> Success(T payload) => new(true, payload);

    public static new Result<T> Fail(string message, int statusCode = 400) => new(false, default, message, statusCode);
}
