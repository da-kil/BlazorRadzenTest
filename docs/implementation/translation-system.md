# Translation System Implementation Guide

## Overview

The BeachBreak application uses a comprehensive multilingual translation system with support for English and German. Translations are stored in `TestDataGenerator/test-translations.json` and loaded at runtime.

---

## Adding New Translations

**CRITICAL**: When adding new UI text that uses `@T("translation.key")`, you MUST add the translation to test-translations.json IMMEDIATELY.

### Step-by-Step Process

1. **Add translation key to your Razor component:**
   ```razor
   <RadzenText>@T("sections.my-new-section")</RadzenText>
   ```

2. **Add entry to test-translations.json:**
   ```json
   {
     "key": "sections.my-new-section",
     "german": "Mein neuer Abschnitt",
     "english": "My New Section",
     "category": "sections",
     "createdDate": "2025-12-04T12:00:00.0000000+00:00"
   }
   ```

3. **Validate translations:**
   ```bash
   powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1
   ```

---

## Translation Key Naming Conventions

### Format Rules

- **Format**: Lowercase with hyphens: `my-translation-key`
- **No Special Characters**: Only lowercase letters, numbers, hyphens, and periods
- **Hierarchical Structure**: Use periods for namespacing: `sections.employee.assessment`

### Semantic Prefixes

Use category-based prefixes for consistent organization:

- `pages.*` - Page titles and descriptions
- `sections.*` - Section headings
- `tabs.*` - Tab labels
- `buttons.*` - Button text
- `labels.*` - Form labels and UI labels
- `columns.*` - Data grid column headers
- `placeholders.*` - Input placeholders
- `filters.*` - Filter labels
- `messages.*` - User messages
- `dialogs.*` - Dialog titles and content
- `tooltips.*` - Tooltip text
- `notifications.*` - Toast/notification messages
- `status.*` - Status labels
- `workflow-states.*` - Workflow state labels
- `actions.*` - Action descriptions and button labels
- `templates.*` - Template-related labels
- `assignments.*` - Assignment-related labels

---

## German Translation Guidelines

### Domain-Specific Terminology

**Maintain consistency** across all translations:

- "Questionnaire" → "Fragebogen"
- "Assignment" → "Zuweisung"
- "Employee" → "Mitarbeiter"
- "Template" → "Vorlage"
- "Category" → "Kategorie"
- "Status" → "Status"
- "Manager" → "Manager"
- "Review" → "Überprüfung"
- "Assessment" → "Bewertung"
- "Goal" → "Ziel"
- "Section" → "Abschnitt"
- "Progress" → "Fortschritt"
- "Initialize" → "Initialisieren"
- "Finalize" → "Abschließen"
- "Submit" → "Einreichen"

### Unicode Encoding for German Umlauts

**CRITICAL**: Use Unicode escape sequences in JSON files to prevent encoding issues:

- ä → `\u00E4`
- ö → `\u00F6`
- ü → `\u00FC`
- Ä → `\u00C4`
- Ö → `\u00D6`
- Ü → `\u00DC`
- ß → `\u00DF`

**Example**:
```json
{
  "key": "employees.select",
  "german": "Mitarbeiter ausw\u00E4hlen",
  "english": "Select Employees"
}
```

### Formality and Grammar

- **Formality**: Use professional business German (Sie-Form)
- **Capitalization**: Follow German capitalization rules (nouns are capitalized)
- **Button Text**: Use imperative form (e.g., "Hinzufügen", "Löschen")
- **Descriptions**: Use complete sentences with proper punctuation

---

## Validation Tools

### Comprehensive Translation Validation

**Validate all translations:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1
```

**What this script does**:
- Scans all .razor files for `@T("...")` calls
- Compares against test-translations.json
- Reports missing translations with detailed breakdown by category
- Returns exit code 1 if translations are missing
- Shows progress and summary statistics

**Sample Output**:
```
=== Translation Validation Report ===
Checking 127 Razor files...
Found 312 translation keys in use

Missing translations by category:
- pages: 3 missing keys
- sections: 7 missing keys
- buttons: 2 missing keys

Total: 12 missing translation keys

❌ Validation failed: 12 translation keys are missing from test-translations.json
```

### Verify Specific Pages

**Verify specific pages:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\verify-target-pages.ps1
```

---

## Historical Context

### 2025-12-04: Translation Recovery

**Background**: During initial multilingual migration (commit c79f22a), 187 translation keys were used in code but missing from test-translations.json.

**Recovery Process**:
- Recovered 54 missing translations for QuestionnaireAssignments, ProjectionReplayAdmin, and CategoryAdmin pages
- Original English text extracted from git commit 4a3f807 (pre-migration)
- German translations generated using AI-assisted translation with domain terminology
- Created validate-translations.ps1 to prevent future gaps

**Lesson Learned**: Always add translations atomically with UI changes. Running the validation script before committing prevents missing translations from reaching production.

### Translation System Evolution

**Phase 1** (2025-11-15): Initial multilingual support
- Basic @T() helper implementation
- Manual translation file creation
- English/German language support

**Phase 2** (2025-12-04): Comprehensive validation
- Automated translation validation scripts
- Recovery of missing translations
- Standardized naming conventions

**Phase 3** (Current): Mature system
- 400+ translation keys managed
- Automated validation in CI/CD pipeline
- Comprehensive domain terminology glossary

---

## Code Review Checklist

When reviewing PRs that add or modify UI text:

- [ ] All new `@T("...")` calls have entries in test-translations.json
- [ ] German translations are accurate and use professional business terminology
- [ ] Translation keys follow naming conventions (lowercase-with-hyphens)
- [ ] Proper category prefix used (pages.*, sections.*, etc.)
- [ ] German umlauts properly Unicode-escaped in JSON
- [ ] Validation script passes: `validate-translations.ps1`
- [ ] Tested language switching (English ↔ German)
- [ ] No hardcoded UI text visible (all text uses @T())
- [ ] Translation keys are semantically meaningful (not generic like "text1", "label2")
- [ ] German translations use correct business formality (Sie-Form)

---

## Common Mistakes to Avoid

### 1. Hardcoded Text Without Translations

**❌ Using hardcoded text without translations:**
```razor
<RadzenText>Select Employees</RadzenText>  <!-- BAD -->
```

**✅ Always use translation keys:**
```razor
<RadzenText>@T("sections.select-employees")</RadzenText>  <!-- GOOD -->
```

### 2. Missing Translation Entries

**❌ Adding @T() calls without adding to test-translations.json:**
- This causes translation keys to appear in the UI instead of actual text
- Always add both simultaneously

**Signs of missing translations**:
- UI shows "sections.select-employees" instead of "Select Employees"
- Validation script reports missing keys
- Language switching doesn't work for new text

### 3. Incorrect Unicode Encoding

**❌ Using wrong Unicode encoding:**
```json
"german": "Mitarbeiter auswählen"  <!-- BAD - will break JSON -->
```

**✅ Use proper Unicode escaping:**
```json
"german": "Mitarbeiter ausw\u00E4hlen"  <!-- GOOD -->
```

### 4. Inconsistent Domain Terminology

**❌ Inconsistent terms:**
- Using "Angestellte" instead of "Mitarbeiter" for "Employee"
- Using "Formular" instead of "Fragebogen" for "Questionnaire"

**✅ Check existing translations for consistency:**
- Search test-translations.json for similar terms
- Follow the domain-specific terminology glossary above

### 5. Generic or Meaningless Keys

**❌ Generic keys:**
```json
{
  "key": "text1",
  "english": "Select Employees"
}
```

**✅ Semantic keys:**
```json
{
  "key": "employees.select",
  "english": "Select Employees"
}
```

---

## Advanced Translation Management

### Merge Translations

If you've created new translations in a separate file:

```bash
cd TestDataGenerator
powershell -ExecutionPolicy Bypass -File merge-translations.ps1
```

This will merge, deduplicate, and sort all translations alphabetically by key.

### Bulk Translation Operations

**Add missing translations for specific category:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\add-category-translations.ps1 -Category "sections"
```

**Generate translations from code analysis:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\generate-missing-translations.ps1
```

### Translation File Structure

**test-translations.json format:**
```json
[
  {
    "key": "pages.questionnaire-management",
    "german": "Fragebogen-Verwaltung",
    "english": "Questionnaire Management",
    "category": "pages",
    "createdDate": "2025-12-04T12:00:00.0000000+00:00"
  }
]
```

**Required fields**:
- `key`: Unique identifier using naming conventions
- `german`: German translation with Unicode-escaped umlauts
- `english`: English translation
- `category`: Category prefix from the key (e.g., "pages" from "pages.questionnaire-management")
- `createdDate`: ISO timestamp of when translation was added

---

## Performance Considerations

### Translation Loading

- Translations are loaded at application startup
- Cached in memory for fast @T() lookups
- No database queries for translation resolution
- Language switching requires browser refresh (acceptable UX trade-off)

### File Size Management

- Current: ~400 translation entries
- Expected growth: ~50-100 entries per major feature
- File size remains manageable (< 100KB)
- Consider splitting into multiple files if exceeds 1000 entries

---

## Integration with Component Architecture

### Translation in Optimized Components

**Pattern**: Translation keys are resolved in parent components, not within OptimizedQuestionRenderer:

```razor
<!-- Parent component (DynamicQuestionnaire.razor) -->
<OptimizedQuestionRenderer
    QuestionType="@section.Type"
    Configuration="@section.Configuration"
    SectionTitle="@T($"sections.{section.Title.ToLowerInvariant()}")"
    ResponseData="@GetResponseDataForSection(section.Id)" />
```

**Benefits**:
- Keeps OptimizedQuestionRenderer translation-agnostic
- Allows flexible title resolution based on context
- Maintains separation of concerns

### Dynamic Translation Key Generation

**Pattern**: Build translation keys dynamically when needed:

```csharp
// For workflow states
var stateKey = $"workflow-states.{state.ToString().ToKebabCase()}";
var displayName = T(stateKey);

// For question types
var typeKey = $"question-types.{questionType.ToString().ToLowerInvariant()}";
var typeName = T(typeKey);
```

**Helper Methods**:
- `ToKebabCase()` extension method converts PascalCase to kebab-case
- `ToLowerInvariant()` for consistent casing

---

## Testing Translation Integration

### Manual Testing Checklist

When testing translations:

- [ ] Switch to German: All UI text displays in German
- [ ] Switch to English: All UI text displays in English
- [ ] No translation keys visible in UI (indicates missing translations)
- [ ] German umlauts display correctly (not as Unicode escapes)
- [ ] Professional business terminology used consistently
- [ ] Button labels are actionable in both languages
- [ ] Error messages are localized appropriately

### Automated Testing

**Validation Pipeline**:
1. `validate-translations.ps1` runs in CI/CD pipeline
2. Fails build if missing translations detected
3. Reports exact locations of missing keys for easy fixing

**Future Enhancements**:
- Unit tests for translation key coverage
- Automated German grammar validation
- Translation key usage analysis (detect unused keys)

---

## Troubleshooting

### Common Issues

**1. Translation Key Displays Instead of Text**
- **Cause**: Key used in @T() but not found in test-translations.json
- **Fix**: Add missing entry to translation file
- **Prevention**: Run validation script before committing

**2. German Umlauts Display as Unicode**
- **Cause**: Browser receiving raw Unicode escapes instead of decoded text
- **Fix**: Verify JSON file has valid Unicode escapes (not raw umlauts)
- **Example**: Use `\u00E4` not `ä` in JSON

**3. Language Switching Doesn't Work**
- **Cause**: Translation service not properly configured or cache not refreshing
- **Fix**: Restart application, check translation service registration
- **Note**: Language switching requires browser refresh (by design)

**4. Missing Category in Translation Key**
- **Cause**: Key doesn't follow naming convention
- **Fix**: Add proper category prefix (pages.*, sections.*, etc.)
- **Example**: Change "select-employees" to "sections.select-employees"

### Debug Translation Issues

**Check translation file validity:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\debug-translations.ps1
```

**Verify specific key exists:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\find-translation.ps1 -Key "sections.my-key"
```

---

## Performance Optimization

### Translation Caching Strategy

**Current Implementation**:
- Translations loaded once at startup
- Stored in memory dictionary for O(1) lookups
- No lazy loading (acceptable for current size)

**Future Optimization** (if needed):
- Lazy loading of translation categories
- Background refresh for development mode
- Compression for large translation files

### Key Lookup Performance

**Fast Pattern**:
```csharp
@T("sections.employee-assessment")  // Direct key lookup
```

**Avoid** (slower):
```csharp
@T($"sections.{dynamicCategory}.assessment")  // Runtime string concatenation
```

**Compromise** (acceptable):
```csharp
@T($"workflow-states.{state.ToString().ToKebabCase()}")  // Limited dynamic keys
```

---

## Migration and Maintenance

### Adding New Languages

To add support for additional languages:

1. **Update Translation Model**:
   ```json
   {
     "key": "pages.dashboard",
     "english": "Dashboard",
     "german": "Dashboard",
     "french": "Tableau de bord",
     "spanish": "Panel de control"
   }
   ```

2. **Update Translation Service**: Add language selection logic
3. **Update UI**: Add language selector component
4. **Create Validation Scripts**: Extend validation for new languages

### Removing Unused Translations

**Identify unused keys:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\find-unused-keys.ps1
```

**Remove safely:**
- Verify key is truly unused (check all file types)
- Consider keeping keys for features in development
- Remove from test-translations.json
- Re-run validation to ensure no missing keys

### Bulk Translation Updates

**Update all translations matching pattern:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\bulk-update-translations.ps1 -Pattern "sections.*" -OldTerm "Mitarbeiter" -NewTerm "Angestellte"
```

---

## Integration with Development Workflow

### Git Hooks Integration

**Pre-commit Hook Example**:
```bash
#!/bin/sh
# Validate translations before commit
cd TestDataGenerator
powershell -ExecutionPolicy Bypass -File validate-translations.ps1
if [ $? -ne 0 ]; then
  echo "❌ Commit blocked: Missing translations detected"
  echo "Run validation script for details"
  exit 1
fi
```

### IDE Integration

**Visual Studio Code Extensions**:
- JSON validation for test-translations.json
- Razor syntax highlighting with @T() recognition
- Translation key autocomplete (custom extension)

**Recommended Workflow**:
1. Write UI code with @T() placeholders
2. Add translations to test-translations.json immediately
3. Run validation script before testing
4. Test both English and German modes
5. Commit atomically (UI code + translations)

---

## Historical Context and Lessons Learned

### 2025-12-04: Translation Recovery Crisis

**What Happened**:
During the initial multilingual migration (commit c79f22a), 187 translation keys were discovered in the codebase but were missing from test-translations.json. This meant:
- UI displayed raw translation keys instead of text
- Language switching was broken
- User experience was severely degraded

**Recovery Process**:
- **Step 1**: Analyzed all .razor files to extract used keys
- **Step 2**: Retrieved original English text from git commit 4a3f807 (pre-migration)
- **Step 3**: Generated German translations using AI assistance with domain context
- **Step 4**: Created automated validation scripts to prevent recurrence
- **Step 5**: Established atomic update process (code + translations together)

**Key Learnings**:
1. **Never deploy UI changes without corresponding translations**
2. **Validate translations before every deployment**
3. **Translation debt accumulates quickly and is expensive to fix**
4. **Automated validation is essential for preventing gaps**
5. **Historical git commits are valuable for recovering lost English text**

### Evolution of Translation Validation

**Before Validation Scripts** (Pre-2025-12-04):
- Manual translation management
- Frequent missing translations in development
- No systematic way to detect gaps
- Inconsistent domain terminology

**After Validation Scripts** (2025-12-04+):
- Automated detection of missing translations
- CI/CD pipeline integration
- Zero tolerance for missing translations
- Consistent domain terminology enforcement

**Current State** (2026-01-30):
- 400+ translation keys managed systematically
- Comprehensive validation suite
- Mature domain terminology glossary
- Zero translation debt in production

---

## Quick Reference

### Essential Commands

```bash
# Validate all translations (run before commit)
powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1

# Merge external translation files
powershell -ExecutionPolicy Bypass -File TestDataGenerator\merge-translations.ps1

# Find specific translation
powershell -ExecutionPolicy Bypass -File TestDataGenerator\find-translation.ps1 -Key "pages.dashboard"

# Generate missing translations from code analysis
powershell -ExecutionPolicy Bypass -File TestDataGenerator\generate-missing-translations.ps1
```

### Translation Entry Template

```json
{
  "key": "category.descriptive-key-name",
  "german": "German translation with \\u00E4 for umlauts",
  "english": "English Translation",
  "category": "category",
  "createdDate": "2026-01-30T12:00:00.0000000+00:00"
}
```

### Domain Terminology Quick Reference

| English | German | Unicode |
|---------|--------|---------|
| Questionnaire | Fragebogen | - |
| Employee | Mitarbeiter | - |
| Manager | Manager | - |
| Assessment | Bewertung | - |
| Select | Auswählen | ausw\u00E4hlen |
| Review | Überprüfung | \u00DCberpr\u00FCfung |
| Settings | Einstellungen | - |

---

## References

- **CLAUDE.md**: Core translation patterns and rules
- **TestDataGenerator/test-translations.json**: Master translation file
- **TestDataGenerator/validate-translations.ps1**: Validation script
- **05_Frontend/ti8m.BeachBreak.Client/Services/TranslationService.cs**: Translation service implementation

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*