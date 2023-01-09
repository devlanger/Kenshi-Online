using System.Security.Authentication;
using Docker.DotNet;
using Docker.DotNet.Models;
using StackExchange.Redis;
using UDPServer;
using System;
using System.Collections.Generic;
using System.IO;
using ENet;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer;
using Address = ENet.Address;

namespace UDPServer
{

    class Program
    {
        struct Position
        {
            public float x;
            public float y;
            public float z;
        }

        static Host _server = new Host();
        private static Dictionary<uint, Position> _players = new Dictionary<uint, Position>();

        private static string containerName = "test";
        private static int players = 0;
        private static IDockerClient _client;

        private static ConnectionMultiplexer redis;
        
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

        static async Task Main(string[] args)
        {
            containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME") ?? "test";
            ushort port = 5001;
            try
            {
                redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            port = ushort.Parse(Environment.GetEnvironmentVariable("GAME_SERVER_PORT") ?? "5001");
            if (UpdatePlayersAmount(0))
            {
                Console.WriteLine("Successfully connected to redis and initialized server parameters.");
            }
            
            const int maxClients = 100;
            Library.Initialize();

            _server = new Host();
            Address address = new Address();

            address.Port = port;
            _server.Create(address, maxClients);

            Console.WriteLine($"Circle ENet Server started on {port}");

            Event netEvent;
            while (true)
            {
                bool polled = false;

                while (!polled)
                {
                    if (_server.CheckEvents(out netEvent) <= 0)
                    {
                        if (_server.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            netEvent.Peer.Timeout(32, 1000, 4000);
                            players++;
                            UpdatePlayersAmount(players);
                            break;

                        case EventType.Disconnect:
                            Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            HandleLogout(netEvent.Peer.ID);
                            break;

                        case EventType.Timeout:
                            Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            HandleLogout(netEvent.Peer.ID);
                            break;

                        case EventType.Receive:
                            //Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                            HandlePacket(ref netEvent);
                            netEvent.Packet.Dispose();
                            break;
                    }
                }

                _server.Flush();
            }
            Library.Deinitialize();
        }

        static void HandlePacket(ref Event netEvent)
        {
            var readBuffer = new byte[1024];
            var readStream = new MemoryStream(readBuffer);
            var reader = new BinaryReader(readStream);

            readStream.Position = 0;
            netEvent.Packet.CopyTo(readBuffer);
            var packetId = (PacketId)reader.ReadByte();

            if (packetId != PacketId.PositionUpdateRequest)
                Console.WriteLine($"HandlePacket received: {packetId}");

            if (packetId == PacketId.LoginRequest)
            {
                var playerId = netEvent.Peer.ID;
                SendPacket(ref netEvent, new LoginResponsePacket(playerId));
                SendPacketToAll(new LoginEventPacket(playerId));
                foreach (var p in _players)
                {
                    SendPacket(ref netEvent, new LoginEventPacket(p.Key));
                }
                _players.Add(playerId, new Position { x = 0.0f, y = 0.0f, z = 0.0f });
            }
            else if (packetId == PacketId.PositionUpdateRequest)
            {
                var playerId = reader.ReadUInt32();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var z = reader.ReadSingle();
                //Console.WriteLine($"ID: {playerId}, Pos: {x}, {y}");
                SendPacketToAll(new PositionUpdatePacket(playerId, x, y, z));
            }
        }

        static void SendPacket(ref Event netEvent, SendablePacket p)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize(p);
            var packet = default(Packet);
            packet.Create(buffer);
            
            netEvent.Peer.Send(0, ref packet);
        }

        static void SendPacketToAll(SendablePacket p)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize(p);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }

        static void HandleLogout(uint playerId)
        {
            if (!_players.ContainsKey(playerId))
                return;

            _players.Remove(playerId);
            SendPacketToAll(new LogoutEventPacket(playerId));
            players--;
            UpdatePlayersAmount(players);
        }
    }
}