using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public record QuestionnairePdfData(
    QuestionnaireAssignmentDto Assignment,
    QuestionnaireResponseDto Response,
    QuestionnaireTemplateDto Template,
    DateTime GeneratedAt,
    Language Language = Language.English);
