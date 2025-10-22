# Question Rendering Quick Reference

**Last Updated**: 2025-10-22

## ⚡ Quick Start - Always Do This

```csharp
// Use OptimizedQuestionRenderer for ALL question rendering
<OptimizedQuestionRenderer
    Question="@question"
    Response="@response"
    OnResponseChanged="@HandleResponseChanged"
    IsReadOnly="@isReadOnly"
    HideHeader="@hideHeader" />

// Handle response changes with validation update
private async Task HandleResponseChanged(QuestionResponse updatedResponse)
{
    UpdateProgress(); // ✅ CRITICAL - Updates validation state
    await InvokeAsync(StateHasChanged);
}
```

## 🔑 Data Key Formats (Use These Exactly)

| Question Type | Key | Example Value |
|--------------|-----|---------------|
| **Text (single)** | `"value"` | `ComplexValue["value"] = "My answer"` |
| **Text (multiple)** | `"section_0"`, `"section_1"` | `ComplexValue["section_0"] = "Answer 1"` |
| **Assessment Rating** | `"rating_{key}"` | `ComplexValue["rating_communication"] = 8` |
| **Assessment Comment** | `"comment_{key}"` | `ComplexValue["comment_communication"] = "Good"` |
| **Goal Description** | `"Description"` | `ComplexValue["Description"] = "My goal"` |
| **Goal Percentage** | `"AchievementPercentage"` | `ComplexValue["AchievementPercentage"] = 75` |
| **Goal Justification** | `"Justification"` | `ComplexValue["Justification"] = "Because..."` |

### ⛔ OLD FORMAT (DO NOT USE)

```csharp
// ❌ WRONG - Causes data loss bugs
ComplexValue["text_0"] = "answer";      // OLD format
ComplexValue["text_1"] = "answer";      // OLD format

// ✅ CORRECT - Use this instead
ComplexValue["section_0"] = "answer";   // NEW format
ComplexValue["section_1"] = "answer";   // NEW format
```

## 📦 Component Locations

```
05_Frontend/ti8m.BeachBreak.Client/Components/Questions/
├── OptimizedQuestionRenderer.razor      (dispatcher)
├── OptimizedAssessmentQuestion.razor    (competency ratings)
├── OptimizedTextQuestion.razor          (single/multi section text)
└── OptimizedGoalQuestion.razor          (goal achievement)
```

## 🔄 Component Lifecycle Pattern

```csharp
// ✅ CORRECT - Initialize in BOTH methods
protected override void OnInitialized()
{
    base.OnInitialized();
    LoadData(); // First render
}

protected override void OnParametersSet()
{
    if (HasParameterChanged(nameof(Question), Question))
    {
        LoadData(); // Parameter changes
    }
}

// ❌ WRONG - OnParametersSet only
protected override void OnParametersSet()
{
    LoadData(); // May not fire on first render!
}
```

## ✅ Validation Pattern

```csharp
// ✅ CORRECT - Matches OptimizedTextQuestion
private bool IsTextQuestionCompleted(QuestionItem question, QuestionResponse response)
{
    // Single section
    if (textSections.Count == 1)
    {
        return response.ComplexValue?.TryGetValue("value", out var val) == true &&
               !string.IsNullOrWhiteSpace(val?.ToString());
    }

    // Multiple sections - use section_0, section_1, etc.
    for (int i = 0; i < textSections.Count; i++)
    {
        var key = $"section_{i}";
        if (!response.ComplexValue?.TryGetValue(key, out var val) == true ||
            string.IsNullOrWhiteSpace(val?.ToString()))
        {
            return false;
        }
    }
    return true;
}

// ❌ WRONG - Uses old "text_" format
private bool IsTextQuestionCompleted(QuestionItem question, QuestionResponse response)
{
    var key = $"text_{sectionOrder}"; // DON'T USE!
    return response.ComplexValue?.TryGetValue(key, out var val) == true;
}
```

## 🚫 Don't Duplicate These Methods

If you see these methods in your component, **DO NOT** copy them. Use the Optimized components instead:

```csharp
// ❌ DON'T DUPLICATE
GetCompetenciesFromConfiguration()
GetRatingScaleFromQuestion()
GetScaleLowLabelFromQuestion()
GetScaleHighLabelFromQuestion()
GetTextSectionsFromQuestion()
RenderAssessmentQuestion()
RenderTextQuestion()
RenderGoalQuestion()

// ✅ DO THIS INSTEAD
<OptimizedQuestionRenderer ... />
```

## 📝 Code Review Checklist

Before submitting code:

- [ ] ✅ Uses `OptimizedQuestionRenderer` (not inline rendering)
- [ ] ✅ Uses `"section_"` keys (not `"text_"`)
- [ ] ✅ No duplicate configuration parsing methods
- [ ] ✅ Validation matches Optimized component keys
- [ ] ✅ Component initializes in `OnInitialized()` + `OnParametersSet()`
- [ ] ✅ Response handler calls `UpdateProgress()`

## 🐛 Known Bugs These Patterns Prevent

1. **Submit Button Bug**: Button stays enabled when fields cleared → Fixed by calling `UpdateProgress()`
2. **Data Key Mismatch**: Review mode can't read answers → Fixed by using `"section_"` keys
3. **Edit Dialog Corruption**: Wrong keys overwrite data → Fixed by using Optimized components
4. **Missing TextArea**: Component not initializing → Fixed by `OnInitialized()` pattern

## 🆘 When You Get Stuck

1. **Check existing usage**: Look at `DynamicQuestionnaire.razor` (refactored)
2. **Check the components**: Look inside `OptimizedTextQuestion.razor` to see how it handles keys
3. **Read CLAUDE.md**: Section 6 has detailed examples
4. **Ask before breaking rules**: Don't reintroduce duplication

## 📚 Further Reading

- **CLAUDE.md**: Section 6 (Frontend Component Architecture)
- **ADR-001**: Frontend Component Architecture Decision Record
- **Frontend Architecture Review Report**: Full analysis (2025-10-22)

---

**Remember**: These rules exist because we discovered **real production bugs**. Following them prevents entire classes of issues.

**Last Reviewed**: 2025-10-22
