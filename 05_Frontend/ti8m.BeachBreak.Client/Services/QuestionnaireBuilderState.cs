using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// State management service for QuestionnaireBuilder.
/// Encapsulates template editing state and provides centralized state management.
/// Uses lightweight observable pattern without heavy state management framework.
/// </summary>
public class QuestionnaireBuilderState
{
    private QuestionnaireTemplate _template = new();
    private int _currentStep = 1;
    private bool _showQuestionEditor = false;
    private QuestionItem? _editingQuestion = null;
    private int _editingSectionIndex = -1;
    private int _editingQuestionIndex = -1;
    private bool _showQuestionTypeSelection = false;
    private int _selectedSectionIndex = -1;
    private QuestionType? _selectedQuestionType = null;
    private bool _isDirty = false;

    /// <summary>
    /// Event raised when state changes
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// The questionnaire template being edited
    /// </summary>
    public QuestionnaireTemplate Template
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
    /// Whether the question editor dialog is shown
    /// </summary>
    public bool ShowQuestionEditor
    {
        get => _showQuestionEditor;
        set
        {
            if (_showQuestionEditor != value)
            {
                _showQuestionEditor = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// The question being edited (null if creating new)
    /// </summary>
    public QuestionItem? EditingQuestion
    {
        get => _editingQuestion;
        set
        {
            _editingQuestion = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Index of section containing the question being edited
    /// </summary>
    public int EditingSectionIndex
    {
        get => _editingSectionIndex;
        set
        {
            if (_editingSectionIndex != value)
            {
                _editingSectionIndex = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Index of the question being edited within its section
    /// </summary>
    public int EditingQuestionIndex
    {
        get => _editingQuestionIndex;
        set
        {
            if (_editingQuestionIndex != value)
            {
                _editingQuestionIndex = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Whether the question type selection dialog is shown
    /// </summary>
    public bool ShowQuestionTypeSelection
    {
        get => _showQuestionTypeSelection;
        set
        {
            if (_showQuestionTypeSelection != value)
            {
                _showQuestionTypeSelection = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Index of section where new question will be added
    /// </summary>
    public int SelectedSectionIndex
    {
        get => _selectedSectionIndex;
        set
        {
            if (_selectedSectionIndex != value)
            {
                _selectedSectionIndex = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Type of question being created
    /// </summary>
    public QuestionType? SelectedQuestionType
    {
        get => _selectedQuestionType;
        set
        {
            if (_selectedQuestionType != value)
            {
                _selectedQuestionType = value;
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
    /// Whether in edit mode (existing template) vs create mode (new template)
    /// </summary>
    public bool IsEditMode => Template?.Id != Guid.Empty;

    /// <summary>
    /// Resets all state to defaults
    /// </summary>
    public void Reset()
    {
        _template = new QuestionnaireTemplate();
        _currentStep = 1;
        _showQuestionEditor = false;
        _editingQuestion = null;
        _editingSectionIndex = -1;
        _editingQuestionIndex = -1;
        _showQuestionTypeSelection = false;
        _selectedSectionIndex = -1;
        _selectedQuestionType = null;
        _isDirty = false;
        NotifyStateChanged();
    }

    /// <summary>
    /// Opens the question editor for editing an existing question
    /// </summary>
    public void OpenQuestionEditor(QuestionItem question, int sectionIndex, int questionIndex)
    {
        EditingQuestion = question;
        EditingSectionIndex = sectionIndex;
        EditingQuestionIndex = questionIndex;
        ShowQuestionEditor = true;
    }

    /// <summary>
    /// Closes the question editor
    /// </summary>
    public void CloseQuestionEditor()
    {
        ShowQuestionEditor = false;
        EditingQuestion = null;
        EditingSectionIndex = -1;
        EditingQuestionIndex = -1;
    }

    /// <summary>
    /// Opens the question type selection dialog
    /// </summary>
    public void OpenQuestionTypeSelection(int sectionIndex)
    {
        SelectedSectionIndex = sectionIndex;
        ShowQuestionTypeSelection = true;
    }

    /// <summary>
    /// Closes the question type selection dialog
    /// </summary>
    public void CloseQuestionTypeSelection()
    {
        ShowQuestionTypeSelection = false;
        SelectedSectionIndex = -1;
        SelectedQuestionType = null;
    }

    /// <summary>
    /// Marks the template as modified
    /// </summary>
    public void MarkAsDirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Marks the template as saved (no pending changes)
    /// </summary>
    public void MarkAsClean()
    {
        IsDirty = false;
    }

    /// <summary>
    /// Notifies subscribers that state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
