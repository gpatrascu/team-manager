namespace api.Domain.Ports;

public interface ITokenGenerator
{
    string GenerateSecureToken();
}
