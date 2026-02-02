namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Validates goal form data for Add and Edit operations.
/// Centralizes validation logic to avoid duplication between dialogs.
/// </summary>
public static class GoalFormValidator
{
    /// <summary>
    /// Validates goal form fields and returns a list of validation errors.
    /// </summary>
    /// <param name="objectiveDescription">The goal's objective description</param>
    /// <param name="measurementMetric">How the goal will be measured</param>
    /// <param name="timeframeFrom">Start date of the goal</param>
    /// <param name="timeframeTo">End date of the goal</param>
    /// <param name="isInReviewMode">Whether the form is in review mode (requires weighting)</param>
    /// <param name="weightingPercentage">Optional weighting percentage (required in review mode)</param>
    /// <returns>List of validation error messages (empty if valid)</returns>
    public static List<string> ValidateGoalDetails(
        string objectiveDescription,
        string measurementMetric,
        DateTime? timeframeFrom,
        DateTime? timeframeTo,
        bool isInReviewMode = false,
        decimal? weightingPercentage = null)
    {
        var errors = new List<string>();

        // Objective description validation
        if (string.IsNullOrWhiteSpace(objectiveDescription))
        {
            errors.Add("Objective description is required");
        }
        else if (objectiveDescription.Length < 5)
        {
            errors.Add("Objective description must be at least 10 characters");
        }

        // Measurement metric validation
        if (string.IsNullOrWhiteSpace(measurementMetric))
        {
            errors.Add("Measurement metric is required");
        }
        else if (measurementMetric.Length < 3)
        {
            errors.Add("Measurement metric must be at least 5 characters");
        }

        // Timeframe validation
        if (!timeframeFrom.HasValue)
        {
            errors.Add("Start date is required");
        }

        if (!timeframeTo.HasValue)
        {
            errors.Add("End date is required");
        }

        if (timeframeFrom.HasValue && timeframeTo.HasValue && timeframeTo.Value <= timeframeFrom.Value)
        {
            errors.Add("End date must be after start date");
        }

        // InReview mode validation (weighting)
        if (isInReviewMode)
        {
            if (!weightingPercentage.HasValue || weightingPercentage.Value <= 0)
            {
                errors.Add("Weighting percentage must be greater than 0");
            }

            if (weightingPercentage.HasValue && weightingPercentage.Value > 100)
            {
                errors.Add("Weighting percentage cannot exceed 100%");
            }
        }

        return errors;
    }
}
