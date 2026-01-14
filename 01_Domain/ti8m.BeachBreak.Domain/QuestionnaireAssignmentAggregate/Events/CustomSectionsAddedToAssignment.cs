using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when custom question sections are added to an assignment
/// during initialization. Custom sections are instance-specific and excluded from reports.
/// </summary>
public record CustomSectionsAddedToAssignment(
    List<QuestionSectionData> CustomSections,
    DateTime AddedDate,
    Guid AddedByEmployeeId) : IDomainEvent;
