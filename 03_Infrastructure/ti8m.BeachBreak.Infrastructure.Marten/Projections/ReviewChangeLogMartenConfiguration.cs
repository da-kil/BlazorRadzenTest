using JasperFx.Events.Projections;
using Marten;

namespace ti8m.BeachBreak.Infrastructure.Marten.Projections;

public class ReviewChangeLogMartenConfiguration : IConfigureMarten
{
    public void Configure(IServiceProvider services, StoreOptions options)
    {
        options.Projections.Add<ReviewChangeLogProjection>(ProjectionLifecycle.Inline);
    }
}
