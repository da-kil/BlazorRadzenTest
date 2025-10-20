using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the completion of multiple sections by a manager in a single operation.
/// </summary>
public class CompleteBulkSectionsAsManagerCommandHandler
    : ICommandHandler<CompleteBulkSectionsAsManagerCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<CompleteBulkSectionsAsManagerCommandHandler> logger;

    public CompleteBulkSectionsAsManagerCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<CompleteBulkSectionsAsManagerCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CompleteBulkSectionsAsManagerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteBulkSectionsAsManager(command.SectionIds);
            await repository.StoreAsync(assignment, cancellationToken);

            return Result.Success("Sections completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing sections as manager for assignment {AssignmentId}",
                command.AssignmentId);
            return Result.Fail("Failed to complete sections: " + ex.Message, 500);
        }
    }
}
