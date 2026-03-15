using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Mappers;

public static class PdfExportMapper
{
    public static QuestionnaireAssignmentDto MapAssignmentToDto(
        Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment a)
    {
        return new QuestionnaireAssignmentDto
        {
            Id = a.Id,
            TemplateId = a.TemplateId,
            ProcessType = EnumConverter.MapToProcessType(a.ProcessType),
            EmployeeId = a.EmployeeId.ToString(),
            EmployeeName = a.EmployeeName,
            EmployeeEmail = a.EmployeeEmail,
            EmployeeRole = a.EmployeeRole,
            EmployeeOrganisationName = a.EmployeeOrganisationName,
            AssignedDate = a.AssignedDate,
            DueDate = a.DueDate,
            StartedDate = a.StartedDate,
            CompletedDate = a.CompletedDate,
            TemplateName = a.TemplateName,
            TemplateCategoryId = a.TemplateCategoryId,
            WorkflowState = a.WorkflowState,
            ManagerSubmittedByEmployeeName = a.ManagerSubmittedByEmployeeName,
            EmployeeSubmittedDate = a.EmployeeSubmittedDate,
            EmployeeSubmittedByEmployeeName = a.EmployeeSubmittedByEmployeeName,
            EmployeeReviewConfirmedDate = a.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedByEmployeeName = a.EmployeeReviewConfirmedByEmployeeName,
            ManagerReviewSummary = a.ManagerReviewSummary,
            FinalizedDate = a.FinalizedDate,
            FinalizedByEmployeeId = a.FinalizedByEmployeeId,
            FinalizedByEmployeeName = a.FinalizedByEmployeeName,
            ManagerFinalNotes = a.ManagerFinalNotes,
            IsLocked = a.IsLocked,
            InReviewNotes = a.InReviewNotes.Select(n => new InReviewNoteDto
            {
                Id = n.Id,
                Content = n.Content,
                Timestamp = n.Timestamp,
                SectionId = n.SectionId,
                SectionTitle = n.SectionTitle,
                AuthorEmployeeId = n.AuthorEmployeeId,
                AuthorName = n.AuthorName
            }).ToList()
        };
    }

    public static QuestionnaireResponseDto MapResponseToDto(
        Application.Query.Queries.ResponseQueries.QuestionnaireResponse r)
    {
        return new QuestionnaireResponseDto
        {
            Id = r.Id,
            AssignmentId = r.AssignmentId,
            TemplateId = r.TemplateId,
            EmployeeId = r.EmployeeId.ToString(),
            StartedDate = r.StartedDate,
            SectionResponses = MapSectionResponses(r.SectionResponses)
        };
    }

    public static QuestionnaireResponseDto BuildEmptyResponseDto(Guid assignmentId, Guid employeeId, Guid templateId) =>
        new()
        {
            AssignmentId = assignmentId,
            TemplateId = templateId,
            EmployeeId = employeeId.ToString(),
            SectionResponses = new Dictionary<Guid, SectionResponseDto>()
        };

    public static Dictionary<Guid, SectionResponseDto> MapSectionResponses(
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
    {
        var result = new Dictionary<Guid, SectionResponseDto>();

        foreach (var (sectionId, roleResponses) in sectionResponses)
        {
            var roleResponsesDto = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>();

            foreach (var (completionRole, responseValue) in roleResponses)
            {
                var responseRole = completionRole == CompletionRole.Manager ? ResponseRole.Manager : ResponseRole.Employee;

                roleResponsesDto[responseRole] = new Dictionary<Guid, QuestionResponseDto>
                {
                    [sectionId] = new QuestionResponseDto
                    {
                        QuestionId = sectionId,
                        QuestionType = QuestionResponseMapper.InferQuestionType(responseValue),
                        ResponseData = QuestionResponseMapper.MapToDto(responseValue)
                    }
                };
            }

            result[sectionId] = new SectionResponseDto
            {
                SectionId = sectionId,
                RoleResponses = roleResponsesDto
            };
        }

        return result;
    }

    public static QuestionnaireTemplateDto MapTemplateToDto(
        Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate t)
    {
        return new QuestionnaireTemplateDto
        {
            Id = t.Id,
            NameGerman = t.NameGerman,
            NameEnglish = t.NameEnglish,
            DescriptionGerman = t.DescriptionGerman,
            DescriptionEnglish = t.DescriptionEnglish,
            CategoryId = t.CategoryId,
            ProcessType = EnumConverter.MapToProcessType(t.ProcessType),
            IsCustomizable = t.IsCustomizable,
            AutoInitialize = t.AutoInitialize,
            CreatedDate = t.CreatedDate,
            Sections = t.Sections.Select(s => new QuestionSectionDto
            {
                Id = s.Id,
                TitleGerman = s.TitleGerman,
                TitleEnglish = s.TitleEnglish,
                DescriptionGerman = s.DescriptionGerman,
                DescriptionEnglish = s.DescriptionEnglish,
                Order = s.Order,
                CompletionRole = EnumConverter.MapToCompletionRole(s.CompletionRole),
                Type = EnumConverter.MapToQuestionType(s.Type),
                Configuration = s.Configuration
            }).ToList()
        };
    }

    public static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Replace(' ', '_');
    }
}
