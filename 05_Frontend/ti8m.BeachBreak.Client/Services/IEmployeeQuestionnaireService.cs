using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IEmployeeQuestionnaireService
{
    Task<List<QuestionnaireAssignment>> GetMyAssignmentsAsync();
    Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid assignmentId);
    Task<QuestionnaireResponse?> GetMyResponseAsync(Guid assignmentId);
    Task<QuestionnaireResponse> SaveMyResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);
    Task<QuestionnaireResponse?> SubmitMyResponseAsync(Guid assignmentId);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status);
    Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentId);
    Task<List<AssignmentProgress>> GetAllAssignmentProgressAsync();
}

public class AssignmentProgress
{
    public Guid AssignmentId { get; set; }
    public int ProgressPercentage { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? TimeSpent { get; set; }
}