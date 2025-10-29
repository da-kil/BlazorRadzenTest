using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service for authentication and authorization operations.
/// Uses authenticated HTTP client to call backend API.
/// </summary>
public class AuthService : BaseApiService, IAuthService
{
    private const string AuthQueryEndpoint = "q/api/v1/auth";

    public AuthService(IHttpClientFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Gets the current user's application role and employee ID from backend.
    /// Uses authenticated HTTP client so the backend can identify the user.
    /// </summary>
    public async Task<UserRole?> GetMyRoleAsync()
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<UserRole>($"{AuthQueryEndpoint}/me/role");
            return response;
        }
        catch (Exception ex)
        {
            LogError("Error fetching user role", ex);
            return null;
        }
    }
}
