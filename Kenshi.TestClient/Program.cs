using System;
using System.Net;
using System.Net.Sockets;
using ENet;
using Kenshi.Shared;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Event = ENet.Event;
using EventType = ENet.EventType;

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
                ConnectToGameServer(parameters[1], ushort.Parse(parameters[2]));
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
                ConnectToGameServer("localhost", ushort.Parse(port));
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


        public class Client
        {
            public void Start(string ip, ushort port)
            {
                ENet.Library.Initialize();
                using (Host server = new Host()) {
                    Address address = new Address();

                    address.Port = port;
                    server.Create(address, 100);
                    Console.WriteLine($"Server started at port {port}");
                    Event netEvent;

                    while (!Console.KeyAvailable) {
                        bool polled = false;

                        while (!polled) {
                            if (server.CheckEvents(out netEvent) <= 0) {
                                if (server.Service(15, out netEvent) <= 0)
                                    break;

                                polled = true;
                            }

                            switch (netEvent.Type) {
                                case EventType.None:
                                    break;

                                case EventType.Connect:
                                    Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                    break;

                                case EventType.Disconnect:
                                    Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                    break;

                                case EventType.Timeout:
                                    Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                                    break;

                                case EventType.Receive:
                                    Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                                    netEvent.Packet.Dispose();
                                    break;
                            }
                        }
                    }

                    server.Flush();
                }
            }
        }

        public static void ConnectToGameServer(string ip, ushort port)
        {   
            var client = new Client();
            client.Start(ip, port);
        }
    }
}