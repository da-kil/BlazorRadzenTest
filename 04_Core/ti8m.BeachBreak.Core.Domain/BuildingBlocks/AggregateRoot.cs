using ReflectionMagic;

namespace ti8m.BeachBreak.Core.Domain.BuildingBlocks;

public abstract class AggregateRoot : Entity<Guid>
{
    private readonly List<IDomainEvent> uncommittedEvents = new();

    public long Version { get; set; }

    public IEnumerable<IDomainEvent> UncommittedEvents => this.uncommittedEvents;

    public void ClearUncommittedDomainEvents()
    {
        uncommittedEvents.Clear();
    }

    protected void RaiseEvent(IDomainEvent @event)
    {
        this.AsDynamic().Apply(@event);

        Version++;
        uncommittedEvents.Add(@event);
    }
}
