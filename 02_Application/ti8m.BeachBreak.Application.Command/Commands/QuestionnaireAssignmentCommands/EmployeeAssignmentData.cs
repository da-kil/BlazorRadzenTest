namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record EmployeeAssignmentData(
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail);