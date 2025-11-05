using ti8m.BeachBreak.Application.Command.Services;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ModelsResponseRole = ti8m.BeachBreak.CommandApi.Models.ResponseRole;

namespace ti8m.BeachBreak.CommandApi.Services;

/// <summary>
/// Service for mapping between CommandApi DTOs and Domain value objects.
/// Centralizes conversion logic and eliminates Dictionary<string, object> casting.
/// </summary>
public class QuestionResponseMappingService
{

    /// <summary>
    /// Converts API DTOs to type-safe domain format for command handling.
    /// Centralizes the conversion logic used by both EmployeesController and ResponsesController.
    /// </summary>
    public Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> ConvertToTypeSafeFormat(
        Dictionary<Guid, SectionResponse> sectionResponses)
    {
        var typeSafeSectionResponses = new Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>>();

        foreach (var section in sectionResponses)
        {
            var sectionId = section.Key;
            var sectionResponse = section.Value;

            var roleResponses = new Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>();

            foreach (var roleResponse in sectionResponse.RoleResponses)
            {
                // Convert ResponseRole (API enum) to CompletionRole (domain enum)
                var completionRole = roleResponse.Key switch
                {
                    ModelsResponseRole.Employee => CompletionRole.Employee,
                    ModelsResponseRole.Manager => CompletionRole.Manager,
                    _ => throw new InvalidOperationException($"Unknown ResponseRole: {roleResponse.Key}")
                };

                // Convert QuestionResponse objects to QuestionResponseValue
                var questionResponses = new Dictionary<Guid, QuestionResponseValue>();
                foreach (var questionResponse in roleResponse.Value)
                {
                    var questionId = questionResponse.Key;
                    var response = questionResponse.Value;

                    // Convert from API QuestionResponse to domain QuestionResponseValue
                    var legacyFormat = new Dictionary<Guid, object> { { questionId, response.Value ?? new object() } };
                    var convertedResponses = QuestionResponseValueConverter.ConvertQuestionResponses(legacyFormat);

                    if (convertedResponses.TryGetValue(questionId, out var convertedResponse))
                    {
                        questionResponses[questionId] = convertedResponse;
                    }
                }

                roleResponses[completionRole] = questionResponses;
            }

            typeSafeSectionResponses[sectionId] = roleResponses;
        }

        return typeSafeSectionResponses;
    }
}