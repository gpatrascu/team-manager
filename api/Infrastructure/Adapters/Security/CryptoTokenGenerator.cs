using api.Domain.Ports;
using System.Security.Cryptography;

namespace api.Infrastructure.Adapters.Security;

public class CryptoTokenGenerator : ITokenGenerator
{
    public string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);

        // Convert to base64url encoding (URL-safe, no padding)
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
