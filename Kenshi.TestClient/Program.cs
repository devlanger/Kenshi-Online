using System;
using System.Net;
using System.Net.Sockets;
using Kenshi.Shared;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kenshi.TestClient
{
    class Program
    {
        private static Stack<string> commandsHistory = new Stack<string>();

        private static string GetUrl()
        {
            return "http://localhost:3330/gameHub";
        }
        
        static async Task Main(string[] args)
        {
            // Create a connection to the SignalR hub
            var connection = new HubConnectionBuilder()
                .WithUrl(GetUrl())
                .Build();

            NetworkCommandProcessor.RegisterCommand("connect", (string[] parameters) =>
            {
                ConnectToGameServer(parameters[1], int.Parse(parameters[2]));
            });
            
            // Start the connection
            await connection.StartAsync();
            
            // Subscribe to messages from the hub
            connection.On<string>("ListGameRooms", (message) =>
            {
                Console.WriteLine(message);
            });
            
            connection.On<string>("JoinGameRoom", (message) =>
            {
                string port = message;
                ConnectToGameServer("localhost", int.Parse(port));
                //ConnectToGameServer(5000);
                Console.WriteLine(message);
            });

            while (true)
            {
                var input = Console.ReadLine();
                await NetworkCommandProcessor.ProccessCommand(input, connection);

                Thread.Sleep(50);
            }
        }


        public class Client : INetEventListener
        {
            private NetManager _client;
            private NetPeer _serverPeer;

            public void Start(string ip, int port)
            {
                Console.WriteLine($"Try to connect to game server: {ip} {port}");
                _client = new NetManager(this);
                _client.Start();
                _client.Connect(ip, port, "test");
            }

            public void OnPeerConnected(NetPeer peer)
            {
                Console.WriteLine("Connected to server: " + peer.EndPoint);
                _serverPeer = peer;

                // Send a message to the server
                var writer = new NetDataWriter();
                writer.Put("Hello from the client!");
                _serverPeer.Send(writer, DeliveryMethod.Unreliable);
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
                Console.WriteLine("Disconnected from server: " + peer.EndPoint);
                _serverPeer = null;
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
            {
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
            }

            public void OnConnectionRequest(ConnectionRequest request)
            {
            }
        }

        public static void ConnectToGameServer(string ip, int port)
        {   
            var client = new Client();
            client.Start(ip, port);
        }
    }
}