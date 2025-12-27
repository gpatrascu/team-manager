using Microsoft.AspNetCore.Http;

namespace api.Domain.Ports;

public interface IAuthService
{
    string? GetUserIdFromRequest(HttpRequest request);
    ClientPrincipal? GetClientPrincipal(HttpRequest request);
}

public class ClientPrincipal
{
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public string? IdentityProvider { get; set; }
    public List<ClientPrincipalClaim>? Claims { get; set; }
}

public class ClientPrincipalClaim
{
    public string? Typ { get; set; }
    public string? Val { get; set; }
}
