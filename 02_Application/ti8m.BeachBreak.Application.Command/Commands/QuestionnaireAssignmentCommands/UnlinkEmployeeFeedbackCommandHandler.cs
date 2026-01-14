using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles unlinking employee feedback from a questionnaire assignment.
/// </summary>
public class UnlinkEmployeeFeedbackCommandHandler
    : ICommandHandler<UnlinkEmployeeFeedbackCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository assignmentRepository;
    private readonly ILogger<UnlinkEmployeeFeedbackCommandHandler> logger;

    public UnlinkEmployeeFeedbackCommandHandler(
        IQuestionnaireAssignmentAggregateRepository assignmentRepository,
        ILogger<UnlinkEmployeeFeedbackCommandHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(UnlinkEmployeeFeedbackCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Unlinking feedback {FeedbackId} from assignment {AssignmentId} question {QuestionId}",
                command.FeedbackId, command.AssignmentId, command.QuestionId);

            var assignment = await assignmentRepository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            assignment.UnlinkEmployeeFeedback(
                command.QuestionId,
                command.FeedbackId,
                ApplicationRoleMapper.MapToDomain(command.UnlinkedByRole),
                command.UnlinkedByEmployeeId);

            await assignmentRepository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully unlinked feedback {FeedbackId} from assignment {AssignmentId} question {QuestionId}",
                command.FeedbackId, command.AssignmentId, command.QuestionId);

            return Result.Success("Employee feedback unlinked successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when unlinking feedback from assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unlinking feedback {FeedbackId} from assignment {AssignmentId}", command.FeedbackId, command.AssignmentId);
            return Result.Fail("Failed to unlink employee feedback: " + ex.Message, 500);
        }
    }
}
