namespace ti8m.BeachBreak.Core.Domain;

/// <summary>
/// Defines who is responsible for completing a questionnaire section.
/// Values are explicit to prevent serialization bugs across CQRS layers.
/// </summary>
public enum CompletionRole
{
    /// <summary>
    /// Section to be completed by the employee.
    /// </summary>
    Employee = 0,

    /// <summary>
    /// Section to be completed by the manager.
    /// </summary>
    Manager = 1,

    /// <summary>
    /// Section to be completed by both employee and manager.
    /// </summary>
    Both = 2
}
