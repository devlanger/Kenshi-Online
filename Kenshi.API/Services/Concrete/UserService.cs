using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kenshi.API.Models.Abstract;
using Kenshi.API.Models.Concrete;
using Kenshi.API.Models.Concrete.Requests;
using Microsoft.IdentityModel.Tokens;

namespace Kenshi.API.Services.Concrete;

public class UserService : IUserService
{
    private const string TOKEN_SECRET = "ASDASCASCMASLVKNAVSAFDSAD";
    
    private IRepository<User> _userRepo;

    public UserService(IRepository<User> userRepo)
    {
        _userRepo = userRepo;
    }

    public void GetUserByUsername(string username)
    {
        
    }

    public RegisterResponse Register(RegisterRequestModel model)
    {
        RegisterResponse response = new RegisterResponse();

        if (model.Password != model.RepeatPassword)
        {
            response.Message = "Passwords not matching.";
            return response;
        }
        
        // Check if the username is already taken
        if (_userRepo.WithQuery().FirstOrDefault(u => u.Username == model.Username || u.Email == model.Email) != null)
        {
            response.Message = "Username or email is already taken.";
            return response;
        }

        var passwordHash = HashPassword(model.Password);

        var user = new User
        {
            Email = model.Email,
            Username = model.Username,
            PasswordHash = passwordHash
        };

        _userRepo.Persist(user);
        response.User = MapUser(user, new UserDto());
        response.Message = "User registered successfully.";
        response.Success = true;
        return response;
    }

    public CheckTokenResponse CheckToken(string username, string token)
    {
        var response = new CheckTokenResponse();
        var user = _userRepo.WithQuery().FirstOrDefault(u => u.Username == username);
        if (user is null || user.Token is null)
        {
            response.Message = "Invalid token.";
            return response;
        }

        if (user.Token != token)
        {
            response.Message = "Invalid token.";
            return response;
        }

        if (user.TokenExpirationDate < DateTimeOffset.Now)
        {
            response.Message = "Invalid token.";
            return response;
        }

        response.Message = "Token is valid";
        response.User = MapUser(user, new UserDto());
        
        return response;
    }

    public CheckTokenResponse Authenticate(LoginRequestModel model)
    {
        CheckTokenResponse response = new CheckTokenResponse();

        var token = GenerateJwtToken(model.Username);
        var user = _userRepo.WithQuery().FirstOrDefault(u => u.Username == model.Username || u.Email == model.Username);
        if (user is null)
        {
            response.Message = "Incorrect username or password.";
            return response;
        }

        if (!VerifyPassword(model.Password, user.PasswordHash))
        {
            response.Message = "Incorrect username or password.";
            return response;
        }
        
        if (!user.IsActivated)
        {
            response.Message = "Account is not active.";
            return response;
        }
        
        user.Token = token;
        user.TokenExpirationDate = DateTimeOffset.Now.AddDays(7);
        
        response.Message = "Logged in successfully";
        response.User = MapUser(user, new UserDto());
        
        _userRepo.Persist(user);
        return response;
    }

    private UserDto? MapUser(User user, UserDto userDto)
    {
        userDto.Username = user.Username;
        userDto.Token = user.Token;
        
        return userDto;
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, GenerateSalt());
    }

    private static string GenerateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt(5);
    }

    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TOKEN_SECRET);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}