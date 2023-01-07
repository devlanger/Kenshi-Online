using System.Security.Authentication;
using Docker.DotNet;
using Docker.DotNet.Models;
using LiteNetLib;
using LiteNetLib.Utils;
using StackExchange.Redis;

namespace UDPServer
{
    class Program
    {
        private static string containerName = "test";
        private static int players = 0;
        private static IDockerClient _client;

        private static ConnectionMultiplexer redis;
        
        private static async Task Main(string[] args)
        {
            containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME") ?? "test";
            redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis");
            _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
            var port = int.Parse(Environment.GetEnvironmentVariable("GAME_SERVER_PORT") ?? "5001");
            if (UpdatePlayersAmount(0))
            {
                Console.WriteLine("Successfully connected to redis and initialized server parameters.");
            }
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            server.Start(port /* port */);
            Console.WriteLine($"Server Started successfully at port: {port}");
            listener.ConnectionRequestEvent += request =>
            {
                if(server.ConnectedPeersCount < 10 /* max connections */)
                    request.AcceptIfKey("test");
                else
                    request.Reject();
            };
            
            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
                players++;
                // Set the name of the environmental variable
                UpdatePlayersAmount(players);
            };

            listener.PeerDisconnectedEvent += (peer, dcInfo) =>
            {
                Console.WriteLine("We got disconnection: {0} {1}", peer.EndPoint, dcInfo.Reason); // Show peer ip
                players--;
                UpdatePlayersAmount(players);
            };

            while (true)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
            server.Stop();
        }

        private static bool UpdatePlayersAmount(int i)
        {
            try
            {
                var db = redis.GetDatabase();
                db.StringSet($"{containerName}_players", i.ToString());
                Console.WriteLine("Players Count: " + db.StringGet($"{containerName}_players"));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Redis connection error: " + e);
                return false;
            }
        }
    }
}