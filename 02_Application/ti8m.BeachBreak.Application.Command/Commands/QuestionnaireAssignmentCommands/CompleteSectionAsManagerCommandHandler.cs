using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the completion of a questionnaire section by a manager.
/// Marks a specific section as completed in the manager's workflow.
/// </summary>
public class CompleteSectionAsManagerCommandHandler
    : ICommandHandler<CompleteSectionAsManagerCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<CompleteSectionAsManagerCommandHandler> logger;

    public CompleteSectionAsManagerCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<CompleteSectionAsManagerCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CompleteSectionAsManagerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Manager completing section {SectionId} for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteSectionAsManager(command.SectionId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully completed section {SectionId} as manager for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Success("Section completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing section {SectionId} as manager for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Fail("Failed to complete section: " + ex.Message, 500);
        }
    }
}
