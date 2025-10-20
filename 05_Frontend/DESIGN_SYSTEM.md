# ti8m BeachBreak Design System

A comprehensive design token system for consistent UI development across the application.

## Table of Contents
- [Colors](#colors)
- [Typography](#typography)
- [Spacing](#spacing)
- [Shadows](#shadows)
- [Border Radius](#border-radius)
- [Transitions](#transitions)
- [Z-Index](#z-index)
- [Component Tokens](#component-tokens)

---

## Colors

### Primary Brand Colors
Used for primary actions, links, and brand elements.

```css
--primary-color: #2563eb           /* Main brand color */
--primary-color-light: #3b82f6     /* Hover states */
--primary-color-dark: #1d4ed8      /* Active states */
--primary-color-alpha: rgba(37, 99, 235, 0.1)  /* Backgrounds */
```

### Secondary Colors
Used for secondary actions and accents.

```css
--secondary-color: #6366f1
--secondary-color-light: #8b5cf6
--secondary-color-dark: #4c1d95
--secondary-gradient-start: #667eea    /* For gradient backgrounds */
--secondary-gradient-end: #764ba2
```

### Status Colors

#### Success (Green)
Used for successful actions, confirmations, and positive states.

```css
--success-color: #059669
--success-color-light: #10b981
--success-color-dark: #047857
--success-color-bg: #d1e7dd          /* Background for success messages */
--success-gradient-start: #4CAF50
--success-gradient-end: #45a049
```

#### Warning (Orange)
Used for warnings, cautionary states, and pending actions.

```css
--warning-color: #d97706
--warning-color-light: #f59e0b
--warning-color-dark: #b45309
--warning-color-bg: #fff3cd
--warning-gradient-start: #FF9800
--warning-gradient-end: #F57C00
```

#### Danger (Red)
Used for errors, destructive actions, and critical states.

```css
--danger-color: #dc2626
--danger-color-light: #ef4444
--danger-color-dark: #b91c1c
--danger-gradient-start: #dc3545
--danger-gradient-end: #c82333
```

#### Info (Blue)
Used for informational messages and neutral states.

```css
--info-color: #0284c7
--info-color-light: #0ea5e9
--info-color-dark: #0369a1
--info-gradient-start: #0f69ff
--info-gradient-end: #0056d1
```

### Text Colors
Hierarchical text colors for different levels of emphasis.

```css
--text-color: #374151            /* Primary body text */
--text-color-light: #4b5563      /* Slightly lighter */
--text-color-dark: #1f2937       /* Headings, emphasis */
--text-muted: #6b7280            /* Secondary text */
--text-muted-light: #6c757d      /* Alternative muted */
--text-secondary: #9ca3af        /* Tertiary text */
--text-disabled: #d1d5db         /* Disabled states */
--text-white: #ffffff            /* White text */
--text-white-75: rgba(255, 255, 255, 0.75)
--text-white-50: rgba(255, 255, 255, 0.5)
```

### Background Colors

```css
--background-color: #ffffff       /* Main background */
--background-light: #f9fafb       /* Light background */
--background-muted: #f3f4f6       /* Muted sections */
--background-dark: #e5e7eb        /* Dark backgrounds */
```

### Border Colors

```css
--border-color: #d1d5db          /* Default borders */
--border-light: #f3f4f6          /* Light borders */
--border-muted: #e5e7eb          /* Subtle borders */
--border-dark: #9ca3af           /* Prominent borders */
```

### Interaction States

```css
--focus-ring: rgba(37, 99, 235, 0.1)  /* Focus ring background */
--focus-border: #2563eb                /* Focus border color */
--hover-background: #f9fafb            /* Hover background */
--active-background: #f3f4f6           /* Active/pressed state */
```

---

## Typography

### Font Families

```css
--font-family-base: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif
--font-family-mono: 'SF Mono', 'Monaco', 'Cascadia Code', monospace
```

### Font Sizes
Based on a 16px base size with a modular scale.

```css
--font-size-xs: 0.75rem      /* 12px - Captions, labels */
--font-size-sm: 0.875rem     /* 14px - Small text */
--font-size-base: 1rem       /* 16px - Body text */
--font-size-lg: 1.125rem     /* 18px - Large body */
--font-size-xl: 1.25rem      /* 20px - H6 */
--font-size-2xl: 1.5rem      /* 24px - H5 */
--font-size-3xl: 1.875rem    /* 30px - H4 */
--font-size-4xl: 2.25rem     /* 36px - H3 */
```

### Font Weights

```css
--font-weight-normal: 400     /* Body text */
--font-weight-medium: 500     /* Emphasized text */
--font-weight-semibold: 600   /* Subheadings */
--font-weight-bold: 700       /* Headings */
```

### Line Heights

```css
--line-height-tight: 1.25     /* Headings */
--line-height-normal: 1.5     /* Body text */
--line-height-relaxed: 1.75   /* Long-form content */
```

---

## Spacing

Based on a 4px base unit with a harmonious scale.

```css
--spacing-xs: 0.25rem    /* 4px  - Tight spacing */
--spacing-sm: 0.5rem     /* 8px  - Small gaps */
--spacing-md: 1rem       /* 16px - Default spacing */
--spacing-lg: 1.5rem     /* 24px - Large spacing */
--spacing-xl: 2rem       /* 32px - Extra large */
--spacing-2xl: 2.5rem    /* 40px - Section spacing */
--spacing-3xl: 3rem      /* 48px - Major sections */
```

**Usage Guidelines:**
- `xs`: Space between closely related items (e.g., icon + label)
- `sm`: Space between form elements, list items
- `md`: Default padding for cards, buttons, inputs
- `lg`: Padding for larger containers, cards
- `xl`: Space between major sections
- `2xl`/`3xl`: Page-level spacing, hero sections

---

## Shadows

Elevation system for layered UI elements.

```css
--shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05)
/* Subtle shadow for cards, inputs */

--shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)
/* Medium shadow for dropdowns, hover states */

--shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)
/* Large shadow for modals, popovers */

--shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)
/* Extra large shadow for dialogs, overlays */
```

---

## Border Radius

Consistent rounding for UI elements.

```css
--radius-none: 0              /* Sharp corners */
--radius-sm: 0.25rem          /* 4px  - Subtle rounding */
--radius-md: 0.375rem         /* 6px  - Default rounding */
--radius-lg: 0.5rem           /* 8px  - Cards, buttons */
--radius-xl: 0.75rem          /* 12px - Large cards */
--radius-2xl: 1rem            /* 16px - Hero elements */
--radius-full: 9999px         /* Fully rounded (pills, avatars) */
```

---

## Transitions

Consistent animation timing for interactions.

```css
--transition-fast: 150ms ease-in-out     /* Quick interactions (hover) */
--transition-normal: 250ms ease-in-out   /* Default transitions */
--transition-slow: 350ms ease-in-out     /* Slower, emphasized */
```

**Usage:**
```css
.button {
    transition: all var(--transition-fast);
}
```

---

## Z-Index

Layering system for stacked elements.

```css
--z-dropdown: 1000       /* Dropdown menus */
--z-tooltip: 1010        /* Tooltips */
--z-modal: 1020          /* Modal dialogs */
--z-overlay: 1030        /* Modal overlays */
--z-notification: 1040   /* Toast notifications */
```

---

## Component Tokens

Pre-composed tokens for common component patterns.

### Buttons

```css
--button-padding-sm: var(--spacing-xs) var(--spacing-sm)
--button-padding-md: var(--spacing-sm) var(--spacing-md)
--button-padding-lg: var(--spacing-md) var(--spacing-lg)
--button-border-radius: var(--radius-md)
```

### Forms & Inputs

```css
--input-padding: var(--spacing-sm) var(--spacing-md)
--input-border-radius: var(--radius-md)
--input-border-width: 1px
```

### Cards

```css
--card-padding: var(--spacing-lg)
--card-border-radius: var(--radius-lg)
--card-border: 1px solid var(--border-light)
--card-shadow: var(--shadow-sm)
```

### Modals

```css
--modal-overlay-background: rgba(0, 0, 0, 0.5)
--modal-border-radius: var(--radius-lg)
--modal-shadow: var(--shadow-xl)
```

### Tables

```css
--table-border: 1px solid var(--border-light)
--table-stripe-background: var(--background-light)
```

---

## Usage Examples

### Creating a Button
```css
.custom-button {
    padding: var(--button-padding-md);
    background: var(--primary-color);
    color: var(--text-white);
    border-radius: var(--button-border-radius);
    transition: all var(--transition-fast);
    box-shadow: var(--shadow-sm);
}

.custom-button:hover {
    background: var(--primary-color-light);
    box-shadow: var(--shadow-md);
}
```

### Creating a Card
```css
.info-card {
    padding: var(--card-padding);
    background: var(--background-color);
    border: var(--card-border);
    border-radius: var(--card-border-radius);
    box-shadow: var(--card-shadow);
}
```

### Status Messages
```css
.success-message {
    background: var(--success-color-bg);
    border-left: 4px solid var(--success-color);
    padding: var(--spacing-md);
    border-radius: var(--radius-lg);
}
```

### Gradients
```css
.success-gradient-header {
    background: linear-gradient(135deg,
        var(--success-gradient-start) 0%,
        var(--success-gradient-end) 100%);
    color: var(--text-white);
}
```

---

## Best Practices

### ✅ DO:
- Always use design tokens instead of hardcoded values
- Use semantic color names (`--success-color` not `--green`)
- Maintain consistent spacing using the spacing scale
- Use the elevation system (shadows) for visual hierarchy

### ❌ DON'T:
- Don't hardcode hex colors (`#fff` → `var(--background-color)`)
- Don't use arbitrary spacing values (`padding: 13px` → `padding: var(--spacing-md)`)
- Don't create custom shadows (use the predefined scale)
- Don't bypass the design system for "one-off" styles

### Migration Tips
```css
/* Before */
.old-style {
    color: #374151;
    padding: 16px;
    border-radius: 8px;
    background: #f9fafb;
}

/* After */
.new-style {
    color: var(--text-color);
    padding: var(--spacing-md);
    border-radius: var(--radius-lg);
    background: var(--background-light);
}
```

---

## Dark Mode Support

The design system includes support for dark mode through CSS variable overrides:

```css
@media (prefers-color-scheme: dark) {
    :root {
        --background-color: #1a1a1a;
        --background-light: #2a2a2a;
        --text-color: #ffffff;
        --text-muted: #adb5bd;
        --border-color: #404040;
    }
}
```

---

## Maintenance

When updating the design system:
1. Update `shared-variables.css` with new tokens
2. Document changes in this file
3. Run a global search to replace hardcoded values
4. Test in both light and dark modes
5. Update component library examples

---

## Resources

- **Variables File**: `wwwroot/css/shared-variables.css`
- **CSS Architecture**: `CSS_ARCHITECTURE.md`
- **Component Library**: `wwwroot/css/components.css`

**Last Updated**: 2025-10-20
**Version**: 1.0.0
