using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignmentQueryHandler :
    IQueryHandler<QuestionnaireAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>,
    IQueryHandler<QuestionnaireAssignmentQuery, Result<QuestionnaireAssignment>>,
    IQueryHandler<QuestionnaireEmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>
{
    private readonly IQuestionnaireAssignmentRepository repository;
    private readonly ILogger<QuestionnaireAssignmentQueryHandler> logger;

    public QuestionnaireAssignmentQueryHandler(
        IQuestionnaireAssignmentRepository repository,
        ILogger<QuestionnaireAssignmentQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving all questionnaire assignments");

            var assignmentReadModels = await repository.GetAllAssignmentsAsync(cancellationToken);
            var assignments = assignmentReadModels.Select(MapToQuestionnaireAssignment).ToList();

            logger.LogInformation("Retrieved {AssignmentCount} assignments", assignments.Count);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all assignments");
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve assignments: " + ex.Message, 500);
        }
    }

    public async Task<Result<QuestionnaireAssignment>> HandleAsync(QuestionnaireAssignmentQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving assignment {AssignmentId}", query.Id);

            var assignmentReadModel = await repository.GetAssignmentByIdAsync(query.Id, cancellationToken);
            if (assignmentReadModel != null)
            {
                var assignment = MapToQuestionnaireAssignment(assignmentReadModel);
                logger.LogInformation("Retrieved assignment {AssignmentId}", assignment.Id);
                return Result<QuestionnaireAssignment>.Success(assignment);
            }

            logger.LogWarning("Assignment {AssignmentId} not found", query.Id);
            return Result<QuestionnaireAssignment>.Fail($"Assignment {query.Id} not found", 404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId}", query.Id);
            return Result<QuestionnaireAssignment>.Fail("Failed to retrieve assignment: " + ex.Message, 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireEmployeeAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving assignments for employee {EmployeeId}", query.EmployeeId);

            var assignmentReadModels = await repository.GetAssignmentsByEmployeeIdAsync(query.EmployeeId, cancellationToken);
            var assignments = assignmentReadModels.Select(MapToQuestionnaireAssignment).ToList();

            logger.LogInformation("Retrieved {AssignmentCount} assignments for employee {EmployeeId}", assignments.Count, query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve employee assignments: " + ex.Message, 500);
        }
    }

    private static QuestionnaireAssignment MapToQuestionnaireAssignment(Projections.QuestionnaireAssignmentReadModel readModel)
    {
        return new QuestionnaireAssignment
        {
            Id = readModel.Id,
            TemplateId = readModel.TemplateId,
            EmployeeId = readModel.EmployeeId,
            EmployeeName = readModel.EmployeeName,
            EmployeeEmail = readModel.EmployeeEmail,
            AssignedDate = readModel.AssignedDate,
            DueDate = readModel.DueDate,
            StartedDate = readModel.StartedDate,
            CompletedDate = readModel.CompletedDate,
            IsWithdrawn = readModel.IsWithdrawn,
            WithdrawnDate = readModel.WithdrawnDate,
            WithdrawnBy = readModel.WithdrawnBy,
            WithdrawalReason = readModel.WithdrawalReason,
            Status = readModel.Status,
            AssignedBy = readModel.AssignedBy,
            Notes = readModel.Notes
        };
    }
}
