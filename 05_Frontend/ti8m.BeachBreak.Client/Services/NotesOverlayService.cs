using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class NotesOverlayService
{
    public QuestionnaireAssignment? Assignment { get; private set; }
    public QuestionSection? CurrentSection { get; private set; }
    public bool IsDrawerOpen { get; private set; }

    public event Action? OnStateChanged;

    public void Show(QuestionnaireAssignment assignment)
    {
        Assignment = assignment;
        IsDrawerOpen = false;
        NotifyStateChanged();
    }

    public void Hide()
    {
        Assignment = null;
        CurrentSection = null;
        IsDrawerOpen = false;
        NotifyStateChanged();
    }

    public void SetCurrentSection(QuestionSection? section)
    {
        CurrentSection = section;
        NotifyStateChanged();
    }

    public void ToggleDrawer()
    {
        IsDrawerOpen = !IsDrawerOpen;
        NotifyStateChanged();
    }

    public void CloseDrawer()
    {
        IsDrawerOpen = false;
        NotifyStateChanged();
    }

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}
