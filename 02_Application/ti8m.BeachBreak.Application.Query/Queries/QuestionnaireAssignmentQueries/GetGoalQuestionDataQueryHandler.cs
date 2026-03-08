using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query handler to retrieve goal data for a specific question within an assignment.
/// Reads from the QuestionnaireAssignmentReadModel projection (proper CQRS).
/// </summary>
public class GetGoalQuestionDataQueryHandler : IQueryHandler<GetGoalQuestionDataQuery, Result<GoalQuestionDataDto>>
{
    private readonly IQuestionnaireAssignmentRepository _repository;
    private readonly IQuestionnaireResponseRepository _responseRepository;

    public GetGoalQuestionDataQueryHandler(
        IQuestionnaireAssignmentRepository repository,
        IQuestionnaireResponseRepository responseRepository)
    {
        _repository = repository;
        _responseRepository = responseRepository;
    }

    public async Task<Result<GoalQuestionDataDto>> HandleAsync(GetGoalQuestionDataQuery query, CancellationToken cancellationToken = default)
    {
        var assignment = await _repository.GetAssignmentByIdAsync(query.AssignmentId, cancellationToken);

        if (assignment == null)
        {
            return Result<GoalQuestionDataDto>.Fail($"Assignment {query.AssignmentId} not found", 404);
        }

        var dto = new GoalQuestionDataDto
        {
            QuestionId = query.QuestionId,
            WorkflowState = assignment.WorkflowState,
            Goals = new List<GoalDto>()
        };

        if (assignment.AssignmentPredecessorId.HasValue)
        {
            dto.PredecessorAssignmentId = assignment.AssignmentPredecessorId.Value;
            dto.PredecessorGoalRatings = await LoadPredecessorGoalStubsAsync(
                assignment.AssignmentPredecessorId.Value,
                cancellationToken);
        }

        return Result<GoalQuestionDataDto>.Success(dto);
    }

    private async Task<List<GoalRatingDto>> LoadPredecessorGoalStubsAsync(
        Guid predecessorAssignmentId,
        CancellationToken cancellationToken)
    {
        var predecessorResponse = await _responseRepository.GetByAssignmentIdAsync(
            predecessorAssignmentId, cancellationToken);

        if (predecessorResponse == null)
            return new List<GoalRatingDto>();

        var stubs = new List<GoalRatingDto>();

        foreach (var sectionKvp in predecessorResponse.SectionResponses)
        {
            foreach (var roleKvp in sectionKvp.Value)
            {
                if (roleKvp.Value is QuestionResponseValue.GoalResponse goalResponse)
                {
                    foreach (var goal in goalResponse.Goals)
                    {
                        // Skip duplicates (same goal may appear under multiple role responses)
                        if (stubs.Any(s => s.SourceGoalId == goal.GoalId))
                            continue;

                        stubs.Add(new GoalRatingDto
                        {
                            SourceAssignmentId = predecessorAssignmentId,
                            SourceGoalId = goal.GoalId,
                            RatedByRole = goal.AddedByRole,
                            DegreeOfAchievement = 0,
                            Justification = string.Empty,
                            OriginalObjective = goal.ObjectiveDescription,
                            OriginalAddedByRole = goal.AddedByRole
                        });
                    }
                }
            }
        }

        return stubs;
    }
}
