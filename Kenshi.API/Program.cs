using Hangfire;
using Kenshi.API.Extensions;
using Kenshi.API.Helpers;
using Kenshi.API.Hub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using RabbitMQ.Client;

namespace Kenshi.API;

public class Program
{
    public static void Main(string[] args) 
    {
        CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        return builder.
            ConfigureAppConfiguration((ctx, config) =>
            {
                //var builder = new ConfigurationBuilder()
                //    .SetBasePath(Directory.GetCurrentDirectory());
                //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development")
                {
                    config.AddJsonFile("appsettings.Development.json", optional: false);
                }
                else if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                {
                    config.AddJsonFile("appsettings.Docker.json", optional: false);
                }
            }).
            ConfigureWebHostDefaults(x => x.UseStartup <Startup> ());
    }
}