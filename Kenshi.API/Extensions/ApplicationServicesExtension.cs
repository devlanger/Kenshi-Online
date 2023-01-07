using Hangfire;
using Hangfire.MemoryStorage;
using Kenshi.API.Helpers;

namespace Kenshi.API.Extensions;

public static class ApplicationServicesExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddTransient<KubernetesService>();
        
        services.AddHangfire(config =>
        {
            config.UseMemoryStorage();
        });
    }
}