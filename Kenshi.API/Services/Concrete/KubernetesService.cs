using Docker.DotNet;
using Docker.DotNet.Models;
using Kenshi.API.Hub;
using Kenshi.API.Services;
using Kenshi.Shared.Models;
using StackExchange.Redis;

namespace Kenshi.API.Helpers;

using k8s;
using k8s.Models;

public class KubernetesService
{
    private readonly IConfiguration _configuration;
    private readonly IDockerClient _client;
    private readonly ILogger<KubernetesService> _logger;

    private const int MAX_AVAILABLE_ROOMS = 5;
    
    public static string GetPodName(int port) => $"gameroom-{port}";

    public KubernetesService(IConfiguration config, ILogger<KubernetesService> logger)
    {
        _configuration = config;
        _logger = logger;
        try
        {
            _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            throw;
        }
    }

    public async Task DeletePod(string id)
    {
        Console.WriteLine(id);
        await _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters()
        {
            Force = true,
            RemoveVolumes = true
        });
    }

    public async Task<bool> CreatePod(GameRoomPodSettings settings)
    {
        int freePort = settings.Port;
        
        var container = await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Name = GetPodName(freePort),
            Image = "piotrlanger/shindogs:latest",
            Labels = new Dictionary<string, string>()
            {
                { "app", "shindo-gameserver" },
                { "port", freePort.ToString() }
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>()
            {
                { $"{freePort}/udp", new EmptyStruct() }
            },
            NetworkingConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>
                {
                    { "local-dev", new EndpointSettings() }
                }
            },
            HostConfig = new HostConfig()
            {
                DNS = new[] { "8.8.8.8", "8.8.4.4" },
                PortBindings = new Dictionary<string, IList<PortBinding>>()
                {
                    {
                        $"{freePort}/udp", new List<PortBinding>
                        {
                            new PortBinding()
                            {
                                HostPort = freePort.ToString(),
                                HostIP = "0.0.0.0"
                            }
                        }
                    }
                }
            },
            Env = new List<string>
            {
                $"CONTAINER_NAME={GetPodName(freePort)}",
                $"GAME_MODE={(int)settings.GameModeType}",
                $"GAME_SERVER_PORT={freePort}",
                $"REDIS_HOST=redis",
                $"RABBIT_MQ_HOST=kenshirabbitmq",
                $"JWT_SECRET={_configuration["Jwt:Key"]}",
                $"MAP_NAME={settings.MapName}",
            }
        });
        
        bool run = await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()
        {

        });

        if (run)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<List<ContainerDto>> ListPods()
    {
        var containers = await ListContainersAsync();

        List<ContainerDto> result = new List<ContainerDto>();
        foreach (var container in containers)
        {
            var newDto = new ContainerDto()
            {
                Id = container.ID,
                Name = container.Names[0].Replace("/", ""),
                //Ip = "127.0.0.1",
                Port = container.Labels.ContainsKey("port") ? container.Labels["port"] : "0",
            };

            result.Add(newDto);
        }
        
        return result;
    }

    private async Task<IList<ContainerListResponse>> ListContainersAsync()
    {
        return await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                { "label", new Dictionary<string, bool> { {"app=kenshi-gameserver", true }} }
            }
        });
    }

    public async Task DeleteAllPods()
    {
        // List all pods in the namespace
        var pods = await ListPods();

        // Iterate over the pods and delete them
        foreach (var pod in pods)
        {
            await DeletePod(pod.Id);
        }
    }

    public async Task DeletePodsWithZeroPlayers()
    {
        return;
        var redis = ConnectionMultiplexer.Connect(GameHub.RedisString(_configuration));
        var pods = await ListContainersAsync();
        
        // Iterate over the pods and delete them
        foreach (var pod in pods)
        {
            var c = await _client.Containers.InspectContainerAsync(pod.ID);
            if (c.Config.Env.Contains("PLAYERS=0"))
            {
                string players = redis.GetDatabase().StringGet($"{pod.Names[0].Replace("/","")}_players");
                Console.WriteLine(pod.ID + players);
                //await DeletePod(pod.ID);
            }
            else
            {
            }
        }
    }
}