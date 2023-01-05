namespace Kenshi.API.Helpers;

using k8s;
using k8s.Models;

public class KubernetesService
{
    private readonly IConfiguration _configuration;
    private readonly KubernetesClientConfiguration _kubeConfig;
    private readonly Kubernetes _client;

    private string GetPodName(int port) => $"gameroom-{port}";

    public KubernetesService(IConfiguration config)
    {
        _configuration = config;
        _kubeConfig = KubernetesClientConfiguration.BuildDefaultConfig();

        _client = new Kubernetes(_kubeConfig);
    }

    public async Task DeletePod(int port)
    {
        await _client.DeleteNamespacedPodAsync($"my-pod-{port}", "default");
    }

    public async Task CreatePod(GameRoomPodSettings settings)
    {
        var pods = _client.ListNamespacedPod("default").Items;
        int startPort = 5000;
        // Find a free port
        int freePort;
        if (pods.Any())
        {
            // If there are pods in the cluster, find the highest used port number and add 1
            freePort = pods.Max(p => p.Spec.Containers.Max(c => c.Ports.Max(port => port.ContainerPort))) + startPort + 1;
        }
        else
        {
            // If there are no pods in the cluster, set the free port to a default value
            freePort = startPort + 1;
        }

        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                Name = $"{GetPodName(freePort)}",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string>
                {
                    { "app", "my-app" },
                    { "port", freePort.ToString() }
                },
                Annotations = settings.Annotations
            },
            Spec = new V1PodSpec
            {
                HostNetwork = true,
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = "my-container",
                        Image = "piotrlanger/kenshigameserver:latest",
                        Ports = new List<V1ContainerPort>
                        {
                            new V1ContainerPort
                            {
                                Protocol = "UDP",
                                ContainerPort = freePort,
                                HostPort = freePort
                            }
                        },
                        Args = new List<string>
                        {
                            freePort.ToString(),
                        }
                    }
                }
            }
        };

        await _client.CreateNamespacedPodAsync(pod, "default");
    }

    public async Task<List<V1Pod>> ListPods()
    {
        var pods = await _client.ListNamespacedPodAsync("default");
        return pods.Items.ToList();
    }

    public async Task DeleteAllPods()
    {
        // List all pods in the namespace
        var pods = await _client.ListNamespacedPodAsync("default");

        // Iterate over the pods and delete them
        foreach (var pod in pods.Items)
        {
            await _client.DeleteNamespacedPodAsync(pod.Metadata.Name, "default");
        }
    }
}