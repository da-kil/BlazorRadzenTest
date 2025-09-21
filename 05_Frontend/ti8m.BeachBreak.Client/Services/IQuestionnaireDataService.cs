using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IQuestionnaireDataService
{
    Task<List<QuestionnaireAssignment>> GetAssignmentsAsync();
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<List<QuestionnaireTemplate>> GetTemplatesAsync();
    Task<object?> GetAdditionalDataAsync();
}

public class EmployeeQuestionnaireDataService : IQuestionnaireDataService
{
    private readonly IEmployeeQuestionnaireService _employeeService;
    private readonly IQuestionnaireApiService _questionnaireService;

    public EmployeeQuestionnaireDataService(
        IEmployeeQuestionnaireService employeeService,
        IQuestionnaireApiService questionnaireService)
    {
        _employeeService = employeeService;
        _questionnaireService = questionnaireService;
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsAsync()
    {
        return await _employeeService.GetMyAssignmentsAsync();
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        return new List<EmployeeDto>(); // Not applicable for employee view
    }

    public async Task<List<QuestionnaireTemplate>> GetTemplatesAsync()
    {
        return await _questionnaireService.GetAllTemplatesAsync();
    }

    public async Task<object?> GetAdditionalDataAsync()
    {
        return null; // No additional data needed for employee view
    }
}

public class ManagerQuestionnaireDataService : IQuestionnaireDataService
{
    private readonly IManagerQuestionnaireService _managerService;
    private readonly IQuestionnaireApiService _questionnaireService;

    public ManagerQuestionnaireDataService(
        IManagerQuestionnaireService managerService,
        IQuestionnaireApiService questionnaireService)
    {
        _managerService = managerService;
        _questionnaireService = questionnaireService;
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsAsync()
    {
        return await _managerService.GetTeamAssignmentsAsync();
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        return await _managerService.GetTeamMembersAsync();
    }

    public async Task<List<QuestionnaireTemplate>> GetTemplatesAsync()
    {
        return await _questionnaireService.GetAllTemplatesAsync();
    }

    public async Task<object?> GetAdditionalDataAsync()
    {
        return await _managerService.GetTeamAnalyticsAsync();
    }
}

public class HRQuestionnaireDataService : IQuestionnaireDataService
{
    private readonly IHRQuestionnaireService _hrService;
    private readonly IQuestionnaireApiService _questionnaireService;

    public HRQuestionnaireDataService(
        IHRQuestionnaireService hrService,
        IQuestionnaireApiService questionnaireService)
    {
        _hrService = hrService;
        _questionnaireService = questionnaireService;
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsAsync()
    {
        return await _hrService.GetAllAssignmentsAsync();
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        return await _hrService.GetAllEmployeesAsync();
    }

    public async Task<List<QuestionnaireTemplate>> GetTemplatesAsync()
    {
        return await _questionnaireService.GetAllTemplatesAsync();
    }

    public async Task<object?> GetAdditionalDataAsync()
    {
        return await _hrService.GetOrganizationAnalyticsAsync();
    }
}