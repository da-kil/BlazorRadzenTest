namespace ti8m.BeachBreak.Client.Services;

public class ReviewContextService
{
    public string? EmployeeName { get; private set; }
    public string? QuestionnaireName { get; private set; }
    public bool IsActive => EmployeeName != null;

    public event Action? OnStateChanged;

    public void Show(string employeeName, string questionnaireName)
    {
        EmployeeName = employeeName;
        QuestionnaireName = questionnaireName;
        OnStateChanged?.Invoke();
    }

    public void Hide()
    {
        EmployeeName = null;
        QuestionnaireName = null;
        OnStateChanged?.Invoke();
    }
}
