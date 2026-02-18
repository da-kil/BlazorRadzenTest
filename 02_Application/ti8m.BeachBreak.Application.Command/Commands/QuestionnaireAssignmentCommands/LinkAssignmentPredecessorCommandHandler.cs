using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles linking an assignment-wide predecessor for goal access across the entire questionnaire.
/// </summary>
public class LinkAssignmentPredecessorCommandHandler
    : ICommandHandler<LinkAssignmentPredecessorCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<LinkAssignmentPredecessorCommandHandler> logger;

    public LinkAssignmentPredecessorCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<LinkAssignmentPredecessorCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(LinkAssignmentPredecessorCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Linking assignment predecessor {PredecessorId} to assignment {AssignmentId}",
                command.PredecessorAssignmentId, command.AssignmentId);

            // Load current assignment
            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            // Load predecessor to validate it's finalized
            var predecessor = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.PredecessorAssignmentId, cancellationToken: cancellationToken);

            // Business rule: Can only link to finalized predecessors
            if (predecessor.WorkflowState != Domain.QuestionnaireAssignmentAggregate.WorkflowState.Finalized)
            {
                logger.LogWarning(
                    "Cannot link assignment predecessor {PredecessorId} - not finalized (current state: {State})",
                    command.PredecessorAssignmentId, predecessor.WorkflowState);
                return Result.Fail(
                    $"Cannot link assignment predecessor - it must be finalized (current state: {predecessor.WorkflowState})",
                    400);
            }

            // Business rule: Predecessor must belong to the same employee
            if (predecessor.EmployeeId != assignment.EmployeeId)
            {
                logger.LogWarning(
                    "Cannot link assignment predecessor {PredecessorId} - belongs to different employee. Assignment employee: {AssignmentEmployee}, Predecessor employee: {PredecessorEmployee}",
                    command.PredecessorAssignmentId, assignment.EmployeeId, predecessor.EmployeeId);
                return Result.Fail(
                    "Cannot link assignment predecessor - it must belong to the same employee",
                    400);
            }

            assignment.LinkAssignmentPredecessor(
                command.PredecessorAssignmentId,
                ApplicationRoleMapper.MapToDomain(command.LinkedByRole),
                command.LinkedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully linked assignment predecessor {PredecessorId} to assignment {AssignmentId}",
                command.PredecessorAssignmentId, command.AssignmentId);

            return Result.Success("Assignment predecessor linked successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when linking assignment predecessor for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error linking assignment predecessor for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to link assignment predecessor: " + ex.Message, 500);
        }
    }
}