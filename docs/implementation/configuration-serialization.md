# Configuration Serialization Implementation Guide

## Overview

The questionnaire configuration uses polymorphic JSON serialization with a `$type` discriminator. This document explains why the JSON contains both a section-level `Type` field and a configuration-level `$type` discriminator, and why this apparent redundancy is intentional defensive design.

---

## JSON Structure

```json
{
  "Type": 0,  // Section-level question type (QuestionType enum)
  "Configuration": {
    "$type": 0,  // Configuration-level discriminator (same value)
    "Evaluations": [...],
    "RatingScale": 4,
    "ScaleLowLabel": "Poor",
    "ScaleHighLabel": "Excellent"
  }
}
```

## Why Both Type and $type Exist

**1. Section.Type**: Semantic field indicating what type of question this section represents
   - Used by domain logic for validation and business rules
   - Part of the `QuestionSection` domain model
   - Accessed directly in code: `section.Type`

**2. Configuration.$type**: JSON discriminator enabling polymorphic deserialization of `IQuestionConfiguration`
   - Required by the JSON deserializer to determine concrete type
   - Maps to the appropriate configuration class:
     - `$type: 0` → `AssessmentConfiguration`
     - `$type: 1` → `TextQuestionConfiguration`
     - `$type: 2` → `GoalConfiguration`
   - Handled by `QuestionConfigurationJsonConverter`

## The Pattern is Intentional Defensive Design

The apparent redundancy is **intentional and necessary**:

1. **Validation Safety**: The two values must always match, validated by `ValidateConfigurationMatchesType()`

   This catches bugs where frontend and backend disagree on types.

2. **Separation of Concerns**: `IQuestionConfiguration` is used independently in many contexts without access to the parent section:
   - Domain events (`QuestionSectionData`)
   - Command DTOs (`CommandQuestionSection`)
   - Query DTOs (`QuestionSectionDto`)
   - Frontend models (`QuestionSection`)

   In these contexts, the Configuration object needs its own type information.

3. **Standard .NET Pattern**: Using a discriminator for polymorphic JSON follows .NET best practices:
   - Supported by System.Text.Json
   - Well-understood industry pattern
   - Tool support (Swagger, API testing)
   - Fast and unambiguous deserialization

4. **Similar to Other Safety Mechanisms**:
   - Database foreign key constraints
   - Email confirmation fields in forms
   - Checksums in data transmission

   The redundancy **prevents bugs** rather than creating them.

## QuestionConfigurationJsonConverter

**Location**:
- `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/QuestionConfigurationJsonConverter.cs`
- `05_Frontend/ti8m.BeachBreak.Client/Models/QuestionConfigurationJsonConverter.cs`

**Read Method** (Deserialization):
- Looks for `"$type"` property in JSON
- If found: uses it to determine the concrete type
- If NOT found: falls back to property inference (backward compatibility)

**Write Method** (Serialization):
- Always writes `"$type": (int)value.QuestionType` at the beginning
- Then writes type-specific properties

## Common Misconceptions

**❌ WRONG**: "The $type discriminator is redundant because we have Section.Type"

**✅ CORRECT**: The $type discriminator serves a different purpose than Section.Type:
- Section.Type is a domain field for business logic
- Configuration.$type is a JSON serialization mechanism
- Both are necessary and must match

**❌ WRONG**: "We can remove $type and just use Section.Type for deserialization"

**✅ CORRECT**: This would require:
- Custom converter logic coupling Section and Configuration
- Breaking isolated Configuration usage (events, DTOs)
- Loss of backward compatibility
- More complex deserialization code
- Going against .NET best practices

## Design Decision (2025-12-12)

This pattern was explicitly reviewed and the decision was made to **keep the current design**:

**Rationale**:
- Follows .NET best practices for polymorphic JSON
- Provides defensive validation safety
- Enables Configuration to be used independently
- Low cost (~10 bytes per section) vs high complexity of alternatives
- Backward compatible with property inference fallback

**Do not remove the $type discriminator** - it's not redundant, it's defensive validation.

## Historical Context

**Investigation Date**: 2025-12-12

During a review of the questionnaire template JSON structure, the apparent redundancy between `Type` and `$type` was questioned. A comprehensive investigation revealed:

1. The $type discriminator is necessary for polymorphic deserialization
2. The apparent redundancy is intentional defensive design
3. This pattern follows .NET best practices
4. Works correctly in CQRS/Event Sourcing architecture

The investigation confirmed the current implementation is correct and should be preserved.

## References

- JSON Converter: `QuestionConfigurationJsonConverter.cs` (Core.Domain and Client projects)
- Domain Validation: `QuestionSection.ValidateConfigurationMatchesType()`
- Pattern documentation: CLAUDE.md Section 11 "Strongly-Typed Question Configuration Pattern"

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*