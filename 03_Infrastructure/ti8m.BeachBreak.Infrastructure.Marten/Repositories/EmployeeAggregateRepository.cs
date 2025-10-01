using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

public class EmployeeAggregateRepository : EventSourcedAggregateRepository, IEmployeeAggregateRepository
{
    public EmployeeAggregateRepository(IDocumentStore store, ILogger<EventSourcedAggregateRepository> logger) : base(store, logger)
    {
    }

    public async Task<IList<Employee>?> FindEntriesToDeleteAsync<TAggregate>(Guid[] ids, int? version = null, CancellationToken cancellationToken = default) where TAggregate : AggregateRoot
    {
        using var session = store.LightweightSession();
        var employeesToDelete = await session.QueryAsync<EmployeeReadModel>("WHERE (d.data ->> 'Id')::uuid != ALL(?);", ids);

        IList<Employee> employees = [];

        foreach (var employee in employeesToDelete)
        {
            var employeeAggregate = await LoadAsync<Employee>(employee.Id, version, cancellationToken);
            if (employeeAggregate != null)
            {
                employees.Add(employeeAggregate);
            }
        }

        return employees;
    }

    public async Task<Employee?> GetByLoginNameAsync(string loginName, CancellationToken cancellationToken = default)
    {
        using var session = store.LightweightSession();
        var employeeReadModel = await session.Query<EmployeeReadModel>()
            .Where(e => e.LoginName == loginName && !e.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

        if (employeeReadModel == null)
            return null;

        return await LoadAsync<Employee>(employeeReadModel.Id, cancellationToken: cancellationToken);
    }
}
