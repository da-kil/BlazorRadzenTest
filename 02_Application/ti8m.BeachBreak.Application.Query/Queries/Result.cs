using Microsoft.AspNetCore.Http;

namespace ti8m.BeachBreak.Application.Query.Queries;

public class Result
{
    public string? Message { get; }
    public bool Succeeded { get; }
    public int StatusCode { get; }

    protected Result(string message, int statusCode, bool succeeded)
    {
        Message = message;
        StatusCode = statusCode;
        Succeeded = succeeded;
    }

    public static Result Success(string? message = null)
    {
        return new Result(message ?? string.Empty, StatusCodes.Status200OK, true);
    }

    public static Result Fail(string message, int statusCode)
    {
        return new Result(message, statusCode, false);
    }
}

public class Result<TPayload> : Result
{
    public TPayload? Payload { get; }

    private Result(TPayload payload, string message, int statusCode, bool succeeded)
        : base(message, statusCode, succeeded)
    {
        Payload = payload;
    }

    private Result(TPayload payload, bool succeeded, int statusCode)
        : base(string.Empty, statusCode, succeeded)
    {
        Payload = payload;
    }

    public static Result<TPayload> Success(TPayload payload)
    {
        return new Result<TPayload>(payload, true, StatusCodes.Status200OK);
    }

    public static Result<TPayload> Success(TPayload payload, int statusCode)
    {
        return new Result<TPayload>(payload, true, statusCode);
    }

    public new static Result<TPayload> Fail(string message, int statusCode)
    {
        return new Result<TPayload>(default!, message, statusCode, false);
    }
}
