using LiteNetLib;
using LiteNetLib.Utils;

namespace UDPServer
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var port = int.Parse(args[0]);
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            server.Start(port /* port */);
            Console.WriteLine($"{port}");
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
            };

            listener.PeerDisconnectedEvent += (peer, dcInfo) =>
            {
                Console.WriteLine("We got disconnection: {0} {1}", peer.EndPoint, dcInfo.Reason); // Show peer ip
            };

            while (true)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
            server.Stop();
        }
    }
}