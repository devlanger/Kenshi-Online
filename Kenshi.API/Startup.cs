using System.Linq.Expressions;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Common;
using Hangfire.MemoryStorage;
using Kenshi.API.Extensions;
using Kenshi.API.Helpers;
using Kenshi.API.Hub;
using Kenshi.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQ.Client;

namespace Kenshi.API;

public class Startup
{
    private IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddServices(_configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}

        //app.UseHttpsRedirection();
        
        app.UseMetricServer();
        app.UseHttpMetrics();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(x =>
        {
            x.MapMetrics();
            x.MapControllers();
            x.MapHub<GameHub>("/gameHub");
        });
        
        JobHelper.SetSerializerSettings(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
        
        app.UseHangfireServer(new BackgroundJobServerOptions()
        {
        });
        app.UseHangfireDashboard();
        
        var service = app.ApplicationServices.GetRequiredService<KubernetesService>();
        RecurringJob.AddOrUpdate("removal", () => MethodCall(service), "*/5 * * * * *");
        var ss = app.ApplicationServices.GetRequiredService<IMatchmakingService>();
        new Thread(new ThreadStart(() =>
        {
            UpdateMatchmake(ss);
        })).Start();
        var roomsService = app.ApplicationServices.GetService<IGameRoomService>();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        bool createTestServer = true;
        if (createTestServer)
        {
            Console.WriteLine("Create debug room at port 5001 for testing locally (run server manually).");
            roomsService.CreateRoom("5001", true);
        }
    }

    public async Task MethodCall(KubernetesService service)
    {            
        await service.DeletePodsWithZeroPlayers();
    }
    
    public async Task UpdateMatchmake(IMatchmakingService matchmakingService)
    {            
        while (true)
        {        
            await matchmakingService.UpdateMatchmakingLobbies();
            await Task.Delay(5000, new CancellationToken());
        }
    }
}
