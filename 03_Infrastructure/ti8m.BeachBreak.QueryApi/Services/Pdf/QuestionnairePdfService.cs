using ti8m.BeachBreak.Application.Query.Services;
using AppLanguage = ti8m.BeachBreak.Application.Query.Models.Language;
using PdfLanguage = ti8m.BeachBreak.Core.Domain.QuestionConfiguration.Language;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public class QuestionnairePdfService : IQuestionnairePdfService
{
    private readonly IUITranslationService translationService;

    public QuestionnairePdfService(IUITranslationService translationService)
    {
        this.translationService = translationService;
    }

    public async Task<byte[]> GeneratePdfAsync(QuestionnairePdfData data, CancellationToken ct = default)
    {
        var labels = await LoadLabelsAsync(data.Language, ct);
        var document = new QuestionnairePdfDocument(data, labels);
        return document.GeneratePdf();
    }

    private async Task<PdfLabels> LoadLabelsAsync(PdfLanguage language, CancellationToken ct)
    {
        var translations = await translationService.GetTranslationsByCategoryAsync("pdf", ct);
        var appLanguage = (AppLanguage)(int)language;
        var lookup = translations.ToDictionary(t => t.Key, t => t.GetText(appLanguage));

        return new PdfLabels(
            AssignmentDetails: Get(lookup, "pdf.assignment-details"),
            Manager: Get(lookup, "pdf.manager"),
            Email: Get(lookup, "pdf.email"),
            Role: Get(lookup, "pdf.role"),
            Organisation: Get(lookup, "pdf.organisation"),
            Assigned: Get(lookup, "pdf.assigned"),
            Due: Get(lookup, "pdf.due"),
            Finalized: Get(lookup, "pdf.finalized"),
            FinalizedBy: Get(lookup, "pdf.finalized-by"),
            GeneratedPrefix: Get(lookup, "pdf.generated-prefix"),
            ScalePrefix: Get(lookup, "pdf.scale-prefix"),
            Competency: Get(lookup, "pdf.competency"),
            EmployeeAbbr: Get(lookup, "pdf.employee-abbr"),
            ManagerAbbr: Get(lookup, "pdf.manager-abbr"),
            EmployeeComment: Get(lookup, "pdf.employee-comment"),
            ManagerComment: Get(lookup, "pdf.manager-comment"),
            NoAssessmentItems: Get(lookup, "pdf.no-assessment-items"),
            NoResponses: Get(lookup, "pdf.no-responses"),
            EmployeeLabel: Get(lookup, "pdf.employee-label"),
            ManagerLabel: Get(lookup, "pdf.manager-label"),
            CurrentGoals: Get(lookup, "pdf.current-goals"),
            NoGoals: Get(lookup, "pdf.no-goals"),
            Objective: Get(lookup, "pdf.objective"),
            From: Get(lookup, "pdf.from"),
            To: Get(lookup, "pdf.to"),
            Weight: Get(lookup, "pdf.weight"),
            AddedBy: Get(lookup, "pdf.added-by"),
            PredecessorRatings: Get(lookup, "pdf.predecessor-ratings"),
            DegreeOfAchievement: Get(lookup, "pdf.degree-of-achievement"),
            Justification: Get(lookup, "pdf.justification"),
            RatedBy: Get(lookup, "pdf.rated-by"),
            InReviewNotes: Get(lookup, "pdf.in-review-notes"),
            SectionPrefix: Get(lookup, "pdf.section-prefix"),
            ReviewSummary: Get(lookup, "pdf.review-summary"),
            ManagerFinalNotes: Get(lookup, "pdf.manager-final-notes"),
            SignOff: Get(lookup, "pdf.sign-off"),
            Employee: Get(lookup, "pdf.employee"),
            Name: Get(lookup, "pdf.name"),
            Date: Get(lookup, "pdf.date"),
            Signature: Get(lookup, "pdf.signature"));
    }

    private static string Get(Dictionary<string, string> lookup, string key) =>
        lookup.TryGetValue(key, out var value) ? value : key;
}
