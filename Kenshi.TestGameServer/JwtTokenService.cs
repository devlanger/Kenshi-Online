using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UDPServer;

public class JwtTokenService
{
    public static TokenValidationParameters GetTokenValidationParameters(string key)
    {
        return new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = configuration["Jwt:Issuer"],
            //ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    }
    
    public static bool VerifyToken(string secret, string token)
    {
        var tokenValidationParameters = GetTokenValidationParameters(secret);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }
}