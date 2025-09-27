using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Microsoft.Extensions.Logging;
using System.Globalization;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class EmployeeCommandHandler :
    ICommandHandler<BulkInsertEmployeesCommand, Result>,
    ICommandHandler<BulkUpdateEmployeesCommand, Result>,
    ICommandHandler<BulkDeleteEmployeesCommand, Result>
{
    private static readonly Guid namespaceGuid = new("BF24D00E-4E5E-4B4D-AFC2-798860B2DA73");
    private readonly IEmployeeAggregateRepository repository;
    private readonly ILogger<EmployeeCommandHandler> logger;

    public EmployeeCommandHandler(IEmployeeAggregateRepository repository, ILogger<EmployeeCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(BulkInsertEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        int countInserted = 0;
        int countUpdated = 0;
        int countUndeleted = 0;
        int countDeleted = 0;
        foreach (var e in command.Employees)
        {
            Guid employeeId = Deterministic.Create(namespaceGuid, e.EmployeeId);
            var employee = await repository.LoadAsync<Employee>(employeeId, cancellationToken: cancellationToken);

            if (employee is not null && employee.IsDeleted)
            {
                logger.LogInformation("Undeleting employee {EmployeeId}", employee.EmployeeId);

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
                logger.LogInformation("Update employee {EmployeeId}", employee.EmployeeId);

                UpdateEmployee(e, employee);

                if (employee.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(employee, cancellationToken);
                    countUpdated++;
                }
            }
            else
            {
                logger.LogInformation("Insert employee {EmployeeId}", e.EmployeeId);

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

        var ids = command.Employees.Select(o => Deterministic.Create(namespaceGuid, o.EmployeeId)).ToArray();
        var employeesToDelete = await repository.FindEntriesToDeleteAsync<Employee>(ids, cancellationToken: cancellationToken);

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
}