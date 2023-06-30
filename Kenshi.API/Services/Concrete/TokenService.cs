using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Kenshi.API.Services.Concrete;

public class TokenService : ITokenService
{
    private const string GAME_TOKEN_SECRET = "ASDASCASCMASLVKNAVSAFDSAD";

    public string GenerateEmailActivationToken()
    {
        int length = 32;
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var token = new char[length];

        for (int i = 0; i < length; i++)
        {
            token[i] = chars[random.Next(chars.Length)];
        }

        return new string(token);
    }

    public string GenerateJwtGameToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GAME_TOKEN_SECRET);
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