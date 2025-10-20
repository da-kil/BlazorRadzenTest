namespace ti8m.BeachBreak.CommandApi.Dto;

public class QuestionnaireTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool RequiresManagerReview { get; set; } = true;

    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }        // First publish timestamp
    public DateTime? LastPublishedDate { get; set; }    // Most recent publish
    public Guid? PublishedByEmployeeId { get; set; }    // Employee who published it

    public List<QuestionSectionDto> Sections { get; set; } = new();
}

public enum TemplateStatus
{
    Draft = 0,      // Template can be edited, not assignable
    Published = 1,  // Template is read-only, can be assigned
    Archived = 2    // Template is inactive, cannot be assigned or edited
}
