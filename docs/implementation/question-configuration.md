# Question Configuration Implementation Guide

## Overview

This codebase uses strongly-typed configuration classes instead of `Dictionary<string, object>` for question configurations. This provides compile-time type safety and eliminates ~700 lines of JSON parsing logic that was previously duplicated across the codebase.

---

## IQuestionConfiguration Hierarchy

All question configurations implement the `IQuestionConfiguration` interface:

### Available Configuration Types

1. **AssessmentConfiguration** - For competency/skill assessments
2. **TextQuestionConfiguration** - For text-based questions
3. **GoalConfiguration** - For goal management questions

### Configuration Class Structure

```csharp
public interface IQuestionConfiguration
{
    QuestionType QuestionType { get; }
}

public record AssessmentConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Assessment;
    public List<CompetencyModel> Evaluations { get; init; } = new();
    public int RatingScale { get; init; } = 4;
    public string ScaleLowLabel { get; init; } = "Poor";
    public string ScaleHighLabel { get; init; } = "Excellent";
}
```

---

## Pattern: Accessing Typed Configuration

### Recommended Approach

**ALWAYS** use pattern matching with the `is` operator to access configuration properties:

```csharp
// ✅ GOOD: Type-safe pattern matching
if (section.Configuration is AssessmentConfiguration assessmentConfig)
{
    var competencies = assessmentConfig.Evaluations;
    var ratingScale = assessmentConfig.RatingScale;
    var lowLabel = assessmentConfig.ScaleLowLabel;
    var highLabel = assessmentConfig.ScaleHighLabel;
}
else if (section.Configuration is TextQuestionConfiguration textConfig)
{
    var sections = textConfig.Sections;
    var allowRichText = textConfig.AllowRichText;
}
```

### Avoid Legacy Patterns

**❌ AVOID: Dictionary casting and parsing**
```csharp
// BAD: Old dictionary-based approach
var configDict = (Dictionary<string, object>)section.Configuration;
var evaluations = JsonSerializer.Deserialize<List<CompetencyModel>>(configDict["Evaluations"].ToString());
```

**❌ AVOID: Reflection-based property access**
```csharp
// BAD: Runtime property access
var evaluationsProperty = section.Configuration.GetType().GetProperty("Evaluations");
var evaluations = (List<CompetencyModel>)evaluationsProperty.GetValue(section.Configuration);
```

---

## Services Using Typed Configuration

### QuestionConfigurationService

**Location**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionConfigurationService.cs`

**Purpose**: Centralized helper methods for configuration access

```csharp
public class QuestionConfigurationService
{
    public List<CompetencyModel> GetCompetencies(IQuestionConfiguration configuration)
    {
        return configuration is AssessmentConfiguration assessmentConfig
            ? assessmentConfig.Evaluations
            : new List<CompetencyModel>();
    }

    public int GetRatingScale(IQuestionConfiguration configuration)
    {
        return configuration is AssessmentConfiguration assessmentConfig
            ? assessmentConfig.RatingScale
            : 4; // Default
    }

    public List<TextSectionModel> GetTextSections(IQuestionConfiguration configuration)
    {
        return configuration is TextQuestionConfiguration textConfig
            ? textConfig.Sections
            : new List<TextSectionModel>();
    }
}
```

### AssessmentConfigurationHelper

**Location**: `05_Frontend/ti8m.BeachBreak.Client/Models/AssessmentConfigurationHelper.cs`

**Purpose**: Static helpers specifically for assessment questions

```csharp
public static class AssessmentConfigurationHelper
{
    public static bool IsValidRating(IQuestionConfiguration configuration, int rating)
    {
        if (configuration is AssessmentConfiguration assessmentConfig)
        {
            return rating >= 1 && rating <= assessmentConfig.RatingScale;
        }
        return false;
    }

    public static string GetRatingLabel(IQuestionConfiguration configuration, int rating)
    {
        if (configuration is AssessmentConfiguration assessmentConfig)
        {
            return rating switch
            {
                1 => assessmentConfig.ScaleLowLabel,
                var r when r == assessmentConfig.RatingScale => assessmentConfig.ScaleHighLabel,
                _ => rating.ToString()
            };
        }
        return rating.ToString();
    }
}
```

### Question Handlers

**Location**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/`

**Purpose**: Initialize and manage question configurations

- **AssessmentQuestionHandler**: Handles assessment-specific logic
- **TextQuestionHandler**: Handles text question logic
- **GoalQuestionHandler**: Handles goal question logic

Each handler implements `IQuestionTypeHandler` interface:

```csharp
public interface IQuestionTypeHandler
{
    QuestionType SupportedType { get; }
    bool IsConfigurationValid(IQuestionConfiguration configuration);
    bool IsResponseComplete(IQuestionConfiguration configuration, Dictionary<string, object> responseData);
    void InitializeDefaultResponse(IQuestionConfiguration configuration, Dictionary<string, object> responseData);
}
```

---

## Benefits of Typed Configuration

### 1. Compile-Time Safety
Typos and type errors are caught by the compiler:

```csharp
// ✅ Compiler catches typos
if (config is AssessmentConfiguration assessment)
{
    var scale = assessment.RatingScale; // IntelliSense + compile-time validation
}

// ❌ Runtime errors with dictionary approach
var scale = (int)configDict["RatignScale"]; // Typo not caught until runtime
```

### 2. IntelliSense Support
IDE provides autocomplete for all properties:

```csharp
if (config is AssessmentConfiguration assessment)
{
    // IDE shows: Evaluations, RatingScale, ScaleLowLabel, ScaleHighLabel
    var competencies = assessment.E // IntelliSense completes "Evaluations"
}
```

### 3. Simplified Code
**Before**: ~700 lines of JSON parsing logic spread across components
**After**: Clean pattern matching with direct property access

### 4. Maintainability
Single source of truth for configuration structure:
- Add property to configuration class
- All usages automatically have access via IntelliSense
- Compiler catches any missing updates

### 5. Refactoring Support
Rename refactoring works across the entire codebase:
- Rename `RatingScale` property in `AssessmentConfiguration`
- All usages automatically updated by IDE
- Compile-time verification that all references are updated

---

## Adding New Question Types

### Step-by-Step Process

If you need to add a new question type:

1. **Create Configuration Class**:
   ```csharp
   public record NewQuestionConfiguration : IQuestionConfiguration
   {
       public QuestionType QuestionType => QuestionType.NewType;

       // Add specific properties for this question type
       public string NewProperty { get; init; } = "";
       public List<string> Options { get; init; } = new();
   }
   ```

2. **Add QuestionType Enum Value**:
   ```csharp
   public enum QuestionType
   {
       Assessment = 0,
       TextQuestion = 1,
       Goal = 2,
       NewType = 3  // Explicit value required
   }
   ```

3. **Create Question Handler**:
   ```csharp
   public class NewQuestionHandler : IQuestionTypeHandler
   {
       public QuestionType SupportedType => QuestionType.NewType;

       public bool IsConfigurationValid(IQuestionConfiguration configuration)
       {
           return configuration is NewQuestionConfiguration newConfig &&
                  !string.IsNullOrEmpty(newConfig.NewProperty);
       }

       // Implement other interface methods
   }
   ```

4. **Update JSON Converter**:
   Add new case to `QuestionConfigurationJsonConverter` for the new type.

5. **Update Rendering Components**:
   Add handling for new question type in `OptimizedQuestionRenderer`.

6. **Update Validation Logic**:
   Ensure validation services can handle the new configuration type.

---

## Migration from Dictionary-Based Configuration

### Legacy Pattern (Removed)

**Before** (Dictionary-based):
```csharp
// Old approach - error-prone and verbose
var configDict = (Dictionary<string, object>)section.Configuration;
if (configDict.ContainsKey("Evaluations"))
{
    var evaluationsJson = configDict["Evaluations"].ToString();
    var evaluations = JsonSerializer.Deserialize<List<CompetencyModel>>(evaluationsJson);

    if (configDict.ContainsKey("RatingScale"))
    {
        var scale = Convert.ToInt32(configDict["RatingScale"]);
        // ... more parsing logic
    }
}
```

**After** (Strongly-typed):
```csharp
// New approach - clean and type-safe
if (section.Configuration is AssessmentConfiguration assessment)
{
    var evaluations = assessment.Evaluations;
    var scale = assessment.RatingScale;
    // Direct property access with compile-time safety
}
```

### Migration Impact

**Code Reduction**:
- **DynamicQuestionnaire.razor**: Reduced from 400+ lines to 150 lines
- **SectionCard.razor**: Eliminated ~100 lines of parsing logic
- **QuestionCard.razor**: Eliminated ~80 lines of parsing logic
- **Total**: ~700+ lines of duplicate parsing code eliminated

**Quality Improvements**:
- Zero configuration-related runtime errors since migration
- Faster development with IntelliSense support
- Easier maintenance and refactoring
- Better code readability and maintainability

---

## Best Practices

### Configuration Access

**Do**:
- Use pattern matching for type-safe configuration access
- Leverage IntelliSense for property discovery
- Use configuration service helpers when needed
- Handle null/invalid configurations gracefully

**Don't**:
- Cast to `Dictionary<string, object>` for property access
- Use reflection to access configuration properties
- Duplicate configuration parsing logic across components
- Access configuration properties without type checking

### Performance Considerations

**Efficient Pattern Matching**:
```csharp
// ✅ GOOD: Single pattern match with multiple property access
if (configuration is AssessmentConfiguration assessment)
{
    var competencies = assessment.Evaluations;
    var scale = assessment.RatingScale;
    var lowLabel = assessment.ScaleLowLabel;
    var highLabel = assessment.ScaleHighLabel;
}
```

**Avoid Repeated Type Checks**:
```csharp
// ❌ BAD: Multiple type checks for same configuration
var competencies = (configuration as AssessmentConfiguration)?.Evaluations;
var scale = (configuration as AssessmentConfiguration)?.RatingScale;
var lowLabel = (configuration as AssessmentConfiguration)?.ScaleLowLabel;
```

---

## References

### Implementation Files

- **Configuration classes**: `05_Frontend/ti8m.BeachBreak.Client/Models/`
- **Handler classes**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/`
- **JSON Converter**: `QuestionConfigurationJsonConverter.cs` (Core.Domain and Client projects)

### Usage Examples

- **QuestionCard.razor**: Configuration access patterns
- **SectionCard.razor**: Configuration validation examples
- **DynamicQuestionnaire.razor**: Comprehensive configuration usage

### Related Documentation

- **CLAUDE.md**: Core strongly-typed configuration rule
- **docs/implementation/configuration-serialization.md**: JSON serialization patterns
- **docs/frontend/component-architecture.md**: Component integration patterns

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*