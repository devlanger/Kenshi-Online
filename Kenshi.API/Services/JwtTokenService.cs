using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration, string issuer = "", string audience = "")
    {
        _issuer = issuer;
        _audience = audience;
        _configuration = configuration;
    }

    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration config)
    {
        return new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = configuration["Jwt:Issuer"],
            //ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
        };
    }

    public string GenerateToken(string userId)
    {
        var claims = new[] { new Claim("Name", userId) };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_issuer, _audience, claims, expires: DateTime.Now.AddDays(7), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }   
    
    public bool VerifyToken(string token)
    {
        var tokenValidationParameters = GetTokenValidationParameters(_configuration);

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