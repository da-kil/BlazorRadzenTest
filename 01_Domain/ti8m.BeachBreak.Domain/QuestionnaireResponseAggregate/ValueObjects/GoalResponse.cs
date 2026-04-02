namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for goal questions including goals and predecessor ratings.
    /// </summary>
    public sealed class GoalResponse : QuestionResponseValue
    {
        public IReadOnlyList<GoalData> Goals { get; }
        public IReadOnlyList<PredecessorRating> PredecessorRatings { get; }
        public Guid? PredecessorAssignmentId { get; }

        public GoalResponse(
            IReadOnlyList<GoalData> goals,
            IReadOnlyList<PredecessorRating> predecessorRatings,
            Guid? predecessorAssignmentId = null)
        {
            Goals = goals;
            PredecessorRatings = predecessorRatings;
            PredecessorAssignmentId = predecessorAssignmentId;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var goal in Goals)
                yield return goal;
            foreach (var rating in PredecessorRatings)
                yield return rating;
            yield return PredecessorAssignmentId;
        }
    }
}
