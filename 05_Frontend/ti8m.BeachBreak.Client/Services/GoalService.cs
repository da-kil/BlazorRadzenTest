using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service for Goal question type operations with strongly-typed DTOs.
/// Centralizes goal data parsing, validation, and response operations to prevent duplication.
/// </summary>
public class GoalService
{

    /// <summary>
    /// Validates a goal's data completeness and correctness.
    /// Note: Weighting can be 0 during in-progress states (will be set by manager during review).
    /// </summary>
    public bool IsGoalValid(GoalDataDto goalData)
    {
        if (goalData == null)
            return false;

        // Check required fields
        if (string.IsNullOrWhiteSpace(goalData.ObjectiveDescription))
            return false;

        if (string.IsNullOrWhiteSpace(goalData.MeasurementMetric))
            return false;

        // Validate dates
        if (goalData.TimeframeFrom >= goalData.TimeframeTo)
            return false;

        // Validate weighting - allow 0 for goals being added during in-progress
        // Weighting will be set by manager during InReview state
        if (goalData.WeightingPercentage < 0 || goalData.WeightingPercentage > 100)
            return false;

        return true;
    }

    /// <summary>
    /// Validates a predecessor goal rating's data completeness.
    /// </summary>
    public bool IsPredecessorRatingValid(PredecessorRatingDto ratingData)
    {
        if (ratingData == null)
            return false;

        // Check required fields - SourceGoalId is required
        if (ratingData.SourceGoalId == Guid.Empty)
            return false;

        // Validate degree of achievement range
        if (ratingData.DegreeOfAchievement < 0 || ratingData.DegreeOfAchievement > 100)
            return false;

        // Justification is required
        if (string.IsNullOrWhiteSpace(ratingData.Justification))
            return false;

        return true;
    }

    /// <summary>
    /// Validates if a question response for Goal type is complete.
    /// </summary>
    public bool IsGoalQuestionComplete(QuestionResponse response, bool requiresManagerReview)
    {
        if (response?.ResponseData is not GoalResponseDataDto goalData)
            return false;

        // Check if predecessor is linked (optional but if present, must have ratings)
        bool hasPredecessor = goalData.PredecessorAssignmentId.HasValue;

        if (hasPredecessor)
        {
            // If predecessor linked, must have ratings
            if (!goalData.PredecessorRatings.Any())
                return false;

            // All ratings must be valid
            if (!goalData.PredecessorRatings.All(r => IsPredecessorRatingValid(r)))
                return false;

            // If requires manager review, must have both employee and manager ratings
            if (requiresManagerReview)
            {
                bool hasEmployeeRating = goalData.PredecessorRatings.Any(r =>
                    r.RatedByRole == ApplicationRole.Employee);

                bool hasManagerRating = goalData.PredecessorRatings.Any(r =>
                    r.RatedByRole == ApplicationRole.TeamLead ||
                    r.RatedByRole == ApplicationRole.HR ||
                    r.RatedByRole == ApplicationRole.HRLead ||
                    r.RatedByRole == ApplicationRole.Admin);

                if (!hasEmployeeRating || !hasManagerRating)
                    return false;
            }
        }

        // Check if there are current goals
        if (!goalData.Goals.Any())
            return false;

        // All goals must be valid
        if (!goalData.Goals.All(g => IsGoalValid(g)))
            return false;

        // If requires manager review, must have goals from both roles
        if (requiresManagerReview)
        {
            bool hasEmployeeGoals = goalData.Goals.Any(g => g.AddedByRole == ApplicationRole.Employee);

            bool hasManagerGoals = goalData.Goals.Any(g =>
                g.AddedByRole == ApplicationRole.TeamLead ||
                g.AddedByRole == ApplicationRole.HR ||
                g.AddedByRole == ApplicationRole.HRLead ||
                g.AddedByRole == ApplicationRole.Admin);

            if (!hasEmployeeGoals || !hasManagerGoals)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Extracts the list of current goals from a question response.
    /// </summary>
    public List<GoalDataDto> GetGoals(QuestionResponse response)
    {
        if (response?.ResponseData is not GoalResponseDataDto goalData)
            return new List<GoalDataDto>();

        return goalData.Goals;
    }

    /// <summary>
    /// Extracts the list of predecessor goal ratings from a question response.
    /// </summary>
    public List<PredecessorRatingDto> GetPredecessorRatings(QuestionResponse response)
    {
        if (response?.ResponseData is not GoalResponseDataDto goalData)
            return new List<PredecessorRatingDto>();

        return goalData.PredecessorRatings;
    }

    /// <summary>
    /// Gets the predecessor assignment ID if linked, null otherwise.
    /// </summary>
    public Guid? GetPredecessorAssignmentId(QuestionResponse response)
    {
        if (response?.ResponseData is not GoalResponseDataDto goalData)
            return null;

        return goalData.PredecessorAssignmentId;
    }

    /// <summary>
    /// Creates a new goal data with strongly-typed structure.
    /// </summary>
    public GoalDataDto CreateGoalData(
        Guid goalId,
        string objective,
        DateTime fromDate,
        DateTime toDate,
        string measurement,
        decimal weighting,
        ApplicationRole addedByRole)
    {
        return new GoalDataDto
        {
            GoalId = goalId,
            ObjectiveDescription = objective,
            TimeframeFrom = fromDate,
            TimeframeTo = toDate,
            MeasurementMetric = measurement,
            WeightingPercentage = weighting,
            AddedByRole = addedByRole
        };
    }

    /// <summary>
    /// Creates a new predecessor rating data with strongly-typed structure.
    /// </summary>
    public PredecessorRatingDto CreatePredecessorRatingData(
        Guid sourceGoalId,
        decimal degree,
        string justification,
        ApplicationRole ratedByRole,
        string originalObjective,
        ApplicationRole originalAddedByRole)
    {
        return new PredecessorRatingDto
        {
            SourceGoalId = sourceGoalId,
            DegreeOfAchievement = (int)degree,
            Justification = justification,
            RatedByRole = ratedByRole,
            OriginalObjective = originalObjective,
            OriginalAddedByRole = originalAddedByRole
        };
    }
}
