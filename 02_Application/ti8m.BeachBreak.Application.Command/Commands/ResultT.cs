using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Application.Command.Commands;

public class Result<TPayload>
{
    private readonly TPayload? _payload;

    // For JSON serialization - doesn't throw, returns nullable
    public TPayload? Payload => _payload;

    // For programmatic access - throws if not succeeded
    [JsonIgnore]
    public TPayload PayloadOrThrow
    {
        get
        {
            if (!Succeeded)
                throw new InvalidOperationException($"Cannot access Payload of a failed result. Message: {Message}");
            return _payload!;
        }
    }

    public string? Message { get; }
    public bool Succeeded { get; }
    public int StatusCode { get; }

    public Result(TPayload payload, string message, int statusCode, bool succeeded)
    {
        _payload = payload;
        Message = message;
        StatusCode = statusCode;
        Succeeded = succeeded;
    }

    public Result(TPayload payload, bool succeeded, int statusCode)
    {
        _payload = payload;
        Succeeded = succeeded;
        StatusCode = statusCode;
    }

    public static Result<TPayload> Success(TPayload payload)
    {
        return new Result<TPayload>(payload, null, StatusCodes.Status200OK, true);
    }

    public static Result<TPayload> Success(TPayload payload, int statusCode)
    {
        return new Result<TPayload>(payload, null, statusCode, true);
    }

    public static Result<TPayload> Fail(string message, int statusCode)
    {
        return new Result<TPayload>(default!, message, statusCode, false);
    }
}