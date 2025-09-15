using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services;

public interface IEmployeeApiService
{
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id);
    Task<List<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
    Task<List<EmployeeDto>> SearchEmployeesAsync(string searchTerm);
}

public class EmployeeApiService : IEmployeeApiService
{
    private readonly HttpClient httpQueryClient;

    public EmployeeApiService(IHttpClientFactory factory)
    {
        httpQueryClient = factory.CreateClient("QueryClient");
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<EmployeeDto>>("q/api/v1/employees");
            return response ?? new List<EmployeeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching employees: {ex.Message}");
            return new List<EmployeeDto>();
        }
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            return await httpQueryClient.GetFromJsonAsync<EmployeeDto>($"q/api/v1/employees/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching employee {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<EmployeeDto>> GetEmployeesByDepartmentAsync(string department)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<EmployeeDto>>($"q/api/v1/employees?department={Uri.EscapeDataString(department)}");
            return response ?? new List<EmployeeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching employees by department {department}: {ex.Message}");
            return new List<EmployeeDto>();
        }
    }

    public async Task<List<EmployeeDto>> SearchEmployeesAsync(string searchTerm)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<EmployeeDto>>($"q/api/v1/employees?search={Uri.EscapeDataString(searchTerm)}");
            return response ?? new List<EmployeeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching employees with term {searchTerm}: {ex.Message}");
            return new List<EmployeeDto>();
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

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Department => Organization;
}