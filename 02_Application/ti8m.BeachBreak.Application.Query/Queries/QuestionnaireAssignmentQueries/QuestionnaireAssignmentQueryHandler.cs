using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignmentQueryHandler :
    IQueryHandler<QuestionnaireAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>,
    IQueryHandler<QuestionnaireAssignmentQuery, Result<QuestionnaireAssignment>>,
    IQueryHandler<QuestionnaireEmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>
{
    private readonly IQuestionnaireAssignmentRepository repository;
    private readonly IQuestionnaireTemplateRepository templateRepository;
    private readonly ILogger<QuestionnaireAssignmentQueryHandler> logger;

    public QuestionnaireAssignmentQueryHandler(
        IQuestionnaireAssignmentRepository repository,
        IQuestionnaireTemplateRepository templateRepository,
        ILogger<QuestionnaireAssignmentQueryHandler> logger)
    {
        this.repository = repository;
        this.templateRepository = templateRepository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving all questionnaire assignments");

            var assignmentReadModels = await repository.GetAllAssignmentsAsync(cancellationToken);
            var assignments = await EnrichWithTemplateMetadataAsync(assignmentReadModels, cancellationToken);

            logger.LogInformation("Retrieved {AssignmentCount} assignments", assignments.Count());
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
                var assignments = await EnrichWithTemplateMetadataAsync(new[] { assignmentReadModel }, cancellationToken);
                var assignment = assignments.First();
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
            var assignments = await EnrichWithTemplateMetadataAsync(assignmentReadModels, cancellationToken);

            logger.LogInformation("Retrieved {AssignmentCount} assignments for employee {EmployeeId}", assignments.Count(), query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve employee assignments: " + ex.Message, 500);
        }
    }

    /// <summary>
    /// Enriches assignments with template metadata (name and category).
    /// Only fetches templates referenced by the assignments to minimize data transfer.
    /// </summary>
    private async Task<IEnumerable<QuestionnaireAssignment>> EnrichWithTemplateMetadataAsync(
        IEnumerable<Projections.QuestionnaireAssignmentReadModel> readModels,
        CancellationToken cancellationToken)
    {
        var readModelsList = readModels.ToList();
        if (!readModelsList.Any())
        {
            return Enumerable.Empty<QuestionnaireAssignment>();
        }

        // Get unique template IDs from assignments
        var templateIds = readModelsList.Select(a => a.TemplateId).Distinct().ToList();

        // Fetch only the templates we need
        var templates = await templateRepository.GetAllAsync(cancellationToken);
        var templateLookup = templates
            .Where(t => templateIds.Contains(t.Id))
            .ToDictionary(t => t.Id, t => (t.Name, t.CategoryId));

        // Map and enrich
        return readModelsList.Select(readModel =>
        {
            var assignment = MapToQuestionnaireAssignment(readModel);

            // Denormalize template metadata
            if (templateLookup.TryGetValue(readModel.TemplateId, out var templateMetadata))
            {
                assignment.TemplateName = templateMetadata.Name;
                assignment.TemplateCategoryId = templateMetadata.CategoryId;
            }
            else
            {
                // Template not found - log warning but don't fail
                logger.LogWarning("Template {TemplateId} not found for assignment {AssignmentId}",
                    readModel.TemplateId, readModel.Id);
                assignment.TemplateName = "Unknown Template";
                assignment.TemplateCategoryId = null;
            }

            return assignment;
        });
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
            AssignedBy = readModel.AssignedBy,
            Notes = readModel.Notes,

            // Workflow properties
            WorkflowState = readModel.WorkflowState,
            SectionProgress = readModel.SectionProgress,

            // Submission phase
            EmployeeSubmittedDate = readModel.EmployeeSubmittedDate,
            EmployeeSubmittedBy = readModel.EmployeeSubmittedBy,
            ManagerSubmittedDate = readModel.ManagerSubmittedDate,
            ManagerSubmittedBy = readModel.ManagerSubmittedBy,

            // Review phase
            ReviewInitiatedDate = readModel.ReviewInitiatedDate,
            ReviewInitiatedBy = readModel.ReviewInitiatedBy,
            EmployeeReviewConfirmedDate = readModel.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedBy = readModel.EmployeeReviewConfirmedBy,
            ManagerReviewConfirmedDate = readModel.ManagerReviewConfirmedDate,
            ManagerReviewConfirmedBy = readModel.ManagerReviewConfirmedBy,

            // Final state
            FinalizedDate = readModel.FinalizedDate,
            FinalizedBy = readModel.FinalizedBy,
            IsLocked = readModel.IsLocked
        };
    }
}
