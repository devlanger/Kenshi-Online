using System.Linq.Expressions;
using Hangfire;
using Kenshi.API.Extensions;
using Kenshi.API.Helpers;
using Kenshi.API.Hub;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
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
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}

        app.UseHttpsRedirection();
        
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

        app.UseHangfireDashboard();
        app.UseHangfireServer();
        
        var service = app.ApplicationServices.GetRequiredService<KubernetesService>();
        RecurringJob.AddOrUpdate(() => MethodCall(service), "*/5 * * * * *");
    }

    public static async Task MethodCall(KubernetesService service)
    {            
        await service.DeletePodsWithZeroPlayers();
    }
}