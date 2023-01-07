using Hangfire;
using Kenshi.API.Extensions;
using Kenshi.API.Helpers;
using Kenshi.API.Hub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
namespace Kenshi.API;

public class Program
{
    public static void Main(string[] args) {
        CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) {
        return Host.CreateDefaultBuilder(args).
            ConfigureWebHostDefaults(x => x.UseStartup <Startup> ());
    }
}