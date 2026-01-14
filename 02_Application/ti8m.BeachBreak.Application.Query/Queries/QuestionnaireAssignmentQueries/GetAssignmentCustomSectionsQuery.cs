using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get custom question sections added to an assignment during initialization.
/// Returns instance-specific sections that are excluded from aggregate reports.
/// </summary>
public class GetAssignmentCustomSectionsQuery : IQuery<Result<IEnumerable<QuestionSection>>>
{
    public Guid AssignmentId { get; init; }

    public GetAssignmentCustomSectionsQuery(Guid assignmentId)
    {
        AssignmentId = assignmentId;
    }
}
