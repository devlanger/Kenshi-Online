using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;
using Kenshi.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Kenshi.API.Extensions;

public static class ApplicationServicesExtension
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddTransient<KubernetesService>();
        services.AddTransient<JwtTokenService>();

        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = JwtTokenService.GetTokenValidationParameters(configuration);
        });
        
        services.AddHangfire(config =>
        {
            config.UseMemoryStorage();
        });
    }
}