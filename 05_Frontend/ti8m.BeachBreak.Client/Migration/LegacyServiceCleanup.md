# Legacy Service Cleanup Migration Guide

## Services to Remove After Full Migration

### 1. Role-specific Services (REMOVE)
- ✅ `HRQuestionnaireService.cs`
- ✅ `ManagerQuestionnaireService.cs`
- ✅ `EmployeeQuestionnaireService.cs`

### 2. Interface Files (REMOVE)
- ✅ `IHRQuestionnaireService.cs`
- ✅ `IManagerQuestionnaireService.cs`
- ✅ `IEmployeeQuestionnaireService.cs`

## Replacement Architecture

### Unified Services
- ✅ `IUserContextService` - Centralized authentication & permissions
- ✅ `IApiClientService` - Unified HTTP operations
- ✅ `IQuestionnaireService` - All questionnaire operations

### Benefits After Cleanup
- ~70% reduction in service code
- Single point of authentication
- Consistent error handling
- Role-based security built-in
- Easier testing and maintenance

## Migration Status

### Pages Migrated
- ✅ `MyQuestionnaires.razor`
- ✅ `DynamicQuestionnaire.razor`

### Pages Still Using Legacy Services
- ❓ Check for any other pages using old services
- ❓ Update any remaining components

## Final Cleanup Steps

1. ✅ Remove old service files
2. ✅ Update DI registration
3. ❓ Verify no references remain
4. ❓ Update backend API endpoints
5. ❓ Run full testing suite

## Backend Consolidation (Future)

Replace multiple endpoints:
```
OLD:
- /hr/assignments
- /managers/{id}/assignments
- /employees/{id}/assignments

NEW:
- /api/questionnaires/assignments?role={role}&context={context}
```

This will complete the architectural cleanup.