using LiteNetLib;
using LiteNetLib.Utils;

namespace UDPServer;

public class GameServer
{
    public void Start()
    {
        ushort port = ushort.Parse(Environment.GetEnvironmentVariable("GAME_SERVER_PORT") ?? "5001");
        EventBasedNetListener listener = new EventBasedNetListener();
        NetManager server = new NetManager(listener);
        server.Start(port /* port */);

        listener.ConnectionRequestEvent += request =>
        {
            if(server.ConnectedPeersCount < 10 /* max connections */)
                request.AcceptIfKey("test");
            else
                request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("user connected to litenetib!");
        };
        
        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
            NetDataWriter writer = new NetDataWriter();                 // Create writer class
            writer.Put("Hello client!");                                // Put some string
            peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
        };

        Console.WriteLine("Started litenetlib server");
        while (true)
        {
            server.PollEvents();
            Thread.Sleep(15);
        }
        server.Stop();
    }
}