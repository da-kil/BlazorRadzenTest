# Button Design System

## Overview

The BeachBreak application uses a standardized button system to ensure consistency, accessibility, and maintainability across all pages. This system reduces cognitive load on users and provides predictable interactions.

## Core Principles

1. **Consistency**: Same actions use the same colors across all pages
2. **Semantic**: Button colors convey meaning, not just aesthetics
3. **Limited Palette**: Only 4 color categories (Primary, Secondary, Success, Danger)
4. **Accessibility**: High contrast and clear visual hierarchy

## Button Color System

### 🔵 Primary Actions (Blue)
**When to use**: Main actions, saves, submissions, forward navigation
- `ButtonAction.Primary` - General primary action
- `ButtonAction.Save` - Save/Update data
- `ButtonAction.Submit` - Submit forms
- `ButtonAction.Create` - Create new items
- `ButtonAction.Next` - Navigation forward

### ⚪ Secondary Actions (Gray)
**When to use**: Supporting actions, edits, cancellations, backward navigation
- `ButtonAction.Secondary` - Secondary action
- `ButtonAction.Cancel` - Cancel operations
- `ButtonAction.Previous` - Navigation backward
- `ButtonAction.Edit` - Edit operations
- `ButtonAction.View` - View/Details
- `ButtonAction.Clone` - Clone/Duplicate

### 🟢 Success Actions (Green)
**When to use**: Positive outcomes, publishing, approvals, completions
- `ButtonAction.Success` - Success confirmation
- `ButtonAction.Publish` - Publish content
- `ButtonAction.Complete` - Mark complete
- `ButtonAction.Approve` - Approve actions
- `ButtonAction.Restore` - Restore from archive

### 🔴 Destructive Actions (Red)
**When to use**: Dangerous actions, deletions, unpublishing
- `ButtonAction.Danger` - Dangerous action
- `ButtonAction.Delete` - Delete items
- `ButtonAction.Remove` - Remove items
- `ButtonAction.Unpublish` - Unpublish content

### ⚫ Neutral Actions (Light Gray)
**When to use**: Neutral actions, archiving, closing
- `ButtonAction.Neutral` - Neutral actions
- `ButtonAction.Archive` - Archive items
- `ButtonAction.Close` - Close dialogs
- `ButtonAction.Reset` - Reset forms

## Usage Examples

### ✅ Recommended Usage

```razor
@using ti8m.BeachBreak.Client.Components.Shared

<!-- Save a questionnaire -->
<StandardizedButton Action="ButtonAction.Save"
                   ClickAsync="@SaveQuestionnaire" />

<!-- Delete with confirmation -->
<StandardizedButton Action="ButtonAction.Delete"
                   Text="Delete Template"
                   ClickAsync="@DeleteTemplate" />

<!-- Navigation -->
<StandardizedButton Action="ButtonAction.Next"
                   Text="Next: Build Sections"
                   ClickAsync="@(() => SetCurrentStep(2))" />

<!-- Custom text but semantic action -->
<StandardizedButton Action="ButtonAction.Clone"
                   Text="Duplicate Template"
                   ClickAsync="@CloneTemplate" />
```

### ❌ Avoid These Patterns

```razor
<!-- DON'T: Using inconsistent ButtonStyle directly -->
<AsyncButton ButtonStyle="ButtonStyle.Info" Text="Save" />
<AsyncButton ButtonStyle="ButtonStyle.Warning" Text="Delete" />

<!-- DON'T: Same action with different colors -->
<AsyncButton ButtonStyle="ButtonStyle.Secondary" Text="Save" />  <!-- Page 1 -->
<AsyncButton ButtonStyle="ButtonStyle.Primary" Text="Save" />    <!-- Page 2 -->

<!-- DON'T: Misleading color semantics -->
<AsyncButton ButtonStyle="ButtonStyle.Success" Text="Delete" />  <!-- Green for delete?! -->
```

## Migration Guide

### Step 1: Identify Action Type
Look at what the button does, not how it looks:
- Does it save data? → `ButtonAction.Save`
- Does it delete something? → `ButtonAction.Delete`
- Does it navigate forward? → `ButtonAction.Next`

### Step 2: Replace AsyncButton with StandardizedButton
```razor
<!-- Before -->
<AsyncButton Text="Publish Template"
            ButtonStyle="ButtonStyle.Success"
            Icon="publish"
            Click="@PublishTemplate" />

<!-- After -->
<StandardizedButton Action="ButtonAction.Publish"
                   Text="Publish Template"
                   ClickAsync="@PublishTemplate" />
```

### Step 3: Test Consistency
- Same actions should look identical across pages
- Color meanings should be intuitive to users
- Icons should match the action semantics

## Common Action Mappings

| Old Pattern | New Action | Reason |
|-------------|------------|---------|
| `ButtonStyle.Info` + Clone | `ButtonAction.Clone` | Info is not semantic |
| `ButtonStyle.Warning` + Unpublish | `ButtonAction.Unpublish` | Unpublish is destructive |
| `ButtonStyle.Light` + Edit | `ButtonAction.Edit` | Edit is secondary action |
| `ButtonStyle.Secondary` + Save | `ButtonAction.Save` | Save is primary action |

## Design System Benefits

### Before Standardization
- 7+ different button colors used inconsistently
- Same actions had different appearances across pages
- Cognitive load on users to understand button meanings
- Maintenance nightmare with scattered color definitions

### After Standardization
- 4 semantic color categories
- Consistent appearance for same actions
- Clear visual hierarchy and meaning
- Centralized color management

## Accessibility Considerations

- High contrast ratios maintained across all button styles
- Consistent icon usage for screen readers
- Semantic HTML structure with proper roles
- Focus management and keyboard navigation

## Performance Benefits

- Reduced CSS specificity
- Consistent rendering across browsers
- Fewer style recalculations
- Better caching of button styles

## Future Enhancements

1. **Dark Mode Support**: Extend color variables for dark theme
2. **Animation Consistency**: Standardized hover/click animations
3. **Size Variants**: Consistent sizing across different contexts
4. **Loading States**: Unified loading indicators

## Troubleshooting

### Q: Can I still use AsyncButton directly?
**A**: Yes, for edge cases, but prefer StandardizedButton for consistency.

### Q: What if I need a custom color?
**A**: Consider if it fits into one of the 4 semantic categories. If not, discuss with the team.

### Q: How do I handle complex button groups?
**A**: Use the semantic actions - primary for main action, secondary for supporting actions.

### Q: Can I override the default icon?
**A**: Yes, use the `Icon` parameter, but ensure it matches the action semantics.

## Code Review Checklist

- [ ] Button action matches its semantic purpose
- [ ] Same actions use same `ButtonAction` values across pages
- [ ] No direct `ButtonStyle` usage (prefer `StandardizedButton`)
- [ ] Icon choices are intuitive and accessible
- [ ] Text is clear and action-oriented

---

*This design system was created based on UI analysis findings from November 2025 identifying inconsistent button usage across 5+ pages with 7+ different color schemes.*