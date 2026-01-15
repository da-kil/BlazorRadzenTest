using ti8m.BeachBreak.Application.Query.Projections.Models;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get feedback question data including all linked feedback records.
/// Used to display feedback sections during questionnaire workflow.
/// </summary>
public record GetFeedbackQuestionDataQuery(
    Guid AssignmentId,
    Guid QuestionId) : IQuery<Result<FeedbackQuestionDataDto>>;
