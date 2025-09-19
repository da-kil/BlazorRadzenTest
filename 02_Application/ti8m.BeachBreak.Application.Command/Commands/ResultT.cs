using Microsoft.AspNetCore.Http;

namespace ti8m.BeachBreak.Application.Command.Commands;

public class Result<TPayload>
{
    public TPayload? Payload { get; }
    public string? Message { get; }
    public bool Succeeded { get; }
    public int StatusCode { get; }

    public Result(TPayload payload, string message, int statusCode, bool succeeded)
    {
        Payload = payload;
        Message = message;
        StatusCode = statusCode;
        Succeeded = succeeded;
    }

    public Result(TPayload payload, bool succeeded, int statusCode)
    {
        Payload = payload;
        Succeeded = succeeded;
        StatusCode = statusCode;
    }

    public static Result<TPayload> Success(TPayload payload)
    {
        return new Result<TPayload>(payload, true, StatusCodes.Status200OK);
    }

    public static Result<TPayload> Success(TPayload payload, int statusCode)
    {
        return new Result<TPayload>(payload, true, statusCode);
    }

    public static Result<TPayload> Fail(string message, int statusCode)
    {
        return new Result<TPayload>(default!, message, statusCode, false);
    }
}