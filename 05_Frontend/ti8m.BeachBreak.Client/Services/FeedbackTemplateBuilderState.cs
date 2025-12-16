using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// State management service for FeedbackTemplateBuilder.
/// Encapsulates template editing state and provides centralized state management.
/// Uses lightweight observable pattern without heavy state management framework.
/// </summary>
public class FeedbackTemplateBuilderState
{
    private FeedbackTemplate _template = new();
    private int _currentStep = 1;
    private bool _isDirty = false;

    /// <summary>
    /// Event raised when state changes
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// The feedback template being edited
    /// </summary>
    public FeedbackTemplate Template
    {
        get => _template;
        set
        {
            _template = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Current step in the wizard (1-3)
    /// Step 1: Basic information
    /// Step 2: Criteria and text sections selection
    /// Step 3: Review and publish
    /// </summary>
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (_currentStep != value)
            {
                _currentStep = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Whether the template has unsaved changes
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty != value)
            {
                _isDirty = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Available evaluation criteria (loaded from a predefined set or custom)
    /// </summary>
    public List<EvaluationItem> AvailableCriteria { get; set; } = new();

    /// <summary>
    /// Keys of selected criteria (for tracking selection state)
    /// </summary>
    public List<string> SelectedCriteriaKeys { get; set; } = new();

    /// <summary>
    /// Available text sections (loaded from a predefined set or custom)
    /// </summary>
    public List<TextSectionDefinition> AvailableTextSections { get; set; } = new();

    /// <summary>
    /// Keys of selected text sections (for tracking selection state)
    /// </summary>
    public List<string> SelectedTextSectionKeys { get; set; } = new();

    /// <summary>
    /// Current user ID (for authorization checks)
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// Current user role (for authorization checks)
    /// </summary>
    public ApplicationRole CurrentUserRole { get; set; }

    /// <summary>
    /// Whether in edit mode (existing template) vs create mode (new template)
    /// </summary>
    public bool IsEditMode => Template?.Id != Guid.Empty;

    /// <summary>
    /// Resets all state to defaults for creating a new template
    /// </summary>
    public void Reset()
    {
        _template = new FeedbackTemplate
        {
            Id = Guid.NewGuid(),
            RatingScale = 10,
            ScaleLowLabel = "Poor",
            ScaleHighLabel = "Excellent",
            AllowedSourceTypes = new List<int> { 0, 1, 2 } // All source types by default
        };
        _currentStep = 1;
        _isDirty = false;
        SelectedCriteriaKeys.Clear();
        SelectedTextSectionKeys.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Marks the template as modified
    /// </summary>
    public void MarkAsDirty()
    {
        IsDirty = true;
        NotifyStateChanged();
    }

    /// <summary>
    /// Marks the template as saved (no pending changes)
    /// </summary>
    public void MarkAsClean()
    {
        IsDirty = false;
        NotifyStateChanged();
    }

    /// <summary>
    /// Checks if the current user can edit the template
    /// </summary>
    public bool CanEditTemplate()
    {
        return Template.CanBeEdited(CurrentUserId, CurrentUserRole);
    }

    /// <summary>
    /// Moves to the next step if valid
    /// </summary>
    public void NextStep()
    {
        if (CurrentStep < 3)
        {
            CurrentStep++;
        }
    }

    /// <summary>
    /// Moves to the previous step
    /// </summary>
    public void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    /// <summary>
    /// Notifies subscribers that state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
