using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the submission of a completed manager questionnaire.
/// Marks the manager's portion of the questionnaire as submitted.
/// </summary>
public class SubmitManagerQuestionnaireCommandHandler
    : ICommandHandler<SubmitManagerQuestionnaireCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<SubmitManagerQuestionnaireCommandHandler> logger;

    public SubmitManagerQuestionnaireCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<SubmitManagerQuestionnaireCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(SubmitManagerQuestionnaireCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Manager submitting questionnaire for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                command.ExpectedVersion,
                cancellationToken);
            assignment.SubmitManagerQuestionnaire(command.SubmittedByEmployeeId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully submitted manager questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Manager questionnaire submitted");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting manager questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to submit manager questionnaire: " + ex.Message, 500);
        }
    }
}
