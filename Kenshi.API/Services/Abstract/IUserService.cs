using Kenshi.API.Models.Concrete;
using Kenshi.API.Models.Concrete.Requests;
using Kenshi.API.Services.Concrete;

namespace Kenshi.API.Services;

public interface IUserService
{
    CheckTokenResponse Authenticate(LoginRequestModel model);
    
    RegisterResponse Register(RegisterRequestModel model);
    
    CheckTokenResponse ActivateAccountEmail(string username, string emailToken);
    
    CheckTokenResponse CheckToken(string username, string token);
}