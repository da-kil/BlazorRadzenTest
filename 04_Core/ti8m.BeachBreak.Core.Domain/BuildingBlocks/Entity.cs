namespace ti8m.BeachBreak.Core.Domain.BuildingBlocks;

public abstract class Entity<TIdentityType>
{
    protected Entity()
    {
    }

    protected Entity(TIdentityType id)
    {
        Id = id;
    }

    public static bool operator ==(Entity<TIdentityType>? left, Entity<TIdentityType>? right) =>
        left?.Equals(right) ?? Equals(right, null);

    public static bool operator !=(Entity<TIdentityType>? left, Entity<TIdentityType>? right) =>
        !left?.Equals(right) ?? !Equals(right, null);

    public TIdentityType Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TIdentityType> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (this.GetType() != other.GetType())
        {
            return false;
        }

        TIdentityType? id = this.Id;

        return id != null && id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        TIdentityType? id = this.Id;
        return id != null ? (this.GetType().ToString() + (id.GetHashCode() ^ 31)).GetHashCode(StringComparison.Ordinal) : 0;
    }
}
