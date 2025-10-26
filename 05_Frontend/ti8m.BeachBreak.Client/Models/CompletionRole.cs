namespace ti8m.BeachBreak.Client.Models;

public enum CompletionRole
{
    Employee = 0,   // Only the employee completes this section
    Manager = 1,    // Only the line manager completes this section
    Both = 2        // Both employee and manager provide input
}