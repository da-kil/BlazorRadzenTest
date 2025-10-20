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
    private readonly IQuestionnaireTemplateService _templateService;

    public EmployeeQuestionnaireDataService(
        IEmployeeQuestionnaireService employeeService,
        IQuestionnaireTemplateService templateService)
    {
        _employeeService = employeeService;
        _templateService = templateService;
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
        return await _templateService.GetAllTemplatesAsync();
    }

    public async Task<object?> GetAdditionalDataAsync()
    {
        return null; // No additional data needed for employee view
    }
}

public class ManagerQuestionnaireDataService : IQuestionnaireDataService
{
    private readonly IManagerQuestionnaireService _managerService;
    private readonly IQuestionnaireTemplateService _templateService;

    public ManagerQuestionnaireDataService(
        IManagerQuestionnaireService managerService,
        IQuestionnaireTemplateService templateService)
    {
        _managerService = managerService;
        _templateService = templateService;
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
        return await _templateService.GetAllTemplatesAsync();
    }

    public async Task<object?> GetAdditionalDataAsync()
    {
        return await _managerService.GetTeamAnalyticsAsync();
    }
}

public class HRQuestionnaireDataService : IQuestionnaireDataService
{
    private readonly IHRQuestionnaireService _hrService;
    private readonly IQuestionnaireTemplateService _templateService;
    private readonly List<Organization>? _organizations;

    public HRQuestionnaireDataService(
        IHRQuestionnaireService hrService,
        IQuestionnaireTemplateService templateService,
        List<Organization>? organizations = null)
    {
        _hrService = hrService;
        _templateService = templateService;
        _organizations = organizations;
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
        return await _templateService.GetAllTemplatesAsync();
    }

    public async Task<object?> GetAdditionalDataAsync()
    {
        // Return organizations if available, otherwise return analytics
        if (_organizations != null && _organizations.Any())
        {
            return _organizations;
        }
        return await _hrService.GetOrganizationAnalyticsAsync();
    }
}