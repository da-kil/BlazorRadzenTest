using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Handles queries for goal data within questionnaire assignments.
/// Loads aggregates from event store to retrieve goal data (no projections yet - can optimize later).
/// </summary>
public class GoalQueryHandler :
    IQueryHandler<GetAvailablePredecessorsQuery, Result<IEnumerable<AvailablePredecessorDto>>>,
    IQueryHandler<GetGoalQuestionDataQuery, Result<GoalQuestionDataDto>>
{
    private readonly IQuestionnaireAssignmentRepository repository;
    private readonly IQuestionnaireTemplateRepository templateRepository;
    private readonly ILogger<GoalQueryHandler> logger;

    public GoalQueryHandler(
        IQuestionnaireAssignmentRepository repository,
        IQuestionnaireTemplateRepository templateRepository,
        ILogger<GoalQueryHandler> logger)
    {
        this.repository = repository;
        this.templateRepository = templateRepository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<AvailablePredecessorDto>>> HandleAsync(
        GetAvailablePredecessorsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Getting available predecessors for assignment {AssignmentId}, question {QuestionId}",
                query.AssignmentId, query.QuestionId);

            // Load current assignment to get employee and template context
            var currentAssignment = await repository.LoadAggregateAsync(query.AssignmentId, cancellationToken);
            if (currentAssignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", query.AssignmentId);
                return Result<IEnumerable<AvailablePredecessorDto>>.Fail("Assignment not found", 404);
            }

            // Get category of current template
            var currentTemplate = await templateRepository.GetByIdAsync(
                currentAssignment.TemplateId, cancellationToken);
            if (currentTemplate == null)
            {
                logger.LogWarning("Template {TemplateId} not found", currentAssignment.TemplateId);
                return Result<IEnumerable<AvailablePredecessorDto>>.Fail("Template not found", 404);
            }

            // Find all assignments for same employee
            var employeeAssignments = await repository.GetAssignmentsByEmployeeIdAsync(
                currentAssignment.EmployeeId, cancellationToken);

            // Load aggregates and filter by:
            // 1. Same employee (already filtered)
            // 2. Same category
            // 3. Has goals for the specified question type
            // 4. Is completed (Finalized state)
            // 5. Not the current assignment
            var predecessors = new List<AvailablePredecessorDto>();

            foreach (var readModel in employeeAssignments.Where(a => a.Id != query.AssignmentId))
            {
                // Load aggregate to check for goals
                var aggregate = await repository.LoadAggregateAsync(readModel.Id, cancellationToken);
                if (aggregate == null)
                    continue;

                // Check if assignment is finalized
                if (aggregate.WorkflowState != Domain.QuestionnaireAssignmentAggregate.WorkflowState.Finalized)
                    continue;

                // Get template to check category
                var template = await templateRepository.GetByIdAsync(aggregate.TemplateId, cancellationToken);
                if (template == null || template.CategoryId != currentTemplate.CategoryId)
                    continue;

                // Check if it has goals for the specified question
                if (!aggregate.GoalsByQuestion.ContainsKey(query.QuestionId))
                    continue;

                var goals = aggregate.GoalsByQuestion[query.QuestionId];
                if (!goals.Any())
                    continue;

                predecessors.Add(new AvailablePredecessorDto
                {
                    AssignmentId = aggregate.Id,
                    TemplateName = template.Name,
                    AssignedDate = aggregate.AssignedDate,
                    CompletedDate = aggregate.CompletedDate,
                    GoalCount = goals.Count
                });
            }

            logger.LogInformation(
                "Found {Count} available predecessors for assignment {AssignmentId}",
                predecessors.Count, query.AssignmentId);

            return Result<IEnumerable<AvailablePredecessorDto>>.Success(predecessors.OrderByDescending(p => p.AssignedDate));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error getting available predecessors for assignment {AssignmentId}",
                query.AssignmentId);
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                "Failed to get available predecessors: " + ex.Message, 500);
        }
    }

    public async Task<Result<GoalQuestionDataDto>> HandleAsync(
        GetGoalQuestionDataQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Getting goal data for assignment {AssignmentId}, question {QuestionId}",
                query.AssignmentId, query.QuestionId);

            var assignment = await repository.LoadAggregateAsync(query.AssignmentId, cancellationToken);
            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", query.AssignmentId);
                return Result<GoalQuestionDataDto>.Fail("Assignment not found", 404);
            }

            var dto = new GoalQuestionDataDto
            {
                QuestionId = query.QuestionId
            };

            // Get predecessor link if exists
            if (assignment.PredecessorLinks.TryGetValue(query.QuestionId, out var predecessorId))
            {
                dto.PredecessorAssignmentId = predecessorId;
            }

            // Map goals
            if (assignment.GoalsByQuestion.TryGetValue(query.QuestionId, out var goals))
            {
                dto.Goals = goals.Select(g => new GoalDto
                {
                    Id = g.Id,
                    QuestionId = g.QuestionId,
                    AddedByRole = g.AddedByRole.ToString(), // Convert enum to string
                    TimeframeFrom = g.TimeframeFrom,
                    TimeframeTo = g.TimeframeTo,
                    ObjectiveDescription = g.ObjectiveDescription,
                    MeasurementMetric = g.MeasurementMetric,
                    WeightingPercentage = g.WeightingPercentage,
                    AddedAt = g.AddedAt,
                    AddedByEmployeeId = g.AddedByEmployeeId,
                    Modifications = g.Modifications.Select(m => new GoalModificationDto
                    {
                        ModifiedByRole = m.ModifiedByRole.ToString(), // Convert enum to string
                        ChangeReason = m.ChangeReason,
                        ModifiedAt = m.ModifiedAt,
                        ModifiedByEmployeeId = m.ModifiedByEmployeeId,
                        TimeframeFrom = m.TimeframeFrom,
                        TimeframeTo = m.TimeframeTo,
                        ObjectiveDescription = m.ObjectiveDescription,
                        MeasurementMetric = m.MeasurementMetric,
                        WeightingPercentage = m.WeightingPercentage
                    }).ToList()
                }).ToList();
            }

            // Map goal ratings
            if (assignment.GoalRatingsByQuestion.TryGetValue(query.QuestionId, out var ratings))
            {
                dto.PredecessorGoalRatings = ratings.Select(r => new GoalRatingDto
                {
                    Id = r.Id,
                    SourceAssignmentId = r.SourceAssignmentId,
                    SourceGoalId = r.SourceGoalId,
                    QuestionId = r.QuestionId,
                    RatedByRole = r.RatedByRole,
                    DegreeOfAchievement = r.DegreeOfAchievement,
                    Justification = r.Justification,
                    OriginalObjectiveDescription = r.Snapshot.ObjectiveDescription,
                    OriginalTimeframeFrom = r.Snapshot.TimeframeFrom,
                    OriginalTimeframeTo = r.Snapshot.TimeframeTo,
                    OriginalMeasurementMetric = r.Snapshot.MeasurementMetric,
                    OriginalAddedByRole = r.Snapshot.AddedByRole,
                    OriginalWeightingPercentage = r.Snapshot.WeightingPercentage
                }).ToList();
            }

            logger.LogInformation(
                "Retrieved {GoalCount} goals and {RatingCount} ratings for assignment {AssignmentId}, question {QuestionId}",
                dto.Goals.Count, dto.PredecessorGoalRatings.Count, query.AssignmentId, query.QuestionId);

            return Result<GoalQuestionDataDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error getting goal data for assignment {AssignmentId}, question {QuestionId}",
                query.AssignmentId, query.QuestionId);
            return Result<GoalQuestionDataDto>.Fail(
                "Failed to get goal data: " + ex.Message, 500);
        }
    }
}
