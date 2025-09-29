using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class OrganizationApiService : BaseApiService, IOrganizationApiService
{
    private const string BaseEndpoint = "q/api/v1/organizations";

    public OrganizationApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<List<Organization>> GetAllOrganizationsAsync(bool includeDeleted = false, bool includeIgnored = false)
    {
        var queryString = $"includeDeleted={includeDeleted}&includeIgnored={includeIgnored}";
        return await GetAllAsync<Organization>(BaseEndpoint, queryString);
    }

    public async Task<Organization?> GetOrganizationByIdAsync(Guid id)
    {
        return await GetByIdAsync<Organization>(BaseEndpoint, id);
    }

    public async Task<Organization?> GetOrganizationByNumberAsync(string number)
    {
        try
        {
            var endpoint = $"{BaseEndpoint}/by-number/{Uri.EscapeDataString(number)}";
            return await HttpQueryClient.GetFromJsonAsync<Organization>(endpoint);
        }
        catch (Exception ex)
        {
            LogError($"Error fetching organization with number {number}", ex);
            return null;
        }
    }

    public async Task<List<Organization>> GetOrganizationsByParentIdAsync(Guid parentId)
    {
        var queryString = $"parentId={parentId}";
        return await GetAllAsync<Organization>(BaseEndpoint, queryString);
    }
}