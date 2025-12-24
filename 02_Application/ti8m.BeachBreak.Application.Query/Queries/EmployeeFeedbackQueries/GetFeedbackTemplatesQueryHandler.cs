using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Handler for GetFeedbackTemplatesQuery that provides feedback source type metadata.
/// Returns the fixed system-level source types (Customer, Peer, Project Colleague) with their validation requirements.
/// Templates are fully customizable - users build their own questions without predefined criteria.
/// </summary>
public class GetFeedbackTemplatesQueryHandler : IQueryHandler<GetFeedbackTemplatesQuery, Result<FeedbackTemplatesResponse>>
{
    public async Task<Result<FeedbackTemplatesResponse>> HandleAsync(GetFeedbackTemplatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var response = new FeedbackTemplatesResponse
            {
                SourceTypeOptions = CreateSourceTypeOptions()
            };

            // Filter by source type if specified
            if (request.SourceType.HasValue)
            {
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
    /// Creates source type options - the fixed system-level feedback source types.
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