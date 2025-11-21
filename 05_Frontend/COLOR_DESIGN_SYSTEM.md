# BeachBreak Color Design System

## Overview

The BeachBreak application uses a systematic approach to color that ensures consistency, accessibility, and semantic meaning across all UI components. This system was established based on UI analysis findings that identified inconsistent color usage across multiple pages.

## Design Principles

1. **Semantic Consistency**: Colors convey meaning, not just aesthetics
2. **Accessibility First**: All colors meet WCAG 2.1 AA contrast standards
3. **Limited Palette**: Focused set of colors reduces cognitive load
4. **Hierarchical**: Clear visual hierarchy through systematic color usage
5. **Scalable**: System supports future components and themes

## Core Color Palette

### 🔵 Primary Blue - Brand & Main Actions
```css
--primary-color: #2563eb;        /* Blue-600 */
--primary-color-light: #3b82f6;  /* Blue-500 */
--primary-color-dark: #1d4ed8;   /* Blue-700 */
--primary-color-rgb: 37, 99, 235;
```
**Usage**: Main actions, links, brand elements, focused states

### ⚪ Secondary Gray - Supporting Elements
```css
--secondary-color: #6b7280;      /* Gray-500 */
--secondary-color-light: #9ca3af; /* Gray-400 */
--secondary-color-dark: #4b5563;  /* Gray-600 */
--secondary-color-rgb: 107, 114, 128;
```
**Usage**: Secondary actions, supporting text, borders

### 🟢 Success Green - Positive Actions
```css
--success-color: #059669;        /* Emerald-600 */
--success-color-light: #10b981;  /* Emerald-500 */
--success-color-dark: #047857;   /* Emerald-700 */
--success-color-rgb: 5, 150, 105;
```
**Usage**: Success messages, publish actions, completed states

### 🔴 Danger Red - Destructive Actions
```css
--danger-color: #dc2626;         /* Red-600 */
--danger-color-light: #ef4444;   /* Red-500 */
--danger-color-dark: #b91c1c;    /* Red-700 */
--danger-color-rgb: 220, 38, 38;
```
**Usage**: Delete actions, error messages, destructive operations

### 🟡 Warning Amber - Caution & Attention
```css
--warning-color: #d97706;        /* Amber-600 */
--warning-color-light: #f59e0b;  /* Amber-500 */
--warning-color-dark: #b45309;   /* Amber-700 */
--warning-color-rgb: 217, 119, 6;
```
**Usage**: Warnings, unpublish actions, caution states

### 🔵 Info Blue - Informational Content
```css
--info-color: #0284c7;           /* Sky-600 */
--info-color-light: #0ea5e9;     /* Sky-500 */
--info-color-dark: #0369a1;      /* Sky-700 */
--info-color-rgb: 2, 132, 199;
```
**Usage**: Information messages, tooltips, help content

## Neutral Colors

### Background Colors
```css
--background-color: #ffffff;      /* White */
--background-light: #f9fafb;      /* Gray-50 */
--background-muted: #f3f4f6;      /* Gray-100 */
--background-dark: #111827;       /* Gray-900 - Dark mode */
```

### Text Colors
```css
--text-color: #111827;            /* Gray-900 */
--text-muted: #6b7280;            /* Gray-500 */
--text-light: #9ca3af;            /* Gray-400 */
--text-white: #ffffff;            /* White */
```

### Border Colors
```css
--border-color: #d1d5db;          /* Gray-300 */
--border-light: #e5e7eb;          /* Gray-200 */
--border-dark: #9ca3af;           /* Gray-400 */
```

### Focus & Interaction States
```css
--focus-border: #2563eb;          /* Primary blue */
--focus-ring: rgba(37, 99, 235, 0.2); /* Primary with opacity */
--hover-background: rgba(37, 99, 235, 0.05);
```

## Component-Specific Guidelines

### Buttons
| Action Type | Color | When to Use |
|-------------|-------|-------------|
| Primary Actions | Primary Blue | Save, Submit, Create, Next |
| Secondary Actions | Secondary Gray | Cancel, Edit, Previous |
| Success Actions | Success Green | Publish, Complete, Approve |
| Destructive Actions | Danger Red | Delete, Remove, Unpublish |
| Neutral Actions | Light Gray | Archive, Close, Reset |

### Status Indicators
| Status | Color | Usage |
|--------|-------|-------|
| Active/Published | Success Green | Published templates, active assignments |
| Draft/Pending | Warning Amber | Draft templates, pending approvals |
| Archived/Inactive | Secondary Gray | Archived items, inactive states |
| Error/Failed | Danger Red | Failed operations, error states |
| Info/Processing | Info Blue | Loading states, information |

### Data Grid & Tables
```css
/* Row states */
.row-hover: var(--hover-background);
.row-selected: rgba(var(--primary-color-rgb), 0.1);
.row-error: rgba(var(--danger-color-rgb), 0.1);

/* Header styling */
.header-background: var(--background-light);
.header-border: var(--border-color);
```

### Form Elements
```css
/* Input states */
.input-border: var(--border-color);
.input-focus: var(--focus-border);
.input-error: var(--danger-color);
.input-success: var(--success-color);

/* Labels and help text */
.label-color: var(--text-color);
.help-text: var(--text-muted);
.error-text: var(--danger-color);
```

### Cards & Containers
```css
/* Card styling */
.card-background: var(--background-color);
.card-border: var(--border-light);
.card-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);

/* Interactive cards */
.card-hover-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
.card-selected-border: var(--primary-color);
```

## Accessibility Standards

### Contrast Ratios
All color combinations meet WCAG 2.1 AA standards:
- **Normal text**: 4.5:1 minimum
- **Large text**: 3:1 minimum
- **Interactive elements**: 3:1 minimum

### Color Blindness Support
- No information conveyed by color alone
- Icons and text labels accompany color coding
- High contrast alternatives available

### Focus Management
- Clear focus indicators using `--focus-border`
- Consistent focus ring with `--focus-ring`
- Skip to content functionality

## Usage Guidelines

### ✅ Do
- Use semantic colors that match their meaning
- Maintain consistent color usage across pages
- Test color combinations for accessibility
- Use CSS custom properties for all colors
- Follow the established color hierarchy

### ❌ Don't
- Use hard-coded hex colors in components
- Mix semantic meanings (e.g., red for success)
- Use too many colors in a single interface
- Ignore accessibility guidelines
- Create custom colors without design approval

## Implementation

### CSS Custom Properties
All colors are defined as CSS custom properties in `shared-variables.css`:

```css
:root {
  /* Primary Colors */
  --primary-color: #2563eb;
  --primary-color-light: #3b82f6;
  --primary-color-dark: #1d4ed8;
  /* ... additional colors */
}
```

### Component Usage
```razor
<!-- Use semantic color classes -->
<div class="status-published">Published</div>
<button class="btn btn-primary">Save</button>

<!-- Use CSS custom properties in styles -->
<div style="color: var(--text-muted)">Help text</div>
```

## Dark Mode Considerations

The color system is designed to support future dark mode implementation:

```css
@media (prefers-color-scheme: dark) {
  :root {
    --background-color: #111827;
    --background-light: #1f2937;
    --text-color: #f9fafb;
    --text-muted: #9ca3af;
    /* Maintain semantic color meanings */
  }
}
```

## Migration Guide

### Phase 1: Update CSS Variables
- Review and update `shared-variables.css`
- Add missing color definitions
- Remove inconsistent color values

### Phase 2: Component Updates
- Replace hard-coded colors with CSS custom properties
- Update component-specific styles
- Test accessibility compliance

### Phase 3: Testing & Validation
- Cross-browser testing
- Accessibility audit
- User testing for color blindness

## Maintenance

### Adding New Colors
1. Justify the need for a new color
2. Ensure it fits the semantic system
3. Test accessibility compliance
4. Document usage guidelines
5. Update this design system

### Regular Reviews
- Quarterly color usage audit
- Accessibility compliance check
- Component consistency review
- User feedback integration

## Benefits of This System

### Before
- 7+ inconsistent button colors
- Hard-coded hex values throughout codebase
- Poor accessibility compliance
- Inconsistent user experience
- Maintenance difficulties

### After
- 4 semantic color categories
- Centralized color management
- WCAG 2.1 AA compliance
- Consistent user experience
- Easy maintenance and updates

---

*This color design system addresses findings from the November 2025 UI analysis that identified critical inconsistencies in color usage across the BeachBreak application.*