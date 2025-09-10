using Microsoft.AspNetCore.Http;

namespace ti8m.BeachBreak.Application.Command.Commands;

public class Result
{
    public string? Message { get; }
    public bool Succeeded { get; }
    public int StatusCode { get; }

    public Result(string message, int statusCode, bool succeeded)
    {
        Message = message;
        StatusCode = statusCode;
        Succeeded = succeeded;
    }

    public Result(bool succeeded, int statusCode)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
    }

    public static Result Success()
    {
        return new Result(true, StatusCodes.Status200OK);
    }

    public static Result Success(string message)
    {
        return new Result(message, StatusCodes.Status200OK, true);
    }

    public static Result Success(int statusCode)
    {
        return new Result(true, statusCode);
    }

    public static Result Fail(string message, int statusCode)
    {
        return new Result(message, statusCode, false);
    }
}
