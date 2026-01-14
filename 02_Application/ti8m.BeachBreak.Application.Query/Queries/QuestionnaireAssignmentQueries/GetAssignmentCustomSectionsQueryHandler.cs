using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query handler to retrieve custom question sections for an assignment.
/// Returns sections with IsInstanceSpecific = true that were added during initialization.
/// </summary>
public class GetAssignmentCustomSectionsQueryHandler
    : IQueryHandler<GetAssignmentCustomSectionsQuery, Result<IEnumerable<QuestionSection>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;

    public GetAssignmentCustomSectionsQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository)
    {
        this.assignmentRepository = assignmentRepository;
    }

    public async Task<Result<IEnumerable<QuestionSection>>> HandleAsync(
        GetAssignmentCustomSectionsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assignment = await assignmentRepository.GetAssignmentByIdAsync(
                query.AssignmentId, cancellationToken);

            if (assignment == null)
            {
                return Result<IEnumerable<QuestionSection>>.Fail(
                    $"Assignment {query.AssignmentId} not found", 404);
            }

            // Return custom sections (already filtered by IsInstanceSpecific in the read model)
            return Result<IEnumerable<QuestionSection>>.Success(assignment.CustomSections);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<QuestionSection>>.Fail(
                $"Failed to retrieve custom sections: {ex.Message}", 500);
        }
    }
}
