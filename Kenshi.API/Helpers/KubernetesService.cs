namespace Kenshi.API.Helpers;

using k8s;
using k8s.Models;

public class KubernetesService
{
    private readonly IConfiguration _configuration;
    private readonly KubernetesClientConfiguration _kubeConfig;
    private readonly Kubernetes _client;
    
    public KubernetesService(IConfiguration config)
    {
        _configuration = config;
        _kubeConfig = KubernetesClientConfiguration.BuildDefaultConfig();

        _client = new Kubernetes(_kubeConfig);
    }

    public async Task DeletePod(int port)
    {
        await _client.DeleteNamespacedPodAsync($"my-pod-{port}","default");
    }
    
    public async Task CreatePod(GameRoomPodSettings settings)
    {
        var pods = _client.ListNamespacedPod("default").Items;
        
        // Find a free port
        int freePort;
        if (pods.Any())
        {
            // If there are pods in the cluster, find the highest used port number and add 1
            freePort = pods.Max(p => p.Spec.Containers.Max(c => c.Ports.Max(port => port.ContainerPort))) + 1;
        }
        else
        {
            // If there are no pods in the cluster, set the free port to a default value
            freePort = 1;
        }
        
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                Name = $"my-pod-{freePort}",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string>
                {
                    { "app", "my-app" }
                },
                Annotations = settings.Annotations
            },
            Spec = new V1PodSpec
            {
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = "my-container",
                        Image = "nginx:latest",
                        Ports = new List<V1ContainerPort>
                        {
                            new V1ContainerPort
                            {
                                ContainerPort = freePort
                            }
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
}