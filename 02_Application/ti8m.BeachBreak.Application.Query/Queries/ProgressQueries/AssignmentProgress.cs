namespace ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;

public class AssignmentProgress
{
    public Guid AssignmentId { get; set; }
    public Guid TemplateId { get; set; }
    public int ProgressPercentage { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? TimeSpent { get; set; }
}