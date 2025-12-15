using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Handler for GetFeedbackTemplatesQuery that provides available feedback templates and criteria.
/// Returns default templates for Customer, Peer, and Project Colleague feedback sources.
/// </summary>
public class GetFeedbackTemplatesQueryHandler : IQueryHandler<GetFeedbackTemplatesQuery, Result<FeedbackTemplatesResponse>>
{
    public async Task<Result<FeedbackTemplatesResponse>> HandleAsync(GetFeedbackTemplatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var response = new FeedbackTemplatesResponse
            {
                DefaultTemplates = CreateDefaultTemplates(),
                AvailableCriteria = CreateAvailableCriteria(),
                StandardTextSections = CreateStandardTextSections(),
                SourceTypeOptions = CreateSourceTypeOptions()
            };

            // Filter by source type if specified
            if (request.SourceType.HasValue)
            {
                response.DefaultTemplates = response.DefaultTemplates
                    .Where(t => t.Key == request.SourceType.Value)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                response.SourceTypeOptions = response.SourceTypeOptions
                    .Where(o => o.Value == request.SourceType.Value)
                    .ToList();
            }

            return Result<FeedbackTemplatesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<FeedbackTemplatesResponse>.Fail($"Failed to get feedback templates: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Creates default templates for each feedback source type.
    /// </summary>
    private static Dictionary<int, EmployeeFeedbackConfiguration> CreateDefaultTemplates()
    {
        return new Dictionary<int, EmployeeFeedbackConfiguration>
        {
            { 0, EmployeeFeedbackConfiguration.CreateCustomerFeedbackDefault() }, // Customer
            { 1, EmployeeFeedbackConfiguration.CreatePeerFeedbackDefault() }, // Peer
            { 2, EmployeeFeedbackConfiguration.CreateProjectColleagueFeedbackDefault() } // Project Colleague
        };
    }

    /// <summary>
    /// Creates the full list of available evaluation criteria with keys only.
    /// Frontend components use @T("criteria.{key}") for localized display.
    /// </summary>
    private static List<EvaluationItem> CreateAvailableCriteria()
    {
        return new List<EvaluationItem>
        {
            new("overall_satisfaction", "", "", "", "", false, 0),
            new("leadership_behavior", "", "", "", "", false, 1),
            new("technical_methodological_skills", "", "", "", "", false, 2),
            new("commitment", "", "", "", "", false, 3),
            new("reliability", "", "", "", "", false, 4),
            new("teamwork", "", "", "", "", false, 5),
            new("quality_of_work", "", "", "", "", false, 6),
            new("communication_skills", "", "", "", "", false, 7),
            new("problem_solving", "", "", "", "", false, 8),
            new("innovation", "", "", "", "", false, 9),
            new("adaptability", "", "", "", "", false, 10),
            new("customer_orientation", "", "", "", "", false, 11)
        };
    }

    /// <summary>
    /// Creates standard text sections for unstructured feedback with keys only.
    /// Frontend components use @T("feedback-sections.{key}.title") and @T("feedback-sections.{key}.placeholder") for localized display.
    /// </summary>
    private static List<TextSectionDefinition> CreateStandardTextSections()
    {
        return new List<TextSectionDefinition>
        {
            new()
            {
                Key = "positive_impressions",
                TitleEnglish = "",
                TitleGerman = "",
                PlaceholderEnglish = "",
                PlaceholderGerman = "",
                IsRequired = false,
                Order = 0
            },
            new()
            {
                Key = "potential_for_improvement",
                TitleEnglish = "",
                TitleGerman = "",
                PlaceholderEnglish = "",
                PlaceholderGerman = "",
                IsRequired = false,
                Order = 1
            },
            new()
            {
                Key = "general_comments",
                TitleEnglish = "",
                TitleGerman = "",
                PlaceholderEnglish = "",
                PlaceholderGerman = "",
                IsRequired = false,
                Order = 2
            },
            new()
            {
                Key = "project_specific_feedback",
                TitleEnglish = "",
                TitleGerman = "",
                PlaceholderEnglish = "",
                PlaceholderGerman = "",
                IsRequired = false,
                Order = 3
            }
        };
    }

    /// <summary>
    /// Creates source type options with IDs only.
    /// Frontend components use @T("source-types.{type}.name") and @T("source-types.{type}.description") for localized display.
    /// </summary>
    private static List<SourceTypeOption> CreateSourceTypeOptions()
    {
        return new List<SourceTypeOption>
        {
            new(0, "", "", false, false), // Customer
            new(1, "", "", false, true),  // Peer
            new(2, "", "", true, true)    // Project Colleague
        };
    }
}