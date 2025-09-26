namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate;

public enum TemplateStatus
{
    Draft = 0,      // Template can be edited, not assignable
    Published = 1,  // Template is read-only, can be assigned if active
    Archived = 2    // Template is inactive, cannot be assigned or edited
}