using Microsoft.Extensions.Logging;
using System.Globalization;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class EmployeeCommandHandler :
    ICommandHandler<BulkInsertEmployeesCommand, Result>,
    ICommandHandler<BulkUpdateEmployeesCommand, Result>,
    ICommandHandler<BulkDeleteEmployeesCommand, Result>,
    ICommandHandler<ChangeEmployeeApplicationRoleCommand, Result>
{
    private static readonly Guid namespaceGuid = new("BF24D00E-4E5E-4B4D-AFC2-798860B2DA73");
    private readonly IEmployeeAggregateRepository repository;
    private readonly ILogger<EmployeeCommandHandler> logger;
    private readonly IAuthorizationCacheInvalidationService? authorizationCacheService;
    private readonly UserContext userContext;

    public EmployeeCommandHandler(
        IEmployeeAggregateRepository repository,
        ILogger<EmployeeCommandHandler> logger,
        UserContext userContext,
        IAuthorizationCacheInvalidationService? authorizationCacheService = null)
    {
        this.repository = repository;
        this.logger = logger;
        this.userContext = userContext;
        this.authorizationCacheService = authorizationCacheService;
    }

    public async Task<Result> HandleAsync(BulkInsertEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        int countInserted = 0;
        int countUpdated = 0;
        int countUndeleted = 0;
        int countDeleted = 0;
        IList<Guid> employeeIds = [];

        foreach (var e in command.Employees)
        {
            var employee = await repository.LoadAsync<Employee>(e.Id, cancellationToken: cancellationToken);
            employeeIds.Add(e.Id);

            if (employee is not null && employee.IsDeleted)
            {
                logger.LogInformation("Undeleting employee {Id} - {EmployeeId}", employee.Id, employee.EmployeeId);

                employee.Undelete();
                UpdateEmployee(e, employee);

                if (employee.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(employee, cancellationToken);
                    countUndeleted++;
                }
            }
            else if (employee is not null)
            {
                logger.LogInformation("Update employee {Id} - {EmployeeId}", employee.Id, employee.EmployeeId);

                UpdateEmployee(e, employee);

                if (employee.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(employee, cancellationToken);
                    countUpdated++;
                }
            }
            else
            {
                logger.LogInformation("Insert employee {Id} - {EmployeeId}", e.Id, e.EmployeeId);

                var startDate = ParseStartDate(e.StartDate);
                var lastStartDate = ParseLastStartDate(e.LastStartDate);
                var endDate = ParseEndDate(e.EndDate);

                employee = new Employee(
                    e.Id,
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.Role,
                    e.EMail,
                    startDate,
                    endDate,
                    lastStartDate,
                    e.ManagerId,
                    e.LoginName,
                    int.Parse(e.OrganizationNumber));

                await repository.StoreAsync(employee, cancellationToken);
                countInserted++;
            }
        }

        var employeesToDelete = await repository.FindEntriesToDeleteAsync<Employee>(employeeIds.ToArray(), cancellationToken: cancellationToken);

        if (employeesToDelete is not null)
        {
            foreach (var employee in employeesToDelete)
            {
                employee.Delete();
                await repository.StoreAsync(employee, cancellationToken);
                countDeleted++;
            }
        }

        logger.LogInformation("Employee bulk import inserted: {CountInserted}, undeleted: {CountUndeleted}, updated: {countUpdated}, deleted: {CountDeleted}", countInserted, countUndeleted, countUpdated, countDeleted);
        return Result.Success();
    }

    public async Task<Result> HandleAsync(BulkUpdateEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var e in command.Employees)
        {
            var employee = await repository.LoadRequiredAsync<Employee>(e.Id, cancellationToken: cancellationToken);
            UpdateEmployee(e, employee);
            await repository.StoreAsync(employee, cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result> HandleAsync(BulkDeleteEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var e in command.EmployeeIds)
        {
            var employee = await repository.LoadRequiredAsync<Employee>(e, cancellationToken: cancellationToken);
            employee.Delete();
            await repository.StoreAsync(employee, cancellationToken);
        }
        return Result.Success();
    }

    private static void UpdateEmployee(SyncEmployee e, Employee employee)
    {
        // Update only the properties that exist in both command Employee and domain Employee aggregate
        employee.ChangeName(e.FirstName, e.LastName);
        employee.ChangeEmail(e.EMail);
        employee.ChangeRole(e.Role);
        employee.ChangeManager(e.ManagerId);
        employee.ChangeLoginName(e.LoginName);
        employee.ChangeDepartment(int.Parse(e.OrganizationNumber));
        employee.ChangeStartDate(ParseStartDate(e.StartDate));
        employee.ChangeEndDate(ParseEndDate(e.EndDate));
    }

    private static DateOnly ParseStartDate(string syncStartDate)
    {
        _ = DateOnly.TryParseExact(syncStartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate);
        return startDate;
    }

    private static DateOnly ParseLastStartDate(string syncLastStartDate)
    {
        _ = DateOnly.TryParseExact(syncLastStartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastStartDate);
        return lastStartDate;
    }

    private static DateOnly? ParseEndDate(string? syncEndDate)
    {
        bool hasDateOut = DateOnly.TryParseExact(syncEndDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
        return hasDateOut ? endDate : null;
    }

    public async Task<Result> HandleAsync(ChangeEmployeeApplicationRoleCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogChangeEmployeeApplicationRole(command.EmployeeId, command.NewRole.ToString());

            var employee = await repository.LoadRequiredAsync<Employee>(command.EmployeeId, cancellationToken: cancellationToken);

            if (employee.IsDeleted)
            {
                logger.LogChangeRoleForDeletedEmployee(command.EmployeeId);
                return Result.Fail("Cannot change application role for a deleted employee", 400);
            }

            // Change the application role with audit information from UserContext
            // RequesterRole is provided by infrastructure layer
            // Domain method validates authorization rules
            var userId = Guid.TryParse(userContext.Id, out var parsedUserId) ? parsedUserId : Guid.Empty;
            var userName = string.IsNullOrEmpty(userContext.Name) ? "System" : userContext.Name;

            var domainResult = employee.ChangeApplicationRole(
                ApplicationRoleMapper.MapToDomain(command.NewRole),
                ApplicationRoleMapper.MapToDomain(command.RequesterRole),
                userId,
                userName);

            if (!domainResult.IsSuccess)
            {
                logger.LogWarning("Failed to change application role for employee {EmployeeId}: {ErrorMessage}",
                    command.EmployeeId, domainResult.ErrorMessage);
                return Result.Fail(domainResult.ErrorMessage!, domainResult.StatusCode ?? 403);
            }

            await repository.StoreAsync(employee, cancellationToken);

            // Invalidate the authorization cache for this employee
            if (authorizationCacheService != null)
            {
                await authorizationCacheService.InvalidateEmployeeRoleCacheAsync(command.EmployeeId, cancellationToken);
            }

            logger.LogEmployeeApplicationRoleChanged(command.EmployeeId, command.NewRole.ToString());

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogChangeEmployeeApplicationRoleFailed(command.EmployeeId, ex.Message, ex);
            throw;
        }
    }
}