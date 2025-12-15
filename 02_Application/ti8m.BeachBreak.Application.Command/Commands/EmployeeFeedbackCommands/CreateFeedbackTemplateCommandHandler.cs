using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handler for CreateFeedbackTemplateCommand.
/// Creates a new custom feedback template that can be reused for employee feedback recording.
/// </summary>
public class CreateFeedbackTemplateCommandHandler : ICommandHandler<CreateFeedbackTemplateCommand, Result<Guid>>
{
    private readonly UserContext userContext;

    public CreateFeedbackTemplateCommandHandler(UserContext userContext)
    {
        this.userContext = userContext;
    }

    public async Task<Result<Guid>> HandleAsync(CreateFeedbackTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the command
            var validationResult = ValidateCommand(request);
            if (!validationResult.Succeeded)
                return Result<Guid>.Fail(validationResult.Message, validationResult.StatusCode);

            // Set the creator ID
            request.CreatedBy = Guid.Parse(userContext.Id);

            // Generate a new template ID
            var templateId = Guid.NewGuid();

            // TODO: Implement actual template storage
            // For now, return success with the generated ID
            // In a full implementation, this would:
            // 1. Create a FeedbackTemplate aggregate or entity
            // 2. Validate that evaluation criteria are unique within the template
            // 3. Validate that text section keys are unique
            // 4. Store the template in the database or event store
            // 5. Publish a FeedbackTemplateCreated event if using event sourcing

            return Result<Guid>.Success(templateId);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Failed to create feedback template: {ex.Message}", 500);
        }
    }

    private Result ValidateCommand(CreateFeedbackTemplateCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateName))
            return Result.Fail("Template name is required", 400);

        if (request.TemplateName.Length > 100)
            return Result.Fail("Template name cannot exceed 100 characters", 400);

        if (request.SourceType < 0 || request.SourceType > 2)
            return Result.Fail("Source type must be 0 (Customer), 1 (Peer), or 2 (Project Colleague)", 400);

        if (request.RatingScale < 2 || request.RatingScale > 10)
            return Result.Fail("Rating scale must be between 2 and 10", 400);

        if (string.IsNullOrWhiteSpace(request.ScaleLowLabel))
            return Result.Fail("Scale low label is required", 400);

        if (string.IsNullOrWhiteSpace(request.ScaleHighLabel))
            return Result.Fail("Scale high label is required", 400);

        // Validate evaluation criteria have unique keys
        var criteriaKeys = request.EvaluationCriteria.Select(c => c.Key).ToList();
        if (criteriaKeys.Count != criteriaKeys.Distinct().Count())
            return Result.Fail("Evaluation criteria must have unique keys", 400);

        // Validate text sections have unique keys
        var textSectionKeys = request.TextSections.Select(t => t.Key).ToList();
        if (textSectionKeys.Count != textSectionKeys.Distinct().Count())
            return Result.Fail("Text sections must have unique keys", 400);

        // Ensure at least one evaluation criterion or text section
        if (!request.EvaluationCriteria.Any() && !request.TextSections.Any())
            return Result.Fail("Template must have at least one evaluation criterion or text section", 400);

        return Result.Success();
    }
}