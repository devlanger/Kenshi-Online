using Hangfire;
using Kenshi.API.Extensions;
using Kenshi.API.Helpers;
using Kenshi.API.Hub;
using Kenshi.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using RabbitMQ.Client;

namespace Kenshi.API;

public class Program
{
    public static void Main(string[] args) 
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var playerDbContext = services.GetRequiredService<PlayerDbContext>();
                playerDbContext.Database.Migrate();

                var masterDbContext = services.GetRequiredService<MasterDbContext>();
                masterDbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("An error occurred while migrating the database: " + ex.Message);
            }
        }
        
        host.Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        return builder.
            ConfigureAppConfiguration((ctx, config) =>
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development")
                {
                    config.AddJsonFile("appsettings.Development.json", optional: false).AddEnvironmentVariables();
                }
                else if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                {
                    config.AddJsonFile("appsettings.Docker.json", optional: false).AddEnvironmentVariables();
                }
            }).
            ConfigureWebHostDefaults(x => x.UseStartup <Startup> ());
    }
}