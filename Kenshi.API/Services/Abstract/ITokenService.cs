namespace Kenshi.API.Services;

public interface ITokenService
{
    string GenerateEmailActivationToken();
    
    string GenerateJwtGameToken(string username);
}