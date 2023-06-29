using System.Linq.Expressions;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Common;
using Hangfire.MemoryStorage;
using Kenshi.API.Extensions;
using Kenshi.API.Helpers;
using Kenshi.API.Hub;
using Kenshi.API.Models;
using Kenshi.API.Models.Abstract;
using Kenshi.API.Models.Concrete;
using Kenshi.API.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;

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
        string passwordString = $"Password={_configuration["MSSQL_SA_PASSWORD"]};";
        Console.WriteLine(_configuration.GetConnectionString("master_database") + passwordString);
        services.AddDbContext<MasterDbContext>(options =>
            options.UseSqlServer(_configuration.GetConnectionString("master_database") + passwordString));
        
        services.AddDbContext<PlayerDbContext>(options =>
            options.UseSqlServer(_configuration.GetConnectionString("player_database") + passwordString));

        services.AddServices(_configuration);
        
        services.AddScoped(typeof(IRepository<>), typeof(MasterRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(PlayerRepository<>));
        
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
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            app.UseHttpsRedirection();
        }

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
        
        var service = app.ApplicationServices.GetRequiredService<KubernetesService>();
        new Thread(new ThreadStart(() =>
        {
            MethodCall(service);
        })).Start();
        
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
        while (true)
        {        
            //await service.DeletePodsWithZeroPlayers();
            await Task.Delay(5000, new CancellationToken());
        }
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
