namespace ti8m.BeachBreak.Core.Domain.BuildingBlocks;

/// <summary>
/// Represents the result of a domain operation.
/// Used for operations that may fail due to business rule violations.
/// </summary>
public class DomainResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public int? StatusCode { get; }

    private DomainResult(bool isSuccess, string? errorMessage = null, int? statusCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static DomainResult Success() => new(true);

    public static DomainResult Failure(string errorMessage, int statusCode = 400) =>
        new(false, errorMessage, statusCode);
}
