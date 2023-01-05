using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kenshi.TestClient
{
    class Program
    {
        private static Stack<string> commandsHistory = new Stack<string>();

        static async Task Main(string[] args)
        {
            // Create a connection to the SignalR hub
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/gameHub")
                .Build();

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
                //ConnectToGameServer(int.Parse(port));
                ConnectToGameServer(5000);
                Console.WriteLine(message);
            });

            while (true)
            {
                var input = Console.ReadLine();
                string[] parameters = input.Split(" ");

                try
                {
                    switch (parameters[0])
                    {
                        case "dc" or "disconnect":
                            return;
                        case "create_game":
                            await connection.SendAsync("CreateGameRoom", "test-room");
                            break;
                        case "delete_game":
                            await connection.SendAsync("DeleteGameRoom", int.Parse(parameters[1]));
                            Console.WriteLine($"delete game {int.Parse(parameters[1])}");
                            break;
                        case "delete_all_games":
                            await connection.SendAsync("DeleteAllGameRooms");
                            Console.WriteLine($"delete all games");
                            break;
                        case "join_game":
                            await connection.SendAsync("JoinGameRoom", parameters[1]);
                            break;
                        case "games":
                            await connection.SendAsync("ListGameRooms");
                            break;
                    }

                    if (!string.IsNullOrEmpty(input))
                    {
                        commandsHistory.Push(input);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error executing command." + e);
                }
                
                Thread.Sleep(50);
            }
        }

        public class Client : INetEventListener
        {
            private NetManager _client;
            private NetPeer _serverPeer;

            public void Start(string ip, int port)
            {
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

        public static void ConnectToGameServer(int port)
        {
            var client = new Client();
            client.Start("127.0.0.1", port);
        }
    }
}