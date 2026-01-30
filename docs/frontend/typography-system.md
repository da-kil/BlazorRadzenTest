# Typography System Implementation Guide

## Overview

The BeachBreak application uses a consolidated typography system with semantic CSS variables to ensure consistent font styling across all components.

---

## Font Weight Architecture

### Base Font System
- **Font Family**: Roboto only (`font-family: 'Roboto', sans-serif`)
- **Default Body Weight**: 300 (Roboto Light)
- **Default Heading Weight**: 500 (Roboto Medium)

### Font Weight Variables

#### Base Weights
- `--font-weight-light: 300` - Roboto Light
- `--font-weight-regular: 400` - Roboto Regular
- `--font-weight-medium: 500` - Roboto Medium
- `--font-weight-semibold: 600` - Roboto Semibold

#### Semantic Aliases (Preferred)
- `--font-weight-body: 300` - Body text default
- `--font-weight-heading: 500` - All headings (h1-h6)
- `--font-weight-emphasis: 500` - Emphasized text
- `--font-weight-strong: 600` - Strong emphasis

#### Component-Specific Variables
- `--font-weight-card-title: 500` - Card headers
- `--font-weight-section-title: 500` - Section headings
- `--font-weight-form-label: 500` - Form labels
- `--font-weight-badge: 500` - Badge text
- `--font-weight-button: 500` - Button text
- `--font-weight-nav-item: 500` - Navigation items

## Usage Guidelines

### Preferred Approach

**✅ Use semantic variables**:
```css
.my-heading {
    font-weight: var(--font-weight-heading);
}

.my-body-text {
    font-weight: var(--font-weight-body);
}
```

**❌ Never hardcode values**:
```css
.my-heading {
    font-weight: 500; /* BAD */
}
```

### Component Integration

**In Razor components**:
```razor
<RadzenText style="font-weight: var(--font-weight-emphasis)">
    Important text
</RadzenText>
```

**In CSS classes**:
```css
.section-title {
    font-weight: var(--font-weight-section-title);
    font-family: 'Roboto', sans-serif;
}
```

---

## Design System Integration

### CSS Variable Structure

The typography system is part of the broader design system that includes:
- Color variables
- Spacing variables
- Font weight variables (this system)
- Border radius variables
- Shadow variables

### Maintenance

**Adding New Variables**:
1. Define in main CSS file with semantic naming
2. Document in this guide
3. Update component CSS to use new variables
4. Test across all components for consistency

**Changing Existing Values**:
1. Update variable definition (changes apply globally)
2. Test visual impact across entire application
3. Verify accessibility compliance (contrast ratios)
4. Update documentation if semantic meaning changes

---

## References

- **CLAUDE.md**: Core typography pattern rule
- **docs/frontend/component-architecture.md**: Component-specific styling patterns
- **DesignSystem.md**: Broader design system guidelines

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*