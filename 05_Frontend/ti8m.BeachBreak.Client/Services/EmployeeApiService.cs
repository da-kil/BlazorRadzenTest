using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services;

public interface IEmployeeApiService
{
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id);
    Task<List<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
    Task<List<EmployeeDto>> SearchEmployeesAsync(string searchTerm);
    Task<bool> ChangeApplicationRoleAsync(Guid employeeId, ApplicationRole newRole);
    Task<EmployeeDashboardDto?> GetMyDashboardAsync();
}

public class EmployeeApiService : BaseApiService, IEmployeeApiService
{
    private const string BaseEndpoint = "q/api/v1/employees";

    public EmployeeApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        return await GetAllAsync<EmployeeDto>(BaseEndpoint);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id)
    {
        return await GetByIdAsync<EmployeeDto>(BaseEndpoint, id);
    }

    public async Task<List<EmployeeDto>> GetEmployeesByDepartmentAsync(string department)
    {
        var queryString = $"department={Uri.EscapeDataString(department)}";
        return await GetAllAsync<EmployeeDto>(BaseEndpoint, queryString);
    }

    public async Task<List<EmployeeDto>> SearchEmployeesAsync(string searchTerm)
    {
        return await SearchAsync<EmployeeDto>(BaseEndpoint, searchTerm);
    }

    public async Task<bool> ChangeApplicationRoleAsync(Guid employeeId, ApplicationRole newRole)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync($"c/api/v1/employees/{employeeId}/application-role", new { NewRole = newRole });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error changing application role for employee {employeeId}", ex);
            return false;
        }
    }

    public async Task<EmployeeDashboardDto?> GetMyDashboardAsync()
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<EmployeeDashboardDto>($"{BaseEndpoint}/me/dashboard");
        }
        catch (Exception ex)
        {
            LogError("Error retrieving employee dashboard", ex);
            return null;
        }
    }
}

public class EmployeeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? LastStartDate { get; set; }
    public Guid? ManagerId { get; set; }
    public string Manager { get; set; } = string.Empty;
    public string LoginName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public int OrganizationNumber { get; set; }
    public string Organization { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public ApplicationRole ApplicationRole { get; set; } = ApplicationRole.Employee;

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Department => Organization;
}