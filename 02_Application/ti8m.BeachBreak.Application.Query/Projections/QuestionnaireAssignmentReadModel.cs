using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class QuestionnaireAssignmentReadModel
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsWithdrawn { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public string? WithdrawnBy { get; set; }
    public string? WithdrawalReason { get; set; }

    // Computed properties for compatibility with existing UI
    public AssignmentStatus Status => DetermineStatus();

    private AssignmentStatus DetermineStatus()
    {
        if (IsWithdrawn) return AssignmentStatus.Withdrawn;
        if (CompletedDate.HasValue) return AssignmentStatus.Completed;
        if (StartedDate.HasValue) return AssignmentStatus.InProgress;
        if (DueDate.HasValue && DueDate.Value < DateTime.UtcNow) return AssignmentStatus.Overdue;
        return AssignmentStatus.Assigned;
    }

    // Apply methods for all QuestionnaireAssignment domain events
    public void Apply(QuestionnaireAssignmentAssigned @event)
    {
        Id = @event.AggregateId;
        TemplateId = @event.TemplateId;
        EmployeeId = @event.EmployeeId;
        EmployeeName = @event.EmployeeName;
        EmployeeEmail = @event.EmployeeEmail;
        AssignedDate = @event.AssignedDate;
        DueDate = @event.DueDate;
        AssignedBy = @event.AssignedBy;
        Notes = @event.Notes;
    }

    public void Apply(AssignmentWorkStarted @event)
    {
        StartedDate = @event.StartedDate;
    }

    public void Apply(AssignmentWorkCompleted @event)
    {
        CompletedDate = @event.CompletedDate;
    }

    public void Apply(AssignmentDueDateExtended @event)
    {
        DueDate = @event.NewDueDate;
    }

    public void Apply(AssignmentWithdrawn @event)
    {
        IsWithdrawn = true;
        WithdrawnDate = @event.WithdrawnDate;
        WithdrawnBy = @event.WithdrawnBy;
        WithdrawalReason = @event.WithdrawalReason;
    }
}