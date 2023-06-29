using Kenshi.API.Models.Concrete;
using Kenshi.API.Models.Concrete.Requests;
using Kenshi.API.Services.Concrete;

namespace Kenshi.API.Services;

public interface IUserService
{
    void GetUserByUsername(string username);
    
    CheckTokenResponse Authenticate(LoginRequestModel model);
    
    RegisterResponse Register(RegisterRequestModel model);
    
    CheckTokenResponse CheckToken(string username, string token);
}