using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles linking employee feedback to a questionnaire assignment.
/// </summary>
public class LinkEmployeeFeedbackCommandHandler
    : ICommandHandler<LinkEmployeeFeedbackCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository assignmentRepository;
    private readonly IEmployeeFeedbackAggregateRepository feedbackRepository;
    private readonly ILogger<LinkEmployeeFeedbackCommandHandler> logger;

    public LinkEmployeeFeedbackCommandHandler(
        IQuestionnaireAssignmentAggregateRepository assignmentRepository,
        IEmployeeFeedbackAggregateRepository feedbackRepository,
        ILogger<LinkEmployeeFeedbackCommandHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.feedbackRepository = feedbackRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(LinkEmployeeFeedbackCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Linking feedback {FeedbackId} to assignment {AssignmentId} for question {QuestionId}",
                command.FeedbackId, command.AssignmentId, command.QuestionId);

            // Load assignment aggregate
            var assignment = await assignmentRepository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            // Validate feedback exists and belongs to the same employee
            var feedback = await feedbackRepository.LoadRequiredAsync(command.FeedbackId, cancellationToken);

            if (feedback.EmployeeId != assignment.EmployeeId)
            {
                logger.LogWarning(
                    "Cannot link feedback {FeedbackId} - belongs to different employee (feedback: {FeedbackEmployeeId}, assignment: {AssignmentEmployeeId})",
                    command.FeedbackId, feedback.EmployeeId, assignment.EmployeeId);
                return Result.Fail(
                    "Feedback must belong to the same employee as the assignment",
                    400);
            }

            if (feedback.IsDeleted)
            {
                logger.LogWarning(
                    "Cannot link feedback {FeedbackId} - feedback is deleted",
                    command.FeedbackId);
                return Result.Fail("Cannot link deleted feedback", 400);
            }

            // Link feedback via domain method
            assignment.LinkEmployeeFeedback(
                command.QuestionId,
                command.FeedbackId,
                ApplicationRoleMapper.MapToDomain(command.LinkedByRole),
                command.LinkedByEmployeeId);

            // Save aggregate
            await assignmentRepository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully linked feedback {FeedbackId} to assignment {AssignmentId} question {QuestionId}",
                command.FeedbackId, command.AssignmentId, command.QuestionId);

            return Result.Success("Employee feedback linked successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when linking feedback to assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Authorization error when linking feedback to assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail(ex.Message, 403);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error linking feedback {FeedbackId} to assignment {AssignmentId}", command.FeedbackId, command.AssignmentId);
            return Result.Fail("Failed to link employee feedback: " + ex.Message, 500);
        }
    }
}
