using Docker.DotNet;
using Docker.DotNet.Models;
using Kenshi.Shared.Models;
using StackExchange.Redis;

namespace Kenshi.API.Helpers;

using k8s;
using k8s.Models;

public class KubernetesService
{
    private readonly IConfiguration _configuration;
    private readonly IDockerClient _client;

    private string GetPodName(int port) => $"gameroom-{port}";

    public KubernetesService(IConfiguration config)
    {
        _configuration = config;
        _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
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

    public async Task CreatePod(GameRoomPodSettings settings)
    {
        int startPort = 5000;
        // Find a free port
        int freePort = startPort + 1;
        if (ListPods().Result.Any())
        {
            // If there are pods in the cluster, find the highest used port number and add 1
            freePort = ListPods().Result.Max(c => int.Parse(c.Port) + 1);
        }
        
        var container = await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Name = GetPodName(freePort),
            Image = "piotrlanger/kenshigameserver:latest",
            Labels = new Dictionary<string, string>()
            {
                { "app", "kenshi-gameserver" },
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
                $"GAME_SERVER_PORT={freePort}",
                $"REDIS_HOST=redis",
            }
        });
        
        await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()
        {

        });
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
                Port = container.Labels.ContainsKey("port") ? container.Labels["port"] : "0"
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
        var redis = ConnectionMultiplexer.Connect("redis");
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