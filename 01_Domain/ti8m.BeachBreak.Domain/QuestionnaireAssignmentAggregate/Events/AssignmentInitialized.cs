using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a manager initializes a questionnaire assignment.
/// Initialization is the first step after assignment, enabling optional tasks
/// like linking predecessor questionnaires and adding custom questions.
/// </summary>
public record AssignmentInitialized(
    DateTime InitializedDate,
    Guid InitializedByEmployeeId,
    string? InitializationNotes) : IDomainEvent;
