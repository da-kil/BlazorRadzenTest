# CSS Architecture & Best Practices

## Overview
This document defines the CSS organization strategy for the ti8m BeachBreak Blazor application, balancing maintainability, reusability, and developer velocity.

---

## Three-Tier CSS Strategy

### Tier 1: Global Styles (`wwwroot/css/*.css`)
**Use for:**
- ✅ Design system variables (colors, spacing, typography)
- ✅ Reusable utility classes (`.text-center`, `.mt-4`)
- ✅ Component patterns used across multiple components
- ✅ Layout systems and grid frameworks
- ✅ Third-party library overrides

**Files:**
- `shared-variables.css` - Design tokens and CSS custom properties
- `components.css` - Shared component styles
- `utilities.css` - Utility classes
- `layout.css` - Layout and grid systems
- `questionnaire-unified.css` - Domain-specific styles
- `main.css` - Central import file

**Example:**
```css
/* shared-variables.css */
:root {
    --primary-color: #0f69ff;
    --success-color: #4CAF50;
    --spacing-md: 1rem;
    --border-color: #e9ecef;
}

/* utilities.css */
.flex-center {
    display: flex;
    justify-content: center;
    align-items: center;
}
```

---

### Tier 2: Component-Scoped CSS (`Component.razor.css`)
**Use for:**
- ✅ Component-specific styles that should be reusable elsewhere
- ✅ Styles that need scoping to avoid global namespace pollution
- ✅ Components that are shared across multiple pages

**Benefits:**
- Automatic CSS isolation (Blazor adds unique attributes like `b-abc123`)
- No naming conflicts
- Better performance (only loaded when component is used)
- Co-located with component code

**Example:**
```css
/* AsyncButton.razor.css */
.button-container {
    display: inline-block;
}

.processing {
    opacity: 0.6;
    pointer-events: none;
}
```

---

### Tier 3: Inline `<style>` in Components
**Use ONLY for:**
- ✅ Highly specific styles that are never reused elsewhere
- ✅ Component-internal layout that doesn't belong in global CSS
- ✅ Dynamic styles that depend on component state
- ✅ Rapid prototyping (then refactor to Tier 1/2 later)

**Example:**
```razor
<style>
    /* This grid layout is specific to QuestionCard's competency display */
    .competency-input-row {
        display: grid;
        grid-template-columns: 1fr auto;
        align-items: start;
        gap: 1rem;
    }
</style>
```

---

## Decision Rules

### The "3-Component Rule"
**If CSS is used in 3+ components → extract to global CSS**

When you find yourself copying the same CSS across multiple components, it's time to extract it to a global stylesheet.

**Example:**
```css
/* components.css */
.dialog-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--spacing-md);
    padding-top: var(--spacing-md);
    border-top: 1px solid var(--border-color);
}
```

---

### The "Reusable Component Rule"
**Shared components → use `.razor.css`**

Components in the `Components/Shared/` directory should have their own scoped CSS file for maintainability and reusability.

**Example:**
```
Components/
└── Shared/
    ├── AsyncButton.razor
    └── AsyncButton.razor.css  ← Scoped styles
```

---

### The "Unique Layout Rule"
**Component-specific layout → inline `<style>` is acceptable**

Styles that are truly unique to a single component's internal structure can remain inline for co-location and simplicity.

**Example:**
- QuestionCard's specific grid layouts
- SectionCard's specialized visual structure
- Dialog-specific positioning that's never reused

---

## Decision Tree

```
START: Where should my CSS go?
│
├─ Is it a design token (color, spacing, font)?
│  └─ YES → shared-variables.css
│
├─ Is it used in 3+ different components?
│  └─ YES → components.css or utilities.css
│
├─ Is it a reusable pattern for 1-2 components?
│  └─ YES → Component.razor.css (scoped)
│
├─ Is it specific to ONE component and never reused?
│  └─ YES → <style> block in component
│
└─ Is it page-specific layout?
   └─ YES → Page.razor.css (if complex) OR <style> block (if simple)
```

---

## Code Review Checklist

When reviewing CSS in pull requests, ask:

- [ ] Are there repeated patterns that should be extracted to global CSS?
- [ ] Should this shared component have a `.razor.css` file?
- [ ] Are design tokens (colors, spacing) used instead of hard-coded values?
- [ ] Is the inline `<style>` block truly component-specific?
- [ ] Could this utility class be added to `utilities.css` for reuse?

---

## Common Patterns to Extract

### Dialog Patterns
```css
/* components.css */
.dialog-header {
    background: linear-gradient(135deg, var(--primary-color), var(--primary-color-dark));
    color: white;
    padding: 1.5rem;
    border-radius: 12px 12px 0 0;
    margin: -1.5rem -1.5rem 1rem -1.5rem;
}

.dialog-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    padding-top: 1rem;
    border-top: 1px solid var(--border-color);
}

.dialog-content {
    padding: 1.5rem;
}

.icon-box {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
}

.info-section {
    background: #f8f9fa;
    border-radius: 8px;
    padding: 1rem;
    margin-bottom: 1rem;
    border-left: 4px solid var(--primary-color);
}
```

### Card Patterns
```css
/* components.css */
.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem;
    border-bottom: 1px solid var(--border-color);
}

.card-actions {
    display: flex;
    gap: 0.5rem;
}
```

---

## Action Plan

### Phase 1: Extract Common Dialog Patterns (Priority: HIGH)
**Goal:** Eliminate duplication across dialog components

**Tasks:**
1. Create dialog pattern classes in `components.css`
2. Update all dialog components to use extracted classes
3. Remove duplicate inline styles

**Components to update:**
- ConfirmationDialog
- FinishReviewMeetingDialog
- ConfirmEmployeeReviewDialog
- FinalizeQuestionnaireDialog
- DepartmentDetailsDialog
- TeamMemberDetailsDialog
- QuestionnaireAnalyticsDialog
- AssignmentDetailsDialog
- EditAnswerDialog
- EditAssignmentDialog

---

### Phase 2: Create Scoped CSS for Top Shared Components (Priority: MEDIUM)
**Goal:** Use Blazor CSS isolation for better maintainability

**Components to add `.razor.css` files:**
1. `AsyncButton.razor.css`
2. `WorkflowActionButtons.razor.css`
3. `ReviewComments.razor.css`
4. `SectionCard.razor.css`
5. `QuestionCard.razor.css`

---

### Phase 3: Document & Review (Priority: LOW)
**Goal:** Establish long-term practices

**Tasks:**
1. Add CSS architecture section to main README
2. Update code review guidelines
3. Conduct team training session
4. Create VS Code snippets for common patterns

---

## Migration Example

### Before (Duplicated)
```razor
<!-- ConfirmationDialog.razor -->
<style>
    .dialog-actions {
        display: flex;
        justify-content: flex-end;
        gap: 1rem;
        padding-top: 1rem;
        border-top: 1px solid #e9ecef;
    }
</style>

<!-- FinishReviewMeetingDialog.razor -->
<style>
    .dialog-actions {
        display: flex;
        justify-content: flex-end;
        gap: 1rem;
        padding-top: 1rem;
        border-top: 1px solid #e9ecef;
    }
</style>
```

### After (Centralized)
```css
/* components.css */
.dialog-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--spacing-md);
    padding-top: var(--spacing-md);
    border-top: 1px solid var(--border-color);
}
```

```razor
<!-- Both dialogs now use the same class -->
<div class="dialog-actions">
    <!-- buttons -->
</div>
```

---

## Benefits of This Approach

✅ **Maintainability:** Common patterns in one place, easy to update globally
✅ **Reusability:** Shared styles available to all components
✅ **Performance:** Scoped CSS is tree-shaken, inline styles are minimal
✅ **Developer Velocity:** Clear rules = fast decisions
✅ **Consistency:** Design system enforced through variables
✅ **Flexibility:** Inline styles still allowed for true one-offs

---

## Questions?

If you're unsure where CSS should go, ask:
1. "Will this pattern be used elsewhere?" → If yes, extract it
2. "Is this component shared?" → If yes, use `.razor.css`
3. "Is this truly unique to this component?" → If yes, inline is fine

When in doubt, start inline and refactor to global/scoped when you see duplication.
