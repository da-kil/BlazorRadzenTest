using Marten;
using Marten.Events.Projections;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query;

public class MartenProjectionConfiguration : IConfigureMarten
{
    public void Configure(IServiceProvider services, StoreOptions options)
    {
        options.Projections.Snapshot<CategoryReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<QuestionnaireTemplateReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<EmployeeReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<OrganizationReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<QuestionnaireAssignmentReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<QuestionnaireResponseReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<ProjectionReplayReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<FeedbackTemplateReadModel>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<EmployeeFeedbackReadModel>(SnapshotLifecycle.Inline);

        options.Schema.For<UITranslation>().Index(x => x.Key);
        options.Schema.For<UITranslation>().Index(x => x.Category);
    }
}
