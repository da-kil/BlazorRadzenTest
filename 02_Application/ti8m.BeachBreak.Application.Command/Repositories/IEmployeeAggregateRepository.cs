using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Repositories;

public interface IEmployeeAggregateRepository : IAggregateRepository
{
    Task<IList<Employee>?> FindEntriesToDeleteAsync<TAggregate>(
    Guid[] ids,
    int? version = null,
    CancellationToken cancellationToken = default)
    where TAggregate : AggregateRoot;
}