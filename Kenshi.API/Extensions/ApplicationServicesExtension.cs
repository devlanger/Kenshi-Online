using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;
using Kenshi.API.Helpers;
using Kenshi.API.Metrics;
using Kenshi.API.Services;
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
        services.AddTransient<IGameRoomService, GameRoomService>();
        services.AddTransient<UserService>();
        services.AddTransient<MetricsService>();

        services.AddSingleton<ILoggerProvider>(new NestLoggerProvider(new Uri(configuration["ConnectionStrings:elasticsearch"])));
        
        Console.WriteLine(configuration["ConnectionStrings:rabbitmq"]);
        services.AddHostedService<RabbitConsumer>();
        
        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = JwtTokenService.GetTokenValidationParameters(configuration);
            opts.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    //var accessToken = context.Request.Query["access_token"];

                    //// If the request is for our hub...
                    //var path = context.HttpContext.Request.Path;
                    //if (!string.IsNullOrEmpty(accessToken) &&
                    //    (path.StartsWithSegments("/gameHub")))
                    //{
                    //    // Read the token out of the query string
                    //    context.Token = accessToken;
                    //}
                    return Task.CompletedTask;
                }
            };
        });
        
        services.AddHangfire(config =>
        {
            config.UseMemoryStorage();
        });
    }
}