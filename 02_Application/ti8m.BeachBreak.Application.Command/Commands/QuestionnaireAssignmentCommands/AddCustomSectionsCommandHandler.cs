using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles adding custom question sections to an assignment during initialization.
/// Maps CommandQuestionSection DTOs to domain QuestionSection entities.
/// </summary>
public class AddCustomSectionsCommandHandler
    : ICommandHandler<AddCustomSectionsCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<AddCustomSectionsCommandHandler> logger;

    public AddCustomSectionsCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<AddCustomSectionsCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(
        AddCustomSectionsCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogAddCustomSections(command.AssignmentId, command.AddedByEmployeeId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            // Map CommandQuestionSection DTOs to domain QuestionSection entities
            var domainSections = command.Sections.Select(dto =>
                QuestionSection.CreateCustomSection(
                    dto.Id,
                    new Translation(dto.TitleEnglish, dto.TitleGerman),
                    new Translation(dto.DescriptionEnglish, dto.DescriptionGerman),
                    dto.Order,
                    dto.IsRequired,
                    dto.CompletionRole,
                    dto.Type,
                    dto.Configuration))
                .ToList();

            assignment.AddCustomSections(domainSections, command.AddedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogCustomSectionsAdded(command.AssignmentId, command.Sections.Count);
            return Result.Success($"Successfully added {command.Sections.Count} custom section(s)");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogAddCustomSectionsFailed(command.AssignmentId, ex.Message, ex);
            return Result.Fail($"Cannot add custom sections: {ex.Message}", 400);
        }
        catch (ArgumentException ex)
        {
            logger.LogAddCustomSectionsFailed(command.AssignmentId, ex.Message, ex);
            return Result.Fail($"Invalid custom section data: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            logger.LogAddCustomSectionsFailed(command.AssignmentId, ex.Message, ex);
            return Result.Fail($"Failed to add custom sections: {ex.Message}", 500);
        }
    }
}
