namespace ti8m.BeachBreak.Core.Infrastructure.Contexts;

/// <summary>
/// Extension methods for UserContext to simplify user ID extraction and validation.
/// Eliminates duplicate user ID parsing logic across controllers.
/// </summary>
public static class UserContextExtensions
{
    /// <summary>
    /// Attempts to parse the user ID from the context.
    /// </summary>
    /// <param name="userContext">The user context</param>
    /// <param name="userId">The parsed user ID if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryGetUserId(this UserContext userContext, out Guid userId)
    {
        userId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(userContext.Id))
            return false;

        return Guid.TryParse(userContext.Id, out userId);
    }

    /// <summary>
    /// Gets the user ID from the context, or null if not available or invalid.
    /// </summary>
    /// <param name="userContext">The user context</param>
    /// <returns>The user ID, or null if not available or invalid</returns>
    public static Guid? GetUserIdOrDefault(this UserContext userContext)
    {
        return TryGetUserId(userContext, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the user ID from the context, throwing an exception if not available or invalid.
    /// </summary>
    /// <param name="userContext">The user context</param>
    /// <returns>The user ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID is not available or invalid</exception>
    public static Guid GetUserIdOrThrow(this UserContext userContext)
    {
        if (!TryGetUserId(userContext, out var userId))
        {
            throw new UnauthorizedAccessException(
                string.IsNullOrWhiteSpace(userContext.Id)
                    ? "User ID is not available in context"
                    : $"User ID '{userContext.Id}' is not a valid GUID");
        }

        return userId;
    }

    /// <summary>
    /// Gets the user ID from the context with a descriptive error message if parsing fails.
    /// Useful for returning error responses from controllers.
    /// </summary>
    /// <param name="userContext">The user context</param>
    /// <param name="userId">The parsed user ID if successful</param>
    /// <param name="errorMessage">The error message if parsing failed</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryGetUserId(this UserContext userContext, out Guid userId, out string errorMessage)
    {
        userId = Guid.Empty;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(userContext.Id))
        {
            errorMessage = "User ID is not available in context";
            return false;
        }

        if (!Guid.TryParse(userContext.Id, out userId))
        {
            errorMessage = $"User ID '{userContext.Id}' is not a valid GUID";
            return false;
        }

        return true;
    }
}
