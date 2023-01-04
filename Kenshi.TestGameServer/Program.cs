using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace UDPServer
{
    class Server : INetEventListener
    {
        private NetManager _server;

        public void Start(int port)
        {
            _server = new NetManager(this);
            _server.Start(port);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("Client connected: " + peer.EndPoint);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("Client disconnected: " + peer.EndPoint);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine("Network error: " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("Received message from client: " + reader.GetString(100));
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint endPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Handle unconnected messages
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // Update latency information
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.Accept();
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var port = int.Parse(args[0]);
            var server = new Server();
            server.Start(port);
            Console.WriteLine($"Server started {port}");
            Console.ReadKey();
        }
    }
}