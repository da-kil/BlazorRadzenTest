using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles linking a predecessor questionnaire for rating previous goals.
/// </summary>
public class LinkPredecessorQuestionnaireCommandHandler
    : ICommandHandler<LinkPredecessorQuestionnaireCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<LinkPredecessorQuestionnaireCommandHandler> logger;

    public LinkPredecessorQuestionnaireCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<LinkPredecessorQuestionnaireCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(LinkPredecessorQuestionnaireCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Linking predecessor questionnaire {PredecessorId} to assignment {AssignmentId} for question {QuestionId}",
                command.PredecessorAssignmentId, command.AssignmentId, command.QuestionId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            assignment.LinkPredecessorQuestionnaire(
                command.QuestionId,
                command.PredecessorAssignmentId,
                command.LinkedByRole,
                command.LinkedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully linked predecessor questionnaire {PredecessorId} to assignment {AssignmentId}",
                command.PredecessorAssignmentId, command.AssignmentId);

            return Result.Success("Predecessor questionnaire linked successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when linking predecessor for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error linking predecessor questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to link predecessor questionnaire: " + ex.Message, 500);
        }
    }
}
