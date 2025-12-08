using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.Mappers;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Implementation of language context service using Marten.
/// Uses integer language codes to maintain Clean Architecture (avoids Domain dependencies in Core.Infrastructure).
/// Internally converts between language codes and Domain.Language enum using DomainLanguageMapper.
/// </summary>
public class LanguageContext : ILanguageContext
{
    private readonly IEmployeeAggregateRepository employeeRepository;
    private readonly UserContext userContext;
    private readonly ILogger<LanguageContext> logger;

    public LanguageContext(
        IEmployeeAggregateRepository employeeRepository,
        UserContext userContext,
        ILogger<LanguageContext> logger)
    {
        this.employeeRepository = employeeRepository;
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
            var employee = await employeeRepository.LoadAsync<Employee>(userId);

            if (employee == null)
            {
                logger.LogWarning("Employee not found for ID: {UserId}, returning English (0) as default language", userId);
                return 0; // English
            }

            var languageCode = DomainLanguageMapper.ToLanguageCode(employee.PreferredLanguage);
            logger.LogDebug("Retrieved language preference {Language} (code: {LanguageCode}) for user {UserId}", employee.PreferredLanguage, languageCode, userId);
            return languageCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving language preference for user {UserId}, returning English (0) as default", userId);
            return 0; // English
        }
    }

    public async Task SetUserPreferredLanguageCodeAsync(Guid userId, int languageCode)
    {
        try
        {
            var employee = await employeeRepository.LoadAsync<Employee>(userId);

            if (employee == null)
            {
                logger.LogWarning("Cannot set language preference - Employee not found for ID: {UserId}", userId);
                return;
            }

            var domainLanguage = DomainLanguageMapper.FromLanguageCode(languageCode);
            employee.ChangePreferredLanguage(domainLanguage);
            await employeeRepository.StoreAsync(employee, CancellationToken.None);

            logger.LogInformation("Updated language preference to {Language} (code: {LanguageCode}) for user {UserId}", domainLanguage, languageCode, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting language preference code {LanguageCode} for user {UserId}", languageCode, userId);
            throw;
        }
    }
}