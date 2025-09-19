namespace ti8m.BeachBreak.Application.Query.Queries.HRQueries;

public class HRDepartmentAssignmentListQuery : IQuery<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>
{
    public string Department { get; set; }

    public HRDepartmentAssignmentListQuery(string department)
    {
        Department = department;
    }
}