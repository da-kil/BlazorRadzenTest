using System.ComponentModel.DataAnnotations;
using ti8m.BeachBreak.Client.Components.Dialogs;

namespace ti8m.BeachBreak.Client.Components.ValidationHelpers;

public static class AssignmentValidationExtensions
{
    public static List<string> ValidateAssignment(this AssignQuestionnaireDialog.AssignmentModel model)
    {
        var errors = new List<string>();

        if (model.SelectedEmployeeId == Guid.Empty)
        {
            errors.Add("Please select an employee for the assignment.");
        }

        if (model.SelectedQuestionnaireId == Guid.Empty)
        {
            errors.Add("Please select a questionnaire to assign.");
        }

        if (model.DueDate.HasValue && model.DueDate.Value.Date < DateTime.Today)
        {
            errors.Add("Due date cannot be in the past.");
        }

        if (model.DueDate.HasValue && model.DueDate.Value.Date > DateTime.Today.AddYears(1))
        {
            errors.Add("Due date cannot be more than one year in the future.");
        }

        if (!string.IsNullOrEmpty(model.Notes) && model.Notes.Length > 1000)
        {
            errors.Add("Notes cannot exceed 1000 characters.");
        }

        if (string.IsNullOrWhiteSpace(model.Priority))
        {
            errors.Add("Please select a priority level.");
        }

        return errors;
    }

    public static List<string> ValidateBulkAssignment(this BulkAssignQuestionnaireDialog.BulkAssignmentModel model)
    {
        var errors = new List<string>();

        if (!model.SelectedEmployeeIds.Any())
        {
            errors.Add("Please select at least one employee for the bulk assignment.");
        }

        if (model.SelectedEmployeeIds.Count > 100)
        {
            errors.Add("Bulk assignments are limited to 100 employees at once. Please reduce the selection.");
        }

        if (model.SelectedQuestionnaireId == Guid.Empty)
        {
            errors.Add("Please select a questionnaire to assign.");
        }

        if (model.DueDate.HasValue && model.DueDate.Value.Date < DateTime.Today)
        {
            errors.Add("Due date cannot be in the past.");
        }

        if (model.DueDate.HasValue && model.DueDate.Value.Date > DateTime.Today.AddYears(1))
        {
            errors.Add("Due date cannot be more than one year in the future.");
        }

        if (!string.IsNullOrEmpty(model.Notes) && model.Notes.Length > 1000)
        {
            errors.Add("Notes cannot exceed 1000 characters.");
        }

        if (string.IsNullOrWhiteSpace(model.Priority))
        {
            errors.Add("Please select a priority level.");
        }

        if (model.StaggerAssignments && (model.StaggerDays < 1 || model.StaggerDays > 30))
        {
            errors.Add("Stagger days must be between 1 and 30 days.");
        }

        if (model.StaggerAssignments && model.SelectedEmployeeIds.Count * model.StaggerDays > 365)
        {
            errors.Add("Staggered assignment timeline would exceed one year. Please reduce employees or stagger days.");
        }

        return errors;
    }

    public static bool IsValidEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch (ArgumentException)
        {
            // Email validation can throw ArgumentException for invalid formats
            return false;
        }
    }

    public static string GetPriorityColor(string priority)
    {
        return priority switch
        {
            "Low" => "var(--green-new)",
            "Normal" => "var(--blue-new)",
            "High" => "var(--golden-milk)",
            "Critical" => "var(--peach-kiss)",
            _ => "var(--blue-new)"
        };
    }

    public static string GetPriorityIcon(string priority)
    {
        return priority switch
        {
            "Low" => "keyboard_arrow_down",
            "Normal" => "remove",
            "High" => "keyboard_arrow_up",
            "Critical" => "priority_high",
            _ => "remove"
        };
    }
}