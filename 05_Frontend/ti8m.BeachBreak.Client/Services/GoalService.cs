using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service for Goal question type operations.
/// Centralizes goal data parsing, validation, and response key format to prevent duplication.
/// </summary>
public class GoalService
{
    // Current goals keys (goals added during in-progress)
    public const string GoalsKey = "Goals";
    public const string GoalIdKey = "GoalId";
    public const string GoalObjectiveKey = "ObjectiveDescription";
    public const string GoalTimeframeFromKey = "TimeframeFrom";
    public const string GoalTimeframeToKey = "TimeframeTo";
    public const string GoalMeasurementKey = "MeasurementMetric";
    public const string GoalWeightingKey = "WeightingPercentage";
    public const string GoalAddedByRoleKey = "AddedByRole";

    // Predecessor ratings keys (rating goals from previous questionnaire)
    public const string PredecessorAssignmentIdKey = "PredecessorAssignmentId";
    public const string PredecessorRatingsKey = "PredecessorRatings";
    public const string RatingSourceGoalIdKey = "SourceGoalId";
    public const string RatingDegreeKey = "DegreeOfAchievement";
    public const string RatingJustificationKey = "Justification";
    public const string RatingByRoleKey = "RatedByRole";
    public const string RatingOriginalObjectiveKey = "OriginalObjective";
    public const string RatingOriginalAddedByRoleKey = "OriginalAddedByRole";

    /// <summary>
    /// Validates a goal's data completeness and correctness.
    /// Note: Weighting can be 0 or null during in-progress states (will be set by manager during review).
    /// </summary>
    public bool IsGoalValid(Dictionary<string, object> goalData)
    {
        if (goalData == null || !goalData.Any())
            return false;

        // Check required fields
        if (!goalData.ContainsKey(GoalObjectiveKey) ||
            string.IsNullOrWhiteSpace(goalData[GoalObjectiveKey]?.ToString()))
            return false;

        if (!goalData.ContainsKey(GoalMeasurementKey) ||
            string.IsNullOrWhiteSpace(goalData[GoalMeasurementKey]?.ToString()))
            return false;

        // Validate dates
        if (!goalData.ContainsKey(GoalTimeframeFromKey) ||
            !goalData.ContainsKey(GoalTimeframeToKey))
            return false;

        if (goalData[GoalTimeframeFromKey] is DateTime fromDate &&
            goalData[GoalTimeframeToKey] is DateTime toDate)
        {
            if (fromDate >= toDate)
                return false;
        }
        else
        {
            return false; // Dates must be DateTime
        }

        // Validate weighting - allow 0 or null during in-progress (will be set by manager later)
        if (!goalData.ContainsKey(GoalWeightingKey))
            return true; // Weighting is optional during goal creation

        if (goalData[GoalWeightingKey] is decimal weighting)
        {
            // Allow 0 for goals being added during in-progress
            // Weighting will be set by manager during InReview state
            if (weighting < 0 || weighting > 100)
                return false;
        }
        else
        {
            // If weighting exists but is not decimal, it's invalid
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates a predecessor goal rating's data completeness.
    /// </summary>
    public bool IsPredecessorRatingValid(Dictionary<string, object> ratingData)
    {
        if (ratingData == null || !ratingData.Any())
            return false;

        // Check required fields
        if (!ratingData.ContainsKey(RatingSourceGoalIdKey))
            return false;

        if (!ratingData.ContainsKey(RatingDegreeKey))
            return false;

        if (ratingData[RatingDegreeKey] is decimal degree)
        {
            if (degree < 0 || degree > 100)
                return false;
        }
        else
        {
            return false;
        }

        if (!ratingData.ContainsKey(RatingJustificationKey) ||
            string.IsNullOrWhiteSpace(ratingData[RatingJustificationKey]?.ToString()))
            return false;

        return true;
    }

    /// <summary>
    /// Validates if a question response for Goal type is complete.
    /// </summary>
    public bool IsGoalQuestionComplete(QuestionResponse response, bool requiresManagerReview)
    {
        if (response?.ComplexValue == null || !response.ComplexValue.Any())
            return false;

        // Check if predecessor is linked (optional but if present, must have ratings)
        bool hasPredecessor = response.ComplexValue.ContainsKey(PredecessorAssignmentIdKey);

        if (hasPredecessor)
        {
            // If predecessor linked, must have ratings
            if (!response.ComplexValue.ContainsKey(PredecessorRatingsKey))
                return false;

            var ratings = GetPredecessorRatings(response);
            if (!ratings.Any())
                return false;

            // All ratings must be valid
            if (!ratings.All(r => IsPredecessorRatingValid(r)))
                return false;

            // If requires manager review, must have both employee and manager ratings
            if (requiresManagerReview)
            {
                bool hasEmployeeRating = ratings.Any(r =>
                    r.ContainsKey(RatingByRoleKey) &&
                    r[RatingByRoleKey]?.ToString() == "Employee");

                bool hasManagerRating = ratings.Any(r =>
                    r.ContainsKey(RatingByRoleKey) &&
                    r[RatingByRoleKey]?.ToString() == "Manager");

                if (!hasEmployeeRating || !hasManagerRating)
                    return false;
            }
        }

        // Check if there are current goals
        if (!response.ComplexValue.ContainsKey(GoalsKey))
            return false;

        var goals = GetGoals(response);
        if (!goals.Any())
            return false;

        // All goals must be valid
        if (!goals.All(g => IsGoalValid(g)))
            return false;

        // If requires manager review, must have goals from both roles
        if (requiresManagerReview)
        {
            bool hasEmployeeGoals = goals.Any(g =>
                g.ContainsKey(GoalAddedByRoleKey) &&
                g[GoalAddedByRoleKey]?.ToString() == "Employee");

            bool hasManagerGoals = goals.Any(g =>
                g.ContainsKey(GoalAddedByRoleKey) &&
                g[GoalAddedByRoleKey]?.ToString() == "Manager");

            if (!hasEmployeeGoals || !hasManagerGoals)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Extracts the list of current goals from a question response.
    /// </summary>
    public List<Dictionary<string, object>> GetGoals(QuestionResponse response)
    {
        if (response?.ComplexValue == null || !response.ComplexValue.ContainsKey(GoalsKey))
            return new List<Dictionary<string, object>>();

        var goalsValue = response.ComplexValue[GoalsKey];

        if (goalsValue is List<Dictionary<string, object>> goalsList)
            return goalsList;

        // Try to deserialize from JSON if needed
        if (goalsValue is System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var deserialized = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                    jsonElement.GetRawText(),
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return deserialized ?? new List<Dictionary<string, object>>();
            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }

        return new List<Dictionary<string, object>>();
    }

    /// <summary>
    /// Extracts the list of predecessor goal ratings from a question response.
    /// </summary>
    public List<Dictionary<string, object>> GetPredecessorRatings(QuestionResponse response)
    {
        if (response?.ComplexValue == null || !response.ComplexValue.ContainsKey(PredecessorRatingsKey))
            return new List<Dictionary<string, object>>();

        var ratingsValue = response.ComplexValue[PredecessorRatingsKey];

        if (ratingsValue is List<Dictionary<string, object>> ratingsList)
            return ratingsList;

        // Try to deserialize from JSON if needed
        if (ratingsValue is System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var deserialized = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                    jsonElement.GetRawText(),
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return deserialized ?? new List<Dictionary<string, object>>();
            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }

        return new List<Dictionary<string, object>>();
    }

    /// <summary>
    /// Gets the predecessor assignment ID if linked, null otherwise.
    /// </summary>
    public Guid? GetPredecessorAssignmentId(QuestionResponse response)
    {
        if (response?.ComplexValue == null || !response.ComplexValue.ContainsKey(PredecessorAssignmentIdKey))
            return null;

        var value = response.ComplexValue[PredecessorAssignmentIdKey];

        if (value is Guid guidValue)
            return guidValue;

        if (Guid.TryParse(value?.ToString(), out var parsedGuid))
            return parsedGuid;

        return null;
    }

    /// <summary>
    /// Creates a new goal data dictionary with proper key format.
    /// </summary>
    public Dictionary<string, object> CreateGoalData(
        Guid goalId,
        string objective,
        DateTime fromDate,
        DateTime toDate,
        string measurement,
        decimal weighting,
        string addedByRole)
    {
        return new Dictionary<string, object>
        {
            [GoalIdKey] = goalId,
            [GoalObjectiveKey] = objective,
            [GoalTimeframeFromKey] = fromDate,
            [GoalTimeframeToKey] = toDate,
            [GoalMeasurementKey] = measurement,
            [GoalWeightingKey] = weighting,
            [GoalAddedByRoleKey] = addedByRole
        };
    }

    /// <summary>
    /// Creates a new predecessor rating data dictionary with proper key format.
    /// </summary>
    public Dictionary<string, object> CreatePredecessorRatingData(
        Guid sourceGoalId,
        decimal degree,
        string justification,
        string ratedByRole,
        string originalObjective)
    {
        return new Dictionary<string, object>
        {
            [RatingSourceGoalIdKey] = sourceGoalId,
            [RatingDegreeKey] = degree,
            [RatingJustificationKey] = justification,
            [RatingByRoleKey] = ratedByRole,
            [RatingOriginalObjectiveKey] = originalObjective
        };
    }
}
