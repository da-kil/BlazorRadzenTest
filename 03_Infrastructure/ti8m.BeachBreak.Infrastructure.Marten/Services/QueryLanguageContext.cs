using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Query-side implementation of language context service using Marten query sessions.
/// Uses integer language codes to maintain Clean Architecture (avoids Domain dependencies in Core.Infrastructure).
/// This implementation is read-only and cannot set language preferences - use CommandApi for that.
/// </summary>
public class QueryLanguageContext : ILanguageContext
{
    private readonly IQuerySession querySession;
    private readonly UserContext userContext;
    private readonly ILogger<QueryLanguageContext> logger;

    public QueryLanguageContext(
        IQuerySession querySession,
        UserContext userContext,
        ILogger<QueryLanguageContext> logger)
    {
        this.querySession = querySession;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<int> GetCurrentLanguageCodeAsync()
    {
        if (string.IsNullOrEmpty(userContext.Id))
        {
            logger.LogWarning("No user context available, returning English (0) as default language");
            return 0; // English
        }

        if (!Guid.TryParse(userContext.Id, out var userId))
        {
            logger.LogWarning("Invalid user ID in context: {UserId}, returning English (0) as default language", userContext.Id);
            return 0; // English
        }

        return await GetUserPreferredLanguageCodeAsync(userId);
    }

    public async Task<int> GetUserPreferredLanguageCodeAsync(Guid userId)
    {
        try
        {
            var employee = await querySession.LoadAsync<EmployeeReadModel>(userId);

            if (employee == null)
            {
                logger.LogWarning("Employee not found for ID: {UserId}, returning English (0) as default language", userId);
                return 0; // English
            }

            var languageCode = LanguageMapper.ToLanguageCode(employee.PreferredLanguage);
            logger.LogDebug("Retrieved language preference {Language} (code: {LanguageCode}) for user {UserId}", employee.PreferredLanguage, languageCode, userId);
            return languageCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving language preference for user {UserId}, returning English (0) as default", userId);
            return 0; // English
        }
    }

    public Task SetUserPreferredLanguageCodeAsync(Guid userId, int languageCode)
    {
        logger.LogWarning("SetUserPreferredLanguageCodeAsync called on Query-side LanguageContext. Language preferences can only be set via Command API.");
        throw new InvalidOperationException("Language preferences can only be set via Command API. Query-side LanguageContext is read-only.");
    }
}