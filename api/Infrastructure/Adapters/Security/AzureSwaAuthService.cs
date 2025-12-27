using api.Domain.Ports;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace api.Infrastructure.Adapters.Security;

public class AzureSwaAuthService : IAuthService
{
    private readonly bool _isDevelopment;

    public AzureSwaAuthService()
    {
        // Check if we're in development mode
        _isDevelopment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development"
                      || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }

    public string? GetUserIdFromRequest(HttpRequest request)
    {
        var principal = GetClientPrincipal(request);
        return principal?.UserId;
    }

    public ClientPrincipal? GetClientPrincipal(HttpRequest request)
    {
        var header = request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();

        if (string.IsNullOrEmpty(header))
        {
            // In local development, return a mock user if header is missing
            if (_isDevelopment)
            {
                return GetDevelopmentUser();
            }
            return null;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header));
            var principal = JsonSerializer.Deserialize<ClientPrincipal>(decoded, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return principal;
        }
        catch
        {
            // In local development, return a mock user if deserialization fails
            if (_isDevelopment)
            {
                return GetDevelopmentUser();
            }
            return null;
        }
    }

    private ClientPrincipal GetDevelopmentUser()
    {
        // Return a mock user for local development
        return new ClientPrincipal
        {
            UserId = "dev-user-123",
            UserDetails = "Development User",
            IdentityProvider = "google",
            Claims = new List<ClientPrincipalClaim>
            {
                new ClientPrincipalClaim
                {
                    Typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                    Val = "dev@example.com"
                },
                new ClientPrincipalClaim
                {
                    Typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                    Val = "Development User"
                }
            }
        };
    }
}
